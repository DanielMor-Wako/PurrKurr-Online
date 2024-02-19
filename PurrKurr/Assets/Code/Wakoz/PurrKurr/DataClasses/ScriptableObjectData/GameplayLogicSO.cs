using Code.Wakoz.PurrKurr.DataClasses.Effects;
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
        [SerializeField] private LayerMask WhatIsPlatform;
        [SerializeField] private LayerMask WhatIsTraversable;
        [SerializeField] private LayerMask WhatIsClingable;
        [SerializeField] private LayerMask WhatIsTraversableClingable;

        [Header("Effects Definition")]
        [SerializeField] private EffectsData _effects;

        [Header("Character State Definitions")]
        [SerializeField] private List<Definitions.CharacterState> CharacterStatesConsideredAsGrounded;
        [SerializeField] private List<Definitions.CharacterState> CharacterStatesConsideredAsAerial;
        [SerializeField][Min(0)] private float MinMagnitudeConsideredAsRunnin = 20;

        public LayerMask GetSolidSurfaces() =>
            WhatIsSolid;

        public LayerMask GetPlatformSurfaces() =>
            WhatIsPlatform;

        public LayerMask GetSurfaces() =>
            WhatIsSolid | WhatIsClingable | WhatIsTraversable | WhatIsTraversableClingable | WhatIsInteractable | WhatIsPlatform;

        public LayerMask GetDamageables() =>
            WhatIsCharacter | WhatIsInteractable;

        public EffectData GetEffectByType(Definitions.Effect2DType effectType) => _effects.GetDataByType(effectType);

        public bool IsStateConsideredAsGrounded(Definitions.CharacterState specificState) {

            return IsStateIncludedInList(specificState, CharacterStatesConsideredAsGrounded);
        }

        public bool IsStateConsideredAsAerial(Definitions.CharacterState specificState) {

            return IsStateIncludedInList(specificState, CharacterStatesConsideredAsAerial);
        }

        public bool IsStateConsideredAsRunning(Definitions.CharacterState specificState, float magnitude) =>
            specificState is Definitions.CharacterState.Running && IsVelocityConsideredAsRunning(magnitude);

        public bool IsVelocityConsideredAsRunning(float magnitude) => magnitude > MinMagnitudeConsideredAsRunnin;

        // unoptimized search version
        /*private bool IsStateIncludedInList(Definitions.CharacterState specificState, List<Definitions.CharacterState> statesList) {
            
            foreach (var state in statesList) {
                if (state == specificState) { 
                    return true;
                }
            }

            return false;
        }*/

        // optimized search version - testing
        private Dictionary<List<Definitions.CharacterState>, Dictionary<Definitions.CharacterState, bool>> stateCaches = new Dictionary<List<Definitions.CharacterState>, Dictionary<Definitions.CharacterState, bool>>();

        private bool IsStateIncludedInList(Definitions.CharacterState specificState, List<Definitions.CharacterState> statesList) {
            
            if (!stateCaches.ContainsKey(statesList)) {
                stateCaches[statesList] = new Dictionary<Definitions.CharacterState, bool>();
            }

            if (stateCaches[statesList].ContainsKey(specificState)) {
                return stateCaches[statesList][specificState];
            }

            foreach (var state in statesList) {
                if (state == specificState) {
                    stateCaches[statesList][specificState] = true;
                    return true;
                }
            }

            stateCaches[statesList][specificState] = false;
            return false;
        }
    }

}