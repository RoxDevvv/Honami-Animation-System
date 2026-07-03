using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using HonamiAnimationSystem.Runtime.Core;

namespace HonamiAnimationSystem.Editor
{
    public sealed class HonamiLinkedAnimatorNode : Node
    {
        public HonamiLinkedAnimatorNodeBase BrainNode { get; private set; }
        public string NodeGuid => BrainNode != null ? BrainNode.guid : "";

        public Port FlowIn;
        public Port FlowOut;

        private Label _titleLabel;
        private Label _descLabel;

        public HonamiLinkedAnimatorNode(HonamiLinkedAnimatorNodeBase brainNode, HonamiLinkedAnimatorGraphView graphView)
        {
            BrainNode = brainNode;
            title = brainNode != null ? FormatNodeTitle(brainNode) : "Null Node";

            capabilities &= ~Capabilities.Resizable;
            capabilities &= ~Capabilities.Collapsible;

            titleContainer.style.display = DisplayStyle.None;

            BuildCustomUI(brainNode, graphView);

            var divider = this.Q("divider");
            if (divider != null) divider.style.display = DisplayStyle.None;

            CreateInputPorts(graphView);
            CreateOutputPorts(graphView);

            RefreshExpandedState();
            RefreshPorts();
        }

        private void CreateInputPorts(HonamiLinkedAnimatorGraphView graphView)
        {
            FlowIn = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(bool));
            FlowIn.AddManipulator(new EdgeConnector<Edge>(graphView.EdgeConnectorListener));
            FlowIn.portName = "In";
            FlowIn.portColor = new Color(0.7f, 0.9f, 1f);
            inputContainer.Add(FlowIn);
            inputContainer.style.display = DisplayStyle.Flex;
        }

        private void CreateOutputPorts(HonamiLinkedAnimatorGraphView graphView)
        {
            if (BrainNode is LinkedAnimatorConditionNode)
            {
                var truePort = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
                truePort.AddManipulator(new EdgeConnector<Edge>(graphView.EdgeConnectorListener));
                truePort.portName = "True";
                truePort.portColor = HonamiGraphStyles.Green;
                outputContainer.Add(truePort);

                var falsePort = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
                falsePort.AddManipulator(new EdgeConnector<Edge>(graphView.EdgeConnectorListener));
                falsePort.portName = "False";
                falsePort.portColor = HonamiGraphStyles.Red;
                outputContainer.Add(falsePort);
            }
            else
            {
                FlowOut = Port.Create<Edge>(Orientation.Horizontal, Direction.Output,
                    BrainNode is LinkedAnimatorSequenceNode ? Port.Capacity.Multi : Port.Capacity.Single,
                    typeof(bool));
                FlowOut.AddManipulator(new EdgeConnector<Edge>(graphView.EdgeConnectorListener));
                FlowOut.portName = "Out";
                FlowOut.portColor = new Color(0.7f, 0.9f, 1f);
                outputContainer.Add(FlowOut);
            }

            outputContainer.style.display = DisplayStyle.Flex;
        }

        private void BuildCustomUI(HonamiLinkedAnimatorNodeBase brainNode, HonamiLinkedAnimatorGraphView graphView)
        {
            string nodeCss = GetNodeCssFromType(brainNode);
            string topCss = nodeCss.Replace("honami-node-", "honami-node-top-");
            string avatarCss = nodeCss.Replace("honami-node-", "honami-node-avatar-");
            string iconCss = nodeCss.Replace("honami-node-", "honami-node-icon-");

            AddToClassList(nodeCss);

            VisualElement topBar = new VisualElement();
            topBar.AddToClassList("honami-node-top");
            topBar.AddToClassList(topCss);

            VisualElement customBody = new VisualElement();
            customBody.AddToClassList("honami-node-body");
            customBody.style.minWidth = 160;
            customBody.style.paddingTop = 6;
            customBody.style.paddingBottom = 6;

            VisualElement avatar = new VisualElement();
            avatar.AddToClassList("honami-node-avatar");
            avatar.AddToClassList(avatarCss);
            avatar.style.width = avatar.style.height = 32;

            VisualElement icon = new VisualElement();
            icon.AddToClassList("honami-node-icon");
            icon.AddToClassList(iconCss);
            icon.style.width = icon.style.height = 18;

            avatar.Add(icon);

            VisualElement textContainer = new VisualElement();
            textContainer.AddToClassList("honami-node-text-container");
            textContainer.style.marginLeft = 6;

            _titleLabel = new Label(FormatNodeTitle(brainNode));
            _titleLabel.name = "honami-title-label";
            _titleLabel.AddToClassList("honami-node-label");
            _titleLabel.style.fontSize = 13;

            _descLabel = new Label("");
            _descLabel.AddToClassList("honami-node-subtitle");
            _descLabel.style.whiteSpace = WhiteSpace.Normal;
            _descLabel.style.opacity = 0.6f;
            _descLabel.style.fontSize = 10;

            textContainer.Add(_titleLabel);
            textContainer.Add(_descLabel);

            customBody.Add(avatar);
            customBody.Add(textContainer);

            extensionContainer.style.display = DisplayStyle.Flex;
            extensionContainer.Add(topBar);
            extensionContainer.Add(customBody);

            UpdateUI();
        }

