using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using VGameFoundation.Scripts.Utilities.LogService;

/// <summary>
/// Manages global time scale operations including freezing, resetting, and slow-motion effects.
/// </summary>
public static class GlobalTimeScale
{
    private static float previousTimeScale = 1f;
    private static bool isFrozen = false;

    public static event Action<float> OnTimeScaleChanged;

    /// <summary>
    /// Freezes the global time (timeScale = 0).
    /// </summary>
    public static void Freeze()
    {
        if (isFrozen)
        {
            LogService.Log("[GlobalTimeScale] Already frozen, skipping Freeze().");
            return;
        }

        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        isFrozen = true;
        OnTimeScaleChanged?.Invoke(Time.timeScale);

        LogService.Log($"[GlobalTimeScale] Freeze() → previousTimeScale = {previousTimeScale}");
    }

    /// <summary>
    /// Restores timeScale to the value it had before Freeze was called.
    /// </summary>
    public static void Reset()
    {
        if (!isFrozen)
        {
            LogService.Log("[GlobalTimeScale] Not currently frozen, skipping Reset().");
            return;
        }

        Time.timeScale = previousTimeScale;
        isFrozen = false;
        OnTimeScaleChanged?.Invoke(previousTimeScale);

        LogService.Log($"[GlobalTimeScale] Reset() → Restored timeScale to {previousTimeScale}");
    }

    /// <summary>
    /// Sets a specific timeScale value directly. Can be used for slow-motion or speed-up effects.
    /// </summary>
    /// <param name="value">New timeScale value (clamped between 0 and 10).</param>
    public static void Set(float value)
    {
        value = Mathf.Clamp(value, 0f, 10f);
        previousTimeScale = value;
        Time.timeScale = value;
        isFrozen = (value == 0f);
        OnTimeScaleChanged?.Invoke(previousTimeScale);

        LogService.Log($"[GlobalTimeScale] Set() → timeScale = {value}");
    }

    /// <summary>
    /// Freezes time for a specified duration (using real-time) then automatically Resets.
    /// </summary>
    /// <param name="seconds">Duration of the freeze in real seconds.</param>
    public static async UniTask FreezeForSeconds(float seconds)
    {
        if (isFrozen)
        {
            LogService.Log("[GlobalTimeScale] FreezeForSeconds() ignored because time is already frozen.");
            return;
        }

        Freeze();
        LogService.Log($"[GlobalTimeScale] FreezeForSeconds({seconds}) starting...");

        // Delay using real-time (ignoreTimeScale: true) because timeScale is now 0.
        await UniTask.Delay(TimeSpan.FromSeconds(seconds), ignoreTimeScale: true);

        Reset();
        LogService.Log("[GlobalTimeScale] FreezeForSeconds() complete, timeScale restored.");
    }

    /// <summary>
    /// Returns true if the timeScale is currently frozen (0).
    /// </summary>
    public static bool IsFrozen => isFrozen;
}