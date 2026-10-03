namespace VGameFoundation.Script.Services.Localization
{
    using TMPro;
    using UnityEditorHomeMade;
    using UnityEngine;
    using VGameFoundation.Scripts.Utilities.LogService;

#if Unity_Localization
    [DisallowMultipleComponent]
    public class BaseTextMeshPro : MonoBehaviour
    {
        [SerializeField] protected TMP_Text txtText;

        private string        lastKey;
        private object[]      lastArgs;
        
        [SerializeField]
        [ReadOnly]
        private string defaultMat;

        private bool isInitialized = false;

        private void Awake()
        {
            this.EnsureInitialized();

            if (string.IsNullOrEmpty(this.lastKey))
            {
                this.SetText(this.txtText.text);
            }
        }

        private void EnsureInitialized()
        {
            if (this.isInitialized) return;

            this.txtText ??= this.GetComponent<TMP_Text>();
            
            if (this.txtText != null && this.txtText.fontSharedMaterial != null)
            {
                this.defaultMat = this.txtText.fontSharedMaterial.name;
            }

            if (LocalizationService.Instance != null)
            {
                LocalizationService.Instance.OnLanguageChange -= this.OnLanguageChange;
                LocalizationService.Instance.OnLanguageChange += this.OnLanguageChange;
            }

            this.isInitialized = true;
        }
        
        private void OnDestroy() 
        { 
            if (LocalizationService.Instance != null)
            {
                LocalizationService.Instance.OnLanguageChange -= this.OnLanguageChange;
            }
        }

        public TMP_Text Text => this.txtText;
        
        private void OnLanguageChange()
        {
            if (string.IsNullOrEmpty(this.lastKey)) return;

            if (this.lastArgs != null && this.lastArgs.Length > 0)
                this.SetText(this.lastKey, this.lastArgs);
            else
                this.SetText(this.lastKey);
        }

        /// <summary>
        /// Static text by key
        /// </summary>
        public void SetText(string key, Color colorCode = default) { this.ApplyLocalization(key, null, colorCode); }

        /// <summary>
        /// Dynamic text with parameters, e.g. key = "key_level" -> Level {0}, args = {7} => "Level 7"
        /// </summary>
        public void SetText(string key, object[] args, Color colorCode = default) { this.ApplyLocalization(key, args, colorCode); }

        private async void ApplyLocalization(string key, object[] args, Color colorCode)
        {
            this.EnsureInitialized();

            var localizedFormat = LocalizationService.Instance.GetTextWithKey(key);
            var finalText = args != null && args.Length > 0
                ? string.Format(localizedFormat, args)
                : localizedFormat;
            
            var localizedMaterial = LocalizationService.Instance.GetTextWithKey(this.defaultMat);

            this.txtText.text = finalText;

            if (colorCode != default) this.txtText.color = colorCode;

            this.lastKey  = key;
            this.lastArgs = args;

            var font = await LocalizationService.Instance.GetFontAsset();
            var material = await LocalizationService.Instance.GetMaterial(localizedMaterial);

            if (font != null)
                this.txtText.font = font;
            else
                LogService.LogError("Font is null");

            this.txtText.fontSharedMaterial = material;
        }
    }   
#endif
}