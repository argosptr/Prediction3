using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ensage;
using Ensage.Common;
using SharpDX;
using SharpDX.Direct3D9;
using Ensage.Common.Extensions;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Runtime.InteropServices;


namespace Prediction3
{
    class Program
    {

        static void Main(string[] args)
        {
            Variables.BottomRune.current = false;
            Variables.TopRune.current = false;
            Variables.TopRune.rune = new Rune();
            Variables.BottomRune.rune = new Rune();
            Drawing.OnDraw += Drawing_OnDraw; //Graphical Drawer
        }
        #region Not in use
        private static void Drawing_OnEndScene(EventArgs args)
        {
            if (Variables.DrawNotification)
            {
                int width = Variables.font.MeasureText(null, Variables.NotificationText, FontDrawFlags.Left).Width / 2;
                Variables.font.DrawText(null, Variables.NotificationText, (1920 / 2) - width, 75, Color.Red);
            }
        }

        private static void CurrentDomain_DomainUnload(object sender, EventArgs e)
        {
            Variables.font.Dispose();
        }

        private static void Drawing_OnPreReset(EventArgs args)
        {
            Variables.font.OnLostDevice();
        }

        private static void Drawing_OnPostReset(EventArgs args)
        {
            Variables.font.OnResetDevice();
        }

        private static void Game_OnUpdate(EventArgs args)
        {

        }
        #endregion
        private static void Drawing_OnDraw(EventArgs args)
        {
            #region Fundamentals
            Variables.me = ObjectMgr.LocalHero;
            if (!Variables.inGame)
            {
                if (!Game.IsInGame || Variables.me == null)
                    return;
                Variables.inGame = true;
            }
            if (!Game.IsInGame || Variables.me == null)
            {
                Variables.inGame = false;
                return;
            }
            #endregion

            /// <summary>
            /// Get or reset runes after the countdown of the appearance of a new rune.
            /// Draw notification of when to hook friendly to bring them back.
            /// Draw player information from icon bar
            /// Automatically cast spells.
            /// </summary>

            /* First assign and declare your variables */


            //Get players
            var players = ESP.Calculate.SpecificLists.GetPlayersNoSpecsNoIllusionsNoNull(); //Get Players
            List<Player> pla = players;
            if (!players.Any())
                return;

            //Reset runes after waiting time
            if (Variables.Settings.Rune_Tracker_Value.val == 0)
            {
                Variables.TimeTillNextRune = 120 - ((int)Game.GameTime % 120);
                if (Utils.SleepCheck("runeResetAntiSpam"))
                    RuneHandler.ResetRunes();
                if (Utils.SleepCheck("runeCheck"))
                    RuneHandler.GetRunes();
            }


            if (Variables.DeveloperMode)
                if (Variables.HookLocationDrawer)
                {
                    Drawing.DrawText("HOOKED HERE", Variables.AutoHookLocation, Color.Red, FontFlags.AntiAlias | FontFlags.Outline);
                    Drawing.DrawText("ENEMY WAS HERE", Variables.EnemyLocation, Color.Red, FontFlags.AntiAlias | FontFlags.Outline);
                    Drawing.DrawText("PREDICTION", Variables.PredictionLocation, Color.Cyan, FontFlags.AntiAlias | FontFlags.Outline);
                }
            ESP.Draw.Notifier.Backdrop(10, 47, 120, 53, new Color(0, 0, 0, 200));
            //Get runes
            var topRune = RuneHandler.GetRuneType(Variables.TopRune);
            var botRune = RuneHandler.GetRuneType(Variables.BottomRune);
            ESP.Draw.Notifier.Info("Top:", Color.Green, 0);
            ESP.Draw.Notifier.Info(topRune.RuneType, topRune.color, 0, 6 * 4);
            ESP.Draw.Notifier.Info("Bot:", Color.Green, 1);
            ESP.Draw.Notifier.Info(botRune.RuneType, botRune.color, 1, 6 * 4);
            //Draw ESP            
            Variables.EnemyIndex = 0;
            int enemyIndex = 0;
            foreach (var enemy in ESP.Calculate.SpecificLists.EnemyHeroNotIllusion(players))
            {
                if (enemy.Player.Hero.IsAlive && enemy.Player.Hero.IsVisible)
                {
                    Variables.EnemyTracker[enemyIndex].EnemyTracker = enemy;
                    Variables.EnemyTracker[enemyIndex].RelativeGameTime = (int)Game.GameTime;
                    Variables.EnemiesPos[Variables.EnemyIndex] = enemy.Position;
                    Variables.EnemyIndex++;
                }
                else if (Variables.EnemyTracker[enemyIndex].EnemyTracker != null) //Draw last known direction
                    ESP.Draw.Enemy.LastKnownPosition(enemy, enemyIndex);
                enemyIndex++;
            }
        }
    }
    class Variables
    {
        public static Hero me;
        public static GlobalClasses.Tracker[] EnemyTracker = { new GlobalClasses.Tracker(null, 0), new GlobalClasses.Tracker(null, 0), new GlobalClasses.Tracker(null, 0), new GlobalClasses.Tracker(null, 0), new GlobalClasses.Tracker(null, 0), };// new GlobalClasses.Tracker[5];//Hero[] EnemyTracker = new Hero[5];
        //Strings//
        public static string AuthorNotes = "Pudge+ created by NadeHouse\nCurrently running: public access BETA\nTips\n\tPress 'e' when a prediction box appears on the screen and the script\n\twill hook for you\n\t\tNote: It will attempt to hook closest enemy to your\n\t\tmouse, identified with the RED Prediction text\n\tA non moving enemy that can be automattically hooked will be identified\n\twith the orange 'Locked' text\n\tNote: Do not rely on this script to hook Spirit Breaker as he charges";
        public static string LoadMessage = " > Pudge+ is now running";
        public static string UnloadMessage = " > Pudge+ is waiting for the next game to start.";
        public static string PredictMethod = "two"; //one = Pure maths //two = maths & prediction
        public static string visibleParticleEffect = @"particles\ui_mouseactions\hero_highlighter_playerglow.vpcf";//"particles\items2_fx\shivas_guard_impact.vpcf"; @"particles\ui_mouseactions\hero_highlighter_playerglow.vpcf"
        public static string NotificationText = "You are visible";