        public void SetActive(bool active)
        {
            if (active)
                AddToClassList("honami-node-active");
            else
                RemoveFromClassList("honami-node-active");
        }

        public void UpdateUI()
        {
            if (BrainNode == null) return;
            string desc = GetNodeDescription(BrainNode);
            if (!string.IsNullOrEmpty(desc))
            {
                _descLabel.text = desc;
                _descLabel.style.display = DisplayStyle.Flex;
            }
            else
            {
                _descLabel.style.display = DisplayStyle.None;
            }
            _titleLabel.text = FormatNodeTitle(BrainNode);
        }

        private static string FormatNodeTitle(HonamiLinkedAnimatorNodeBase node)
        {
            if (node == null) return "Null";
            string name = node.GetType().Name;
            name = name.Replace("LinkedAnimator", "").Replace("Node", "");
            return ObjectNames.NicifyVariableName(name);
        }

        private static string GetNodeCssFromType(HonamiLinkedAnimatorNodeBase node)
        {
            if (node is LinkedAnimatorBroadcastActionNode) return "honami-node-sequencer";
            if (node is LinkedAnimatorPlayStateNode) return "honami-node-animation";
            if (node is LinkedAnimatorSetParameterNode) return "honami-node-blend";
            if (node is LinkedAnimatorWaitNode) return "honami-node-repeater";
            if (node is LinkedAnimatorSequenceNode) return "honami-node-random";
            if (node is LinkedAnimatorConditionNode) return "honami-node-any";
            if (node is LinkedAnimatorLogNode) return "honami-node-default";
            return "honami-node-animation";
        }

        private static string GetNodeDescription(HonamiLinkedAnimatorNodeBase node)
        {
            if (node is LinkedAnimatorBroadcastActionNode ba)
                return ba.actionId != null ? $"Broadcast: {ba.actionId.name}" : "No Action Set";
            if (node is LinkedAnimatorPlayStateNode ps)
                return !string.IsNullOrEmpty(ps.stateName) ? $"Play: {ps.stateName}" : "No State Set";
            if (node is LinkedAnimatorSetParameterNode sp)
                return !string.IsNullOrEmpty(sp.parameterName) ? $"{sp.action}: {sp.parameterName}" : "No Parameter Set";
            if (node is LinkedAnimatorWaitNode w)
                return $"Wait: {w.duration}s";
            if (node is LinkedAnimatorSequenceNode seq)
                return $"{seq.children?.Count ?? 0} Steps";
            if (node is LinkedAnimatorConditionNode c)
                return !string.IsNullOrEmpty(c.parameterName) ? $"If {c.parameterName} {c.conditionType}" : "No Condition";
            if (node is LinkedAnimatorLogNode log)
                return $"Log: {log.message}";
            return null;
        }

        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);
            if (BrainNode != null)
            {
                Undo.RecordObject(BrainNode, "Move Brain Node");
                BrainNode.editorPosition = new Vector2(newPos.x, newPos.y);
                EditorUtility.SetDirty(BrainNode);
            }
        }
    }
}

