using UnityEngine;

namespace VGameFoundation.Script.SoundService
{
    using UnityEngine.UI;

    [RequireComponent(typeof(Button))]
    public class BtnSFX : MonoBehaviour
    {
        /*[SerializeField] private SoundType soundType; // = SoundType.BtnClick

        [Inject] private ISoundService soundService;

        private void Awake()
        {
            var btn = this.GetComponent<Button>();
            btn.onClick.AddListener(() => { this.soundService.PlaySFX(this.soundType); });
        }*/
    }
}