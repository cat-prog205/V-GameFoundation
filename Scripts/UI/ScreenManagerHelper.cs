namespace VGameFoundation.Scripts.UI
{
    using UnityEngine;

    public class ScreenManagerHelper : MonoBehaviour
    {
        [SerializeField] private RectTransform uiScreenParent;
        [SerializeField] private RectTransform uiPopupOverlayParent;
        [SerializeField] private RectTransform uiPopupNonOverlayParent;
        [SerializeField] private RectTransform uiCloseParent;

        public RectTransform UIPopupOverlayParent    => this.uiPopupOverlayParent;
        public RectTransform UIPopupNonOverlayParent => this.uiPopupNonOverlayParent;
        public RectTransform UICloseParent           => this.uiCloseParent;
        public RectTransform UIScreenParent          => this.uiScreenParent;

        private void Awake()
        {
            DontDestroyOnLoad(this.gameObject);
        }

        private void Reset()
        {
            this.AutoAssignParents();
        }

        [ContextMenu("Refresh Parents")] 
        public void AutoAssignParents()
        {
            this.uiScreenParent          = this.FindChildByName("UIScreenParent");
            this.uiPopupOverlayParent    = this.FindChildByName("UIPopupOverlayParent");
            this.uiPopupNonOverlayParent = this.FindChildByName("UIPopupNonOverlayParent");
            this.uiCloseParent           = this.FindChildByName("UICloseParent");
        }

        private RectTransform FindChildByName(string childName)
        {
            Transform child = this.transform.Find(childName);
            if (child != null)
            {
                return child.GetComponent<RectTransform>();
            }
            return null;
        }
    }
}