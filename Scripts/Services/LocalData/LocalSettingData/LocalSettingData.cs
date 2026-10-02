namespace VGameFoundation.Script.LocalData
{
    using System;
    using R3;
    using VGameFoundation.Scripts.Services.LocalData;

    [LocalDataKey("LocalSettingData")]
    public class LocalSettingData : BaseLocalData
    {
        public float                   Sensitivity;
        public ReactiveProperty<float> MusicVolume = new(1f);
        public ReactiveProperty<float> SoundVolume = new(1f);
        public bool                    IsVibrationEnabled;

        [NonSerialized] public Action<float> OnMusicVolumeChanged;
        [NonSerialized] public Action<float> OnSoundVolumeChanged;
        [NonSerialized] public Action<bool>  OnVibrationChanged;

        public override void Init()
        {
            this.Sensitivity        = 0.5f;
            this.IsVibrationEnabled = true;
            this.MusicVolume        = new ReactiveProperty<float>(1f);
            this.SoundVolume        = new ReactiveProperty<float>(1f);
        }

        // --- SOUND ---
        public void SetOnOfSound(bool enabled)
        {
            this.SetSoundVolume(enabled ? 1f : 0f);
        }

        public bool GetOnOfSound()
        {
            return this.SoundVolume.Value > 0;
        }

        public void SetSoundVolume(float value)
        {
            this.SoundVolume.Value = value;
            this.OnSoundVolumeChanged?.Invoke(value);
        }

        // --- MUSIC ---
        public void SetOnOfMusic(bool enabled)
        {
            this.SetMusicVolume(enabled ? 1f : 0f);
        }

        public bool GetOnOfMusic()
        {
            return this.MusicVolume.Value > 0;
        }

        public void SetMusicVolume(float value)
        {
            this.MusicVolume.Value = value;
            this.OnMusicVolumeChanged?.Invoke(value);
        }

        // --- VIBRATION ---
        public void SetVibration(int value)
        {
            this.SetVibration(value != 0);
        }

        public int GetVibration()
        {
            return this.IsVibrationEnabled ? 1 : 0;
        }

        public void SetVibration(bool enabled)
        {
            this.IsVibrationEnabled = enabled;
            this.OnVibrationChanged?.Invoke(this.IsVibrationEnabled);
        }

        public void ToggleVibration()
        {
            this.SetVibration(!this.IsVibrationEnabled);
        }
    }
}