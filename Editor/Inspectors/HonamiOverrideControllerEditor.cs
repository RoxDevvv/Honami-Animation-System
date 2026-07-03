using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using HonamiAnimationSystem.Runtime.Core;

namespace HonamiAnimationSystem.Editor.Inspectors
{
    [CustomEditor(typeof(HonamiOverrideController))]
    public sealed class HonamiOverrideControllerEditor : UnityEditor.Editor
    {
        private SerializedProperty _parentControllerProp;
        private SerializedProperty _nodeOverridesProp;
        private SerializedProperty _additionalLayersProp;
        private SerializedProperty _additionalParametersProp;

        private void OnEnable()
        {
            _parentControllerProp = serializedObject.FindProperty("parentController");
            _nodeOverridesProp = serializedObject.FindProperty("nodeOverrides");
            _additionalLayersProp = serializedObject.FindProperty("additionalLayers");
            _additionalParametersProp = serializedObject.FindProperty("additionalParameters");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var overrideController = target as HonamiOverrideController;

            if (GUILayout.Button("Open Animator Graph", GUILayout.Height(30)))
            {
                HonamiGraphWindow.OpenWindow();
                HonamiGraphWindow.LoadController(overrideController);
            }

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(_parentControllerProp);

            if (overrideController.parentController == null)
            {
                EditorGUILayout.HelpBox("Please assign a parent Honami Controller to override it.", MessageType.Info);
                serializedObject.ApplyModifiedProperties();
                return;
            }

            var parent = overrideController.parentController as HonamiController;
            if (parent == null)
            {
                EditorGUILayout.HelpBox("Parent must be an actual HonamiController.", MessageType.Warning);
                serializedObject.ApplyModifiedProperties();
                return;
            }

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(_additionalLayersProp, true);
            EditorGUILayout.PropertyField(_additionalParametersProp, true);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("State Overrides", EditorStyles.boldLabel);

            if (parent.states == null || parent.states.Count == 0)
            {
                EditorGUILayout.HelpBox("Parent controller has no states.", MessageType.Info);
                serializedObject.ApplyModifiedProperties();
                return;
            }


            SyncOverridesToParent(overrideController, parent);

            for (int i = 0; i < parent.states.Count; i++)
            {
                var state = parent.states[i];
                if (state == null) continue;

                var overrideIndex = overrideController.nodeOverrides.FindIndex(o => o.stateGuid == state.guid);
                if (overrideIndex == -1) continue;

                var overrideProp = _nodeOverridesProp.GetArrayElementAtIndex(overrideIndex);
                var overrideNodeProp = overrideProp.FindPropertyRelative("overrideNode");

                bool isOverridden = overrideNodeProp.objectReferenceValue != null;

                if (isOverridden)
                {
                    GUI.backgroundColor = new Color(0.8f, 1f, 0.8f);
                }

                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                GUI.backgroundColor = Color.white;

                GUIStyle labelStyle = new GUIStyle(EditorStyles.label);
                if (isOverridden)
                {
                    labelStyle.fontStyle = FontStyle.Bold;
                }

                string nodeTypeStr = state.node != null ? state.node.GetType().Name.Replace("Honami", "").Replace("Node", "") : "None";
                EditorGUILayout.LabelField($"{state.stateName} ({nodeTypeStr})", labelStyle, GUILayout.Width(EditorGUIUtility.labelWidth));

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(overrideNodeProp, GUIContent.none);
                if (EditorGUI.EndChangeCheck())
                {

                }

                if (isOverridden)
                {
                    if (GUILayout.Button("X", GUILayout.Width(25)))
                    {
                        var oldNode = overrideNodeProp.objectReferenceValue;
                        if (oldNode != null && AssetDatabase.GetAssetPath(oldNode) == AssetDatabase.GetAssetPath(overrideController))
                        {
                            AssetDatabase.RemoveObjectFromAsset(oldNode);
                            DestroyImmediate(oldNode, true);
                        }
                        overrideNodeProp.objectReferenceValue = null;
                        EditorUtility.SetDirty(overrideController);
                        HonamiGraphView.DeferredSave();

                    }
                }
                else
                {
                    if (GUILayout.Button("Create", GUILayout.Width(60)))
                    {
                        ShowCreateMenu(overrideController, overrideIndex, state.node);
                    }
                }

                EditorGUILayout.EndHorizontal();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void SyncOverridesToParent(HonamiOverrideController overrideController, HonamiController parent)
        {
            bool modified = false;


            for (int i = overrideController.nodeOverrides.Count - 1; i >= 0; i--)
            {
                var guid = overrideController.nodeOverrides[i].stateGuid;
                if (!parent.states.Any(s => s != null && s.guid == guid))
                {

                    var obsoleteNode = overrideController.nodeOverrides[i].overrideNode;
                    if (obsoleteNode != null && AssetDatabase.GetAssetPath(obsoleteNode) == AssetDatabase.GetAssetPath(overrideController))
                    {
                        AssetDatabase.RemoveObjectFromAsset(obsoleteNode);
                        DestroyImmediate(obsoleteNode, true);
                    }
                    overrideController.nodeOverrides.RemoveAt(i);
                    modified = true;
                }
            }


            foreach (var state in parent.states)
            {
                if (state == null) continue;
                if (!overrideController.nodeOverrides.Any(o => o.stateGuid == state.guid))
                {
                    overrideController.nodeOverrides.Add(new HonamiNodeOverride { stateGuid = state.guid, overrideNode = null });
                    modified = true;
                }
            }

            if (modified)
            {
                EditorUtility.SetDirty(overrideController);
                serializedObject.Update();
            }
        }

        private void ShowCreateMenu(HonamiOverrideController controller, int overrideIndex, HonamiNodeBase originalNode)
        {
            var menu = new GenericMenu();


            var nodeTypes = TypeCache.GetTypesDerivedFrom<HonamiNodeBase>()
                .Where(t => !t.IsAbstract && !t.IsGenericType)
                .OrderBy(t => t.Name).ToList();

            foreach (var type in nodeTypes)
            {
                string typeName = type.Name.Replace("Honami", "").Replace("Node", "");
                menu.AddItem(new GUIContent(typeName), false, () =>
                {
                    CreateNodeOverride(controller, overrideIndex, type);
                });
            }

            menu.ShowAsContext();
        }

        private void CreateNodeOverride(HonamiOverrideController controller, int overrideIndex, System.Type nodeType)
        {
            var newNode = ScriptableObject.CreateInstance(nodeType) as HonamiNodeBase;
            if (newNode == null) return;

            newNode.name = $"{nodeType.Name}_{System.Guid.NewGuid().ToString().Substring(0, 5)}";


            var parentStateGuid = controller.nodeOverrides[overrideIndex].stateGuid;
            var parent = controller.parentController as HonamiController;
            if (parent != null)
            {
                var parentState = parent.states.Find(x => x != null && x.guid == parentStateGuid);
                if (parentState != null)
                    newNode.name = $"Override_{parentState.stateName}";
            }

            AssetDatabase.AddObjectToAsset(newNode, controller);


            var oldNode = controller.nodeOverrides[overrideIndex].overrideNode;
            if (oldNode != null && AssetDatabase.GetAssetPath(oldNode) == AssetDatabase.GetAssetPath(controller))
            {
                AssetDatabase.RemoveObjectFromAsset(oldNode);
                DestroyImmediate(oldNode, true);
            }


            var item = controller.nodeOverrides[overrideIndex];
            item.overrideNode = newNode;
            controller.nodeOverrides[overrideIndex] = item;

            EditorUtility.SetDirty(controller);
            HonamiGraphView.DeferredSave();

            serializedObject.Update();
        }
    }
}
