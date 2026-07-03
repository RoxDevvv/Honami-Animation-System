using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using HonamiAnimationSystem.Runtime.Core;

namespace HonamiAnimationSystem.Editor
{
    public sealed class HonamiLeftPanel
    {
        public VisualElement Root { get; }

        private readonly HonamiGraphWindow _window;
        private VisualElement _layersContent;
        private VisualElement _paramsContent;
        private readonly List<(int layerIndex, VisualElement row, VisualElement props, Label nameLabel)> _layerRows = new();
        private int _currentTab = 0;
        private VisualElement _tabBar;
        private readonly List<Button> _tabButtons = new();

        private static readonly Dictionary<int, (Runtime.Core.HonamiAnimator animator, bool value)> ForcedBools = new();
        private static bool _updateHooked = false;

        private static void GlobalForceUpdate()
        {
            if (!EditorApplication.isPlaying)
            {
                ForcedBools.Clear();
                if (_updateHooked)
                {
                    EditorApplication.update -= GlobalForceUpdate;
                    _updateHooked = false;
                }
                return;
            }

            foreach (var kvp in ForcedBools)
            {
                if (kvp.Value.animator != null)
                {
                    kvp.Value.animator.Parameters.SetBool(kvp.Key, kvp.Value.value);
                }
            }
        }

        public HonamiLeftPanel(HonamiGraphWindow window)
        {
            _window = window;
            Root = BuildShell();
        }

        private VisualElement BuildShell()
        {
            var root = new VisualElement();
            root.style.flexGrow = 1;
            root.style.flexDirection = FlexDirection.Column;

            _tabBar = new VisualElement();
            _tabBar.style.flexDirection = FlexDirection.Row;
            _tabBar.style.backgroundColor = new Color(0.14f, 0.14f, 0.15f);
            _tabBar.style.height = 22;
            root.Add(_tabBar);

            MakeTabButton("Layers", 0, _tabBar);
            MakeTabButton("Parameters", 1, _tabBar);

            var scroll = new ScrollView();
            scroll.style.flexGrow = 1;
            root.Add(scroll);

            var pad = new VisualElement();
            pad.style.paddingLeft = pad.style.paddingRight =
            pad.style.paddingTop = pad.style.paddingBottom = 6;
            scroll.Add(pad);

            _layersContent = new VisualElement();
            _paramsContent = new VisualElement();
            _paramsContent.style.display = DisplayStyle.None;

            pad.Add(_layersContent);
            pad.Add(_paramsContent);

            return root;
        }

        private Button MakeTabButton(string label, int tabIdx, VisualElement parent)
        {
            var btn = new Button(() => SelectTab(tabIdx)) { text = label };
            btn.style.flexGrow = 1;
            btn.style.height = 22;
            btn.style.borderTopWidth = btn.style.borderBottomWidth =
            btn.style.borderLeftWidth = btn.style.borderRightWidth = 0;
            btn.style.borderTopLeftRadius = btn.style.borderTopRightRadius =
            btn.style.borderBottomLeftRadius = btn.style.borderBottomRightRadius = 0;
            btn.style.unityFontStyleAndWeight = FontStyle.Bold;
            btn.name = $"tab-btn-{tabIdx}";
            parent.Add(btn);
            _tabButtons.Add(btn);
            UpdateTabButtonStyle(btn, tabIdx == _currentTab);
            return btn;
        }

        private void SelectTab(int idx)
        {
            _currentTab = idx;
            _layersContent.style.display = idx == 0 ? DisplayStyle.Flex : DisplayStyle.None;
            _paramsContent.style.display = idx == 1 ? DisplayStyle.Flex : DisplayStyle.None;

            for (int i = 0; i < _tabButtons.Count; i++)
                UpdateTabButtonStyle(_tabButtons[i], i == idx);
        }

