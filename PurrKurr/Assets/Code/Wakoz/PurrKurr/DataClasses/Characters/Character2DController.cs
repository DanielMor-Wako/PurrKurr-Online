using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Code.Wakoz.Utils.Extensions;
using Code.Wakoz.Utils.GraphicUtils.TransformUtils;
using Code.Wakoz.PurrKurr.Screens.Init;
using Code.Wakoz.PurrKurr.DataClasses.Enums;
using Code.Wakoz.PurrKurr.DataClasses.GameCore;
using Code.Wakoz.PurrKurr.DataClasses.Effects;
using Code.Wakoz.PurrKurr.DataClasses.GameCore.Anchors;
using Code.Wakoz.PurrKurr.Logic.GameFlow;

namespace Code.Wakoz.PurrKurr.DataClasses.Characters {

    [DefaultExecutionOrder(15)]
    public class Character2DController : Controller, IInteractableBody {

        public event Action<List<DisplayedableStatData>> OnUpdatedStats;
        public event Action<Character2DState> OnStateChanged;

        [SerializeField] private CharacterStats _stats;
        [SerializeField] private Character2DEffects _effects;
        [SerializeField] private Rigidbody2D _rigidbody;
        [SerializeField] private CircleCollider2D _legsCollider;
        [SerializeField] private WheelJoint2D _legs;
        [SerializeField] private HingeJoint2D _cling;
        [SerializeField] private JointMotor2D _motor;
        [SerializeField] private TransformMover _transformMover;
        [SerializeField] private CharacterSenses _senses;
        [SerializeField] private Character2DRig _rigAnimator;
        [SerializeField] private Transform _bodyDamager;
        private SpriteRenderer _sprite;

        private Rigidbody2D _legsRigidBody;

        private Vector3 NewPositionToSetOnFixedUpdte;
        private Vector2 ForceDirToSetOnFixedUpdate;

        private LayerMask _whatIsSolid;
        private LayerMask _whatIsSurface;
        private LayerMask _whatIsDamageableCharacter;
        private LayerMask _whatIsDamageable;

        const float GroundedRadius = .7f;
        const float CharacterMaxMotorTorque = 10000;
        const float AdditionalOuterRadius = 0.3f;

        private Character2DState _characterState;
        private AnchorHandler _anchor;

        private LogicController _logic;
        private DebugController _debug;
        
        private Collider2D[] _solidObjectsColliders;
        private Collider2D[] _solidOutRadiusObjectsColliders;

        public Collider2D[] NearbyInteractables() {
 
            var _nearbyInteractions = _senses.NearbyInteractables();
            
            if (_nearbyInteractions == null || _legsCollider == null) {
                return null;
            }

            var potentialInteractions =
                _nearbyInteractions.Where(potentialFoe => potentialFoe != _legsCollider).
                OrderBy(obj => Vector2.Distance(obj.transform.position, LegsPosition)).ToArray();

            List<Collider2D> validInteractables = new();
            int validInteractablesInfrontOfCharacter = 0;

            foreach (var interaction in potentialInteractions) {
            
                var objPosition = interaction.transform.position;
                var dirFromCharacterToFoe = (objPosition - LegsPosition).normalized;
                var isFacingTowardsFoe = dirFromCharacterToFoe.x > 0 == _characterState.IsFacingRight();

                var blockingObjectsInAttackDirection = Physics2D.Raycast(LegsPosition,
                    dirFromCharacterToFoe,
                    Vector2.Distance(objPosition, LegsPosition), _whatIsSolid);

                if (blockingObjectsInAttackDirection.collider != null) {
                    continue;
                }

                if (isFacingTowardsFoe) {
                    validInteractables.Insert(validInteractablesInfrontOfCharacter, interaction);
                    validInteractablesInfrontOfCharacter ++;
                } else {
                    validInteractables.Add(interaction);
                }
                
            }

            return validInteractables.ToArray();
        }


