using System;
using System.Collections.Generic;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Events;

namespace ChatMotivator
{
    class Program
    {
        public static Obj_AI_Base Player = ObjectManager.Player;

        public static Menu ChatMenu, SettingsMenu;
        public static List<string> Messages;
        public static List<string> Starts;
        public static List<string> Endings;
        public static List<string> Ending;
        public static List<string> Smileys;
        public static List<string> Greetings;
        public static Dictionary<GameEventId, int> Rewards;
        public static Random Rand = new Random();

        public static int Kills = 0;
        public static int Deaths = 0;
        public static float CongratzTime = 0;
        public static float LastMessage = 0;

        public static Menu Settings;

        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Game_OnGameLoad;
            Game.OnLoad += Game_OnGameStart;
            Game.OnNotify += Game_OnGameNotifyEvent;
            Game.OnUpdate += Game_OnGameUpdate;
            Game.OnEnd += Game_OnGameEnd;
        }

        static void Game_OnGameLoad(EventArgs args)
        {
            SetupMenu();
            SetupRewards();
            SetupMessages();

            Chat.Print("<font color = \"#2fff0a\">ChatMotivator by xaxi</font>");
            
        }

        static void SetupMenu()
        {
            ChatMenu = MainMenu.AddMenu("ChatMotivator", "chat_motivator");
            ChatMenu.AddGroupLabel("Chat Settings");
            ChatMenu.AddSeparator();
            ChatMenu.AddLabel("By Xaxixeo");

            SettingsMenu = ChatMenu.AddSubMenu("Settings", "settings");
            SettingsMenu.AddGroupLabel("Settings");
            SettingsMenu.AddLabel("Chat");
            SettingsMenu.Add("sayGreeting", new CheckBox("Greeting"));
            SettingsMenu.Add("sayGreetingAllChat", new CheckBox("All Chat Greeting"));
            SettingsMenu.Add("sayGreetingDelayMin", new Slider("Greeting Min Delay", 30, 10, 120));
            SettingsMenu.Add("sayGreetingDelayMax", new Slider("Greeting Max Delay", 90, 10, 120));
            SettingsMenu.AddSeparator();
            SettingsMenu.Add("sayCongratulate", new CheckBox("Congratulate players"));
            SettingsMenu.Add("sayCongratulateDelayMin", new Slider("Congratulate Min Delay", 7, 5, 60));
            SettingsMenu.Add("sayCongratulateDelayMax", new Slider("Congratulate Max Delay", 15, 5, 60));
            SettingsMenu.Add("sayCongratulateInterval", new Slider("Interval Between Messages", 30, 5, 600));
            SettingsMenu.AddSeparator();
            SettingsMenu.Add("sayEnding", new CheckBox("End Message"));
        }

        static void SetupRewards()
        {
            Rewards = new Dictionary<GameEventId, int>
            {
                { GameEventId.OnChampionKill, 1 },  // champion kill
                { GameEventId.OnTurretKill, 1 }, // turret kill
            };
        }

        static void SetupMessages()
        {
            Messages = new List<string>
            {
                "gj", "good job", "very gj", "very good job", "wp", "well played",
                "well", "nicely played", "np", "amazing",
                "nice", "nice1", "nice one", "well done", "sweet", "good"
            };

            Starts = new List<string>
            {
                "", " ", "that was ",
                "  ", "wow ", "wow, "
            };

            Endings = new List<string>
            {
                "", " m8", " mate",
                " friend", " team",  " guys",
                " friends", " boys", " man"
            };

            Ending = new List<string>
            {
                "gg", "wp", "gg & wp",
                "good game", "well played"
            };

            Smileys = new List<string>
            {
                "",
                " ^^",
                " :p"
            };

            Greetings = new List<string>
            {
                "gl", "good luck", "hf", "have fun", "Good Luck & Have Fun",
                "Let's Fun", "gl hf", "gl and hf", "gl & hf",
                "Good Luck, Have Fun", "Let's all have a nice game!"
            };
        }