        public static GlobalClasses.SkillShotClass[] SkillShots = { new GlobalClasses.SkillShotClass("modifier_invoker_sun_strike", "hero_invoker/invoker_sun_strike_team", 175, 1700, "Sun Strike"), new GlobalClasses.SkillShotClass("modifier_lina_light_strike_array", "hero_lina/lina_spell_light_strike_array_ring_collapse", 225, 500, "Lina Stun"), new GlobalClasses.SkillShotClass("modifier_kunkka_torrent_thinker", "hero_kunkka/kunkka_spell_torrent_pool", 225, 1600, "Torrent"), new GlobalClasses.SkillShotClass("modifier_leshrac_split_earth_thinker", "hero_leshrac/leshrac_split_earth_b", 225, 350, "Split Earth") };
        public static List<GlobalClasses.SkillShotClass> DrawTheseSkillshots = new List<GlobalClasses.SkillShotClass>();
        public static readonly Dictionary<Unit, ParticleEffect> SkillShotEffect = new Dictionary<Unit, ParticleEffect>();
        //Bools//
        public static bool DeveloperMode = false;
        public static bool inGame = false;
        public static bool DrawNotification = false;
        public static bool HookForMe = false;
        public static bool CoolDownMethod = true; //True = advanced, false = basic
        public static bool HookLocationDrawer = false;
        //Ints//
        public static int TimeTillNextRune = -999;
        public static int EnemyIndex = 0;
        public static int HookSpeed = 1600;
        public static int HookCounter = 0;
        public static int Offset = 0;
        public static int MouseOffset = 0;
        public static string ResponseIndex = "null";
        public static int AttemptsRemaining = 3;
        //floats//
        public static float ToolTipActivationY;
        public static float ToolTipRadiantStart;
        public static float ToolTipDireStart;
        public static float WindowWidth;
        public static float TeamGap;
        public static float HeroIconWidth;
        //Vectors
        public static Vector2 ESP_Notifier_StartingCoords = new Vector2(15, 50);
        public static Vector2 AutoHookLocation;
        public static Vector2 EnemyLocation;
        public static Vector2 PredictionLocation;
        public static Vector3[] EnemiesPos = new Vector3[5];
        //Runes
        public static CustomRune TopRune = new CustomRune();
        public static CustomRune BottomRune = new CustomRune();
        //misc
        public static Font font;
        public static ParticleEffect visibleGlow;
        public static float GapRatio = 0.171875f;
        public static float RadiantStartRatio = 0.2760416667f;
        public static float DireStartRatio = 0.5546875f;
        public static float ToolTipActivationYRatio = 0.04166666667f;
        //
        public class Settings
        {
            public static string[] OnOff = new string[] { "On", "Off" };
            public static string FilePath = string.Format(@"{0}\Pudge+\Settings.txt", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
            public static string Directory = string.Format(@"{0}\Pudge+\", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
            public static string[] DefaultConfig = { "[Name]Basic ESP[Value]0" , "[Name]Combo Status[Value]0", "[Name]Maximum Damage Output[Value]0", "[Name]Mana Required[Value]0", "[Name]Auto Hook[Value]0", "[Name]Auto Combo[Value]0",
                "[Name]Prediction Box[Value]0", "[Name]Enemy Skills[Value]1", "[Name]Enemy Tracker[Value]0", "[Name]Inventory Tracker[Value]0", "[Name]Rune Tracker[Value]0", "[Name]Eul's Timer[Value]0", "[Name]Teleport Timer[Value]0",
                "[Name]Last Hit Notifier[Value]0", "[Name]Visible By Enemy[Value]0", "[Name]Spirit Breaker Charge[Value]0", "[Name]Skill Shot Notifier[Value]0", "[Name]Hook Lines[Value]0" };
            public static string SaveConfig;
            public static CustomInteger Basic_ESP_Value = new CustomInteger(0);
            public static CustomInteger Combo_Status_Value = new CustomInteger(0);
            public static CustomInteger Maximum_Damage_Output_Value = new CustomInteger(0);
            public static CustomInteger Mana_Required_Value = new CustomInteger(0);
            public static CustomInteger Auto_Hook_Value = new CustomInteger(0);
            public static CustomInteger Auto_Combo_Value = new CustomInteger(0);
            public static CustomInteger Prediction_Box_Value = new CustomInteger(0);
            public static CustomInteger Enemy_Skills_Value = new CustomInteger(0);
            public static CustomInteger Enemy_Tracker_Value = new CustomInteger(0);
            public static CustomInteger Inventory_Tracker_Value = new CustomInteger(0);
            public static CustomInteger Rune_Tracker_Value = new CustomInteger(0);
            public static CustomInteger Euls_Timer_Value = new CustomInteger(0);
            public static CustomInteger Teleport_Timer_Value = new CustomInteger(0);
            public static CustomInteger Last_Hit_Notifier_Value = new CustomInteger(0);
            public static CustomInteger Visisble_By_Enemy_Value = new CustomInteger(0);
            public static CustomInteger Spirit_Breaker_Charge_Value = new CustomInteger(0);
            public static CustomInteger Skill_Shot_Notifier_Value = new CustomInteger(0);
            public static CustomInteger Hook_Lines_value = new CustomInteger(0);
            public static CustomInteger Save_Value = new CustomInteger(0);
            public static int SelectedIndex = 0;
            public static bool ShowMenu = true;
        }
        public class CustomInteger
        {
            public int val;
            public CustomInteger(int foo)
            {
                val = foo;
            }
        }

    }
    class GlobalClasses
    {
        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool GetWindowRect(IntPtr hWnd, out Rectangle lpRect);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
        public class SkillShotClass
        {
            public string ModName { get; set; }
            public string EffectName { get; set; }
            public int Range { get; set; }
            public float Duration { get; set; }
            public string FriendlyName { get; set; }
            public Vector3 Location { get; set; }
            public SkillShotClass(string mod, string effectName, int range, float duration, string name)
            {
                ModName = mod;
                EffectName = effectName;
                Range = range;
                Duration = duration;
                FriendlyName = name;
            }
            public SkillShotClass(string mod, string effectName, int range, float duration, string name, Vector3 pos)
            {
                ModName = mod;
                EffectName = effectName;
                Range = range;
                Duration = duration;
                FriendlyName = name;
                Location = pos;
            }
        }
        public class Tracker
        {
            public Hero EnemyTracker { get; set; }
            public int RelativeGameTime { get; set; }
            public Tracker(Hero target, int time)
            {
                EnemyTracker = target;
                RelativeGameTime = time;
            }
        }
        public static bool ToggleBool(ref bool target)
        {
            target = !target;
            return target;
        }
        public static int GetWidth()
        {
            Rectangle rect = new Rectangle();
            GetWindowRect(GetForegroundWindow(), out rect);
            return rect.Width;
        }
        public static int GetHeight()
        {
            Rectangle rect = new Rectangle();
            GetWindowRect(GetForegroundWindow(), out rect);
            return rect.Height;
        }
        private static string GetCountry()
        {
            string culture = CultureInfo.CurrentCulture.EnglishName;
            string country = culture.Substring(culture.IndexOf('(') + 1, culture.LastIndexOf(')') - culture.IndexOf('(') - 1);   // You could also use a regex, of course
            return country;
        }

