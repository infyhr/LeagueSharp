using System;
using LeagueSharp;
using LeagueSharp.Common;

namespace netstat_ {
    class Program {
        public static netstat netstat;

        private static void Main(string[] args) {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args) {
            netstat = new netstat();
        }
    }
}