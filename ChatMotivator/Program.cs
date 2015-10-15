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
        public static Random rand = new Random();

        public static int kills = 0;
        public static int deaths = 0;
        public static float congratzTime = 0;
        public static float lastMessage = 0;

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
            setupMenu();
            setupRewards();
            setupMessages();

            Chat.Print("<font color = \"#2fff0a\">ChatMotivator by xaxi</font>");
            
        }

        static void setupMenu()
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

        static void setupRewards()
        {
            Rewards = new Dictionary<GameEventId, int>
            {
                { GameEventId.OnChampionKill, 1 },  // champion kill
                { GameEventId.OnTurretKill, 1 }, // turret kill
            };
        }

        static void setupMessages()
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

        static string getRandomElement(List<string> collection, bool firstEmpty = true)
        {
            if (firstEmpty && rand.Next(3) == 0)
                return collection[0];

            return collection[rand.Next(collection.Count)];
        }

        static string generateMessage()
        {
            string message = getRandomElement(Starts);
            message += getRandomElement(Messages, false);
            message += getRandomElement(Endings);
            message += getRandomElement(Smileys);
            return message;
        }

        static string generateGreeting()
        {
            string greeting = getRandomElement(Greetings, false);
            greeting += getRandomElement(Smileys);
            return greeting;
        }

        static string generateEnding()
        {
            string ending = getRandomElement(Ending, false);
            ending += getRandomElement(Smileys);
            return ending;
        }

        static void sayCongratulations()
        {
            if (SettingsMenu["sayCongratulate"].Cast<CheckBox>().CurrentValue && Game.Time > lastMessage + SettingsMenu["sayCongratulateInterval"].Cast<Slider>().CurrentValue)
            {
                lastMessage = Game.Time;
                Chat.Print(generateMessage());
            }
        }

        static void sayGreeting()
        {
            if( SettingsMenu["sayGreetingAllChat"].Cast<CheckBox>().CurrentValue)
            {
                Chat.Print("/all " + generateGreeting());
            }
            else
            {
                Chat.Print(generateGreeting());
            }
        }
        static void sayEnding()
        {
            if( SettingsMenu["sayEnding"].Cast<CheckBox>().CurrentValue)
            {
                Chat.Print("/all " + generateEnding());
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
            Core.DelayAction(sayGreeting, rand.Next(Math.Min(minDelay, maxDelay), Math.Max(minDelay, maxDelay)) * 1000);
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
            Core.DelayAction(sayEnding, rand.Next(Math.Min(minfDelay, maxfDelay), Math.Max(minfDelay, maxfDelay)));
        }

        static void Game_OnGameUpdate(EventArgs args)
        {
            // champ kill message
            if (kills > deaths && congratzTime < Game.Time && congratzTime != 0)
            {
                sayCongratulations();

                kills = 0;
                deaths = 0;
                congratzTime = 0;
            }
            else if (kills != deaths && congratzTime < Game.Time)
            {
                kills = 0;
                deaths = 0;
                congratzTime = 0;
            }            
        }

        static void Game_OnGameNotifyEvent(GameNotifyEventArgs args)
        {
            if( Rewards.ContainsKey( args.EventId ) )
            {
                Obj_AI_Base Killer = ObjectManager.GetUnitByNetworkId<Obj_AI_Base>((uint)args.NetworkId);
                
                if( Killer.IsAlly )
                {
                    // we will not congratulate ourselves lol :D
                    if( (kills == 0 && !Killer.IsMe) || kills > 0 )
                    {
                        kills += Rewards[args.EventId];
                    }
                }
                else
                {
                    deaths += Rewards[args.EventId];
                }
            }
            else
            {
                return;
            }

            int minDelay = SettingsMenu["sayCongratulateDelayMin"].Cast<Slider>().CurrentValue;
            int maxDelay = SettingsMenu["sayCongratulateDelayMax"].Cast<Slider>().CurrentValue;
     
            congratzTime = Game.Time + rand.Next( Math.Min(minDelay, maxDelay), Math.Max(minDelay, maxDelay) );
        }
    }
}