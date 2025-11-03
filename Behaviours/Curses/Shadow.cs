using CursedScraps.Managers;
using GameNetcodeStuff;
using static CursedScraps.Registries.CSCurseRegistry;

namespace CursedScraps.Behaviours.Curses;

public class Shadow(int playerWhoHit, int duration, System.Action onApply, System.Action onExpire, System.Action onUpdate)
    : CurseEffect(Type, playerWhoHit, duration, onApply, onExpire, onUpdate)
{
    private static readonly CurseEffectType Type = curseEffectTypes.Find(t => t.Name.Equals(Constants.SHADOW));

    public static void ApplyShadow(EnemyAI enemy)
    {
        if (ConfigManager.shadowExclusions.Value.Contains(enemy.enemyType.enemyName)) return;

        PlayerControllerB player = GameNetworkManager.Instance?.localPlayerController;
        if (player != null && HasCurse(player.gameObject, Constants.SHADOW))
        {
            ScanNodeProperties scanNode = enemy.gameObject.GetComponentInChildren<ScanNodeProperties>();
            if (scanNode != null && HUDManager.Instance != null)
            {
                enemy.EnableEnemyMesh(HUDManager.Instance.scanNodes.ContainsValue(scanNode));
                return;
            }
        }
        enemy.EnableEnemyMesh(true);
    }
}