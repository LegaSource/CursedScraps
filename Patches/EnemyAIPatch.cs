using CursedScraps.Behaviours;
using CursedScraps.Managers;
using HarmonyLib;
using System.Linq;

namespace CursedScraps.Patches
{
    internal class EnemyAIPatch
    {
        [HarmonyPatch(typeof(EnemyAI), nameof(EnemyAI.SetClientCalculatingAI))]
        [HarmonyPostfix]
        private static void UpdateMeshWithShadowCurse(ref EnemyAI __instance)
        {
            if (!ConfigManager.shadowExclusions.Value.Contains(__instance.enemyType.enemyName))
            {
                PlayerCSBehaviour playerBehaviour = GameNetworkManager.Instance.localPlayerController.GetComponent<PlayerCSBehaviour>();
                if (playerBehaviour != null
                    && playerBehaviour.activeCurses.FirstOrDefault(c => c.CurseName.Equals(Constants.SHADOW)) != null)
                {
                    ScanNodeProperties scanNode = __instance.gameObject.GetComponentInChildren<ScanNodeProperties>();
                    if (scanNode != null && HUDManager.Instance != null)
                    {
                        if (HUDManager.Instance.scanNodes.ContainsValue(scanNode))
                        {
                            __instance.EnableEnemyMesh(true);
                        }
                        else
                        {
                            __instance.EnableEnemyMesh(false);
                        }
                    }
                    else
                    {
                        __instance.EnableEnemyMesh(true);
                    }
                }
            }
        }
    }
}
