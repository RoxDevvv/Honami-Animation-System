using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using HonamiAnimationSystem.Runtime.Core;

namespace HonamiAnimationSystem.Editor
{
    public sealed class HonamiLinkedAnimatorEventNode : Node
    {
        public HonamiLinkedAnimatorEvent BrainEvent { get; private set; }
        public string EventGuid => BrainEvent != null ? BrainEvent.guid : "";

        public Port FlowOut;

        private Label _titleLabel;

        public HonamiLinkedAnimatorEventNode(HonamiLinkedAnimatorEvent brainEvent, HonamiLinkedAnimatorGraphView graphView)
        {
            BrainEvent = brainEvent;
            title = brainEvent != null ? brainEvent.eventName : "Event";

            capabilities &= ~Capabilities.Resizable;
            capabilities &= ~Capabilities.Collapsible;

            titleContainer.style.display = DisplayStyle.None;
            inputContainer.style.display = DisplayStyle.None;
            outputContainer.style.display = DisplayStyle.None;

            BuildCustomUI(brainEvent, graphView);

            outputContainer.style.display = DisplayStyle.None;
            var divider = this.Q("divider");
            if (divider != null) divider.style.display = DisplayStyle.None;

            RefreshExpandedState();
            RefreshPorts();
        }

        private void BuildCustomUI(HonamiLinkedAnimatorEvent brainEvent, HonamiLinkedAnimatorGraphView graphView)
        {
            AddToClassList("honami-node-portal-entrance");

            VisualElement topBar = new VisualElement();
            topBar.AddToClassList("honami-node-top");
            topBar.AddToClassList("honami-node-top-portal-entrance");

            VisualElement customBody = new VisualElement();
            customBody.AddToClassList("honami-node-body");
            customBody.style.minWidth = 140;

            VisualElement avatar = new VisualElement();
            avatar.AddToClassList("honami-node-avatar");
            avatar.AddToClassList("honami-node-avatar-portal-entrance");
            avatar.style.width = avatar.style.height = 36;

            VisualElement icon = new VisualElement();
            icon.AddToClassList("honami-node-icon");
            icon.AddToClassList("honami-node-icon-portal-entrance");
            icon.style.width = icon.style.height = 20;

            avatar.Add(icon);

            VisualElement textContainer = new VisualElement();
            textContainer.AddToClassList("honami-node-text-container");
            textContainer.style.marginLeft = 8;

            _titleLabel = new Label(brainEvent != null ? brainEvent.eventName : "Event");
            _titleLabel.name = "honami-title-label";
            _titleLabel.AddToClassList("honami-node-label");
            _titleLabel.style.fontSize = 14;

            Label subtitleLabel = new Label("EVENT ENTRY");
            subtitleLabel.AddToClassList("honami-node-subtitle");
            subtitleLabel.style.opacity = 0.5f;

            textContainer.Add(_titleLabel);
            textContainer.Add(subtitleLabel);

            customBody.Add(avatar);
            customBody.Add(textContainer);

            extensionContainer.style.display = DisplayStyle.Flex;
            extensionContainer.Add(topBar);
            extensionContainer.Add(customBody);

            // Add a small footer for the port
            VisualElement portArea = new VisualElement();
            portArea.style.flexDirection = FlexDirection.Row;
            portArea.style.justifyContent = Justify.FlexEnd;
            portArea.style.backgroundColor = new Color(0, 0, 0, 0.2f);
            portArea.style.paddingRight = 4;
            portArea.style.borderBottomLeftRadius = portArea.style.borderBottomRightRadius = 4;

            Label flowLbl = new Label("Flow");
            flowLbl.style.fontSize = 9;
            flowLbl.style.color = new Color(0.7f, 0.7f, 0.7f);
            flowLbl.style.alignSelf = Align.Center;
            portArea.Add(flowLbl);

            FlowOut = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(bool));
            FlowOut.AddManipulator(new EdgeConnector<Edge>(graphView.EdgeConnectorListener));
            FlowOut.portName = "";
            FlowOut.portColor = new Color(1f, 0.4f, 0.4f);
            portArea.Add(FlowOut);

            extensionContainer.Add(portArea);
        }

        public void UpdateTitle()
        {
            if (BrainEvent != null && _titleLabel != null)
                _titleLabel.text = BrainEvent.eventName;
        }

        public void SetActive(bool active)
        {
            if (active)
                AddToClassList("honami-node-active");
            else
                RemoveFromClassList("honami-node-active");
        }

        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);
            if (BrainEvent != null)
            {
                Undo.RecordObject(BrainEvent, "Move Brain Event");
                BrainEvent.editorPosition = new Vector2(newPos.x, newPos.y);
                EditorUtility.SetDirty(BrainEvent);
            }
        }
    }
}

