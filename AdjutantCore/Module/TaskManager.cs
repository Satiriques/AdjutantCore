using AdjutantCore.Discord;
using Discord.Commands;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;

namespace AdjutantCore.Module
{
    [RequireOwner]
    public class TaskManager : ModuleBase
    {

        private DiscordTask TreeTask = new DiscordTask("root", 0);


        TaskManager()
        {
            try
            {
                var obj = JsonConvert.DeserializeObject<DiscordTask>(File.ReadAllText("Module/tasks.json"));
                if (obj != null) TreeTask = obj;
                TreeTask.FixParents();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        [Command("taskadd")]
        [Alias("ta")]
        [Summary("Adds a new task")]
        public async Task TaskAdd(int priority, [Remainder] string title)
        {
            TreeTask.AddTask(new DiscordTask(title, priority));
            ExportTasks();
            var messages = await Context.Channel.GetMessagesAsync().Flatten();
            await Context.Channel.DeleteMessagesAsync(messages);
            await TaskPrintAll();
        }

        [Command("taskaddchild")]
        [Alias("tac")]
        [Summary("Adds a child task to an existing task")]
        public async Task TaskAddChild(string id, int priority, [Remainder] string title)
        {
            TreeTask.GetTask(id).AddTask(new DiscordTask(title, priority));
            ExportTasks();
            var messages = await Context.Channel.GetMessagesAsync().Flatten();
            await Context.Channel.DeleteMessagesAsync(messages);
            await TaskPrintAll();
        }

        [Command("taskremove")]
        [Alias("tr")]
        [Summary("Remove a task")]
        public async Task TaskRemove(string id)
        {
            TreeTask.RemoveTask(id);
            ExportTasks();
            var messages = await Context.Channel.GetMessagesAsync().Flatten();
            await Context.Channel.DeleteMessagesAsync(messages);
            await TaskPrintAll();
        }

        [Command("taskcompleted")]
        [Alias("tc")]
        [Summary("Sets a task to completed")]
        public async Task TaskCompleted(string id)
        {
            TreeTask.GetTask(id).SetProgression(100);
            ExportTasks();
            var messages = await Context.Channel.GetMessagesAsync().Flatten();
            await Context.Channel.DeleteMessagesAsync(messages);
            await TaskPrintAll();
        }
        [Command("taskprogression")]
        [Alias("tp")]
        [Summary("Change the task progression (0-100%)")]
        public async Task TaskProgression(string id, int progression)
        {
            TreeTask.GetTask(id).SetProgression(progression);
            ExportTasks();
            var messages = await Context.Channel.GetMessagesAsync().Flatten();
            await Context.Channel.DeleteMessagesAsync(messages);
            await TaskPrintAll();
        }


        [Command("taskprint")]
        [Alias("tprint")]
        [Summary("Prints a specific task")]
        public async Task TaskPrint(string id)
        {
            foreach (var message in TreeTask.GetTask(id).GenerateDiscordMessages())
                await ReplyAsync(message);

        }

        [Command("taskprintall")]
        [Alias("tpa")]
        [Summary("Prints all the task")]
        public async Task TaskPrintAll()
        {
            var messages = await Context.Channel.GetMessagesAsync().Flatten();
            await Context.Channel.DeleteMessagesAsync(messages);
            foreach (var message in TreeTask.GenerateDiscordMessages())
                await ReplyAsync(message);

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

        private void ExportTasks()
        {
            File.WriteAllText("Module/tasks.json", JsonConvert.SerializeObject(TreeTask, Formatting.Indented));
        }
    }

}
