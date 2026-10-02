using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using System;

namespace VGameFoundation.Scripts.UI
{
    public class UISlideAnimation : UIAnimationModuleByDOTween
    {
        public enum SlideDirection
        {
            Top,
            Bottom,
            Left,
            Right
        }

        [Header("Slide Settings")] [SerializeField]
        private RectTransform targetTransform;

        [SerializeField] private SlideDirection direction      = SlideDirection.Bottom;
        [SerializeField] private float          offsetDistance = 500f;

        private Vector2 originalPosition;
        private bool    isInitialized = false;

        private void Reset() { this.targetTransform = this.GetComponent<RectTransform>(); }

        private void Initialize()
        {
            if (this.isInitialized) return;

            if (this.targetTransform == null) this.targetTransform = this.GetComponent<RectTransform>();

            if (this.targetTransform != null) this.originalPosition = this.targetTransform.anchoredPosition;

            this.isInitialized = true;
        }

        public override void ResetState()
        {
            this.Initialize();
            if (this.targetTransform) this.targetTransform.anchoredPosition = this.originalPosition;
        }

        public override void Prepare()
        {
            this.Initialize();
            if (this.targetTransform) this.targetTransform.anchoredPosition = this.GetOffsetPosition();
        }

        public override void Kill() { this.targetTransform.DOKill(); }

        public override async UniTask Play(bool isForward)
        {
            this.Initialize();

            if (!this.targetTransform) return;

            this.Kill();

            var endValue = isForward ? this.originalPosition : this.GetOffsetPosition();

            await this.targetTransform.DOAnchorPos(endValue, this.duration)
                .SetEase(this.ease)
                .SetUpdate(true)
                .ToUniTask();
        }

        private Vector2 GetOffsetPosition()
        {
            return this.direction switch
            {
                SlideDirection.Top => this.originalPosition + new Vector2(0, this.offsetDistance),
                SlideDirection.Bottom => this.originalPosition + new Vector2(0, -this.offsetDistance),
                SlideDirection.Left => this.originalPosition + new Vector2(-this.offsetDistance, 0),
                SlideDirection.Right => this.originalPosition + new Vector2(this.offsetDistance, 0),
                _ => this.originalPosition
            };
        }
    }
}