using Code.Wakoz.PurrKurr.DataClasses.GameCore.Anchors;
using Code.Wakoz.PurrKurr.Screens.InteractableObjectsPool;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace Code.Wakoz.PurrKurr.DataClasses.GameCore.Projectiles {

    [DefaultExecutionOrder(15)]
    public class RopeController : Controller {

        [Tooltip("The first link with adjustments to the offset to the anchor")]
        [SerializeField] private RopeLinkController firstLink;
        [Tooltip("The link of the rope, essetially the actual interactable rope")]
        [SerializeField] private RopeLinkController continousLink;
        [Tooltip("The last link that acts as weight, to create more realstic rope behaviour on the edge")]
        [SerializeField] private RopeLinkController weight;

        [SerializeField] private HingeJoint2D _anchorJoint;
        private AnchorHandler _anchor;

        private List<RopeLinkController> _ropeLinks = new List<RopeLinkController>();
        private InteractablesController _pool;
        private Transform _anchorPoint;

        public void Initialize(RopeData ropeData) {

            _pool ??= SingleController.GetController<InteractablesController>();
            _pool.CreateObjectPool(continousLink, ropeData.linkPositions.Length, 100, "RopesLinks");

            Vector3 startPosition = ropeData.linkPositions[0];
            Vector3 endPosition = ropeData.linkPositions[ropeData.linkPositions.Length - 1];
            float angle = Vector2.SignedAngle(startPosition, endPosition);
            var rotation = Quaternion.Euler(0, 0, angle+90); // 90 is the original z offset of the object

            Transform anchorParent = null; // change from null to a transform to set its parent
            _anchor.ModifyAnchor(startPosition, anchorParent);
            _anchorJoint.connectedBody.simulated = true; // anchorParent != null;

            //transform.SetPositionAndRotation(startPosition, Quaternion.Euler(0, 0, 180));
            firstLink.transform.SetPositionAndRotation(startPosition, rotation);

            if (_ropeLinks.Count > 0) {
                foreach (var ropeLink in _ropeLinks) {
                    _pool.ReleaseInstance(ropeLink);
                }
                _ropeLinks.Clear();
            }

            //Vector3 distance = endPosition - startPosition;
            //float linkLength = distance.magnitude / (ropeData.linkPositions.Length - 1);

            Transform previousLink = firstLink.transform;

            for (int i = 1; i < ropeData.linkPositions.Length - 2; i++) {
                Vector2 position = ropeData.linkPositions[i];

                //Transform newLink = Instantiate(linkPrefab, position, Quaternion.identity, transform).transform;
                var newLink = _pool.GetInstance<RopeLinkController>();
                if (newLink == null) {
                    Debug.LogWarning("No available rope instance in pool, consider increasing the max items capacity");
                    return;
                }

                newLink.transform.SetPositionAndRotation(position, rotation);

                newLink.ConnectJointTo(previousLink.GetComponent<Rigidbody2D>());

                previousLink = newLink.transform;
                _ropeLinks.Add(newLink);
            }

            weight.transform.SetPositionAndRotation(endPosition, rotation);
            weight.ConnectJointTo(previousLink.GetComponent<Rigidbody2D>());
            // Additional graphic implementation
        }

        public Transform GetClosestLink(Vector3 referencePoint) {

            Transform closestLink = null;
            float closestDistance = float.MaxValue;

            foreach (var link in _ropeLinks) {
                float distance = Vector3.Distance(link.transform.position, referencePoint);
                if (distance < closestDistance) {
                    closestLink = link.transform;
                    closestDistance = distance;
                }
            }

            return closestLink;
        }

        protected override void Clean() {
        }

        protected override Task Initialize() {

            _anchor ??= gameObject.AddComponent<AnchorHandler>();
            _anchorJoint.connectedBody = SingleController.GetController<AnchorsController>().GetAnchorRigidbody(_anchor);

            return Task.CompletedTask;
        }

    }
}