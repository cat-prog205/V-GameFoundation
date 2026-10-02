namespace VGameFoundation.Script.Services.MobiCommon.Setting
{
    using Cysharp.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.UI;
    using VGameFoundation.Script.LocalData;
    using VGameFoundation.Scripts.UI;
    using VGameFoundation.Signals;

    public class UIPopupPause : UIPopupBaseView
    {
        [Header("Buttons")] public Button soundBtn;
        public                     Button musicBtn;
        public                     Button vibrateBtn;
        public                     Button closeBtn;
        public                     Button continueBtn;
        public                     Button homeBtn;

        [Header("Images")] public Image soundImageOn;
        public                    Image soundImageOff;
        public                    Image musicImageOn;
        public                    Image musicImageOff;
        public                    Image vibrateImageOn;
        public                    Image vibrateImageOff;
    }

    [PopupInfo(nameof(UIPopupPause))]
    public class UIPopupPausePresenter : BaseScreenPresenter<UIPopupPause>
    {
        private readonly LocalSettingData localSettingData;

        public UIPopupPausePresenter(ISignalBus signalBus, LocalSettingData localSettingData) : base(signalBus) { this.localSettingData = localSettingData; }

        public override void OnViewReady()
        {
            this.View.homeBtn.onClick.AddListener(this.OnHomeClick);
            this.View.closeBtn.onClick.AddListener(this.CloseView);
            this.View.continueBtn.onClick.AddListener(this.CloseView);
            this.View.vibrateBtn.onClick.AddListener(this.OnClickVibrate);
            this.View.soundBtn.onClick.AddListener(this.OnSoundClick);
            this.View.musicBtn.onClick.AddListener(this.OnMusicClick);
        }

        private void OnSoundClick()
        {
            var isOn = !this.localSettingData.GetOnOfSound();
            this.localSettingData.SetOnOfSound(isOn);
            this.UpdateSoundUI();
        }

        private void OnMusicClick()
        {
            var isOn = !this.localSettingData.GetOnOfMusic();
            this.localSettingData.SetOnOfSound(isOn);
            this.UpdateMusicUI();
        }

        private void UpdateSoundUI()
        {
            var isOn = this.localSettingData.GetOnOfSound();
            this.View.soundImageOn.gameObject.SetActive(isOn);
            this.View.soundImageOff.gameObject.SetActive(!isOn);
        }

        private void UpdateMusicUI()
        {
            var isOn = this.localSettingData.GetOnOfMusic();
            this.View.musicImageOn.gameObject.SetActive(isOn);
            this.View.musicImageOff.gameObject.SetActive(!isOn);
        }

        private void UpdateVibrateUI()
        {
            var isOn = this.localSettingData.IsVibrationEnabled;
            this.View.vibrateImageOn.gameObject.SetActive(!isOn);
            this.View.vibrateImageOff.gameObject.SetActive(isOn);
        }

        private void OnClickVibrate()
        {
            var isOn = !this.localSettingData.IsVibrationEnabled;
            this.localSettingData.SetVibration(isOn);
            this.View.vibrateImageOn.gameObject.SetActive(!isOn);
            this.View.vibrateImageOff.gameObject.SetActive(isOn);
        }

        /*public override UniTask OpenViewAsync()
        {
            TimeScaleUtilities.Freeze();
            this.UpdateSoundUI();
            this.UpdateMusicUI();
            this.UpdateVibrateUI();

            return base.OpenViewAsync();
        }

        public override UniTask CloseViewAsync()
        {
            TimeScaleUtilities.Reset();

            return base.CloseViewAsync();
        }*/

        public override UniTask BindData()
        {
            this.UpdateSoundUI();
            this.UpdateMusicUI();
            this.UpdateVibrateUI();

            return UniTask.CompletedTask;
        }

        private void OnHomeClick()
        {
            this.CloseView();
            //this.gameFlowService.BackHome();
        }
    }
}