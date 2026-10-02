using Cysharp.Threading.Tasks;

using UnityEngine;
using UnityEngine.UI;
using VGameFoundation.Script.LocalData;
using VGameFoundation.Scripts.UI;
using VGameFoundation.Signals;

namespace VGameFoundation.Scripts.Services.MobiCommon.Settings
{
    public class UIPopupSetting : UIPopupBaseView
{
    public Slider sldSound;
    public Slider sldMusic;
    //public Slider sldSensitivity;

    [Header("Vibration")] public Button     btnVibration;
    public                       GameObject onVibration;
    public                       GameObject offVibration;

    //[Header("Language")] public Button nextButton;
    //public                      Button prevButton;
}

[PopupInfo(nameof(UIPopupSetting))]
public class UIPopupSettingPresenter : BaseScreenPresenter<UIPopupSetting>
{
    private readonly LocalSettingData localSettingData;

    public UIPopupSettingPresenter(ISignalBus signalBus, LocalSettingData localSettingData) : base(signalBus) { this.localSettingData = localSettingData; }

    public override void OnViewReady()
    {
        this.View.sldSound?.onValueChanged.AddListener(this.OnSoundChanged);
        /*this.View.nextButton.onClick.AddListener(this.OnNextLanguage);
        this.View.prevButton.onClick.AddListener(this.OnPrevLanguage);*/
        this.View.sldMusic?.onValueChanged.AddListener(this.OnMusicChanged);
        //this.View.sldSensitivity?.onValueChanged.AddListener(this.OnSensitivityChanged);
        this.View.btnVibration.onClick.AddListener(this.OnVibration);
    }

    public override UniTask BindData()
    {
        //Vibration
        this.View.onVibration.SetActive(this.localSettingData.IsVibrationEnabled);
        this.View.offVibration.SetActive(!this.localSettingData.IsVibrationEnabled);

        //Sound
        this.View.sldSound.value = this.localSettingData.SoundVolume.Value;

        //Music
        this.View.sldMusic.value = this.localSettingData.MusicVolume.Value;

        //Sensitivity
        //this.View.sldSensitivity.value = this.localSettingData.Sensitivity;
        
        return UniTask.CompletedTask;
    }

    private void OnVibration()
    {
        this.localSettingData.ToggleVibration();
        this.View.onVibration.SetActive(this.localSettingData.IsVibrationEnabled);
        this.View.offVibration.SetActive(!this.localSettingData.IsVibrationEnabled);
    }

    //private void OnSensitivityChanged(float arg0) { this.localSettingData.Sensitivity = arg0; }

    private void OnMusicChanged(float arg0) { this.localSettingData.SetMusicVolume(arg0); }

    private void OnSoundChanged(float arg0) { this.localSettingData.SetSoundVolume(arg0); }

    /*private void OnPrevLanguage() { this.ChangeLanguage(-1); }

    private void OnNextLanguage() { this.ChangeLanguage(1); }

    private void ChangeLanguage(int offset)
    {
        var index = -1;
        var langList = LocalizationService.Instance.ListLanguages();
        for (var i = 0; i < langList.Count; i++)
            if (langList[i] == LocalizationService.Instance.CurrentLanguage)
            {
                index = i + offset;

                break;
            }

        if (index < 0)
            index = langList.Count - 1;
        else if (index >= langList.Count)
            index = 0;

        LocalizationService.Instance.ChangeLanguage(langList[index]);
    }*/
}
}

