using UnityEngine;
using UnityEngine.Playables;

namespace HonamiAnimationSystem.Runtime.Core
{
    /// <summary>
    /// Sub-node that applies procedural Perlin jitter to playable speed.
    /// </summary>
    public sealed class HonamiJitterSpeedSubNode : HonamiSubNodeBase
    {
        [Header("Settings")]
        public float scale = 0.15f;
        public float frequency = 1.2f;

        public override void UpdateRuntime(in HonamiExecutionContext ctx)
        {
            if (!ctx.Playable.IsValid())
            {
                return;
            }

            float time = (float)ctx.Playable.GetTime() + ctx.StateIndex * 133.7f;
            float noise = Mathf.PerlinNoise(time * frequency, 0f) * 2f - 1f;
            float modifier = 1f + noise * scale;
            float baseSpeed = ctx.State.speed * (ctx.State.isReversed ? -1f : 1f);

            ctx.Playable.SetSpeed(baseSpeed * modifier);
        }
    }
}
