using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEditorInternal;
using System.Linq;
using System.Collections.Generic;
using HonamiAnimationSystem.Runtime.Core;

namespace HonamiAnimationSystem.Editor
{
    /// <summary>
    /// Editor window interface for constructing 1D and 2D blend trees. 
    /// Manages the layout and calculation visualization of blend spaces.
    /// </summary>
    public sealed class HonamiBlendTreeWindow : EditorWindow
    {
        private HonamiController _controller;
        private string _stateGuid;
        private HonamiState _state;
        private SerializedObject _serializedState;
        private HonamiNotificationPanel _notificationPanel;

        private HonamiBlendTreeNode BlendNode => _state?.node as HonamiBlendTreeNode;
        private sealed class BlendTreeGraphView : GraphView
        {
            public System.Action<float> OnPreviewValueChanged;
            private float _previewValue;

            public float PreviewValue
            {
                get => _previewValue;
                set
                {
                    _previewValue = value;
                    OnPreviewValueChanged?.Invoke(_previewValue);
                }
            }

            public BlendTreeGraphView()
            {
                Insert(0, new GridBackground());
                this.AddManipulator(new ContentZoomer());
                this.AddManipulator(new ContentDragger());
                this.AddManipulator(new SelectionDragger());

                styleSheets.Add(Resources.Load<StyleSheet>("HonamiGraphStyle"));
            }

            public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter) => new();
        }

        private BlendTreeGraphView _graphView;
        private List<BlendTreeMotionNode> _motionNodes = new();
        private VisualElement _inspectorRoot;
        private WeightVisualizer _weightVisualizer;
        private Slider _previewSlider;
        private Label _titleLabel;
        private TwoPaneSplitView _splitView;
        private VisualElement _rightPanel;

        [MenuItem("Window/Honami/Honami Blend Tree")]
        public static void OpenWindow()
        {
            var window = GetWindow<HonamiBlendTreeWindow>();
            window.titleContent = HonamiEditorIcons.IconContent("HonamiBlendtreeWhite", "Honami Blend Tree");
            window.minSize = new Vector2(1000, 650);
        }

        public static void InspectState(HonamiController controller, string stateGuid)
        {
            var window = GetWindow<HonamiBlendTreeWindow>();
            window._controller = controller;
            window._stateGuid = stateGuid;
            window.RefreshState();
        }

        private void OnEnable()
        {
            ConstructLayout();
            Undo.undoRedoPerformed += OnUndoRedo;
            string pt = EditorPrefs.GetString("HonamiBlendTree_Controller", "");
            if (!string.IsNullOrEmpty(pt))
            {
                _controller = AssetDatabase.LoadAssetAtPath<HonamiController>(pt);
                _stateGuid = EditorPrefs.GetString("HonamiBlendTree_State", "");
            }
            EditorApplication.delayCall += () => { LoadPreferences(); RefreshState(); };
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoRedo;
            SavePreferences();
        }

        private void SavePreferences()
        {
            if (_graphView != null)
            {
                var tr = _graphView.contentViewContainer.resolvedStyle.translate;
                var sc = _graphView.contentViewContainer.resolvedStyle.scale;
                EditorPrefs.SetFloat("HonamiBlendTree_PosX", tr.x);
                EditorPrefs.SetFloat("HonamiBlendTree_PosY", tr.y);
                EditorPrefs.SetFloat("HonamiBlendTree_Scale", sc.value.x);
            }
            if (_previewSlider != null)
                EditorPrefs.SetFloat("HonamiBlendTree_Preview", _previewSlider.value);

            if (_rightPanel != null && _rightPanel.layout.width > 50)
                EditorPrefs.SetFloat("HonamiBlendTree_PaneWidth", _rightPanel.layout.width);

            if (_controller != null)
                EditorPrefs.SetString("HonamiBlendTree_Controller", AssetDatabase.GetAssetPath(_controller));
            else
                EditorPrefs.DeleteKey("HonamiBlendTree_Controller");

            if (!string.IsNullOrEmpty(_stateGuid))
                EditorPrefs.SetString("HonamiBlendTree_State", _stateGuid);
        }

