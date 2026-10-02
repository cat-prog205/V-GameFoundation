namespace VGameFoundation.Scripts.UI
{
    using Cysharp.Threading.Tasks;
    using UnityEngine;
    using VGameFoundation.Scripts.Utilities.LogService;

    [RequireComponent(typeof(CanvasGroup))]
    public class UIView : MonoBehaviour, IUIView
    {
        private CanvasGroup          canvasGroup;
        public bool          IsReadyToUse  { get; set; }
        public RectTransform RectTransform { get; set; }

        protected virtual void Awake()
        {
            this.RectTransform     = GetComponent<RectTransform>();
            this.canvasGroup       = GetComponent<CanvasGroup>();
            this.canvasGroup.alpha = 0;
            this.IsReadyToUse      = true;
        }

        public virtual UniTask Open()
        {
            this.Show();
            LogService.Log($"UI Open {this.GetType().Name}");
            return UniTask.CompletedTask;
        }

        public virtual UniTask Close()
        {
            this.Hide();
            LogService.Log($"UI Close {this.GetType().Name}");
            return UniTask.CompletedTask;
        }

        public virtual void CloseImmediate()
        {
            this.Hide();
            LogService.Log($"UI Close {this.GetType().Name}");
        }

        public virtual void Show()
        {
            this.canvasGroup.Show();
        }

        public void DestroyView()
        {
            Destroy(this.gameObject);
        }

        public virtual void Hide()
        {
            this.canvasGroup.Hide();
        }
    }
}

