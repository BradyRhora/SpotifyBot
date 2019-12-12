﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.IO;
using System.Reflection;

namespace SpotifyBot
{
    class Bot
    {
        static void Main(string[] args) => new Bot().Run().GetAwaiter().GetResult();

        Random rdm = new Random();

        public static Discord.WebSocket.DiscordSocketClient client;
        public static CommandService commands;

        public async Task Run()
        {
            try
            {
                DiscordSocketConfig config = new DiscordSocketConfig() { MessageCacheSize = 1000 };
                Console.WriteLine("Welcome. Initializing ForkBot...");
                client = new DiscordSocketClient(config);
                Console.WriteLine("Client Initialized.");
                commands = new CommandService();
                Console.WriteLine("Command Service Initialized.");
                await InstallCommands();
                Console.WriteLine("Commands Installed, logging in.");
                if (!File.Exists("Constants/bottoken"))
                {
                    File.WriteAllText("Constants/bottoken", "");
                    Console.WriteLine("Created bottoken file in Constants folder, you will need to put the token in this file.");
                }
                await client.LoginAsync(TokenType.Bot, File.ReadAllText("Constants/bottoken"));

                Console.WriteLine("Successfully logged in!");
                await client.StartAsync();
                Console.WriteLine($"Bot successfully initialized.");
                await Task.Delay(-1);
            }
            catch (Exception e)
            {
                Console.WriteLine("\n==========================================================================");
                Console.WriteLine("                                  ERROR                        ");
                Console.WriteLine("==========================================================================\n");
                Console.WriteLine($"Error occured in {e.Source}");
                Console.WriteLine(e.Message);
                Console.WriteLine(e.InnerException);

                Console.Read();
            }
        }
        public async Task InstallCommands()
        {
            client.MessageReceived += HandleCommand;
            await commands.AddModulesAsync(Assembly.GetEntryAssembly(), services: null);
        }

        public async Task HandleCommand(SocketMessage messageParam)
        {
            SocketUserMessage message = messageParam as SocketUserMessage;
            if (message == null) return;
            if (message.Author.Id == client.CurrentUser.Id) return; //doesn't allow the bot to respond to itself

            int argPos = 0;
            //detect and execute commands
            if (message.HasCharPrefix(';', ref argPos))
            {
                // new user prevention
                var userCreationDate = message.Author.CreatedAt;
                var existenceTime = DateTime.UtcNow.Subtract(userCreationDate.DateTime);
                var week = new TimeSpan(7, 0, 0, 0);

                var context = new CommandContext(client, message);
                var result = await commands.ExecuteAsync(context, argPos, services: null);

                if (!result.IsSuccess)
                {
                    if (result.Error != CommandError.UnknownCommand)
                    {
                        Console.WriteLine(result.ErrorReason);
                        await message.Channel.SendMessageAsync("ERROR:\n" + result.ErrorReason);
                    }
                }
            }
            else return;

        }
    }
}
