using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using SharpDX.Direct3D9;
using Color = System.Drawing.Color;
using Font = SharpDX.Direct3D9.Font;

namespace netstat_ {
    internal class netstat {

        // 2d vektor
        static Vector2 wts = new Vector2(Drawing.Width - 210f, 25f);

        int ping_delta = 0;
        int last_ping = 0;
        int ping_max = 0;
        int ping_min = 65535;

        // bar
        Font Text;
        //static float BarX = Drawing.Width  * 0.425f;
        static float BarX = Drawing.Width-200f;
        //static float BarY = Drawing.Height * 0.80f;
        static float BarY = wts[1]+20;
        static int BarWidth = (int)(Drawing.Width - 2 * BarX);
        int BarHeight = 6;
        int SeperatorHeight = 5;
        static float Scale = (float)BarWidth / 800;

        // Constructor
        public netstat() {
            Text = new Font(Drawing.Direct3DDevice, new FontDescription { FaceName = "Calibri", Height = 13, Width = 6, OutputPrecision = FontPrecision.Default, Quality = FontQuality.Default });
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
            Game.OnUpdate                += Game_OnGameUpdate;
            Drawing.OnDraw               += Drawing_OnDraw;
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
        }

        public void Game_OnGameUpdate(EventArgs args) {
            int current_ping = LeagueSharp.Game.Ping; // Get the current ping
            ping_delta = current_ping - last_ping; // Calculate the delta
            last_ping = current_ping; // And reset.

            // Min-max
            if (last_ping > ping_max) { ping_max = last_ping; }
            if (last_ping < ping_min) { ping_min = last_ping; }
        }

        public void DrawRect(float x, float y, int width, int height, float thickness, System.Drawing.Color color) {
            for (int i = 0; i < height; i++)
                Drawing.DrawLine(x, y + i, x + width, y + i, thickness, color);
        }

        public void Drawing_OnDraw(EventArgs args) {
            Drawing.DrawText(wts[0], wts[1], Color.Silver, "Jitter: " + ping_delta.ToString() + " ms");
            Drawing.DrawText(Drawing.Width - 210f, wts[1] + 20, Color.DarkCyan, "Min: " + ping_min.ToString() + " ms");
            Drawing.DrawText(Drawing.Width - 100f, wts[1] + 20, Color.SpringGreen, "Max: " + ping_max.ToString() + " ms");

            /*DrawRect(BarX, BarY, (int)(Scale * (float)last_ping), BarHeight, 1, System.Drawing.Color.FromArgb((int)(100f * 1f), System.Drawing.Color.White));
            //DrawRect(BarX + Scale * (float)last_ping, BarY - SeperatorHeight, 0, SeperatorHeight + 1, 1, System.Drawing.Color.FromArgb((int)(255f * 1f), System.Drawing.Color.White));
            Text.DrawText(null, last_ping.ToString() + "ms", (int)BarX + (int)(Scale * (float)last_ping - (float)(7 * Text.Description.Width) / 2), (int)BarY + SeperatorHeight + Text.Description.Height / 2, new ColorBGRA(255, 92, 92, 255));*/
        }

    }
}