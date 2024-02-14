using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.GameCore.Projectiles {

    [System.Serializable]
    public class RopeData {

        public GameObject anchorGameObject;
        public Vector2[] linkPositions;

        public RopeData(GameObject prefab, Vector2[] positions) {

            anchorGameObject = prefab;
            linkPositions = positions;
        }
    }
}