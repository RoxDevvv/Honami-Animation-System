#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using HonamiAnimationSystem.Runtime.Core;
using HonamiAnimationSystem.Runtime.Timeline;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using HonamiEditorController = HonamiAnimationSystem.Editor.Core.HonamiEditorController;

namespace HonamiAnimationSystem.Editor.Timeline
{
    internal sealed partial class TimelinePanelView
    {
        private struct RowData
        {
            public int Index;
            public string Title;
            public string IconName;
            public Color Color;
            public bool IsSelected;
            public HonamiTimelineTrack Track;

            public bool IsGroup;
            public string GroupId;
            public bool IsExpanded;
            public int BindingStart;
            public int BindingCount;
        }

        private sealed class ClipCurveCache
        {
            public AnimationClip Clip;
            public EditorCurveBinding[] Bindings;
            public AnimationCurve[] Curves;

            public void Clear()
            {
                Clip = null;
                Bindings = null;
                Curves = null;
            }
        }

        private readonly List<RowData> _cachedRows = new();
        private readonly ClipCurveCache _curveCache = new();

        private void EnsureCurveCache()
        {
            if (_curveCache.Clip == _state.ActiveClip && _curveCache.Bindings != null) return;

            _curveCache.Clear();
            _curveCache.Clip = _state.ActiveClip;
            _curveCache.Bindings = AnimationUtility.GetCurveBindings(_state.ActiveClip);
            int count = Mathf.Min(_curveCache.Bindings.Length, 12);
            _curveCache.Curves = new AnimationCurve[count];
            for (int i = 0; i < count; i++)
                _curveCache.Curves[i] = AnimationUtility.GetEditorCurve(_state.ActiveClip, _curveCache.Bindings[i]);
        }

        private bool HasProperties()
        {
            return _state.Mode == TimelineMode.HonamiTimeline && _state.ActiveTimeline != null
                   || _state.Mode == TimelineMode.HonamiState && _state.SelectedState != null
                   || _state.Mode == TimelineMode.HonamiClipEdit && _state.ActiveClip != null;
        }

        private List<RowData> GetRows()
        {
            _cachedRows.Clear();

            switch (_state.Mode)
            {
                case TimelineMode.HonamiTimeline:
                    CollectTimelineRows();
                    break;
                case TimelineMode.HonamiClipEdit:
                    CollectClipEditRows();
                    break;
                default:
                    CollectStateRows();
                    break;
            }
            return _cachedRows;
        }

        private void CollectTimelineRows()
        {
            if (_state.ActiveTimeline == null) return;
            var tracks = _state.ActiveTimeline.tracks;
            for (int i = 0; i < tracks.Count; i++)
            {
                var track = tracks[i];
                _cachedRows.Add(new RowData
                {
                    Index = i,
                    Title = string.IsNullOrEmpty(track.trackName) ? track.trackType.ToString() : track.trackName,
                    Track = track,
                    IconName = track.trackType == HonamiTimelineTrackType.Animation ? "AnimationClip Icon" : "AnimationWindowEvent Icon",
                    Color = track.trackType == HonamiTimelineTrackType.Animation ? TimelineTheme.AnimationTrack : TimelineTheme.EventTrack,
                    IsSelected = _state.SelectedTimelineTrack == track
                });
            }
        }

