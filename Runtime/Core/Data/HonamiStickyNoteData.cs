using System;
using UnityEngine;

namespace HonamiAnimationSystem.Runtime.Core
{
    /// <summary>
    /// Serialized editor graph sticky-note metadata stored with a Honami controller.
    /// </summary>
    [Serializable]
    public sealed class HonamiStickyNoteData
    {
        public string title = "Sticky Note";
        public string contents = "Type something here...";
        public Vector2 position;
        public Vector2 size = new Vector2(200, 160);
        public string guid;
        public int layerIndex;
        public Color themeColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);

        public HonamiStickyNoteData()
        {
            guid = Guid.NewGuid().ToString();
        }
    }
}
