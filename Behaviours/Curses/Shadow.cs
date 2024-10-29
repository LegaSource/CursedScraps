using CursedScraps.Managers;
using System.Linq;

namespace CursedScraps.Behaviours.Curses
{
    public class Shadow
    {
        public static void ApplyShadow(ref EnemyAI enemy)
        {
            if (!ConfigManager.shadowExclusions.Value.Contains(enemy.enemyType.enemyName))
            {
                PlayerCSBehaviour playerBehaviour = GameNetworkManager.Instance.localPlayerController.GetComponent<PlayerCSBehaviour>();
                if (playerBehaviour != null
                    && playerBehaviour.activeCurses.FirstOrDefault(c => c.CurseName.Equals(Constants.SHADOW)) != null)
                {
                    ScanNodeProperties scanNode = enemy.gameObject.GetComponentInChildren<ScanNodeProperties>();
                    if (scanNode != null && HUDManager.Instance != null)
                    {
                        if (HUDManager.Instance.scanNodes.ContainsValue(scanNode))
                        {
                            enemy.EnableEnemyMesh(true);
                        }
                        enemy.EnableEnemyMesh(false);
                        return;
                    }
                }
                enemy.EnableEnemyMesh(true);
            }
        }
    }
}
