﻿using System;
using System.Collections.Generic;
using System.Linq;
using LitJson;
using LoveMachine.Core.Buttplug;
using LoveMachine.Core.Config;
using LoveMachine.Core.Controller;
using LoveMachine.Core.NonPortable;
using LoveMachine.Core.UI.Settings;
using LoveMachine.Core.UI.Util;
using UnityEngine;

namespace LoveMachine.Core.UI
{
    internal class DeviceListUI : CoroutineHandler
    {
        private ButtplugWsClient client;
        private ClassicButtplugController[] controllers;
        private SettingsUI[] drawers;
        private List<Device> cachedDeviceList = new List<Device>();
        private float testPosition;

        private void Start()
        {
            client = GetComponent<ButtplugWsClient>();
            controllers = GetComponents<ClassicButtplugController>();
            drawers = GetComponents<SettingsUI>();
            client.OnDeviceListUpdated += LogDevices;
            DeviceListConfig.OnDraw += DrawFullDeviceList;
        }

        private void LogDevices(object sender, ButtplugWsClient.DeviceListEventArgs args)
        {
            var devices = args.After;
            Logger.LogInfo($"List of devices: {JsonMapper.ToJson(devices)}");
            if (devices.Count == 0 && client.IsConnected)
            {
                Logger.LogMessage("Warning: No devices connected to Intiface.");
                return;
            }
            Logger.LogMessage($"{devices.Count} device(s) connected to Intiface.");
            devices
                .Where(device => !IsDeviceSupported(device))
                .Select(device => $"Warning: device \"{device.DeviceName}\" not supported.")
                .ToList()
                .ForEach(Logger.LogMessage);
        }

        private void DrawFullDeviceList(object sender, EventArgs args)
        {
            GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
            {
                GUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Connect", GUILayout.Width(150)))
                    {
                        client.Connect();
                    }
                    GUI.enabled = client.IsConnected;
                    if (GUILayout.Button("Scan", GUILayout.Width(150)))
                    {
                        client.StartScan();
                    }
                    GUI.enabled = true;
                    GUILayout.FlexibleSpace();
                }
                GUILayout.EndHorizontal();
                GUIUtil.SingleSpace();
                // imgui doesn't expect the layout to change outside of layout events
                if (Event.current.type == EventType.Layout)
                {
                    cachedDeviceList = client.Devices;
                }
                foreach (var device in cachedDeviceList)
                {
                    DrawDevicePanel(device);
                }
                if (DeviceListConfig.ShowOfflineDevices.Value)
                {
                    DrawOfflineDeviceList(cachedDeviceList);
                }
            }
            GUILayout.EndVertical();
        }

        private void DrawDevicePanel(Device device)
        {
            GUILayout.BeginVertical(GetDevicePanelStyle());
            {
                DrawDevice(device);
                GUILayout.BeginHorizontal();
                {
                    GUIUtil.LabelWithTooltip("Test", "Test this device");
                    GUILayout.HorizontalSlider(testPosition, 0f, 1f);
                    GUIUtil.SingleSpace();
                    if (GUILayout.Button("Test", GUILayout.ExpandWidth(false)))
                    {
                        TestDevice(device);
                    }
                }
                GUILayout.EndHorizontal();
                GUIUtil.SingleSpace();
            }
            GUILayout.EndVertical();
            GUIUtil.SingleSpace();
        }

        private void DrawOfflineDeviceList(List<Device> onlineDevices)
        {
            var settings = DeviceManager.DeviceSettings;
            foreach (var setting in settings)
            {
                if (onlineDevices.Any(device => device.Matches(setting)))
                {
                    continue;
                }
                GUILayout.BeginVertical(GetOfflineDevicePanelStyle());
                {
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.FlexibleSpace();
                        GUILayout.Label($"{setting.DeviceName} (Offline)");
                        GUILayout.FlexibleSpace();
                    }
                    GUILayout.EndHorizontal();
                    GUIUtil.SingleSpace();
                    Array.ForEach(drawers, drawer => drawer.Draw(setting));
                }
                GUILayout.EndVertical();
                GUIUtil.SingleSpace();
            }
            DeviceManager.DeviceSettings = settings;
        }
        
        private void DrawDevice(Device device)
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                GUILayout.Label(device.DeviceName);
                GUILayout.FlexibleSpace();
            }
            GUILayout.EndHorizontal();
            if (device.HasBatteryLevel)
            {
                GUIUtil.SingleSpace();
                GUILayout.BeginHorizontal();
                {
                    GUIUtil.PercentBar("Battery", "Current battery level.", device.BatteryLevel);
                }
                GUILayout.EndHorizontal();
            }
            GUIUtil.SingleSpace();
            GUILayout.BeginHorizontal();
            {
                GUIUtil.LabelWithTooltip("Features", "What this device can do.");
                foreach (var controller in controllers)
                {
                    GUILayout.Toggle(controller.IsDeviceSupported(device), controller.FeatureName);
                }
            }
            GUILayout.EndHorizontal();
            GUIUtil.SingleSpace();
            Array.ForEach(drawers, drawer => drawer.Draw(device.Settings));
        }

        private void TestDevice(Device device) => controllers.ToList()
            .ForEach(controller => controller.Test(device, pos => testPosition = pos));

        private bool IsDeviceSupported(Device device) =>
            controllers.Any(controller => controller.IsDeviceSupported(device));

        private static GUIStyle GetDevicePanelStyle() => new GUIStyle
        {
            margin = new RectOffset { left = 20, right = 20, top = 5, bottom = 5 },
            normal = new GUIStyleState { background = GetTexture(new Color(0f, 1f, 0.5f, 0.2f)) }
        };

        private static GUIStyle GetOfflineDevicePanelStyle() => new GUIStyle
        {
            margin = new RectOffset { left = 20, right = 20, top = 5, bottom = 5 },
            normal = new GUIStyleState { background = GetTexture(new Color(1f, 0f, 0.2f, 0.2f)) }
        };

        private static Texture2D GetTexture(Color color)
        {
            var texture = new Texture2D(1, 1);
            texture.SetPixels(new[] { color });
            texture.Apply();
            return texture;
        }
    }
}