using Code.Wakoz.PurrKurr.DataClasses.Enums;
using Code.Wakoz.PurrKurr.DataClasses.GameCore.Anchors;
using Code.Wakoz.PurrKurr.Screens.InteractableObjectsPool;
using Code.Wakoz.PurrKurr.Screens.Ui_Controller;
using Code.Wakoz.Utils.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using static Code.Wakoz.PurrKurr.DataClasses.Enums.Definitions;

namespace Code.Wakoz.PurrKurr.DataClasses.GameCore.Ropes {

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
        [Tooltip("The distance joint from the anchor the weight position")]
        [SerializeField] private DistanceJoint2D _distanceJoint;

        [SerializeField][Min(0)] private float delayBeforeRopeActivation = 0f;
        [SerializeField] private float ForceMulti = 3000f;
        [Header("Offsets")]
        [SerializeField] private float LinkRotationOffset = -90f;
        [SerializeField] private float WeightRotationOffset = 90f;
        [Header("Distance between links")]
        [SerializeField] private float distanceFromAnchor = 0f;
        [SerializeField] private float linksDistance = 0.7f;
        [SerializeField] private float weightDistanceFromLink = 0.5f;

        private RopeData _ropeData;
        private AnchorHandler _anchor;
        private InteractablesController _pool;
        private List<RopeLinkController> _ropeLinks = new List<RopeLinkController>();
        List<RopeLinkController> _unchainedLinks = new();
        private Coroutine _activationInAction;
        private Coroutine _updatingState;
        private RopeLinkController _bodyToApplyForce = null;
        private Vector2 _forceToApply = Vector2.zero;

        public async Task Initialize(RopeData ropeData, IInteractableBody ropeInitiator = null) {

            _ropeData = ropeData;

            Vector2 startPosition = ropeData.LinkPositions[0];
            Vector2 endPosition = ropeData.LinkPositions[ropeData.LinkPositions.Length - 1];
            var normalizedDir = (endPosition - startPosition).normalized;
            var angle = Mathf.Atan2(normalizedDir.y, normalizedDir.x) * Mathf.Rad2Deg;
            var linksRotation = Quaternion.Euler(0, 0, angle + LinkRotationOffset);

            await ClearAnyExistingLinks();

            _weight.SetSimulated(false);

            SetAnchorPosition(startPosition, null);// change from null to a transform to set its parent

            var proceduralPosition = startPosition + normalizedDir * distanceFromAnchor;

            var linksPositionData = HelperFunctions.GenerateVectorsBetween(proceduralPosition, endPosition, linksDistance);

            CreateLinks(ref _anchorRigidBody, ref linksPositionData, ref linksRotation, ref _ropeLinks);

            var weightDistance = normalizedDir * weightDistanceFromLink;
            proceduralPosition = endPosition + weightDistance;

            PrepareLink(_weight, proceduralPosition, (Quaternion.Euler(0, 0, angle + WeightRotationOffset)), _ropeLinks[_ropeLinks.Count - 1].GetRigidBody());

            if (ropeInitiator != null) {
                Debug.Log($"Rope original initialor {ropeInitiator}");
            }

            if (_activationInAction != null) {
                StopCoroutine(_activationInAction);
                _activationInAction = null;
            }
            _activationInAction = StartCoroutine(ActivateLinksWhenAnchorPositionHasUpdated());

            if (_updatingState != null) {
                StopCoroutine(_updatingState);
                _updatingState = null;
            }
        }

        private void DisconnectInteractableBody(RopeLinkController link, IInteractableBody bodyToDisconnectFromLink) {

            DisconnectGrabbers(link, bodyToDisconnectFromLink);

            // todo: check for individual body to disconnect -> bodyToDisconnectFromLink
            //bodyToDisconnectFromLink.SetAsGrabbing(null);
            //bodyToDisconnectFromLink.SetAsGrabbed(null, bodyToDisconnectFromLink.GetCenterPosition());
            //link.SetAsGrabbed(bodyToDisconnectFromLink, link.GetCenterPosition());
        }

        public void HandleMoveToNextLink(RopeLinkController ropeLink, IInteractableBody bodyToConnectToLink, bool isNavUp) {

            var nextLink = TryGetNextAvailableLink(ropeLink, isNavUp);
            if (nextLink == null || nextLink == ropeLink) {
                return;
            }

            DisconnectInteractableBody(ropeLink, bodyToConnectToLink);
            ConnectInteractableBody(nextLink, bodyToConnectToLink);

            UpdateWeightPosition();
            UpdateStrechMode();
        }

