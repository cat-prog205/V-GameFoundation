using System;
using Cysharp.Threading.Tasks;

namespace VGameFoundation.Script.Services.MobiCommon.RateUs
{
#if UNITY_IOS
    using UnityEngine.iOS;
#endif

#if UNITY_ANDROID
    using Google.Play.Review;
    using VGameFoundation.Scripts.Utilities.LogService;
#endif
    public static class RateUsService
    {
        public static async void RequestReviewsAsync(Action afterRateAction = null)
        {
#if UNITY_ANDROID
            LogService.Log($"==> RequestReviews-----1 <==");
            var reviewManager = new ReviewManager();

            var requestFlowOperation = reviewManager.RequestReviewFlow();
            LogService.Log($"==> RequestReviews-----2 <==");

            await requestFlowOperation.ToUniTask();

            LogService.Log($"==> RequestReviews-----3 <==");
            if (requestFlowOperation.Error != ReviewErrorCode.NoError)
            {
                LogService.LogError($"==> requestFlowOperation-----4-error: " + requestFlowOperation.Error.ToString() + " <==");

                return;
            }

            LogService.Log($"==> RequestReviews-----5 <==");
            var playReviewInfo = requestFlowOperation.GetResult();

            var launchFlowOperation = reviewManager.LaunchReviewFlow(playReviewInfo);
            LogService.Log($"==> RequestReviews-----6 <==");

            await launchFlowOperation.ToUniTask();

            LogService.Log($"==> RequestReviews-----7 <==");
            playReviewInfo = null; // Reset the object
            if (launchFlowOperation.Error != ReviewErrorCode.NoError)
            {
                LogService.LogError($"==> launchFlowOperation-----8-error: " + launchFlowOperation.Error.ToString() + " <==");

                return;
            }

            LogService.Log($"==> RequestReviews-----9 <==");
#endif
#if UNITY_IOS
            Device.RequestStoreReview();
#endif
            afterRateAction?.Invoke();
        }
    }
}