using UnityEditor;
using UnityEngine;
using HonamiAnimationSystem.Runtime.Timeline;

namespace HonamiAnimationSystem.Editor.Sequence
{
    [CustomEditor(typeof(HonamiTimeline))]
    public sealed class HonamiTimelineEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var tl = (HonamiTimeline)target;

            EditorGUILayout.Space(4);
            if (GUILayout.Button("Open in Timeline Window", GUILayout.Height(30)))
                HonamiAnimationSystem.Editor.Timeline.HonamiTimelineWindow.InspectTimeline(tl);

            EditorGUILayout.Space(8);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("timelineName"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("durationOverride"),
                new GUIContent("Duration Override", "0 = auto from tracks"));

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Tracks", EditorStyles.boldLabel);

            var tracksP = serializedObject.FindProperty("tracks");
            for (int i = 0; i < tracksP.arraySize; i++)
            {
                var tp = tracksP.GetArrayElementAtIndex(i);
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(tp.FindPropertyRelative("trackName"), GUIContent.none, GUILayout.ExpandWidth(true));
                    EditorGUILayout.PropertyField(tp.FindPropertyRelative("trackType"), GUIContent.none, GUILayout.Width(100));
                    tp.FindPropertyRelative("muted").boolValue = GUILayout.Toggle(
                        tp.FindPropertyRelative("muted").boolValue,
                        new GUIContent("M", "Mute track"), GUILayout.Width(20));
                    if (GUILayout.Button(HonamiEditorSymbols.Remove, GUILayout.Width(22)))
                    {
                        tracksP.DeleteArrayElementAtIndex(i);
                        serializedObject.ApplyModifiedProperties();
                        return;
                    }
                    EditorGUILayout.EndHorizontal();

                    var trackTypeProp = tp.FindPropertyRelative("trackType");
                    if (trackTypeProp.enumValueIndex == (int)HonamiTimelineTrackType.Animation)
                    {
                        EditorGUILayout.PropertyField(tp.FindPropertyRelative("target"), new GUIContent("Target GameObject"));
                        EditorGUILayout.PropertyField(tp.FindPropertyRelative("clips"), new GUIContent("Clips"), true);
                    }
                    else
                    {
                        EditorGUILayout.PropertyField(tp.FindPropertyRelative("events"), new GUIContent("Events"), true);
                    }
                }
                EditorGUILayout.Space(2);
            }

            EditorGUILayout.Space(4);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+ Animation Track"))
            {
                tracksP.InsertArrayElementAtIndex(tracksP.arraySize);
                var np = tracksP.GetArrayElementAtIndex(tracksP.arraySize - 1);
                np.FindPropertyRelative("trackName").stringValue = "Animation Track";
                np.FindPropertyRelative("trackType").enumValueIndex = (int)HonamiTimelineTrackType.Animation;
            }
            if (GUILayout.Button("+ Event Track"))
            {
                tracksP.InsertArrayElementAtIndex(tracksP.arraySize);
                var np = tracksP.GetArrayElementAtIndex(tracksP.arraySize - 1);
                np.FindPropertyRelative("trackName").stringValue = "Event Track";
                np.FindPropertyRelative("trackType").enumValueIndex = (int)HonamiTimelineTrackType.Event;
            }
            EditorGUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
