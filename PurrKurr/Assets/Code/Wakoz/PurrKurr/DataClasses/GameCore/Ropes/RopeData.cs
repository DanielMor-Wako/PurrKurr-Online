using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.GameCore.Ropes {

    [System.Serializable]
    public class RopeData {

        public GameObject AnchorGameObject;
        public Vector2[] LinkPositions;
        public LayerMask WhatIsSolid;

        public RopeData(GameObject prefab, Vector2[] positions, LayerMask whatIsSolid) {

            AnchorGameObject = prefab;
            LinkPositions = positions;
            WhatIsSolid = whatIsSolid;
        }
    }
}