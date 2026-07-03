using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace HonamiAnimationSystem.Runtime.Core
{
    /// <summary>
    /// Sub-node that applies procedural Perlin jitter to a state's mixer weight.
    /// </summary>
    public sealed class HonamiJitterWeightSubNode : HonamiSubNodeBase
    {
        [Header("Settings")]
        public float scale = 0.1f;
        public float frequency = 2.0f;

        public override void UpdateRuntime(in HonamiExecutionContext ctx)
        {
            if (!ctx.LayerMixer.IsValid() || !ctx.Playable.IsValid())
            {
                return;
            }

            float time = (float)ctx.Playable.GetTime() + ctx.StateIndex * 133.7f;
            float noise = Mathf.PerlinNoise(0f, time * frequency) * 2f - 1f;
            float modifier = 1f + noise * scale;
            float currentWeight = ctx.LayerMixer.GetInputWeight(ctx.PortIndex);

            if (currentWeight > 0f)
            {
                ctx.LayerMixer.SetInputWeight(ctx.PortIndex, Mathf.Clamp01(currentWeight * modifier));
            }
        }
    }
}
