using Discord;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Diagnostics;

namespace Sassafras
{
    public class Program
    {
        public static void Main(string[] args)
            => new Program().ConnectBot().GetAwaiter().GetResult();

        // Connects the User/Client/Bot amalgamation to Discord.
        public async Task ConnectBot()
        {
            CreateAliasFile();

            var client = new DiscordClient();
            String[] creds = getCreds();

            while (true)
            {
                try
                {
                    await client.Connect(creds[0], creds[1]);
                    break;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine("(This probably means your credentials are bad! Try logging in again.)\n\n");
                    creds = getCreds();
                }
            }

            Console.WriteLine("Successfully connected.\n");
            InitMessageHandler(client);

            // Block this task until the program is closed.
            await Task.Delay(-1);
        }


        // Returns the users Email and password, so we can log in.    
        public String[] getCreds()
        {
            String[] creds = { "", "" };

            Console.WriteLine("Please enter your E-Mail and password\nOr type 'Edit' to access the program directory (where the alias file is located).\n");
            Console.Write("E-Mail: ");
            creds[0] = Console.ReadLine();

            if (creds[0].ToLower().Equals("edit"))
            {
                Process.Start(Directory.GetCurrentDirectory());
                Console.Clear();
                getCreds();
            }
            else
            {
                Console.Write("Password: ");

                ConsoleKeyInfo key;
                do
                {
                    key = Console.ReadKey(true);

                    // Backspace Should Not Work
                    if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                    {
                        creds[1] += key.KeyChar;
                        Console.Write("*");
                    }
                    else
                    {
                        if (key.Key == ConsoleKey.Backspace && creds[1].Length > 0)
                        {
                            creds[1] = creds[1].Substring(0, (creds[1].Length - 1));
                            Console.Write("\b \b");
                        }
                    }

                }
                // Stops Receving Keys Once Enter is Pressed
                while (key.Key != ConsoleKey.Enter);
                Console.WriteLine();
            }

            return creds;
        }


        // Turns on our message handler
        public static void InitMessageHandler(DiscordClient bot)
        {
            bot.MessageReceived += (s, e) =>
            {
                if (e.User.Id == bot.CurrentUser.Id)
                {
                    var path = Directory.GetCurrentDirectory() + "\\alias.json";
                    Dictionary<String, String> aliases = null;
                    try
                    {
                        aliases = JsonConvert.DeserializeObject<Dictionary<String, String>>(File.ReadAllText(path));
                    } catch
                    {
                        Console.WriteLine("[Error] The alias file is innaccessable!");
                    }
                    
                    foreach (KeyValuePair<String,String> kv in aliases)
                    {
                        if (e.Message.Text.Contains(kv.Key))
                        {
                            e.Message.Edit(e.Message.Text.Replace(kv.Key, kv.Value));
                        }
                    }

                }
            };
        }

        /**
         * Checks for an alias file.
         * If an alias file is found, the program moves on.
         * If no alias file is found, the user is asked if they want to make one.
         * If they do, it will attempt to make one.
         * If it fails, or if they don't want to, the program will close, as it can not run without the alias file.
         */
        public void CreateAliasFile()
        {
            String path = Directory.GetCurrentDirectory();

            if (File.Exists(path + "\\alias.json"))
            {
                Console.WriteLine("Alias file found.\n");
            } else
            {
                Console.WriteLine("No alias file found. Would you like to create a new one?\n");
                var x = Console.ReadLine();

                if (x.ToLower().StartsWith("y"))
                {
                    try
                    {
                        File.Create(path + "\\alias.json");
                        Console.WriteLine("The alias file has been created:\n" + path + "\\alias.json"); 
                    } catch (Exception e)
                    {
                        Console.WriteLine(
                            "Uh oh, something bad happened while attempting to make the alias file.\n" +
                            "This can happen for many reasons, such as your account on the computer not having enough permissions,\n" +
                            "the folder this program is in being read-only, or God hating you. If the problem persists, try running me as an admin.\n" +
                            "Or, just go in and put a file called 'alias.json' next to me, that works too...\n Press ENTER to end..."
                            );
                        Console.ReadLine();
                        Environment.Exit(0);
                    }
                    
                    CreateAliasFile();
                } else
                {
                    Console.WriteLine(
                            "\nThis program requires an alias file to run.\nIf you don't want the program to create it's own, set a file named 'alias.json' next to it.\n" +
                            "Press ENTER to end..."
                            );
                    Console.ReadLine();
                    Environment.Exit(0);
                }
            }

        }



    }
}