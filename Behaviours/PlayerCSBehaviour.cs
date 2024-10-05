using GameNetcodeStuff;
using System.Collections.Generic;
using UnityEngine;

namespace CursedScraps.Behaviours
{
    internal class PlayerCSBehaviour : MonoBehaviour
    {
        public PlayerControllerB playerProperties;
        public List<CurseEffect> activeCurses = new List<CurseEffect>();
        public List<CurseEffect> actionsBlockedBy = new List<CurseEffect>();
        // DIMINUTIVE
        public Vector3 originalScale;
        public bool doubleJump = false;
        // EXPLORATION
        public EntranceTeleport targetDoor;
        public bool isRendered = false;
        // COOP CURSES
        public PlayerControllerB coopPlayer;
        // COMMUNICATION
        public GrabbableObject trackedScrap;
        // SYNCHRONIZATION
        public Vector2 inputLookVector = Vector2.zero;
    }
}
