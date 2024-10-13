using CursedScraps.Behaviours;
using CursedScraps.Managers;
using GameNetcodeStuff;
using LethalCompanyInputUtils.Api;
using LethalCompanyInputUtils.BindingPathEnums;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CursedScraps.CustomInputs
{
    public class CommunicationInputs : LcInputActions
    {
        public static readonly CommunicationInputs Instance = new();
        public InputAction HotParticleKey => Asset["hotparticlekey"];
        public InputAction ColdParticleKey => Asset["coldparticlekey"];
        public bool isOnCooldown = false;

        public override void CreateInputActions(in InputActionMapBuilder builder)
        {
            builder.NewActionBinding()
                .WithActionId("hotparticlekey")
                .WithActionType(InputActionType.Button)
                .WithKeyboardControl(KeyboardControl.H)
                .WithGamepadControl(GamepadControl.ButtonNorth)
                .WithBindingName("HotParticle")
                .Finish();
            builder.NewActionBinding()
                .WithActionId("coldparticlekey")
                .WithActionType(InputActionType.Button)
                .WithKeyboardControl(KeyboardControl.C)
                .WithGamepadControl(GamepadControl.ButtonNorth)
                .WithBindingName("ColdParticle")
                .Finish();
        }

        public void EnableInputs()
        {
            HotParticleKey.performed += SpawnHotParticle;
            ColdParticleKey.performed += SpawnColdParticle;
        }

        public void DisableInputs()
        {
            HotParticleKey.performed -= SpawnHotParticle;
            ColdParticleKey.performed -= SpawnColdParticle;
        }

        public void SpawnHotParticle(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                SpawnParticle(true);
            }
        }

        public void SpawnColdParticle(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                SpawnParticle(false);
            }
        }

        public void SpawnParticle(bool isHot)
        {
            PlayerControllerB localPlayer = GameNetworkManager.Instance.localPlayerController;
            if (!isOnCooldown
                && localPlayer.isPlayerDead
                && !StartOfRound.Instance.overrideSpectateCamera
                && localPlayer.spectatedPlayerScript != null
                && !localPlayer.spectatedPlayerScript.isPlayerDead)
            {
                PlayerCSBehaviour playerBehaviour = localPlayer.spectatedPlayerScript.GetComponent<PlayerCSBehaviour>();
                if (playerBehaviour.trackedItem != null)
                {
                    localPlayer.StartCoroutine(CooldownCoroutine());
                    CursedScrapsNetworkManager.Instance.SpawnParticleServerRpc((int)playerBehaviour.playerProperties.actualClientId, isHot);
                }
            }
        }

        public IEnumerator CooldownCoroutine()
        {
            isOnCooldown = true;
            yield return new WaitForSeconds(ConfigManager.communicationCooldown.Value);
            isOnCooldown = false;
        }
    }
}
