using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using DG.Tweening;

namespace VGameFoundation.Scripts.UI
{
    public abstract class UIAnimationModule : MonoBehaviour
    {
        public abstract void ResetState();

        public abstract void Prepare();

        public abstract UniTask Play(bool isForward);

        public abstract void Kill();
    }

    public abstract class UIAnimationModuleByDOTween : UIAnimationModule
    {
        [Header("Settings")] [SerializeField] protected float duration = 0.5f;
        [SerializeField]                      protected Ease  ease     = Ease.OutQuad;
    }
}