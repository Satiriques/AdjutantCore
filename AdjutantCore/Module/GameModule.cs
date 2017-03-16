using System;
using System.Collections.Generic;
using System.Text;
using Discord;
using Discord.WebSocket;
using Discord.Addons.InteractiveCommands;
using Discord.Commands;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using System.Linq;

namespace AdjutantCore.Module
{
    [Group("smash")]
    public class GameModule : InteractiveModuleBase
    {
        List<GameJson> Queries;
        const string FilePath = "Module/games.json";

        GameModule()
        {
            Queries = JsonConvert.DeserializeObject<List<GameJson>>(File.ReadAllText(FilePath));

        }
        
        [Command("")]
        public async Task Smash(string query)
        {
            var cmd = Queries.Where(x => x.Command == query).FirstOrDefault();
            if (cmd != null)
            {
                var emb = new EmbedBuilder()
                {
                    Title = cmd.Title,
                    Description = cmd.Description,
                    ImageUrl = cmd.ThumbnailUrl,
                    Url = cmd.Source,
                    Color = new Color(255, 255, 255)
                };
                await ReplyAsync("", false, emb);
            }
            else
            {
                await ReplyAsync("Cmd not found");
            }
        }

        [Command("listqueries")]
        public async Task ListQueries()
        {
            await ReplyAsync(Format.Code(String.Join(", ", Queries.Select(x => x.Command))));
        }
            
        [Command("addquery",RunMode=RunMode.Async)]
        public async Task AddQuery()
        {
            IUserMessage cmd = null, title = null, desc = null, img = null, src = null;
			
            await ReplyAsync("Enter the command argument");
            cmd = await WaitForMessage(Context.Message.Author, Context.Channel,new TimeSpan(0, 5, 0));
			if(cmd != null)
			{
				await ReplyAsync("Enter the title");
				title = await WaitForMessage(Context.Message.Author, Context.Channel, new TimeSpan(0, 5, 0));
            }
			if(title!=null)
			{
				await ReplyAsync("Enter the description");
				desc = await WaitForMessage(Context.Message.Author, Context.Channel, new TimeSpan(0, 5, 0));
            }
			if(desc!= null)
			{
                await ReplyAsync("Enter the thumbnail url");
                img = await WaitForMessage(Context.Message.Author, Context.Channel, new TimeSpan(0, 5, 0));
            }
			if(img!= null)
			{
                await ReplyAsync("Enter the source url");
                src = await WaitForMessage(Context.Message.Author, Context.Channel, new TimeSpan(0, 5, 0));
            }
			
			if(cmd!= null && title!=null && desc!=null && img!=null && src!=null)
			{
				//add it to the current queries
                Queries.Add(new GameJson(cmd.Content, title.Content, desc.Content, img.Content, src.Content));
				
				//save it
                File.WriteAllText(FilePath, JsonConvert.SerializeObject(Queries.OrderBy(x => x.Title),Formatting.Indented));



                //visualisation
                var emb = new EmbedBuilder()
                {
                    Title = title.Content,
                    Description = desc.Content,
                    ImageUrl = img.Content,
                    Url = src.Content,
                    Color = new Color(255, 255, 255)
				};

				await ReplyAsync("", false, emb);
			}
			else
			{
				await ReplyAsync("erreur");
			}

        }

