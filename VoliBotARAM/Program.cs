using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using LX_Orbwalker;
using System.Collections;

namespace VoliBotARAM
{
    class Program
    {
        static string[] assasins = new string[] { "Akali", "Diana", "Evelynn", "Fizz", "Katarina", "Nidalee" };
        static string[] adtanks = new string[] { "Braum", "DrMundo", "Garen", "Hecarim", "Jarvan IV", "Nasus", "Skarner", "Volibear", "Yorick", "Gnar" };
        static string[] adcs = new string[] { "Ashe", "Caitlyn", "Corki", "Draven", "Ezreal", "Gankplank", "Graves", "Jinx", "KogMaw", "Lucian", "MissFortune", "Quinn", "Sivir", "Thresh", "Tristana", "Tryndamere", "Twitch", "Urgot", "Varus", "Vayne", "Kalista" };
        static string[] mages = new string[] { "Ahri", "Anivia", "Annie","Azir", "Brand", "Cassiopeia", "Galio", "Gragas", "Heimerdinger", "Janna", "Karma", "Karthus", "LeBlanc", "Lissandra", "Lulu", "Lux", "Malzahar", "Morgana", "Nami", "Nunu", "Orianna", "Ryze", "Sona", "Soraka", "Swain", "Syndra", "Taric", "TwistedFate", "Veigar","Vel'Koz", "Viktor", "Xerath", "Ziggs", "Zilian", "Zyra" };
        static string[] hybrids = new string[] { "Kayle", "Teemo" };
        static string[] aptanks = new string[] { "Alistar", "Amumu", "Blitzcrank", "ChoGath", "Leona", "Malphite", "Maokai", "Nautilus", "Rammus", "Sejuani", "Shen", "Singed", "Zac" };
        static string[] bruisers = new string[] { "Darius", "Irelia", "Khazix", "LeeSin", "Olaf", "Pantheon", "Renekton", "Rengar", "Riven", "Shyvana", "Talon", "Trundle", "Vi", "Wukong", "Zed" };
        static string[] fighters = new string[] { "Aatrox", "Fiora", "Jax", "Jayce", "MasterYi", "Nocturne", "Poppy", "Sion", "Udyr", "Warwick", "XinZhao", "Yasuo" };
        static string[] apcs = new string[] { "Elise", "FiddleSticks", "Kennen", "Mordekaiser", "Rumble", "Vladimir" };
        static int[] abilityOrder, shopList;
        static Spell Q, W, E, R;
        static int heroType, heroState, buyIndex, qOff, wOff, eOff, rOff = 0;
        static Obj_AI_Hero player = ObjectManager.Player;
        static Menu Menu;
        static GameObject Objects { get; set; }
        static Vector2 buff1 = new Vector2(8922, 7869);
        static Vector2 buff2 = new Vector2(7473, 6617);
        static Vector2 buff3 = new Vector2(5929, 5190);
        static Vector2 buff4 = new Vector2(4751, 3901);
        static Vector2 lastPos;
        static bool isOrder, isWalkingToBuff, buffStatus1, buffStatus2, buffStatus3, buffStatus4 = false;
        static Obj_AI_Base minion;
        static Obj_AI_Base nearestMinion;
        static Vector2 lastMinionPos;
        
        public static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        public static void Game_OnGameLoad(EventArgs args)
        {
            Game.PrintChat("VoliBot's ARAM Script for LeagueSharp!");
            Menu = new Menu("ARAM", player.ChampionName, true);
            Menu.AddSubMenu(new Menu("Debug", "Debug"));
            Menu.SubMenu("Debug").AddItem(new MenuItem("debug", "Debug Script").SetValue(true));
            getHeroType();
            getShopList();
            getAbilityOrder();
            if (isOrder)
            {
                moveTo(new Vector2(5340, 6185));
            }
            else
            {
                moveTo(new Vector2(6608, 7425));
            }
            Game.OnGameUpdate += OnTick;
            GameObject.OnCreate += OnCreateObject;
            GameObject.OnDelete += OnDeleteObject;
            CustomEvents.Unit.OnLevelUp += OnLevelUp;
            Drawing.OnDraw += Drawing_OnDraw;
            
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            Utility.DrawCircle(minion.Position, 500, System.Drawing.Color.Red);
        }

