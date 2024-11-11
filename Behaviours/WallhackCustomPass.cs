using System.Collections.Generic;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine;

namespace CursedScraps.Behaviours
{
    public class WallhackCustomPass : CustomPass
    {
        public Material wallhackMaterial;
        private List<Renderer> targetRenderers = new List<Renderer>();

        public void SetTargetRenderers(Renderer[] renderers, Material material)
        {
            targetRenderers.Clear();
            targetRenderers.AddRange(renderers);
            wallhackMaterial = material;
        }

        public void ClearTargetRenderers()
        {
            targetRenderers.Clear();
        }

        public override void Execute(CustomPassContext ctx)
        {
            if (targetRenderers == null || wallhackMaterial == null)
            {
                return;
            }

            foreach (Renderer renderer in targetRenderers)
            {
                if (renderer == null || renderer.sharedMaterials == null) continue;

                for (int i = 0; i < renderer.sharedMaterials.Length; i++)
                {
                    ctx.cmd.DrawRenderer(renderer, wallhackMaterial);
                }
            }
        }
    }
}
