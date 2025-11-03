using CursedScraps.Managers;
using GameNetcodeStuff;
using LegaFusionCore.Registries;
using LegaFusionCore.Utilities;
using UnityEngine;
using static CursedScraps.Registries.CSCurseRegistry;

namespace CursedScraps.Behaviours.Curses;

public class Paralysis(int playerWhoHit, int duration, System.Action onApply, System.Action onExpire, System.Action onUpdate)
    : CurseEffect(Type, playerWhoHit, duration, onApply, onExpire, onUpdate)
{
    private static readonly CurseEffectType Type = curseEffectTypes.Find(t => t.Name.Equals(Constants.PARALYSIS));
    private static readonly string[] actionNames = ["Move", "Jump", "Crouch", "Interact", "ItemSecondaryUse", "ItemTertiaryUse", "ActivateItem", "SwitchItem", "InspectItem", "Emote1", "Emote2"];
    private static bool isParalyzed = false;
    private static float paralyzeTimer = 0f;

    public static void ScanPerformed(PlayerControllerB player, ScanNodeProperties scanNodeProperties)
    {
        if (!HasCurse(player.gameObject, Constants.PARALYSIS) || scanNodeProperties.nodeType != 1) return;

        foreach (string actionName in actionNames)
        {
            isParalyzed = true;
            player.JumpToFearLevel(0.6f);
            LFCPlayerActionRegistry.AddLock(actionName, $"{CursedScraps.modName}{Constants.PARALYSIS}");
        }
    }

    public override void Update(GameObject entity)
    {
        base.Update(entity);
        if (!isParalyzed) return;

        PlayerControllerB player = LFCUtilities.GetSafeComponent<PlayerControllerB>(entity);
        if (!LFCUtilities.ShouldBeLocalPlayer(player)) return;

        paralyzeTimer += Time.deltaTime;
        if (paralyzeTimer >= ConfigManager.paralysisTime.Value)
        {
            isParalyzed = false;
            paralyzeTimer = 0f;
            foreach (string actionName in actionNames) LFCPlayerActionRegistry.RemoveLock(actionName, $"{CursedScraps.modName}{Constants.PARALYSIS}");
        }
    }

    public override void Expire(GameObject entity)
    {
        base.Expire(entity);

        if (isParalyzed)
        {
            PlayerControllerB player = LFCUtilities.GetSafeComponent<PlayerControllerB>(entity);
            if (!LFCUtilities.ShouldBeLocalPlayer(player)) return;

            foreach (string actionName in actionNames) LFCPlayerActionRegistry.RemoveLock(actionName, $"{CursedScraps.modName}{Constants.PARALYSIS}");
        }
    }
}