        public static void OnTick(EventArgs args)
        {
            if (!player.IsDead)
            {
                switch (heroState)
                {
                    case 0: //Default
                        if (player.IsAutoAttacking)
                        {
                            return;
                        }
                        var HealthPercentage = player.MaxHealth / player.Health;
                        if(HealthPercentage > 3)
                        {
                            Debug("Low Health Mode");
                            heroState = 1;
                        }
                        FarmAndAttack();
                        FollowAlly();
                        break;
                    case 1: //Low Health
                        if (!isOrder && buffStatus1 == true)
                        {
                            if (!isWalkingToBuff)
                            {
                                moveTo(buff1);
                                isWalkingToBuff = true;
                            }
                            if (player.Position.To2D() == buff1)
                            {
                                heroState = 0;
                                isWalkingToBuff = false;
                            }
                        } else if (!isOrder && buffStatus2 == true)
                        {
                            if (!isWalkingToBuff)
                            {
                                moveTo(buff2);
                                isWalkingToBuff = true;
                            }
                            if (player.Position.To2D() == buff1)
                            {
                                heroState = 0;
                                isWalkingToBuff = false;
                            }
                        } else if (isOrder && buffStatus3 == true)
                        {
                            if (!isWalkingToBuff)
                            {
                                lastPos = player.Position.To2D();
                                isWalkingToBuff = true;
                            }
                            if (player.Position.To2D() == buff1)
                            {
                                heroState = 0;
                                isWalkingToBuff = false;
                            }
                        } else if (isOrder && buffStatus4 == true)
                        {
                            if (!isWalkingToBuff)
                            {
                                moveTo(buff4);
                                isWalkingToBuff = true;
                            }
                            if (player.Position.To2D() == buff1)
                            {
                                heroState = 0;
                                isWalkingToBuff = false;
                            }
                        }
                        break;
                    case 2: // TeamFight Mode
                        break;
                    case 3: // Alone
                        break;
                    default:
                        break;
                }
            }
            if (player.IsDead || Utility.InShopRange())
            {
                if(shopList[buyIndex] != 0){
                    int thisItem = shopList[buyIndex];                    
                    if (!hasItem(thisItem))
                    {
                        buyItem(thisItem);
                    }
                    if (hasItem(thisItem))
                    {
                        buyIndex += 1;
                        Debug(buyIndex + "");
                    }

                }
            }
        }

        public static void OnCreateObject(GameObject sender, EventArgs args)
        {
            if (sender.Name.Contains("HA_AP_HealthRelic4.1.1"))
            {
                Packet.S2C.Ping.Encoded(new Packet.S2C.Ping.Struct(sender.Position.X, sender.Position.Y)).Process();
                buffStatus4 = true;
            }
            if (sender.Name.Contains("HA_AP_HealthRelic3.1.1"))
            {
                Packet.S2C.Ping.Encoded(new Packet.S2C.Ping.Struct(sender.Position.X, sender.Position.Y)).Process();
                buffStatus3 = true;
            }
            if (sender.Name.Contains("HA_AP_HealthRelic2.1.1"))
            {
                Packet.S2C.Ping.Encoded(new Packet.S2C.Ping.Struct(sender.Position.X, sender.Position.Y)).Process();
                buffStatus2 = true;
            }
            if (sender.Name.Contains("HA_AP_HealthRelic1.1.1"))
            {
                Packet.S2C.Ping.Encoded(new Packet.S2C.Ping.Struct(sender.Position.X, sender.Position.Y)).Process();
                buffStatus1 = true;
            }
        }