        private void LoadPreferences()
        {
            if (_graphView != null && EditorPrefs.HasKey("HonamiBlendTree_PosX"))
            {
                float x = EditorPrefs.GetFloat("HonamiBlendTree_PosX", 0);
                float y = EditorPrefs.GetFloat("HonamiBlendTree_PosY", 0);
                float scale = EditorPrefs.GetFloat("HonamiBlendTree_Scale", 1);
                _graphView.UpdateViewTransform(new Vector3(x, y, 0), new Vector3(scale, scale, 1));
            }
            if (_previewSlider != null)
                _previewSlider.value = EditorPrefs.GetFloat("HonamiBlendTree_Preview", 0f);
        }

        private void OnFocus() => RefreshState();

        private void RefreshState()
        {
            if (_controller != null && !string.IsNullOrEmpty(_stateGuid))
            {
                _state = _controller.states.FirstOrDefault(s => s != null && s.guid == _stateGuid);
                if (_state != null)
                {
                    _serializedState = new SerializedObject(_state);
                    if (_titleLabel != null) _titleLabel.text = _state.stateName;
                }
                else _serializedState = null;
            }
            else
            {
                _state = null;
                _serializedState = null;
            }

            RegenerateGraph();
            UpdateInspector();
        }

        private void OnInspectorUpdate()
        {
            if (_serializedState != null && _serializedState.targetObject != null)
                _serializedState.Update();
        }

        private void ConstructLayout()
        {
            var root = rootVisualElement;
            HonamiEditorLayout.PrepareRoot(root);

            _notificationPanel = new();
            root.Add(_notificationPanel);

            var header = HonamiEditorLayout.Header(HonamiEditorIcons.BlendTree, "No State Selected", "Blend Tree Editor", out _titleLabel);
            var frameBtn = HonamiEditorLayout.HeaderButton("Frame All", () => _graphView?.FrameAll());
            header.Add(frameBtn);
            root.Add(header);

            _splitView = new TwoPaneSplitView(1, EditorPrefs.GetFloat("HonamiBlendTree_PaneWidth", 380f), TwoPaneSplitViewOrientation.Horizontal);
            _splitView.style.flexGrow = 1;
            root.Add(_splitView);

            var leftPanel = new VisualElement { style = { flexGrow = 1 } };
            _splitView.Add(leftPanel);

            _graphView = new();
            _graphView.style.flexGrow = 1;
            leftPanel.Add(_graphView);

            var bottomArea = new VisualElement();
            bottomArea.AddToClassList("honami-bt-visualizer-container");
            leftPanel.Add(bottomArea);

            var vizHeader = new VisualElement();
            vizHeader.AddToClassList("honami-bt-visualizer-header");
            bottomArea.Add(vizHeader);

            var vizTitle = new Label("PREVIEW & BLEND VISUALIZER");
            vizTitle.AddToClassList("honami-bt-visualizer-title");
            vizHeader.Add(vizTitle);

            vizHeader.Add(HonamiGraphStyles.Spacer());

            var sliderLabel = new Label("VALUE");
            sliderLabel.AddToClassList("honami-bt-preview-label");
            vizHeader.Add(sliderLabel);

            _previewSlider = new Slider();
            _previewSlider.lowValue = -1f;
            _previewSlider.highValue = 1f;
            _previewSlider.AddToClassList("honami-bt-preview-slider");
            _previewSlider.RegisterValueChangedCallback(evt =>
            {
                _graphView.PreviewValue = evt.newValue;
                _weightVisualizer.MarkDirtyRepaint();
                UpdateNodePreviews();
            });
            vizHeader.Add(_previewSlider);

            _weightVisualizer = new(this);
            _weightVisualizer.AddToClassList("honami-bt-weight-visualizer");
            bottomArea.Add(_weightVisualizer);

            _rightPanel = new VisualElement();
            HonamiEditorLayout.StyleSidePanel(_rightPanel, leftBorder: true);
            _rightPanel.style.minWidth = 380;

            _inspectorRoot = new ScrollView();
            _inspectorRoot.style.flexGrow = 1;
            _inspectorRoot.style.paddingLeft = HonamiEditorLayout.InspectorPadding;
            _inspectorRoot.style.paddingRight = HonamiEditorLayout.InspectorPadding;
            _inspectorRoot.style.paddingTop = HonamiEditorLayout.InspectorPadding;
            _inspectorRoot.style.paddingBottom = HonamiEditorLayout.InspectorPadding;
            _rightPanel.Add(_inspectorRoot);

            _splitView.Add(_rightPanel);
        }

