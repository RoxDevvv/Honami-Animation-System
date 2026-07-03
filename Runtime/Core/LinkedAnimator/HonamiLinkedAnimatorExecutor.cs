using System.Collections.Generic;
using UnityEngine;

namespace HonamiAnimationSystem.Runtime.Core
{
    public sealed class HonamiLinkedAnimatorExecutor
    {
        private readonly List<ActiveEventExecution> _activeExecutions = new();
        private HonamiLinkedAnimator _brain;

        public bool HasActiveExecutions => _activeExecutions.Count > 0;
        public IReadOnlyList<HonamiLinkedAnimatorNodeBase> ActiveNodes
        {
            get
            {
                var list = new List<HonamiLinkedAnimatorNodeBase>(_activeExecutions.Count);
                foreach (var ex in _activeExecutions) list.Add(ex.Node);
                return list;
            }
        }

        public void Initialize(HonamiLinkedAnimator brain)
        {
            _brain = brain;
        }

        public void TriggerEvent(HonamiLinkedAnimatorGraph graph, string eventName)
        {
            if (graph == null || string.IsNullOrEmpty(eventName)) return;

            var brainEvent = graph.FindEvent(eventName);
            if (brainEvent == null) return;

            TriggerEvent(brainEvent);
        }

        public void TriggerEvent(HonamiLinkedAnimatorEvent brainEvent)
        {
            if (brainEvent == null || brainEvent.rootNodes == null) return;

            var ctx = CreateContext(0f);

            for (int i = 0; i < brainEvent.rootNodes.Count; i++)
            {
                var node = brainEvent.rootNodes[i];
                if (node == null) continue;

                ExecuteNodeChain(node, ctx);
            }
        }

        private void ExecuteNodeChain(HonamiLinkedAnimatorNodeBase node, HonamiLinkedAnimatorContext ctx)
        {
            if (node == null) return;

            node.OnBegin(ctx);
            var result = node.Execute(ctx);

            if (result == LinkedAnimatorNodeResult.Running)
            {
                _activeExecutions.Add(new ActiveEventExecution
                {
                    Node = node,
                    ElapsedTime = 0f
                });
            }
            else
            {
                node.OnEnd(ctx);
                
                // Follow the flow
                if (node is LinkedAnimatorConditionNode cond)
                {
                    ExecuteNodeChain(cond.GetResultBranch(ctx), ctx);
                }
                else if (node.next != null)
                {
                    ExecuteNodeChain(node.next, ctx);
                }
            }
        }

        public void Tick(float deltaTime)
        {
            if (_activeExecutions.Count == 0) return;

            var ctx = CreateContext(deltaTime);

            for (int i = _activeExecutions.Count - 1; i >= 0; i--)
            {
                var exec = _activeExecutions[i];
                exec.ElapsedTime += deltaTime;
                _activeExecutions[i] = exec;

                var result = exec.Node.Execute(ctx);

                if (result == LinkedAnimatorNodeResult.Done)
                {
                    exec.Node.OnEnd(ctx);
                    var nextNode = exec.Node is LinkedAnimatorConditionNode cond ? cond.GetResultBranch(ctx) : exec.Node.next;
                    _activeExecutions.RemoveAt(i);

                    if (nextNode != null)
                        ExecuteNodeChain(nextNode, ctx);
                }
            }
        }

        public void StopAll()
        {
            var ctx = CreateContext(0f);
            foreach (var exec in _activeExecutions)
            {
                if (exec.Node != null)
                {
                    exec.Node.OnEnd(ctx);
                    exec.Node.Reset();
                }
            }
            _activeExecutions.Clear();
        }

        private HonamiLinkedAnimatorContext CreateContext(float deltaTime)
        {
            return new HonamiLinkedAnimatorContext(
                _brain,
                _brain != null ? _brain.LinkedAnimators : null,
                deltaTime,
                0f);
        }

        private struct ActiveEventExecution
        {
            public HonamiLinkedAnimatorNodeBase Node;
            public float ElapsedTime;
        }
    }
}