        [Command("modifyquery", RunMode = RunMode.Async)]
        public async Task ModifyQuery(string cmd)
        {
            int index = Queries.FindLastIndex(x => x.Command == cmd.ToLower());
            //if the item exists
            if (index != -1)
            {
                bool quit = false;
                var query = Queries[index];
                //make sure it's a hollow copy
                string menu = Format.Code(
                $"#Voici ce que vous pouvez changer pour la commande.\n" +
                $"1. Change the command argument.\n" +
                $"2. Change the title.\n" +
                $"3. Change the description.\n" +
                $"4. Change the thumbnail url.\n" +
                $"5. Change the title url (usually the source link).\n" +
                $"6. Quit without saving\n" +
                $"7. Save and Quit","md");
                IMessage option = null;
                IMessage ans = null;
                IMessage message = null;

                //parameters of quitting
                //null: when the timer is out
                //"6": user quit without saving
                //"7": user quit while saving
                while (!quit)
                {
                    if (message != null)
                        //removes the old menu message
                        await message.DeleteAsync();
                    if (option != null)
                        //removes the old user response message
                        await option.DeleteAsync();
                    message = await ReplyAsync(menu);
                    option = await WaitForMessage(Context.Message.Author, Context.Channel, new TimeSpan(0, 5, 0));
                    if (option != null)
                    {
                        switch (option.Content)
                        {
                            case "1":
                                await ReplyAsync(
                                $"Command:\n" +
                                $"{Format.Italics(query.Command)}\n" +//TO DO italics
                                $"Enter the new command: ");
                                ans = await WaitForMessage(Context.Message.Author, Context.Channel, new TimeSpan(0, 5, 0));
                                if (ans != null)
                                {
                                    query.Command = ans.Content;
                                }
                                else
                                    await ReplyAsync("Delai terminer");
                                break;

                            case "2":
                                await ReplyAsync(
                                $"Title:\n" +
                                $"{Format.Italics(query.Title)}\n" +//TO DO italics
                                $"Enter the new title ");
                                ans = await WaitForMessage(Context.Message.Author, Context.Channel, new TimeSpan(0, 5, 0));
                                if (ans != null)
                                {
                                    query.Title = ans.Content;
                                }
                                else
                                    await ReplyAsync("Delai terminer");
                                break;

                            case "3":
                                await ReplyAsync(
                                $"Description:\n" +
                                $"{Format.Italics(query.Description)}\n" +//TO DO italics
                                $"Enter the new description ");
                                ans = await WaitForMessage(Context.Message.Author, Context.Channel, new TimeSpan(0, 5, 0));
                                if (ans != null)
                                {
                                    query.Description = ans.Content;
                                }
                                else
                                    await ReplyAsync("Delai terminer");
                                break;

                            case "4":
                                await ReplyAsync(
                                $"Thumbnail Url:\n" +
                                $"{Format.Italics(query.ThumbnailUrl)}\n" +//TO DO italics
                                $"Enter the new thumbnail url ");
                                ans = await WaitForMessage(Context.Message.Author, Context.Channel, new TimeSpan(0, 5, 0));
                                if (ans != null)
                                {
                                    query.ThumbnailUrl = ans.Content;
                                }
                                else
                                    await ReplyAsync("Delai terminer");
                                break;

                            case "5":
                                await ReplyAsync(
                                $"Title url:\n" +
                                $"{Format.Italics(query.Source)}\n" +//TO DO italics
                                $"Enter the new title url ");
                                ans = await WaitForMessage(Context.Message.Author, Context.Channel, new TimeSpan(0, 5, 0));
                                if (ans != null)
                                {
                                    query.Source = ans.Content;
                                }
                                else
                                    await ReplyAsync("Delai terminer");
                                break;
                            case "6":
                                quit = true;
                                break;

                            case "7":
                                Queries[index] = query;
                                File.WriteAllText(FilePath, JsonConvert.SerializeObject(Queries.OrderBy(x => x.Title), Formatting.Indented));
                                quit = true;
                                break;


                        }
                    }


                }
            }
            else
            {
                await ReplyAsync($"The command {cmd}  doesn't exists");
            }


        }

















        //"Command": "wavedash",
        //"Title": "Wavedash",
        //"Description": "...",
        //"ThumbnailUrl": "",
        //"Source": ""



        //[Command("addquery",RunMode=RunMode.Async)]
        //public async Task AddQuery()
        //{
        //    await ReplyAsync("Enter the commande argument");
        //    var cmd = await WaitForMessage(Context.Message.Author, Context.Channel);
        //    await ReplyAsync("Enter the title");
        //    var title = await WaitForMessage(Context.Message.Author, Context.Channel);
        //    await ReplyAsync("Enter the description");
        //    var desc = await WaitForMessage(Context.Message.Author, Context.Channel);
        //    await ReplyAsync("Enter the thumbnail url");
        //    var img = await WaitForMessage(Context.Message.Author, Context.Channel);
        //    await ReplyAsync("Enter the source url");
        //    var src = await WaitForMessage(Context.Message.Author, Context.Channel);

        //    var emb = new EmbedBuilder()
        //    {
        //        Title = title.Content,
        //        Description = desc.Content,
        //        ImageUrl = img.Content,
        //        Url = src.Content
        //    };

        //    await ReplyAsync("", false, emb);

        //}
    }
    public class GameJson
    {
        public string Command { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ThumbnailUrl { get; set; }
        public string Source { get; set; }
        public GameJson(string cmd, string title, string desc, string thumbnailurl, string src)
        {
            Command = cmd;
            Title = title;
            Description = desc;
            ThumbnailUrl = thumbnailurl;
            Source = src;
        }
        public GameJson ShallowCopy()
        {
            return (GameJson)this.MemberwiseClone();
        }
    }

}
