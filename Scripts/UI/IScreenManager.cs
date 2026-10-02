namespace VGameFoundation.Scripts.UI
{
     using Cysharp.Threading.Tasks;

     public interface IScreenManager
     {
          UniTask<TPresenter> OpenScreen<TPresenter, TModel>(TModel model)
               where TPresenter : IUIPresenter<TModel>
               where TModel : IUIModel;

          UniTask<TPresenter> OpenScreen<TPresenter>()
               where TPresenter : IUIPresenter;
          
          UniTask<TPresenter> OpenPopupNoneOverLay<TPresenter, TModel>(TModel model)
                      where TPresenter : IUIPresenter<TModel>
                      where TModel : IUIModel;

          UniTask<TPresenter> OpenPopupNoneOverLay<TPresenter>()
               where TPresenter : IUIPresenter;
          
          UniTask<TPresenter> OpenPopupOverLay<TPresenter, TModel>(TModel model)
               where TPresenter : IUIPresenter<TModel>
               where TModel : IUIModel;

          UniTask<TPresenter> OpenPopupOverLay<TPresenter>()
               where TPresenter : IUIPresenter;
          
          /*public UniTask CloseScreen<TPresenter>()
               where TPresenter : IUIPresenter;*/

          bool IsAnyPopupOpen();
          void CleanUpAllScreen();
          
          void ShowAllScreen(){}
          void HideAllScreen(){}
          
          void ShowAllPopup() {}
          void HideAllPopup() {}
          
          /// <summary>
          /// Close all currently open popups (synchronous version)
          /// </summary>
          void CloseAllPopups();
     }
}
