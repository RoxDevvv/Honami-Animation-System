using System;
using System.Collections.Generic;
using UnityEngine;

namespace HonamiAnimationSystem.Runtime.Core
{
    /// <summary>
    /// Maps a parent state GUID to an override node.
    /// </summary>
    [Serializable]
    public struct HonamiNodeOverride
    {
        public string stateGuid;
        public HonamiNodeBase overrideNode;
    }

    /// <summary>
    /// Maps a parent state GUID to an override transition list.
    /// </summary>
    [Serializable]
    public struct HonamiStateTransitionsOverride
    {
        public string stateGuid;
        public List<HonamiTransition> transitions;
    }

    /// <summary>
    /// Runtime controller that layers node, state, parameter, and transition overrides on top of another controller.
    /// </summary>
    public sealed class HonamiOverrideController : HonamiRuntimeController
    {
        [Tooltip("The base controller to inherit layers, parameters, and states from.")]
        public HonamiRuntimeController parentController;

        [Tooltip("List of nodes overridden from the parent controller.")]
        public List<HonamiNodeOverride> nodeOverrides = new List<HonamiNodeOverride>();

        [Tooltip("Additional layers exclusively for this override controller.")]
        public List<HonamiLayer> additionalLayers = new List<HonamiLayer>();

        [Tooltip("Additional parameters exclusively for this override controller.")]
        public List<HonamiParameter> additionalParameters = new List<HonamiParameter>();

        [HideInInspector]
        public List<HonamiState> additionalStates = new List<HonamiState>();

        [HideInInspector]
        public List<HonamiGroupData> additionalGroups = new List<HonamiGroupData>();

        [HideInInspector]
        public List<HonamiStickyNoteData> additionalStickyNotes = new List<HonamiStickyNoteData>();

        [HideInInspector]
        public List<HonamiStateTransitionsOverride> transitionOverrides = new List<HonamiStateTransitionsOverride>();

        private CompositeListView<HonamiLayer> _layersView;
        private CompositeListView<HonamiParameter> _parametersView;
        private CompositeListView<HonamiState> _statesView;
        private Dictionary<string, HonamiNodeBase> _guidToNodeCache;

        public override IReadOnlyList<HonamiLayer> ActiveLayers
            => _layersView ??= new CompositeListView<HonamiLayer>(
                GetParentLayers,
                () => additionalLayers);

        public override IReadOnlyList<HonamiParameter> ActiveParameters
            => _parametersView ??= new CompositeListView<HonamiParameter>(
                GetParentParameters,
                () => additionalParameters);

        public override IReadOnlyList<HonamiState> ActiveStates
            => _statesView ??= new CompositeListView<HonamiState>(
                GetParentStates,
                () => additionalStates);

        public override HonamiNodeBase GetActiveNode(HonamiState state)
        {
            if (state == null)
            {
                return null;
            }

            if (state.guid == null)
            {
                return state.node;
            }

            EnsureGuidCache();
            return _guidToNodeCache.TryGetValue(state.guid, out var node) ? node : state.node;
        }

        public override HonamiNodeBase GetActiveNodeByGuid(string guid)
        {
            EnsureGuidCache();

            if (guid != null && _guidToNodeCache.TryGetValue(guid, out var node))
            {
                return node;
            }

            return null;
        }

        public override IReadOnlyList<HonamiTransition> GetTransitions(HonamiState state)
        {
            if (state == null)
            {
                return null;
            }

            if (transitionOverrides != null)
            {
                foreach (var transitionOverride in transitionOverrides)
                {
                    if (transitionOverride.stateGuid == state.guid)
                    {
                        return transitionOverride.transitions;
                    }
                }
            }

            return state.transitions;
        }

        /// <summary>
        /// Clears all runtime views and override lookup caches.
        /// </summary>
        public void ClearCaches()
        {
            _layersView?.Invalidate();
            _parametersView?.Invalidate();
            _statesView?.Invalidate();
            _guidToNodeCache = null;
        }

        private void OnEnable()
        {
            ClearCaches();
        }

        private IReadOnlyList<HonamiLayer> GetParentLayers()
        {
            return parentController != null && parentController != this ? parentController.ActiveLayers : null;
        }

