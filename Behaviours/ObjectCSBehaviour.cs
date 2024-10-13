using CursedScraps.Behaviours.Curses;
using System.Collections.Generic;
using UnityEngine;

namespace CursedScraps.Behaviours
{
    internal class ObjectCSBehaviour : MonoBehaviour
    {
        public GrabbableObject objectProperties;
        public List<CurseEffect> curseEffects = new List<CurseEffect>();
        public GameObject particleEffect;
    }
}
