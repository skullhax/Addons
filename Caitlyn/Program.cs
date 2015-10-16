//used CookieMonster10, flux and xRp scripts as references *-*
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
        public static AIHeroClient Caity = ObjectManager.Player;

        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }

        
        public static Spell.Skillshot Q, E;
        public static Spell.Targeted W, R;
        public static Menu CaityMenu, SettingsMenu;

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            //Only if u select Caitlyn
            if (Player.Instance.ChampionName != "Caitlyn") return;

            Bootstrap.Init(null);
            TargetSelector.Init();
            Chat.Print("<font color = \"#2fff0a\">xaxiCait by xaxi</font>");

            //      SpellSlot, Range, Skillshot Type, Cast, Delay, Width.
            Q = new Spell.Skillshot(SpellSlot.Q, 1240, SkillShotType.Linear, (int)0.25f, (int)2000f, (int)60f); 
            W = new Spell.Targeted(SpellSlot.W, 820);
            E = new Spell.Skillshot(SpellSlot.E, 800, SkillShotType.Linear, (int)0.25f, (int)1600f, (int)80f); 
            R = new Spell.Targeted(SpellSlot.R, 2000);

            //Menu
            CaityMenu = MainMenu.AddMenu("xaxiCaitlyn", "xaxi_cait");
            CaityMenu.AddGroupLabel("Caitlyn Settings");
            CaityMenu.AddSeparator();
            CaityMenu.AddLabel("Rewritten by Xaxixeo credis: CookieMonster10");

            SettingsMenu = CaityMenu.AddSubMenu("Settings", "settings");
            SettingsMenu.AddGroupLabel("Settings");
            SettingsMenu.AddLabel("Combo");
            SettingsMenu.Add("comboQ", new CheckBox("Use Q"));
            SettingsMenu.Add("comboW", new CheckBox("Use W"));
            SettingsMenu.Add("comboE", new CheckBox("Use E KS"));
            SettingsMenu.Add("comboE2", new CheckBox("Use E Escape"));
            SettingsMenu.Add("comboR", new CheckBox("Use R KS in Combo"));
            SettingsMenu.AddSeparator();
            SettingsMenu.AddLabel("Harass");
            SettingsMenu.Add("harassQ", new CheckBox("Use Q"));
            SettingsMenu.Add("harassMana", new Slider("Mana% Q", 70, 1, 99));
            SettingsMenu.AddSeparator();
            SettingsMenu.AddLabel("LaneClear");
            SettingsMenu.Add("laneclearQ", new CheckBox("Use Q"));
            SettingsMenu.Add("laneclearMana", new Slider("Mana% Q", 90, 1, 99));
            SettingsMenu.AddSeparator();
            SettingsMenu.AddLabel("JungleClear");
            SettingsMenu.Add("jungleclearQ", new CheckBox("Use Q"));
            SettingsMenu.Add("jungleclearMana", new Slider("Mana% Q", 10, 1, 99));
            SettingsMenu.AddSeparator();
            SettingsMenu.AddLabel("Drawings");
            SettingsMenu.Add("drawQ", new CheckBox("Q Range"));
            SettingsMenu.Add("drawW", new CheckBox("W Range"));
            SettingsMenu.Add("drawE", new CheckBox("E Range"));
            SettingsMenu.Add("drawR", new CheckBox("R Range"));
            SettingsMenu.AddSeparator();
            SettingsMenu.AddLabel("Misc");
            SettingsMenu.Add("Dash", new KeyBind("Dash to mouse pos", false, KeyBind.BindTypes.HoldActive, 'Z'));
            SettingsMenu.Add("Smart Q", new CheckBox("Smart Q"));
            SettingsMenu.Add("Smart W", new CheckBox("Smart W"));
            SettingsMenu.Add("Smart E", new CheckBox("Smart E"));
            SettingsMenu.Add("Smart R", new CheckBox("KS R No Combo"));

            Game.OnTick += Game_onTick;
            Drawing.OnDraw += Drawing_OnDraw;
            Interrupter.OnInterruptableSpell += Interrupter_OnInterruptableSpell;
            Gapcloser.OnGapcloser += Gapcloser_OnGapCloser;
        }
        // Interrupter
        private static void Interrupter_OnInterruptableSpell(Obj_AI_Base sender,
            Interrupter.InterruptableSpellEventArgs args)
        {
            var intTarget = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
            {
                if (Q.IsReady() && sender.IsValidTarget(Q.Range) && SettingsMenu["Smart Q"].Cast<CheckBox>().CurrentValue)
                    Q.Cast(intTarget.ServerPosition);
            }
        }

        // GapCloser
        private static void Gapcloser_OnGapCloser
            (AIHeroClient sender, Gapcloser.GapcloserEventArgs gapcloser)
        {
            if (!SettingsMenu["Smart E"].Cast<CheckBox>().CurrentValue) return;
            if (ObjectManager.Player.Distance(gapcloser.Sender, true) <
                E.Range * E.Range && sender.IsValidTarget())
            {
                E.Cast(gapcloser.Sender);
            }
        }
        private static void Game_onTick(EventArgs args)
        {
            if (Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.Harass) do_Haress();
            if (Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.Combo) do_Combo();
            if (SettingsMenu["Dash"].Cast<KeyBind>().CurrentValue) Dash_To_Mouse();
            if (SettingsMenu["Smart R"].Cast<CheckBox>().CurrentValue) Ult_Anyone();
            if (SettingsMenu["Smart W"].Cast<CheckBox>().CurrentValue) SmartW();
            
        }
        public static Obj_AI_Base GetEnemy(float range, GameObjectType t)
        {
            switch (t)
            {
                case GameObjectType.AIHeroClient:
                    return EntityManager.Heroes.Enemies.OrderBy(a => a.Health).FirstOrDefault(
                        a => a.Distance(Player.Instance) < range && !a.IsDead && !a.IsInvulnerable);
                default:
                    return EntityManager.MinionsAndMonsters.EnemyMinions.OrderBy(a => a.Health).FirstOrDefault(
                        a => a.Distance(Player.Instance) < range && !a.IsDead && !a.IsInvulnerable);
            }
        }
        public static Obj_AI_Base GetJgEnemy(float range, GameObjectType t)
        {
            switch (t)
            {
                case GameObjectType.AIHeroClient:
                    return EntityManager.Heroes.Enemies.OrderBy(a => a.Health).FirstOrDefault(
                        a => a.Distance(Player.Instance) < range && !a.IsDead && !a.IsInvulnerable);
                default:
                    return EntityManager.MinionsAndMonsters.GetJungleMonsters().FirstOrDefault(
                        a => a.Distance(Player.Instance) < range && !a.IsDead && !a.IsInvulnerable);
            }
        }
        public static float QDmg(Obj_AI_Base target)
        {
            {
                return Caity.GetSpellDamage(target, SpellSlot.Q);
            }
        }

        public static float WDmg(Obj_AI_Base target)
        {
            {
                return Caity.GetSpellDamage(target, SpellSlot.W);
            }
        }

        public static float EDmg(Obj_AI_Base target)
        {
            {
                return Caity.GetSpellDamage(target, SpellSlot.E);
            }
        }

        public static float RDmg(Obj_AI_Base target)
        {
            {
                return Caity.GetSpellDamage(target, SpellSlot.R);
            }
        }

        private static void do_Haress()
        {
            var useQ = SettingsMenu["harassQ"].Cast<CheckBox>().CurrentValue;
            var mana = SettingsMenu["harassQMana"].Cast<Slider>().CurrentValue;

            if (Player.Instance.ManaPercent > mana)
            {
                foreach (var enemy in EntityManager.Heroes.Enemies.Where(target => target.IsValidTarget(Q.Range) && !target.IsZombie))
                {
                    var predictionQ = Q.GetPrediction(enemy);
                    if (useQ && Q.IsReady() && Caity.GetAutoAttackRange() < Caity.Distance(enemy) && predictionQ.HitChancePercent >= 70)
                    {
                        Q.Cast(predictionQ.CastPosition);
                    }
                }
            }
        }
        public static void do_LaneClear()
        {
            var useLc = SettingsMenu["laneclearQ"].Cast<CheckBox>().CurrentValue;
            var manaL = SettingsMenu["laneclearMana"].Cast<Slider>().CurrentValue;

            if (Player.Instance.ManaPercent > manaL)
            {
                var useQlc =
                    (Obj_AI_Minion)GetEnemy(Q.Range, GameObjectType.obj_AI_Minion);

                if (useLc && Q.IsReady())
                {
                    Q.Cast(useQlc.ServerPosition);
                }
            }
        }
        public static void do_JungleClear()
        {
            var useJc = SettingsMenu["jungleclearQ"].Cast<CheckBox>().CurrentValue;
            var manaJ = SettingsMenu["jungleclearMana"].Cast<Slider>().CurrentValue;

            if (Player.Instance.ManaPercent > manaJ)
            {
                var useQjc =
                    (Obj_AI_Minion)GetJgEnemy(Q.Range, GameObjectType.obj_AI_Minion);

                if (useJc && Q.IsReady())
                {
                    Q.Cast(useQjc.ServerPosition);
                }
            }
        }
        private static void do_Combo()
        {

            var useQ = SettingsMenu["comboQ"].Cast<CheckBox>().CurrentValue;
            var useW = SettingsMenu["comboW"].Cast<CheckBox>().CurrentValue;
            var useE = SettingsMenu["comboE"].Cast<CheckBox>().CurrentValue;
            var useE2 = SettingsMenu["comboE2"].Cast<CheckBox>().CurrentValue;
            var useR = SettingsMenu["comboR"].Cast<CheckBox>().CurrentValue;

            foreach(var enemy in EntityManager.Heroes.Enemies.Where(target => target.IsValidTarget(Q.Range) && !target.IsZombie))
            {
                var predictionQ = Q.GetPrediction(enemy);
                if (useQ && Q.IsReady() && Caity.GetAutoAttackRange() < Caity.Distance(enemy) && predictionQ.HitChancePercent >= 70)
                {
                    Q.Cast(predictionQ.CastPosition);
                }
                if (useW && W.IsReady() && W.Cast(enemy)&&
                    (enemy.HasBuffOfType(BuffType.Stun )
                    || enemy.HasBuffOfType(BuffType.Snare) 
                    || enemy.HasBuffOfType(BuffType.Taunt)
                    || enemy.HasBuffOfType(BuffType.Knockup) 
                    || enemy.HasBuff("Recall")
                    || enemy.Health <= WDmg(enemy)))
                {
                    W.Cast(enemy);
                }

                if (useE2 && E.IsReady() && E.GetPrediction(enemy).HitChancePercent >= 70 && Caity.Distance(enemy) <= 200)
                {
                    E.Cast(enemy);
                }

                if (useE && E.IsReady() && E.GetPrediction(enemy).HitChancePercent >= 70 && enemy.Health <= EDmg(enemy))
                {
                    E.Cast(enemy);
                }
                
                if (useR && R.IsReady() && enemy.Health <= RDmg(enemy))
                {
                    R.Cast(enemy);
                    
                }
               

            }
            
        }

        private static void Dash_To_Mouse()
        {   //L# Marksman by Legacy3
            if(E.IsReady()) E.Cast( ObjectManager.Player.ServerPosition.To2D().Extend(Game.CursorPos.To2D(), -300).To3D() );
            
        }

        private static void Ult_Anyone()
        {
            if (R.IsReady())
            {
                foreach (var enemy in EntityManager.Heroes.Enemies.Where(target => target.IsValidTarget(R.Range) 
                                                                      && !target.IsZombie
                                                                      && target.Health <= RDmg(target)))
                {
                    R.Cast(enemy);
                }
            }
        }

        private static void SmartW()
        {
            foreach (var enemy in EntityManager.Heroes.Enemies.Where(target => target.IsValidTarget(W.Range) &&
                                                                               (target.HasBuffOfType(BuffType.Stun)
                                                                                || target.HasBuffOfType(BuffType.Snare)
                                                                                || target.HasBuffOfType(BuffType.Taunt)
                                                                                || target.HasBuffOfType(BuffType.Knockup)
                                                                                || target.HasBuff("Recall")
                                                                                || target.Health <= WDmg(target))))
            {
                W.Cast(enemy);
            }
        }
        
        private static void Drawing_OnDraw(EventArgs args)
        {
            if (SettingsMenu["drawQ"].Cast<CheckBox>().CurrentValue)
            {
                new Circle { Color = Color.Black, BorderWidth = 1, Radius = Q.Range }.Draw(Caity.Position);
            }

            if (SettingsMenu["drawW"].Cast<CheckBox>().CurrentValue)
            {
                new Circle { Color = Color.Black, BorderWidth = 1, Radius = W.Range }.Draw(Caity.Position);
            }

            if (SettingsMenu["drawE"].Cast<CheckBox>().CurrentValue)
            {
                new Circle { Color = Color.Black, BorderWidth = 1, Radius = E.Range }.Draw(Caity.Position);
            }

            if (SettingsMenu["drawR"].Cast<CheckBox>().CurrentValue)
            {
                new Circle { Color = Color.Black, BorderWidth = 1, Radius = R.Range }.Draw(Caity.Position);
            }
        }

    }
}
