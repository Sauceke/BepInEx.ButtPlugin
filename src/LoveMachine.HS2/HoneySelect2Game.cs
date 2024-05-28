﻿using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using IllusionUtility.GetUtility;
using UnityEngine;

namespace LoveMachine.HS2
{
    internal sealed class HoneySelect2Game : AbstractHS2Game
    {
        private readonly string[] idleAnimations =
        {
            "Idle", "WIdle", "SIdle", "Insert", "D_Idle", "D_Insert",
            "Orgasm_A", "Orgasm_OUT_A", "Drink_A", "Vomit_A", "Orgasm_IN_A", "OrgasmM_OUT_A",
            "D_Orgasm_A", "D_Orgasm_OUT_A", "D_Orgasm_IN_A", "D_OrgasmM_OUT_A"
        };
        
        private HScene hScene;
        private GameObject[] roots;
        internal Animator[] animators;

        protected override int HeroineCount =>
            Array.FindAll(hScene.GetFemales(), f => f != null).Length;

        protected override int MaxHeroineCount => 2;

        protected override bool IsHardSex => hScene.ctrlFlag.loopType == 1;

        protected override int AnimationLayer => 0;

        protected override float PenisSize => 0.4f;

        protected override Transform PenisBase => throw new NotImplementedException();

        protected override Transform[] PenisBases => hScene.GetMales()
            .Where(male => male != null)
            .Select(male => male.objBodyBone.transform.FindLoop("cm_J_dan_f_L").transform)
            .ToArray();

        protected override MethodInfo[] StartHMethods =>
            new[] { AccessTools.Method(typeof(HScene), nameof(HScene.SetStartVoice)) };

        protected override MethodInfo[] EndHMethods =>
            new[] { AccessTools.Method(typeof(HScene), nameof(HScene.EndProc)) };

        protected override Animator GetFemaleAnimator(int girlIndex) => animators[girlIndex];

        protected override GameObject GetFemaleRoot(int girlIndex) => roots[girlIndex];

        protected override string GetPose(int girlIndex) =>
            // couldn't find accessor for animation name so going with hash
            hScene.ctrlFlag.nowAnimationInfo.id
                + "." + GetAnimatorStateInfo(girlIndex).fullPathHash;

        protected override IEnumerator UntilReady(object instance)
        {
            hScene = (HScene)instance;
            yield return new WaitWhile(() => hScene.GetFemales().Length == 0
                || hScene.GetFemales()[0] == null
                || hScene.GetMales().Length == 0
                || hScene.GetMales()[0] == null);
            animators = hScene?.GetFemales().Select(female => female?.animBody).ToArray();
            roots = hScene?.GetFemales().Select(female => female?.objBodyBone).ToArray();
        }

        protected override bool IsIdle(int girlIndex) =>
            idleAnimations.Any(GetAnimatorStateInfo(girlIndex).IsName);

        protected override bool IsOrgasming(int girlIndex) => hScene.ctrlFlag.nowOrgasm;
    }
}