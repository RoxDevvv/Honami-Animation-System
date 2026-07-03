using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using HonamiAnimationSystem.Runtime.Core;

namespace HonamiAnimationSystem.Editor
{
    public sealed class HonamiSearchProvider : ScriptableObject, ISearchWindowProvider
    {
        private HonamiGraphView _graphView;
        private EditorWindow _window;
        private Texture2D _indentIcon;

        public void Init(HonamiGraphView graphView, EditorWindow window)
        {
            _graphView = graphView;
            _window = window;
            _indentIcon = new Texture2D(1, 1);
            _indentIcon.SetPixel(0, 0, Color.clear);
            _indentIcon.Apply();
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            List<SearchTreeEntry> tree = new();
            tree.Add(new SearchTreeGroupEntry(new GUIContent("Create Node")));

            Dictionary<string, int> groupLevels = new();

            foreach ((HonamiNodeEditorAttribute attr, Type _) in HonamiNodeEditorRegistry.AllEntries)
            {
                string[] parts = attr.MenuPath.Split('/');

                string groupPath = "";
                for (int i = 0; i < parts.Length - 1; i++)
                {
                    groupPath = i == 0 ? parts[i] : groupPath + "/" + parts[i];
                    if (!groupLevels.ContainsKey(groupPath))
                    {
                        groupLevels[groupPath] = true ? 1 : 0;
                        tree.Add(new SearchTreeGroupEntry(new GUIContent(parts[i]), i + 1));
                    }
                }

                tree.Add(new SearchTreeEntry(new GUIContent(parts[^1], _indentIcon))
                {
                    level = parts.Length,
                    userData = attr.NodeType
                });
            }

            tree.Add(new SearchTreeGroupEntry(new GUIContent("Tools"), 1));
            tree.Add(new SearchTreeEntry(new GUIContent("Decoration Group", _indentIcon)) { level = 2, userData = "Group" });
            tree.Add(new SearchTreeEntry(new GUIContent("Sticky Note", _indentIcon)) { level = 2, userData = "Note" });

            return tree;
        }

        public bool OnSelectEntry(SearchTreeEntry entry, SearchWindowContext context)
        {
            Vector2 mousePosition = _window.rootVisualElement.ChangeCoordinatesTo(_window.rootVisualElement.parent,
                context.screenMousePosition - _window.position.position);
            Vector2 graphPosition = _graphView.contentViewContainer.WorldToLocal(mousePosition);

            if (entry.userData is Type nodeType)
            {
                _graphView.CreateNewStateNodeOfType(graphPosition, nodeType);
                return true;
            }
            if (entry.userData is string tools)
            {
                if (tools == "Group") _graphView.CreateNewGroup(graphPosition);
                else if (tools == "Note") _graphView.CreateNewStickyNote(graphPosition);
                return true;
            }

            return false;
        }
    }
}
