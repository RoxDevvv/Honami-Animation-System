#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace HonamiAnimationSystem.Editor
{
    /// <summary>
    /// Factory for the shared toolbar look: buttons, icon buttons and toggles with the
    /// common Honami hover/press styling, reused across the editor windows.
    /// </summary>
    internal static class HonamiToolbarControls
    {
        public static Button ToolbarButton(string text, Action onClick)
        {
            var button = new Button(onClick) { text = text };
            StyleToolbarElement(button);
            return button;
        }

        public static Button IconButton(string iconName, string fallback, Action onClick)
        {
            var button = ToolbarButton("", onClick);
            button.tooltip = fallback;
            button.style.paddingLeft = button.style.paddingRight = 0;
            button.style.justifyContent = Justify.Center;
            button.style.alignItems = Align.Center;

            var icon = EditorGUIUtility.IconContent(iconName).image;
            if (icon != null)
            {
                button.Add(new Image { image = icon, style = { width = 16, height = 16, flexShrink = 0 } });
                button.style.width = 32;
            }
            else
            {
                button.text = fallback;
                button.style.minWidth = 45;
                button.style.paddingLeft = button.style.paddingRight = 7;
            }
            return button;
        }

        public static Toggle ToolbarToggle(string text, bool value, Action<bool> changed)
        {
            var toggle = new Toggle { text = text, value = value };
            StyleToolbarElement(toggle);
            toggle.RegisterValueChangedCallback(evt => changed(evt.newValue));
            return toggle;
        }

        public static void StyleToolbarElement(VisualElement element)
        {
            element.style.height = 24;
            element.style.marginLeft = 2;
            element.style.marginRight = 2;
            element.style.paddingLeft = 7;
            element.style.paddingRight = 7;
            element.style.backgroundColor = HonamiEditorTheme.ToolbarButton;
            element.style.borderTopWidth = element.style.borderRightWidth = element.style.borderBottomWidth = element.style.borderLeftWidth = 1;
            element.style.borderTopColor = element.style.borderRightColor = element.style.borderBottomColor = element.style.borderLeftColor = HonamiEditorTheme.SubtleLine;
            element.style.borderTopLeftRadius = element.style.borderTopRightRadius = element.style.borderBottomLeftRadius = element.style.borderBottomRightRadius = 4;

            element.RegisterCallback<PointerEnterEvent>(_ =>
            {
                if (!element.enabledSelf) return;
                element.style.backgroundColor = HonamiEditorTheme.ToolbarButtonHot;
                element.style.borderTopColor = element.style.borderRightColor = element.style.borderBottomColor = element.style.borderLeftColor = HonamiEditorTheme.AccentDim;
            });
            element.RegisterCallback<PointerLeaveEvent>(_ =>
            {
                element.style.backgroundColor = HonamiEditorTheme.ToolbarButton;
                element.style.borderTopColor = element.style.borderRightColor = element.style.borderBottomColor = element.style.borderLeftColor = HonamiEditorTheme.SubtleLine;
            });
            element.RegisterCallback<PointerDownEvent>(_ =>
            {
                if (!element.enabledSelf) return;
                element.style.backgroundColor = HonamiEditorTheme.ToolbarButtonPressed;
                element.style.borderTopColor = element.style.borderRightColor = element.style.borderBottomColor = element.style.borderLeftColor = HonamiEditorTheme.Accent;
            });
            element.RegisterCallback<PointerUpEvent>(_ =>
            {
                if (!element.enabledSelf) return;
                element.style.backgroundColor = HonamiEditorTheme.ToolbarButtonHot;
                element.style.borderTopColor = element.style.borderRightColor = element.style.borderBottomColor = element.style.borderLeftColor = HonamiEditorTheme.AccentDim;
            });
        }
    }
}
#endif
