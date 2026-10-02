using Cysharp.Threading.Tasks;
using UnityEngine;

namespace VGameFoundation.Scripts.UI
{
    using Sirenix.OdinInspector;

    public class UIPopupBaseView : UIView
    {
        [Header("Animation Modules")] [SerializeField] [InlineEditor(InlineEditorObjectFieldModes.Boxed)]
        private UIAnimationModule openAnimation;

        [SerializeField] [PropertySpace(SpaceAfter = 20)] [InlineEditor(InlineEditorObjectFieldModes.Boxed)]
        private UIAnimationModule closeAnimation;

        public override async UniTask Open()
        {
            await base.Open();

            if (this.closeAnimation != null) this.closeAnimation.ResetState();
            if (this.openAnimation != null) this.openAnimation.ResetState();

            if (this.openAnimation != null)
            {
                this.openAnimation.Prepare();
                await this.openAnimation.Play(true);
            }
        }

        public override async UniTask Close()
        {
            if (this.closeAnimation != null) await this.closeAnimation.Play(false);

            await base.Close();
        }

        private void OnDestroy()
        {
            if (this.openAnimation) this.openAnimation.Kill();
            if (this.closeAnimation) this.closeAnimation.Kill();
        }
    }
}