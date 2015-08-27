using System;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using SPrediction;

namespace NeedASpear_ {
    class NeedASpear {

        private static Menu Menu;
        private static readonly Spell    Javelin          = new Spell(SpellSlot.Q, 1500f);
        private static readonly Obj_AI_Hero Self          = ObjectManager.Player;
        private static readonly string[] HitchanceDisplay = { "Low", "Medium", "High", "Very high", "Dashing", "Immobile" };
        private static readonly HitChance[] Hitchance     = { HitChance.Low, HitChance.Medium, HitChance.High, HitChance.VeryHigh, HitChance.Dashing, HitChance.Immobile };
        //private Obj_AI_Hero target = ObjectManager.Player;

        public NeedASpear() {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
            Game.OnUpdate += Game_OnGameUpdate;
        }

        public void Game_OnGameLoad(EventArgs args) {
            // We injected.
            Console.WriteLine("NeedASpear: injected successfully.");

            // Check if we are Nidalee.
            if (Self.ChampionName != "Nidalee") return;
            Game.PrintChat("<font color='#c0392b'>NeedASpear</font> lodaded");

            // Set up the main menu
            Menu = new Menu("NeedASpear", "nidalee", true);
            Menu.AddItem(new MenuItem("active", "Active").SetValue(new KeyBind("L".ToCharArray()[0], KeyBindType.Toggle)));
            Menu.AddItem(new MenuItem("useharass", "Harass")).SetValue(new KeyBind(67, KeyBindType.Press));
            Menu.AddItem(new MenuItem("checkCollision", "Check collision")).SetValue(true);
            Menu.AddItem(new MenuItem("prediction", "Prediction")).SetValue(new StringList(
                new[] {"SPrediction", "Kurisu", "Kurisu Old", "Drito"}
            ));
            Menu.AddItem(new MenuItem("hitchance", "Hit Chance").SetValue<StringList>(new StringList(HitchanceDisplay, 3)));
            Menu.AddItem(new MenuItem("manapct", "Minimum Mana %")).SetValue(new Slider(55));

            // Set the skillshot.
            Javelin.SetSkillshot(Game.Version.Contains("5.15") ? 0.25f : 0.125f, 40f, 1300f, true, SkillshotType.SkillshotLine);

            // Init SPrediction
            SPrediction.Prediction.Initialize(Menu);
        }

        public void Game_OnGameUpdate(EventArgs args) {
            // Check if enabled
            if (!Menu.Item("active").GetValue<bool>()) return;

            // Target selector, select our target based on collision if set by the user.
            var target = Menu.Item("checkCollision").GetValue<Bool>() ? TargetSelector.GetTargetNoCollision(Javelin) : TargetSelector.GetTarget(Javelin.Range, TargetSelector.DamageType.Magical);
            if (!target.IsValidTarget(Javelin.Range) || !Javelin.IsReady()) return;

            // Find out what kind of prediction we're using
            var mode = Menu.Item("prediction").GetValue<StringList>().SelectedIndex;

            // Calcuate if we are OOM
            var myMana = (int)((Self.Mana / Self.MaxMana) * 100);
            if (myMana < Menu.Item("manapct").GetValue<Slider>().Value) return;

            switch (mode) {
                default:
                case 0: // SPrediction
                    _SPrediction(target);
                    break;
                case 1: // Kurisu
                    Kurisu(target);
                    break;
                case 2: // Kurisu Old
                    Kurisu_Old(target);
                    break;
                case 3: // Drito
                    Drito(target);
                    break;
            }
        }

        public int getIndex() { return Menu.Item("hitchance").GetValue<StringList>().SelectedIndex; }

        public void _SPrediction(Obj_AI_Hero target) {
            if (Self.Distance(target.ServerPosition) <= Javelin.Range) {
                // Get user set HitChance
                HitChance hc = Hitchance[getIndex()];

                // Cast.
                Javelin.SPredictionCast(target, hc);
            }
        }

        public void Kurisu(Obj_AI_Hero target) {
            if(target.Distance(Self.ServerPosition, true) <= Javelin.RangeSqr)
                Javelin.CastIfHitchanceEquals(target, Hitchance[getIndex()]);
        }

        public void Kurisu_Old(Obj_AI_Hero target) {
            var prediction = Javelin.GetPrediction(target);
            if (target.Distance(Self.ServerPosition, true) <= Javelin.RangeSqr) {
                if (prediction.Hitchance >= Hitchance[getIndex()]) {
                    Javelin.Cast(prediction.CastPosition);
                }
            }
        }

        public void Drito(Obj_AI_Hero target) {
            if(Self.Distance(target.ServerPosition) <= Javelin.Range) {
                var JavelinPrediction = Javelin.GetPrediction(target);
                var HitThere = JavelinPrediction.CastPosition.Extend(Self.Position, -140); /// ???
                if (JavelinPrediction.Hitchance >= HitChance.High) {
                    Javelin.Cast(HitThere);
                }
            }
        }
    }
}