        private void RegenerateGraph()
        {
            if (_graphView == null) return;

            _graphView.graphViewChanged -= OnGraphViewChanged;
            foreach (var element in _graphView.graphElements.ToList()) _graphView.RemoveElement(element);
            _graphView.graphViewChanged += OnGraphViewChanged;

            _motionNodes.Clear();

            if (_state == null || BlendNode == null) return;

            var rootNode = new BlendTreeRootNode(_state);
            _graphView.AddElement(rootNode);

            if (BlendNode.blendMotions != null)
            {
                float startY = 50;
                for (int i = 0; i < BlendNode.blendMotions.Count; i++)
                {
                    var motion = BlendNode.blendMotions[i];
                    var motionNode = new BlendTreeMotionNode(motion, i);
                    motionNode.SetPosition(new Rect(50, startY + (i * 120), 220, 90));
                    _graphView.AddElement(motionNode);
                    _motionNodes.Add(motionNode);

                    var edge = new Edge
                    {
                        output = motionNode.OutputPort,
                        input = rootNode.InputPort,
                        userData = "readonly",
                        capabilities = Capabilities.Selectable
                    };
                    edge.output.Connect(edge);
                    edge.input.Connect(edge);
                    _graphView.AddElement(edge);
                }
            }

            UpdateSliderRange();

            if (!EditorPrefs.HasKey("HonamiBlendTree_PosX"))
                EditorApplication.delayCall += () => _graphView.FrameAll();
        }

        private void UpdateSliderRange()
        {
            if (_state == null || BlendNode == null || BlendNode.blendMotions == null || BlendNode.blendMotions.Count == 0 || _previewSlider == null) return;

            float min = float.MaxValue;
            float max = float.MinValue;
            foreach (var m in BlendNode.blendMotions)
            {
                if (m.threshold < min) min = m.threshold;
                if (m.threshold > max) max = m.threshold;
            }

            if (min > max) { min = 0; max = 0; }
            if (Mathf.Approximately(min, max)) { min -= 1f; max += 1f; }
            else { float r = (max - min) * 0.15f; min -= r; max += r; }

            _previewSlider.lowValue = min;
            _previewSlider.highValue = max;

            if (_previewSlider.value < min) _previewSlider.value = min;
            if (_previewSlider.value > max) _previewSlider.value = max;
        }

        private void UpdateNodePreviews()
        {
            if (_state == null || BlendNode == null || BlendNode.blendMotions == null) return;
            float[] weights = CalculateWeights(_graphView.PreviewValue);
            for (int i = 0; i < _motionNodes.Count && i < weights.Length; i++) _motionNodes[i].UpdateWeightProgress(weights[i]);
        }

        private struct ThresholdComparer : IComparer<(float threshold, int index)>
        {
            public int Compare((float threshold, int index) x, (float threshold, int index) y) => x.threshold.CompareTo(y.threshold);
        }
        private static readonly ThresholdComparer _comparer = new();

        public float[] CalculateWeights(float parameterValue)
        {
            int count = BlendNode?.blendMotions?.Count ?? 0;
            float[] weights = new float[count];
            if (count > 0) CalculateWeights(parameterValue, weights);
            return weights;
        }

        public void CalculateWeights(float parameterValue, float[] weights)
        {
            int count = BlendNode?.blendMotions?.Count ?? 0;
            if (count == 0 || weights == null || weights.Length < count) return;

            System.Array.Clear(weights, 0, weights.Length);

            if (count == 1) { weights[0] = 1f; return; }

            var sorted = new (float threshold, int index)[count];
            for (int i = 0; i < count; i++) sorted[i] = (BlendNode.blendMotions[i].threshold, i);
            System.Array.Sort(sorted, _comparer);

            if (parameterValue <= sorted[0].threshold) weights[sorted[0].index] = 1f;
            else if (parameterValue >= sorted[count - 1].threshold) weights[sorted[count - 1].index] = 1f;
            else
            {
                for (int i = 0; i < count - 1; i++)
                {
                    float t1 = sorted[i].threshold;
                    float t2 = sorted[i + 1].threshold;
                    if (parameterValue >= t1 && parameterValue <= t2)
                    {
                        float t = (parameterValue - t1) / (t2 - t1);
                        weights[sorted[i].index] = 1f - t;
                        weights[sorted[i + 1].index] = t;
                        break;
                    }
                }
            }
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange change)
        {
            if (change.elementsToRemove != null) change.elementsToRemove.Clear();
            return change;
        }