        public void FilterNearbyCharactersAroundHitPointByDistance(ref IInteractableBody[] interactedColliders, Vector2 hitPosition, float distanceLimiter = 0.5f) {

            if (interactedColliders.Length < 2) {
                return;
            }

            var interactedCollider = interactedColliders.FirstOrDefault();

            Vector3 hitPoint = interactedCollider != null ? interactedCollider.GetCenterPosition() : hitPosition;

            /*var solidOutRadiusObjectsColliders =
                Physics2D.OverlapCircleAll(hitPoint, LegsRadius + 0.2f, _whatIsSolid).
                    Where(collz => collz.gameObject != gameObject).ToArray();

            var isHitPointNearAnySurfaceAndAttackValidForMultiHit = solidOutRadiusObjectsColliders.Length > 0;

            if (!isHitPointNearAnySurfaceAndAttackValidForMultiHit) {
                interactedColliders = new Collider2D[] { interactedColliders.FirstOrDefault() };
                return;
            }*/

            var damageableColliders = interactedColliders.Where(
                obj => Vector2.Distance(obj.GetCenterPosition(), hitPosition) < distanceLimiter).OrderBy(obj
                        => Vector2.Distance(obj.GetCenterPosition(), hitPosition)).ToArray();
            
            interactedColliders = damageableColliders;
        }

        public bool TryGetRopeDirection(Vector2 dir, ref Vector2 endPosition, ref Quaternion rotation, out Vector2 cursorPosition, ref float distancePercentReached) {

            var distance = 12;
            var result = RaycastAgainstSolid(dir, distance, out var hitPosition, ref distancePercentReached);
            endPosition = hitPosition;

            rotation = Quaternion.identity;

            cursorPosition = (Vector2)LegsPosition + dir.normalized * 6;

            return result;
        }

        public bool TryGetProjectileDirection(Vector2 dir, ref Vector2 endPosition, ref Quaternion rotation, ref float distancePercentReached) {

            var distance = 9;
            var result = RaycastAgainstSolid(dir, distance, out var hitPosition, ref distancePercentReached);
            endPosition = hitPosition;

            // Create a rotation using the calculated angle by Atan2
            var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90;
            rotation = Quaternion.AngleAxis(angle, Vector3.forward);

            return result;
        }

        public bool TryGetJumpTrajectory(Vector2 dir, ref Vector2 endPosition, ref Vector3[] linePoints) {

            //calculate the points new path for aiming and charging jump
            List<Vector3> points = new List<Vector3>();

            float xcomponent = dir.normalized.x;
            float ycomponent = dir.normalized.y;
            //float force = (nCounter - (maxTimeToEndCharging - Time.time)) * (maxForce - minForce) + minForce;
            float force = _stats.JumpForce * 0.25f;
            //force = Mathf.Clamp(force, minForce, maxForce);
            Vector2 jumpDir = new Vector2(xcomponent, ycomponent);
            Vector2 u = jumpDir * (force);
            Vector2 g = new Vector2(0f, -10f);
            Vector2 p1 = LegsPosition;
            points.Add(p1);

            Vector2 s, p2;
            RaycastHit2D fhit;

            var hasObjectBlockingTrajectory = false;

            //for (float t = 0; t < 2f; t += 0.02f) {
            for (float t = 0; t < 0.35f; t += 0.015f) {
                s = (u * t) + (0.5f * g * t * t);
                p2 = p1 + s;

                fhit = Physics2D.Raycast(p1, u, Vector2.Distance(p1, p2), _whatIsSolid);
                if (fhit.collider != null) {
                    //Debug.DrawLine(p1, fhit.point, Color.red, 0.2f);
                    Debug.DrawLine(fhit.point, p2, Color.green, 1f);
                    hasObjectBlockingTrajectory = true;
                    //m_AimingFocusPos = fhit.point;
                    t = 0.35f; // to end the 'for' loop
                } else {
                    Debug.DrawLine(p1, p2, Color.red, 2f); //0.2f);
                    
                    //m_AimingFocusPos = p1;
                }

                points.Add(p2);
                p1 = p2;
                u = u + (g * t);

            }

            linePoints = new Vector3[points.Count];
            for (int i = 0; i < points.Count; i++) {
                linePoints[i] = new Vector3(points[i][0], points[i][1], 0);
            }

            // Create a rotation using the calculated angle by Atan2
            //var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90;
            //linePoints = Quaternion.AngleAxis(angle, Vector3.forward);

            return hasObjectBlockingTrajectory;
        }

