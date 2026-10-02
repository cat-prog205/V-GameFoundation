using UnityEngine;

// Conditional compilation để tránh lỗi khi NiceVibrations chưa được import
#if LOFELT_NICEVIBRATIONS
using Lofelt.NiceVibrations;
#endif

namespace VGameFoundation.Script.Services.MobiCommon.Vibration
{
    using VContainer.Unity;
    using VGameFoundation.Script.LocalData;
    using VGameFoundation.Scripts.Utilities.LogService;
    using VGameFoundation.Signals;

    /// <summary>
    /// VibrationService service wrapper for NiceVibrations package
    /// Provides easy-to-use vibration and haptic feedback functionality
    /// Updated for latest Nice Vibrations API
    /// </summary>
    public class VibrationService : IVibrationService, IInitializable
    {
        private readonly LocalSettingData localSettingData;
        private readonly ISignalBus       signalBus;

        //Vibration Settings
        private bool vibrationEnabled = true;
        private bool debugMode = false;

        //Default Settings
        private const float _defaultIntensity = 0.8f;
        private const float _defaultSharpness = 0.8f;
        private const float _defaultDuration  = 0.1f;

        public VibrationService(LocalSettingData localSettingData, ISignalBus signalBus)
        {
            this.localSettingData = localSettingData;
            this.signalBus        = signalBus;
        }

        #region Unity Lifecycle
        
        public void Initialize()
        {
            this.InitializeVibrationManager();
            
            this.signalBus.Subscribe<UserDataLoadedSignal>(() =>
            {
                this.SetVibrationEnabled(this.localSettingData.IsVibrationEnabled);
            });

            this.localSettingData.OnVibrationChanged += this.SetVibrationEnabled;
        }

        #endregion

        #region Initialization

        private void InitializeVibrationManager()
        {
#if LOFELT_NICEVIBRATIONS
            // Set initial haptics state
            HapticController.hapticsEnabled = this.vibrationEnabled;
            
            if (this.debugMode)
            {
                LogService.Log($"[VibrationService] Initialized - Haptics Enabled: {HapticController.hapticsEnabled}");
                LogService.Log($"[VibrationService] Device Capabilities: {DeviceCapabilities.isVersionSupported}");
                LogService.Log($"[VibrationService] Advanced Requirements: {DeviceCapabilities.meetsAdvancedRequirements}");
            }
#else
            if (_debugMode)
            {
                Debug.Log("[VibrationService] NiceVibrations not available. Please import the package.");
            }
#endif
        }

        #endregion

        #region Public API - Basic Vibration

        /// <summary>
        /// Triggers a simple vibration using medium impact
        /// </summary>
        public void Vibrate()
        {
            if (!this.vibrationEnabled) return;
            
#if LOFELT_NICEVIBRATIONS
            HapticPatterns.PlayPreset(HapticPatterns.PresetType.MediumImpact);
#else
            // Fallback to basic vibration
            Handheld.Vibrate();
#endif
        }

        /// <summary>
        /// Triggers a haptic feedback with predefined type
        /// </summary>
        /// <param name="type">The type of haptic feedback</param>
        public void Haptic(int type)
        {
            if (!this.vibrationEnabled) return;
            
#if LOFELT_NICEVIBRATIONS
            var presetType = (HapticPatterns.PresetType)type;
            HapticPatterns.PlayPreset(presetType);
#else
            // Fallback to basic vibration
            Handheld.Vibrate();
#endif
        }

        /// <summary>
        /// Triggers a selection haptic feedback
        /// </summary>
        public void Selection()
        {
#if LOFELT_NICEVIBRATIONS
            Haptic((int)HapticPatterns.PresetType.Selection);
#else
            Vibrate();
#endif
        }

        /// <summary>
        /// Triggers a success haptic feedback
        /// </summary>
        public void Success()
        {
#if LOFELT_NICEVIBRATIONS
            Haptic((int)HapticPatterns.PresetType.Success);
#else
            Vibrate();
#endif
        }

        /// <summary>
        /// Triggers a warning haptic feedback
        /// </summary>
        public void Warning()
        {
#if LOFELT_NICEVIBRATIONS
            Haptic((int)HapticPatterns.PresetType.Warning);
#else
            Vibrate();
#endif
        }