        private IReadOnlyList<HonamiParameter> GetParentParameters()
        {
            return parentController != null && parentController != this ? parentController.ActiveParameters : null;
        }

        private IReadOnlyList<HonamiState> GetParentStates()
        {
            return parentController != null && parentController != this ? parentController.ActiveStates : null;
        }

        private void EnsureGuidCache()
        {
            if (_guidToNodeCache != null)
            {
                return;
            }

            _guidToNodeCache = new Dictionary<string, HonamiNodeBase>();
            AddParentNodesToCache();
            AddAdditionalStatesToCache();
            ApplyNodeOverridesToCache();
        }

        private void AddParentNodesToCache()
        {
            if (parentController == null || parentController == this)
            {
                return;
            }

            foreach (var state in parentController.ActiveStates)
            {
                if (state != null && state.guid != null)
                {
                    _guidToNodeCache[state.guid] = parentController.GetActiveNodeByGuid(state.guid);
                }
            }
        }

        private void AddAdditionalStatesToCache()
        {
            if (additionalStates == null)
            {
                return;
            }

            foreach (var state in additionalStates)
            {
                if (state != null && state.guid != null)
                {
                    _guidToNodeCache[state.guid] = state.node;
                }
            }
        }

        private void ApplyNodeOverridesToCache()
        {
            if (nodeOverrides == null)
            {
                return;
            }

            foreach (var nodeOverride in nodeOverrides)
            {
                if (nodeOverride.stateGuid != null && nodeOverride.overrideNode != null)
                {
                    _guidToNodeCache[nodeOverride.stateGuid] = nodeOverride.overrideNode;
                }
            }
        }

#if UNITY_EDITOR
        public override HonamiController BaseController => parentController != null ? parentController.BaseController : null;
        public override bool IsOverride => true;

        private void OnValidate()
        {
            ClearCaches();

            if (HonamiAssetImportGuard.IsImportingAssets)
            {
                return;
            }

            RemoveCircularParentReference();
        }

        private void RemoveCircularParentReference()
        {
            if (parentController == null)
            {
                return;
            }

            var current = parentController;
            int depth = 0;

            while (current != null && current is HonamiOverrideController overrideController)
            {
                if (overrideController == this)
                {
                    Debug.LogError($"[Honami] Circular dependency detected in HonamiOverrideController '{name}'. Removing parent controller.");
                    parentController = null;
                    break;
                }

                if (depth > 20)
                {
                    Debug.LogError("[Honami] Too many nested override controllers. Removing parent controller.");
                    parentController = null;
                    break;
                }

                current = overrideController.parentController;
                depth++;
            }
        }
#endif

        private sealed class CompositeListView<T> : IReadOnlyList<T>
        {
            private readonly Func<IReadOnlyList<T>> _getParentItems;
            private readonly Func<IReadOnlyList<T>> _getLocalItems;
            private T[] _cache;

            public CompositeListView(Func<IReadOnlyList<T>> getParentItems, Func<IReadOnlyList<T>> getLocalItems)
            {
                _getParentItems = getParentItems;
                _getLocalItems = getLocalItems;
            }

            public T this[int index]
            {
                get
                {
                    EnsureCache();
                    return _cache[index];
                }
            }

            public int Count
            {
                get
                {
                    EnsureCache();
                    return _cache.Length;
                }
            }

            public void Invalidate() => _cache = null;

            public IEnumerator<T> GetEnumerator()
            {
                EnsureCache();

                for (int i = 0; i < _cache.Length; i++)
                {
                    yield return _cache[i];
                }
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

            private void EnsureCache()
            {
                if (_cache != null)
                {
                    return;
                }

                var list = new List<T>();
                AddRangeIfPresent(list, _getParentItems?.Invoke());
                AddRangeIfPresent(list, _getLocalItems?.Invoke());
                _cache = list.ToArray();
            }

            private static void AddRangeIfPresent(List<T> destination, IReadOnlyList<T> source)
            {
                if (source == null)
                {
                    return;
                }

                for (int i = 0; i < source.Count; i++)
                {
                    destination.Add(source[i]);
                }
            }
        }
    }
}
