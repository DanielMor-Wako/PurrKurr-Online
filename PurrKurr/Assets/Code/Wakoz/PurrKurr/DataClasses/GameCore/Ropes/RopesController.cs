using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.GameCore.Ropes {

    [DefaultExecutionOrder(12)]
    public class RopesController : SingleController {

        private Dictionary<string, RopeController> activeRopes = new Dictionary<string, RopeController>();

        public RopeController CreateRope(RopeData ropeData, string ropeName) {

            var ropeController = GetRopeInstance(ropeData);
            ropeController.Initialize(ropeData);
            activeRopes.Add(ropeName, ropeController);

            return ropeController;
        }

        private RopeController GetRopeInstance(RopeData ropeData) {

            GameObject ropeGO = Object.Instantiate(ropeData.anchorGameObject, transform);
            RopeController ropeController = ropeGO.AddComponent<RopeController>();
            return ropeController;
        }

        public RopeController GetRopeController(string ropeName) {

            if (activeRopes.ContainsKey(ropeName)) {
                return activeRopes[ropeName];
            } else {
                Debug.LogWarning("RopeController with name " + ropeName + " does not exist.");
                return null;
            }
        }

        protected override void Clean() {
        }

        protected override Task Initialize() {

            return Task.CompletedTask;
        }

    }
}