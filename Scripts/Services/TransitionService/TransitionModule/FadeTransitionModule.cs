namespace VGameFoundation.Scripts.Services.TransitionService
{
    using System;
    using Cysharp.Threading.Tasks;
    using DG.Tweening;
    using UnityEngine;
    using UnityEngine.UI;

    public class FadeTransitionModule : TransitionModuleBase
    {
        [SerializeField] private Image fadeImage;

        protected override void KillTween() { DOTween.Kill(this.fadeImage); }

        public override void SetVisualState(bool isObscured)
        {
            this.ActivateModule();
            this.KillTween();
            this.fadeImage.color = new Color(this.fadeImage.color.r, this.fadeImage.color.g, this.fadeImage.color.b, isObscured ? 1f : 0f);
        }

        public override UniTask Show(float duration, Action onComplete)
        {
#if UNITASK_DOTWEEN_SUPPORT
            this.SetVisualState(false);

            return this.fadeImage.DOFade(1f, duration).SetUpdate(true).OnComplete(() => onComplete?.Invoke()).ToUniTask();
#endif
        }

        public override UniTask Hide(float duration, Action onComplete)
        {
#if UNITASK_DOTWEEN_SUPPORT
            this.SetVisualState(true);

            return this.fadeImage.DOFade(0f, duration).SetUpdate(true).OnComplete(() =>
            {
                this.ResetState();
                onComplete?.Invoke();
            }).ToUniTask();
        }
#endif
    }
}