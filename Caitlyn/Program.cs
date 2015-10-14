//used MarioGK's HU3:Ezreal script as reference c:
using System;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Rendering;

using Color = System.Drawing.Color;

namespace Caitlyn
{
    class Program
    {

        //public static Obj_AI_Base t;
        public static AIHeroClient Caity
        {
            get { return ObjectManager.Player; }
        }

        static void Main(string[] args1)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }

        
        public static Spell.Skillshot Q;
        public static Spell.Targeted W;
        public static Spell.Skillshot E;
        public static Spell.Targeted R;
        public static Menu CaityMenu, SettingsMenu;

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            if (Player.Instance.ChampionName != "Caitlyn") return;

            Bootstrap.Init(null);
            TargetSelector.Init();


            // SpellSlot , Range , Skillshot type , Cast  delay , width 
            Q = new Spell.Skillshot(SpellSlot.Q, 1240, SkillShotType.Linear, (int)0.25f, (int)2000f, (int)60f); 
            W = new Spell.Targeted(SpellSlot.W, 820);
            E = new Spell.Skillshot(SpellSlot.E, 800, SkillShotType.Linear, (int)0.25f, (int)1600f, (int)80f); 
            R = new Spell.Targeted(SpellSlot.R, 2000);

            CaityMenu = MainMenu.AddMenu("CM:Caitlyn", "cm_cait");
            CaityMenu.AddGroupLabel("Caitlyn 1.0");
            CaityMenu.AddSeparator();
            CaityMenu.AddLabel("Written by CookieMonster10");

            SettingsMenu = CaityMenu.AddSubMenu("Settings", "settings");
            SettingsMenu.AddGroupLabel("Settings");
            SettingsMenu.AddLabel("Combo");
            SettingsMenu.Add("comboQ", new CheckBox("Use Q on Combo"));
            SettingsMenu.Add("comboW", new CheckBox("Use W on Combo"));
            SettingsMenu.Add("comboE", new CheckBox("Use E on Combo"));
            SettingsMenu.Add("comboR", new CheckBox("Use R on Combo"));
            SettingsMenu.AddLabel("Harass");
            SettingsMenu.Add("harassQ", new CheckBox("Use Q on Harass"));
            SettingsMenu.Add("laneclearMana", new Slider("Mana % To Use Q", 30, 0, 100));
            SettingsMenu.AddLabel("Drawings");
            SettingsMenu.Add("drawQ", new CheckBox("Draw Q Range"));
            SettingsMenu.Add("drawW", new CheckBox("Draw W Range"));
            SettingsMenu.Add("drawE", new CheckBox("Draw E Range"));
            SettingsMenu.Add("drawR", new CheckBox("Draw R Range"));
            SettingsMenu.AddLabel("Misc");
            //SettingsMenu.Add("antigapcloser", new CheckBox("Use E to get away from enemy"));
            SettingsMenu.Add("Dash", new KeyBind("Dash to mouse pos", false, KeyBind.BindTypes.HoldActive, 'Z'));
            //SettingsMenu.Add("EQ Combo", new KeyBind("Perform E-Q combo" , false , KeyBind.BindTypes.HoldActive, 'T'));
            SettingsMenu.Add("Auto Ult", new CheckBox("Stay alart for low hp enemies to auto ult them"));
            SettingsMenu.Add("Smart W", new CheckBox("Smart W"));
            SettingsMenu.Add("Smart Q", new CheckBox("Smart Q"));

            Game.OnTick += Game_onTick;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        private static void Game_onTick(EventArgs args)
        {
            if (Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.Harass) do_Haress();
            if (Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.Combo) do_Combo();
            if (SettingsMenu["Dash"].Cast<KeyBind>().CurrentValue) Dash_To_Mouse();
            if (SettingsMenu["Auto Ult"].Cast<CheckBox>().CurrentValue) Ult_Anyone();
            if (SettingsMenu["Smart W"].Cast<CheckBox>().CurrentValue) SmartW();
            if (SettingsMenu["Smart Q"].Cast<CheckBox>().CurrentValue) SmartQ();
            //if (!SettingsMenu["EQ Combo"].Cast<KeyBind>().CurrentValue) EQC();   WIP

        }

        public static float QDmg(Obj_AI_Base target)
        {

            // { PHYSICAL DAMAGE: 20 / 60 / 100 / 140 / 180 } + (+ 130% AD) 
            return Caity.CalculateDamageOnUnit(target, DamageType.Physical,
                (float)(new[] { 20, 60, 100, 140, 180 }[Program.Q.Level] + 1.3 * Caity.FlatPhysicalDamageMod));
        }

        public static float WDmg(Obj_AI_Base target)
        {
            return Caity.CalculateDamageOnUnit(target, DamageType.Magical,
                (float)(new[] { 80, 130, 180 , 230, 280 }[Program.W.Level] + .60 * Caity.FlatMagicDamageMod));
        }

