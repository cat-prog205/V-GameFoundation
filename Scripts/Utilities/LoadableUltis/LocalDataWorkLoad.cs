namespace Base.LoadModule
{
    using System;
    using Cysharp.Threading.Tasks;
    using UnityEngine;
    using VContainer;
    using VGameFoundation.Scripts.Services.LocalData;

    public class LocalDataWorkLoad : MonoBehaviour,
        ILoadable
    {
        [Inject] private UserDataManager userDataManager;

        [SerializeField] private int   workEstimate;
        [SerializeField] private float timeoutSeconds = 30f;

        private void Reset() { this.gameObject.name = $"Task {this.GetType().Name}"; }

        private bool  isDone;
        private float startTime;

        public void Setup()
        {
            this.WorkLoad  = this.LoadLocalData;
            this.IsLoaded  = () => this.isDone;
            this.IsTimeout = () => !this.isDone && Time.realtimeSinceStartup - this.startTime > this.timeoutSeconds;
        }

        private async void LoadLocalData()
        {
            this.startTime = Time.realtimeSinceStartup;

            try
            {
                await this.userDataManager.LoadUserData();
                await UniTask.Delay(TimeSpan.FromSeconds(2));
            }
            finally
            {
                // Never leave the loading screen waiting, even if loading blew up.
                this.isDone = true;
            }
        }

        public Action WorkLoad { get; set; }

        public Func<bool> IsLoaded     { get;                      set; }
        public Func<bool> IsTimeout    { get;                      set; }
        public float      WorkEstimate { get => this.workEstimate; set { } }

        public float GetProgress()
        {
            if (this.isDone) return 1;

            return 0;
        }
    }
}