﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LoveMachine.Core.Buttplug;
using LoveMachine.Core.Game;
using LoveMachine.Core.NonPortable;
using UnityEngine;

namespace LoveMachine.Core.Controller
{
    internal abstract class ButtplugController : CoroutineHandler
    {
        private readonly Dictionary<Device, float> normalizedLatencies =
            new Dictionary<Device, float>();
        
        private AnimationAnalyzer analyzer;
        
        [HideFromIl2Cpp]
        protected ButtplugWsClient Client { get; private set; }
        
        [HideFromIl2Cpp]
        protected GameAdapter Game { get; private set; }
        
        [HideFromIl2Cpp]
        public abstract string FeatureName { get; }
        
        public abstract bool IsDeviceSupported(Device device);

        protected abstract IEnumerator Run(Device device);
        
        private void Start()
        {
            Client = GetComponent<ButtplugWsClient>();
            Game = GetComponent<GameAdapter>();
            analyzer = GetComponent<AnimationAnalyzer>();
            Game.OnHStarted += (s, a) => OnStartH();
            Game.OnHEnded += (s, a) => OnEndH();
            Client.OnDeviceListUpdated += (s, a) => Restart();
        }

        private void OnStartH() => HandleCoroutine(Run());

        private void OnEndH()
        {
            StopAllCoroutines();
            Client.StopAllDevices();
        }

        private void Restart()
        {
            if (Game.IsHSceneRunning)
            {
                OnEndH();
                OnStartH();
            }
        }

        private void OnDestroy() => StopAllCoroutines();

        private IEnumerator Run()
        {
            foreach (var device in Client.Devices.Where(IsDeviceSupported))
            {
                Logger.LogInfo($"Running controller {GetType().Name} " +
                               $"on device #{device.DeviceIndex} ({device.DeviceName}).");
                HandleCoroutine(Run(device));
                HandleCoroutine(RunLatencyUpdateLoop(device));
            }
            yield break;
        }
        
        private IEnumerator RunLatencyUpdateLoop(Device device)
        {
            while (true)
            {
                // updating the latency in real time causes a lot of stutter when
                // there's a gradual change in animation speed
                // updating every 3s and caching the result solves this
                yield return new WaitForSecondsRealtime(3f);
                float animTimeSecs = Game.GetAnimationTimeSecs(device.Settings.GirlIndex);
                normalizedLatencies[device] = device.Settings.LatencyMs / 1000f / animTimeSecs;
            }
        }

        private float GetLatencyCorrectedNormalizedTime(Device device)
        {
            if (!normalizedLatencies.TryGetValue(device, out float normalizedLatency))
            {
                normalizedLatency = 0f;
            }
            Game.GetAnimState(device.Settings.GirlIndex, out float currentNormTime, out _, out _);
            return currentNormTime + normalizedLatency;
        }

        protected virtual bool TryGetCurrentStrokeInfo(Device device, out StrokeInfo result)
        {
            var girlIndex = device.Settings.GirlIndex;
            var bone = device.Settings.Bone;
            float normalizedTime = GetLatencyCorrectedNormalizedTime(device);
            return analyzer.TryGetCurrentStrokeInfo(girlIndex, bone, normalizedTime, out result);
        }
        
        /// <summary>
        /// Converts the given seconds to in-game time if possible, and waits that long. <br/>
        /// WARNING: THIS WILL STILL HANG IF THE GAME IS PAUSED DURING THE YIELD! <br/>
        /// Why use it then: more in-sync with the game than Realtime, and probably more efficient
        /// </summary>
        protected object WaitForSecondsUnscaled(float seconds) => Time.timeScale > 0f
            ? (object)new WaitForSeconds(seconds * Time.timeScale)
            : new WaitForSecondsRealtime(seconds);
    }
}