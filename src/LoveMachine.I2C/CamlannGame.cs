﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using LoveMachine.Core.Game;
using LoveMachine.Core.Common;
using UnityEngine;

namespace LoveMachine.I2C
{
    public class CamlannGame : GameAdapter
    {
        private GameObject hRoot;
        private Animator hAnimator;

        protected override MethodInfo[] StartHMethods =>
            new[] { AccessTools.Method("MyHMotionController, Assembly-CSharp:Start") };
        
        protected override MethodInfo[] EndHMethods => new[]
        {
            AccessTools.Method("MyTeikouHandler, Assembly-CSharp:ResetToLocomotion"),
            AccessTools.Method("MyGalleryHandler, Assembly-CSharp:DelusionSwitchTo")
        };

        protected override Dictionary<Bone, string> FemaleBoneNames => new Dictionary<Bone, string>
        {
            { Bone.Vagina, "MANKO-PIXEL" }
        };

        protected override Transform PenisBase => throw new NotImplementedException();

        protected override Transform[] PenisBases => FindDeepChildrenByPath(hRoot, "Dildo");

        protected override int AnimationLayer => 0;

        protected override int HeroineCount => 1;
        
        protected override int MaxHeroineCount => 1;

        protected override bool IsHardSex => true;

        protected override Animator GetFemaleAnimator(int girlIndex) => hAnimator;

        protected override GameObject GetFemaleRoot(int girlIndex) => hRoot;

        protected override string GetPose(int girlIndex) =>
            GetAnimatorStateInfo(0).fullPathHash.ToString();

        protected override bool IsIdle(int girlIndex) =>
            GetAnimatorStateInfo(0).IsName("loop_after");

        protected override bool IsOrgasming(int girlIndex) =>
            GetAnimatorStateInfo(0).IsName("orgasm");

        protected override IEnumerator UntilReady(object instance)
        {
            yield return new WaitForSeconds(1f);
            hAnimator = Traverse.Create(instance).Field<Animator>("motion_animator").Value;
            hRoot = hAnimator.gameObject;
        }
    }
}
