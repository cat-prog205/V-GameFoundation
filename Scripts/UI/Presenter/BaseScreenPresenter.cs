namespace VGameFoundation.Scripts.UI
{
     using Cysharp.Threading.Tasks;
     using UnityEngine;
     using VGameFoundation.Scripts.Utilities.LogService;
     using VGameFoundation.Signals;

     public abstract class BaseScreenPresenter<TView> : IUIPresenter
          where TView : IUIView
     {
          readonly ISignalBus signalBus;

          protected BaseScreenPresenter(ISignalBus signalBus) { this.signalBus = signalBus; }

          public TView View { get; private set; }

          public ScreenStatus ScreenStatus { get; private set; } = ScreenStatus.Closed;

          public async void SetView(IUIView viewInstance)
          {
               this.View = (TView)viewInstance;

               if (this.View.IsReadyToUse)
               {
                    this.OnViewReady();
               }
               else
               {
                    await UniTask.WaitUntil(() => this.View.IsReadyToUse);
                    this.OnViewReady();
               }
          }

          public abstract void OnViewReady();

          public void SetViewParent(Transform parent)
          {
               if (parent == null)
               {
                    LogService.LogError(parent.name + "is null");

                    return;
               }

               if (this.View == null) return;

               this.View.RectTransform.SetParent(parent);
          }

          public Transform GetViewParent() { return this.View.RectTransform.parent; }

          public abstract UniTask BindData();

          public virtual async UniTask OpenViewAsync()
          {
               await this.BindData();

               if (this.ScreenStatus == ScreenStatus.Opened)
                    return;

               this.ScreenStatus = ScreenStatus.Opened;
               _                 = View.Open();
               this.signalBus.Fire(new OnOpenScreenUI() { UIPresenter = this, });
          }

          public virtual async UniTask CloseViewAsync()
          {
               if (this.ScreenStatus == ScreenStatus.Closed)
                    return;

               this.ScreenStatus = ScreenStatus.Closed;
               await this.View.Close();
               this.signalBus.Fire(new OnCloseScreenUI() { UIPresenter = this });
               this.Dispose();
          }

          public virtual async void CloseView() { await this.CloseViewAsync(); }

          public virtual void CloseViewImmediate()
          {
               if (this.ScreenStatus == ScreenStatus.Closed)
                    return;

               this.ScreenStatus = ScreenStatus.Closed;
               this.View.CloseImmediate();
               this.signalBus.Fire(new OnCloseScreenUI() { UIPresenter = this });
               this.Dispose();
          }

          public void HideView()
          {
               if (this.ScreenStatus is ScreenStatus.Destroyed or ScreenStatus.Hide) return;

               this.ScreenStatus = ScreenStatus.Hide;
               this.View.Hide();
               this.Dispose();
          }

          public void ShowView()
          {
               this.ScreenStatus = ScreenStatus.Hide;
               this.View.Show();
               this.BindData();
          }

          public void DestroyView()
          {
               if (this.ScreenStatus is ScreenStatus.Destroyed or ScreenStatus.Hide)
                    return;

               this.ScreenStatus = ScreenStatus.Hide;
               this.View.DestroyView();
               this.Dispose();
          }

          /// <summary>
          /// call when close screen (use to unregister signal ,remove listener of button ...)
          /// </summary>
          public virtual void Dispose() { LogService.Log($"dispose screen {this.GetType()}"); }
     }

     public abstract class BaseScreenPresenter<TView, TModel> : BaseScreenPresenter<TView>,
          IUIPresenter<TModel>
          where TView : IUIView
          where TModel : IUIModel
     {
          protected TModel Model { get; set; }

          protected BaseScreenPresenter(ISignalBus signalBus) : base(signalBus) { }

          public async UniTask OpenView(TModel model)
          {
               if (model != null)
               {
                    this.Model = model;
                    await this.BindData(model);
               }

               await this.OpenViewAsync();
          }

          public sealed override UniTask BindData() { return UniTask.CompletedTask; }

          public abstract UniTask BindData(TModel screenModel);
     }
}
