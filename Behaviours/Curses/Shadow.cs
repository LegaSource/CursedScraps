using CursedScraps.Managers;
using System.Linq;

namespace CursedScraps.Behaviours.Curses
{
    public class Shadow
    {
        public static void ApplyShadow(EnemyAI enemy)
        {
            if (ConfigManager.shadowExclusions.Value.Contains(enemy.enemyType.enemyName)) return;

            PlayerCSBehaviour playerBehaviour = GameNetworkManager.Instance?.localPlayerController?.GetComponent<PlayerCSBehaviour>();
            if (playerBehaviour != null && playerBehaviour.activeCurses.Any(c => c.CurseName.Equals(Constants.SHADOW)))
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
}
