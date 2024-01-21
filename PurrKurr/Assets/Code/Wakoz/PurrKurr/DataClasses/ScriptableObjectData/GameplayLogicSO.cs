using Code.Wakoz.PurrKurr.DataClasses.Enums;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.ScriptableObjectData {

    [CreateAssetMenu(fileName = "GameplayLogicData", menuName = "Data/Gameplay")]
    public class GameplayLogicSO : ScriptableObject {
        
        [Header("Environmental Surfaces")]
        [SerializeField] private LayerMask WhatIsInteractable; // add this
        [SerializeField] private LayerMask WhatIsCharacter;
        [SerializeField] private LayerMask WhatIsSolid;
        [SerializeField] private LayerMask WhatIsTraversable;
        [SerializeField] private LayerMask WhatIsClingable;
        [SerializeField] private LayerMask WhatIsTraversableClingable;

        [Header("Character State Definitions")]
        [SerializeField] private List<Definitions.CharacterState> CharacterStatesConsideredAsGrounded;
        [SerializeField] private List<Definitions.CharacterState> CharacterStatesConsideredAsAerial;
        [SerializeField][Min(0)] private float MinMagnitudeConsideredAsRunnin = 20;

        public LayerMask GetSolidSurfaces() =>
            WhatIsSolid;
        
        public LayerMask GetSurfaces() =>
            WhatIsSolid | WhatIsClingable | WhatIsTraversable | WhatIsTraversableClingable | WhatIsInteractable;

        public LayerMask GetDamageables() =>
            WhatIsCharacter | WhatIsInteractable;

        public bool IsStateConsideredAsGrounded(Definitions.CharacterState specificState) {

            return IsStateIncludedInList(specificState, CharacterStatesConsideredAsGrounded);
        }

        public bool IsStateConsideredAsAerial(Definitions.CharacterState specificState) {

            return IsStateIncludedInList(specificState, CharacterStatesConsideredAsAerial);
        }

        public bool IsStateConsideredAsRunning(Definitions.CharacterState specificState, float magnitude) =>
            specificState is Definitions.CharacterState.Running && (magnitude > MinMagnitudeConsideredAsRunnin);

        private bool IsStateIncludedInList(Definitions.CharacterState specificState, List<Definitions.CharacterState> statesList) {
            
            foreach (var state in statesList) {
                if (state == specificState) { 
                    return true;
                }
            }

            return false;
        }

    }

}