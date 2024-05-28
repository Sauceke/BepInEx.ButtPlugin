﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using H;
using HarmonyLib;
using IllusionUtility.GetUtility;
using LoveMachine.Core.Game;
using LoveMachine.Core.Common;
using UnityEngine;

namespace LoveMachine.PH
{
    internal sealed class PlayHomeGame : GameAdapter
    {
        private static readonly H_STATE[] activeHStates = { H_STATE.LOOP, H_STATE.SPURT };

        private static readonly H_STATE[] orgasmStates =
        {
            H_STATE.IN_EJA_IN, H_STATE.IN_EJA_TREMBLE,
            H_STATE.OUT_EJA_IN, H_STATE.OUT_EJA_TREMBLE
        };

        private H_Scene scene;

        protected override Dictionary<Bone, string> FemaleBoneNames => new Dictionary<Bone, string>
        {
            { Bone.LeftBreast, "k_f_munenipL_00" },
            { Bone.RightBreast, "k_f_munenipR_00" },
            { Bone.Vagina, "k_f_kokan_00" },
            { Bone.Mouth, "cf_J_MouthCavity" },
            { Bone.LeftHand, "cf_J_Hand_Index01_L" },
            { Bone.RightHand, "cf_J_Hand_Index01_R" },
            { Bone.LeftFoot, "k_f_toeL_00" },
            { Bone.RightFoot, "k_f_toeR_00" }
        };

        protected override int HeroineCount => scene.mainMembers.females.Count;

        protected override int MaxHeroineCount => 2;

        protected override bool IsHardSex => true;

        protected override int AnimationLayer => 0;

        protected override float PenisSize => 0.2f;

        protected override Transform PenisBase => throw new NotImplementedException();

        protected override Transform[] PenisBases => scene.mainMembers.males
            .Select(male => male.objBodyBone.transform.FindLoop("k_m_tamaC_00").transform)
            .ToArray();

        protected override MethodInfo[] StartHMethods =>
            new[] { AccessTools.Method(typeof(H_Scene), nameof(H_Scene.Awake)) };

        protected override MethodInfo[] EndHMethods =>
            new[] { AccessTools.Method(typeof(H_Scene), nameof(H_Scene.Exit)) };

        protected override Animator GetFemaleAnimator(int girlIndex) =>
            scene.mainMembers.females[girlIndex].body.Anime;

        protected override GameObject GetFemaleRoot(int girlIndex) =>
            scene.mainMembers.females[girlIndex].objBodyBone;

        protected override string GetPose(int girlIndex) =>
            (scene.mainMembers.StyleData?.id ?? "none")
                + "." + GetAnimatorStateInfo(girlIndex).fullPathHash;

        protected override bool IsIdle(int _) =>
            !activeHStates.Contains(scene.mainMembers.StateMgr.nowStateID);

        protected override bool IsOrgasming(int _) =>
            orgasmStates.Contains(scene.mainMembers.StateMgr.nowStateID);

        protected override IEnumerator UntilReady(object instance)
        {
            scene = (H_Scene)instance;
            yield return new WaitWhile(() => scene.mainMembers?.StyleData == null
                || scene.mainMembers.females.IsNullOrEmpty()
                || scene.mainMembers.males.IsNullOrEmpty()
                || scene.mainMembers.females[0] == null
                || scene.mainMembers.males[0] == null);
        }
    }
}