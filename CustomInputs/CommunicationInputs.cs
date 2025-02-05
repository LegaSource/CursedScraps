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
        private static CommunicationInputs instance;

        public static CommunicationInputs Instance
        {
            get
            {
                instance ??= new CommunicationInputs();
                return instance;
            }
            private set { instance = value; }
        }

        [InputAction(KeyboardControl.H, GamepadControl = GamepadControl.ButtonNorth, Name = "Communication - Hot Particle")]
        public InputAction HotParticleKey { get; set; }

        [InputAction(KeyboardControl.C, GamepadControl = GamepadControl.ButtonSouth, Name = "Communication - Cold Particle")]
        public InputAction ColdParticleKey { get; set; }

        public bool isOnCooldown = false;

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
            if (!context.performed) return;
            SpawnParticle(true);
        }

        public void SpawnColdParticle(InputAction.CallbackContext context)
        {
            if (!context.performed) return;
            SpawnParticle(false);
        }

        public void SpawnParticle(bool isHot)
        {
            PlayerControllerB localPlayer = GameNetworkManager.Instance.localPlayerController;
            if (isOnCooldown) return;
            if (!localPlayer.isPlayerDead) return;
            if (StartOfRound.Instance.overrideSpectateCamera) return;
            if (localPlayer.spectatedPlayerScript == null) return;
            if (localPlayer.spectatedPlayerScript.isPlayerDead) return;

            PlayerCSBehaviour playerBehaviour = localPlayer.spectatedPlayerScript.GetComponent<PlayerCSBehaviour>();
            if (playerBehaviour.trackedItem == null) return;

            localPlayer.StartCoroutine(CooldownCoroutine());
            CursedScrapsNetworkManager.Instance.SpawnParticleServerRpc((int)playerBehaviour.playerProperties.actualClientId, isHot);
        }

        public IEnumerator CooldownCoroutine()
        {
            isOnCooldown = true;
            yield return new WaitForSeconds(ConfigManager.communicationCooldown.Value);
            isOnCooldown = false;
        }
    }
}
