using System.Threading;
using UnityEngine;
using UnityEngine.Audio;

namespace VGameFoundation.Script.SoundService
{
    using VGameFoundation.Scripts.Services.ObjectPooling;

    public class Sound : MonoBehaviour
    {
        private AudioSource             audioSource;
        private CancellationTokenSource tokenSource;

        public float Length => this.audioSource?.clip?.length ?? 0f;

        public CancellationToken Token => this.tokenSource?.Token ?? CancellationToken.None;

        private void Awake()
        {
            if (this.audioSource == null)
                this.audioSource = this.GetComponent<AudioSource>();
        }

        public void Init(AudioClip       clip,
                         AudioMixerGroup mixerGroup,
                         bool            loop,
                         bool            is3D        = false,
                         float           minDistance = 1f,
                         float           maxDistance = 20f)
        {
            if (this.audioSource == null)
                this.audioSource = this.GetComponent<AudioSource>();

            Debug.Log($"[SoundService] Init {clip.name}");
            Debug.Log($"[SoundService] Init {mixerGroup.name}");

            this.audioSource.clip                  = clip;
            this.audioSource.outputAudioMixerGroup = mixerGroup;
            this.audioSource.loop                  = loop;
            this.audioSource.spatialBlend          = is3D ? 1f : 0f;
            this.audioSource.minDistance           = minDistance;
            this.audioSource.maxDistance           = maxDistance;
            this.audioSource.playOnAwake           = false;

            this.tokenSource?.Cancel();
            this.tokenSource?.Dispose();
            this.tokenSource = new CancellationTokenSource();
        }

        public void Play() { this.audioSource?.Play(); }

        public void Stop()
        {
            this.audioSource?.Stop();
            this.CancelToken();
        }

        public void Despawn()
        {
            this.CancelToken();
            this.gameObject.Recycle();
        }

        private void CancelToken()
        {
            if (this.tokenSource != null && !this.tokenSource.IsCancellationRequested)
            {
                this.tokenSource.Cancel();
                this.tokenSource.Dispose();
                this.tokenSource = null;
            }
        }
    }
}