        private bool RaycastAgainstSolid(Vector2 dir, int distance, out Vector2 endPosition, ref float distancePercentReached) {

            var TargetDir = dir.normalized * distance;
            var TargetPoint = (Vector2)LegsPosition + TargetDir;
            var hit = Physics2D.Raycast((Vector2)LegsPosition, dir, TargetDir.magnitude, _whatIsSolid);

            var hitFound = hit.collider != null;

            endPosition = hitFound ? hit.collider.ClosestPoint(hit.point) : TargetPoint;
            _debug.DrawLine((Vector2)LegsPosition, endPosition, hitFound ? Color.green : Color.grey, 2);
            distancePercentReached = !hitFound ? 1 : HelperFunctions.PercentReachedBetweenPoints(LegsPosition, TargetPoint, endPosition);
            return hitFound;
        }

        public void TryGetDodgeDirection(Vector2 dodgeDirection, ref Vector2 endPosition) {

            var isPerformingDodge = dodgeDirection != Vector2.zero;
            if (!isPerformingDodge) {
                return;
            }

            var potentialEndPosition = (Vector2)LegsPosition + dodgeDirection;

            // Calculate the perpendicular vector to the dodgeDirection
            var perpendicularDirection = new Vector2(-dodgeDirection.y, dodgeDirection.x).normalized;

            // Perform raycasts from the edges of the collider to detect obstacles in the dodge path
            var checkDistance = (dodgeDirection + dodgeDirection.normalized * LegsRadius).magnitude;
            var perpendicularDistance = perpendicularDirection * (LegsRadius / 2);
            var hitLeft = Physics2D.Raycast((Vector2)LegsPosition - perpendicularDistance, dodgeDirection, checkDistance, _whatIsSolid);
            var hitRight = Physics2D.Raycast((Vector2)LegsPosition + perpendicularDistance, dodgeDirection, checkDistance, _whatIsSolid);

            if (hitLeft.collider != null) { _debug.DrawLine((Vector2)LegsPosition - perpendicularDistance, hitLeft.point, Color.grey, 2); }
            if (hitRight.collider != null) { _debug.DrawLine((Vector2)LegsPosition + perpendicularDistance, hitRight.point, Color.grey, 2); }

            if (hitLeft.collider != null || hitRight.collider != null) {
                // Adjust the end position to avoid the collider
                var hasLeftHit = hitLeft.collider != null;
                var hasRightHit = hitRight.collider != null;
                // Get the higer point out of the two hits if possible, if not then gets the original hit
                var higherHit =
                    hasLeftHit && hasRightHit ? hitLeft.point.y > hitLeft.point.y ? hitLeft.point : hitRight.point :
                    hasLeftHit ? hitLeft.point : hitRight.point;
                endPosition = (higherHit) - dodgeDirection.normalized * (LegsRadius);

            } else {
                // If no obstacles are found in the raycasts, set the end position to the potential end position
                endPosition = potentialEndPosition;
            }

            _debug.DrawLine(LegsPosition, endPosition, Color.green, 2);

        }

        public Vector3 LegsPosition => _legsCollider.transform.position;
        public float LegsRadius => _legsCollider.radius;
        
        public void SetMinLevel() => UpdateStats(0);

        public void SetLevel(int level) => UpdateStats(level);

        public void SetMaxLevel() => UpdateStats(_stats.MaxLevel);

        public void Revive() => UpdateStats();

        public Vector2 Velocity => _rigidbody.velocity;

        public CharacterStats Stats => _stats;

        public Character2DState State => _characterState;
        
        private void UpdateStats(int level = -1) {

            level = level == -1 ? _stats.GetCurrentLevel() : level;
            _stats.UpdateStats(level);
            OnUpdatedStats?.Invoke(null);

            if (_rigAnimator != null) {
                _rigAnimator.UpdateBodyScale(_stats.BodySize);
            }

        }

        protected override Task Initialize() {

            _logic = SingleController.GetController<LogicController>();
            _debug = SingleController.GetController<DebugController>();

            _whatIsSurface = _logic.GameplayLogic.GetSurfaces();
            _whatIsSolid = _logic.GameplayLogic.GetSolidSurfaces();
            _whatIsDamageableCharacter = _logic.GameplayLogic.GetDamageables();
            _characterState = new Character2DState();

            // todo: check if get logic is more performant under benchmark
            //_whatIsDamageableCharacter = _logic.TryGet<GameplayLogic>().GetDamageables();

            _motor = new JointMotor2D();
            _motor.maxMotorTorque = CharacterMaxMotorTorque;

            _senses.Init(_whatIsDamageableCharacter, _whatIsDamageable);

            UpdateStats();
            _stats.InitUpgrades();
            _rigAnimator?.Init();

            OnUpdatedStats?.Invoke(null);

            _anchor ??= gameObject.AddComponent<AnchorHandler>();
            _cling.connectedBody = SingleController.GetController<AnchorsController>().GetAnchorRigidbody(_anchor);
            
            return Task.CompletedTask;
        }

