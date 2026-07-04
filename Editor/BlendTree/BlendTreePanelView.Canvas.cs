#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace HonamiAnimationSystem.Editor.BlendTree
{
    internal sealed partial class BlendTreePanelView
    {
        private BlendTreeGraphCanvas _canvas;
        private readonly List<BlendTreeMotionCard> _motionNodes = new();
        private BlendTreeOutputCard _outputNode;
        private BlendTreeGraphEdges _edges;

        private void BuildCanvasArea()
        {
            _canvas = new BlendTreeGraphCanvas(_state, this);
            _mainArea.Add(_canvas);

            RegenerateGraph();

            if (_state.HasValidViewTransform)
            {
                _canvas.UpdateTransform(new Vector3(_state.ViewPosition.x, _state.ViewPosition.y, 0f), _state.ViewScale);
            }
            else
            {
                _canvas.FrameAllWhenReady();
            }
        }

        private void RegenerateGraph()
        {
            if (_canvas == null) return;

            _canvas.ContentContainer.Clear();
            _motionNodes.Clear();
            _outputNode = null;
            _edges = null;

            if (_state.Node == null) return;

            _edges = new BlendTreeGraphEdges(_state);
            _canvas.ContentContainer.Add(_edges);

            _outputNode = new BlendTreeOutputCard(_state, OnNodePositionChanged);
            Vector2 outPos = _state.HasNodePositions
                ? _state.OutputNodePosition
                : new Vector2(-BlendTreeTheme.OutputNodeWidth * 0.5f, 40f);
            _outputNode.style.left = outPos.x;
            _outputNode.style.top = outPos.y;
            _state.OutputNodePosition = outPos;
            _canvas.ContentContainer.Add(_outputNode);

            var motions = _state.Node.blendMotions;
            if (motions == null) return;
            var motionsProp = _state.NodeSO.FindProperty("blendMotions");

            for (int i = 0; i < motions.Count; i++)
            {
                var motionProp = motionsProp.GetArrayElementAtIndex(i);
                var nodeView = new BlendTreeMotionCard(motionProp, motions[i], i, motions.Count, _state, OnNodePositionChanged);

                Vector2 nodePos = _state.HasNodePositions && _state.NodePositions.TryGetValue(i, out var saved)
                    ? saved
                    : ComputeTreePosition(i, motions.Count);
                _state.NodePositions[i] = nodePos;

                nodeView.style.left = nodePos.x;
                nodeView.style.top = nodePos.y;

                _canvas.ContentContainer.Add(nodeView);
                _motionNodes.Add(nodeView);
            }

            _state.HasNodePositions = true;
        }

        private static Vector2 ComputeTreePosition(int index, int count)
        {
            float horizontalSpacing = 60f;
            float totalWidth = (count * BlendTreeTheme.NodeWidth) + ((count - 1) * horizontalSpacing);
            float startX = -(totalWidth * 0.5f);
            float x = startX + index * (BlendTreeTheme.NodeWidth + horizontalSpacing);
            return new Vector2(x, 240f);
        }

        private void OnNodePositionChanged()
        {
            if (_outputNode != null)
                _state.OutputNodePosition = new Vector2(_outputNode.style.left.value.value, _outputNode.style.top.value.value);

            for (int i = 0; i < _motionNodes.Count; i++)
            {
                var node = _motionNodes[i];
                _state.NodePositions[i] = new Vector2(node.style.left.value.value, node.style.top.value.value);
            }

            _edges?.MarkDirtyRepaint();
        }

        public void AutoLayout()
        {
            if (_state.Node == null || _motionNodes.Count == 0) return;

            int count = _motionNodes.Count;

            for (int i = 0; i < count; i++)
            {
                var pos = ComputeTreePosition(i, count);
                _motionNodes[i].style.left = pos.x;
                _motionNodes[i].style.top = pos.y;
                _state.NodePositions[i] = pos;
            }

            var outPos = new Vector2(-BlendTreeTheme.OutputNodeWidth * 0.5f, 40f);
            if (_outputNode != null)
            {
                _outputNode.style.left = outPos.x;
                _outputNode.style.top = outPos.y;
            }
            _state.OutputNodePosition = outPos;

            OnNodePositionChanged();
            _canvas.schedule.Execute(FrameAll).ExecuteLater(50);
        }

        public void FrameAll()
        {
            _canvas?.FrameAll();
        }

        public void OnCanvasZoomChanged(float scale)
        {
            _zoomSlider?.SetValueWithoutNotify(scale);
        }

        private void UpdateNodeWeights()
        {
            if (_state.Node == null) return;
            _state.CalculateWeights(_state.PreviewValue);

            _state.GetSliderRange(out float min, out float max);

            for (int i = 0; i < _motionNodes.Count && i < _state.Weights.Length; i++)
            {
                _motionNodes[i].UpdateWeight(_state.Weights[i]);
            }

            _outputNode?.UpdatePreviewValue(_state.PreviewValue, min, max);

            _edges?.MarkDirtyRepaint();
        }
    }
}
#endif
