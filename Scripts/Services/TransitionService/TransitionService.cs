using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer.Unity;

namespace VGameFoundation.Scripts.Services.TransitionService
{
    using V_GameFoundation.Scripts.Utilities.UI;
    using VGameFoundation.Scripts.Services.GameAsset;

    public interface ITransitionService
    {
        UniTask Show<T>(float duration = 0.5f, Action onComplete = null) where T : Component, ITransitionModule;

        UniTask Hide<T>(float duration = 0.5f, Action onComplete = null) where T : Component, ITransitionModule;
    }

    public class TransitionService : ITransitionService, IInitializable
    {
        private readonly IGameAssets           gameAssets;
        private          LoadingTransitionView currentView;

        public TransitionService(IGameAssets gameAssets) { this.gameAssets = gameAssets; }

        public void Initialize() { this.PrepareView(); }

        private async void PrepareView()
        {
            if (this.currentView != null) return;

            var targetCanvas = CanvasOverlayHelper.GetCanvas(CanvasOverlayType.Transition);

            var prefab =
                await this.gameAssets.LoadAssetAsync<GameObject>(nameof(LoadingTransitionView));
            var view = UnityEngine.Object.Instantiate(prefab, targetCanvas.transform).GetComponent<LoadingTransitionView>();

            this.currentView = view;
            this.currentView.Initialize();
        }

        public async UniTask Show<T>(float duration = 0.5f, Action onComplete = null) where T : Component, ITransitionModule
        {
            this.PrepareView();
            await this.currentView.PlayShow<T>(duration, onComplete);
        }

        public async UniTask Hide<T>(float duration = 0.5f, Action onComplete = null) where T : Component, ITransitionModule
        {
            if (this.currentView == null) return;
            await this.currentView.PlayHide<T>(duration, onComplete);
        }
    }
}