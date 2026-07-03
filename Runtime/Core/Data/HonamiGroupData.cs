using System;
using System.Collections.Generic;
using UnityEngine;

namespace HonamiAnimationSystem.Runtime.Core
{
    /// <summary>
    /// Serialized editor graph group metadata stored with a Honami controller.
    /// </summary>
    [Serializable]
    public sealed class HonamiGroupData
    {
        public string title = "New Group";
        public Vector2 position;
        public Vector2 size;
        public string guid;
        public List<string> containedNodes = new List<string>();
        public int layerIndex;

        public HonamiGroupData()
        {
            guid = Guid.NewGuid().ToString();
        }
    }
}