        public static void OnDeleteObject(GameObject sender, EventArgs args)
        {
            if (sender.Name.Contains("HA_AP_HealthRelic4.1.1"))
            {
                Packet.S2C.Ping.Encoded(new Packet.S2C.Ping.Struct(sender.Position.X, sender.Position.Y)).Process();
                buffStatus4 = false;
            }
            if (sender.Name.Contains("HA_AP_HealthRelic3.1.1"))
            {
                Packet.S2C.Ping.Encoded(new Packet.S2C.Ping.Struct(sender.Position.X, sender.Position.Y)).Process();
                buffStatus3 = false;
            }
            if (sender.Name.Contains("HA_AP_HealthRelic2.1.1"))
            {
                Packet.S2C.Ping.Encoded(new Packet.S2C.Ping.Struct(sender.Position.X, sender.Position.Y)).Process();
                buffStatus2 = false;
            }
            if (sender.Name.Contains("HA_AP_HealthRelic1.1.1"))
            {
                Packet.S2C.Ping.Encoded(new Packet.S2C.Ping.Struct(sender.Position.X, sender.Position.Y)).Process();
                buffStatus1 = false;
            }
        }
        
        public static bool inDistance(Vector2 pos1, Vector2 pos2, float distance)
        {
            float dist2 = Vector2.DistanceSquared(pos1, pos2);
            return (dist2 <= distance * distance) ? true : false;
        }
        
        public static void moveTo(Vector2 Pos)
        {
            Debug("Move to: "+ Pos.ToString());
            player.IssueOrder(GameObjectOrder.MoveTo, Pos.To3D());
        }
        
        public static void OnLevelUp(Obj_AI_Base sender, CustomEvents.Unit.OnLevelUpEventArgs args)
        {
            if ((qOff + wOff + eOff + rOff) < player.Level)
            {
                int i = player.Level - 1;
                SpellSlot abilitySlot;
                if (abilityOrder[i] == 1)
                {
                    abilitySlot = SpellSlot.Q;
                }
                else if (abilityOrder[i] == 2)
                {
                    abilitySlot = SpellSlot.W;
                }
                else if (abilityOrder[i] == 3)
                {
                    abilitySlot = SpellSlot.E;
                }
                else if (abilityOrder[i] == 4)
                {
                    abilitySlot = SpellSlot.R;
                } else
                {
                    abilitySlot = SpellSlot.Q;
                }
                ObjectManager.Player.Spellbook.LevelUpSpell(abilitySlot);
            }
        }
        
        public static void buyItem(int id)
        {
            Packet.C2S.BuyItem.Encoded(new Packet.C2S.BuyItem.Struct(id, ObjectManager.Player.NetworkId)).Send();        
        }
        
        public static bool hasItem(int id)
        {
            return Items.HasItem(id, player);
        }
        
        public static SpellSlot getSpell(int id)
        {
            switch (id)
            {
                case 1:
                    return SpellSlot.Q;
                case 2:
                    return SpellSlot.W;
                case 3:
                    return SpellSlot.E;
                case 4:
                    return SpellSlot.R;
                default:
                    return SpellSlot.Q;

            }
        }
        
