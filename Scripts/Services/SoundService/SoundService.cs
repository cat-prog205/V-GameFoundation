using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Audio;
using VContainer.Unity;

namespace VGameFoundation.Script.SoundService
{
    using VGameFoundation.Script.LocalData;
    using VGameFoundation.Scripts.Services.GameAsset;
    using VGameFoundation.Scripts.Services.ObjectPooling;
    using VGameFoundation.Scripts.Utilities.LogService;
    using VGameFoundation.Signals;

    public interface ISoundService
    {
        // 2D
        void PlaySFX2D(string soundName, bool loop = false);

        // 3D - Destroy -> DespawnObject
        void PlaySFX3D(string soundName, Transform parent, bool loop = false, float minDistance = 1f, float maxDistance = 20f);

        void PlayRandomSFX2D(string[] soundNames, bool loop = false);

        void PlayRandomSFX3D(string[] soundNames, Transform parent, bool loop = false, float minDistance = 1f, float maxDistance = 20f);

        UniTask PlayMusic(string musicName, bool isLoop = true, Transform parent = null, Action onCompleted = null);

        void StopSound(string soundName);

        void StopMusic(string musicName);

        void StopAllMusic();

        void StopAllSounds();

        UniTask<float> GetLength(string soundName, bool isMusic = false);
    }

    public class SoundService : ISoundService, IInitializable
    {
        private readonly IGameAssets      gameAssets;
        private readonly LocalSettingData localSettingData;
        private readonly ISignalBus       signalBus;

        private const string MIXER_PATH  = "SoundManagerAudioMixer";
        private const string PREFAB_PATH = "SoundPrefab";

        private AudioMixer      audioMixer;
        private AudioMixerGroup musicGroup;
        private AudioMixerGroup soundGroup;
        private Sound           soundPrefab;

        private readonly Dictionary<string, AudioClip>   sfxClips    = new();
        private readonly Dictionary<string, AudioClip>   musicClips  = new();
        private readonly List<(string name, Sound inst)> activeSfx   = new();
        private readonly List<(string name, Sound inst)> activeMusic = new();

        public event Action<string> OnSoundPlayed;

        private SoundService(IGameAssets    gameAssets, LocalSettingData localSettingData, ISignalBus signalBus)
        {
            this.gameAssets       = gameAssets;
            this.localSettingData = localSettingData;
            this.signalBus    = signalBus;
        }

        public void Initialize()
        {
            LogService.Log("Initializing sound service");
            
            // 🔹 Load AudioMixer
            this.audioMixer = Resources.Load<AudioMixer>(MIXER_PATH);
            if (this.audioMixer == null)
            {
                LogService.LogError($"[SoundService] ❌ Không tìm thấy AudioMixer ở path: {MIXER_PATH}");

                return;
            }
            
            // 🔹 Tìm Music & Sound group
            var musicGroups = this.audioMixer.FindMatchingGroups("Master/Music");
            var soundGroups = this.audioMixer.FindMatchingGroups("Master/Sound");
            

            if (musicGroups.Length > 0)
                this.musicGroup = musicGroups[0];
            else
                LogService.LogWarning("[SoundService] ⚠ Không tìm thấy nhóm Master/Music trong AudioMixer!");

            if (soundGroups.Length > 0)
                this.soundGroup = soundGroups[0];
            else
                LogService.LogWarning("[SoundService] ⚠ Không tìm thấy nhóm Master/Sound trong AudioMixer!");

            // 🔹 Load Sound Prefab
            if (this.soundPrefab == null)
            {
                var go = Resources.Load<GameObject>(PREFAB_PATH);
                if (go == null)
                {
                    LogService.LogError($"[SoundService] ❌ Không tìm thấy Sound Prefab ở path: {PREFAB_PATH}");

                    return;
                }

                this.soundPrefab = go.GetComponent<Sound>();
                LogService.Log($"[SoundService] ✅ Sound prefab loaded: {this.soundPrefab.name}");
            }

            this.audioMixer.GetFloat("MasterVolume", out var masterDB);
            this.audioMixer.GetFloat("MusicVolume",  out var musicDB);
            this.audioMixer.GetFloat("SoundVolume",  out var soundDB);

            LogService.Log($"[SoundService] 📊 Mixer current values → Master:{masterDB} dB | Music:{musicDB} dB | Sound:{soundDB} dB");

            this.signalBus.Subscribe<UserDataLoadedSignal>(() =>
            {
                this.SetMixerVolume("MusicVolume", this.localSettingData.MusicVolume.Value);
                this.SetMixerVolume("SoundVolume", this.localSettingData.SoundVolume.Value);

                this.localSettingData.OnMusicVolumeChanged += f => { this.SetMixerVolume("MusicVolume", this.localSettingData.MusicVolume.Value); };
                this.localSettingData.OnSoundVolumeChanged += f => { this.SetMixerVolume("SoundVolume", this.localSettingData.SoundVolume.Value); };
            });
        }

        private void SetMixerVolume(string parameter, float volume)
        {
            if (this.audioMixer == null) return;

            var dB = volume > 0 ? 20f * Mathf.Log10(volume) : -80f;
            this.audioMixer.SetFloat(parameter, dB);
        }

        // ==============================================================
        // Play
        // ==============================================================
        public void PlaySFX2D(string soundName, bool loop = false) { this.PlaySFXInternal(soundName, null, loop, false).Forget(); }

        public void PlaySFX3D(string soundName,        Transform parent, bool loop = false,
                              float  minDistance = 1f, float     maxDistance = 20f)
        {
            this.PlaySFXInternal(soundName, parent, loop, true, minDistance, maxDistance).Forget();
        }

