using GameNetcodeStuff;
using System.Collections.Generic;
using UnityEngine;

namespace CursedScraps.Behaviours
{
    internal class ObjectCSBehaviour : MonoBehaviour
    {
        public GrabbableObject objectProperties;
        public List<CurseEffect> curseEffects = new List<CurseEffect>();
        public GameObject particleEffect;
        // COOP
        public PlayerControllerB playerOwner;
        public GrabbableObject mainPart;
        public bool isSplitted = false;
    }
}
