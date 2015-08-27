using System;
using System.Collections.Generic;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace netstat_ {
    internal class netstat {

        // 2d vektor for drawing ping stuff
        public static Vector2 wp1; // 2d vector point 1
        public static Vector2 wp2; // 2d vector point 2
        public static Vector2 wts = new Vector2(Drawing.Width - 210f, 25f); // vector to screen

        // Menu
        public static Menu Menu;

        // Ping stuff
        public int ping_delta = 0;
        public int last_ping = 0;
        public int ping_max = 0;
        public int ping_min = 65535;

        // Constructor
        public netstat() {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        public void Game_OnGameLoad(EventArgs args) {
            Game.PrintChat("<font color='#FF0000'>netstat loaded</font>");

            // Return some useless information from the server
            Game.PrintChat("Game ID: " + Game.Id);
            Game.PrintChat("IP: " + Game.IP + ":" + Game.Port);
            Game.PrintChat("Map ID: " + Game.MapId);
            Game.PrintChat("Mode: " + Game.Mode);
            Game.PrintChat("Region: " + Game.Region);
            Game.PrintChat("Game Type: " + Game.Type);
            Game.PrintChat("Server version: " + Game.Version);

            // Set up the menu...
            Menu = new Menu("netstat", "netstat_menu", true);
            Menu.AddSubMenu(new Menu("On", "On"));
            Menu.SubMenu("On").AddItem(new MenuItem("Toggle", "Toggle")).SetValue(true);
            Menu.SubMenu("On").AddItem(new MenuItem("Range", "Scanning Range")).SetValue(new Slider(8000, 2000, 10000));
            Menu.SubMenu("On").AddItem(new MenuItem("AARange", "AA Range")).SetValue(new Slider(800, 1, 1500));
            Menu.SubMenu("On").AddItem(new MenuItem("DrawCircle", "Draw Circle")).SetValue(true);
            Menu.SubMenu("On").AddItem(new MenuItem("Size", "X size")).SetValue(new Slider(6, 2, 15));
            Menu.AddToMainMenu();
        }

        public void Game_OnGameUpdate(EventArgs args) {
            Utility.DelayAction.Add(1000, () => { // Delay 1s because we don't need to tick 80 times a second to get the ping.
                int current_ping = Game.Ping; // Get the current ping
                ping_delta = current_ping - last_ping; // Calculate the delta
                last_ping = current_ping; // And reset.

                // Min-max
                if(last_ping > ping_max) { ping_max = last_ping; }
                if(last_ping < ping_min) { ping_min = last_ping; }
            });
        }

        public void Drawing_OnDraw(EventArgs args) {
            Drawing.DrawText(wts[0], wts[1], Color.Silver, "Jitter: " + ping_delta.ToString() + " ms");
            Drawing.DrawText(Drawing.Width - 210f, wts[1] + 20, Color.DarkCyan, "Min: " + ping_min.ToString() + " ms");
            Drawing.DrawText(Drawing.Width - 100f, wts[1] + 20, Color.SpringGreen, "Max: " + ping_max.ToString() + " ms");

            if(Menu.Item("Toggle").GetValue<bool>()) { Drawing_drawWaypoint(); }
        }

        private static void DrawCross(float x, float y, float size, float thickness, Color color) {
            var topLeft = new Vector2(x - 10 * size, y - 10 * size);
            var topRight = new Vector2(x + 10 * size, y - 10 * size);
            var botLeft = new Vector2(x - 10 * size, y + 10 * size);
            var botRight = new Vector2(x + 10 * size, y + 10 * size);

            Drawing.DrawLine(topLeft.X, topLeft.Y, botRight.X, botRight.Y, thickness, color);
            Drawing.DrawLine(topRight.X, topRight.Y, botLeft.X, botLeft.Y, thickness, color);
        }

        public void Drawing_drawWaypoint() {
            //var enemy = TargetSelector.GetTarget(8000, TargetSelector.DamageType.Physical);
            var enemy = TargetSelector.GetTarget(Menu.Item("Range").GetValue<Slider>().Value, TargetSelector.DamageType.Physical);
            if (enemy == null || enemy.IsDead) return;

            // Only if it's far enough away
            if (ObjectManager.Player.Distance(enemy) < ObjectManager.Player.AttackRange + Menu.Item("AARange").GetValue<Slider>().Value) return;

            // Determine my position
            var my_clientPos    = Drawing.WorldToScreen(ObjectManager.Player.Position);

            // Determine both enemy positions
            var enemy_clientPos = Drawing.WorldToScreen(enemy.Position);
            var enemy_serverPos = Drawing.WorldToScreen(enemy.ServerPosition);

            // Get waypoints
            List<Vector2> waypoints = enemy.GetWaypoints();

            for (int i = 0; i < waypoints.Count - 1; i++) {
                wp1 = Drawing.WorldToScreen(waypoints[i].To3D());
                wp2 = Drawing.WorldToScreen(waypoints[i + 1].To3D());

                // No more waypoints
                if(!waypoints[i].IsOnScreen() && !waypoints[i + 1].IsOnScreen()) continue;

                // Draw
                Drawing.DrawLine(enemy_serverPos.X, enemy_serverPos.Y, wp2[0], wp2[1], 6, Color.Red);
                Drawing.DrawLine(enemy_clientPos.X, enemy_clientPos.Y, wp2[0], wp2[1], 6, Color.Green);

                // Draw a circle if it's set.
                if(Menu.Item("DrawCircle").GetValue<Boolean>()) {
                    Drawing.DrawCircle(enemy.ServerPosition, 100, Color.Red);
                    var size = Menu.Item("Size").GetValue<Slider>().Value;
                    Drawing.DrawLine(wp2[0] - size, wp2[1] - size, wp2[0] + size, wp2[1] + size, 2, Color.Gold);
                    Drawing.DrawLine(wp2[0] + size, wp2[1] - size, wp2[0] - size, wp2[1] + size, 2, Color.Gold);
                }
            }
        }

    }
}