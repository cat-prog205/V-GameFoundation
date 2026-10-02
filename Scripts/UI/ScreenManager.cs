namespace VGameFoundation.Scripts.UI
{
    using System;
    using System.Collections.Generic;
    using Cysharp.Threading.Tasks;
    using UnityEngine;
    using VGameFoundation.DI;
    using VGameFoundation.Script.Utilities.Extension;
    using VGameFoundation.Scripts.Services.GameAsset;
    using VGameFoundation.Scripts.Utilities.LogService;
    using VGameFoundation.Signals;
    using Object = UnityEngine.Object;

    public class ScreenManager : IScreenManager, IDisposable
    {
        private readonly ISignalBus           signalBus;
        private readonly IGameAssets          gameAssets;
        private readonly IDependencyContainer dependencyContainer;
        
        private ScreenManagerHelper screenManagerHelper;
        private IUIPresenter        currentScreenActive;
        private IUIPresenter        currentPopupNonOverlayActive;
        
        private readonly Dictionary<Type, IUIPresenter> cachedScreenPresenterDic = new Dictionary<Type, IUIPresenter>();
        private readonly Queue<(IUIPresenter presenter, Func<UniTask> openAction)> popupQueue 
            = new Queue<(IUIPresenter, Func<UniTask>)>();
        private readonly HashSet<IUIPresenter> activeOverlays = new HashSet<IUIPresenter>();
        
        public ScreenManager(ISignalBus signalBus, IGameAssets gameAssets, IDependencyContainer dependencyContainer)
        {
            this.signalBus           = signalBus;
            this.gameAssets          = gameAssets;
            this.dependencyContainer = dependencyContainer;
            
            this.screenManagerHelper = Object.FindFirstObjectByType<ScreenManagerHelper>();
            
            this.signalBus.Subscribe<OnOpenScreenUI>(OnOpenScreenUIHandler);
            this.signalBus.Subscribe<OnCloseScreenUI>(OnCloseScreenUIHandler);
        }

        #region Screen

        public UniTask<TPresenter> OpenScreen<TPresenter, TModel>(TModel model) 
            where TPresenter : IUIPresenter<TModel> 
            where TModel : IUIModel
        {
            return this.OpenScreenInternal<TPresenter>(p => p.OpenView(model));
        }

        public UniTask<TPresenter> OpenScreen<TPresenter>() 
            where TPresenter : IUIPresenter
        {
            return this.OpenScreenInternal<TPresenter>(p => p.OpenViewAsync());
        }
        
        private async UniTask<TPresenter> OpenScreenInternal<TPresenter>(Func<TPresenter, UniTask> openAction) 
            where TPresenter : IUIPresenter
        {
            if (this.currentScreenActive != null)
            {
                LogService.LogWarning($"{this.currentScreenActive.GetType()} screen is already open.");
                await this.currentScreenActive.CloseViewAsync();
                this.currentScreenActive = null;
            }

            var newScreen = await this.GetScreen<TPresenter>();
            if (newScreen == null)
            {
                LogService.LogError($"{typeof(TPresenter).Name} screen doesn't exist");
                return default;
            }

            newScreen.SetViewParent(this.screenManagerHelper.UIScreenParent);
    
            if (openAction != null)
            {
                await openAction(newScreen);
            }
    
            this.currentScreenActive = newScreen;
            return newScreen;
        }

        #endregion

        #region PopupNoneOverlay

        public UniTask<TPresenter> OpenPopupNoneOverLay<TPresenter, TModel>(TModel model) 
            where TPresenter : IUIPresenter<TModel> where TModel : IUIModel
        {
            return this.OpenPopupInternal<TPresenter>(
                this.screenManagerHelper.UIPopupNonOverlayParent, 
                p => p.OpenView(model));
        }

        public UniTask<TPresenter> OpenPopupNoneOverLay<TPresenter>() 
            where TPresenter : IUIPresenter
        {
            return this.OpenPopupInternal<TPresenter>(
                this.screenManagerHelper.UIPopupNonOverlayParent, 
                p => p.OpenViewAsync());
        }
        
        private async UniTask<TPresenter> OpenPopupInternal<TPresenter>(RectTransform parent, Func<TPresenter, UniTask> openAction) 
            where TPresenter : IUIPresenter
        {
            if (this.IsPopupInQueue(typeof(TPresenter)))
            {
                LogService.LogWarning($"Popup {typeof(TPresenter).Name} is queued");
                return default;
            }

            var screen = await this.GetScreen<TPresenter>();
            if (screen == null)
            {
                LogService.LogError($"{typeof(TPresenter).Name} screen doesn't exist");
                return default;
            }

            screen.SetViewParent(parent);

            this.HandlePopupQueue(screen, () => openAction(screen));

            return screen;
        }
        
        private void HandlePopupQueue(IUIPresenter presenter, Func<UniTask> openAction)
        {
            if (this.currentPopupNonOverlayActive == null)
            {
                this.currentPopupNonOverlayActive = presenter;
                _                            = openAction.Invoke(); 
            }
            else
            {
                LogService.Log($"Add {presenter.GetType().Name} to popup queue.");
                this.popupQueue.Enqueue((presenter, openAction));
            }
        }
        
        private bool IsPopupInQueue(Type typeToCheck)
        {
            foreach (var item in this.popupQueue)
            {
                if (item.presenter.GetType() == typeToCheck) return true;
            }
            return false;
        }

        private void RemovePopupFromQueue(Type typeToRemove)
        {
            var tempKeepList = new List<(IUIPresenter, Func<UniTask>)>();

            while (this.popupQueue.Count > 0)
            {
                var item = popupQueue.Dequeue();
                if (item.presenter.GetType() != typeToRemove)
                {
                    tempKeepList.Add(item);
                }
            }

            foreach (var item in tempKeepList)
            {
                this.popupQueue.Enqueue(item);
            }
        }
        
        #endregion

        #region PopupOverlay
        
        public async UniTask<TPresenter> OpenPopupOverLay<TPresenter>() where TPresenter : IUIPresenter
        {
            return await this.OpenPopupOverlayInternal<TPresenter>(async (p) => await p.OpenViewAsync());
        }

        public async UniTask<TPresenter> OpenPopupOverLay<TPresenter, TModel>(TModel model)
            where TPresenter : IUIPresenter<TModel> where TModel : IUIModel
        {
            return await this.OpenPopupOverlayInternal<TPresenter>(async (p) => await ((TPresenter)p).OpenView(model));
        }
        
        private async UniTask<TPresenter> OpenPopupOverlayInternal<TPresenter>(Func<TPresenter, UniTask> openAction) 
            where TPresenter : IUIPresenter
        {
            var type = typeof(TPresenter);
    
            if (this.IsPopupInQueue(type))
            {
                this.RemovePopupFromQueue(type);
                LogService.Log($"Removed {type.Name} from NonOverlay Queue to open as Overlay.");
            }
    
            var screen = await this.GetScreen<TPresenter>();
            if (screen == null)
            {
                LogService.LogError($"{type.Name} screen doesn't exist");
                return default;
            }
    
            if (this.activeOverlays.Contains(screen))
            {
                return screen;
            }

            screen.SetViewParent(this.screenManagerHelper.UIPopupOverlayParent);

            this.activeOverlays.Add(screen);
    
            if (openAction != null)
            {
                await openAction(screen);
            }

            return screen;
        }
        #endregion

        #region CloseScreen

        private void OnOpenScreenUIHandler(OnOpenScreenUI signal)
        {
            
        }
        
        private void OnCloseScreenUIHandler(OnCloseScreenUI signal)
        {
            if (signal.UIPresenter == null)
                return;
               
            var closeScreenPresenter = signal.UIPresenter;

            if (this.activeOverlays.Contains(closeScreenPresenter))
            {
                this.activeOverlays.Remove(closeScreenPresenter);
            }
            else if (this.currentScreenActive == closeScreenPresenter)
            {
                this.currentScreenActive = null;
            }
            else if (this.currentPopupNonOverlayActive == closeScreenPresenter)
            {
                this.currentPopupNonOverlayActive = null;
            }
            
            closeScreenPresenter.SetViewParent(this.screenManagerHelper.UICloseParent);
            this.CheckAndOpenNextPopup();
        }
        
        private void CheckAndOpenNextPopup()
        {
            if (this.popupQueue.Count > 0)
            {
                var nextPopup = this.popupQueue.Dequeue();
                this.currentPopupNonOverlayActive = nextPopup.presenter;
                
                _ = nextPopup.openAction.Invoke(); 
            }
        }

        #endregion
        
        #region Helpers

        public bool IsAnyPopupOpen()
        {
            return this.currentPopupNonOverlayActive != null || this.activeOverlays.Count > 0;
        }

        public void CleanUpAllScreen()
        {
            this.currentScreenActive?.CloseViewImmediate();
            this.currentScreenActive = null;

            this.CloseAllPopups();
        }

        public void CloseAllPopups()
        {
            this.currentPopupNonOverlayActive?.CloseViewImmediate();
            this.currentPopupNonOverlayActive = null;

            this.popupQueue.Clear();

            foreach (var overlay in this.activeOverlays)
            {
                overlay.CloseViewImmediate();
            }
            this.activeOverlays.Clear();
        }

        public void ShowAllScreen()
        {
            this.currentScreenActive?.ShowView();
            this.ShowAllPopup();
        }

        public void HideAllScreen()
        {
            this.currentScreenActive?.HideView();
            this.HideAllPopup();
        }

        public void ShowAllPopup()
        {
            this.currentPopupNonOverlayActive?.ShowView();
            foreach (var overlay in this.activeOverlays)
            {
                overlay.ShowView();
            }
        }

        public void HideAllPopup()
        {
            this.currentPopupNonOverlayActive?.HideView();
            foreach (var overlay in this.activeOverlays)
            {
                overlay.HideView();
            }
        }

        public async UniTask<T> GetScreen<T>() where T : IUIPresenter
        {
            Type screenType = typeof(T);

            if (this.cachedScreenPresenterDic.TryGetValue(screenType, out IUIPresenter presenter))
            {
                return (T)presenter;
            }
            
            var screen = await this.InstantiateScreen(screenType);
            return (T)screen;
        }
        
        async UniTask<IUIPresenter> InstantiateScreen(Type type)
        {
            var screenPresenter = this.dependencyContainer.Instantiate(type);
            var screenInfo      = screenPresenter.GetCustomAttribute<ScreenInfoAttribute>();

            var prefab = await this.gameAssets.LoadAssetAsync<GameObject>(screenInfo.AddressableScreenName);
            var viewObject = Object.Instantiate(prefab, this.screenManagerHelper.UIScreenParent).GetComponent<IUIView>();

            ((IUIPresenter)screenPresenter).SetView(viewObject);
            this.cachedScreenPresenterDic.Add(type, (IUIPresenter)screenPresenter);

            return (IUIPresenter)screenPresenter;
        }

        #endregion
        
        public void Dispose()
        {
            /*this.CleanUpAllScreen();
            this.cachedScreenPresenterDic.Clear();
            this.popupQueue.Clear();
            this.activeOverlays.Clear();*/
        }
    }

}