        private void CollectClipEditRows()
        {
            if (_state.ActiveClip == null) return;

            EnsureCurveCache();

            var bindings = _curveCache.Bindings;
            int bindIdx = 0;
            while (bindIdx < bindings.Length)
            {
                var b = bindings[bindIdx];
                string pathName = string.IsNullOrEmpty(b.path) ? "root" : System.IO.Path.GetFileName(b.path);
                string prop = b.propertyName;

                if (prop.StartsWith("m_LocalPosition") || prop.StartsWith("m_LocalRotation") || prop.StartsWith("m_LocalScale"))
                {
                    string baseProp = prop.Substring(0, prop.LastIndexOf('.'));
                    string groupId = b.path + ":" + baseProp;

                    int count = 1;
                    for (int j = bindIdx + 1; j < bindings.Length; j++)
                    {
                        if (bindings[j].path == b.path && bindings[j].propertyName.StartsWith(baseProp)) count++;
                        else break;
                    }

                    string propDisplay = baseProp == "m_LocalPosition" ? "Position" :
                                         baseProp == "m_LocalRotation" ? "Rotation" : "Scale";

                    bool isExpanded = _state.ExpandedClipGroups.Contains(groupId);

                    _cachedRows.Add(new RowData
                    {
                        Index = _cachedRows.Count,
                        Title = $"{pathName}  ▸  {propDisplay}",
                        IconName = "Transform Icon",
                        Color = new Color(0.8f, 0.8f, 0.8f, 0.8f),
                        IsGroup = true,
                        GroupId = groupId,
                        IsExpanded = isExpanded,
                        BindingStart = bindIdx,
                        BindingCount = count
                    });

                    if (isExpanded)
                    {
                        for (int j = 0; j < count; j++)
                        {
                            var subBinding = bindings[bindIdx + j];
                            string axis = subBinding.propertyName.Substring(subBinding.propertyName.LastIndexOf('.') + 1).ToUpper();
                            Color trackColor = axis == "X" ? new Color(0.9f, 0.3f, 0.3f) :
                                               axis == "Y" ? new Color(0.3f, 0.9f, 0.3f) :
                                               axis == "Z" ? new Color(0.3f, 0.5f, 1f) : new Color(0.7f, 0.7f, 0.7f);
                            _cachedRows.Add(new RowData
                            {
                                Index = _cachedRows.Count,
                                Title = $"      {axis}",
                                IconName = "",
                                Color = trackColor,
                                BindingStart = bindIdx + j,
                                BindingCount = 1
                            });
                        }
                    }
                    bindIdx += count;
                }
                else
                {
                    bool isBlendShape = prop.StartsWith("blendShape.");
                    Color color = isBlendShape ? new Color(0.8f, 0.4f, 0.8f) : new Color(0.35f, 0.65f, 0.95f, 0.8f);
                    if (isBlendShape) prop = prop.Substring("blendShape.".Length);

                    _cachedRows.Add(new RowData
                    {
                        Index = _cachedRows.Count,
                        Title = $"{pathName}  ▸  {prop}",
                        IconName = "AnimationClip Icon",
                        Color = color,
                        BindingStart = bindIdx,
                        BindingCount = 1
                    });
                    bindIdx++;
                }
            }
        }

        private void CollectStateRows()
        {
            int index = 0;
            if (_state.ShowAnimTrack)
            {
                if (_state.IsRandomState && _state.RandomNode.randomClips != null)
                {
                    var clips = _state.RandomNode.randomClips;
                    for (int i = 0; i < clips.Count; i++)
                        _cachedRows.Add(new RowData { Index = index++, Title = clips[i].clip != null ? clips[i].clip.name : $"Empty Slot {i}", IconName = "AnimationClip Icon", Color = TimelineTheme.RandomTrack });
                }
                else if (_state.IsBlendState && _state.BlendNode.blendMotions != null)
                {
                    var motions = _state.BlendNode.blendMotions;
                    for (int i = 0; i < motions.Count; i++)
                    {
                        var motion = motions[i];
                        string name = motion.clip != null ? motion.clip.name : $"Empty Motion {i}";
                        _cachedRows.Add(new RowData { Index = index++, Title = $"{name}  ({motion.threshold:F2})", IconName = "AnimationClip Icon", Color = TimelineTheme.PreviewTrack });
                    }
                }
                else if (_state.IsSeqState && _state.SeqNode.sequencedClips != null)
                {
                    var clips = _state.SeqNode.sequencedClips;
                    for (int i = 0; i < clips.Count; i++)
                        _cachedRows.Add(new RowData { Index = index++, Title = clips[i].clip != null ? clips[i].clip.name : $"Empty Sequence {i}", IconName = "AnimationClip Icon", Color = TimelineTheme.SequencerTrack });
                }
                else
                {
                    _cachedRows.Add(new RowData { Index = index++, Title = "Animation Clip", IconName = "AnimationClip Icon", Color = TimelineTheme.AnimationTrack });
                }
            }
            if (_state.ShowLocalEventsTrack) _cachedRows.Add(new RowData { Index = index++, Title = "Local Events", IconName = "AnimationWindowEvent Icon", Color = TimelineTheme.EventTrack });
            if (_state.ShowGlobalEventsTrack) _cachedRows.Add(new RowData { Index = index, Title = "Global Events", IconName = "AudioSource Icon", Color = new Color(0.75f, 0.42f, 0.22f, 0.95f) });
        }

