namespace VGameFoundation.Script.Services.Localization
{
    using System;
    using System.Collections.Generic;
    using Cysharp.Threading.Tasks;
    using I2.Loc;
    using TMPro;
    using UnityEngine;
    using VContainer.Unity;
    using VGameFoundation.Scripts.Services.GameAsset;
    using VGameFoundation.Scripts.Utilities.LogService;

    public class LocalizationService : IInitializable
    {
        private readonly IGameAssets         gameAssets;
        public static    LocalizationService Instance { get; private set; }
        public event Action OnLanguageChange;
        
        public LocalizationService(IGameAssets gameAssets)
        {
            this.gameAssets  = gameAssets;
        }

        public void Initialize()
        {
            Instance    = this;
        }

        public string GetTextWithKey(string key, List<string> formation = null, string overrideLanguage = null)
        {
            if (string.IsNullOrEmpty(key)) return string.Empty;

            var output = string.Empty;

            output = LocalizationManager.TryGetTranslation(key, out var localization,
                                                           overrideLanguage: overrideLanguage)
                         ? localization
                         : key;

            if (formation is { Count: > 0 })
            {
                output = string.Format(output, formation.ToArray());
            }

            if (output.Equals(key))
            {
                LogService.LogWarning($"{key} have no localization");
            }

            return output;
        }

        public void ChangeLanguage(string language)
        {
            if( LocalizationManager.HasLanguage(language))
            {
                LocalizationManager.CurrentLanguage = language;
            }
            
            this.OnLanguageChange?.Invoke();
        }

        public async UniTask<TMP_FontAsset> GetFontAsset(string overrideLanguage = null)
        {
            TMP_FontAsset fontAsset = null;
            const string  fontKey   = "TextMeshFontSmart";
            var fontAddress =
                LocalizationManager.TryGetTranslation(fontKey, out var localization, overrideLanguage: overrideLanguage)
                    ? localization
                    : fontKey;
            fontAsset = await this.gameAssets.LoadAssetAsync<TMP_FontAsset>(fontAddress);

            return fontAsset;
        }

        public async UniTask<Material> GetMaterial(string defaultMaterialName)
        {
            return await this.gameAssets.LoadAssetAsync<Material>(defaultMaterialName);
        }

        public List<string> ListLanguages() { return LocalizationManager.GetAllLanguages(); }

        public string CurrentLanguage => LocalizationManager.CurrentLanguage;
    }
}