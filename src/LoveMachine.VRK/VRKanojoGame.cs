﻿using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using LoveMachine.Core.Game;
using LoveMachine.Core.Common;
using UnityEngine;

namespace LoveMachine.VRK
{
    internal class VRKanojoGame : GameAdapter
    {
        private CharFemale sakura;

        protected override int AnimationLayer => 0;

        protected override Dictionary<Bone, string> FemaleBoneNames => new Dictionary<Bone, string>
        {
            { Bone.Vagina, "cf_J_Kosi01_s" },
            { Bone.RightHand, "cf_J_Hand_s_R" },
            { Bone.LeftHand, "cf_J_Hand_s_L" },
            { Bone.RightBreast, "cf_J_Mune_Nip01_s_R" },
            { Bone.LeftBreast, "cf_J_Mune_Nip01_s_L" }
        };

        protected override int HeroineCount => 1;

        protected override int MaxHeroineCount => 1;

        protected override bool IsHardSex => false;

        protected override float PenisSize => 0.1f;

        protected override MethodInfo[] StartHMethods => new[]
        {
            AccessTools.Method(typeof(VK_H_Houshi_Sonyu), nameof(VK_H_Houshi_Sonyu.Start))
        };

        protected override MethodInfo[] EndHMethods => new[]
        {
            AccessTools.Method(typeof(VK_H_Houshi_Sonyu), nameof(VK_H_Houshi_Sonyu.OnDestroy))
        };

        protected override Animator GetFemaleAnimator(int girlIndex) => sakura.animBody;

        protected override Transform PenisBase => GameObject.Find("cm_J_dan_s").transform;

        protected override GameObject GetFemaleRoot(int girlIndex) => null;

        protected override string GetPose(int girlIndex) => sakura.nowMotionName;

        protected override bool IsIdle(int girlIndex) =>
            sakura.nowMotionName == "Idle" || sakura.nowMotionName.EndsWith("_A");

        protected override bool IsOrgasming(int girlIndex) =>
            sakura.nowMotionName.StartsWith("Orgasm") && !sakura.nowMotionName.EndsWith("_A");

        protected override IEnumerator UntilReady(object hscene)
        {
            yield return new WaitForSeconds(5f);
            sakura = ((VK_H_Houshi_Sonyu)hscene).chaFemale;
        }
    }
}