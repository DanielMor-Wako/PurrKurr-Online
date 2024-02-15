using Code.Wakoz.PurrKurr.DataClasses.GameCore.Anchors;
using Code.Wakoz.PurrKurr.Screens.InteractableObjectsPool;
using Code.Wakoz.Utils.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.GameCore.Projectiles {

    [DefaultExecutionOrder(15)]
    public sealed class RopeController : Controller {

        [Tooltip("The link of the rope, essetially the actual interactable rope")]
        [SerializeField] private RopeLinkController _continousLink;
        [Tooltip("The last link that acts as weight, to create more realstic rope behaviour on the edge")]
        [SerializeField] private RopeLinkController _weight;
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
            _weight.SetSimulated(false);

            SetAnchorPosition(startPosition, null);// change from null to a transform to set its parent

            var proceduralPosition = startPosition + normalizedDir * distanceFromAnchor;

            var linksPositionData = HelperFunctions.GenerateVectorsBetween(proceduralPosition, endPosition, linksDistance);

            CreateLinks(ref _anchorRigidBody, ref linksPositionData, ref linksRotation, ref _ropeLinks);

            var weightDistance = normalizedDir * weightDistanceFromLink;
            proceduralPosition = endPosition + weightDistance;

            PrepareLink(_weight, proceduralPosition, (Quaternion.Euler(0, 0, angle + WeightRotationOffset)), _ropeLinks[_ropeLinks.Count - 1].GetRigidBody());

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
                newLink.OnLinkStateChanged += HandleStateChanged;
            }
        }

        public RopeLinkController GetClosestLink(Vector3 referencePoint) {

            RopeLinkController closestLink = null;
            float closestDistance = float.MaxValue;

            foreach (var link in _ropeLinks) {
                float distance = Vector3.Distance(link.transform.position, referencePoint);
                if (distance < closestDistance) {
                    closestLink = link;
                    closestDistance = distance;
                }
            }

            return closestLink;
        }

        public int GetLinksCount() => _ropeLinks.Count;

        public RopeLinkController GetLink(int index) {

            if (_ropeLinks.Contains(_ropeLinks[index])) {
                return _ropeLinks[index];
            }

            return null;
        }

        public RopeLinkController GetLastLink() => GetLink(_ropeLinks.Count-1);

        protected override void Clean() {
            // destroy the weight that is outside of the rope transform?
            // weight.transform.parent = transform;
        }

        protected override Task Initialize() {

            _anchor ??= gameObject.AddComponent<AnchorHandler>();
            _anchorJoint.connectedBody = SingleController.GetController<AnchorsController>().GetAnchorRigidbody(_anchor);

            _pool ??= SingleController.GetController<InteractablesController>();
            _pool.CreateObjectPool(_continousLink, 6, 100, "RopesLinks");

            _weight.transform.parent = transform.parent;

            return Task.CompletedTask;
        }

        private void PrepareLink(RopeLinkController link, Vector2 position, Quaternion rotation, Rigidbody2D bodyToConnect) {

            UpdateLink(link, position, rotation, bodyToConnect);
            link.SetSimulated(false);
        }

        private void UpdateLink(RopeLinkController link, Vector2 position, Quaternion rotation, Rigidbody2D bodyToConnect) {

            link.transform.SetPositionAndRotation(position, rotation);
            link.ConnectJointTo(bodyToConnect);
        }

        private void SetAnchorPosition(Vector2 ropeBuilderPosition, Transform anchorParent) {

            _anchor.ModifyAnchor(ropeBuilderPosition, anchorParent);
            _anchorJoint.connectedBody.simulated = true; // anchorParent != null;
        }

        private void ClearAnyExistingLinks() {

            if (_ropeLinks.Count > 0) {
                foreach (var ropeLink in _ropeLinks) {
                    ropeLink.ConnectJointTo(null);
                    ropeLink.SetSimulated(false);
                    ropeLink.OnLinkStateChanged -= HandleStateChanged;
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
                    ropeLink.SetSimulated(true);
                }
            }

            _weight.SetSimulated(true);
        }

        private void HandleStateChanged(RopeLinkController triggeredRopeLink) {

            if (!_ropeLinks.Contains(triggeredRopeLink)) {
                Debug.LogWarning("Something went wrong, Rope does not have a link of that sort");
                return;
            }

            if (_ropeLinks.Count > 0 && _ropeLinks[0] != null) {
                
                var lastConnectedLink = _ropeLinks[0];
                var linksAreChained = true;
                List<RopeLinkController> unchainedLinks = new();

                for (int i = 0; i < _ropeLinks.Count; i++) {

                    var ropeLink = _ropeLinks[i];
                    // todo: check if the previous link is also deactivated
                    if (linksAreChained && ropeLink.IsChainConnected()) {
                        continue;
                    }

                    if (linksAreChained) {
                        lastConnectedLink = ropeLink;
                        linksAreChained = false;
                    }

                    ropeLink.OnLinkStateChanged -= HandleStateChanged;
                    _ropeLinks.Remove(ropeLink);
                    unchainedLinks.Add(ropeLink);
                }

                if (unchainedLinks.Count > 0) {
                    StartCoroutine(ReleaseObjectsSequentially(unchainedLinks, 3000, 250, () => CheckAllUnchainedLinks())); // 100 milliseconds interval
                }

                var lastLink = lastConnectedLink.transform;
                UpdateLink(_weight, lastLink.position, Quaternion.identity, _ropeLinks[_ropeLinks.Count - 1].GetRigidBody());
            }

        }

        private IEnumerator ReleaseObjectsSequentially(List<RopeLinkController> unchainedLinks, float preDelay, float releaseInterval, Func<Task> actionOnEnd = null) {

            if (preDelay > 0) {
                yield return new WaitForSeconds(preDelay * 0.001f);
            }

            foreach (var ropeLink in unchainedLinks) {

                if (ropeLink == null) {
                    continue;
                }
                _pool.ReleaseInstance(ropeLink);
                yield return new WaitForSeconds(releaseInterval * 0.001f); // Convert milliseconds to seconds
            }

            if (actionOnEnd == null) {
                yield break;
            }

            try {
                actionOnEnd();
            }
            finally { }
        }

        private async Task CheckAllUnchainedLinks() {
            Debug.Log($"Rope has total {_ropeLinks.Count} links");
            if (_ropeLinks.Count > 0) {

                var linksOrderChanged = false;

                foreach (var ropeLink in _ropeLinks) {

                    var connectedToUnchainedLink = ropeLink.IsChainConnected() && !ropeLink.GetChainedBody().gameObject.activeSelf;
                    if (connectedToUnchainedLink) {
                        Debug.Log("link chained to nothing");
                        linksOrderChanged = true;
                        await _pool.ReleaseInstance(ropeLink);
                    }
                }

                if (!linksOrderChanged) {
                    return;
                }

                var lastLink = _ropeLinks[_ropeLinks.Count - 1];
                UpdateLink(_weight, lastLink.GetCenterPosition(), Quaternion.identity, lastLink.GetRigidBody());
            }
        }
    }
}