        public static string GetAttribute(string Attribute, string Context)
        {
            string ReturnVal;
            string[] Splitter = Context.Split(new string[] { "[" + Attribute + "]" }, StringSplitOptions.None);
            string RemainingContent;
            if (Splitter.Length > 1)
                RemainingContent = Splitter[1];
            else
                RemainingContent = Splitter[0];
            if (RemainingContent.Contains('['))
                ReturnVal = RemainingContent.Split('[')[0];
            else
                ReturnVal = RemainingContent;
            return ReturnVal;
            return null;
        }

        public static string ConvertIntToTimeString(int Time)
        {
            TimeSpan result = TimeSpan.FromSeconds(Time);
            return result.ToString("mm':'ss");
        }
        public static string GetTimeDifference(int Time)
        {
            int difference = (int)Game.GameTime - Time;
            if (difference == 0)
                return "";
            else if (difference < 2)
                return difference.ToString() + " detik";
            else if (difference < 60)
                return difference.ToString() + " detik";
            else
                return ConvertIntToTimeString(difference) + " lalu";
        }
        public static string GetHeroNameFromLongHeroName(string Name)
        {
            return Name.Split(new string[] { "npc_dota_hero_" }, StringSplitOptions.None)[1];
        }
        public static Color GetCostColor(Item item)
        {
            Color itemColor = Color.Green;
            if (item.Cost > 2000)
                itemColor = Color.Cyan;
            if (item.Cost >= 2900)
                itemColor = Color.Yellow;
            if (item.Cost >= 4000)
                itemColor = Color.Magenta;
            if (item.Cost > 5000)
                itemColor = Color.Red;
            if (item.Cost > 5600)
                itemColor = Color.Purple;
            return itemColor;
        }
    }
    class ESP
    {
        public static class Draw
        {
            public static class Interface
            {
                private static int MenuIndex = 0;
                private static int Width = 150;
                private static string Title = "Pudge+ By NadeHouse";
                private static List<Item> MenuItems = new List<Item>();
                private class Item
                {
                    public string Text { get; set; }
                    public int Index { get; set; }
                    public Variables.CustomInteger targetVariable { get; set; }
                    public string[] CustomText { get; set; }
                    public int Max { get; set; }
                    public int Min { get; set; }
                    public string ToolTip { get; set; }
                }
                public static void Add(string text, ref Variables.CustomInteger targetVariable, string ToolTip, int Min = 0, int Max = 1, string[] CustomOverride = null)
                {
                    Item item = new Item();
                    item.Text = text;
                    item.Index = MenuIndex;
                    item.targetVariable = targetVariable;
                    item.CustomText = CustomOverride;
                    item.Min = Min;
                    item.Max = Max;
                    item.ToolTip = ToolTip;
                    MenuItems.Add(item);
                    MenuIndex++;
                }
                public static void Render()
                {
                    Vector2 StartingCoords = Variables.ESP_Notifier_StartingCoords;
                    StartingCoords.Y += 53 + 8;
                    Vector2 TitleCoords = StartingCoords;
                    TitleCoords.X += Width / 2 - ((Title.ToCharArray().Length * 6) / 2);

                    StartingCoords.X -= 5;

                    Vector2 backdropUntil = new Vector2(Width, ((MenuIndex + 1) * 12) + 5 + 12 + 5 + 3);
                    Drawing.DrawRect(StartingCoords, backdropUntil, new Color(0, 0, 0, 255));//Background (backdrop)

                    Vector2 tooltipBanner = new Vector2(StartingCoords.X, backdropUntil.Y + StartingCoords.Y - 12 - 5 - 3); //
                    Drawing.DrawRect(tooltipBanner, new Vector2(Width, 12 + 5), Color.RoyalBlue); //Tooltip background
                    Drawing.DrawText(MenuItems[Variables.Settings.SelectedIndex].ToolTip, new Vector2(tooltipBanner.X + Width / 2 - ((MenuItems[Variables.Settings.SelectedIndex].ToolTip.ToCharArray().Length / 2 * 5)), tooltipBanner.Y), Color.DarkGray, FontFlags.AntiAlias | FontFlags.Outline); //Tooltip text
                    Drawing.DrawRect(StartingCoords, new Vector2(Width, (12 * MenuIndex) + 10 + 12 + 3 + 12), Color.DarkBlue, true); //Borderline
                    Drawing.DrawText(Title, TitleCoords, Color.LightSkyBlue, FontFlags.AntiAlias | FontFlags.Outline); // Title
                    Vector2 underLineStart = new Vector2(StartingCoords.X, StartingCoords.Y + 12 + 1);
                    Vector2 underLineEnd = new Vector2(underLineStart.X + Width, underLineStart.Y);
                    Drawing.DrawLine(underLineStart, underLineEnd, Color.DarkBlue);
                    StartingCoords.X += 5;
                    StartingCoords.Y += 2;
                    foreach (var option in MenuItems)
                    {
                        Color color = Color.White;
                        if (option.Index == Variables.Settings.SelectedIndex)
                            color = Color.Cyan;
                        StartingCoords.Y += (12);
                        Drawing.DrawText(option.Text, StartingCoords, color, FontFlags.AntiAlias | FontFlags.Outline);
                        Vector2 valueCoords = StartingCoords;
                        valueCoords.X = (Width + 10) - 12 - 5;
                        string OptionText = "";
                        Color optionColor = Color.Lime;
                        if (option.CustomText != null)
                            OptionText = option.CustomText[option.targetVariable.val];
                        else
                            OptionText = option.targetVariable.val.ToString();
                        if (OptionText == option.Max.ToString())
                            OptionText = "Off";
                        if (OptionText == "Off")
                            optionColor = Color.Red;

                        Drawing.DrawText(OptionText, valueCoords, optionColor, FontFlags.AntiAlias | FontFlags.Outline);
                    }
                }
                public static class MenuControls
                {
                    public static void Left()
                    {
                        if (MenuItems[Variables.Settings.SelectedIndex].targetVariable.val > MenuItems[Variables.Settings.SelectedIndex].Min)
                        {
                            MenuItems[Variables.Settings.SelectedIndex].targetVariable.val--;
                        }
                        else
                            MenuItems[Variables.Settings.SelectedIndex].targetVariable.val = MenuItems[Variables.Settings.SelectedIndex].Max;
                    }
                    public static void Right()
                    {
                        if (MenuItems[Variables.Settings.SelectedIndex].targetVariable.val < MenuItems[Variables.Settings.SelectedIndex].Max)
                            MenuItems[Variables.Settings.SelectedIndex].targetVariable.val++;
                        else
                            MenuItems[Variables.Settings.SelectedIndex].targetVariable.val = MenuItems[Variables.Settings.SelectedIndex].Min;
                    }
                    public static void Down()
                    {
                        if (Variables.Settings.SelectedIndex >= 0 && Variables.Settings.SelectedIndex < ESP.Draw.Interface.MenuIndex - 1)
                            Variables.Settings.SelectedIndex++;
                        else
                            Variables.Settings.SelectedIndex = 0;
                    }
                    public static void Up()
                    {
                        if (Variables.Settings.SelectedIndex <= ESP.Draw.Interface.MenuIndex - 1 && Variables.Settings.SelectedIndex > 0)
                            Variables.Settings.SelectedIndex--;
                        else
                            Variables.Settings.SelectedIndex = ESP.Draw.Interface.MenuIndex - 1;
                    }
                }
            }
            public static class Notifier
            {
                public static void Backdrop(int StartingX, int StartingY, int ClosingX, int ClosingY, Color color)
                {
                    Drawing.DrawRect(new Vector2(StartingX, StartingY), new Vector2(ClosingX, ClosingY), color);//Background (backdrop)
                }
                public static void Info(string Content, Color color, int Index, int x = 0, FontFlags flags = FontFlags.Outline | FontFlags.AntiAlias)
                {
                    Vector2 coords = Variables.ESP_Notifier_StartingCoords;
                    coords.Y += 12 * Index;
                    coords.X += x;
                    Drawing.DrawText(Content, coords, color, flags);
                    /*public static void Info(Hero target, string text, int Index, Color color, FontFlags flags = FontFlags.Outline | FontFlags.AntiAlias, int x = 0)
               {
                   Vector2 coords = Drawing.WorldToScreen(target.Position);
                   coords.Y -= 80;
                   coords.Y += 12 * Index;
                   coords.X += 75 + x;
                   Drawing.DrawText(text, coords, color, flags);
               }*/
                }
                private static void SelectedHeroTopEnemy(int Index, float Base, int Offset = 0)
                {
                    if (Variables.EnemyTracker[Index - Offset].EnemyTracker != null)
                    {
                        int BaseX = (int)Base + ((int)Variables.HeroIconWidth * (Index - Offset));
                        int BaseY = (int)Variables.ToolTipActivationY + 10;
                        int counter = 1;
                        Color itemColor = Color.Green;
                        var Player = ObjectMgr.GetPlayerById((uint)Index);
                        Drawing.DrawText(GlobalClasses.GetHeroNameFromLongHeroName(Player.Hero.Name), new Vector2(BaseX, BaseY), Color.Red, FontFlags.AntiAlias | FontFlags.Outline);
                        foreach (var p in Variables.EnemyTracker)
                        {
                            if (p != null)
                            {
                                if (p.EnemyTracker.Player.Name == Player.Name)
                                    foreach (var item in p.EnemyTracker.Inventory.Items)
                                    {
                                        itemColor = GlobalClasses.GetCostColor(item);
                                        Drawing.DrawText(item.Name.Remove(0, 5), new Vector2(BaseX, BaseY + (counter * 12)), itemColor, FontFlags.AntiAlias | FontFlags.Outline);
                                        counter++;
                                        itemColor = Color.Green;
                                    }
                            }
                        }
                    }
                }
                private static void SelectedHeroTopFriendly(int Index, float Base, int Offset = 0)
                {
                    string PlayerName = GlobalClasses.GetHeroNameFromLongHeroName(ObjectMgr.GetPlayerById((uint)Index).Hero.Name);
                    int counter = 1;
                    int BaseX = (int)Base + ((int)Variables.HeroIconWidth * (Index));
                    int BaseY = (int)Variables.ToolTipActivationY + 10;
                    Color itemColor = Color.Green;
                    Drawing.DrawText(PlayerName, new Vector2(BaseX, BaseY), Color.Red, FontFlags.AntiAlias | FontFlags.Outline);
                    var p = ObjectMgr.GetPlayerById((uint)Index);
                    foreach (var item in p.Hero.Inventory.Items)
                    {
                        itemColor = GlobalClasses.GetCostColor(item);
                        Drawing.DrawText(item.Name.Remove(0, 5), new Vector2(BaseX, BaseY + (counter * 12)), itemColor, FontFlags.AntiAlias | FontFlags.Outline);
                        counter++;
                        itemColor = Color.Green;
                    }
                }
                public static void SelectedHeroTop(int Index)
                {
                    try
                    {
                        var team = Variables.me.Team;
                        if (team == Team.Radiant) //enable only if player's team is radiant - BUG if on DIRE (needs fix)
                        {
                            if (Index >= 5) //Dire
                            {
                                if (team == Team.Dire) //if my team is the dire team
                                    SelectedHeroTopFriendly(Index - 5, Variables.ToolTipDireStart);
                                else // if my team is the radiant team but selected hero is dire - thus enemy selected in the dire region
                                    SelectedHeroTopEnemy(Index, Variables.ToolTipDireStart, 5);
                            }
                            else if (Index < 5) //Radiant
                            {
                                if (team == Team.Radiant) //if my friendly team is radiant
                                    SelectedHeroTopFriendly(Index, Variables.ToolTipRadiantStart);
                                else //enemy are radiant
                                    SelectedHeroTopEnemy(Index, Variables.ToolTipRadiantStart);
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        //Print.Error("error caught in Top Tool Tip\n" + ex.Message);
                    }
                }

                public static void FriendlyVisible(Hero target)
                {
                    if (target.IsVisibleToEnemies)
                    {
                        if (Utils.SleepCheck("particleEffect" + target.Player.Name))
                        {
                            target.AddParticleEffect(Variables.visibleParticleEffect);
                            Utils.Sleep(500, "particleEffect" + target.Player.Name);
                        }
                    }
                }
                public static void SpiritBreakerCharge(Hero target)
                {
                    foreach (var mod in target.Modifiers)
                    {
                        if (mod.Name == "modifier_spirit_breaker_charge_of_darkness_vision")
                            ESP.Draw.Enemy.Info(target, "Charged by Spirit Breaker", 0, Color.DarkOrange);
                    }
                }

            }
            public static void TeleportCancel(Hero friendly)//(float dist, Modifier mod, Hero friendly)
            {
                var dist = friendly.Distance2D(ObjectMgr.LocalHero); //Distance from team mate to 'me'
                if (dist <= Variables.me.Spellbook.Spell1.CastRange && friendly.Name != Variables.me.Name) //Within hook range
                    foreach (var mod in friendly.Modifiers)
                        if (mod.Name.Contains("teleporting")) //Affected by teleport
                        {
                            var distance = dist;
                            var speed = 1600;
                            var time = distance / speed;
                            var hookAirTime = time * 2;

                            var remainingTime = mod.RemainingTime;
                            if (!(remainingTime - hookAirTime >= 0))
                                Drawing.DrawText("HOOK NOW", Drawing.WorldToScreen(friendly.Position), Color.Red, FontFlags.AntiAlias | FontFlags.Outline);
                            else
                                Drawing.DrawText("WAIT " + Math.Round((remainingTime - hookAirTime), 1, MidpointRounding.AwayFromZero), Drawing.WorldToScreen(friendly.Position), Color.Cyan, FontFlags.AntiAlias | FontFlags.Outline);
                        }
            }
            public static void HookEuls(Hero enemy)
            {
                var dist = enemy.Distance2D(ObjectMgr.LocalHero); //Distance from team mate to 'me'
                if (dist <= Variables.me.Spellbook.Spell1.CastRange && enemy.Name != Variables.me.Name) //Within hook range
                    foreach (var mod in enemy.Modifiers)
                        if (mod.Name == "modifier_eul_cyclone") //Affected by teleport
                        {
                            var distance = dist;
                            var speed = 1600;
                            var time = (distance - 100) / speed;
                            var hookAirTime = time;
                            var remainingTime = mod.RemainingTime;
                            var vec = enemy.Position;
                            vec.Z = 0;
                            var vec2D = Drawing.WorldToScreen(vec);
                            vec2D.Y -= 100;
                            if (!(remainingTime - hookAirTime >= 0))
                                Drawing.DrawText("HOOK NOW", vec2D, Color.Red, FontFlags.AntiAlias | FontFlags.Outline);
                            else
                                Drawing.DrawText("WAIT " + Math.Round((remainingTime - hookAirTime), 1, MidpointRounding.AwayFromZero), vec2D, Color.Cyan, FontFlags.AntiAlias | FontFlags.Outline);
                        }
            }
            public static class Enemy
            {
                public static void basic(Hero enemy)
                {
                    if (Variables.Settings.Basic_ESP_Value.val == 0)
                    {
                        ESP.Draw.Enemy.Info(enemy, enemy.Player.Name, 0, Color.White, FontFlags.Outline | FontFlags.AntiAlias);
                        ESP.Draw.Enemy.Info(enemy, int.Parse(enemy.Health.ToString()).ToString(), 1, Color.Red);
                        ESP.Draw.Enemy.Info(enemy, " ," + Math.Round((Decimal)enemy.Mana, 0, MidpointRounding.AwayFromZero).ToString(), 1, Color.Blue, FontFlags.AntiAlias | FontFlags.Outline, enemy.Health.ToString().ToCharArray().Length * 6);
                    }
                    int disCounter = 0;
                    if (Variables.Settings.Enemy_Skills_Value.val == 2) //Draw basic cool downs
                        foreach (var skill in enemy.Spellbook.Spells)
                        {
                            if (skill.AbilityState == AbilityState.OnCooldown)
                            {
                                Vector2 location = Drawing.WorldToScreen(enemy.Position);
                                location.Y += disCounter * 12;
                                Drawing.DrawText(skill.Name + " (" + (int)skill.Cooldown + ")", location, Color.Green, FontFlags.Outline | FontFlags.AntiAlias);
                                disCounter++;
                            }
                        }

                }
                public static void SkillShotText(string Text, Vector3 Location, float Duration, GlobalClasses.SkillShotClass item)
                {
                    if (Utils.SleepCheck(Text + "one")) // if not asleep
                    {
                        Utils.Sleep((double)Duration + 1000, Text + "one"); //make it sleep
                        Utils.Sleep(Duration, Text); //set delay
                    }
                    else //if in original sleep
                    {
                        Vector2 location2D = Drawing.WorldToScreen(Location);
                        location2D.X -= (6 * Text.ToCharArray().Length) / 2;
                        Drawing.DrawText(Text, location2D, Color.Cyan, FontFlags.AntiAlias | FontFlags.Outline);
                    }
                    if (Utils.SleepCheck(Text)) //delay complete
                        Variables.DrawTheseSkillshots.Remove(item);


                }
                public static void LastKnownPosition(Hero enemy, int enemyIndex)
                {
                    try
                    {
                        if (enemy.IsAlive)
                        {
                            var Angle = enemy.FindAngleR();
                            Vector2 StraightDis = Drawing.WorldToScreen(enemy.Position); //Facing position line
                            StraightDis.X += (float)Math.Cos(Angle) * 500;
                            StraightDis.Y += (float)Math.Sin(Angle) * 500;
                            if (Drawing.WorldToScreen(Variables.EnemyTracker[enemyIndex].EnemyTracker.Position).Y > 15)
                            {
                                Drawing.DrawLine(Drawing.WorldToScreen(Variables.EnemyTracker[enemyIndex].EnemyTracker.Position), StraightDis, Color.Red);
                                Drawing.DrawText(string.Format("{0} {1}", GlobalClasses.GetHeroNameFromLongHeroName(enemy.Name), GlobalClasses.GetTimeDifference(Variables.EnemyTracker[enemyIndex].RelativeGameTime)), Drawing.WorldToScreen(Variables.EnemyTracker[enemyIndex].EnemyTracker.Position), Color.Cyan, FontFlags.AntiAlias | FontFlags.Outline);
                            }
                        }
                    }
                    catch (Exception ex)
                    { }
                }
                public static void SkillShotDisplay()
                {
                    var ents = ObjectMgr.GetEntities<Unit>().Where(x => x.ClassID == ClassID.CDOTA_BaseNPC).ToList();
                    foreach (var ent in ents)
                        for (var n = 0; n <= 4; n++)
                        {
                            try
                            {
                                var mod = ent.Modifiers.FirstOrDefault(x => x.Name == Variables.SkillShots[n].ModName);
                                if (mod == null) continue;
                                ParticleEffect effect;
                                if (!Variables.SkillShotEffect.TryGetValue(ent, out effect))
                                {
                                    effect = ent.AddParticleEffect(@"particles\ui_mouseactions\range_display.vpcf");
                                    effect.SetControlPoint(1, new Vector3(Variables.SkillShots[n].Range, 0, 0));
                                    Variables.SkillShotEffect.Add(ent, effect);
                                    var newSkillshot = Variables.SkillShots[n];
                                    //
                                    newSkillshot.Location = ent.Position;
                                    Variables.DrawTheseSkillshots.Add(newSkillshot);
                                    Drawing.DrawText(Variables.SkillShots[n].ModName, Drawing.WorldToScreen(ent.Position), Color.Cyan, FontFlags.AntiAlias | FontFlags.Outline);
                                    new ParticleEffect(@"particles/units/heroes/" + Variables.SkillShots[n].EffectName + ".vpcf", ent.Position);
                                }
                                break;
                            }
                            catch (Exception ex)
                            {
                                //Print.Info(ex.Message);
                            }

                        }
                    if (Variables.DrawTheseSkillshots.Count > 0) //Draw global skill shots
                        try
                        {
                            foreach (var skillshotToDraw in Variables.DrawTheseSkillshots)
                                ESP.Draw.Enemy.SkillShotText(skillshotToDraw.FriendlyName, skillshotToDraw.Location, skillshotToDraw.Duration, skillshotToDraw);
                        }
                        catch { }
                }
                public static void Skills(Hero enemy)
                {
                    if (enemy != null)
                    {
                        try
                        {
                            int counter = 0;
                            foreach (var spell in enemy.Spellbook.Spells)
                            {
                                int Height = 20;
                                if (Variables.Settings.Enemy_Skills_Value.val == 1)
                                    Height = 0;
                                if (spell == null || spell.Name == "attribute_bonus") continue;
                                int Cooldown = (int)spell.Cooldown;
                                //Print.Info(enemy.Name + " " + enemy.Spellbook.Spells.ToList().Count);
                                Vector2 heroBase = Drawing.WorldToScreen(enemy.Position) + new Vector2(-((20 * (enemy.Spellbook.Spells.ToList().Count - 1) / 2)), 40); //Base drawing point
                                if (Variables.Settings.Enemy_Skills_Value.val == 0)
                                    Drawing.DrawRect(heroBase + new Vector2(counter * 20 - 5, 0), new Vector2(20, 20), Drawing.GetTexture(string.Format("materials/ensage_ui/spellicons/{0}.vmat", spell.Name))); //Skill icons
                                Drawing.DrawRect(heroBase + new Vector2(counter * 20 - 5, Height), new Vector2(20, Cooldown == 0 ? 6 : 22), new ColorBGRA(0, 0, 0, 100), true); //Skill box outlines
                                if (spell.ManaCost > enemy.Mana) //Out of mana - Draw background Blue
                                    Drawing.DrawRect(heroBase + new Vector2(counter * 20 - 5, Height), new Vector2(20, Cooldown == 0 ? 6 : 22), new ColorBGRA(0, 0, 150, 150));
                                if (Cooldown > 0) //Draw cool down
                                {
                                    var text = Cooldown.ToString();
                                    var textSize = Drawing.MeasureText(text, "Arial", new Vector2(10, 200), FontFlags.Outline | FontFlags.AntiAlias); //Measure text
                                    var textPos = (heroBase + new Vector2(counter * 20 - 5, Height) - 1 + new Vector2(10 - textSize.X / 2, -textSize.Y / 2 + 12));
                                    Drawing.DrawText(text, textPos, Color.White, FontFlags.AntiAlias | FontFlags.Outline);
                                }
                                if (spell.Level > 0)
                                    for (int lvl = 1; lvl <= spell.Level; lvl++)

                                        Drawing.DrawRect(heroBase + new Vector2(counter * 20 - 5 + 3 * lvl, Height + 2), new Vector2(2, 2), new ColorBGRA(255, 255, 0, 255), true); //Draw skill level
                                counter++; //Skill index
                            }
                            if (Variables.Settings.Enemy_Skills_Value.val == 0)
                            {
                                Item[] specialItems = { enemy.FindItem("item_blink"), enemy.FindItem("item_force_staff"), enemy.GetDagon() };
                                foreach (var item in specialItems)
                                {
                                    if (item != null)
                                    {
                                        int Cooldown = (int)item.Cooldown;
                                        Vector2 heroBase = Drawing.WorldToScreen(enemy.Position) + new Vector2(-((20 * (enemy.Spellbook.Spells.ToList().Count - 1) / 2)), 40); //Base drawing point
                                        Drawing.DrawRect(heroBase + new Vector2(counter * 20 - 5, 0), new Vector2(28, 20), Drawing.GetTexture(string.Format("materials/ensage_ui/items/{0}.vmat", item.Name.Remove(0, 5)))); //Skill box outlines
                                        Drawing.DrawRect(heroBase + new Vector2(counter * 20 - 5, 20), new Vector2(20, Cooldown == 0 ? 6 : 22), new ColorBGRA(0, 0, 0, 100), true); //Skill box outlines
                                        if (item.ManaCost > enemy.Mana) //Out of mana - Draw background Blue
                                            Drawing.DrawRect(heroBase + new Vector2(counter * 20 - 5, 20), new Vector2(20, Cooldown == 0 ? 6 : 22), new ColorBGRA(0, 0, 150, 150));
                                        if (Cooldown > 0) //Draw cool down
                                        {
                                            var text = Cooldown.ToString();
                                            var textSize = Drawing.MeasureText(text, "Arial", new Vector2(10, 200), FontFlags.Outline | FontFlags.AntiAlias); //Measure text
                                            var textPos = (heroBase + new Vector2(counter * 20 - 5, 20) - 1 + new Vector2(10 - textSize.X / 2, -textSize.Y / 2 + 12));
                                            Drawing.DrawText(text, textPos, Color.White, FontFlags.AntiAlias | FontFlags.Outline);
                                        }
                                        counter++; //Skill index
                                    }
                                }
                            }
                        }
                        catch
                        { }
                    }
                }



                public static void Info(Hero target, string text, int Index, Color color, FontFlags flags = FontFlags.Outline | FontFlags.AntiAlias, int x = 0)
                {
                    Vector2 coords = Drawing.WorldToScreen(target.Position);
                    coords.Y -= 80;
                    coords.Y += 12 * Index;
                    coords.X += 75 + x;
                    Drawing.DrawText(text, coords, color, flags);
                }

            }
        }
        public static class Calculate
        {
            public static class Enemy
            {
                public static bool isMoving(Vector3 pos, int Index)
                {
                    if (pos != Variables.EnemiesPos[Index])
                        return true;
                    else
                        return false;
                }
                public static Hero ClosestToMouse(Hero source, float range = 1000)
                {
                    var mousePosition = Game.MousePosition;
                    var enemyHeroes =
                        ObjectMgr.GetEntities<Hero>()
                            .Where(
                                x =>
                                    x.Team == source.GetEnemyTeam() && !x.IsIllusion && x.IsAlive && x.IsVisible
                                    && x.Distance2D(mousePosition) <= range);
                    Hero[] closestHero = { null };
                    foreach (
                        var enemyHero in
                            enemyHeroes.Where(
                                enemyHero =>
                                    closestHero[0] == null ||
                                    closestHero[0].Distance2D(mousePosition) > enemyHero.Distance2D(mousePosition)))
                    {
                        closestHero[0] = enemyHero;
                    }
                    return closestHero[0];
                }
            }
            public static class Mouse
            {
                private static float RadiantMinX = Variables.ToolTipRadiantStart;
                private static float DireMinX = Variables.ToolTipDireStart;
                private static float HeroIconWidth = Variables.HeroIconWidth;

