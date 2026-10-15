// Derived from Kaeno Trader Scrolling (DrakiaXYZ/SPT-Kaeno-TraderScrolling).
// See the original project for attribution and license.
using System.Linq;
using System.Reflection;
using Comfort.Common;
using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine;
using UnityEngine.UI;

namespace SPT.Custom.Patches;

internal class NativeTraderScrollingPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod() =>
        AccessTools.Method(typeof(TraderScreensGroup), nameof(TraderScreensGroup.Show));

    [PatchPostfix]
    public static void PatchPostfix()
    {
        var menu = Singleton<MenuUI>.Instance;
        if (menu != null && menu.GetComponentInChildren<NativeTraderScrollingBehaviour>(true) == null)
        {
            menu.gameObject.AddComponent<NativeTraderScrollingBehaviour>();
        }
    }
}

internal sealed class NativeTraderScrollingBehaviour : MonoBehaviour
{
    private void Awake()
    {
        var cards = GameObject.Find("TraderCards");
        if (cards == null || cards.transform.childCount <= 9)
        {
            Destroy(this);
            return;
        }

        var cardRect = cards.GetComponent<RectTransform>();
        var containerRect = Singleton<MenuUI>.Instance.GetComponentsInChildren<RectTransform>(true)
            .FirstOrDefault(x => x.name == "Container");

        if (cardRect == null || containerRect == null)
        {
            Destroy(this);
            return;
        }

        var extraCards = cards.transform.childCount - 9;
        cardRect.anchorMin = new Vector2(0.595f - 0.065f * extraCards, 1f);
        cardRect.anchorMax = new Vector2(1f, 1f);
        containerRect.anchorMin = new Vector2(0.01f, 0f);
        containerRect.anchorMax = new Vector2(1f, 1f);

        var scrolling = cards.GetComponent<ScrollRect>() ?? cards.AddComponent<ScrollRect>();
        scrolling.content = cardRect;
        scrolling.viewport = containerRect;
        scrolling.horizontal = true;
        scrolling.vertical = false;
        scrolling.movementType = ScrollRect.MovementType.Elastic;
        scrolling.scrollSensitivity = 30f;
    }
}
