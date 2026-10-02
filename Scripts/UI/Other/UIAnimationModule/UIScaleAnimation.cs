namespace VGameFoundation.Scripts.UI
{
    using Cysharp.Threading.Tasks;
    using DG.Tweening;
    using UnityEngine;

    public class UIScaleAnimation : UIAnimationModuleByDOTween
    {
        [SerializeField] private RectTransform targetTransform;

        private void Reset() { this.targetTransform = this.GetComponent<RectTransform>(); }

        public override void ResetState()
        {
            if (this.targetTransform) this.targetTransform.localScale = Vector3.one;
        }

        public override void Prepare()
        {
            if (this.targetTransform) this.targetTransform.localScale = Vector3.zero;
        }

        public override void Kill() { this.targetTransform.DOKill(); }

        public override async UniTask Play(bool isForward)
        {
            if (!this.targetTransform) return;
            this.Kill();
            await this.targetTransform.DOScale(isForward ? Vector3.one : Vector3.zero, this.duration)
                .SetEase(this.ease).SetUpdate(true)
                .ToUniTask();
        }
    }
}