using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using System.Security.Cryptography;
using System.IO;

namespace AdjutantCore.Discord
{
    public class DiscordTask
    {
        public string Title;
        public string Id;
        public int Priority;
        public int Level=-1;
        public int Progression=0;
        public DateTime CreationTime;
        public bool Completed = false;
        public bool WorkInProgress = false;

        private DiscordTask Parent;
        public List<DiscordTask> Childs = new List<DiscordTask>();

        public DiscordTask(string title, int priority)
        {
            Title = title;
            Id = GenerateId();
            CreationTime = DateTime.Now;
            Priority = priority;
        }
        /// <summary>
        /// Generated a random id for file finding in the tree
        /// </summary>
        /// <returns></returns>
        private string GenerateId()
        {
            var bytes = new byte[4];
            var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            uint random = BitConverter.ToUInt32(bytes, 0) % 100000000;
            return String.Format("{0:D8}", random);
        }
        /// <summary>
        /// Adds a child to the node
        /// </summary>
        /// <param name="task"></param>
        public void AddTask(DiscordTask task)
        {
            task.Parent = this;
            task.Level = this.Level + 1;
            this.Childs.Add(task);
            
        }
        public void RemoveTask(string id)
        {
            var task = GetTask(id);
            task.Parent.Childs.Remove(task);

        }
        public void RemoveTask(DiscordTask task)
        {
            task.Parent.Childs.Remove(task);
        }
        public DiscordTask GetTask(string id)
        {
            return this.Descendants().Where(x => x.Id == id).FirstOrDefault();
        }
        public void SetProgression(int percentage)
        {

            if (percentage > 100)
                percentage = 100;
            this.Progression = (int)Math.Round((double)percentage, 1);
            if (Progression < 100 && Progression>0)
            {
                this.WorkInProgress = true;
                this.Completed = false;
            }  
            else if(Progression == 0)
            {
                this.WorkInProgress = false;
                this.Completed = false;
            }
            else
            {
                this.Completed = true;
                this.WorkInProgress = false;
            }
            if(this.Parent != null)
                this.Parent.SetProgression(Convert.ToInt32(Math.Round(this.Parent.Childs.Average(x=>x.Progression),2)));
            
        }   

        /// <summary>
        /// Generate a single line string of the task
        /// </summary>
        /// <returns></returns>
        private string GenerateLine()
        {
            string status;
            if (Completed)
                status = "☑";
            else
                status = "☐";
            string symbol = "";
            if (Priority == 1)
                symbol = "(★)";
            if (Priority == 2)
                symbol = "(☆)";


            string result = $"{status} [{Id}]{symbol} - {Title} ({Progression}%)";
            
            if (this.WorkInProgress)
            {
                result = result.PadLeft(result.Length + (this.Level * 4-1));
                return result.Insert(0, "#");
            }
            else
            {
                result = result.PadLeft(result.Length + (this.Level * 4));
                return result;
            }
        }
        /// <summary>
        /// Prints the entire tree in the console.
        /// </summary>
        public void Print()
        {
            Print(this);
        }
        private void Print(DiscordTask task)
        {
            Console.WriteLine(task.GenerateLine());
            foreach (var child in task.Childs)
                Print(child);
        }
        /// <summary>
        /// Prints the entire tree in discord.
        /// </summary>
        public List<string> GenerateDiscordMessages()
        {
            List<string> message = new List<string>();
            List<string> submessage = new List<string>(); 
            foreach (var child in this.Childs)
            {

                if (LenghtList(submessage) + child.Length() < 2000-100)
                {
                    submessage.Add(string.Join(Environment.NewLine, child.GenerateStringArray()));
                }
                else
                {
                    message.Add(Format.Code(string.Join(Environment.NewLine, submessage),"md"));
                    submessage.Clear();
                }

            }
            if(!message.Contains(string.Join(Environment.NewLine, submessage)))
                message.Add(Format.Code(string.Join(Environment.NewLine, submessage),"md"));
            return message;
                
        }
        private int LenghtList(List<string> list)
        {
            int sum = 0;
            foreach (var str in list)
            {
                sum += str.Length;
            }
            return sum;
        }
        /// <summary>
        /// Generated the task messages as a table of strings
        /// </summary>
        /// <returns></returns>
        public string[] GenerateStringArray()
        {
            return GenerateStringArray(new List<string>());
        }
        private string[] GenerateStringArray(List<string> list)
        {
            list.Add(this.GenerateLine());
            if (this.Childs.Count() == 0)
                return list.ToArray();
            else
                foreach (var child in this.Childs)
                    child.GenerateStringArray(list);
            return list.ToArray();

        }
        /// <summary>
        /// Gives the geneared message string length in total
        /// </summary>
        /// <returns></returns>
        public int Length()
        {
            int sum = 0;
            foreach (var lin in GenerateStringArray())
                sum += lin.Length;
            return sum;
        }
        public void FixParents()
        {
            FixParents(this);
        }
        /// <summary>
        /// Fix the parents after de desarlalized json
        /// </summary>
        /// <param name="task"></param>
        private void FixParents(DiscordTask task)
        {
            foreach (var child in task.Childs)
            {
                child.Parent = task;
                FixParents(child);
            }
        }
        
        //static void Main(string[] args)
        //{
        //    List<DiscordTask> tasks = new List<DiscordTask>();
        //    DiscordTask task = new DiscordTask("Just a task", 1);
        //    task.AddChild(new DiscordTask("child task", 2));
        //    tasks.Add(task);
        //    task = new DiscordTask("2nd task", 1);
        //    task.AddChild(new DiscordTask("child 2 1", 1));
        //    task.AddChild(new DiscordTask("child 2 2", 1));
        //    task.Childs[0].AddChild(new DiscordTask("child of child", 1));
        //    tasks.Add(task);
        //    foreach (var item in tasks)
        //        item.Print();

        //    var test = task.GenerateMessage();
        //    Console.WriteLine(task.Length());

        //    Console.ReadLine();
        //}
    }
    public static class DiscordTaskExtension
    {
        public static IEnumerable<DiscordTask> Descendants(this DiscordTask root)
        {
            var nodes = new Stack<DiscordTask>(new[] { root });
            while (nodes.Any())
            {
                DiscordTask node = nodes.Pop();
                yield return node;
                foreach (var n in node.Childs) nodes.Push(n);
            }
        }
    }

}