        public void PlayRandomSFX2D(string[] soundNames, bool loop = false)
        {
            if (soundNames == null || soundNames.Length == 0) return;
            var random = UnityEngine.Random.Range(0, soundNames.Length);
            this.PlaySFX2D(soundNames[random], loop);
        }

        public void PlayRandomSFX3D(string[] soundNames,       Transform parent, bool loop = false,
                                    float    minDistance = 1f, float     maxDistance = 20f)
        {
            if (soundNames == null || soundNames.Length == 0) return;
            var random = UnityEngine.Random.Range(0, soundNames.Length);
            this.PlaySFX3D(soundNames[random], parent, loop, minDistance, maxDistance);
        }

        public async UniTask PlayMusic(string musicName, bool isLoop = true, Transform parent = null, Action onCompleted = null)
        {
            if (string.IsNullOrEmpty(musicName))
            {
                Debug.LogError("[SoundService] Empty music name.");

                return;
            }

            if (!this.musicClips.TryGetValue(musicName, out var clip))
            {
                clip = await this.gameAssets.LoadAssetAsync<AudioClip>(musicName);
                if (clip == null)
                {
                    Debug.LogError($"[SoundService] ❌ Music not found: {musicName}");

                    return;
                }

                this.musicClips[musicName] = clip;
            }

            var inst = this.soundPrefab.Spawn();
            if (parent != null)
            {
                inst.transform.SetParent(parent, false);
                inst.transform.localPosition = Vector3.zero;
            }

            Debug.Log($"[SoundService] Play {musicName}");

            inst.Init(clip, this.musicGroup, isLoop, false);
            inst.Play();

            this.activeMusic.Add((musicName, inst));

            if (!isLoop)
            {
                await UniTask.WaitForSeconds(
                    inst.Length,
                    true,
                    cancellationToken: inst.Token
                );

                if (inst != null)
                {
                    inst.Stop();
                    inst.Despawn();
                    this.activeMusic.RemoveAll(x => x.inst == inst);
                }

                onCompleted?.Invoke();
            }
        }

        // ==============================================================
        // Stop
        // ==============================================================
        public void StopSound(string soundName)
        {
            for (var i = this.activeSfx.Count - 1; i >= 0; i--)
            {
                var (name, sound) = this.activeSfx[i];
                if (name == soundName)
                {
                    sound.Stop();
                    sound.Despawn();
                    this.activeSfx.RemoveAt(i);
                }
            }
        }

        public void StopMusic(string musicName)
        {
            for (var i = this.activeMusic.Count - 1; i >= 0; i--)
            {
                var (name, sound) = this.activeMusic[i];
                if (name == musicName)
                {
                    sound.Stop();
                    sound.Despawn();
                    this.activeMusic.RemoveAt(i);
                }
            }
        }

        public void StopAllMusic()
        {
            foreach (var (_, sound) in this.activeMusic)
            {
                sound.Stop();
                sound.Despawn();
            }

            this.activeMusic.Clear();
        }

        public void StopAllSounds()
        {
            foreach (var (_, sound) in this.activeSfx)
            {
                sound.Stop();
                sound.Despawn();
            }

            this.activeSfx.Clear();
        }

        // ==============================================================
        // Internal Helper
        // ==============================================================
        private async UniTask PlaySFXInternal(string soundName,        Transform parent, bool loop, bool is3D,
                                              float  minDistance = 1f, float     maxDistance = 20f)
        {
            if (string.IsNullOrEmpty(soundName))
            {
                Debug.LogError("[SoundService] Empty SFX name.");

                return;
            }

            if (!this.sfxClips.TryGetValue(soundName, out var clip))
            {
                clip = await this.gameAssets.LoadAssetAsync<AudioClip>(soundName);
                if (clip == null)
                {
                    Debug.LogError($"[SoundService] ❌ SFX not found: {soundName}");

                    return;
                }

                this.sfxClips[soundName] = clip;
            }

            var inst = this.soundPrefab.Spawn();

            if (parent != null)
            {
                inst.transform.SetParent(parent, false);
                inst.transform.localPosition = Vector3.zero;
            }

            inst.Init(clip, this.soundGroup, loop, is3D, minDistance, maxDistance);
            inst.Play();

            this.OnSoundPlayed?.Invoke(soundName);
            this.activeSfx.Add((soundName, inst));

            if (!loop) this.AutoReturn(inst).Forget();
        }

        private async UniTask AutoReturn(Sound inst)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(inst.Length), true);
            if (inst != null)
            {
                inst.Stop();
                inst.Despawn();
            }

            this.activeSfx.RemoveAll(x => x.inst == inst);
        }

        public async UniTask<float> GetLength(string soundName, bool isMusic = false)
        {
            if (string.IsNullOrEmpty(soundName))
            {
                Debug.LogError("[SoundService] Empty sound name.");

                return 0f;
            }

            AudioClip clip = null;

            if (isMusic)
            {
                if (!this.musicClips.TryGetValue(soundName, out clip))
                {
                    clip = await this.gameAssets.LoadAssetAsync<AudioClip>(soundName);
                    if (clip == null)
                    {
                        Debug.LogError($"[SoundService] ❌ Music not found: {soundName}");

                        return 0f;
                    }

                    this.musicClips[soundName] = clip;
                }
            }
            else
            {
                if (!this.sfxClips.TryGetValue(soundName, out clip))
                {
                    clip = await this.gameAssets.LoadAssetAsync<AudioClip>(soundName);
                    if (clip == null)
                    {
                        Debug.LogError($"[SoundService] ❌ SFX not found: {soundName}");

                        return 0f;
                    }

                    this.sfxClips[soundName] = clip;
                }
            }

            return clip.length;
        }
    }
}