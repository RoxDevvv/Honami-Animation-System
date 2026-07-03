#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace HonamiAnimationSystem.Editor.Timeline
{
    internal sealed class TimelineTabBar : VisualElement
    {
        private const float BarHeight = 24f;

        private readonly Func<IReadOnlyList<TimelineState>> _getTabs;
        private readonly Func<int> _getActive;
        private readonly Action<int> _onSelect;
        private readonly Action<int> _onClose;
        private readonly Action _onAdd;
        private readonly ScrollView _strip;

        public TimelineTabBar(
            Func<IReadOnlyList<TimelineState>> getTabs,
            Func<int> getActive,
            Action<int> onSelect,
            Action<int> onClose,
            Action onAdd)
        {
            _getTabs = getTabs;
            _getActive = getActive;
            _onSelect = onSelect;
            _onClose = onClose;
            _onAdd = onAdd;

            name = "honami-timeline-tabbar";
            style.height = BarHeight;
            style.flexShrink = 0;
            style.flexDirection = FlexDirection.Row;
            style.alignItems = Align.Stretch;
            style.backgroundColor = TimelineTheme.ToolbarBg;
            style.borderBottomWidth = 1;
            style.borderBottomColor = TimelineTheme.Divider;

            _strip = new ScrollView(ScrollViewMode.Horizontal);
            _strip.style.flexGrow = 1;
            _strip.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
            _strip.contentContainer.style.flexDirection = FlexDirection.Row;
            _strip.contentContainer.style.alignItems = Align.Stretch;
            _strip.contentContainer.style.height = Length.Percent(100);
            Add(_strip);

            Refresh();
        }

        public void Refresh()
        {
            _strip.Clear();
            var tabs = _getTabs();
            int active = _getActive();

            for (int i = 0; i < tabs.Count; i++)
                _strip.Add(TabElement(tabs[i], i, i == active));

            _strip.Add(AddButton());
        }

        private VisualElement TabElement(TimelineState state, int index, bool isActive)
        {
            var tab = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    flexShrink = 0,
                    height = Length.Percent(100),
                    paddingLeft = 10,
                    paddingRight = 4,
                    minWidth = 90,
                    maxWidth = 220,
                    backgroundColor = isActive ? TimelineTheme.PanelBg : Color.clear,
                    borderTopWidth = 2,
                    borderTopColor = isActive ? TimelineTheme.Accent : Color.clear,
                    borderRightWidth = 1,
                    borderRightColor = TimelineTheme.Divider
                }
            };

            tab.RegisterCallback<PointerDownEvent>(evt =>
            {
                if (evt.button == 2)
                {
                    _onClose(index);
                    evt.StopPropagation();
                    return;
                }
                if (evt.button != 0) return;
                _onSelect(index);
                evt.StopPropagation();
            });

            if (!isActive)
            {
                tab.RegisterCallback<PointerEnterEvent>(_ => tab.style.backgroundColor = TimelineTheme.ToolbarButtonHot);
                tab.RegisterCallback<PointerLeaveEvent>(_ => tab.style.backgroundColor = Color.clear);
            }

            var icon = new Image
            {
                image = TabIcon(state),
                pickingMode = PickingMode.Ignore,
                tintColor = isActive ? Color.white : new Color(0.6f, 0.6f, 0.6f, 1f),
                style =
                {
                    width = 14,
                    height = 14,
                    marginRight = 5,
                    flexShrink = 0
                }
            };
            tab.Add(icon);

            if (state.PreviewEnabled)
            {
                tab.Add(new VisualElement
                {
                    pickingMode = PickingMode.Ignore,
                    style =
                    {
                        width = 7,
                        height = 7,
                        marginRight = 6,
                        flexShrink = 0,
                        backgroundColor = TimelineTheme.Accent,
                        borderTopLeftRadius = 4,
                        borderTopRightRadius = 4,
                        borderBottomLeftRadius = 4,
                        borderBottomRightRadius = 4
                    },
                    tooltip = "Preview enabled — contributes to scene sampling"
                });
            }

            var label = new Label(TabTitle(state))
            {
                pickingMode = PickingMode.Ignore,
                style =
                {
                    color = isActive ? TimelineTheme.Text : TimelineTheme.MutedText,
                    fontSize = 11,
                    unityFontStyleAndWeight = isActive ? FontStyle.Bold : FontStyle.Normal,
                    unityTextAlign = TextAnchor.MiddleLeft,
                    overflow = Overflow.Hidden,
                    flexShrink = 1,
                    marginRight = 4
                }
            };
            tab.Add(label);

            tab.Add(CloseButton(index));
            return tab;
        }

        private VisualElement CloseButton(int index)
        {
            var close = new Label("×")
            {
                tooltip = "Close tab (middle click also works)",
                style =
                {
                    width = 16,
                    height = 16,
                    flexShrink = 0,
                    unityTextAlign = TextAnchor.MiddleCenter,
                    fontSize = 13,
                    color = TimelineTheme.MutedText,
                    borderTopLeftRadius = 3,
                    borderTopRightRadius = 3,
                    borderBottomLeftRadius = 3,
                    borderBottomRightRadius = 3
                }
            };
            close.RegisterCallback<PointerEnterEvent>(_ =>
            {
                close.style.color = Color.white;
                close.style.backgroundColor = new Color(0.75f, 0.22f, 0.28f, 0.9f);
            });
            close.RegisterCallback<PointerLeaveEvent>(_ =>
            {
                close.style.color = TimelineTheme.MutedText;
                close.style.backgroundColor = Color.clear;
            });
            close.RegisterCallback<PointerDownEvent>(evt =>
            {
                if (evt.button != 0) return;
                _onClose(index);
                evt.StopPropagation();
            });
            return close;
        }

        private VisualElement AddButton()
        {
            var add = new Label("+")
            {
                tooltip = "New tab",
                style =
                {
                    width = 26,
                    height = Length.Percent(100),
                    flexShrink = 0,
                    unityTextAlign = TextAnchor.MiddleCenter,
                    fontSize = 15,
                    color = TimelineTheme.MutedText
                }
            };
            add.RegisterCallback<PointerEnterEvent>(_ =>
            {
                add.style.color = TimelineTheme.Text;
                add.style.backgroundColor = TimelineTheme.ToolbarButtonHot;
            });
            add.RegisterCallback<PointerLeaveEvent>(_ =>
            {
                add.style.color = TimelineTheme.MutedText;
                add.style.backgroundColor = Color.clear;
            });
            add.RegisterCallback<PointerDownEvent>(evt =>
            {
                if (evt.button != 0) return;
                _onAdd();
                evt.StopPropagation();
            });
            return add;
        }

        internal static string TabTitle(TimelineState state)
        {
            return state.Mode switch
            {
                TimelineMode.HonamiTimeline => state.ActiveTimeline != null ? state.ActiveTimeline.name : "Timeline",
                TimelineMode.HonamiClipEdit => state.ActiveClip != null ? state.ActiveClip.name : "Clip",
                _ => state.SelectedState?.stateName
                     ?? (state.Controller != null ? state.Controller.name : "State")
            };
        }

        private static Texture TabIcon(TimelineState state)
        {
            return state.Mode switch
            {
                TimelineMode.HonamiTimeline => HonamiEditorIcons.TimelineWhite,
                TimelineMode.HonamiClipEdit => UnityEditor.EditorGUIUtility.IconContent("AnimationClip Icon").image,
                _ => HonamiEditorIcons.Controller
            };
        }
    }
}
#endif
