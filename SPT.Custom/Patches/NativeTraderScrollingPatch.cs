// Derived from Kaeno Trader Scrolling (DrakiaXYZ/SPT-Kaeno-TraderScrolling).
// See the original project for attribution and license.
using System.Linq;
using BepInEx.Configuration;
using System.Reflection;
using Comfort.Common;
using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine;
using UnityEngine.UI;
using SPT.Reflection.Utils;

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
    internal static ConfigEntry<float> ScrollWheelSpeed;
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
        scrolling.scrollSensitivity = ScrollWheelSpeed?.Value ?? 30f;
        if (ScrollWheelSpeed != null)
        {
            ScrollWheelSpeed.SettingChanged += (_, _) =>
            {
                if (scrolling != null) scrolling.scrollSensitivity = ScrollWheelSpeed.Value;
            };
        }
    }
}


internal class NativeTraderPlayerCardPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod() =>
        typeof(DisplayMoneyPanelTMPText).GetMethod("Show", PatchConstants.PublicFlags);

    [PatchPostfix]
    public static void PatchPostfix()
    {
        var menu = Singleton<MenuUI>.Instance;
        if (menu != null && menu.GetComponent<NativeTraderPlayerCardBehaviour>() == null)
            menu.gameObject.AddComponent<NativeTraderPlayerCardBehaviour>();
    }
}

internal sealed class NativeTraderPlayerCardBehaviour : MonoBehaviour
{
    private void Awake()
    {
        var rightPerson = GameObject.Find("Right Person");
        if (rightPerson == null) { Destroy(this); return; }
        var rects = rightPerson.GetComponentsInChildren<RectTransform>(true);
        var money = rects.FirstOrDefault(x => x.name == "Money");
        if (money != null)
            money.anchoredPosition += new Vector2(60f, 0f);
        var moneyLayout = rightPerson.GetComponentsInChildren<HorizontalLayoutGroup>(true)
            .FirstOrDefault(x => x.name == "Money");
        if (moneyLayout != null) moneyLayout.spacing = 10f;
        var images = rightPerson.GetComponentsInChildren<Image>(true);
        var tile = images.FirstOrDefault(x => x.name == "Background Tile");
        if (tile != null)
        {
            tile.type = Image.Type.Simple;
            tile.gameObject.SetActive(true);
            tile.rectTransform.sizeDelta = new Vector2(500f, 0f);
        }
        var background = images.FirstOrDefault(x => x.name == "Background");
        if (background != null) background.color = Color.black;
    }
}
