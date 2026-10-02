namespace VGameFoundation.Scripts.Services.LocalData
{
    using System;
    using System.Linq;
    using Cysharp.Threading.Tasks;
    using UnityEngine.Scripting;
    using VGameFoundation.Scripts.Utilities.Extension;
    using VGameFoundation.Scripts.Utilities.LogService;
    using VGameFoundation.Signals;

    public class UserDataManager
    {
        private readonly ISignalBus              signalBus;
        private readonly IHandleUserDataServices handleUserDataService;

        [Preserve]
        public UserDataManager(ISignalBus signalBus, IHandleUserDataServices handleUserDataService)
        {
            this.signalBus             = signalBus;
            this.handleUserDataService = handleUserDataService;
        }

        public async UniTask LoadUserData()
        {
            // The loading screen uses InitScreenManually, so BindData will be called in Awake. Therefore, we need to wait one frame to allow the other services to subscribe to UserDataLoadedSignal in Start.
            await UniTask.NextFrame();

            try
            {
                var types = ReflectionUtils.GetAllDerivedTypes<ILocalData>().ToArray();
                var datas = await this.handleUserDataService.Load(types);

                foreach (var data in datas)
                {
                    try
                    {
                        data.OnDataLoaded();
                    }
                    catch (Exception e)
                    {
                        LogService.Exception(e, $"OnDataLoaded failed on {data.GetType().Name}");
                    }
                }
            }
            catch (Exception e)
            {
                LogService.Exception(e, "LoadUserData failed");
            }
            finally
            {
                // The signal must fire no matter what, otherwise the loading screen never completes.
                this.signalBus.Fire<UserDataLoadedSignal>();
            }
        }
    }
}
