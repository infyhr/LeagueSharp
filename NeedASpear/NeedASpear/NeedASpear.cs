using System;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using SPrediction;

namespace NeedASpear_ {
    internal class NeedASpear {

        private static Menu Menu;
        private static Spell    Javelin          = new Spell(SpellSlot.Q, 1500f);
        private static Obj_AI_Hero Self          = ObjectManager.Player;
        private static string[] HitchanceDisplay = { "Low", "Medium", "High", "Very high", "Dashing", "Immobile" };
        private static HitChance[] Hitchance     = { HitChance.Low, HitChance.Medium, HitChance.High, HitChance.VeryHigh, HitChance.Dashing, HitChance.Immobile };

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
            Menu.AddItem(new MenuItem("activated", "Active")).SetValue(true);
            Menu.AddItem(new MenuItem("useharass", "Harass")).SetValue(true);
            Menu.AddItem(new MenuItem("checkCollision", "Check collision")).SetValue(true);
            Menu.AddItem(new MenuItem("prediction", "Prediction")).SetValue(new StringList(
                new[] {"SPrediction", "Kurisu", "Kurisu Old", "Drito"}
            ));
            Menu.AddItem(new MenuItem("hitchance", "Hit Chance").SetValue<StringList>(new StringList(HitchanceDisplay, 3)));
            Menu.AddItem(new MenuItem("manapct", "Minimum Mana %")).SetValue(new Slider(55));

            Menu.AddToMainMenu();

            // Set the skillshot.
            Javelin.SetSkillshot(Game.Version.Contains("5.15") ? 0.25f : 0.125f, 40f, 1300f, true, SkillshotType.SkillshotLine);

            // Init SPrediction
            SPrediction.Prediction.Initialize(Menu);
        }

        public void Game_OnGameUpdate(EventArgs args) {
            // Check if enabled
            if (!Menu.Item("activated").GetValue<bool>()) return;

            // Target selector, select our target based on collision if set by the user.
            var cc = Menu.Item("checkCollision").GetValue<bool>();
            var target = cc ? TargetSelector.GetTargetNoCollision(Javelin) : TargetSelector.GetTarget(Javelin.Range, TargetSelector.DamageType.Magical);
            //var target = TargetSelector.GetTargetNoCollision(Javelin);
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