        private static void UpdateTabButtonStyle(Button btn, bool active)
        {
            if (btn == null) return;
            btn.style.backgroundColor = active
                ? HonamiGraphStyles.Accent
                : new Color(0.18f, 0.18f, 0.19f);
            btn.style.color = active ? Color.white : new Color(0.8f, 0.8f, 0.8f);
        }

        public void Rebuild()
        {
            RebuildLayers();
            RebuildParams();
        }


        public void UpdateLayerHighlight()
        {
            for (int i = 0; i < _layerRows.Count; i++)
            {
                var (layerIndex, row, props, _) = _layerRows[i];
                bool sel = _window.CurrentLayerIndex == layerIndex;
                row.style.backgroundColor = sel
                    ? HonamiGraphStyles.AccentDim
                    : HonamiGraphStyles.ListBoxBg;
                props.style.display = sel ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        private void RebuildLayers()
        {
            _layersContent.Clear();
            _layerRows.Clear();

            var sc = _window.SerializedController;
            if (sc == null || sc.targetObject == null) return;
            sc.Update();

            var hdr = HonamiGraphStyles.Row();
            hdr.style.marginBottom = 6;
            hdr.Add(HonamiGraphStyles.SubTitle("Animation Layers"));
            hdr.Add(HonamiGraphStyles.Spacer());
            var addBtn = HonamiGraphStyles.SmallButton("+");
            addBtn.tooltip = "Add new layer";

            bool isOverride = _window.RuntimeController != null && _window.RuntimeController.IsOverride;

            addBtn.clicked += () =>
            {
                var targetSc = isOverride ? new SerializedObject(_window.RuntimeController) : sc;
                var propName = isOverride ? "additionalLayers" : "layers";
                targetSc.Update();
                var lp = targetSc.FindProperty(propName);
                if (lp == null) return;
                int next = lp.arraySize;
                lp.InsertArrayElementAtIndex(next);
                lp.GetArrayElementAtIndex(next).FindPropertyRelative("name").stringValue = "New Layer";
                lp.GetArrayElementAtIndex(next).FindPropertyRelative("weight").floatValue = 1f;
                lp.GetArrayElementAtIndex(next).FindPropertyRelative("parentLayerIndex").intValue = -1;
                targetSc.ApplyModifiedProperties();
                HonamiNotificationPanel.ShowGlobal("Layer Added", "New animation layer created.", HonamiNotificationType.Success);
                RebuildLayers();
            };
            hdr.Add(addBtn);
            _layersContent.Add(hdr);

            var layersProp = sc.FindProperty("layers");
            int globalIndex = 0;
            if (layersProp != null)
            {
                MigrateSerializedLayerInheritance(sc, layersProp);
                NormalizeLayerInheritanceProperties(sc, layersProp, 0);
                BuildLayerTree(sc, layersProp, 0, isOverride);
                globalIndex = layersProp.arraySize;
            }

            if (isOverride)
            {
                var overrideSc = new SerializedObject(_window.RuntimeController);
                var addLayersProp = overrideSc.FindProperty("additionalLayers");
                if (addLayersProp != null)
                {
                    NormalizeLayerInheritanceProperties(overrideSc, addLayersProp, globalIndex);
                    for (int i = 0; i < addLayersProp.arraySize; i++)
                    {
                        BuildLayerRow(overrideSc, addLayersProp, i, globalIndex++, false, 0, false);
                    }
                }
            }

            UpdateLayerHighlight();
        }

        private static void MigrateSerializedLayerInheritance(SerializedObject sc, SerializedProperty layersProp)
        {
            var versionProp = sc.FindProperty("serializedLayerInheritanceVersion");
            if (versionProp == null || versionProp.intValue > 0) return;

            for (int i = 0; i < layersProp.arraySize; i++)
            {
                var parentProp = layersProp.GetArrayElementAtIndex(i).FindPropertyRelative("parentLayerIndex");
                if (parentProp != null) parentProp.intValue = -1;
            }

            versionProp.intValue = 1;
            sc.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void NormalizeLayerInheritanceProperties(SerializedObject sc, SerializedProperty layersProp, int globalOffset)
        {
            bool changed = false;
            for (int i = 0; i < layersProp.arraySize; i++)
            {
                var parentProp = layersProp.GetArrayElementAtIndex(i).FindPropertyRelative("parentLayerIndex");
                if (parentProp == null) continue;

                int globalIndex = globalOffset + i;
                if (parentProp.intValue >= globalIndex || parentProp.intValue < -1)
                {
                    parentProp.intValue = -1;
                    changed = true;
                }
            }

            if (changed)
                sc.ApplyModifiedPropertiesWithoutUndo();
        }

        private void BuildLayerTree(SerializedObject sc, SerializedProperty layersProp, int globalOffset, bool isLockedBase)
        {
            var childrenByParent = new Dictionary<int, List<int>>();
            var roots = new List<int>();

            for (int i = 0; i < layersProp.arraySize; i++)
            {
                var parentProp = layersProp.GetArrayElementAtIndex(i).FindPropertyRelative("parentLayerIndex");
                int globalIndex = globalOffset + i;
                int parentIndex = parentProp != null ? parentProp.intValue : -1;

                if (parentIndex >= globalOffset && parentIndex < globalIndex)
                {
                    if (!childrenByParent.TryGetValue(parentIndex, out var children))
                    {
                        children = new List<int>();
                        childrenByParent[parentIndex] = children;
                    }
                    children.Add(i);
                }
                else
                {
                    roots.Add(i);
                }
            }

            for (int i = 0; i < roots.Count; i++)
                BuildLayerBranch(sc, layersProp, roots[i], globalOffset, isLockedBase, 0, childrenByParent);
        }

        private void BuildLayerBranch(
            SerializedObject sc,
            SerializedProperty layersProp,
            int arrayIdx,
            int globalOffset,
            bool isLockedBase,
            int depth,
            Dictionary<int, List<int>> childrenByParent)
        {
            int globalIdx = globalOffset + arrayIdx;
            BuildLayerRow(sc, layersProp, arrayIdx, globalIdx, isLockedBase, depth, depth > 0);

            if (!childrenByParent.TryGetValue(globalIdx, out var children)) return;

            for (int i = 0; i < children.Count; i++)
                BuildLayerBranch(sc, layersProp, children[i], globalOffset, isLockedBase, depth + 1, childrenByParent);
        }

        private void BuildLayerRow(SerializedObject sc, SerializedProperty layersProp, int arrayIdx, int globalIdx, bool isLockedBase, int depth, bool isInherited)
        {
            var layerProp = layersProp.GetArrayElementAtIndex(arrayIdx);
            var nameProp = layerProp.FindPropertyRelative("name");
            var weightProp = layerProp.FindPropertyRelative("weight");
            var parentProp = layerProp.FindPropertyRelative("parentLayerIndex");

            var outer = new VisualElement();
            HonamiGraphStyles.ApplyListBox(outer);
            outer.style.marginBottom = 3;
            outer.style.marginLeft = depth * 16;
            if (isInherited)
            {
                outer.style.borderLeftWidth = 2;
                outer.style.borderLeftColor = new Color(0.5f, 0.65f, 0.9f, 0.55f);
            }

            var hdr = HonamiGraphStyles.Row();
            hdr.style.marginBottom = 0;

            var nameLabelText = nameProp.stringValue;
            if (isLockedBase) nameLabelText += " (Base)";
            var nameLabel = new Label(nameLabelText);

            if (isLockedBase) nameLabel.style.color = new Color(0.6f, 0.6f, 0.6f);
            else if (isInherited) nameLabel.style.color = new Color(0.78f, 0.88f, 1f);

            nameLabel.style.flexGrow = 1;
            nameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            nameLabel.style.paddingLeft = 4;
            nameLabel.style.paddingTop = nameLabel.style.paddingBottom = 4;

            int capturedGlobal = globalIdx;
            int capturedArray = arrayIdx;
            hdr.style.paddingTop = hdr.style.paddingBottom = 2;
            hdr.RegisterCallback<ClickEvent>(evt =>
            {
                if (evt.target is Button) return;
                if (_window.CurrentLayerIndex != capturedGlobal) _window.SetLayer(capturedGlobal);
                else UpdateLayerHighlight();
            });
            hdr.RegisterCallback<PointerEnterEvent>(_ =>
            {
                if (_window.CurrentLayerIndex != capturedGlobal)
                    hdr.style.backgroundColor = new Color(1f, 1f, 1f, 0.06f);
            });
            hdr.RegisterCallback<PointerLeaveEvent>(_ =>
            {
                hdr.style.backgroundColor = StyleKeyword.Null;
            });

            var menuBtn = HonamiGraphStyles.SmallButton("...");
            if (isLockedBase)
            {
                menuBtn.SetEnabled(false);
                menuBtn.tooltip = "Cannot edit base layers from an Override Controller.";
            }

            menuBtn.clicked += () =>
            {
                if (isLockedBase) return;
                string cName = nameProp.stringValue;
                var menu = new GenericMenu();

                // For override additional layers, we can't use DuplicateLayer from window easily since it duplicates base layer.
                // We will only allow Delete for additional layers if it's an Override Controller for now.
                if (_window.RuntimeController != null && _window.RuntimeController.IsOverride)
                {
                    menu.AddItem(new GUIContent("Delete"), false, () =>
                    {
                        if (EditorUtility.DisplayDialog("Delete Additional Layer", $"Delete '{cName}'?", "Delete", "Cancel"))
                        {
                            sc.Update();
                            layersProp.DeleteArrayElementAtIndex(capturedArray);
                            sc.ApplyModifiedProperties();
                            if (_window.CurrentLayerIndex >= capturedGlobal) _window.SetLayer(Mathf.Max(0, capturedGlobal - 1));
                            RebuildLayers();
                        }
                    });
                }
                else
                {
                    menu.AddItem(new GUIContent("Duplicate"), false, () => { _window.DuplicateLayer(capturedGlobal); RebuildLayers(); });
                    menu.AddItem(new GUIContent("Create Child Layer"), false, () => { _window.CreateInheritedLayer(capturedGlobal); RebuildLayers(); });
                    if (isInherited && parentProp != null)
                    {
                        menu.AddItem(new GUIContent("Detach From Parent"), false, () =>
                        {
                            sc.Update();
                            var liveParentProp = layersProp.GetArrayElementAtIndex(capturedArray).FindPropertyRelative("parentLayerIndex");
                            if (liveParentProp != null) liveParentProp.intValue = -1;
                            sc.ApplyModifiedProperties();
                            _window.Controller?.ClearEffectiveStateCache();
                            _window.SetLayer(capturedGlobal);
                            RebuildLayers();
                        });
                    }
                    menu.AddItem(new GUIContent("Copy"), false, () => { _window.CopyLayer(capturedGlobal); });

                    if (HonamiGraphWindow.ClipboardController != null)
                        menu.AddItem(new GUIContent("Paste As New Layer"), false, () => { _window.PasteLayer(); RebuildLayers(); });
                    else
                        menu.AddDisabledItem(new GUIContent("Paste As New Layer"));

                    if (capturedGlobal > 0)
                    {
                        menu.AddSeparator("");
                        menu.AddItem(new GUIContent("Delete"), false, () => { _window.DeleteLayer(capturedGlobal, cName); RebuildLayers(); });
                    }
                    else
                        menu.AddDisabledItem(new GUIContent("Delete (Base Layer)"));
                }
                menu.ShowAsContext();
            };

            hdr.Add(nameLabel);
            hdr.Add(menuBtn);
            outer.Add(hdr);

            var props = new VisualElement();
            props.style.marginTop = 4;
            props.style.display = DisplayStyle.None;

            var nameField = new PropertyField(nameProp, "Name");
            nameField.BindProperty(nameProp);
            nameField.RegisterCallback<SerializedPropertyChangeEvent>(evt =>
            {
                evt.StopImmediatePropagation();
                nameLabel.text = nameProp.stringValue + (isLockedBase ? " (Base)" : "");
            });
            if (isLockedBase) nameField.SetEnabled(false);
            props.Add(nameField);

            if (globalIdx > 0)
            {
                var weightField = new PropertyField(weightProp, "Weight");
                weightField.BindProperty(weightProp);
                if (isLockedBase) weightField.SetEnabled(false);
                props.Add(weightField);
            }

            var maskProp = layerProp.FindPropertyRelative("avatarMask");
            var maskField = new PropertyField(maskProp, "Avatar Mask");
            maskField.BindProperty(maskProp);
            if (isLockedBase) maskField.SetEnabled(false);
            props.Add(maskField);

            var mirrorProp = layerProp.FindPropertyRelative("mirror");
            var mirrorField = new PropertyField(mirrorProp, "Mirror Layer");
            mirrorField.BindProperty(mirrorProp);
            if (isLockedBase) mirrorField.SetEnabled(false);
            props.Add(mirrorField);

            if (isInherited && parentProp != null)
            {
                string parentName = "Parent Layer";
                int parentIndex = parentProp.intValue;
                if (_window.Controller != null && parentIndex >= 0 && parentIndex < _window.Controller.layers.Count)
                    parentName = _window.Controller.layers[parentIndex].name;

                props.Add(HonamiGraphStyles.MiniLabel($"Inherits from {parentName}", new Color(0.65f, 0.78f, 1f)));
            }

            outer.Add(props);
            _layersContent.Add(outer);
            _layerRows.Add((globalIdx, outer, props, nameLabel));
        }

        private void RebuildParams()
        {
            _paramsContent.Clear();

            var sc = _window.SerializedController;
            if (sc == null || sc.targetObject == null) return;
            sc.Update();

            var hdr = HonamiGraphStyles.Row();
            hdr.style.marginBottom = 6;
            hdr.Add(HonamiGraphStyles.SubTitle("State Parameters"));
            hdr.Add(HonamiGraphStyles.Spacer());
            var addBtn = HonamiGraphStyles.SmallButton("+");
            addBtn.tooltip = "Add new parameter";

            bool isOverride = _window.RuntimeController != null && _window.RuntimeController.IsOverride;

            addBtn.clicked += () =>
            {
                if (isOverride)
                {
                    var overrideSc = new SerializedObject(_window.RuntimeController);
                    overrideSc.Update();
                    var pProp = overrideSc.FindProperty("additionalParameters");
                    if (pProp == null) return;
                    var menu = new GenericMenu();
                    menu.AddItem(new GUIContent("Float"), false, () => { AddAdditionalParameter(overrideSc, pProp, HonamiParameterType.Float); RebuildParams(); });
                    menu.AddItem(new GUIContent("Int"), false, () => { AddAdditionalParameter(overrideSc, pProp, HonamiParameterType.Int); RebuildParams(); });
                    menu.AddItem(new GUIContent("Bool"), false, () => { AddAdditionalParameter(overrideSc, pProp, HonamiParameterType.Bool); RebuildParams(); });
                    menu.AddItem(new GUIContent("Trigger"), false, () => { AddAdditionalParameter(overrideSc, pProp, HonamiParameterType.Trigger); RebuildParams(); });
                    menu.AddItem(new GUIContent("Random"), false, () => { AddAdditionalParameter(overrideSc, pProp, HonamiParameterType.Random); RebuildParams(); });
                    menu.AddItem(new GUIContent("Random"), false, () => { AddAdditionalParameter(overrideSc, pProp, HonamiParameterType.Random); RebuildParams(); });
                    menu.ShowAsContext();
                    return;
                }

                var baseMenu = new GenericMenu();
                baseMenu.AddItem(new GUIContent("Float"), false, () => { _window.AddParameter(HonamiParameterType.Float); RebuildParams(); });
                baseMenu.AddItem(new GUIContent("Int"), false, () => { _window.AddParameter(HonamiParameterType.Int); RebuildParams(); });
                baseMenu.AddItem(new GUIContent("Bool"), false, () => { _window.AddParameter(HonamiParameterType.Bool); RebuildParams(); });
                baseMenu.AddItem(new GUIContent("Trigger"), false, () => { _window.AddParameter(HonamiParameterType.Trigger); RebuildParams(); });
                baseMenu.AddItem(new GUIContent("Random"), false, () => { _window.AddParameter(HonamiParameterType.Random); RebuildParams(); });
                baseMenu.AddItem(new GUIContent("Random"), false, () => { _window.AddParameter(HonamiParameterType.Random); RebuildParams(); });
                baseMenu.ShowAsContext();
            };
            hdr.Add(addBtn);
            _paramsContent.Add(hdr);

            int globalIndex = 0;
            var paramsProp = sc.FindProperty("parameters");
            if (paramsProp != null)
            {
                for (int i = 0; i < paramsProp.arraySize; i++)
                    BuildParamRow(sc, paramsProp, i, globalIndex++, isOverride);
            }

            if (isOverride)
            {
                var overrideSc = new SerializedObject(_window.RuntimeController);
                var addParamsProp = overrideSc.FindProperty("additionalParameters");
                if (addParamsProp != null)
                {
                    for (int i = 0; i < addParamsProp.arraySize; i++)
                    {
                        BuildParamRow(overrideSc, addParamsProp, i, globalIndex++, false);
                    }
                }
            }
        }

        private void AddAdditionalParameter(SerializedObject sc, SerializedProperty listProp, HonamiParameterType type)
        {
            int next = listProp.arraySize;
            listProp.InsertArrayElementAtIndex(next);
            var newItem = listProp.GetArrayElementAtIndex(next);
            newItem.FindPropertyRelative("name").stringValue = "New " + type;
            newItem.FindPropertyRelative("type").enumValueIndex = (int)type;
            sc.ApplyModifiedProperties();
        }

        private void BuildParamRow(SerializedObject sc, SerializedProperty paramsProp, int arrayIdx, int globalIdx, bool isLockedBase)
        {
            var pProp = paramsProp.GetArrayElementAtIndex(arrayIdx);
            var nameProp = pProp.FindPropertyRelative("name");
            var typeProp = pProp.FindPropertyRelative("type");
            var pType = (HonamiParameterType)typeProp.enumValueIndex;

            var row = HonamiGraphStyles.ListBox();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;

            var typeLbl = new Label(pType.ToString().Substring(0, 1) + (isLockedBase ? " (B)" : ""));
            typeLbl.style.color = isLockedBase ? new Color(0.5f, 0.5f, 0.5f) : HonamiGraphStyles.GreyText;
            typeLbl.style.unityFontStyleAndWeight = FontStyle.Bold;
            typeLbl.style.width = isLockedBase ? 30 : 16;
            typeLbl.style.unityTextAlign = TextAnchor.MiddleCenter;
            row.Add(typeLbl);

            if (!isLockedBase)
            {
                row.AddManipulator(new ContextualMenuManipulator(menuEvent =>
                {
                    void ChangeType(HonamiParameterType newType)
                    {
                        sc.Update();
                        var currentParamsProp = sc.FindProperty(paramsProp.propertyPath);
                        var targetProp = currentParamsProp.GetArrayElementAtIndex(arrayIdx);
                        targetProp.FindPropertyRelative("type").enumValueIndex = (int)newType;
                        sc.ApplyModifiedProperties();
                        RebuildParams();
                    }

                    menuEvent.menu.AppendAction("Change Type/Float", _ => ChangeType(HonamiParameterType.Float), pType == HonamiParameterType.Float ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
                    menuEvent.menu.AppendAction("Change Type/Int", _ => ChangeType(HonamiParameterType.Int), pType == HonamiParameterType.Int ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
                    menuEvent.menu.AppendAction("Change Type/Bool", _ => ChangeType(HonamiParameterType.Bool), pType == HonamiParameterType.Bool ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
                    menuEvent.menu.AppendAction("Change Type/Trigger", _ => ChangeType(HonamiParameterType.Trigger), pType == HonamiParameterType.Trigger ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
                    menuEvent.menu.AppendAction("Change Type/Random", _ => ChangeType(HonamiParameterType.Random), pType == HonamiParameterType.Random ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
                }));
            }

            var nameField = new PropertyField(nameProp) { label = "" };
            nameField.BindProperty(nameProp);
            nameField.style.flexGrow = 1;
            nameField.style.minWidth = 50;
            if (isLockedBase) nameField.SetEnabled(false);
            row.Add(nameField);

            bool isPlaying = EditorApplication.isPlaying && _window.RuntimeAnimator != null;
            int paramHash = Runtime.Core.HonamiAnimator.StringToHash(nameProp.stringValue);
            int capturedArray = arrayIdx;
            switch (pType)
            {
                case HonamiParameterType.Float:
                    if (isPlaying)
                    {
                        var fField = new FloatField { value = _window.RuntimeAnimator.Parameters.GetFloat(paramHash) };
                        fField.style.width = 60;
                        fField.RegisterValueChangedCallback(ev => _window.RuntimeAnimator.Parameters.SetFloat(paramHash, ev.newValue));
                        fField.schedule.Execute(() => { if (_window.RuntimeAnimator != null) fField.SetValueWithoutNotify(_window.RuntimeAnimator.Parameters.GetFloat(paramHash)); }).Every(100);
                        row.Add(fField);
                    }
                    else
                    {
                        var fProp = pProp.FindPropertyRelative("defaultFloat");
                        var fField = new PropertyField(fProp) { label = "" };
                        fField.BindProperty(fProp);
                        fField.style.width = 60;
                        row.Add(fField);
                    }
                    break;
                case HonamiParameterType.Int:
                    if (isPlaying)
                    {
                        var iField = new IntegerField { value = _window.RuntimeAnimator.Parameters.GetInteger(paramHash) };
                        iField.style.width = 60;
                        iField.RegisterValueChangedCallback(ev => _window.RuntimeAnimator.Parameters.SetInteger(paramHash, ev.newValue));
                        iField.schedule.Execute(() => { if (_window.RuntimeAnimator != null) iField.SetValueWithoutNotify(_window.RuntimeAnimator.Parameters.GetInteger(paramHash)); }).Every(100);
                        row.Add(iField);
                    }
                    else
                    {
                        var iProp = pProp.FindPropertyRelative("defaultInt");
                        var iField = new PropertyField(iProp) { label = "" };
                        iField.BindProperty(iProp);
                        iField.style.width = 60;
                        row.Add(iField);
                    }
                    break;
                case HonamiParameterType.Bool:
                    if (isPlaying)
                    {
                        var container = new VisualElement();
                        container.style.flexDirection = FlexDirection.Row;

                        bool isForced = ForcedBools.ContainsKey(paramHash);
                        var bField = new Toggle { value = isForced ? ForcedBools[paramHash].value : _window.RuntimeAnimator.Parameters.GetBool(paramHash) };
                        bField.style.width = 30;

                        var forceToggle = new Toggle { tooltip = "Force value (override scripts)", value = isForced };
                        forceToggle.style.width = 30;

                        bField.RegisterValueChangedCallback(ev =>
                        {
                            _window.RuntimeAnimator.Parameters.SetBool(paramHash, ev.newValue);
                            if (forceToggle.value)
                            {
                                ForcedBools[paramHash] = (_window.RuntimeAnimator, ev.newValue);
                            }
                        });

                        forceToggle.RegisterValueChangedCallback(ev =>
                        {
                            if (ev.newValue)
                            {
                                ForcedBools[paramHash] = (_window.RuntimeAnimator, bField.value);
                                if (!_updateHooked)
                                {
                                    EditorApplication.update += GlobalForceUpdate;
                                    _updateHooked = true;
                                }
                            }
                            else
                            {
                                ForcedBools.Remove(paramHash);
                            }
                        });

                        bField.schedule.Execute(() =>
                        {
                            if (_window.RuntimeAnimator != null && !forceToggle.value)
                                bField.SetValueWithoutNotify(_window.RuntimeAnimator.Parameters.GetBool(paramHash));
                        }).Every(100);

                        container.Add(bField);
                        container.Add(forceToggle);
                        row.Add(container);
                    }
                    else
                    {
                        var bProp = pProp.FindPropertyRelative("defaultBool");
                        var bField = new PropertyField(bProp) { label = "" };
                        bField.BindProperty(bProp);
                        bField.style.width = 60;
                        row.Add(bField);
                    }
                    break;
                case HonamiParameterType.Trigger:
                    if (isPlaying)
                    {
                        var tBtn = new Button(() => _window.RuntimeAnimator.Parameters.SetTrigger(paramHash)) { text = "Fire" };
                        tBtn.style.width = 60;
                        tBtn.style.height = 18;
                        tBtn.style.paddingLeft = 0;
                        tBtn.style.paddingRight = 0;
                        tBtn.schedule.Execute(() =>
                        {
                            if (_window.RuntimeAnimator != null)
                            {
                                bool isSet = _window.RuntimeAnimator.Parameters.IsTriggerSet(paramHash);
                                tBtn.style.backgroundColor = isSet ? new Color(0.2f, 0.6f, 0.2f, 1f) : new StyleColor(StyleKeyword.Null);
                            }
                        }).Every(100);
                        row.Add(tBtn);
                    }
                    else
                    {
                        var dash = new Label("-");
                        dash.style.width = 60;
                        dash.style.unityTextAlign = TextAnchor.MiddleCenter;
                        row.Add(dash);
                    }
                    break;
                case HonamiParameterType.Random:
                    if (isPlaying)
                    {
                        var rField = new FloatField { value = _window.RuntimeAnimator.Parameters.GetFloat(paramHash) };
                        rField.style.width = 60;
                        rField.isReadOnly = true;
                        rField.schedule.Execute(() => { if (_window.RuntimeAnimator != null) rField.SetValueWithoutNotify(_window.RuntimeAnimator.Parameters.GetFloat(paramHash)); }).Every(100);
                        row.Add(rField);
                    }
                    else
                    {
                        var dash = new Label("-");
                        dash.style.width = 60;
                        dash.style.unityTextAlign = TextAnchor.MiddleCenter;
                        row.Add(dash);
                    }
                    break;
            }
            var copyBtn = HonamiGraphStyles.SmallButton("C", () =>
            {
                EditorGUIUtility.systemCopyBuffer = nameProp.stringValue;
                HonamiNotificationPanel.ShowGlobal("Copied", $"Parameter '{nameProp.stringValue}' name copied to clipboard.", HonamiNotificationType.Info, 1.5f);
            });
            copyBtn.tooltip = "Copy parameter name";
            row.Add(copyBtn);

            var del = HonamiGraphStyles.SmallButton(HonamiEditorSymbols.Remove);
            if (isLockedBase)
            {
                del.SetEnabled(false);
                del.tooltip = "Cannot delete base parameters in an Override Controller.";
            }

            del.clicked += () =>
            {
                if (isLockedBase) return;
                sc.Update();
                paramsProp.DeleteArrayElementAtIndex(capturedArray);
                sc.ApplyModifiedProperties();
                RebuildParams();
            };
            row.Add(del);

            _paramsContent.Add(row);
        }
    }
}




