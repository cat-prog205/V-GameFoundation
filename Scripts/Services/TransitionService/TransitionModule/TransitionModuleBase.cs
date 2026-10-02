using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace VGameFoundation.Scripts.Services.TransitionService
{
    public interface ITransitionModule
    {
        void Setup(GameObject rootObj);

        UniTask Show(float duration, Action onComplete);

        UniTask Hide(float duration, Action onComplete);

        void SetVisualState(bool isObscured);

        void ResetState();
    }

    [RequireComponent(typeof(CanvasGroup))]
    public abstract class TransitionModuleBase : MonoBehaviour, ITransitionModule
    {
        [SerializeField] protected CanvasGroup canvasGroup;

        private void Reset() { this.canvasGroup = this.GetComponent<CanvasGroup>(); }

        public virtual void Setup(GameObject rootObj) { this.ResetState(); }

        public abstract UniTask Show(float duration, Action onComplete);

        public abstract UniTask Hide(float duration, Action onComplete);

        public abstract void SetVisualState(bool isObscured);

        public virtual void ResetState()
        {
            this.KillTween();
            this.canvasGroup.alpha          = 0f;
            this.canvasGroup.blocksRaycasts = false;
        }

        protected void ActivateModule()
        {
            this.canvasGroup.alpha          = 1f;
            this.canvasGroup.blocksRaycasts = true;
        }

        protected virtual void KillTween() { }
    }
}