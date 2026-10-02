namespace VGameFoundation.Script.Services.MobiCommon.Setting
{
    using Cysharp.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.UI;
    using VGameFoundation.Script.LocalData;
    using VGameFoundation.Scripts.UI;
    using VGameFoundation.Signals;
    
    //On Off
    public class UIPopupSettingSimple : UIPopupBaseView
    {
        [Header("Buttons")] public Button soundBtn;
        public                     Button musicBtn;
        public                     Button vibrateBtn;
        public                     Button closeBtn;

        [Header("Images")] public Image soundImageOn;
        public                    Image soundImageOff;
        public                    Image musicImageOn;
        public                    Image musicImageOff;
        public                    Image vibrateImageOn;
        public                    Image vibrateImageOff;
    }

    [PopupInfo(nameof(UIPopupSettingSimple))]
    public class UIPopupSettingSimplePresenter : BaseScreenPresenter<UIPopupSettingSimple>
    {
        private readonly ISignalBus   signalBus;
        private readonly LocalSettingData localSettingData;

        public UIPopupSettingSimplePresenter(ISignalBus   signalBus, LocalSettingData localSettingData) : base(signalBus)
        {
            this.signalBus        = signalBus;
            this.localSettingData = localSettingData;
        }

        public override void OnViewReady()
        {
            this.View.closeBtn.onClick.AddListener(this.CloseView);
            this.View.vibrateBtn.onClick.AddListener(this.OnClickVibrate);
            this.View.soundBtn.onClick.AddListener(this.OnSoundClick);
            this.View.musicBtn.onClick.AddListener(this.OnMusicClick);

            this.UpdateSoundUI();
            this.UpdateMusicUI();
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

        public override UniTask OpenViewAsync()
        {
            //GlobalTimeScale.Freeze();
            this.UpdateSoundUI();
            this.UpdateMusicUI();
            this.UpdateVibrateUI();

            return base.OpenViewAsync();
        }

        public override UniTask CloseViewAsync()
        {
            //GlobalTimeScale.Reset();

            return base.CloseViewAsync();
        }

        public override UniTask BindData()
        {
            this.UpdateSoundUI();
            this.UpdateMusicUI();

            return UniTask.CompletedTask;
        }
    }
}