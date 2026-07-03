using HonamiAnimationSystem.Runtime.Events;

namespace HonamiAnimationSystem.Runtime.Core
{
    public static class HonamiEventEvaluator
    {
        public static void FireEvent(HonamiEventMarker evt, HonamiLocalEventReceiver localEventReceiver, HonamiGlobalEvent[] globalEvents)
        {
            if (evt.eventType == HonamiEventType.Local && localEventReceiver != null)
            {
                localEventReceiver.TriggerEvent(evt.eventName);
            }
            else if (evt.eventType == HonamiEventType.Global && globalEvents != null)
            {
                foreach (var ge in globalEvents)
                {
                    if (ge.eventId == evt.globalEventId) ge.ExecuteEvent();
                }
            }
        }
    }
}
