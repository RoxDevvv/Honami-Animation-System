namespace HonamiAnimationSystem.Runtime.Core
{
    public partial class HonamiAnimator
    {
        private void InitializeGraph(bool keepGraph = false, bool forceRebuildStates = false) => HonamiGraphBuilder.InitializeGraph(this, keepGraph, forceRebuildStates);
        private void BuildMetadataForState(HonamiState state, int index) => HonamiGraphBuilder.BuildMetadataForState(this, state, index);
        private void BakePortals() => HonamiGraphBuilder.BakePortals(this);
        private void PlayDefaultStates() => HonamiGraphBuilder.PlayDefaultStates(this);
    }
}
