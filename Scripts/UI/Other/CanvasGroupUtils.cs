using UnityEngine;

public static class CanvasGroupUtils
{
    public static CanvasGroup GetOrAddCanvasGroup(this GameObject go)
    {
        var cg      = go.GetComponent<CanvasGroup>();
        if (!cg) cg = go.AddComponent<CanvasGroup>();

        return cg;
    }

    public static void Show(this CanvasGroup cg, bool interactable = true, bool blocksRaycasts = true)
    {
        cg.alpha          = 1f;
        cg.interactable   = interactable;
        cg.blocksRaycasts = blocksRaycasts;
    }

    public static void Hide(this CanvasGroup cg)
    {
        cg.alpha          = 0f;
        cg.interactable   = false;
        cg.blocksRaycasts = false;
    }

    public static void SetVisible(this CanvasGroup cg, bool visible, bool interactable = true, bool blocksRaycasts = true)
    {
        if (visible) cg.Show(interactable, blocksRaycasts);
        else cg.Hide();
    }
}