using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using SharpDX.Direct3D9;
using Color = SharpDX.Color;

namespace Khazix
{
    class Program
    {
        private static string Name = "Khazix";
        private static Obj_AI_Hero Player = ObjectManager.Player;
        private static Orbwalking.Orbwalker Orbwalker;
        private static Spell Q, W, E, R;
        private static SpellDataInst Qi, Wi, Ei, Ri;
        private static Menu config;
        private static bool Wnorm = true, Wevolved, Eevolved;
        private static bool Jumping = false;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Player = ObjectManager.Player;

            if (Player.BaseSkinName != Name)
                return;

            Qi = Player.GetSpell(SpellSlot.Q);
            Wi = Player.GetSpell(SpellSlot.W);
            Ei = Player.GetSpell(SpellSlot.E);
            Ri = Player.GetSpell(SpellSlot.R);

            Q = new Spell(Qi.Slot, Qi.SData.CastRange);
            W = new Spell(Wi.Slot, Wi.SData.CastRange);
            E = new Spell(Ei.Slot, Ei.SData.CastRange);
            R = new Spell(Ri.Slot, Ri.SData.CastRange);

            InitSpell(Q, Qi);
            InitSpell(W, Wi);
            InitSpell(E, Ei);
            InitSpell(R, Ri);

            W.SetSkillshot(0.225f, 80f, 828.5f, true, SkillshotType.SkillshotLine);
            E.SetSkillshot(0.25f, 100f, 1000f, false, SkillshotType.SkillshotCircle);

            // Root Menu
            config = new Menu("Khazix", "Khazix", true);

            // Target Selector
            var tsMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(tsMenu);
            config.AddSubMenu(tsMenu);

            // Orbwalker
            config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(config.SubMenu("Orbwalking"));

            config.AddSubMenu(new Menu("Combo", "Combo"));
            config.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "Use Q")).SetValue(true);
            config.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "Use W")).SetValue(true);

            config.AddSubMenu(new Menu("Harass", "Harass"));
            config.SubMenu("Harass").AddItem(new MenuItem("UseQHarass", "Use Q")).SetValue(true);
            config.SubMenu("Harass").AddItem(new MenuItem("UseWHarass", "Use W")).SetValue(true);

            // Keys
            var keys = config.AddSubMenu(new Menu("Keys", "Keys"));
            {
                keys.AddItem(new MenuItem("Jump", "Jump To Mouse").SetValue(new KeyBind('T', KeyBindType.Press)));
            }

            var settings = config.AddSubMenu(new Menu("Misc", "Misc"));
            {
                settings.AddItem(new MenuItem("DelayJump", "Delay of Jump").SetValue(new Slider(600, 580, 620)));
            }

            config.AddToMainMenu();

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;

            Game.PrintChat("Khazix Loaded.");
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            Render.Circle.DrawCircle(Player.Position, E.Range, System.Drawing.Color.Aqua);
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (Player.IsDead) return;

            if (config.Item("Jump").GetValue<KeyBind>().Active)
            {
                JumpIt();
                //JumpIt(ObjectManager.Get<Obj_AI_Hero>().FirstOrDefault());
            }

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo || Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
                Harass();
        }

        private static void Harass()
        {
            Obj_AI_Hero target = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Physical);
            if (target != null)
            {
                if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) <= Q.Range && config.Item("UseQHarass").GetValue<bool>() && Q.IsReady() && !Jumping)
                {
                    Orbwalker.SetAttack(false);
                    Q.Cast(target);
                    Orbwalker.SetAttack(true);
                }

                if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) <= W.Range && config.Item("UseWHarass").GetValue<bool>() && W.IsReady() &&
                    Wnorm)
                {
                    W.Cast(target);
                }
                if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) <= W.Range && config.Item("UseWHarass").GetValue<bool>() && W.IsReady() &&
                    Wevolved)
                {
                    W.Cast(target);
                }
            }
        }

        private static void JumpIt(Obj_AI_Base unit)
        {
            if (!E.IsReady()) return;

            var myPos = Player.ServerPosition;
            var enemyPos = unit.ServerPosition;
            var castPos = myPos - (myPos - enemyPos).Normalized() * E.Range;
            E.Cast(castPos);
            Utility.DelayAction.Add(600,
                () => E.Cast(Game.CursorPos));
            return;
        }

        private static void JumpIt()
        {
            if (!E.IsReady()) return;

            var myPos = Player.ServerPosition;
            var castPos = myPos - (myPos - Game.CursorPos).Normalized() * E.Range;
            E.Cast(castPos);
            Utility.DelayAction.Add(config.Item("DelayJump").GetValue<Slider>().Value,
                () => E.Cast(Game.CursorPos));
        }

        private static void InitSpell(Spell s, SpellDataInst si)
        {
            s = new Spell(si.Slot, si.SData.CastRange);
            return;
        }
    }
}
