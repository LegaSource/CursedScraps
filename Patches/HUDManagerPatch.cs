using CursedScraps.Behaviours.Curses;
using CursedScraps.Managers;
using CursedScraps.Registries;
using GameNetcodeStuff;
using HarmonyLib;
using LegaFusionCore.Utilities;
using System.Linq;
using TMPro;
using UnityEngine;

namespace CursedScraps.Patches;

internal class HUDManagerPatch
{
    public static TextMeshProUGUI cursesText;

    [HarmonyPatch(typeof(HUDManager), nameof(HUDManager.Start))]
    [HarmonyPostfix]
    private static void StartHUDManager()
        => cursesText = CreateUIElement(
            name: "CursesUI",
            anchorMin: new Vector2(1f, 0f),
            anchorMax: new Vector2(1f, 0f),
            pivot: new Vector2(1f, 0f),
            anchoredPosition: new Vector2(ConfigManager.deadCursesPosX.Value, ConfigManager.deadCursesPosY.Value),
            alignment: TextAlignmentOptions.BottomRight
        );

    public static TextMeshProUGUI CreateUIElement(string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition, TextAlignmentOptions alignment)
    {
        Transform parent = GameObject.Find("Systems/UI/Canvas/Panel/GameObject/PlayerScreen").transform;
        GameObject uiObject = new GameObject(name);
        uiObject.transform.localPosition = Vector3.zero;

        RectTransform rectTransform = uiObject.AddComponent<RectTransform>();
        rectTransform.SetParent(parent, worldPositionStays: false);
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.pivot = pivot;
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = new Vector2(300f, 300f);

        TextMeshProUGUI textMesh = uiObject.AddComponent<TextMeshProUGUI>();
        textMesh.alignment = alignment;
        textMesh.font = HUDManager.Instance.controlTipLines[0].font;
        textMesh.fontSize = 14f;

        return textMesh;
    }

    [HarmonyPatch(typeof(HUDManager), nameof(HUDManager.AssignNodeToUIElement))]
    [HarmonyPostfix]
    private static void ScanPerformed(ref HUDManager __instance, ref ScanNodeProperties node)
    {
        if (!__instance.scanNodes.ContainsValue(node)) return;
        Paralysis.ScanPerformed(GameNetworkManager.Instance.localPlayerController, node);
    }

    [HarmonyPatch(typeof(HUDManager), nameof(HUDManager.HoldInteractionFill))]
    [HarmonyPostfix]
    private static void EntranceInteraction(ref bool __result)
    {
        PlayerControllerB player = GameNetworkManager.Instance.localPlayerController;
        if (!__result || !CSCurseRegistry.HasCurse(player.gameObject)) return;

        EntranceTeleport entranceTeleport = player.hoveringOverTrigger?.gameObject.GetComponent<EntranceTeleport>();
        if (entranceTeleport != null && !Exploration.EntranceInteraction(player, entranceTeleport)) __result = false;
    }

    public static void RefreshCursesText(PlayerControllerB player)
    {
        if (!LFCUtilities.ShouldNotBeLocalPlayer(player) || !GameNetworkManager.Instance.localPlayerController.isPlayerDead || !CSCurseRegistry.HasCurse(player.gameObject)) return;

        string cursesName = null;
        foreach (string curseName in CSCurseRegistry.GetCurses(player.gameObject).Select(c => c.Name))
        {
            if (!string.IsNullOrEmpty(cursesName)) cursesName += "\n";
            cursesName += curseName;
        }
        cursesText.text = !string.IsNullOrEmpty(cursesName) ? cursesName : "";
    }
}