        private void OnUndoRedo() { RefreshState(); Repaint(); }
        private void OnDestroy() => Undo.undoRedoPerformed -= OnUndoRedo;

        private void UpdateInspector()
        {
            if (_inspectorRoot == null) return;
            _inspectorRoot.Clear();
            if (_controller == null || _state == null || BlendNode == null || _serializedState == null)
            {
                _inspectorRoot.Add(new HelpBox("Select a Blend Tree node in the Animator to inspect.", HelpBoxMessageType.Info));
                return;
            }

            _serializedState.Update();

            var propBox = HonamiGraphStyles.Box();
            propBox.Add(HonamiGraphStyles.SubTitle("General Settings"));
            _inspectorRoot.Add(propBox);

            var blendNodeSO = new SerializedObject(BlendNode);

            var typeProp = blendNodeSO.FindProperty("blendType");
            if (typeProp != null)
            {
                var typeField = new PropertyField(typeProp, "Blend Mode");
                typeField.BindProperty(typeProp);
                propBox.Add(typeField);
            }

            var paramProp = blendNodeSO.FindProperty("blendParameter");
            var paramNames = _controller.parameters.Where(p => p.type == HonamiParameterType.Float).Select(p => p.name).ToList();
            paramNames.Insert(0, "- None -");

            var paramField = new DropdownField("Parameter", paramNames, string.IsNullOrEmpty(paramProp.stringValue) ? 0 : Mathf.Max(0, paramNames.IndexOf(paramProp.stringValue)));

            paramField.RegisterValueChangedCallback(evt =>
            {
                blendNodeSO.Update();
                paramProp.stringValue = evt.newValue == "- None -" ? "" : evt.newValue;
                blendNodeSO.ApplyModifiedProperties();
                EditorUtility.SetDirty(BlendNode);
            });
            propBox.Add(paramField);

            var speedProp = _serializedState.FindProperty("speed");
            var speedField = new PropertyField(speedProp, "Overall Speed");
            speedField.BindProperty(speedProp);
            propBox.Add(speedField);

            var loopProp = _serializedState.FindProperty("loop");
            var loopField = new PropertyField(loopProp, "Loop");
            loopField.BindProperty(loopProp);
            propBox.Add(loopField);

            var reverseProp = _serializedState.FindProperty("isReversed");
            var reverseField = new PropertyField(reverseProp, "Reverse");
            reverseField.BindProperty(reverseProp);
            propBox.Add(reverseField);

            var mirrorProp = _serializedState.FindProperty("mirror");
            if (mirrorProp != null)
            {
                var mirrorField = new PropertyField(mirrorProp, "Mirror State");
                mirrorField.BindProperty(mirrorProp);
                propBox.Add(mirrorField);
            }

            var dampProp = blendNodeSO.FindProperty("blendParameterDampTime");
            if (dampProp != null)
            {
                var dampField = new PropertyField(dampProp, "Damp Time");
                dampField.BindProperty(dampProp);
                propBox.Add(dampField);
            }

            var motionsFoldout = HonamiGraphStyles.Foldout("Motions (1D)");
            _inspectorRoot.Add(motionsFoldout);

            var motionsProp = blendNodeSO.FindProperty("blendMotions");
            BuildMotionsList(motionsFoldout, blendNodeSO, motionsProp);

            var autoBtn = HonamiGraphStyles.TallButton("Distribute Thresholds Evenly", () =>
            {
                var blendNodeSO2 = new SerializedObject(BlendNode);
                blendNodeSO2.Update();
                var p = blendNodeSO2.FindProperty("blendMotions");
                int cnt = p.arraySize;
                if (cnt > 1) for (int k = 0; k < cnt; k++) p.GetArrayElementAtIndex(k).FindPropertyRelative("threshold").floatValue = k / (float)(cnt - 1);
                else if (cnt == 1) p.GetArrayElementAtIndex(0).FindPropertyRelative("threshold").floatValue = 0f;
                blendNodeSO2.ApplyModifiedProperties();
                UpdateInspector();
                RegenerateGraph();
            });
            motionsFoldout.Add(autoBtn);
        }

