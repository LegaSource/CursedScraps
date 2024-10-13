using CursedScraps.Managers;
using GameNetcodeStuff;
using System.Collections;
using UnityEngine;

namespace CursedScraps.Behaviours.Curses
{
    internal class Diminutive
    {
        public static void ApplyDiminutive(bool enable, ref PlayerCSBehaviour playerBehaviour)
        {
            if (playerBehaviour.playerProperties == GameNetworkManager.Instance.localPlayerController)
            {
                playerBehaviour.playerProperties.localVisor.gameObject.SetActive(!enable);
            }

            if (enable)
            {
                playerBehaviour.originalScale = playerBehaviour.playerProperties.transform.localScale;
                playerBehaviour.playerProperties.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                playerBehaviour.playerProperties.movementSpeed /= ConfigManager.diminutiveSpeed.Value;
                playerBehaviour.playerProperties.grabDistance /= ConfigManager.diminutiveGrab.Value;
            }
            else
            {
                playerBehaviour.playerProperties.transform.localScale = playerBehaviour.originalScale;
                playerBehaviour.playerProperties.movementSpeed *= ConfigManager.diminutiveSpeed.Value;
                playerBehaviour.playerProperties.grabDistance *= ConfigManager.diminutiveGrab.Value;
            }
        }

        public static IEnumerator PlayerDoubleJump(PlayerCSBehaviour playerBehaviour)
        {
            PlayerControllerB player = playerBehaviour.playerProperties;
            playerBehaviour.doubleJump = true;
            player.movementAudio.PlayOneShot(StartOfRound.Instance.playerJumpSFX);
            if (player.jumpCoroutine != null)
            {
                player.StopCoroutine(player.jumpCoroutine);
            }
            player.jumpCoroutine = player.StartCoroutine(player.PlayerJump());

            yield return new WaitUntil(() => player.thisController.isGrounded);

            playerBehaviour.doubleJump = false;
        }
    }
}
