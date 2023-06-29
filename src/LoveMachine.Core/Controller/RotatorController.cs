﻿using System.Collections;
using LoveMachine.Core.Buttplug;
using LoveMachine.Core.Config;
using LoveMachine.Core.Game;
using UnityEngine;

namespace LoveMachine.Core.Controller
{
    internal sealed class RotatorController : ClassicButtplugController
    {
        private bool clockwise = true;

        public override bool IsDeviceSupported(Device device) => device.IsRotator;

        protected override IEnumerator HandleAnimation(Device device, StrokeInfo strokeInfo)
        {
            float strokeTimeSecs = strokeInfo.DurationSecs;
            float timeToCompletionSecs = strokeTimeSecs * (1f - strokeInfo.Completion);
            HandleCoroutine(DoRotate(device, strokeTimeSecs));
            yield return WaitForSecondsUnscaled(timeToCompletionSecs);
            if (UnityEngine.Random.value <= RotatorConfig.RotationDirectionChangeChance.Value)
            {
                clockwise = !clockwise;
            }
        }

        protected override IEnumerator HandleOrgasm(Device device)
        {
            Client.RotateCmd(device, 1f, clockwise);
            yield break;
        }

        protected override void HandleLevel(Device device, float level, float durationSecs) =>
            Client.RotateCmd(device, level, true);

        private IEnumerator DoRotate(Device device, float strokeTimeSecs)
        {
            float downStrokeTimeSecs = strokeTimeSecs / 2f;
            float downSpeed = Mathf.Lerp(0.3f, 1f, 0.4f / strokeTimeSecs) *
                RotatorConfig.RotationSpeedRatio.Value;
            float upSpeed = downSpeed * 0.8f;
            Client.RotateCmd(device, downSpeed, clockwise);
            yield return WaitForSecondsUnscaled(downStrokeTimeSecs);
            Client.RotateCmd(device, upSpeed, !clockwise);
        }
    }
}