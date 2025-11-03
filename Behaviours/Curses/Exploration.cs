using CursedScraps.Managers;
using GameNetcodeStuff;
using LegaFusionCore.Behaviours.Shaders;
using LegaFusionCore.Registries;
using LegaFusionCore.Utilities;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static CursedScraps.Registries.CSCurseRegistry;

namespace CursedScraps.Behaviours.Curses;

public class Exploration(int playerWhoHit, int duration, System.Action onApply, System.Action onExpire, System.Action onUpdate)
    : CurseEffect(Type, playerWhoHit, duration, onApply, onExpire, onUpdate)
{
    private static readonly CurseEffectType Type = curseEffectTypes.Find(t => t.Name.Equals(Constants.EXPLORATION));
    private static EntranceTeleport targetedDoor;

    public override void Apply(GameObject entity)
    {
        base.Apply(entity);

        PlayerControllerB player = LFCUtilities.GetSafeComponent<PlayerControllerB>(entity);
        if (LFCUtilities.ShouldBeLocalPlayer(player)) ChangeRandomEntranceId(!player.isInsideFactory);
    }

    public override void Update(GameObject entity)
    {
        base.Update(entity);

        if (targetedDoor == null) return;
        PlayerControllerB player = LFCUtilities.GetSafeComponent<PlayerControllerB>(entity);
        if (!LFCUtilities.ShouldBeLocalPlayer(player)) return;

        if (Vector3.Distance(targetedDoor.transform.position, player.transform.position) > ConfigManager.explorationDistance.Value)
        {
            CustomPassManager.SetupAuraForObjects([targetedDoor.gameObject], LegaFusionCore.LegaFusionCore.wallhackShader, $"{CursedScraps.modName}{Constants.EXPLORATION}", Color.yellow);
            return;
        }
        CustomPassManager.RemoveAuraByTag($"{CursedScraps.modName}{Constants.EXPLORATION}");
    }

    public override void Expire(GameObject entity)
    {
        base.Expire(entity);

        PlayerControllerB player = LFCUtilities.GetSafeComponent<PlayerControllerB>(entity);
        if (LFCUtilities.ShouldBeLocalPlayer(player))
        {
            CustomPassManager.RemoveAuraByTag($"{CursedScraps.modName}{Constants.EXPLORATION}");
            targetedDoor = null;
        }
    }

    public static bool EntranceInteraction(PlayerControllerB player, EntranceTeleport entranceTeleport)
    {
        if (!HasCurse(player.gameObject, Constants.EXPLORATION)) return true;

        if (entranceTeleport != targetedDoor)
        {
            HUDManager.Instance.DisplayTip(Constants.IMPOSSIBLE_ACTION, "A curse prevents you from using this doorway.");
            return false;
        }
        ChangeRandomEntranceId(player.isInsideFactory);
        return true;
    }

    public static void ChangeRandomEntranceId(bool isEntrance)
    {
        List<EntranceTeleport> entrances = LFCSpawnRegistry.GetAllAs<EntranceTeleport>()
            .Where(e => e.isEntranceToBuilding == isEntrance)
            .ToList();
        targetedDoor = entrances[new System.Random().Next(entrances.Count)];
    }
}
