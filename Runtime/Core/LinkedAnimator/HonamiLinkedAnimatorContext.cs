using System.Collections.Generic;

namespace HonamiAnimationSystem.Runtime.Core
{
    public readonly struct HonamiLinkedAnimatorContext
    {
        public readonly HonamiLinkedAnimator Brain;
        public readonly IReadOnlyCollection<HonamiAnimator> LinkedAnimators;
        public readonly float DeltaTime;
        public readonly float EventTime;

        public HonamiLinkedAnimatorContext(
            HonamiLinkedAnimator brain,
            IReadOnlyCollection<HonamiAnimator> linkedAnimators,
            float deltaTime,
            float eventTime)
        {
            Brain = brain;
            LinkedAnimators = linkedAnimators;
            DeltaTime = deltaTime;
            EventTime = eventTime;
        }
    }
}

