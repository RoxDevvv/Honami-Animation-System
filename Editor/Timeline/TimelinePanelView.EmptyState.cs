#if UNITY_EDITOR
using System;
using HonamiAnimationSystem.Runtime.Core;
using HonamiAnimationSystem.Runtime.Timeline;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace HonamiAnimationSystem.Editor.Timeline
{
    internal sealed partial class TimelinePanelView
    {
        private void DrawEmptyState()
        {
            string msg = _state.Mode switch
            {
                TimelineMode.HonamiTimeline => "No Timeline Selected",
                TimelineMode.HonamiState => "No State Selected",
                TimelineMode.HonamiClipEdit => "No Clip Selected",
                _ => "Select an item to edit"
            };

            var container = new VisualElement();
            container.style.position = Position.Absolute;
            container.style.left = _state.TrackHeaderWidth;
            container.style.right = 0;
            container.style.top = 0;
            container.style.bottom = 0;
            container.style.alignItems = Align.Center;
            container.style.justifyContent = Justify.Center;
            IgnorePicking(container);
            _content.Add(container);

            var box = new VisualElement();
            box.style.backgroundColor = new Color(0.06f, 0.065f, 0.075f, 0.98f);
            box.style.borderTopWidth = box.style.borderBottomWidth = box.style.borderLeftWidth = box.style.borderRightWidth = 1;
            box.style.borderTopColor = new Color(0.2f, 0.22f, 0.25f, 0.6f);
            box.style.borderBottomColor = new Color(0.02f, 0.02f, 0.02f, 0.8f);
            box.style.borderLeftColor = new Color(0.1f, 0.1f, 0.1f, 0.6f);
            box.style.borderRightColor = new Color(0.1f, 0.1f, 0.1f, 0.6f);
            box.style.borderTopLeftRadius = box.style.borderTopRightRadius = box.style.borderBottomLeftRadius = box.style.borderBottomRightRadius = 14;
            box.style.paddingTop = box.style.paddingBottom = 40;
            box.style.paddingLeft = box.style.paddingRight = 40;
            box.style.alignItems = Align.Center;
            box.style.maxWidth = Length.Percent(90);
            box.style.minWidth = 300;
            container.Add(box);

            var icon = new VisualElement();
            icon.style.width = 72;
            icon.style.height = 72;
            icon.style.marginBottom = 20;
            var tex = _state.Mode == TimelineMode.HonamiClipEdit
                ? LoadClipIcon()
                : EditorGUIUtility.Load("Assets/HonamiAnimationSystem/Editor/Resources/Icons/HonamiTimeline.png") as Texture2D;
            if (tex != null)
            {
                icon.style.backgroundImage = tex;
                icon.style.backgroundSize = new BackgroundSize(BackgroundSizeType.Contain);
            }
            else
            {
                icon.style.backgroundColor = TimelineTheme.AccentSoft;
                icon.style.borderTopLeftRadius = icon.style.borderTopRightRadius = icon.style.borderBottomLeftRadius = icon.style.borderBottomRightRadius = 14;
            }
            box.Add(icon);

            var title = new Label("Honami Animation System");
            title.style.fontSize = 22;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.color = TimelineTheme.Text;
            title.style.marginBottom = 8;
            title.style.whiteSpace = WhiteSpace.Normal;
            title.style.unityTextAlign = TextAnchor.MiddleCenter;
            box.Add(title);

            var subtitle = new Label(msg);
            subtitle.style.fontSize = 15;
            subtitle.style.color = TimelineTheme.MutedText;
            subtitle.style.marginBottom = 30;
            subtitle.style.whiteSpace = WhiteSpace.Normal;
            subtitle.style.unityTextAlign = TextAnchor.MiddleCenter;
            box.Add(subtitle);

            if (_state.Mode == TimelineMode.HonamiClipEdit)
            {
                box.Add(CTAButton("Create New Animation Clip", CreateNewClip));
                var hint = new Label("Please select a clip or create a new one to start recording.");
                hint.style.fontSize = 12;
                hint.style.color = TimelineTheme.MutedText;
                hint.style.marginTop = 15;
                box.Add(hint);
            }
            else if (_state.Mode == TimelineMode.HonamiTimeline)
            {
                box.Add(CTAButton("Create New Timeline", CreateNewTimeline));
            }
            else if (_state.Mode == TimelineMode.HonamiState)
            {
                box.Add(CTAButton("Select Honami Controller", () =>
                {
                    EditorApplication.delayCall += () =>
                        EditorGUIUtility.ShowObjectPicker<HonamiController>(null, false, "", 0);
                }));
            }
        }

        private static Texture2D _clipIcon;

        private static Texture2D LoadClipIcon()
        {
            if (_clipIcon != null) return _clipIcon;

            _clipIcon = EditorGUIUtility.FindTexture("AnimationClip Icon@2x");
            if (_clipIcon == null)
                _clipIcon = EditorGUIUtility.Load("Icons/AnimationClip Icon@2x.png") as Texture2D;
            if (_clipIcon == null)
                _clipIcon = EditorGUIUtility.IconContent("AnimationClip Icon").image as Texture2D;
            return _clipIcon;
        }

        private static Button CTAButton(string text, Action onClick)
        {
            var btn = new Button(onClick) { text = text };
            btn.style.height = 36;
            btn.style.paddingLeft = btn.style.paddingRight = 30;
            btn.style.backgroundColor = TimelineTheme.Accent;
            btn.style.color = Color.white;
            btn.style.unityFontStyleAndWeight = FontStyle.Bold;
            btn.style.fontSize = 13;
            btn.style.borderTopLeftRadius = btn.style.borderTopRightRadius = btn.style.borderBottomLeftRadius = btn.style.borderBottomRightRadius = 8;
            btn.style.borderTopWidth = btn.style.borderBottomWidth = btn.style.borderLeftWidth = btn.style.borderRightWidth = 1;
            btn.style.borderTopColor = btn.style.borderBottomColor = btn.style.borderLeftColor = btn.style.borderRightColor = new Color(1, 1, 1, 0.1f);
            btn.style.marginTop = 10;

            btn.RegisterCallback<PointerEnterEvent>(_ =>
            {
                btn.style.backgroundColor = Color.Lerp(TimelineTheme.Accent, Color.white, 0.2f);
                btn.style.borderTopColor = btn.style.borderBottomColor = btn.style.borderLeftColor = btn.style.borderRightColor = new Color(1, 1, 1, 0.4f);
            });
            btn.RegisterCallback<PointerLeaveEvent>(_ =>
            {
                btn.style.backgroundColor = TimelineTheme.Accent;
                btn.style.borderTopColor = btn.style.borderBottomColor = btn.style.borderLeftColor = btn.style.borderRightColor = new Color(1, 1, 1, 0.1f);
            });
            btn.RegisterCallback<PointerDownEvent>(_ =>
            {
                btn.style.backgroundColor = Color.Lerp(TimelineTheme.Accent, Color.black, 0.1f);
                btn.style.borderTopColor = btn.style.borderBottomColor = btn.style.borderLeftColor = btn.style.borderRightColor = new Color(1, 1, 1, 0.2f);
            });
            btn.RegisterCallback<PointerUpEvent>(_ =>
            {
                btn.style.backgroundColor = Color.Lerp(TimelineTheme.Accent, Color.white, 0.2f);
                btn.style.borderTopColor = btn.style.borderBottomColor = btn.style.borderLeftColor = btn.style.borderRightColor = new Color(1, 1, 1, 0.4f);
            });

            return btn;
        }

        private void CreateNewTimeline()
        {
            string path = EditorUtility.SaveFilePanelInProject("Create New Honami Timeline", "New Timeline", "asset", "Please enter a file name");
            if (string.IsNullOrEmpty(path)) return;
            var timeline = ScriptableObject.CreateInstance<HonamiTimeline>();
            AssetDatabase.CreateAsset(timeline, path);
            AssetDatabase.SaveAssets();
            _state.ActiveTimeline = timeline;
            _rebuild();
        }

        private void CreateNewClip()
        {
            string path = EditorUtility.SaveFilePanelInProject("Create New Animation Clip", "New Animation", "anim", "Please enter a file name to save the animation clip to");
            if (string.IsNullOrEmpty(path)) return;

            var clip = new AnimationClip();
            AssetDatabase.CreateAsset(clip, path);
            AssetDatabase.SaveAssets();
            _state.ActiveClip = clip;
            _rebuild();
        }
    }
}
#endif
