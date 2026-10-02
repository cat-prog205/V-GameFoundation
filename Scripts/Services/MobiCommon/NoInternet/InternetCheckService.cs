using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.Networking;
using VContainer.Unity;
using VGameFoundation.Signals;

namespace VGameFoundation.Script.Services.MobiCommon.NoInternet
{
    public class InternetCheckService : IInitializable, IDisposable
    {
        private const int CheckInterval = 3;
        
        private readonly ISignalBus              signal;
        private readonly CompositeDisposable     disposables = new();
        private readonly CancellationTokenSource cts         = new();
        private readonly ReactiveProperty<bool> internetState = new(true);
        public bool HasInternetConnection => this.internetState.Value;

        public InternetCheckService(ISignalBus signal)
        {
            this.signal = signal;
        }

        public void Initialize()
        {
            this.CheckConnectionLoop(this.cts.Token).Forget();

            this.internetState
                .DistinctUntilChanged()
                .Subscribe(state =>
                {
                    this.signal.Fire(new OnInternetStateChangeSignal { HaveInternet = state });
                })
                .AddTo(this.disposables);
        }

        private async UniTaskVoid CheckConnectionLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                this.internetState.Value = await this.IsNetworkAvailable(token);
                await UniTask.Delay(TimeSpan.FromSeconds(CheckInterval), DelayType.UnscaledDeltaTime, cancellationToken: token);
            }
        }

        private async UniTask<bool> IsNetworkAvailable(CancellationToken token)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
                return false;

            try
            {
                using var request = UnityWebRequest.Head("https://www.google.com");
                request.timeout = 3;
                await request.SendWebRequest().WithCancellation(token);
                return request.result == UnityWebRequest.Result.Success;
            }
            catch
            {
                return false;
            }
        }

        public void Dispose()
        {
            this.cts.Cancel();
            this.cts.Dispose();
            this.disposables.Dispose();
        }
    }
}