        public static void getShopList()
        {
            switch(heroType)
            {
                case 1:
                    shopList = new int[] { 3006, 1042, 3086, 3087, 3144, 3153, 1038, 3181, 1037, 3035, 3026, 0 };
                    break;
                case 2:
                    shopList = new int[] { 3047, 1011, 3134, 3068, 3024, 3025, 3071, 3082, 3143, 3005, 0 };
                    break;
                case 3:
                    shopList = new int[] { 3111, 1031, 3068, 1057, 3116, 1026, 3001, 3082, 3110, 3102, 0 };
                    break;
                case 4:
                    shopList = new int[] { 1001, 3108, 3115, 3020, 1026, 3136, 3089, 1043, 3091, 3151, 3116 };
                    break;
                case 5:
                    shopList = new int[] { 3111, 3134, 1038, 3181, 3155, 3071, 1053, 3077, 3074, 3156, 3190 };
                    break;
                case 6:
                    shopList = new int[] { 3020, 3057, 3100, 1026, 3089, 3136, 3151, 1058, 3157, 3135, 0 };
                    break;
                case 7:
                    shopList = new int[] { 3028, 1001, 3020, 3136, 1058, 3089, 3174, 3151, 1026, 3001, 3135, 0 };
                    break;
                case 8:
                    shopList = new int[] { 3145, 3020, 3152, 1026, 3116, 1058, 3089, 1026, 3001, 3157 };
                    break;
                case 9:
                    shopList = new int[] { 3111, 3044, 3086, 3078, 3144, 3153, 3067, 3065, 3134, 3071, 3156, 0 };
                    break;
                default:
                    shopList = new int[] { 3111, 3044, 3086, 3078, 3144, 3153, 3067, 3065, 3134, 3071, 3156, 0 };
                    break;
            }
        }
        
        public static void getHeroType()
        {
            if (player.Team.ToString() == "Order")
            {
                isOrder = true;
            }
            else
            {
                isOrder = false;
            }
            var Hero = ObjectManager.Player.ChampionName;
            Game.PrintChat(Hero.ToString());
            if (assasins.Contains(Hero.ToString()))
            {
                heroType = 1;
                Debug("HeroType: Assasin!");
            }
            if (adtanks.Contains(Hero.ToString()))
            {
                heroType = 2;
                Debug("HeroType: AdTank!");
            }
            if (adcs.Contains(Hero.ToString()))
            {
                heroType = 3;
                Debug("HeroType: ADC!");
            }
            if (mages.Contains(Hero.ToString()))
            {
                heroType = 4;
                Debug("HeroType: Mage!");
            }
            if (hybrids.Contains(Hero.ToString()))
            {
                heroType = 5;
                Debug("HeroType: Hybrid!");
            }
            if (aptanks.Contains(Hero.ToString()))
            {
                heroType = 6;
                Debug("HeroType: ApTank!");
            }
            if (bruisers.Contains(Hero.ToString()))
            {
                heroType = 7;
                Debug("HeroType: Bruiser!");
            }
            if (fighters.Contains(Hero.ToString()))
            {
                heroType = 8;
                Debug("HeroType: Fighter!");
            }
            if (apcs.Contains(Hero.ToString()))
            {
                heroType = 9;
                Debug("HeroType: APC!");
            }
            if (heroType == 0)
            {
                heroType = 10;
                Debug("HeroType: DEFAULT!");
            }
        }
        