        private void BuildMotionsList(VisualElement parent, SerializedObject blendNodeSO, SerializedProperty motionsProp)
        {
            var list = new ReorderableList(blendNodeSO, motionsProp, true, true, true, true)
            {
                elementHeight = EditorGUIUtility.singleLineHeight + 8f
            };

            list.drawHeaderCallback = rect =>
            {
                const float thresholdWidth = 76f;
                const float speedWidth = 58f;
                const float mirrorWidth = 48f;
                const float gap = 6f;

                var motionRect = new Rect(rect.x + 14f, rect.y, rect.width - thresholdWidth - speedWidth - mirrorWidth - gap * 4f - 14f, rect.height);
                var thresholdRect = new Rect(motionRect.xMax + gap, rect.y, thresholdWidth, rect.height);
                var speedRect = new Rect(thresholdRect.xMax + gap, rect.y, speedWidth, rect.height);
                var mirrorRect = new Rect(speedRect.xMax + gap, rect.y, mirrorWidth, rect.height);

                EditorGUI.LabelField(motionRect, "Motion", EditorStyles.boldLabel);
                EditorGUI.LabelField(thresholdRect, "Threshold", EditorStyles.boldLabel);
                EditorGUI.LabelField(speedRect, "Speed", EditorStyles.boldLabel);
                EditorGUI.LabelField(mirrorRect, "Mirror", EditorStyles.boldLabel);
            };

            list.drawElementCallback = (rect, index, active, focused) =>
            {
                if (index < 0 || index >= motionsProp.arraySize) return;

                SerializedProperty element = motionsProp.GetArrayElementAtIndex(index);
                SerializedProperty clip = element.FindPropertyRelative("clip");
                SerializedProperty threshold = element.FindPropertyRelative("threshold");
                SerializedProperty speed = element.FindPropertyRelative("speed");
                SerializedProperty mirror = element.FindPropertyRelative("mirror");

                const float thresholdWidth = 76f;
                const float speedWidth = 58f;
                const float mirrorWidth = 48f;
                const float gap = 6f;
                float y = rect.y + 4f;
                float height = EditorGUIUtility.singleLineHeight;

                var clipRect = new Rect(rect.x, y, rect.width - thresholdWidth - speedWidth - mirrorWidth - gap * 3f, height);
                var thresholdRect = new Rect(clipRect.xMax + gap, y, thresholdWidth, height);
                var speedRect = new Rect(thresholdRect.xMax + gap, y, speedWidth, height);
                var mirrorRect = new Rect(speedRect.xMax + gap, y, mirrorWidth, height);

                EditorGUI.PropertyField(clipRect, clip, GUIContent.none);
                EditorGUI.PropertyField(thresholdRect, threshold, GUIContent.none);
                EditorGUI.PropertyField(speedRect, speed, GUIContent.none);
                mirror.boolValue = EditorGUI.Toggle(mirrorRect, mirror.boolValue);
            };

            list.onAddCallback = reorderableList =>
            {
                blendNodeSO.Update();
                int index = motionsProp.arraySize;
                motionsProp.InsertArrayElementAtIndex(index);
                SerializedProperty element = motionsProp.GetArrayElementAtIndex(index);
                element.FindPropertyRelative("clip").objectReferenceValue = null;
                element.FindPropertyRelative("threshold").floatValue = index;
                element.FindPropertyRelative("speed").floatValue = 1f;
                element.FindPropertyRelative("mirror").boolValue = false;
                blendNodeSO.ApplyModifiedProperties();
                UpdateInspectorAndGraph();
            };

            list.onRemoveCallback = reorderableList =>
            {
                if (reorderableList.index < 0 || reorderableList.index >= motionsProp.arraySize) return;

                blendNodeSO.Update();
                motionsProp.DeleteArrayElementAtIndex(reorderableList.index);
                blendNodeSO.ApplyModifiedProperties();
                UpdateInspectorAndGraph();
            };

            list.onReorderCallbackWithDetails = (_, _, _) =>
            {
                blendNodeSO.ApplyModifiedProperties();
                UpdateInspectorAndGraph();
            };

            list.onChangedCallback = _ =>
            {
                blendNodeSO.ApplyModifiedProperties();
                UpdateSliderRange();
                _weightVisualizer?.MarkDirtyRepaint();
                RegenerateGraph();
            };

            var listContainer = new IMGUIContainer(() =>
            {
                if (BlendNode == null) return;

                blendNodeSO.Update();
                EditorGUI.BeginChangeCheck();
                list.DoLayoutList();
                if (EditorGUI.EndChangeCheck())
                {
                    blendNodeSO.ApplyModifiedProperties();
                    UpdateSliderRange();
                    _weightVisualizer?.MarkDirtyRepaint();
                    RegenerateGraph();
                }
            });

            listContainer.style.marginTop = 6;
            parent.Add(listContainer);
        }

