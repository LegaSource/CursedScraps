﻿using UnityEngine.Rendering.HighDefinition;
using UnityEngine;
using System.Linq;
using CursedScraps.Behaviours;
using System.Collections.Generic;

namespace CursedScraps.Managers
{
    public class CustomPassManager : MonoBehaviour
    {
        public static WallhackCustomPass wallhackPass;
        public static CustomPassVolume customPassVolume;

        public static CustomPassVolume CustomPassVolume
        {
            get
            {
                if (customPassVolume == null)
                {
                    customPassVolume = GameNetworkManager.Instance.localPlayerController.gameplayCamera.gameObject.AddComponent<CustomPassVolume>();
                    if (customPassVolume != null)
                    {
                        customPassVolume.targetCamera = GameNetworkManager.Instance.localPlayerController.gameplayCamera;
                        customPassVolume.injectionPoint = (CustomPassInjectionPoint)1;
                        customPassVolume.isGlobal = true;

                        wallhackPass = new WallhackCustomPass();
                        customPassVolume.customPasses.Add(wallhackPass);
                    }
                }
                return customPassVolume;
            }
        }

        public static void SetupCustomPassForDoor(EntranceTeleport targetDoor)
        {
            Renderer[] doorRenderers = FindObjectsOfType<Renderer>()
                .Where(r => Vector3.Distance(r.transform.position, targetDoor.transform.position) == 0f
                            && ConfigManager.rendererNames.Value.Any(n => r.name.StartsWith(n)))
                .ToArray();

            if (CustomPassVolume == null)
            {
                CursedScraps.mls.LogError("CustomPassVolume is not assigned.");
                return;
            }

            wallhackPass = CustomPassVolume.customPasses.Find(pass => pass is WallhackCustomPass) as WallhackCustomPass;
            if (wallhackPass == null)
            {
                CursedScraps.mls.LogError("WallhackCustomPass could not be found in CustomPassVolume.");
                return;
            }

            wallhackPass.SetTargetRenderers(doorRenderers, CursedScraps.wallhackShader);
        }

        public static void SetupCustomPassForDoors(bool isEntrance)
        {
            List<Renderer> doorRenderers = new List<Renderer>();
            foreach (EntranceTeleport entranceTeleport in FindObjectsOfType<EntranceTeleport>())
            {
                if (entranceTeleport.isEntranceToBuilding != isEntrance) continue;

                doorRenderers.AddRange(FindObjectsOfType<Renderer>()
                    .Where(r => Vector3.Distance(r.transform.position, entranceTeleport.transform.position) == 0f
                                && ConfigManager.rendererNames.Value.Any(n => r.name.StartsWith(n)))
                    .ToArray());
            }

            if (CustomPassVolume == null)
            {
                CursedScraps.mls.LogError("CustomPassVolume is not assigned.");
                return;
            }

            wallhackPass = CustomPassVolume.customPasses.Find(pass => pass is WallhackCustomPass) as WallhackCustomPass;
            if (wallhackPass == null)
            {
                CursedScraps.mls.LogError("WallhackCustomPass could not be found in CustomPassVolume.");
                return;
            }

            wallhackPass.SetTargetRenderers(doorRenderers.ToArray(), CursedScraps.wallhackShader);
        }

        public static void RemoveAuraFromDoor()
            => wallhackPass?.ClearTargetRenderers();
    }
}
