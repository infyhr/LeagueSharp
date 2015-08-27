using System;
using LeagueSharp;
using LeagueSharp.Common;

namespace NeedASpear {
    class Program {
        public static NeedASpear NeedASpear;

        private static void Main(string[] args) {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args) {
            new NeedASpear();
        }
    }
}