        private float DisplayFps()
        {
            if (_state.Mode == TimelineMode.HonamiClipEdit)
                return _state.ActiveClip != null ? _state.ActiveClip.frameRate : 0f;

            var clip = _state.AnimNode?.clip;
            if (clip == null && _state.IsRandomState && _state.RandomNode.randomClips != null
                && _state.RandomPreviewIdx >= 0 && _state.RandomPreviewIdx < _state.RandomNode.randomClips.Count)
                clip = _state.RandomNode.randomClips[_state.RandomPreviewIdx].clip;
            if (clip == null && _state.IsBlendState && _state.BlendNode.blendMotions != null)
                clip = _state.BlendNode.blendMotions.FirstOrDefault(m => m.clip != null)?.clip;
            return clip != null ? clip.frameRate : 0f;
        }

        private static VisualElement RectElement(float x, float y, float width, float height, Color color)
        {
            return new VisualElement
            {
                style =
                {
                    position = Position.Absolute,
                    left = x,
                    top = y,
                    width = width,
                    height = height,
                    backgroundColor = color
                }
            };
        }

        private static void IgnorePicking(VisualElement element)
        {
            if (element != null)
                element.pickingMode = PickingMode.Ignore;
        }

        private static VisualElement ColorBar(Color color)
        {
            return new VisualElement
            {
                pickingMode = PickingMode.Ignore,
                style =
                {
                    position = Position.Absolute,
                    left = 0,
                    top = 0,
                    width = 4,
                    height = Length.Percent(100),
                    backgroundColor = color
                }
            };
        }

        private static VisualElement RowIcon(string iconName)
        {
            return new Image
            {
                image = EditorGUIUtility.IconContent(iconName).image,
                pickingMode = PickingMode.Ignore,
                style =
                {
                    position = Position.Absolute,
                    left = 13,
                    top = 12,
                    width = 16,
                    height = 16
                }
            };
        }

        private static Label RowLabel(string text)
        {
            return new Label(text)
            {
                pickingMode = PickingMode.Ignore,
                style =
                {
                    position = Position.Absolute,
                    left = 36,
                    right = 30,
                    top = 10,
                    height = 20,
                    color = TimelineTheme.Text,
                    fontSize = 12,
                    overflow = Overflow.Hidden
                }
            };
        }

        private Button MuteButton(HonamiTimelineTrack track)
        {
            var button = new Button(() =>
            {
                HonamiEditorController.ToggleTrackMute(_state.ActiveTimeline, track);
                _rebuild();
            })
            {
                text = track.muted ? "M" : "V",
                tooltip = track.muted ? "Muted" : "Visible"
            };
            button.style.position = Position.Absolute;
            button.style.right = 6;
            button.style.top = 9;
            button.style.width = 22;
            button.style.height = 22;
            button.style.backgroundColor = TimelineTheme.ToolbarButton;
            button.style.borderTopWidth = button.style.borderRightWidth = button.style.borderBottomWidth = button.style.borderLeftWidth = 1;
            button.style.borderTopColor = button.style.borderRightColor = button.style.borderBottomColor = button.style.borderLeftColor = TimelineTheme.SubtleLine;
            button.style.color = TimelineTheme.Text;
            button.RegisterCallback<PointerEnterEvent>(_ => button.style.backgroundColor = TimelineTheme.ToolbarButtonHot);
            button.RegisterCallback<PointerLeaveEvent>(_ => button.style.backgroundColor = TimelineTheme.ToolbarButton);
            button.RegisterCallback<PointerDownEvent>(_ => button.style.backgroundColor = TimelineTheme.ToolbarButtonPressed);
            button.RegisterCallback<PointerUpEvent>(_ => button.style.backgroundColor = TimelineTheme.ToolbarButtonHot);
            return button;
        }
    }
}
#endif
