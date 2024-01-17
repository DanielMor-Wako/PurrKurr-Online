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

        public LayerMask GetSolidSurfaces() => WhatIsSolid;
        
        public LayerMask GetSurfaces() =>
            WhatIsSolid | WhatIsClingable | WhatIsTraversable | WhatIsTraversableClingable | WhatIsInteractable;

        public LayerMask GetDamageables() => WhatIsCharacter | WhatIsInteractable;

    }

}