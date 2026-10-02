using Cysharp.Threading.Tasks;
using UnityEngine.UI;

namespace VGameFoundation.Script.Services.MobiCommon.NoInternet
{
    using VGameFoundation.Scripts.UI;
    using VGameFoundation.Signals;

    public class UIPopupReconnect : UIPopupBaseView
    {
        public Button reconnectButton;
    }

    [PopupInfo(nameof(UIPopupReconnect))]
    public class UIPopupReconnectPresenter : BaseScreenPresenter<UIPopupReconnect>
    {
        private readonly ISignalBus       signalBus;
        private readonly InternetCheckService internetService;

        public UIPopupReconnectPresenter(ISignalBus signalBus,
            InternetCheckService internetService) : base(signalBus)
        {
            this.signalBus       = signalBus;
            this.internetService = internetService;
        }

        public override void OnViewReady()
        {
            this.View.reconnectButton.onClick.AddListener(this.Reconnect);
        }

        public override UniTask BindData()
        {
            //GlobalTimeScale.Freeze();
            return UniTask.CompletedTask;
        }

        private void Reconnect()
        {
            if (this.internetService.HasInternetConnection)
            {
                //GlobalTimeScale.Reset();
                this.CloseView();
            }
        }
    }
}