        private void UpdateInspectorAndGraph()
        {
            UpdateSliderRange();
            _weightVisualizer?.MarkDirtyRepaint();
            RegenerateGraph();
            UpdateInspector();
        }

        private sealed class BlendTreeRootNode : Node
        {
            public Port InputPort;
            public BlendTreeRootNode(HonamiState state)
            {
                title = state.stateName;
                capabilities &= ~Capabilities.Deletable;

                var body = new VisualElement();
                body.AddToClassList("honami-node-body");

                var avatar = new VisualElement();
                avatar.AddToClassList("honami-node-avatar");
                avatar.AddToClassList("honami-node-avatar-blend");
                var icon = new VisualElement();
                icon.AddToClassList("honami-node-icon");
                icon.AddToClassList("honami-node-icon-blend");
                avatar.Add(icon);
                body.Add(avatar);

                var txt = new VisualElement();
                txt.AddToClassList("honami-node-text-container");
                var lbl = new Label("Final Output");
                lbl.AddToClassList("honami-node-label");
                txt.Add(lbl);
                body.Add(txt);

                extensionContainer.Add(body);

                InputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(bool));
                InputPort.portName = "Motions";
                inputContainer.Add(InputPort);

                SetPosition(new Rect(450, 200, 220, 100));
                expanded = true;
                RefreshExpandedState();
            }
        }

        private sealed class BlendTreeMotionNode : Node
        {
            public Port OutputPort;
            public int Index;
            private VisualElement _progressBar;
            private Label _weightLabel;

            public BlendTreeMotionNode(HonamiBlendTreeMotion motion, int index)
            {
                Index = index;
                title = motion.clip != null ? motion.clip.name : "Unset Motion";
                capabilities &= ~Capabilities.Deletable;

                var body = new VisualElement();
                body.AddToClassList("honami-node-body");
                body.style.flexDirection = FlexDirection.Row;
                body.style.alignItems = Align.Center;
                body.style.paddingLeft = 8;
                body.style.paddingRight = 8;
                body.style.paddingTop = 8;
                body.style.paddingBottom = 8;

                var avatar = new VisualElement();
                avatar.AddToClassList("honami-node-avatar");
                avatar.AddToClassList("honami-node-avatar-animation");
                var icon = new VisualElement();
                icon.AddToClassList("honami-node-icon");
                icon.AddToClassList("honami-node-icon-animation");
                avatar.Add(icon);
                body.Add(avatar);

                var weightRow = new VisualElement { style = { flexGrow = 1, flexDirection = FlexDirection.Row, alignItems = Align.Center, marginLeft = 10 } };
                var barBg = new VisualElement { style = { flexGrow = 1, height = 8, minWidth = 80, backgroundColor = new Color(0, 0, 0, 0.5f), borderTopLeftRadius = 4, borderTopRightRadius = 4, borderBottomLeftRadius = 4, borderBottomRightRadius = 4, overflow = Overflow.Hidden } };
                _progressBar = new VisualElement();
                _progressBar.AddToClassList("honami-bt-weight-progress-bar");
                _progressBar.style.width = Length.Percent(0);
                _progressBar.style.height = Length.Percent(100);
                _progressBar.style.backgroundColor = HonamiGraphStyles.Accent;
                barBg.Add(_progressBar);
                weightRow.Add(barBg);

                _weightLabel = new Label("0%");
                _weightLabel.style.width = 40;
                _weightLabel.style.fontSize = 11;
                _weightLabel.style.unityTextAlign = TextAnchor.MiddleRight;
                _weightLabel.style.color = HonamiGraphStyles.GreyText;
                weightRow.Add(_weightLabel);
                body.Add(weightRow);

                if (motion.mirror)
                {
                    var mirrorLabel = new Label("M");
                    mirrorLabel.tooltip = "Mirrored motion";
                    mirrorLabel.style.width = 18;
                    mirrorLabel.style.height = 18;
                    mirrorLabel.style.marginLeft = 6;
                    mirrorLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
                    mirrorLabel.style.fontSize = 10;
                    mirrorLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                    mirrorLabel.style.color = Color.white;
                    mirrorLabel.style.backgroundColor = HonamiGraphStyles.Accent;
                    body.Add(mirrorLabel);
                }

                extensionContainer.Add(body);

                OutputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
                OutputPort.portName = "";
                outputContainer.Add(OutputPort);

                expanded = true;
                RefreshExpandedState();
            }

            public void UpdateWeightProgress(float w)
            {
                _progressBar.style.width = Length.Percent(Mathf.Clamp01(w) * 100f);
                _weightLabel.text = $"{(w * 100f):F0}%";
                _weightLabel.style.color = w > 0.05f ? Color.white : HonamiGraphStyles.GreyText;
            }
        }

        private sealed class WeightVisualizer : VisualElement
        {
            private HonamiBlendTreeWindow _window;
            public WeightVisualizer(HonamiBlendTreeWindow window)
            {
                _window = window;
                generateVisualContent += OnGenerateVisualContent;
                RegisterCallback<MouseDownEvent>(OnMouseDown);
                RegisterCallback<MouseMoveEvent>(OnMouseMove);
            }

            private void OnMouseDown(MouseDownEvent evt) => HandleInput(evt.localMousePosition);
            private void OnMouseMove(MouseMoveEvent evt) { if (evt.pressedButtons == 1) HandleInput(evt.localMousePosition); }

            private void HandleInput(Vector2 pos)
            {
                if (_window._previewSlider == null) return;
                float t = (pos.x / contentRect.width) * (_window._previewSlider.highValue - _window._previewSlider.lowValue) + _window._previewSlider.lowValue;
                _window._previewSlider.value = Mathf.Clamp(t, _window._previewSlider.lowValue, _window._previewSlider.highValue);
            }

            private float[] _weightsBuffer;
            private float[,] _weightsGrid;
            private int[] _sortedIndices;

            private struct IndexComparer : IComparer<int>
            {
                public HonamiBlendTreeNode BlendNode;
                public int Compare(int x, int y) => BlendNode.blendMotions[x].threshold.CompareTo(BlendNode.blendMotions[y].threshold);
            }

            private void OnGenerateVisualContent(MeshGenerationContext ctx)
            {
                var blendNode = _window.BlendNode;
                if (blendNode == null || blendNode.blendMotions == null || blendNode.blendMotions.Count == 0) return;

                var painter = ctx.painter2D;
                Rect rect = contentRect;
                float minT = _window._previewSlider.lowValue;
                float maxT = _window._previewSlider.highValue;
                float rangeT = maxT - minT;
                if (rangeT <= 0) return;

                painter.fillColor = new Color(0.11f, 0.11f, 0.12f, 1f);
                painter.BeginPath();
                painter.MoveTo(new Vector2(0, 0));
                painter.LineTo(new Vector2(rect.width, 0));
                painter.LineTo(new Vector2(rect.width, rect.height));
                painter.LineTo(new Vector2(0, rect.height));
                painter.ClosePath();
                painter.Fill();

                painter.strokeColor = new Color(1, 1, 1, 0.035f);
                painter.lineWidth = 1;

                int gridSections = 10;
                for (int i = 0; i <= gridSections; i++)
                {
                    float x = rect.width * (i / (float)gridSections);
                    painter.BeginPath(); painter.MoveTo(new(x, 0)); painter.LineTo(new(x, rect.height)); painter.Stroke();
                }

                for (int i = 1; i < 5; i++)
                {
                    float y = rect.height * (i / 5f);
                    painter.BeginPath(); painter.MoveTo(new(0, y)); painter.LineTo(new(rect.width, y)); painter.Stroke();
                }

                float topMargin = 20f;
                float bottomMargin = 25f;
                float graphHeight = rect.height - topMargin - bottomMargin;

                painter.strokeColor = new Color(1, 1, 1, 0.15f);
                painter.lineWidth = 2f;
                painter.BeginPath();
                painter.MoveTo(new(0, rect.height - bottomMargin));
                painter.LineTo(new(rect.width, rect.height - bottomMargin));
                painter.Stroke();

                int count = blendNode.blendMotions.Count;
                if (_sortedIndices == null || _sortedIndices.Length != count) _sortedIndices = new int[count];
                for (int i = 0; i < count; i++) _sortedIndices[i] = i;
                System.Array.Sort(_sortedIndices, new IndexComparer { BlendNode = blendNode });

                int res = 100;
                if (_weightsBuffer == null || _weightsBuffer.Length != count) _weightsBuffer = new float[count];
                if (_weightsGrid == null || _weightsGrid.GetLength(0) != res + 1 || _weightsGrid.GetLength(1) != count) _weightsGrid = new float[res + 1, count];

                for (int s = 0; s <= res; s++)
                {
                    float ev = minT + (s / (float)res) * rangeT;
                    _window.CalculateWeights(ev, _weightsBuffer);
                    for (int j = 0; j < count; j++) _weightsGrid[s, j] = _weightsBuffer[j];
                }

                for (int i = 0; i < count; i++)
                {
                    float hue = count == 1 ? 0.55f : (i / (float)count);
                    Color baseCol = Color.HSVToRGB(hue, 0.70f, 0.95f);
                    int origIdx = _sortedIndices[i];

                    painter.fillColor = new Color(baseCol.r, baseCol.g, baseCol.b, 0.15f);
                    painter.BeginPath();
                    painter.MoveTo(new(0, rect.height - bottomMargin));
                    for (int s = 0; s <= res; s++)
                    {
                        float w = _weightsGrid[s, origIdx];
                        painter.LineTo(new((s / (float)res) * rect.width, rect.height - bottomMargin - (w * graphHeight)));
                    }
                    painter.LineTo(new(rect.width, rect.height - bottomMargin));
                    painter.ClosePath(); painter.Fill();

                    painter.strokeColor = baseCol;
                    painter.lineWidth = 2.5f;
                    painter.BeginPath();
                    for (int s = 0; s <= res; s++)
                    {
                        float w = _weightsGrid[s, origIdx];
                        Vector2 p = new((s / (float)res) * rect.width, rect.height - bottomMargin - (w * graphHeight));
                        if (s == 0) painter.MoveTo(p); else painter.LineTo(p);
                    }
                    painter.Stroke();

                    float tx = ((blendNode.blendMotions[origIdx].threshold - minT) / rangeT) * rect.width;

                    painter.strokeColor = new Color(baseCol.r, baseCol.g, baseCol.b, 0.35f);
                    painter.lineWidth = 1f;
                    painter.BeginPath();
                    painter.MoveTo(new(tx, topMargin));
                    painter.LineTo(new(tx, rect.height - bottomMargin));
                    painter.Stroke();

                    painter.fillColor = baseCol;
                    painter.strokeColor = new Color(0.11f, 0.11f, 0.12f, 1f);
                    painter.lineWidth = 2f;
                    painter.BeginPath();
                    painter.Arc(new(tx, rect.height - bottomMargin), 6.5f, 0, 360);
                    painter.Fill();
                    painter.Stroke();
                }

                float pv = _window._graphView.PreviewValue;
                float px = ((pv - minT) / rangeT) * rect.width;
                painter.strokeColor = new Color(1f, 1f, 1f, 0.7f);
                painter.lineWidth = 1.5f;
                painter.BeginPath(); painter.MoveTo(new(px, 0)); painter.LineTo(new(px, rect.height)); painter.Stroke();

                _window.CalculateWeights(pv, _weightsBuffer);
                for (int i = 0; i < count; i++)
                {
                    int idx = _sortedIndices[i];
                    float w = _weightsBuffer[idx];
                    if (w > 0.01f)
                    {
                        float py = rect.height - bottomMargin - (w * graphHeight);
                        painter.fillColor = Color.HSVToRGB(count == 1 ? 0.55f : (i / (float)count), 0.70f, 0.95f);
                        painter.strokeColor = Color.white;
                        painter.lineWidth = 1.5f;
                        painter.BeginPath(); painter.Arc(new(px, py), 4.5f, 0, 360); painter.Fill(); painter.Stroke();
                    }
                }
            }
        }
    }
}
