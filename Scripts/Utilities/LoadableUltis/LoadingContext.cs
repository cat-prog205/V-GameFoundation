namespace Base
{
    using System.Linq;
    using Base.LoadModule;
    using Cysharp.Threading.Tasks;
    using UnityEngine;
    using VGameFoundation.DI;
    using VGameFoundation.Scripts.Utilities.LogService;

    public class LoadingContext : SceneScope
    {
        [SerializeField] private LoadingSceneTemplate uiTemplate;

        private LoadScreenWorkLoad sceneTask;

        private async void Start()
        {
            if (this.uiTemplate != null) this.uiTemplate.OnLoadingVisualComplete = this.OnVisualDone;

            var allTasks = this.GetComponentsInChildren<ILoadable>();
            this.sceneTask = allTasks.OfType<LoadScreenWorkLoad>().FirstOrDefault();

            foreach (var task in allTasks)
            {
                task.Setup();
                task.WorkLoad?.Invoke();
            }

            await UniTask.WaitUntil(() => allTasks.All(t => t.IsLoaded()));

            LogService.Log("Data Loaded. Waiting for UI animation...");

            if (this.uiTemplate != null)
                this.uiTemplate.SetDataCompleted();
            else
                this.OnVisualDone();
        }

        private void OnVisualDone()
        {
            LogService.Log("All Complete. Activating Scene...");

            if (this.sceneTask != null) this.sceneTask.ActivateScene();
        }
    }
}