        public static float EDmg(Obj_AI_Base target)
        {
            return Caity.CalculateDamageOnUnit(target, DamageType.Magical,
                (float)(new[] { 80, 130, 180, 230, 280 }[Program.E.Level] + .80 * Caity.FlatMagicDamageMod));
        }

        public static float RDmg(Obj_AI_Base target)
        {
            return Caity.CalculateDamageOnUnit(target, DamageType.Physical,
                (float)(new[] { 250, 475, 700}[Program.R.Level] + 2.0 * Caity.FlatPhysicalDamageMod));
        }

        private static void do_Haress()
        {
            var useQ = SettingsMenu["harassQ"].Cast<CheckBox>().CurrentValue;
            var mana = SettingsMenu["laneclearMana"].Cast<Slider>().CurrentValue;

            if (Player.Instance.ManaPercent > mana)
            {
                foreach (var enemy in HeroManager.Enemies.Where(target => target.IsValidTarget(Q.Range) && !target.IsDead && !target.IsZombie))
                {
                    if (useQ && Q.IsReady() && Q.GetPrediction(enemy).HitChance >= HitChance.Medium)
                    {
                        Q.Cast(enemy);
                    }
                }
            }
        }

        private static void do_Combo()
        {

            var useQ = SettingsMenu["comboQ"].Cast<CheckBox>().CurrentValue;
            var useW = SettingsMenu["comboW"].Cast<CheckBox>().CurrentValue;
            var useE = SettingsMenu["comboE"].Cast<CheckBox>().CurrentValue;
            var useR = SettingsMenu["comboR"].Cast<CheckBox>().CurrentValue;

            foreach(var enemy in HeroManager.Enemies.Where(target => target.IsValidTarget(Q.Range) && !target.IsDead && !target.IsZombie))
            {
                if(useQ && Q.IsReady() && Q.GetPrediction(enemy).HitChance >= HitChance.Medium)
                {
                    Q.Cast(enemy);
                }
                if(useE && E.IsReady() && E.GetPrediction(enemy).HitChance >= HitChance.Medium && enemy.Health <= EDmg(enemy))
                {
                    E.Cast(enemy);
                }
                if (useR && R.IsReady() && enemy.Health <= RDmg(enemy))
                {
                    R.Cast(enemy);
                    
                }
               

            }

            return;
        }

        private static void Dash_To_Mouse()
        {   //L# Marksman by Legacy3
            if(E.IsReady()) E.Cast( ObjectManager.Player.ServerPosition.To2D().Extend(Game.CursorPos.To2D(), -300).To3D() );
            
        }

        private static void Ult_Anyone()
        {
            if (R.IsReady())
            {
                foreach (var enemy in HeroManager.Enemies.Where(target => target.IsValidTarget(R.Range) 
                                                                      && !target.IsDead 
                                                                      && !target.IsZombie 
                                                                      && target.Health <= RDmg(target)  ))
                {
                    R.Cast(enemy);
                }
            }
        }

        private static void SmartW()
        {
            foreach (var enemy in HeroManager.Enemies.Where(target => target.IsValidTarget(W.Range) && !target.IsDead && 
                    (target.HasBuffOfType(BuffType.Stun )
                    || target.HasBuffOfType(BuffType.Snare) 
                    || target.HasBuffOfType(BuffType.Taunt)
                    || target.HasBuffOfType(BuffType.Knockup) 
                    || target.HasBuff("Recall")  )))
                {
                W.Cast(enemy);
                }
        }

        private static void SmartQ()
        {
            foreach (var enemy in HeroManager.Enemies.Where(target => target.IsValidTarget(Q.Range) && !target.IsDead &&
                    (target.HasBuffOfType(BuffType.Stun) 
                    || target.HasBuffOfType(BuffType.Snare) 
                    || target.HasBuffOfType(BuffType.Taunt) 
                    || target.HasBuffOfType(BuffType.Slow)
                    || target.HasBuffOfType(BuffType.Suppression)
                    || target.HasBuffOfType(BuffType.Charm)
                    || target.HasBuffOfType(BuffType.Knockup)
                    || target.HasBuff("Recall")    )))
            {
                Q.Cast(enemy);
            }
        }


        private static void Drawing_OnDraw(EventArgs args)
        {
            if (SettingsMenu["drawQ"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Blue, BorderWidth = 1, Radius = Q.Range }.Draw(Caity.Position);
            }

            if (SettingsMenu["drawW"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.SkyBlue, BorderWidth = 1, Radius = W.Range }.Draw(Caity.Position);
            }

            if (SettingsMenu["drawE"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.YellowGreen, BorderWidth = 1, Radius = E.Range }.Draw(Caity.Position);
            }

            if (SettingsMenu["drawR"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.White, BorderWidth = 1, Radius = R.Range }.Draw(Caity.Position);
            }
        }

    }
}