        public void HandleDisconnectInteractableBody(RopeLinkController link, IInteractableBody bodyToConnectToLink) {

            DisconnectInteractableBody(link, bodyToConnectToLink);

            UpdateWeightPosition();
            UpdateStrechMode();
        }

        public void HandleConnectInteractableBody(RopeLinkController link, IInteractableBody bodyToConnectToLink) {

            ConnectInteractableBody(link, bodyToConnectToLink);

            UpdateWeightPosition();
            UpdateStrechMode();
        }

        public void HandleSwingMommentum(RopeLinkController ropeLink, bool isNavRight) {

            UpdateStrechMode();

            TrySwingMommentum(ropeLink, isNavRight);
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

        public RopeLinkController GetLink(int index) {

            // todo?: check if index is -1, then return anchor item like index 0

            if (index < _ropeLinks.Count && _ropeLinks[index] != null &&_ropeLinks.Contains(_ropeLinks[index])) {
                return _ropeLinks[index];
            }

            return null;
        }

        public bool HasNoChainedLinks() => _ropeLinks.Count == _unchainedLinks.Count;

        public List<RopeLinkController> GetChainedLinks() => _ropeLinks.Except(_unchainedLinks).ToList();

        public RopeLinkController GetFirstChainedLink() => GetLink(0);

        public RopeLinkController GetLastChainedLink() => GetLink(_ropeLinks.Count - _unchainedLinks.Count - 1);

        public RopeLinkController GetNextChainedLink(RopeLinkController currentItem, int indexOffset) {

            if (currentItem == null || indexOffset == 0) {
                return currentItem;
            }

            var currentIndex = GetChainedLinks().FindIndex(item => item == currentItem);
            if (currentIndex == -1) {
                return null;
            }

            var newIndex = currentIndex + indexOffset;
            var newIndexWithinRange = newIndex >= 0 && newIndex < _ropeLinks.Count;

            return newIndexWithinRange ? _ropeLinks[newIndex] : null;
        }

        public void ApplyForce(Vector2 forceDir) {
            _forceToApply = forceDir;
        }

        public bool IsRopeStreched() => _distanceJoint.maxDistanceOnly == false;

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

        private void CreateLinks(ref Rigidbody2D anchorRigidBody, ref List<Vector2> linksPositionData, ref Quaternion linksRotation, ref List<RopeLinkController> ropeLinks) {

            var previousRigidBody = anchorRigidBody;

            for (int i = 1; i < linksPositionData.Count; i++) {

                var newLink = _pool.GetInstance<RopeLinkController>();
                if (newLink == null) {
                    Debug.LogWarning("No available rope instance in pool, consider increasing the max items capacity");
                    return;
                }

                newLink.gameObject.name = $"L {i}";

                PrepareLink(newLink, linksPositionData[i], linksRotation, previousRigidBody);

                previousRigidBody = newLink.GetRigidBody();
                ropeLinks.Add(newLink);
                newLink.OnLinkStateChanged += HandleStateChanged;
                newLink.OnLinkInteracted += HandleLinkInteraction;
            }
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

        private async Task ClearAnyExistingLinks() {

            if (_ropeLinks.Count > 0) {

                foreach (var ropeLink in _ropeLinks) {

                    //await SafeRemoveInstance(ropeLink);
                    DisconnectGrabbers(ropeLink);
                    ropeLink.ConnectJointTo(null);
                    ropeLink.SetSimulated(false);
                    ropeLink.OnLinkStateChanged -= HandleStateChanged;
                    ropeLink.OnLinkInteracted -= HandleLinkInteraction;
                    await _pool.ReleaseInstance(ropeLink);
                }
                _ropeLinks.Clear();
            }
            _unchainedLinks.Clear();
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

            UpdateStrechMode();
        }

        private void StrechRope(bool strechted) {
            _distanceJoint.maxDistanceOnly = !strechted;
        }

        private void HandleStateChanged(RopeLinkController triggeredRopeLink) {

            if (_updatingState != null) {
                if (_unchainedLinks.Contains(triggeredRopeLink)) {
                    return;
                } else {
                    StopCoroutine(_updatingState);
                    _updatingState = null;
                }
            }

            if (!_ropeLinks.Contains(triggeredRopeLink) && !_unchainedLinks.Contains(triggeredRopeLink)) {
                Debug.LogWarning("Something went wrong, Rope does not have a link "+triggeredRopeLink.name);
                return;
            }

            RopeLinkController lastConnectedLink = null;

            if (_ropeLinks.Count > 0 && _ropeLinks[0] != null) {

                var linksAreChained = true;

                for (int i = 0; i < _ropeLinks.Count; i++) {

                    var ropeLink = _ropeLinks[i];
                    // todo: check if the previous link is also deactivated
                    if (linksAreChained && ropeLink.IsChainConnected()) {
                        lastConnectedLink = ropeLink;
                        continue;
                    }

                    if (linksAreChained) {
                        linksAreChained = false;
                    }

                    if (!_unchainedLinks.Contains(ropeLink)) {
                        _unchainedLinks.Add(ropeLink);
                        DisconnectGrabbers(ropeLink);
                    }
                }

            }

            if (_unchainedLinks.Count > 0) {

                var weightPosition = lastConnectedLink != null ? (Vector2)lastConnectedLink.GetCenterPosition() : _anchorRigidBody.position;
                var weightConnectedBody = lastConnectedLink != null ? lastConnectedLink.GetRigidBody() : _anchorRigidBody;

                UpdateWeightPosition();
                UpdateStrechMode();

                if (lastConnectedLink == null) { Debug.Log("No Links, Last connected link is anchor");
                } else { Debug.Log("Last connected link is "+lastConnectedLink.name); }

                _updatingState = StartCoroutine(ReleaseObjectsSequentially(_unchainedLinks, 3000, 200, (instance) => SafeRemoveInstance(instance), () => { _updatingState = null; return SafeRemoveAllUnchainedLinks(); } )); // 100 milliseconds interval
            }
        }

        // make this into a generic for the objectpool wrapper class
        private IEnumerator ReleaseObjectsSequentially(List<RopeLinkController> unchainedLinks, float preDelay, float releaseInterval, 
                Func<RopeLinkController, Task> actionBeforeRelease = null, Func<Task> actionOnEndProcess = null) {

            var hasActionOnRelease = actionBeforeRelease != null;

            if (preDelay > 0) {
                yield return new WaitForSeconds(preDelay * 0.001f);
            }

            //var allInstances = _pool.GetAllInstances<RopeLinkController>();

            foreach (var ropeLink in unchainedLinks) {

                if (ropeLink == null) {
                    continue;
                }

                try {
                    if (hasActionOnRelease) {
                        yield return actionBeforeRelease(ropeLink);
                    }
                }
                finally {
                    //if (allInstances.Contains(ropeLink))
                        //_pool.ReleaseInstance(ropeLink);
                }

                yield return new WaitForSeconds(releaseInterval * 0.001f); // Convert milliseconds to seconds
            }

            if (actionOnEndProcess == null) {
                yield break;
            }

            try {
                actionOnEndProcess();
            }
            finally { }
        }

        private async Task SafeRemoveInstance(RopeLinkController ropeLink = null) {

            if (ropeLink == null) {
                return;
            }

            // Release instance
            //ropeLink.OnLinkStateChanged -= HandleStateChanged;
            //ropeLink.OnLinkInteracted -= HandleLinkInteraction;
            //if (_ropeLinks.Contains(ropeLink)) { _ropeLinks.Remove(ropeLink); }
            //if (_unchainedLinks.Contains(ropeLink)) { _unchainedLinks.Remove(ropeLink); }
        }

        private void DisconnectGrabbers(RopeLinkController ropeLink, IInteractableBody bodyToDisconnectFromLink = null) {

            if (ropeLink == null) {
                return;
            }

            var connectedBody = bodyToDisconnectFromLink != null ? bodyToDisconnectFromLink : ropeLink.GetGrabbedTarget();
            if (connectedBody == null) {
                return;
            }

            connectedBody.SetAsGrabbing(null);
            connectedBody.SetAsGrabbed(null, ropeLink.GetCenterPosition());
            ropeLink.SetAsGrabbed(null, ropeLink.GetCenterPosition());
            //Debug.Log($"Disconnect {connectedBody.GetTransform().name} from {ropeLink.name}");
        }

        private async Task SafeRemoveAllUnchainedLinks() {
            Debug.Log($"Rope has {_unchainedLinks.Count}/{_ropeLinks.Count} chained links");
            if (_ropeLinks.Count > 0 || _unchainedLinks.Count > 0) {

                foreach (var unchainedLink in _unchainedLinks) {

                    // todo: maybe release the instance safe and check connected
                    await SafeRemoveInstance(unchainedLink);
                    //await _pool.ReleaseInstance(ropeLink);
                }

            }

        }

        private void HandleLinkInteraction(RopeLinkController ropeLink, ActionInput actionInput, Definitions.NavigationType navDir) {

            // move character input interpretter?
            if (actionInput.ActionGroupType == Definitions.ActionTypeGroup.Navigation) {

                //if (TryPerformInteractionAvailable(ropeLink, navDir)) {
                    //PerformInteraction(ropeLink, navDir);
                //}
            }
        }

        // Gets Next Link if not Occupied By Another InteractableBody
        public RopeLinkController TryGetNextAvailableLink(RopeLinkController ropeLink, bool isNavUp) {

            if (ropeLink == null || _unchainedLinks.Contains(ropeLink) || HasNoChainedLinks()) {
                return null;
            }

            if (!isNavUp && ropeLink != GetLastChainedLink() || isNavUp && ropeLink != GetFirstChainedLink()) {
                var nextChainedLink = GetNextChainedLink(ropeLink, isNavUp ? -1 : 1);
                return !nextChainedLink.IsGrabbed() ? nextChainedLink : null;
            }

            return null;
        }

        private void ConnectInteractableBody(RopeLinkController link, IInteractableBody bodyToConnectToLink) {

            bodyToConnectToLink.SetAsGrabbing(link);
            bodyToConnectToLink.SetAsGrabbed(link, link.GetCenterPosition());
            link.SetAsGrabbed(bodyToConnectToLink, link.GetCenterPosition());
        }

        private void UpdateWeightPosition() {

            if (HasNoChainedLinks()) {
                if (_weight.GetChainedBody() != _anchorRigidBody) {
                    UpdateLink(_weight, _anchorRigidBody.transform.position, Quaternion.identity, _anchorRigidBody);
                }
                return;
            }

            var chainedLinks = GetChainedLinks();
            foreach (var chainedLink in chainedLinks) {
                if (chainedLink.IsGrabbed()) {
                    if (_weight.GetChainedBody() != chainedLink.GetRigidBody()) {
                        UpdateLink(_weight, chainedLink.GetCenterPosition(), Quaternion.identity, chainedLink.GetRigidBody());
                    }
                    return;
                }
            }

            var lastChainedLink = chainedLinks[chainedLinks.Count - 1];
            if (_weight.GetChainedBody() != lastChainedLink.GetRigidBody()) {
                UpdateLink(_weight, lastChainedLink.GetCenterPosition(), Quaternion.identity, lastChainedLink.GetRigidBody());
            }
        }

        private void UpdateStrechMode() {

            if (HasNoChainedLinks()) {
                StrechRope(false);
                return;
            }

            var ropeLinkConnectedToWeight = _weight.GetChainedBody().GetComponent<IInteractableBody>();
            var hasInteractableConnected = ropeLinkConnectedToWeight?.GetGrabbedTarget() != null;
            var linksNotTouchingSolids = IsRopePathCleanOfSolids();

            StrechRope(hasInteractableConnected && linksNotTouchingSolids);
        }

        private bool IsRopePathCleanOfSolids() {

            if (HasNoChainedLinks()) {
                return false;
            }

            var ropeLinkConnectedToWeight = _weight.GetChainedBody().GetComponent<IInteractableBody>();
            var firstLink = (Vector2)GetFirstChainedLink().GetCenterPosition();
            var ropeAngle = (Vector2)ropeLinkConnectedToWeight.GetCenterPosition() - firstLink;

            RaycastHit2D hit = Physics2D.Raycast(firstLink, ropeAngle.normalized, ropeAngle.magnitude, _ropeData.WhatIsSolid);
            Debug.DrawLine(firstLink, (Vector2)ropeLinkConnectedToWeight.GetCenterPosition(), Color.green, 0.2f);

            var ropeHitPointTouchingSolid = transform.position;
            if (hit.collider != null) {
                Debug.DrawRay(hit.point, ropeAngle.normalized * 2f, Color.red, 0.5f);
                ropeHitPointTouchingSolid = hit.point;
            }

            var hasCleanPath = ropeHitPointTouchingSolid == transform.position;
            return hasCleanPath;
        }

        private bool TrySwingMommentum(RopeLinkController ropeLink, bool isNavRight) {

            if (ropeLink == null || _unchainedLinks.Contains(ropeLink)) {
                return false;
            }

            var angle = isNavRight ? 90 : -90;

            var pericularDirection = HelperFunctions.CalculateRotatedVector2(transform.position, ropeLink.GetCenterPosition(), angle);
            _forceToApply = pericularDirection * Vector2.one * ForceMulti * Time.deltaTime;
            _bodyToApplyForce = ropeLink;

            return true;
        }

        private void FixedUpdate() {

            if (_forceToApply != Vector2.zero && _bodyToApplyForce != null && _ropeLinks.Count > 0) {

                _bodyToApplyForce.ApplySwingForce(_forceToApply);
                
                _forceToApply = Vector2.zero;
                _bodyToApplyForce = null;
            }
        }
    }
}