        protected override void Clean() {

        }

        private float GetPlayerLegsHeight() => _legs.anchor.y;

        public void DoMove(float speed) {

            if (speed != 0) {
                FlipCharacterTowardsPoint(speed < 0);
            }
            _motor.motorSpeed = speed;
            _legs.motor = _motor;
        }
        
        public void FlipCharacterTowardsPoint(bool facingRight) {

            _characterState.SetFacingRight(facingRight);
            _rigAnimator.SetFacingRight(facingRight);
        }

        public Collider2D GetCollider() => this != null ? _legsCollider ?? null : null;
        public Transform GetTransform() => this != null ? transform ?? null : null;
        public Definitions.CharacterState GetCurrentState() => _characterState.CurrentState;

        public Vector3 GetCenterPosition() => LegsPosition;

        public Vector2 GetVelocity() => State.Velocity;

        public float GetHpPercent() => _stats.GetHealthPercentage();

        public int DealDamage(int damage) {

            var newHealthPoint = Mathf.Clamp(_stats.Health - damage, 0, Stats.MaxHealth);
            _stats.UpdateHealth(newHealthPoint);

            if (newHealthPoint != 0) {
                //_debug.Log("HP " + Stats.Health);
            } else {
                //_debug.Log("Dead");
                SetSpriteOrder(0);
            }

            OnUpdatedStats?.Invoke(new List<DisplayedableStatData>() { new DisplayedableStatData(Definitions.CharacterDisplayableStat.Health , Stats.GetHealthPercentage()) });

            return newHealthPoint;
        }

        public void SetSpriteOrder(int orderLayer) {

            _sprite ??= _rigAnimator.GetComponentInChildren<SpriteRenderer>();
            
            if (_sprite != null) {
                _sprite.sortingOrder = orderLayer;
            }
           
        }

        public void ApplyForce(Vector2 forceDir) {
            SetForceDir(forceDir);
        }

        public void SetTargetPosition(Vector2 newPosition, float percentToPerform = 1) {
            if (newPosition != Vector2.zero) {
                newPosition.y -= GetPlayerLegsHeight();
            }
            NewPositionToSetOnFixedUpdte = newPosition;
        }

        public void SetAsGrabbing(IInteractableBody grabbedBody) {

            _characterState.SetAsGrabbing(grabbedBody);
        }

        public void SetAsGrabbed(IInteractableBody anchorParent, Vector2 position) {

            _transformMover.EndMove();
            transform.position = position;

            _anchor.ModifyAnchor(position, anchorParent?.GetTransform());
            _cling.connectedBody.simulated = anchorParent != null;

            _characterState.SetAsGrabbed(anchorParent);
        }

        public bool IsGrabbing() => _characterState.IsGrabbing();

        public bool IsGrabbed() => _cling.connectedBody.simulated && _characterState.IsGrabbed();
        
        public IInteractableBody GetGrabbedTarget() {
            return _characterState.GetGrabbedTarget();
        }

        public void SetForceDir(Vector2 newForceDir, bool additionalForce = false) {

            if (additionalForce) {
                ForceDirToSetOnFixedUpdate += newForceDir;

            } else if (ForceDirToSetOnFixedUpdate == Vector2.zero) {
                ForceDirToSetOnFixedUpdate = newForceDir;
            }

        }
        
        private void Update() {

            UpdateCharacterState();
        }

        private void FixedUpdate() {

            
            if (NewPositionToSetOnFixedUpdte != Vector3.zero) {

                SwitchRigidBodyType(RigidbodyType2D.Kinematic, 0, true);
                _transformMover.MoveToPosition(transform, NewPositionToSetOnFixedUpdte, 0.15f); // 0.15f is the fastest combat turn
                SwitchRigidBodyType(RigidbodyType2D.Dynamic, 0.15f, true);
                _characterState.SetMoveAnimation(Time.time + (Stats.AttackDurationInMilliseconds * 0.001f));
                NewPositionToSetOnFixedUpdte = Vector3.zero;
                
            } else if (ForceDirToSetOnFixedUpdate != Vector2.zero) {

                SwitchRigidBodyType(RigidbodyType2D.Dynamic, 0, false);
                _transformMover.EndMove();
                _rigidbody.velocity = ForceDirToSetOnFixedUpdate;
                ForceDirToSetOnFixedUpdate = Vector2.zero;
            }
            
            UpdateAnimatorRigRotation();

        }
        
