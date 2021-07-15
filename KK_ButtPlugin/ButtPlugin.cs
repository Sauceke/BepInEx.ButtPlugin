﻿using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using KKAPI.MainGame;

namespace KK_ButtPlugin
{
    [BepInPlugin(GUID, "ButtPlugin", Version)]
    internal class ButtPlugin : BaseUnityPlugin
    {
        public const string GUID = "Sauceke.ButtPlugin";
        public const string Version = "1.0.0";

        public static new ManualLogSource Logger { get; private set; }

        public static ConfigEntry<string> WebSocketAddress { get; private set; }
        public static ConfigEntry<int> MaxStrokesPerMinute { get; private set; }
        public static ConfigEntry<int> LatencyMs { get; private set; }
        public static ConfigEntry<bool> EnableVibrate { get; private set; }

        private void Start()
        {
            WebSocketAddress = Config.Bind(
                section: "Network",
                key: "WebSocket address",
                defaultValue: "ws://localhost:12345/",
                "The Buttplug server address (requires game restart).");
            MaxStrokesPerMinute = Config.Bind(
                section: "Device",
                key: "Maximum strokes per minute",
                defaultValue: 140,
                "The top speed possible on your stroker at 70% stroke length.");
            LatencyMs = Config.Bind(
                section: "Device",
                key: "Latency (ms)",
                defaultValue: 0,
                "The difference in latency between your stroker and your display. \n" +
                "Negative if your stroker has lower latency.");
            EnableVibrate = Config.Bind(
                section: "Device",
                key: "Enable Vibrators",
                defaultValue: false,
                "Maps control speed to vibrations");
            Logger = base.Logger;

            GameAPI.RegisterExtraBehaviour<ButtplugController>(GUID);
            Hooks.InstallHooks();
        }
    }
}
