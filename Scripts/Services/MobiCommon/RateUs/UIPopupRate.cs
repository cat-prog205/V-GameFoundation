using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using System;
using VGameFoundation.Scripts.Services.GameAsset;
using VGameFoundation.Scripts.UI;
using VGameFoundation.Signals;

namespace VGameFoundation.Script.Services.MobiCommon.RateUs
{
    public class UIPopupRate : UIPopupBaseView
    {
        public Button[] rateButtons;
        public string   rateSpriteOnAddressName;
        public string   rateSpriteOffAddressName;

        private void OnValidate() { this.rateButtons = this.GetComponentsInChildren<Button>(true); }
    }

    [PopupInfo(nameof(UIPopupRate))]
    public class UIPopupRatePresenter : BaseScreenPresenter<UIPopupRate>
    {
        private const int DefaultRate = 3;
        
        private readonly IGameAssets gameAssets;
        
        private Button curButton;
        private Sprite rateSpriteOn;
        private Sprite rateSpriteOff;
        
        public UIPopupRatePresenter(ISignalBus signalBus, IGameAssets gameAssets) : base(signalBus) { this.gameAssets = gameAssets; }
        public override void OnViewReady()
        {
            foreach (var viewRateButton in this.View.rateButtons)
                viewRateButton.onClick.AddListener(() => OnClickRate(viewRateButton));

        }

        public override async UniTask BindData()
        {
            this.rateSpriteOn = await this.gameAssets.LoadAssetAsync<Sprite>(this.View.rateSpriteOnAddressName);
            this.rateSpriteOff = await this.gameAssets.LoadAssetAsync<Sprite>(this.View.rateSpriteOffAddressName);
            this.UpdateSprite(this.View.rateButtons[DefaultRate]);
        }

        /*public override UniTask OpenViewAsync()
        {
            TimeScaleUtilities.Freeze();
            return base.OpenViewAsync();
        }

        public override UniTask CloseViewAsync()
        {
            TimeScaleUtilities.Reset();
            return base.CloseViewAsync();
        }*/

        void OnClickRate(Button button)
        {
            this.curButton = button;
            this.UpdateSprite(this.curButton);
            RateUsService.RequestReviewsAsync(this.CloseView);
        }

        void UpdateSprite(Button button)
        {
            int index = Array.IndexOf(this.View.rateButtons, button);
            for (var i = 0; i < this.View.rateButtons.Length; i++)
            {
                this.View.rateButtons[i].GetComponent<Image>().sprite =
                    i <= index ? this.rateSpriteOn : this.rateSpriteOff;
            }
        }
    }
}