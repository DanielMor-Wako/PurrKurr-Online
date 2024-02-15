using Code.Wakoz.PurrKurr.DataClasses.GameCore.Anchors;
using Code.Wakoz.PurrKurr.Screens.InteractableObjectsPool;
using Code.Wakoz.Utils.Extensions;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.GameCore.Projectiles {

    [DefaultExecutionOrder(15)]
    public class RopeController : Controller {

        [Tooltip("The link of the rope, essetially the actual interactable rope")]
        [SerializeField] private RopeLinkController continousLink;
        [Tooltip("The last link that acts as weight, to create more realstic rope behaviour on the edge")]
        [SerializeField] private RopeLinkController weight;
        [Tooltip("The rigid body of the anchor")]
        [SerializeField] private Rigidbody2D _anchorRigidBody;
        [Tooltip("The joint of the anchor")]
        [SerializeField] private HingeJoint2D _anchorJoint;

        [SerializeField][Min(0)] private float delayBeforeRopeActivation = 0f;
        [Header("Offsets")]
        [SerializeField] private float AnchorRotationOffset = -90f;
        [SerializeField] private float LinkRotationOffset = -90f;
        [SerializeField] private float WeightRotationOffset = 90f;
        [Header("Distance between links")]
        [SerializeField] private float distanceFromAnchor = 0f;
        [SerializeField] private float linksDistance = 0.7f;
        [SerializeField] private float weightDistanceFromLink = 0.5f;

        private AnchorHandler _anchor;
        private List<RopeLinkController> _ropeLinks = new List<RopeLinkController>();
        private InteractablesController _pool;

        public void Initialize(RopeData ropeData) {

            Vector2 startPosition = ropeData.linkPositions[0];
            Vector2 endPosition = ropeData.linkPositions[ropeData.linkPositions.Length - 1];
            var normalizedDir = (endPosition - startPosition).normalized;
            var angle = Mathf.Atan2(normalizedDir.y, normalizedDir.x) * Mathf.Rad2Deg;
            var linksRotation = Quaternion.Euler(0, 0, angle + LinkRotationOffset);

            ClearAnyExistingLinks();
            weight.SetActiveState(false);

            SetAnchorPosition(startPosition, null);// change from null to a transform to set its parent

            var proceduralPosition = startPosition + normalizedDir * distanceFromAnchor;
            
            var linksPositionData = HelperFunctions.GenerateVectorsBetween(proceduralPosition, endPosition, linksDistance);

            CreateLinks(ref _anchorRigidBody, ref linksPositionData, ref linksRotation, ref _ropeLinks);

            var weightDistance = normalizedDir * weightDistanceFromLink;
            proceduralPosition = endPosition + weightDistance;

            PrepareLink(weight, proceduralPosition, (Quaternion.Euler(0, 0, angle + WeightRotationOffset)), _ropeLinks[_ropeLinks.Count - 1].GetRigidBody());

            StartCoroutine(ActivateLinksWhenAnchorPositionHasUpdated());
        }

        private void CreateLinks(ref Rigidbody2D anchorRigidBody, ref List<Vector2> linksPositionData, ref Quaternion linksRotation, ref List<RopeLinkController> ropeLinks) {

            var previousRigidBody = anchorRigidBody;

            for (int i = 1; i < linksPositionData.Count; i++) {

                var newLink = _pool.GetInstance<RopeLinkController>();
                if (newLink == null) {
                    Debug.LogWarning("No available rope instance in pool, consider increasing the max items capacity");
                    return;
                }

                PrepareLink(newLink, linksPositionData[i], linksRotation, previousRigidBody);

                previousRigidBody = newLink.GetRigidBody();
                ropeLinks.Add(newLink);
            }
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
            // destroy the weight that is outside of the rope transform?
            // weight.transform.parent = transform;
        }

        protected override Task Initialize() {

            _anchor ??= gameObject.AddComponent<AnchorHandler>();
            _anchorJoint.connectedBody = SingleController.GetController<AnchorsController>().GetAnchorRigidbody(_anchor);

            _pool ??= SingleController.GetController<InteractablesController>();
            _pool.CreateObjectPool(continousLink, 6, 100, "RopesLinks");

            weight.transform.parent = transform.parent;

            return Task.CompletedTask;
        }

        private void PrepareLink(RopeLinkController link, Vector2 position, Quaternion rotation, Rigidbody2D bodyToConnect) {
            
            link.transform.SetPositionAndRotation(position, rotation);
            link.ConnectJointTo(bodyToConnect);
            link.SetActiveState(false);
        }

        private void SetAnchorPosition(Vector2 ropeBuilderPosition, Transform anchorParent) {

            _anchor.ModifyAnchor(ropeBuilderPosition, anchorParent);
            _anchorJoint.connectedBody.simulated = true; // anchorParent != null;
        }

        private void ClearAnyExistingLinks() {

            if (_ropeLinks.Count > 0) {
                foreach (var ropeLink in _ropeLinks) {
                    ropeLink.ConnectJointTo(null);
                    ropeLink.SetActiveState(false);
                    _pool.ReleaseInstance(ropeLink);
                }
                _ropeLinks.Clear();
            }
        }

        private IEnumerator ActivateLinksWhenAnchorPositionHasUpdated() {

            yield return new WaitForFixedUpdate();
            yield return new WaitForSeconds(delayBeforeRopeActivation);

            if (_ropeLinks.Count <= 0) {
                yield break;
            }

            foreach (var ropeLink in _ropeLinks) {
                if (ropeLink != null) {
                    ropeLink.SetActiveState(true);
                }
            }

            weight.SetActiveState(true);
        }
    }
}