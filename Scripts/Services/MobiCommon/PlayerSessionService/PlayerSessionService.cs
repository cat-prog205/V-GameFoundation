using System;
using UnityEngine;

namespace VGameFoundation.Scripts.Services.MobiCommon
{
    using VContainer.Unity;

    public interface IPlayerSessionService
    {
        int GetDayIndex();

        int GetSessionIndex();

        void StartNewSession();

        float GetSessionPlayTime();

        int GetSessionIndexTracking();
    }

    public class PlayerSessionService : IPlayerSessionService, IInitializable, ITickable
    {
        private const string FIRST_LOGIN_KEY = "FirstLoginDate";
        private const string SESSION_KEY     = "SessionIndex";

        private int      sessionIndex;
        private DateTime firstLoginDate;

        private float sessionPlayTime;
        private bool  isSessionActive;

        public void Initialize()
        {
            // Load first login date
            if (PlayerPrefs.HasKey(FIRST_LOGIN_KEY))
            {
                var dateStr = PlayerPrefs.GetString(FIRST_LOGIN_KEY);
                this.firstLoginDate = DateTime.Parse(dateStr);
                Debug.Log($"[PlayerSession] Loaded first login date: {this.firstLoginDate:yyyy-MM-dd}");
            }
            else
            {
                this.firstLoginDate = DateTime.Today;
                PlayerPrefs.SetString(FIRST_LOGIN_KEY, this.firstLoginDate.ToString("yyyy-MM-dd"));
                Debug.Log($"[PlayerSession] First login, set date: {this.firstLoginDate:yyyy-MM-dd}");
            }

            this.sessionIndex = PlayerPrefs.GetInt(SESSION_KEY, -1);
            this.StartNewSession();
        }

        public void StartNewSession()
        {
            this.sessionIndex++;
            PlayerPrefs.SetInt(SESSION_KEY, this.sessionIndex);
            PlayerPrefs.Save();

            this.sessionPlayTime = 0f;
            this.isSessionActive = true;
            Debug.Log($"[PlayerSession] Start new session -> SessionIndex: {this.sessionIndex}");
        }

        public int GetDayIndex()
        {
            var diff     = DateTime.Today - this.firstLoginDate;
            var dayIndex = diff.Days + 1;

            return dayIndex; // First day = 1
        }

        public int GetSessionIndex() { return this.sessionIndex; }

        public float GetSessionPlayTime() { return this.sessionPlayTime; }

        public int GetSessionIndexTracking()
        {
            var sessionTracking = PlayerPrefs.GetInt(SESSION_KEY, 0) + 1;

            return sessionTracking;
        }

        public void Tick()
        {
            if (!this.isSessionActive) return;

            this.sessionPlayTime += Time.unscaledDeltaTime;
        }

        public void EndSession()
        {
            this.isSessionActive = false;
            Debug.Log($"[PlayerSession] EndSession -> Total play time: {this.sessionPlayTime:F2}s");
        }
    }
}