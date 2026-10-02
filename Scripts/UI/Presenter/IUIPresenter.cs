namespace VGameFoundation.Scripts.UI
{
    using System;
    using Cysharp.Threading.Tasks;
    using UnityEngine;

    public interface IUIPresenter : IPresenter
    {
        public void SetView(IUIView viewInstance);
        public void SetViewParent(Transform parent);

        public UniTask OpenViewAsync();
        public UniTask CloseViewAsync();

        public void CloseView();

        public void CloseViewImmediate();
        public void HideView();
        public void ShowView();
        
        public void DestroyView();
    }
    
    public interface IUIPresenter<in TModel> : IUIPresenter where TModel:IUIModel
    {
        public UniTask OpenView(TModel model);
    }
    
    public enum ScreenStatus
    {
        Opened,
        Closed,
        Hide,
        Destroyed,
    }

}

