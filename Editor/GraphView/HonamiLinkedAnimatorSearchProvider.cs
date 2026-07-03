using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using HonamiAnimationSystem.Runtime.Core;

namespace HonamiAnimationSystem.Editor
{
    public sealed class HonamiLinkedAnimatorSearchProvider : ScriptableObject, ISearchWindowProvider
    {
        private HonamiLinkedAnimatorGraphView _graphView;
        private EditorWindow _window;
        private Texture2D _indentIcon;
        public Port SourcePort { get; set; }

        private static readonly (string path, Type type)[] LinkedNodeTypes = new[]
        {
            ("Actions/Broadcast Action", typeof(LinkedAnimatorBroadcastActionNode)),
            ("Actions/Play State",       typeof(LinkedAnimatorPlayStateNode)),
            ("Actions/Set Parameter",    typeof(LinkedAnimatorSetParameterNode)),
            ("Flow/Sequence",            typeof(LinkedAnimatorSequenceNode)),
            ("Flow/Condition",           typeof(LinkedAnimatorConditionNode)),
            ("Flow/Wait",                typeof(LinkedAnimatorWaitNode)),
            ("Debug/Log",                typeof(LinkedAnimatorLogNode)),
        };

        public void Init(HonamiLinkedAnimatorGraphView graphView, EditorWindow window)
        {
            _graphView = graphView;
            _window = window;
            _indentIcon = new Texture2D(1, 1);
            _indentIcon.SetPixel(0, 0, Color.clear);
            _indentIcon.Apply();
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var tree = new List<SearchTreeEntry>();
            tree.Add(new SearchTreeGroupEntry(new GUIContent("Add Node")));

            tree.Add(new SearchTreeEntry(new GUIContent("Event Trigger", _indentIcon))
            {
                level = 1,
                userData = "Event"
            });

            var groups = new HashSet<string>();

            foreach (var (path, type) in LinkedNodeTypes)
            {
                string[] parts = path.Split('/');

                string groupPath = "";
                for (int i = 0; i < parts.Length - 1; i++)
                {
                    groupPath = i == 0 ? parts[i] : groupPath + "/" + parts[i];
                    if (groups.Add(groupPath))
                        tree.Add(new SearchTreeGroupEntry(new GUIContent(parts[i]), i + 1));
                }

                tree.Add(new SearchTreeEntry(new GUIContent(parts[^1], _indentIcon))
                {
                    level = parts.Length,
                    userData = type
                });
            }

            var customTypes = TypeCache.GetTypesDerivedFrom<HonamiLinkedAnimatorNodeBase>()
                .Where(t => !t.IsAbstract && !LinkedNodeTypes.Any(b => b.type == t));

            bool hasCustomHeader = false;
            foreach (var ct in customTypes)
            {
                if (!hasCustomHeader)
                {
                    tree.Add(new SearchTreeGroupEntry(new GUIContent("Custom"), 1));
                    hasCustomHeader = true;
                }

                string displayName = ct.Name.Replace("Linked", "").Replace("Node", "");
                tree.Add(new SearchTreeEntry(new GUIContent(displayName, _indentIcon))
                {
                    level = 2,
                    userData = ct
                });
            }

            return tree;
        }

        public bool OnSelectEntry(SearchTreeEntry entry, SearchWindowContext context)
        {
            Vector2 mousePosition = _window.rootVisualElement.ChangeCoordinatesTo(
                _window.rootVisualElement.parent,
                context.screenMousePosition - _window.position.position);
            Vector2 graphPosition = _graphView.contentViewContainer.WorldToLocal(mousePosition);

            if (entry.userData is string s && s == "Event")
            {
                var newNode = _graphView.CreateBrainEvent(graphPosition);
                if (SourcePort != null && newNode != null)
                {
                    _graphView.AutoConnectPorts(SourcePort, newNode);
                }
                SourcePort = null;
                return true;
            }

            if (entry.userData is Type nodeType)
            {
                var newNode = _graphView.CreateBrainNode(graphPosition, nodeType);
                if (SourcePort != null && newNode != null)
                {
                    _graphView.AutoConnectPorts(SourcePort, newNode);
                }
                SourcePort = null;
                return true;
            }

            SourcePort = null;
            return false;
        }
    }
}

