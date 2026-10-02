namespace Base.LoadModule
{
    using System;
    using Cysharp.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.UI;

    public class LoadingSceneTemplate : MonoBehaviour
    {
        [SerializeField] private Slider slider;
        [SerializeField] private float  fakeDuration = 2.0f;

        public Action OnLoadingVisualComplete;

        private bool isDataReady = false;

        private void Start() { this.RunFakeProgress().Forget(); }

        public void SetDataCompleted() { this.isDataReady = true; }

        private async UniTaskVoid RunFakeProgress()
        {
            var timer      = 0f;
            var currentVal = 0f;

            while (currentVal < 0.9f)
            {
                timer      += Time.deltaTime;
                currentVal =  Mathf.MoveTowards(0, 0.9f, timer / this.fakeDuration);

                if (this.slider != null) this.slider.value = currentVal;
                await UniTask.Yield();
            }

            await UniTask.WaitUntil(() => this.isDataReady);

            while (currentVal < 1.0f)
            {
                currentVal = Mathf.MoveTowards(currentVal, 1.0f, 2.0f * Time.deltaTime); // Tốc độ nhanh (2.0)
                if (this.slider != null) this.slider.value = currentVal;
                await UniTask.Yield();
            }

            this.OnLoadingVisualComplete?.Invoke();
        }
    }
}