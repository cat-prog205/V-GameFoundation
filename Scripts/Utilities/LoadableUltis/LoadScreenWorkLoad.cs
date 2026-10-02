namespace Base.LoadModule
{
    using System;
    using Cysharp.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.ResourceManagement.AsyncOperations; // Cần cho AsyncOperationHandle
    using UnityEngine.ResourceManagement.ResourceProviders; // Cần cho SceneInstance
    using UnityEngine.SceneManagement;
    using VContainer;
    using VGameFoundation.Scripts.Services.GameAsset;
    using VGameFoundation.Scripts.Utilities.LogService;

    public class LoadScreenWorkLoad : MonoBehaviour, ILoadable
    {
        [SerializeField] private int           loadSceneIndex = 0;
        [SerializeField] private string[]      nameScene;
        [SerializeField] private LoadSceneMode loadSceneMode;

        [Inject] private IGameAssets gameAssets;

        private AsyncOperationHandle<SceneInstance> sceneHandle;
        private bool                                isLoadedInMemory = false;

        private void Reset() { this.gameObject.name = $"Task {this.GetType().Name}"; }

        public void Setup()
        {
            this.WorkLoad  = this.HandleLoadScene;
            this.IsLoaded  = () => this.isLoadedInMemory;
            this.IsTimeout = () => false;
        }

        private async void HandleLoadScene()
        {
            try
            {
                this.sceneHandle = this.gameAssets.LoadSceneAsync(
                    this.nameScene[this.loadSceneIndex],
                    this.loadSceneMode,
                    false
                );

                await this.sceneHandle.Task;

                this.isLoadedInMemory = true;
            }
            catch (Exception e)
            {
                LogService.LogError(e.Message);
            }
        }

        public float GetProgress()
        {
            if (!this.sceneHandle.IsValid()) return 0f;

            return this.sceneHandle.PercentComplete;
        }

        public void ActivateScene()
        {
            if (this.sceneHandle.IsValid() && this.sceneHandle.Status == AsyncOperationStatus.Succeeded)
                this.sceneHandle.Result.ActivateAsync();
            else
                LogService.LogError("Scene handle is invalid or not succeeded.");
        }

        public Action     WorkLoad     { get;       set; }
        public Func<bool> IsLoaded     { get;       set; }
        public Func<bool> IsTimeout    { get;       set; }
        public float      WorkEstimate { get => 10; set { } }
    }
}