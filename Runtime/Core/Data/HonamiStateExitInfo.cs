namespace HonamiAnimationSystem.Runtime.Core
{
    /// <summary>
    /// Describes why a Honami state stopped being active.
    /// </summary>
    public enum HonamiStateExitReason
    {
        Completed,
        Transition,
        Interrupted,
        Stopped,
        Restarted,
        ControllerChanged
    }

    /// <summary>
    /// Immutable state-exit payload raised by <see cref="HonamiAnimator"/>.
    /// </summary>
    public readonly struct HonamiStateExitInfo
    {
        public readonly string StateName;
        public readonly string StateGuid;
        public readonly int StateIndex;
        public readonly int Layer;
        public readonly int PortIndex;
        public readonly HonamiStateExitReason Reason;
        public readonly string NextStateName;
        public readonly string NextStateGuid;
        public readonly int NextStateIndex;

        public bool WasCompleted => Reason == HonamiStateExitReason.Completed;
        public bool WasInterrupted => Reason != HonamiStateExitReason.Completed;

        public HonamiStateExitInfo(
            string stateName,
            string stateGuid,
            int stateIndex,
            int layer,
            int portIndex,
            HonamiStateExitReason reason,
            string nextStateName,
            string nextStateGuid,
            int nextStateIndex)
        {
            StateName = stateName;
            StateGuid = stateGuid;
            StateIndex = stateIndex;
            Layer = layer;
            PortIndex = portIndex;
            Reason = reason;
            NextStateName = nextStateName;
            NextStateGuid = nextStateGuid;
            NextStateIndex = nextStateIndex;
        }
    }
}
