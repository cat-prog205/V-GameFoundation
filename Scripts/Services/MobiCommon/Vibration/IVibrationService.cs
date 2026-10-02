namespace VGameFoundation.Script.Services.MobiCommon.Vibration
{
     /// <summary>
     /// Interface for vibration service
     /// Updated for latest Nice Vibrations API with backward compatibility
     /// </summary>
     public interface IVibrationService
     {
          // Basic vibration methods
          void Vibrate();

          void Haptic(int type);

          // Convenience methods for common haptic types
          void Selection();

          void Success();

          void Warning();

          void Failure();

          void LightImpact();

          void MediumImpact();

          void HeavyImpact();

          void RigidImpact();

          void SoftImpact();

          // Advanced vibration methods
          void TransientHaptic(float intensity, float sharpness);

          void TransientHaptic();

          void StartContinuousHaptic(float intensity, float sharpness, float duration);

          void StartContinuousHaptic(float duration);

          void UpdateContinuousHaptic(float intensity, float sharpness);

          void StopAllHaptics();

          void StopContinuousHaptic();

          // Game-specific vibration methods
          void ButtonPress();

          void ItemCollected();

          void DamageTaken();

          void PlayerDeath();

          void LevelComplete();

          void PowerUpActivated();

          void Explosion();

          // Settings methods
          void SetVibrationEnabled(bool enabled);

          bool IsVibrationEnabled();

          bool IsHapticsSupported();

          bool MeetsAdvancedRequirements();

          void SetDebugMode(bool debugMode);

          void SetOutputLevel(float level);

          float GetOutputLevel();

          bool IsPlaying();
     }
}