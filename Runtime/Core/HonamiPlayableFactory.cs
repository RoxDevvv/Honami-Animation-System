using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace HonamiAnimationSystem.Runtime.Core
{
    public static class HonamiPlayableFactory
    {
        public static Playable CreateAndConnect(PlayableGraph graph, HonamiRuntimeController controller, HonamiState state, AnimationMixerPlayable parentMixer, int portIndex, System.Func<PlayableGraph, Playable> mirrorFactory = null)
        {
            parentMixer.SetInputWeight(portIndex, 0f);
            var node = controller.GetActiveNode(state);
            if (node == null || node.IsVirtual) return Playable.Null;

            Playable root = node.CreatePlayable(graph, state, mirrorFactory);
            if (root.IsValid()) graph.Connect(root, 0, parentMixer, portIndex);

            return root;
        }
    }
}
