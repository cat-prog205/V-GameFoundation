namespace VGameFoundation.Scripts.Services.LoadSceneService
{
    using System;
    using Cysharp.Threading.Tasks;
    using UnityEngine.ResourceManagement.AsyncOperations;
    using UnityEngine.SceneManagement;
    using VGameFoundation.Scripts.Services.GameAsset;
    using VGameFoundation.Scripts.Utilities.LogService;

    public interface ILoadSceneService
    {
        event Action<float>  OnProgressChanged;
        event Action<string> OnLoadingStarted;
        event Action<string> OnLoadingFinished;

        UniTask LoadSceneAsync(
            object sceneKey,
            Action onSuccess = null,
            Action<string> onFail = null,
            LoadSceneMode loadMode = LoadSceneMode.Single,
            bool activateOnLoad = true
        );
    }

    public class LoadSceneService : ILoadSceneService
    {
        private readonly IGameAssets gameAssets;

        public event Action<float>  OnProgressChanged;
        public event Action<string> OnLoadingStarted;
        public event Action<string> OnLoadingFinished;

        public LoadSceneService(IGameAssets gameAssets) { this.gameAssets = gameAssets; }

        public async UniTask LoadSceneAsync(
            object sceneKey,
            Action onSuccess = null,
            Action<string> onFail = null,
            LoadSceneMode loadMode = LoadSceneMode.Single,
            bool activateOnLoad = true)
        {
            this.OnLoadingStarted?.Invoke(sceneKey.ToString());
            this.OnProgressChanged?.Invoke(0f);

            try
            {
                var handle = this.gameAssets.LoadSceneAsync(sceneKey, loadMode, activateOnLoad);

                while (!handle.IsDone)
                {
                    this.OnProgressChanged?.Invoke(handle.PercentComplete);
                    await UniTask.Yield(PlayerLoopTiming.Update);
                }

                this.OnProgressChanged?.Invoke(1f);

                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    LogService.Log($"[LoadSceneService] Load scene '{sceneKey}' success.");

                    onSuccess?.Invoke();
                }
                else
                {
                    var errorMsg = handle.OperationException != null
                        ? handle.OperationException.Message
                        : "Unknown error from Addressables";

                    LogService.LogError($"[LoadSceneService] Load scene '{sceneKey}' failed: {errorMsg}");

                    onFail?.Invoke(errorMsg);
                }
            }
            catch (Exception ex)
            {
                LogService.LogError($"[LoadSceneService] Exception: {ex.Message}");
                onFail?.Invoke(ex.Message);
            }
            finally
            {
                this.OnLoadingFinished?.Invoke(sceneKey.ToString());
            }
        }
    }
}