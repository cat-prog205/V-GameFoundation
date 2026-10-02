namespace VGameFoundation.Scripts.UI
{
    using Cysharp.Threading.Tasks;
    using DG.Tweening;
    using UnityEngine;

    [RequireComponent(typeof(CanvasGroup))]
    public class UIFadeAnimation : UIAnimationModuleByDOTween
    {
        [SerializeField] private CanvasGroup targetGroup;

        private void Reset() { this.targetGroup = this.GetComponent<CanvasGroup>(); }

        public override void ResetState()
        {
            if (this.targetGroup)
            {
                this.targetGroup.alpha          = 1f;
                this.targetGroup.blocksRaycasts = true;
            }
        }

        public override void Prepare()
        {
            if (this.targetGroup) this.targetGroup.alpha = 0f;
        }

        public override void Kill() { this.targetGroup.DOKill(); }

        public override async UniTask Play(bool isForward)
        {
            if (!this.targetGroup) return;
            this.Kill();
            await this.targetGroup.DOFade(isForward ? 1f : 0f, this.duration)
                .SetEase(this.ease).SetUpdate(true)
                .ToUniTask();
        }
    }
}