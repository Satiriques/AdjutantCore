﻿using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using System.IO;

namespace DiscordExampleBot
{
    public class Program
    {
        // Convert our sync main to an async main.
        public static void Main(string[] args) =>
            new Program().Start().GetAwaiter().GetResult();

        private DiscordSocketClient client;
        private CommandHandler handler;

        public async Task Start()
        {
            Console.WriteLine("start");
            // Define the DiscordSocketClient
            client = new DiscordSocketClient();

            var token = File.ReadAllText("Token.txt");

            client.Connected += async () =>
            {
                await client.SetGameAsync("?help");
            };

            // Login and connect to Discord.
            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();

            var map = new DependencyMap();
            map.Add(client);

            handler = new CommandHandler();
            await handler.Install(map);
            Console.WriteLine("Connected");
            // Block this program until it is closed.
            await Task.Delay(-1);
           
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}