        static string GetRandomElement(List<string> collection, bool firstEmpty = true)
        {
            if (firstEmpty && Rand.Next(3) == 0)
                return collection[0];

            return collection[Rand.Next(collection.Count)];
        }

        static string GenerateMessage()
        {
            string message = GetRandomElement(Starts);
            message += GetRandomElement(Messages, false);
            message += GetRandomElement(Endings);
            message += GetRandomElement(Smileys);
            return message;
        }

        static string GenerateGreeting()
        {
            string greeting = GetRandomElement(Greetings, false);
            greeting += GetRandomElement(Smileys);
            return greeting;
        }

        static string GenerateEnding()
        {
            string ending = GetRandomElement(Ending, false);
            ending += GetRandomElement(Smileys);
            return ending;
        }

        static void SayCongratulations()
        {
            if (SettingsMenu["sayCongratulate"].Cast<CheckBox>().CurrentValue && Game.Time > LastMessage + SettingsMenu["sayCongratulateInterval"].Cast<Slider>().CurrentValue)
            {
                LastMessage = Game.Time;
                Chat.Say(GenerateMessage());
            }
        }

        static void SayGreeting()
        {
            if( SettingsMenu["sayGreetingAllChat"].Cast<CheckBox>().CurrentValue)
            {
                Chat.Say("/all " + GenerateGreeting());
            }
            else
            {
                Chat.Say(GenerateGreeting());
            }
        }
        static void SayEnding()
        {
            if( SettingsMenu["sayEnding"].Cast<CheckBox>().CurrentValue)
            {
                Chat.Say("/all " + GenerateEnding());
            }
        }

        static void Game_OnGameStart(EventArgs args)
        {
            if( !SettingsMenu["sayGreeting"].Cast<CheckBox>().CurrentValue)
            {
                return;
            }

            int minDelay = SettingsMenu["sayGreetingDelayMin"].Cast<Slider>().CurrentValue;
            int maxDelay = SettingsMenu["sayGreetingDelayMax"].Cast<Slider>().CurrentValue;

            // greeting message
            Core.DelayAction(SayGreeting, Rand.Next(Math.Min(minDelay, maxDelay), Math.Max(minDelay, maxDelay)) * 1000);
        }

        static void Game_OnGameEnd(EventArgs args)
        {
            if( !SettingsMenu["sayEnding"].Cast<CheckBox>().CurrentValue)
            {
                return;
            }
            const int minfDelay = 100;
            const int maxfDelay = 1001;
            //end message
            Core.DelayAction(SayEnding, Rand.Next(Math.Min(minfDelay, maxfDelay), Math.Max(minfDelay, maxfDelay)));
        }

        static void Game_OnGameUpdate(EventArgs args)
        {
            // champ kill message
            if (Kills > Deaths && CongratzTime < Game.Time && CongratzTime != 0)
            {
                SayCongratulations();

                Kills = 0;
                Deaths = 0;
                CongratzTime = 0;
            }
            else if (Kills != Deaths && CongratzTime < Game.Time)
            {
                Kills = 0;
                Deaths = 0;
                CongratzTime = 0;
            }            
        }

        static void Game_OnGameNotifyEvent(GameNotifyEventArgs args)
        {
            if( Rewards.ContainsKey( args.EventId ) )
            {
                var killer = ObjectManager.GetUnitByNetworkId<Obj_AI_Base>(args.NetworkId);
                
                if( killer.IsAlly )
                {
                    // we will not congratulate ourselves lol :D
                    if( (Kills == 0 && !killer.IsMe) || Kills > 0 )
                    {
                        Kills += Rewards[args.EventId];
                    }
                }
                else
                {
                    Deaths += Rewards[args.EventId];
                }
            }
            else
            {
                return;
            }

            int minDelay = SettingsMenu["sayCongratulateDelayMin"].Cast<Slider>().CurrentValue;
            int maxDelay = SettingsMenu["sayCongratulateDelayMax"].Cast<Slider>().CurrentValue;
     
            CongratzTime = Game.Time + Rand.Next( Math.Min(minDelay, maxDelay), Math.Max(minDelay, maxDelay) );
        }
    }
}