﻿using BepInEx.Unity.IL2CPP.Utils.Collections;
using System.Collections;
using UnityEngine;

namespace LoveMachine.Core
{
    public class CoroutineHandler : MonoBehaviour
    {
        protected Coroutine HandleCoroutine(IEnumerator coroutine,
            bool suppressExceptions = false) =>
            StartCoroutine(CoroutineUtil.HandleExceptions(coroutine, suppressExceptions)
                .WrapToIl2Cpp());

        // couldn't figure out how to convert Func<> to Il2CppSystem.Func<>
        // oh well
        private static IEnumerator _WaitWhile(Func<bool> condition)
        {
            while (condition())
            {
                yield return new WaitForEndOfFrame();
            }
        }

        protected Coroutine WaitWhile(Func<bool> condition) =>
            HandleCoroutine(_WaitWhile(condition));

        protected Coroutine WaitUntil(Func<bool> condition) => WaitWhile(() => !condition());
    }
}