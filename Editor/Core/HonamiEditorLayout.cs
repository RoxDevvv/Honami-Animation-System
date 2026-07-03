#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UIElements;

namespace HonamiAnimationSystem.Editor
{
    public static class HonamiEditorLayout
    {
        public const float HeaderHeight = 52f;
        public const float PanelMinWidth = 0f;
        public const float InspectorPadding = 12f;

        public static readonly Color HeaderBg = new(0.07f, 0.074f, 0.082f);
        public static readonly Color HeaderBorder = new(0f, 0f, 0f, 0.68f);
        public static readonly Color PanelBorder = HonamiEditorTheme.Divider;
        public static readonly Color MutedText = HonamiEditorTheme.MutedText;
        public static readonly Color Warning = new(0.90f, 0.50f, 0.18f);
        public static readonly Color Danger = new(0.95f, 0.25f, 0.25f);

        public static void PrepareRoot(VisualElement root)
        {
            root.Clear();
            root.style.backgroundColor = HonamiGraphStyles.WindowBg;
            root.style.flexDirection = FlexDirection.Column;
        }

        public static VisualElement Header(Texture2D icon, string title, string section, out Label titleLabel)
        {
            VisualElement header = new()
            {
                style =
                {
                    height = HeaderHeight,
                    backgroundColor = HeaderBg,
                    borderBottomWidth = 1,
                    borderBottomColor = HeaderBorder,
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    paddingLeft = 18,
                    paddingRight = 18,
                    flexShrink = 0
                }
            };

            if (icon != null)
            {
                VisualElement titleIcon = new()
                {
                    style =
                    {
                        width = 24,
                        height = 24,
                        backgroundImage = icon,
                        marginRight = 10,
                        flexShrink = 0
                    }
                };
                header.Add(titleIcon);
            }

            VisualElement titleContainer = new()
            {
                style =
                {
                    flexDirection = FlexDirection.Column,
                    minWidth = 0
                }
            };

            titleLabel = new Label(title)
            {
                style =
                {
                    fontSize = 18,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    color = HonamiGraphStyles.TitleClr,
                    whiteSpace = WhiteSpace.NoWrap
                }
            };
            titleContainer.Add(titleLabel);
            titleContainer.Add(Breadcrumb("Honami", section));
            header.Add(titleContainer);
            header.Add(HonamiGraphStyles.Spacer());

            return header;
        }

        public static VisualElement Breadcrumb(string root, string section)
        {
            VisualElement breadcrumbs = new()
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    marginTop = -2
                }
            };
            breadcrumbs.Add(HonamiGraphStyles.MiniLabel(root, HonamiGraphStyles.AccentHeader));
            breadcrumbs.Add(HonamiGraphStyles.MiniLabel(" / ", new Color(0.34f, 0.35f, 0.37f)));
            breadcrumbs.Add(HonamiGraphStyles.MiniLabel(section, MutedText));
            return breadcrumbs;
        }

        public static Label Badge(string text, Color color)
        {
            Label badge = new(text)
            {
                style =
                {
                    backgroundColor = color,
                    color = Color.white,
                    paddingLeft = 6,
                    paddingRight = 6,
                    paddingTop = 2,
                    paddingBottom = 2,
                    marginLeft = 10,
                    borderBottomLeftRadius = 4,
                    borderBottomRightRadius = 4,
                    borderTopLeftRadius = 4,
                    borderTopRightRadius = 4,
                    fontSize = 10,
                    unityFontStyleAndWeight = FontStyle.Bold
                }
            };
            return badge;
        }

        public static void StyleSidePanel(VisualElement panel, bool leftBorder = false, bool rightBorder = false)
        {
            panel.style.backgroundColor = HonamiGraphStyles.PanelBg;
            panel.style.minWidth = PanelMinWidth;
            panel.style.minHeight = 0;
            panel.style.overflow = Overflow.Hidden;

            if (leftBorder)
            {
                panel.style.borderLeftWidth = 1;
                panel.style.borderLeftColor = PanelBorder;
            }

            if (rightBorder)
            {
                panel.style.borderRightWidth = 1;
                panel.style.borderRightColor = PanelBorder;
            }
        }

        public static VisualElement PaddedInspectorRoot()
        {
            VisualElement root = new()
            {
                style =
                {
                    paddingLeft = InspectorPadding,
                    paddingRight = InspectorPadding,
                    paddingTop = InspectorPadding,
                    paddingBottom = InspectorPadding
                }
            };
            return root;
        }

        public static Button HeaderButton(string text, System.Action onClick)
        {
            Button button = HonamiGraphStyles.SmallButton(text, onClick);
            button.style.width = 84;
            button.style.height = 26;
            button.style.marginLeft = 8;
            return button;
        }
    }
}
#endif
