namespace Base.LoadModule
{
    using System;
    using BlueprintFlow.BlueprintControlFlow;
    using Cysharp.Threading.Tasks;
    using UnityEngine;
    using VContainer;
    using VGameFoundation.Scripts.Utilities.LogService;

    public class BluePrintWorkLoad : MonoBehaviour,
        ILoadable
    {
        [Inject] private BlueprintReaderManager blueprintReaderManager;
        private          bool                   isLoaded;

        private void Reset() { this.gameObject.name = $"Task {this.GetType().Name}"; }

        public void Setup()
        {
            this.WorkLoad  = this.HandleLoadBlueprint;
            this.IsLoaded  = () => this.isLoaded;
            this.IsTimeout = () => false;
        }

        private async void HandleLoadBlueprint()
        {
            try
            {
                await this.blueprintReaderManager.LoadBlueprint();
                await UniTask.Delay(TimeSpan.FromSeconds(2));
            }
            catch (Exception e)
            {
                //
                LogService.LogError("error on load blue print ");
                LogService.LogError(e.Message);
            }

            this.isLoaded = true;
        }

        public Action     WorkLoad     { get;      set; }
        public Func<bool> IsLoaded     { get;      set; }
        public Func<bool> IsTimeout    { get;      set; }
        public float      WorkEstimate { get => 1; set { } }

        public float GetProgress() { return this.isLoaded ? 1f : 0f; }
    }
}