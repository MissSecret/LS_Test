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

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Player = ObjectManager.Player;

            if (Player.BaseSkinName != Name)
                return;

            Qi = Player.Spellbook.GetSpell(SpellSlot.Q);
            Wi = Player.Spellbook.GetSpell(SpellSlot.W);
            Ei = Player.Spellbook.GetSpell(SpellSlot.E);
            Ri = Player.Spellbook.GetSpell(SpellSlot.R);

            Q = NewSpell(Qi);
            W = NewSpell(Wi);
            E = NewSpell(Ei);
            R = NewSpell(Ri);

            W.SetSkillshot(0.225f, 80f, 828.5f, true, SkillshotType.SkillshotLine);
            E.SetSkillshot(0.25f, 100f, 1000f, false, SkillshotType.SkillshotCircle);

            // Root Menu
            config = new Menu("Khazix", "Khazix", true);

            // Target Selector
            var tsMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(tsMenu);
            config.AddSubMenu(tsMenu);

            // Orbwalker
            if (config.Item("UseOrbwalker") == null || config.Item("UseOrbwalker").GetValue<bool>())
            {
                config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
                Orbwalker = new Orbwalking.Orbwalker(config.SubMenu("Orbwalking"));
            }

            // Keys
            var keys = config.AddSubMenu(new Menu("Keys", "Keys"));
            {
                keys.AddItem(new MenuItem("JumpCursor", "Jump To Cursor").SetValue(new KeyBind('T', KeyBindType.Press)));
                keys.AddItem(new MenuItem("JumpHome", "Jump To Home").SetValue(new KeyBind('G', KeyBindType.Press)));
            }

            config.AddItem(new MenuItem("UseOrbwalker", "Use Orbwalker (Need Reload)").SetValue(false));

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

            if (config.Item("JumpCursor").GetValue<KeyBind>().Active && E.IsReady())
            {
                JumpExploit(JumpType.ToCursor);
            }

            if (config.Item("JumpHome").GetValue<KeyBind>().Active && E.IsReady())
            {
                JumpExploit(JumpType.ToHome);
            }
        }

        private static void JumpExploit(JumpType type)
        {
            Vector3 myPos = Player.ServerPosition;
            Vector3 castPos;

            if (type == JumpType.ToCursor)
                castPos = myPos - (myPos - Game.CursorPos).Normalized() * E.Range;
            else
                castPos = myPos - (myPos - GetHomePos(Player.Team)).Normalized() * E.Range;

            E.Cast(castPos);
            Utility.DelayAction.Add(600,
                () => E.Cast(Game.CursorPos));
        }

        private static Vector3 GetHomePos(GameObjectTeam team)
        {
            if (team == GameObjectTeam.Order) // Blue Team
            {
                return new Vector3(396f, 462f, 182.1325f);
            }
            else if (team == GameObjectTeam.Chaos) // Red Team
            {
                return new Vector3(14340f, 14390f, 171.9777f);
            }
            else
            {
                Game.PrintChat("Unknown Team : " + team.ToString());
                return new Vector3();
            }
        }

        enum JumpType
        {
            ToCursor = 0,
            ToHome = 1
        }

        private static Spell NewSpell(SpellDataInst spell, bool IsChargedSkill = false)
        {
            var s = new Spell(spell.Slot, -1f);

            if (spell.SData.CastRangeDisplayOverride <= 0)
                if (spell.SData.CastRange <= 0)
                    s.Range = spell.SData.CastRadius;
                else
                    s.Range = IsChargedSkill ? spell.SData.CastRange : spell.SData.CastRadius;
            else
                s.Range = spell.SData.CastRangeDisplayOverride;

            return s;
        }
    }
}