        public void SetProjectileState(bool isActive) {
            
            if (_bodyDamager == null) {
                return;
            }

            _bodyDamager.gameObject.SetActive(isActive);
        }

        public EffectData GetEffectData(Definitions.Effect2DType effect) {

            if (_effects == null) {
                return null;
            }

            return _effects.GetDataByType(effect);
        }

        private async void SwitchRigidBodyType(RigidbodyType2D bodyType, float delayDuration = 0, bool resetVelocity = false) {

            if (delayDuration > 0) {
                await Task.Delay(TimeSpan.FromSeconds(delayDuration));
            }
            
            _rigidbody.bodyType = bodyType;

            _legsRigidBody ??= _legsCollider.GetComponent<Rigidbody2D>();
            _legsRigidBody.bodyType = bodyType;

            if (resetVelocity) {
                _rigidbody.velocity = Vector2.zero;
                _legsRigidBody.velocity = Vector2.zero;
            }
        }

        private void UpdateAnimatorRigRotation() {

            if (_rigAnimator == null) {
                return;
            }

            var isAlive = Stats.Health > 0;
            
            var terrainQuaternion = _characterState.ReturnForwardDirByTerrainQuaternion();
            if (_characterState.IsJumping()) {
                // aditional 50 to the jump direction, so character will start into rotation when jum[ing to create the flip effect. 50 is pretty random, 90 will turn the flip to obselete
                terrainQuaternion *= Quaternion.AngleAxis(_characterState.GetFacingRightAsInt() * 50, new Vector3(0, 0, 1));
            }
            
            _rigAnimator.UpdateRigRotation(isAlive, terrainQuaternion);
        }

        private void UpdateCharacterState() {

            var prevState = _characterState.CurrentState;
            
            // solid objects are considered only as valid ground for player to move on
            var _groundColliders =
                Physics2D.OverlapCircleAll(LegsPosition, GroundedRadius, _whatIsSolid);

            _solidObjectsColliders =
                Physics2D.OverlapCircleAll(LegsPosition, LegsRadius, _whatIsSurface);
            
            var hitPoint = Vector2.zero;
            var collDir = Vector2.zero;
            var collLayer = -1;
            var legsPosition = (Vector2)LegsPosition;

            var allSurfaces = new List<Collider2D>();
            if (_groundColliders.Length > 0) { allSurfaces.AddRange(_groundColliders); }
            if (_solidObjectsColliders.Length > 0) { allSurfaces.AddRange(_solidObjectsColliders); }

            if (allSurfaces.Count > 0) {
                foreach (var coll in allSurfaces) {
                    if (coll.gameObject == gameObject) {
                        continue;
                    }

                    var newPoint = coll.ClosestPoint(legsPosition);
                    if (hitPoint == Vector2.zero || (hitPoint - legsPosition).magnitude > (newPoint - legsPosition).magnitude ) {
                        
                        hitPoint = newPoint;
                        collDir = (hitPoint - legsPosition).normalized;
                        collLayer = coll.gameObject.layer;
                    }
                }
            }
            
            _solidOutRadiusObjectsColliders =
                Physics2D.OverlapCircleAll(LegsPosition, LegsRadius + AdditionalOuterRadius, _whatIsSolid).
                    Where(collz => collz.gameObject != gameObject).ToArray();
            
            var outerRadiusCollDir = Vector2.zero;
            if (_solidOutRadiusObjectsColliders.Length > 0) {
                var closestOuterRadiusColliderPosition = Vector2.zero;
                foreach (var coll in _solidOutRadiusObjectsColliders) {
                    var newPoint = coll.ClosestPoint(legsPosition);
                    if (closestOuterRadiusColliderPosition == Vector2.zero || (closestOuterRadiusColliderPosition - legsPosition).magnitude > (newPoint - legsPosition).magnitude ) {
                        
                        closestOuterRadiusColliderPosition = newPoint;
                        outerRadiusCollDir = (closestOuterRadiusColliderPosition - legsPosition).normalized;
                    }
                }
            }

            //_debug.DrawRay(hitPoint, collDir * 5, Color.white, 1f);
            //_debug.Log("dir "+ collDir+", go = "+i.name);
            var hasGroundBeneathByRayCast = _characterState.IsGrounded();
            if (!hasGroundBeneathByRayCast && (collDir != Vector2.zero || outerRadiusCollDir != Vector2.zero)) {
                
                var downwardAndSlightForwardDir = HelperFunctions.RotateVector(-(Vector2.up) * 4, _characterState.GetFacingRightAsInt() * 9);
                hasGroundBeneathByRayCast = Physics2D.Raycast(legsPosition, downwardAndSlightForwardDir, 4, _whatIsSolid);
                _debug.DrawRay(legsPosition, downwardAndSlightForwardDir, hasGroundBeneathByRayCast ? Color.yellow : Color.grey, hasGroundBeneathByRayCast ? 2 : 1);
                if (hasGroundBeneathByRayCast && !_characterState.CanMoveOnSurface()) {
                    _characterState.SetAsLanded();
                }
            }
            
            _characterState.DiagnoseState(hitPoint, collDir, outerRadiusCollDir, _rigidbody.velocity, hasGroundBeneathByRayCast);

            var shouldCallStateChange = prevState != _characterState.CurrentState ;// || _characterState.CurrentState == Definitions.CharacterState.Grounded;
            
            if (shouldCallStateChange) {

                OnStateChanged?.Invoke(_characterState);

                if (_characterState.CurrentState == Definitions.CharacterState.Landed && ForceDirToSetOnFixedUpdate != Vector2.zero) {
                    ForceDirToSetOnFixedUpdate = new Vector2(_rigidbody.velocity.x, 0);
                    DoMove(0);
                }
            }
        }

