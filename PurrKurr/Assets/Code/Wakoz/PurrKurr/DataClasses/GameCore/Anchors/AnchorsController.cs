using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.GameCore.Anchors {

    [DefaultExecutionOrder(12)]
    public class AnchorsController : SingleController {

        [SerializeField] private DynamicAnchor AnchorPrefab;

        private Dictionary<AnchorHandler, DynamicAnchor> anchors = new Dictionary<AnchorHandler, DynamicAnchor>();

        protected override void Clean() {
            AnchorHandler.OnAnchorAdded -= AddAnchor;
            AnchorHandler.OnAnchorRemoved -= RemoveAnchor;
        }

        protected override Task Initialize() {
            AnchorHandler.OnAnchorAdded += AddAnchor;
            AnchorHandler.OnAnchorRemoved += RemoveAnchor;

            return Task.CompletedTask;
        }

        public Rigidbody2D GetAnchorRigidbody(AnchorHandler opponentAnchor)
        {
            if (anchors.ContainsKey(opponentAnchor)) {
                return anchors[opponentAnchor].GetComponent<Rigidbody2D>();
            }
                
            return null;
        }
    
        private void AddAnchor(AnchorHandler opponentAnchor) {
            //Debug.Log("Adding anchor for "+ opponentAnchor.name);
            if (!anchors.ContainsKey(opponentAnchor)) {
                var anchor = Instantiate(AnchorPrefab, transform);
                anchor.name = "dynAnchor_" + opponentAnchor.name;
                anchors.Add(opponentAnchor, anchor);
                //anchor.Bind(opponentAnchor);
                opponentAnchor.OnAnchorChanged += HandleAnchorChange;
            }
        }

        private void RemoveAnchor(AnchorHandler opponentAnchor) {

            if (anchors.ContainsKey(opponentAnchor)) {
                if (anchors[opponentAnchor] != null && anchors[opponentAnchor].gameObject != null) {
                    opponentAnchor.OnAnchorChanged -= HandleAnchorChange;
                    //anchors[opponentAnchor].Unbind();
                    Destroy(anchors[opponentAnchor].gameObject);
                }
                anchors.Remove(opponentAnchor);
            }
        }

        private void HandleAnchorChange(AnchorHandler anchor, Vector3 position, Transform anchorParent) {

            anchors[anchor].transform.position = position;

            if (anchorParent != null) {

                AssignAnchor(anchor, anchorParent);

            } else {

                DissociateAnchor(anchor);
            }
            
        }

        private void AssignAnchor(AnchorHandler anchor, Transform anchorParent) {

            anchors[anchor].transform.parent = anchorParent;
        }
        
        private void DissociateAnchor(AnchorHandler anchor) {

            anchors[anchor].transform.parent = this.transform;
        }

    }
}