        public static void getAbilityOrder()
        {
            string champName = player.ChampionName;
            switch (champName)
            {
                case "Aatrox":
                    abilityOrder = new int[] { 1, 2, 3, 2, 2, 4, 2, 3, 2, 3, 4, 3, 3, 1, 1, 4, 1, 1, };
                    break;
                case "Ahri":
                    abilityOrder = new int[] { 1, 3, 1, 2, 1, 4, 1, 2, 1, 2, 4, 2, 2, 3, 3, 4, 2, 2, };
                    break;
                case "Akali":
                    abilityOrder = new int[] { };
                    break;
                case "Alistar":
                    abilityOrder = new int[] { };
                    break;
                case "Amumu":
                    abilityOrder = new int[] { };
                    break;
                case "Anivia":
                    abilityOrder = new int[] { };
                    break;
                case "Annie":
                    abilityOrder = new int[] { };
                    break;
                case "Ashe":
                    abilityOrder = new int[] { };
                    break;
                case "Azir":
                    abilityOrder = new int[] { };
                    break;
                case "Blitzcrank":
                    abilityOrder = new int[] { };
                    break;
                case "Brand":
                    abilityOrder = new int[] { };
                    break;
                case "Braum":
                    abilityOrder = new int[] { };
                    break;
                case "Caitlyn":
                    abilityOrder = new int[] { };
                    break;
                case "Cassiopeia":
                    abilityOrder = new int[] { };
                    break;
                case "Chogath":
                    abilityOrder = new int[] { };
                    break;
                case "Corki":
                    abilityOrder = new int[] { };
                    break;
                case "Darius":
                    abilityOrder = new int[] { };
                    break;
                case "Diana":
                    abilityOrder = new int[] { };
                    break;
                case "DrMundo":
                    abilityOrder = new int[] { };
                    break;
                case "Draven":
                    abilityOrder = new int[] { };
                    break;
                case "Elise":
                    abilityOrder = new int[] { };
                    rOff = -1;
                    break;
                case "Evelynn":
                    abilityOrder = new int[] { };
                    break;
                case "Ezreal":
                    abilityOrder = new int[] { };
                    break;
                case "FiddleSticks":
                    abilityOrder = new int[] { };
                    break;
                case "Fiora":
                    abilityOrder = new int[] { };
                    break;
                case "Fizz":
                    abilityOrder = new int[] { };
                    break;
                case "Galio":
                    abilityOrder = new int[] { };
                    break;
                case "Gangplank":
                    abilityOrder = new int[] { };
                    break;
                case "Garen":
                    abilityOrder = new int[] { };
                    break;
                case "Gnar":
                    abilityOrder = new int[] { };
                    break;
                case "Gragas":
                    abilityOrder = new int[] { };
                    break;
                case "Graves":
                    abilityOrder = new int[] { };
                    break;
                case "Hecarim":
                    abilityOrder = new int[] { };
                    break;
                case "Heimerdinger":
                    abilityOrder = new int[] { };
                    break;
                case "Irelia":
                    abilityOrder = new int[] { };
                    break;
                case "Janna":
                    abilityOrder = new int[] { };
                    break;
                case "JarvanIV":
                    abilityOrder = new int[] { };
                    break;
                case "Jax":
                    abilityOrder = new int[] { };
                    break;
                case "Jayce":
                    abilityOrder = new int[] { };
                    rOff = -1;
                    break;
                case "Jinx":
                    abilityOrder = new int[] { };
                    break;
                case "Karma":
                    abilityOrder = new int[] { };
                    break;
                case "Karthus":
                    abilityOrder = new int[] { };
                    break;
                case "Kassadin":
                    abilityOrder = new int[] { };
                    break;
                case "Katarina":
                    abilityOrder = new int[] { };
                    break;
                case "Kayle":
                    abilityOrder = new int[] { };
                    break;
                case "Kennen":
                    abilityOrder = new int[] { };
                    break;
                case "Khazix":
                    abilityOrder = new int[] { };
                    break;
                case "KogMaw":
                    abilityOrder = new int[] { };
                    break;
                case "Leblanc":
                    abilityOrder = new int[] { };
                    break;
                case "LeeSin":
                    abilityOrder = new int[] { };
                    break;
                case "Leona":
                    abilityOrder = new int[] { };
                    break;
                case "Lissandra":
                    abilityOrder = new int[] { };
                    break;
                case "Lucian":
                    abilityOrder = new int[] { };
                    break;
                case "Lulu":
                    abilityOrder = new int[] { };
                    break;
                case "Lux":
                    abilityOrder = new int[] { };
                    break;
                case "Malphite":
                    abilityOrder = new int[] { };
                    break;
                case "Malzahar":
                    abilityOrder = new int[] { };
                    break;
                case "Maokai":
                    abilityOrder = new int[] { };
                    break;
                case "MasterYi":
                    abilityOrder = new int[] { };
                    break;
                case "MissFortune":
                    abilityOrder = new int[] { };
                    break;
                case "MonkeyKing":
                    abilityOrder = new int[] { };
                    break;
                case "Mordekaiser":
                    abilityOrder = new int[] { };
                    break;
                case "Morgana":
                    abilityOrder = new int[] { };
                    break;
                case "Nami":
                    abilityOrder = new int[] { };
                    break;
                case "Nasus":
                    abilityOrder = new int[] { };
                    break;
                case "Nautilus":
                    abilityOrder = new int[] { };
                    break;
                case "Nidalee":
                    abilityOrder = new int[] { };
                    break;
                case "Nocturne":
                    abilityOrder = new int[] { };
                    break;
                case "Nunu":
                    abilityOrder = new int[] { };
                    break;
                case "Olaf":
                    abilityOrder = new int[] { };
                    break;
                case "Orianna":
                    abilityOrder = new int[] { };
                    break;
                case "Pantheon":
                    abilityOrder = new int[] { };
                    break;
                case "Poppy":
                    abilityOrder = new int[] { };
                    break;
                case "Quinn":
                    abilityOrder = new int[] { };
                    break;
                case "Rammus":
                    abilityOrder = new int[] { };
                    break;
                case "Renekton":
                    abilityOrder = new int[] { };
                    break;
                case "Rengar":
                    abilityOrder = new int[] { };
                    break;
                case "Riven":
                    abilityOrder = new int[] { };
                    break;
                case "Rumble":
                    abilityOrder = new int[] { };
                    break;
                case "Ryze":
                    abilityOrder = new int[] { };
                    break;
                case "Sejuani":
                    abilityOrder = new int[] { };
                    break;
                case "Shaco":
                    abilityOrder = new int[] { };
                    break;
                case "Shen":
                    abilityOrder = new int[] { };
                    break;
                case "Shyvana":
                    abilityOrder = new int[] { };
                    break;
                case "Singed":
                    abilityOrder = new int[] { };
                    break;
                case "Sion":
                    abilityOrder = new int[] { };
                    break;
                case "Sivir":
                    abilityOrder = new int[] { };
                    break;
                case "Skarner":
                    abilityOrder = new int[] { };
                    break;
                case "Sona":
                    abilityOrder = new int[] { };
                    break;
                case "Soraka":
                    abilityOrder = new int[] { };
                    break;
                case "Swain":
                    abilityOrder = new int[] { };
                    break;
                case "Syndra":
                    abilityOrder = new int[] { };
                    break;
                case "Talon":
                    abilityOrder = new int[] { };
                    break;
                case "Taric":
                    abilityOrder = new int[] { };
                    break;
                case "Teemo":
                    abilityOrder = new int[] { };
                    break;
                case "Thresh":
                    abilityOrder = new int[] { };
                    break;
                case "Tristana":
                    abilityOrder = new int[] { };
                    break;
                case "Trundle":
                    abilityOrder = new int[] { };
                    break;
                case "Tryndamere":
                    abilityOrder = new int[] { };
                    break;
                case "TwistedFate":
                    abilityOrder = new int[] { };
                    break;
                case "Twitch":
                    abilityOrder = new int[] { };
                    break;
                case "Udyr":
                    abilityOrder = new int[] { };
                    break;
                case "Urgot":
                    abilityOrder = new int[] { };
                    break;
                case "Varus":
                    abilityOrder = new int[] { };
                    break;
                case "Vayne":
                    abilityOrder = new int[] { };
                    break;
                case "Veigar":
                    abilityOrder = new int[] { };
                    break;
                case "Vel'Koz":
                    abilityOrder = new int[] { };
                    break;
                case "Vi":
                    abilityOrder = new int[] { };
                    break;
                case "Viktor":
                    abilityOrder = new int[] { };
                    break;
                case "Vladimir":
                    abilityOrder = new int[] { };
                    break;
                case "Volibear":
                    abilityOrder = new int[] { };
                    break;
                case "Warwick":
                    abilityOrder = new int[] { };
                    break;
                case "Xerath":
                    abilityOrder = new int[] { };
                    break;
                case "XinZhao":
                    abilityOrder = new int[] { };
                    break;
                case "Yasuo":
                    abilityOrder = new int[] { };
                    break;
                case "Yorick":
                    abilityOrder = new int[] { };
                    break;
                case "Zac":
                    abilityOrder = new int[] { };
                    break;
                case "Zed":
                    abilityOrder = new int[] { };
                    break;
                case "Ziggs":
                    abilityOrder = new int[] { };
                    break;
                case "Zilean":
                    abilityOrder = new int[] { };
                    break;
                case "Zyra":
                    abilityOrder = new int[] { };
                    break;
                default:
                    break;
            }
            if (player.Level == 3)
            {
                ObjectManager.Player.Spellbook.LevelUpSpell(getSpell(abilityOrder[0]));
                ObjectManager.Player.Spellbook.LevelUpSpell(getSpell(abilityOrder[1]));
                ObjectManager.Player.Spellbook.LevelUpSpell(getSpell(abilityOrder[2]));
            }
            else if (player.Level == 1)
            {
                ObjectManager.Player.Spellbook.LevelUpSpell(getSpell(abilityOrder[0]));
            }
        }
        