        /// <summary>
        /// Triggers a failure haptic feedback
        /// </summary>
        public void Failure()
        {
#if LOFELT_NICEVIBRATIONS
            Haptic((int)HapticPatterns.PresetType.Failure);
#else
            Vibrate();
#endif
        }

        /// <summary>
        /// Triggers a light impact haptic feedback
        /// </summary>
        public void LightImpact()
        {
#if LOFELT_NICEVIBRATIONS
            Haptic((int)HapticPatterns.PresetType.LightImpact);
#else
            Vibrate();
#endif
        }

        /// <summary>
        /// Triggers a medium impact haptic feedback
        /// </summary>
        public void MediumImpact()
        {
#if LOFELT_NICEVIBRATIONS
            Haptic((int)HapticPatterns.PresetType.MediumImpact);
#else
            Vibrate();
#endif
        }

        /// <summary>
        /// Triggers a heavy impact haptic feedback
        /// </summary>
        public void HeavyImpact()
        {
#if LOFELT_NICEVIBRATIONS
            Haptic((int)HapticPatterns.PresetType.HeavyImpact);
#else
            Vibrate();
#endif
        }

        /// <summary>
        /// Triggers a rigid impact haptic feedback
        /// </summary>
        public void RigidImpact()
        {
#if LOFELT_NICEVIBRATIONS
            Haptic((int)HapticPatterns.PresetType.RigidImpact);
#else
            Vibrate();
#endif
        }

        /// <summary>
        /// Triggers a soft impact haptic feedback
        /// </summary>
        public void SoftImpact()
        {
#if LOFELT_NICEVIBRATIONS
            Haptic((int)HapticPatterns.PresetType.SoftImpact);
#else
            Vibrate();
#endif
        }

        #endregion

        #region Public API - Advanced Vibration

        /// <summary>
        /// Triggers a transient haptic with custom intensity and sharpness
        /// </summary>
        /// <param name="intensity">Intensity of the haptic (0-1)</param>
        /// <param name="sharpness">Sharpness of the haptic (0-1)</param>
        public void TransientHaptic(float intensity, float sharpness)
        {
            if (!this.vibrationEnabled) return;
            
#if LOFELT_NICEVIBRATIONS
            HapticPatterns.PlayEmphasis(intensity, sharpness);
#else
            Handheld.Vibrate();
#endif
        }

        /// <summary>
        /// Triggers a transient haptic with default values
        /// </summary>
        public void TransientHaptic()
        {
            TransientHaptic(_defaultIntensity, _defaultSharpness);
        }

        /// <summary>
        /// Starts a continuous haptic feedback
        /// </summary>
        /// <param name="intensity">Intensity of the haptic (0-1)</param>
        /// <param name="sharpness">Sharpness of the haptic (0-1)</param>
        /// <param name="duration">Duration in seconds</param>
        public void StartContinuousHaptic(float intensity, float sharpness, float duration)
        {
            if (!this.vibrationEnabled) return;
            
#if LOFELT_NICEVIBRATIONS
            HapticPatterns.PlayConstant(intensity, sharpness, duration);
#else
            Handheld.Vibrate();
#endif
        }

        /// <summary>
        /// Starts a continuous haptic feedback with default values
        /// </summary>
        /// <param name="duration">Duration in seconds</param>
        public void StartContinuousHaptic(float duration)
        {
            StartContinuousHaptic(_defaultIntensity, _defaultSharpness, duration);
        }

        /// <summary>
        /// Updates the current continuous haptic feedback
        /// </summary>
        /// <param name="intensity">New intensity (0-1)</param>
        /// <param name="sharpness">New sharpness (0-1)</param>
        public void UpdateContinuousHaptic(float intensity, float sharpness)
        {
            if (!this.vibrationEnabled) return;
            
#if LOFELT_NICEVIBRATIONS
            HapticController.clipLevel = intensity;
            HapticController.clipFrequencyShift = sharpness;
#else
            // No-op for fallback
#endif
        }

