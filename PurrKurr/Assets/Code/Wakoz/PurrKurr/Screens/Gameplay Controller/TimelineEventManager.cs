using Code.Wakoz.PurrKurr.DataClasses.Characters;
using Code.Wakoz.PurrKurr.DataClasses.GameCore;
using Code.Wakoz.PurrKurr.DataClasses.ScriptableObjectData;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Code.Wakoz.PurrKurr.DataClasses.Enums.Definitions;

namespace Code.Wakoz.PurrKurr.Screens.Gameplay_Controller {
    public sealed class TimelineEventManager {

        [SerializeField] private List<TimelineEvent> futureEvents;
        [SerializeField] private List<TimelineEvent> activeEvents;
        [SerializeField] private List<TimelineEvent> timelineHistory;

        public TimelineEventManager() {

            futureEvents = new();
            activeEvents = new();
            timelineHistory = new();
        }

        public void RegisterInteractionEvent(Character2DController attacker, IInteractableBody[] targets, float startTime, AttackData
            attackData, AttackAbility interactionAction, Vector2 interactionPosition, AttackBaseStats attackProperties) {

            var newEvent = new TimelineEvent(attacker, targets, startTime, attackProperties.ActionDuration, interactionPosition, attackData, interactionAction, attackProperties);

            // Check for overlapping events and resolve effects
            foreach (var existingEvent in futureEvents) {
                if (IsEventOverlap(existingEvent, newEvent)) {
                    ResolveEventOverlap(existingEvent, newEvent);
                }
            }

            Debug.Log($"{newEvent.InteractionType} event started.  Data: {newEvent.StartTime} -> {newEvent.StartTime+newEvent.DurationInMilliseconds}");
            futureEvents.Add(newEvent);
        }

        public void UpdateEvents(float currentTime, ref List<TimelineEvent> startedEventsRef, ref List<TimelineEvent> finishedEventsRef) {

            var startedEvents = futureEvents.FindAll(e => e.StartTime < currentTime);

            if (startedEvents.Count > 0) {
                Debug.Log(startedEvents.Count + " events started.  1-> " + startedEvents.FirstOrDefault().InteractionType);
                activeEvents.AddRange(startedEvents);
                futureEvents.RemoveAll(startedEvents.Contains);
            }

            startedEventsRef = startedEvents.Count > 0 ? startedEvents : null;
            
            var finishedEvents = activeEvents.FindAll(e => e.StartTime + e.DurationInMilliseconds < currentTime);

            if (finishedEvents.Count > 0) {
                Debug.Log(finishedEvents.Count + " events ended.  1-> " + finishedEvents.FirstOrDefault().InteractionType);
                timelineHistory.AddRange(finishedEvents);
                activeEvents.RemoveAll(finishedEvents.Contains);
            }

            finishedEventsRef = finishedEvents;
        }

        private bool IsEventOverlap(TimelineEvent existingEvent, TimelineEvent newEvent) {
            float existingEventEndTime = existingEvent.StartTime + existingEvent.DurationInMilliseconds;
            float newEventEndTime = newEvent.StartTime + newEvent.DurationInMilliseconds;

            bool isOverlap = existingEvent.StartTime < newEventEndTime && newEvent.StartTime < existingEventEndTime;
            return isOverlap;
        }

        private void ResolveEventOverlap(TimelineEvent existingEvent, TimelineEvent newEvent) {
            // Implement event resolution logic here based on the interaction actions and attack data
            // For example, resolve the effects of overlapping attack and block actions
            Debug.Log($"overlapping events in timeline {existingEvent.StartTime} <> {newEvent.StartTime}");
        }

    }
}