namespace VGameFoundation.Scripts.Services.TransitionService
{
    using System;
    using Cysharp.Threading.Tasks;
    using DG.Tweening;
    using UnityEngine;

    public class CircleTransitionModule : TransitionModuleBase
    {
        [SerializeField] private RectTransform circleTransform;
        [SerializeField] private float         maxSize = 2500f;
        [SerializeField] private float         minSize = 0f;

        protected override void KillTween() { DOTween.Kill(this.circleTransform); }

        public override void SetVisualState(bool isObscured)
        {
            this.ActivateModule();
            this.KillTween();
            var size = isObscured ? this.minSize : this.maxSize;
            this.circleTransform.sizeDelta = new Vector2(size, size);
        }

        public override UniTask Show(float duration, Action onComplete)
        {
#if UNITASK_DOTWEEN_SUPPORT
            this.SetVisualState(false);

            return this.circleTransform.DOSizeDelta(new Vector2(this.minSize, this.minSize), duration)
                .SetEase(Ease.OutQuad).SetUpdate(true)
                .OnComplete(() => onComplete?.Invoke()).ToUniTask();
#endif
        }

        public override UniTask Hide(float duration, Action onComplete)
        {
#if UNITASK_DOTWEEN_SUPPORT
            this.SetVisualState(true);

            return this.circleTransform.DOSizeDelta(new Vector2(this.maxSize, this.maxSize), duration)
                .SetEase(Ease.OutQuad).SetUpdate(true)
                .OnComplete(() =>
                {
                    this.ResetState();
                    onComplete?.Invoke();
                }).ToUniTask();
#endif
        }
    }
}