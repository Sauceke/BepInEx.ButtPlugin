﻿using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using LoveMachine.Core.Game;
using LoveMachine.Core.Common;
using UnityEngine;

namespace LoveMachine.SC
{
    internal class SuccubusCafeGame : GameAdapter
    {
        private Traverse<int> ladyNumber;
        private Traverse<int> aideNumber;
        private Traverse<bool> aide_function;
        private Traverse<string> animName;
        private Traverse<float> upperAnimBlend;
        private Traverse<int> upperpower;
        private Traverse<bool> lowerAnimEvent;

        protected override int AnimationLayer => 0;

        protected override MethodInfo[] StartHMethods =>
            new[] { AccessTools.Method("Menu_Function, Assembly-CSharp:LadySexModeEnter") };

        protected override MethodInfo[] EndHMethods =>
            new[] { AccessTools.Method("Menu_Function, Assembly-CSharp:BackToMainMenu") };

        protected override Dictionary<Bone, string> FemaleBoneNames => new Dictionary<Bone, string>
        {
            { Bone.Vagina, "Upper_Vaginal" },
            { Bone.Mouth, "Lip_Tongue_3" },
            { Bone.LeftHand, "IndexFinger1_L" },
            { Bone.RightHand, "IndexFinger1_R" },
            { Bone.LeftFoot, "BigToe1_L" },
            { Bone.RightFoot, "BigToe1_R" },
            { Bone.LeftBreast, "Left_Nipple_Target" },
            { Bone.RightBreast, "Right_Nipple_Target" }
        };

        protected override Transform PenisBase => GameObject.Find("All_Model/player_A/Main/" +
            "DeformationSystem/Penis_root/Penis_0/Left_Testicle_root/Left_Testicle_0/" +
            "Left_Testicle_1")?.transform;

        protected override int HeroineCount => aide_function.Value ? 2 : 1;

        protected override int MaxHeroineCount => 2;

        protected override bool IsHardSex => false;

        protected override Animator GetFemaleAnimator(int girlIndex) =>
            GetFemaleRoot(girlIndex).GetComponent<Animator>();

        protected override GameObject GetFemaleRoot(int girlIndex) =>
            GameObject.Find("All_Model/Lady_" +
                "ABC"[(girlIndex == 0 ? ladyNumber : aideNumber).Value]);

        protected override string GetPose(int girlIndex) =>
            $"{animName.Value}.{upperAnimBlend.Value}.{upperpower.Value}";

        protected override bool IsIdle(int girlIndex) => string.IsNullOrEmpty(animName.Value);

        protected override bool IsOrgasming(int girlIndex) => !lowerAnimEvent.Value;

        protected override IEnumerator UntilReady(object menu)
        {
            yield return new WaitWhile(() => PenisBase == null);
            var script = Traverse.Create(menu).Field("ladySexAnimation_Function");
            ladyNumber = script.Field<int>("ladyNumber");
            aideNumber = script.Field<int>("aideNumber");
            aide_function = script.Field<bool>("aide_function");
            animName = script.Field<string>("animName");
            upperAnimBlend = script.Field<float>("upperAnimBlend");
            upperpower = script.Field<int>("upperpower");
            lowerAnimEvent = script.Field<bool>("lowerAnimEvent");
        }
    }
}