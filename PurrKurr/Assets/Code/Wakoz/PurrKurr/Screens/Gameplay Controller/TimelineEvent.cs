using Code.Wakoz.PurrKurr.DataClasses.Characters;
using Code.Wakoz.PurrKurr.DataClasses.GameCore;
using Code.Wakoz.PurrKurr.DataClasses.ScriptableObjectData;
using UnityEngine;
using static Code.Wakoz.PurrKurr.DataClasses.Enums.Definitions;

namespace Code.Wakoz.PurrKurr.Screens.Gameplay_Controller {
    public class TimelineEvent {

        public Character2DController Attacker { get; private set; }
        public IInteractableBody[] Targets { get; private set; }
        public float StartTime { get; private set; }
        public float DurationInMilliseconds { get; private set; }
        public Vector2 InteractionPosition { get; private set; }
        public AttackData AttackData { get; private set; }
        public AttackAbility InteractionType { get; private set; }
        public AttackBaseStats AttackProperties { get; private set; }

        public TimelineEvent(Character2DController attacker, IInteractableBody[] targets, float startTime, float duration, Vector2 interactionPosition, AttackData attackData, AttackAbility interactionType, AttackBaseStats attackProperties) {
            
            Attacker = attacker;
            Targets = targets;
            StartTime = startTime;
            DurationInMilliseconds = duration;
            InteractionPosition = interactionPosition;
            AttackData = attackData;
            InteractionType = interactionType;
            AttackProperties = attackProperties;
        }
    }
}