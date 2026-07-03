using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace HonamiAnimationSystem.Editor
{
    public enum HonamiNotificationType
    {
        Info,
        Warning,
        Error,
        Success
    }

    public sealed class HonamiNotificationPanel : VisualElement
    {
        private static readonly List<HonamiNotificationPanel> _activePanels = new List<HonamiNotificationPanel>();

        private const int MaxNotifications = 5;
        private const float DefaultDuration = 4f;

        public HonamiNotificationPanel()
        {
            pickingMode = PickingMode.Ignore;
            AddToClassList("honami-notification-container");

            var styleSheet = Resources.Load<StyleSheet>("HonamiNotificationStyle");
            if (styleSheet != null)
                styleSheets.Add(styleSheet);

            RegisterCallback<AttachToPanelEvent>(evt => _activePanels.Add(this));
            RegisterCallback<DetachFromPanelEvent>(evt => _activePanels.Remove(this));
        }

        public static void ShowGlobal(string title, string message, HonamiNotificationType type = HonamiNotificationType.Info, float duration = DefaultDuration)
        {
            if (_activePanels.Count == 0)
            {
                Debug.Log($"[Honami] {title}: {message}");
                return;
            }

            foreach (var panel in _activePanels)
            {
                panel.Show(title, message, type, duration);
            }
        }

        public void Show(string title, string message, HonamiNotificationType type = HonamiNotificationType.Info, float duration = DefaultDuration)
        {
            if (childCount >= MaxNotifications)
            {
                RemoveAt(0);
            }

            NotificationItem item = null;
            item = new NotificationItem(title, message, type, () => Remove(item));
            Add(item);

            EditorApplication.delayCall += () =>
            {
                item.AddToClassList("honami-notification-item--visible");
            };

            // Auto-hide
            if (duration > 0)
            {
                item.schedule.Execute(() =>
                {
                    HideItem(item);
                }).StartingIn((long)(duration * 1000));
            }
        }

        private void HideItem(NotificationItem item)
        {
            if (item == null || item.parent == null) return;

            item.RemoveFromClassList("honami-notification-item--visible");

            // Remove after animation finish
            item.RegisterCallback<TransitionEndEvent>(evt =>
            {
                if (item.parent != null)
                    Remove(item);
            });

            // Fallback if transition doesn't fire
            item.schedule.Execute(() =>
            {
                if (item.parent != null)
                    Remove(item);
            }).StartingIn(350);
        }

        private class NotificationItem : VisualElement
        {
            public NotificationItem(string title, string message, HonamiNotificationType type, System.Action onClose)
            {
                AddToClassList("honami-notification-item");
                AddToClassList($"honami-notification-item--{type.ToString().ToLower()}");

                // Icon
                var icon = new VisualElement();
                icon.AddToClassList("honami-notification-item__icon");
                string iconName = type switch
                {
                    HonamiNotificationType.Info => "console.infoicon.sml",
                    HonamiNotificationType.Warning => "console.warnicon.sml",
                    HonamiNotificationType.Error => "console.erroricon.sml",
                    HonamiNotificationType.Success => "FilterSelectedOnly",
                    _ => "console.infoicon.sml"
                };
                icon.style.backgroundImage = (StyleBackground)EditorGUIUtility.IconContent(iconName).image;
                Add(icon);

                // Text Container
                var textContainer = new VisualElement();
                textContainer.AddToClassList("honami-notification-item__text-container");
                Add(textContainer);

                // Title
                var titleLabel = new Label(title);
                titleLabel.AddToClassList("honami-notification-item__title");
                textContainer.Add(titleLabel);

                // Message
                if (!string.IsNullOrEmpty(message))
                {
                    var messageLabel = new Label(message);
                    messageLabel.AddToClassList("honami-notification-item__message");
                    textContainer.Add(messageLabel);
                }

                // Close Button
                var closeButton = new Button(() => onClose?.Invoke());
                closeButton.text = "×";
                closeButton.AddToClassList("honami-notification-item__close-button");
                Add(closeButton);
            }
        }
    }
}