        public static void FarmAndAttack()
        {
            foreach (Obj_AI_Base enemy in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsEnemy))
            if (!IsInsideEnemyTower(player.Position))
            {
                ObjectManager.Player.IssueOrder(GameObjectOrder.AttackUnit, enemy);
                return;
            }
            foreach (Obj_AI_Base turret in ObjectManager.Get<Obj_AI_Turret>().Where(turret => turret.IsValidTarget(GetAutoAttackRange(player, turret))))
            if (turret.IsEnemy)
            {
                ObjectManager.Player.IssueOrder(GameObjectOrder.AttackUnit, turret);
                return;
            }
            minion = MinionManager.GetMinions(player.ServerPosition, player.AttackRange, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.Health).FirstOrDefault();
            if (minion == null)
                minion = MinionManager.GetMinions(player.ServerPosition, player.AttackRange).FirstOrDefault();
            if (minion.Health < player.BaseAttackDamage || minion.Health == minion.MaxHealth)
            {
                ObjectManager.Player.IssueOrder(GameObjectOrder.AttackUnit, minion);
            }
          
        }
        
        public static void FollowAlly()
        {
            var toFollow = ObjectManager.Get<Obj_AI_Hero>().FirstOrDefault(hero => hero.IsAlly && !hero.IsDead && !hero.IsMe);
            if(toFollow == null || toFollow == player)
            {
                WalkToNearestMinion();
            }
            var V = Vector3.Normalize(toFollow.Position - ObjectManager.Player.Position);
            var V2 = toFollow.Position - V * 150;
            ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, V2);
        }
        
        public static void WalkToNearestMinion()
        {
            nearestMinion =
                MinionManager.GetMinions(player.ServerPosition, 1500, MinionTypes.Melee, MinionTeam.Ally,
                    MinionOrderTypes.Health).FirstOrDefault();
            if (nearestMinion == null)
            {
                nearestMinion = MinionManager.GetMinions(player.ServerPosition, 1500).FirstOrDefault();
            }
            if (lastMinionPos != nearestMinion.Position.To2D())
            {
                moveTo(nearestMinion.Position.To2D());
                lastMinionPos = nearestMinion.Position.To2D();
            }
        }
        
        public static bool IsInsideEnemyTower(Vector3 position)
        {
            return ObjectManager.Get<Obj_AI_Turret>()
                                    .Any(tower => tower.IsEnemy && tower.Health > 0 && tower.Position.Distance(position) < 775);
        }
        
        public static float GetAutoAttackRange(Obj_AI_Base source = null, Obj_AI_Base target = null)
        {
            if (source == null)
                source = player;
            var ret = source.AttackRange + player.BoundingRadius;
            if (target != null)
                ret += target.BoundingRadius;
            return ret;
        }
        
        public static void Debug(string msg)
        {
            if (Menu.Item("debug").GetValue<bool>())
            {
                Game.PrintChat("<font color='#FF0000'>[DEBUG]:</font> " + msg);
            }
        } 
    }
}
