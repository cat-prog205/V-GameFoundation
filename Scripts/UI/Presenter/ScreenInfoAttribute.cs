namespace VGameFoundation.Scripts.UI
{
    using System;

    /// <summary> attributes to store basic information of a screen </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class ScreenInfoAttribute : Attribute
    {
        public string AddressableScreenName { get; }

        public ScreenInfoAttribute(string addressableScreenName)
        {
            this.AddressableScreenName = addressableScreenName;
        }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class PopupInfoAttribute : ScreenInfoAttribute
    {
        public bool IsEnableBlur          { get; }
        public bool IsCloseWhenTapOutside { get; }

        public PopupInfoAttribute(
            string addressableScreenName,
            bool   isEnableBlur          = true,
            bool   isCloseWhenTapOutside = true
        ) : base(addressableScreenName)
        {
            this.IsEnableBlur          = isEnableBlur;
            this.IsCloseWhenTapOutside = isCloseWhenTapOutside;
        }
    }
}