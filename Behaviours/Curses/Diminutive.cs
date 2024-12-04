using CursedScraps.Managers;
using GameNetcodeStuff;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace CursedScraps.Behaviours.Curses
{
    public class Diminutive
    {
        public static void ApplyDiminutive(bool enable, ref PlayerCSBehaviour playerBehaviour)
        {
            if (playerBehaviour.playerProperties == GameNetworkManager.Instance.localPlayerController)
                playerBehaviour.playerProperties.localVisor.gameObject.SetActive(!enable);

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

        public static bool IsDiminutive(PlayerCSBehaviour playerBehaviour)
        {
            if (playerBehaviour != null && playerBehaviour.activeCurses.Any(c => c.CurseName.Equals(Constants.DIMINUTIVE)))
                return true;
            return false;
        }

        public static bool PreventJump(PlayerCSBehaviour playerBehaviour)
        {
            PlayerControllerB player = playerBehaviour.playerProperties;
            if (IsDiminutive(playerBehaviour)
                && !player.isExhausted
                && player.playerBodyAnimator.GetBool("Jumping")
                && !playerBehaviour.doubleJump)
            {
                player.StartCoroutine(PlayerDoubleJump(playerBehaviour));
                return true;
            }
            return false;
        }

        public static IEnumerator PlayerDoubleJump(PlayerCSBehaviour playerBehaviour)
        {
            PlayerControllerB player = playerBehaviour.playerProperties;

            playerBehaviour.doubleJump = true;
            player.movementAudio.PlayOneShot(StartOfRound.Instance.playerJumpSFX);
            if (player.jumpCoroutine != null)
                player.StopCoroutine(player.jumpCoroutine);
            player.jumpCoroutine = player.StartCoroutine(player.PlayerJump());

            yield return new WaitUntil(() => player.thisController.isGrounded);

            playerBehaviour.doubleJump = false;
        }

        public static void PlayerCollision(PlayerCSBehaviour playerBehaviour)
        {
            PlayerControllerB player = playerBehaviour.playerProperties;
            if (player == GameNetworkManager.Instance.localPlayerController
                && !IsDiminutive(playerBehaviour))
            {
                foreach (Collider collider in Physics.OverlapSphere(player.transform.position, 0.65f, StartOfRound.Instance.playersMask))
                {
                    PlayerCSBehaviour pushedPlayerBehaviour = collider.GetComponent<PlayerControllerB>()?.GetComponent<PlayerCSBehaviour>();
                    if (IsDiminutive(pushedPlayerBehaviour)
                        && pushedPlayerBehaviour.playerProperties != player)
                    {
                        if (player.isFallingFromJump)
                        {
                            CursedScrapsNetworkManager.Instance.KillPlayerServerRpc((int)pushedPlayerBehaviour.playerProperties.playerClientId, Vector3.zero, true, (int)CauseOfDeath.Crushing);
                        }
                        else
                        {
                            Vector3 direction = (pushedPlayerBehaviour.playerProperties.transform.position - player.thisController.transform.position).normalized;
                            CursedScrapsNetworkManager.Instance.PushPlayerServerRpc((int)pushedPlayerBehaviour.playerProperties.playerClientId, direction * player.thisController.velocity.magnitude * 0.2f);
                        }
                    }
                }
            }
        }

        // sign: vrai pour multiplication, faux pour soustraction
        public static void ScaleObject(PlayerCSBehaviour playerBehaviour, GrabbableObject grabbableObject, bool sign)
        {
            if (IsDiminutive(playerBehaviour))
            {
                float scaleFactor = sign ? 5f : 0.2f;
                grabbableObject.transform.localScale = grabbableObject.originalScale * scaleFactor;
            }
        }
    }
}
