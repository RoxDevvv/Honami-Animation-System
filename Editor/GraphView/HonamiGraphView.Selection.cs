using UnityEditor.Experimental.GraphView;
using HonamiAnimationSystem.Runtime.Core;

namespace HonamiAnimationSystem.Editor
{
    public partial class HonamiGraphView
    {
        public override void AddToSelection(ISelectable selectable)
        {
            base.AddToSelection(selectable);
            NotifySelectionChanged();
        }

        public override void RemoveFromSelection(ISelectable selectable)
        {
            base.RemoveFromSelection(selectable);
            NotifySelectionChanged();
        }

        public override void ClearSelection()
        {
            base.ClearSelection();
            NotifySelectionChanged();
        }

        private void NotifySelectionChanged()
        {
            ClearSubNodeSelection();
            if (selection == null) return;

            if (selection.Count == 0)
            {
                OnDeselected?.Invoke();
                return;
            }

            var first = selection[0];
            if (first == null) return;

            if (first is HonamiNode node)
            {
                OnNodeSelected?.Invoke(node);
            }
            else if (first is HonamiTransitionEdge edge && edge.userData is HonamiTransition transition)
            {
                OnEdgeSelected?.Invoke(edge, transition);
            }
            else
            {
                OnDeselected?.Invoke();
            }
        }
    }
}
