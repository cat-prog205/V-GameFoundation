using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace VGameFoundation.Scripts.Services.TransitionService
{
    public class LoadingTransitionView : MonoBehaviour
    {
        [SerializeField] private GameObject loadingText;

        private Dictionary<Type, ITransitionModule> modules;
        private ITransitionModule                   activeModule;

        public void Initialize()
        {
            this.modules = new Dictionary<Type, ITransitionModule>();

            var foundModules = this.GetComponentsInChildren<TransitionModuleBase>(true);

            foreach (var module in foundModules)
            {
                module.Setup(this.gameObject);
                var type = module.GetType();
                this.modules.TryAdd(type, module);
            }

            if (this.loadingText) this.loadingText.SetActive(false);
        }

        public async UniTask PlayShow<T>(float duration, Action onComplete) where T : Component, ITransitionModule
        {
            var type = typeof(T);

            if (!this.modules.TryGetValue(type, out var targetModule)) return;

            if (this.activeModule != null && this.activeModule != targetModule) this.activeModule.ResetState();

            this.activeModule = targetModule;
            if (this.loadingText) this.loadingText.SetActive(true);

            await this.activeModule.Show(duration, onComplete);
        }

        public async UniTask PlayHide<T>(float duration, Action onComplete) where T : Component, ITransitionModule
        {
            if (this.loadingText) this.loadingText.SetActive(false);
            var type = typeof(T);

            if (!this.modules.TryGetValue(type, out var targetModule)) return;

            if (this.activeModule != null && this.activeModule != targetModule)
            {
                targetModule.SetVisualState(true);
                this.activeModule.ResetState();
                this.activeModule = targetModule;
            }

            var transitionModule = this.activeModule;
            if (transitionModule != null)
                await transitionModule.Hide(duration, () =>
                {
                    this.activeModule = null;
                    onComplete?.Invoke();
                });
        }
    }
}