        public void SetJumping(float time) {

            _characterState.SetJumping(time);
        }
        
        public Vector3 IsFreeFalling(float distanceToCheckWhenFreeFalling) {

            if (State.CurrentState is not (Definitions.CharacterState.Jumping or Definitions.CharacterState.Falling or Definitions.CharacterState.Landed) || Velocity.y > 0 || State.IsTouchingAnySurface()) {
                //_debug.LogWarning("Is not free falling because state is "+State.State);
                return Vector3.zero;
            }
            
            var hit = FreeFallRayCast(LegsPosition,  Velocity.normalized, distanceToCheckWhenFreeFalling);
            //_debug.DrawRay(legsPosition, Velocity.normalized, Color.black, 3);
            if (hit.collider != null) {
                //_debug.DrawLine(legsPosition, hit.point, Color.white, 2);
                return hit.point;
            }

            var newAngleToCheck = Velocity;
            newAngleToCheck.y -= 10;
            hit = FreeFallRayCast(LegsPosition, newAngleToCheck , distanceToCheckWhenFreeFalling);
            //_debug.DrawRay(legsPosition, newAngleToCheck, Color.magenta, 3);
            if (hit.collider != null) {
                //_debug.DrawLine(legsPosition, hit.point, Color.white, 2);
                return hit.point;
            }
            
            newAngleToCheck = Velocity;
            newAngleToCheck.y -= 10*2;
            hit = FreeFallRayCast(LegsPosition, newAngleToCheck , distanceToCheckWhenFreeFalling);
            //_debug.DrawRay(legsPosition, newAngleToCheck, Color.cyan, 3);
            if (hit.collider != null) {
                //_debug.DrawLine(legsPosition, hit.point, Color.white, 2);
                return hit.point;
            }
            
            return Vector3.zero;
        }

        private RaycastHit2D FreeFallRayCast(Vector3 startPos, Vector3 direction, float distanceToCheckWhenFreeFalling) {

            return Physics2D.Raycast(startPos, direction, distanceToCheckWhenFreeFalling,_whatIsSolid);
        }

        public void SetNavigationDir(Definitions.NavigationType inputDirection) => State.SetNavigationDir(inputDirection);

        public Definitions.NavigationType GetNavigationDir() => State.NavigationDir;

        // todo make this should get the data from the CharacterBase, currently each character can revive itself when health is below 100%
        public bool CanPerformSuper() => Stats.GetHealthPercentage() < 1;

        // todo make this should get the data from the CharacterBase, and return the special action properties
        public bool TryPerformSuper() {

            var result = State.IsNotMoving();

            return result;
        }

    }

}