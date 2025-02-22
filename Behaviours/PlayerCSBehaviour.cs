﻿using CursedScraps.Behaviours.Curses;
using GameNetcodeStuff;
using System.Collections.Generic;
using UnityEngine;

namespace CursedScraps.Behaviours
{
    public class PlayerCSBehaviour : MonoBehaviour
    {
        public PlayerControllerB playerProperties;
        public List<CurseEffect> activeCurses = new List<CurseEffect>();
        public List<CurseEffect> actionsBlockedBy = new List<CurseEffect>();
        // INHIBITION
        public string blockedAction;
        // DIMINUTIVE
        public Vector3 originalScale;
        public bool doubleJump = false;
        // EXPLORATION
        public EntranceTeleport targetDoor;
        public bool isRendered = false;
        // COMMUNICATION
        public GrabbableObject trackedItem;
        public bool canEscape = false;
    }
}
