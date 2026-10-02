namespace VGameFoundation.Scripts.UI
{
    using Cysharp.Threading.Tasks;
    using UnityEngine;

    public interface IUIView : IView
    {
        public bool          IsReadyToUse  { get; }
        public RectTransform RectTransform { get; set; }
        public UniTask       Open();
        public UniTask Close();
        public void CloseImmediate();
        public void Hide();
        public void Show();
        public void DestroyView();
    }
}