                public static int SelectedHero(int MouseX)
                {
                    //1065 = min
                    //1395 = max
                    //330 = dif
                    //66 = width
                    for (int i = 0; i < 5; i++)
                        if (MouseX >= RadiantMinX + (HeroIconWidth * i) && MouseX <= RadiantMinX + (HeroIconWidth * (i + 1)))
                            return i;
                    for (int i = 0; i < 5; i++)
                        if (MouseX >= DireMinX + (HeroIconWidth * i) && MouseX <= DireMinX + (HeroIconWidth * (i + 1)))
                            return i + 5;
                    Print.Error("Error finding hero");
                    return -1;
                }
            }
            public static class Creeps
            {
                public static List<Creep> GetCreeps()
                {
                    try
                    {
                        return ObjectMgr.GetEntities<Creep>().Where(creep => (creep.ClassID == ClassID.CDOTA_BaseNPC_Creep_Lane || creep.ClassID == ClassID.CDOTA_BaseNPC_Creep_Siege || creep.ClassID == ClassID.CDOTA_BaseNPC_Creep_Neutral
                    || creep.ClassID == ClassID.CDOTA_BaseNPC_Creep) && (
                        creep.IsAlive && creep.IsVisible && creep.IsSpawned &&
                         creep.Team == Variables.me.GetEnemyTeam() && creep.Distance2D(Variables.me) < 1500)).ToList(); //Get Creeps
                    }
                    catch
                    { return new List<Creep>(); }
                }
            }
            public static class SpecificLists
            {
                public static List<Player> GetPlayersNoSpecsNoIllusionsNoNull()
                {
                    try
                    {
                        return ObjectMgr.GetEntities<Player>().Where(player => player != null && player.Team != Team.Observer && player.Hero != null && !player.Hero.IsIllusion).ToList();
                    }
                    catch { return new List<Player>(); }
                }
                public static List<Hero> EnemyHeroNotIllusion(List<Player> baseList)
                {
                    try
                    {
                        return baseList.Where(player => player.Hero.Team != Variables.me.Team && !player.Hero.IsIllusion).Select(player => player.Hero).ToList();
                    }
                    catch { return new List<Hero>(); }
                }
                public static List<Hero> TeamMates(List<Player> baseList)
                {
                    try
                    {
                        return baseList.Where(player => (player.Hero.Team == Variables.me.Team)).Select(player => player.Hero).ToList();
                    }
                    catch { return new List<Hero>(); }
                }
            }
        }
    }
    public static class RuneHandler
    {
        public static CustomReturnRune GetRuneType(CustomRune rune)
        {
            CustomReturnRune cus = new CustomReturnRune();
            cus.customRune = rune;
            cus.color = Color.Green;
            cus.RuneType = "Ga keliatan";
            try
            {

                if (rune.current)
                    cus.RuneType = rune.rune.RuneType.ToString();
                else
                    return cus;
                switch (cus.RuneType)
                {
                    case "Ga keliatan": cus.color = Color.Green; break;
                    case "DoubleDamage": cus.RuneType = "Double Damage"; cus.color = Color.Cyan; break;
                    case "Invisibility": cus.color = Color.Purple; break;
                    case "Illusion": cus.color = Color.Yellow; break;
                    case "Haste": cus.color = Color.Red; break;
                    case "Bounty": cus.color = Color.Orange; break;
                    case "Regeneration": cus.color = Color.Lime; break;
                    default: cus.color = Color.Green; cus.RuneType = "UNHANDELED RUNE"; break;
                }
                return cus;
            }
            catch
            {
                cus.RuneType = "Ilang";
                return cus;
            }
        }
        public static void GetRunes()
        {
            bool isRunes = false;
            if (Variables.DeveloperMode)
                Print.Info("Checking for runes");
            foreach (Rune r in ObjectMgr.GetEntities<Rune>().Where(rune => rune.IsVisibleForTeam(Variables.me.Team)).ToList())
            {
                isRunes = true;
                if (Variables.DeveloperMode)
                    Print.Info("Rune found");
                switch (r.Position.X.ToString())
                {
                    case "2988": //Bot Rune
                        if (!Variables.BottomRune.current) //if rune should be updated
                        {
                            if (Variables.DeveloperMode)
                                Print.Info(r.RuneType.ToString());
                            Variables.BottomRune.rune = r;
                            Variables.BottomRune.current = true;
                        }
                        break;
                    case "-2271.531": //Top Rune
                        if (!Variables.TopRune.current) //if rune should be updated
                        {
                            if (Variables.DeveloperMode)
                                Print.Info(r.RuneType.ToString());
                            Variables.TopRune.rune = r;
                            Variables.TopRune.current = true;
                        }
                        break;
                }
                if (Variables.TopRune.current && Variables.BottomRune.current)
                {
                    Utils.Sleep(Variables.TimeTillNextRune * 1000, "runeCheck");
                    if (Variables.DeveloperMode)
                        Print.Info(string.Format("runeCheck sleeping for {0} seconds", Variables.TimeTillNextRune));
                }
                else
                    Utils.Sleep(250, "runeCheck");
            }
            if (!isRunes)
                Utils.Sleep(250, "runeCheck");
        }
        public static void ResetRunes()
        {
            if (Variables.TimeTillNextRune == 120) //Every Two Minutes
            {
                if (Variables.DeveloperMode)
                    Print.Encolored("Runes reset", ConsoleColor.Green);
                Variables.TopRune.current = false; //Declare runes as 'out dated'
                Variables.BottomRune.current = false;
                Utils.Sleep(115000, "runeResetAntiSpam");
            }
        }
    }
    public class CustomReturnRune
    {
        public CustomRune customRune { get; set; }
        public Color color { get; set; }
        public string RuneType { get; set; }
    }
    public class CustomRune
    {
        public Rune rune { get; set; }
        public bool current { get; set; }
    }
    public static class Print
    {
        public static void Info(string text, params object[] arguments)
        {
            Encolored(text, ConsoleColor.White, arguments);
        }

        public static void Success(string text, params object[] arguments)
        {
            Encolored(text, ConsoleColor.Green, arguments);
        }

        public static void Error(string text, params object[] arguments)
        {
            Encolored(text, ConsoleColor.Red, arguments);
        }

        public static void Encolored(string text, ConsoleColor color, params object[] arguments)
        {
            var clr = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(text, arguments);
            Console.ForegroundColor = clr;
        }
    }
}