        /// <summary>
        /// Stops all haptic feedbacks
        /// </summary>
        public void StopAllHaptics()
        {
#if LOFELT_NICEVIBRATIONS
            HapticController.Stop();
#else
            // No-op for fallback
#endif
        }

        /// <summary>
        /// Stops continuous haptic feedback
        /// </summary>
        public void StopContinuousHaptic()
        {
#if LOFELT_NICEVIBRATIONS
            HapticController.Stop();
#else
            // No-op for fallback
#endif
        }

        #endregion

        #region Public API - Game-Specific Vibration

        /// <summary>
        /// Triggers vibration for button press
        /// </summary>
        public void ButtonPress()
        {
            LightImpact();
        }

        /// <summary>
        /// Triggers vibration for item collection
        /// </summary>
        public void ItemCollected()
        {
            Success();
        }

        /// <summary>
        /// Triggers vibration for damage taken
        /// </summary>
        public void DamageTaken()
        {
            Warning();
        }

        /// <summary>
        /// Triggers vibration for player death
        /// </summary>
        public void PlayerDeath()
        {
            Failure();
        }

        /// <summary>
        /// Triggers vibration for level completion
        /// </summary>
        public void LevelComplete()
        {
            Success();
        }

        /// <summary>
        /// Triggers vibration for power-up activation
        /// </summary>
        public void PowerUpActivated()
        {
            MediumImpact();
        }

        /// <summary>
        /// Triggers vibration for explosion
        /// </summary>
        public void Explosion()
        {
            HeavyImpact();
        }

        #endregion

        #region Public API - Settings

        /// <summary>
        /// Enables or disables vibration
        /// </summary>
        /// <param name="enabled">Whether vibration should be enabled</param>
        public void SetVibrationEnabled(bool enabled)
        {
            this.vibrationEnabled = enabled;
            
#if LOFELT_NICEVIBRATIONS
            HapticController.hapticsEnabled = enabled;
#endif
            
            if (this.debugMode)
            {
                LogService.Log($"[VibrationManager] Vibration {(enabled ? "enabled" : "disabled")}");
            }
        }

        /// <summary>
        /// Returns whether vibration is currently enabled
        /// </summary>
        /// <returns>True if vibration is enabled</returns>
        public bool IsVibrationEnabled()
        {
            return this.vibrationEnabled;
        }

        /// <summary>
        /// Returns whether haptics are supported on this device
        /// </summary>
        /// <returns>True if haptics are supported</returns>
        public bool IsHapticsSupported()
        {
#if LOFELT_NICEVIBRATIONS
            return DeviceCapabilities.isVersionSupported;
#else
            return SystemInfo.supportsVibration;
#endif
        }

        /// <summary>
        /// Returns whether device meets advanced haptic requirements
        /// </summary>
        /// <returns>True if device supports advanced haptics</returns>
        public bool MeetsAdvancedRequirements()
        {
#if LOFELT_NICEVIBRATIONS
            return DeviceCapabilities.meetsAdvancedRequirements;
#else
            return false;
#endif
        }

        /// <summary>
        /// Sets debug mode for vibration manager
        /// </summary>
        /// <param name="debugMode">Whether debug mode should be enabled</param>
        public void SetDebugMode(bool debugMode)
        {
            this.debugMode = debugMode;
        }

        /// <summary>
        /// Sets the global output level for haptics
        /// </summary>
        /// <param name="level">Output level (0-1)</param>
        public void SetOutputLevel(float level)
        {
#if LOFELT_NICEVIBRATIONS
            HapticController.outputLevel = Mathf.Clamp01(level);
#else
            // No-op for fallback
#endif
        }

        /// <summary>
        /// Gets the current global output level
        /// </summary>
        /// <returns>Current output level (0-1)</returns>
        public float GetOutputLevel()
        {
#if LOFELT_NICEVIBRATIONS
            return HapticController.outputLevel;
#else
            return 1.0f; // Default fallback
#endif
        }

        /// <summary>
        /// Checks if haptics are currently playing
        /// </summary>
        /// <returns>True if haptics are playing</returns>
        public bool IsPlaying()
        {
#if LOFELT_NICEVIBRATIONS
            return HapticController.IsPlaying();
#else
            return false;
#endif
        }

        #endregion
    }
}