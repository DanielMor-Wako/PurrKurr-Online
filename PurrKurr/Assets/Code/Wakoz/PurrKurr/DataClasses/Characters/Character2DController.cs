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
using Code.Wakoz.PurrKurr.DataClasses.GameCore.Detection;
using Code.Wakoz.PurrKurr.AnimatorBridge;
using static Code.Wakoz.PurrKurr.DataClasses.Enums.Definitions;

namespace Code.Wakoz.PurrKurr.DataClasses.Characters {

    [DefaultExecutionOrder(15)]
    public class Character2DController : Controller, IInteractableBody {

        public event Action<List<DisplayedableStatData>> OnUpdatedStats;
        public event Action<InteractableObject2DState> OnStateChanged;
        public event Action<Collider2D> OnColliderEntered;
        public event Action<Collider2D> OnColliderExited;

        [SerializeField] private CharacterStats _stats;
        [Tooltip("Effects to be used instead of the default effect")]
        [SerializeField] private EffectsData _effectOverrides;
        [SerializeField] private Rigidbody2D _rigidbody;
        [SerializeField] private CircleCollider2D _legsCollider;
        [SerializeField] private WheelJoint2D _legs;
        [SerializeField] private HingeJoint2D _cling;
        [SerializeField] private JointMotor2D _motor;
        [SerializeField] private TransformMover _transformMover;
        [SerializeField] private DetectionZone _senses;
        [SerializeField] private AnimatorPlayer _modelAnimatorPlayer;
        [SerializeField] private Character2DRig _rigAnimator;
        [SerializeField] private Transform _bodyDamager;
        [Header("Ground RayCheck Settings")]
        [Tooltip("Angle Offset from the origin down direction")]
        [SerializeField][Min(0)] private float GroundCheck_AngleOffset = 6f;
        [Tooltip("Offsets the position to the ray from")]
        [SerializeField] private Vector2 GroundCheck_PosOffset;
        private SpriteRenderer _sprite;

        private Rigidbody2D _legsRigidBody;

        private Vector2 ForceDirToSetOnFixedUpdate;
        private Vector2 ForceDirToAddOnFixedUpdate;

        private LayerMask _whatIsSurface;
        private LayerMask _whatIsSolid;
        private LayerMask _whatIsPlatform;
        private LayerMask _whatIsTraversable;
        private LayerMask _whatIsTraversableCrouch;
        private LayerMask _whatIsSolidForProjectile;
        private LayerMask _whatIsDamageableCharacter;
        private LayerMask _whatIsDamageable;

        private const float GroundedRadius = .7f;
        private const float CharacterMaxMotorTorque = 10000;
        private const float AdditionalOuterRadius = 0.3f;
        private const float DefaultSensesRadius = 10f;
        private float _sensesRadius;
        [SerializeField] private Definitions.ObjectState currState = Definitions.ObjectState.Alive; // remove, for inspector use only
        [SerializeField] private Definitions.ObjectState lastState = Definitions.ObjectState.Dead; // remove, for inspector use only
        private InteractableObject2DState _state;
        private AnchorHandler _anchor;

        private LogicController _logic;
        private DebugDrawController _debug;

        public Collider2D[] _solidColliders;
        public Collider2D[] _traversableColliders;

        public DetectionZone Senses => _senses;
        public float SensesRadius()
        {
            if (_sensesRadius == 0) {
                var radiusSensor = _senses.GetComponent<CircleCollider2D>();
                _sensesRadius = radiusSensor != null ? radiusSensor.radius : DefaultSensesRadius;
            }
            return _sensesRadius;
        }

        public Collider2D[] NearbyInteractables() {
 
            var _nearbyInteractions = _senses.GetColliders();
            
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
                var isFacingTowardsFoe = dirFromCharacterToFoe.x > 0 == _state.IsFacingRight();

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

            var distance = 17;
            var result = RaycastAgainstSurface(_whatIsSolid, dir, distance, out var hitPosition, ref distancePercentReached);
            endPosition = result ? hitPosition : (Vector2)LegsPosition + dir.normalized * distance;

            rotation = Quaternion.identity;

            cursorPosition = (Vector2)LegsPosition + dir.normalized * 6;

            return result;
        }

        public bool TryGetProjectileDirection(Vector2 dir, ref Vector2 endPosition, ref Quaternion rotation, ref float distancePercentReached) {

            var distance = 9;
            var result = RaycastAgainstSurface(_whatIsSolidForProjectile, dir, distance, out var hitPosition, ref distancePercentReached);
            endPosition = hitPosition;

            /*var blockingObjectsInAttackDirection = Physics2D.Raycast(LegsPosition,
                    dir,
                    distance, _whatIsSolidForProjectile);

            if (blockingObjectsInAttackDirection.collider != null)
            {
                endPosition = blockingObjectsInAttackDirection.point;
            }*/

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
            Vector2 g = new Vector2(0f, -9.81f);
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

            endPosition = (Vector2)points[points.Count-1];

            // Create a rotation using the calculated angle by Atan2
            //var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90;
            //linePoints = Quaternion.AngleAxis(angle, Vector3.forward);

            return hasObjectBlockingTrajectory;
        }

        private bool RaycastAgainstSurface(LayerMask _raycastAgainstLayer, Vector2 dir, int distance, out Vector2 endPosition, ref float distancePercentReached) {

            var TargetDir = dir.normalized * distance;
            var TargetPoint = (Vector2)LegsPosition + TargetDir;
            var hit = Physics2D.Raycast((Vector2)LegsPosition, dir, TargetDir.magnitude, _raycastAgainstLayer);
            
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

        public InteractableObject2DState State => _state;

        public void RefreshStats() {
            OnUpdatedStats?.Invoke(null);
        }

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
            _debug = SingleController.GetController<DebugDrawController>();

            var gamePlayerLogic = _logic.GameplayLogic;
            _whatIsSurface = gamePlayerLogic.GetSurfaces();
            _whatIsSolid = gamePlayerLogic.GetSolidSurfaces();
            _whatIsPlatform = gamePlayerLogic.GetPlatformSurfaces();
            _whatIsTraversable = gamePlayerLogic.GetTraversableSurfaces();
            _whatIsTraversableCrouch = gamePlayerLogic.GetTraversableCrouchAreas();
            _whatIsSolidForProjectile = gamePlayerLogic.GetSolidSurfacesForProjectile();
            _whatIsDamageableCharacter = gamePlayerLogic.GetDamageables();
            _state = new InteractableObject2DState();

            // todo: check if get logic is more performant under benchmark
            //_whatIsDamageableCharacter = _logic.TryGet<GameplayLogic>().GetDamageables();

            _motor = new JointMotor2D();
            _motor.maxMotorTorque = CharacterMaxMotorTorque;

            if (_senses == null) {
                Debug.LogWarning($"character {name} is missing _senses collider");
            }
            else
            {
                _senses.OnColliderEntered += OnNearInteractable;
                _senses.OnColliderExited += OnAwayFromInteractable;
            }

            UpdateStats();
            _stats.InitUpgrades();
            _rigAnimator?.Init();

            OnUpdatedStats?.Invoke(null);

            _anchor ??= gameObject.AddComponent<AnchorHandler>();
            _cling.connectedBody = SingleController.GetController<AnchorsController>().GetAnchorRigidbody(_anchor);
            
            return Task.CompletedTask;
        }

        protected override void Clean()
        {
            if (_senses != null)
            {
                _senses.OnColliderEntered -= OnNearInteractable;
                _senses.OnColliderExited -= OnAwayFromInteractable;
            }
        }

        private void OnNearInteractable(Collider2D d)
        {
            if (d == _legsCollider)
            {
                return;
            }
            OnColliderEntered?.Invoke(d);
        }

        private void OnAwayFromInteractable(Collider2D d)
        {
            if (d == _legsCollider)
            {
                return;
            }
            OnColliderExited?.Invoke(d);
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

            _state.SetFacingRight(facingRight);
            _rigAnimator.SetFacingRight(facingRight);
        }

        public Collider2D GetCollider() => this != null ? _legsCollider ?? null : null;
        public Transform GetTransform() => this != null ? transform ?? null : null;
        public Transform GetCharacterRigTransform() => this != null ? _rigAnimator.transform ?? null : null;
        public Definitions.ObjectState GetCurrentState() => _state.CurrentState;

        public Vector3 GetCenterPosition() => LegsPosition;

        public Vector2 GetVelocity() => State.Velocity;

        public float GetHpPercent() => _stats.GetHealthPercentage();

        public void DealDamage(int damage) {

            var newHealthPoint = Mathf.Clamp(_stats.Health - damage, 0, Stats.MaxHealth);
            _stats.UpdateHealth(newHealthPoint);

            if (newHealthPoint != 0) {
                //_debug.Log("HP " + Stats.Health);
            } else {
                //_debug.Log("Dead");
                SetSpriteOrder(0);
            }

            OnUpdatedStats?.Invoke(new List<DisplayedableStatData>() { new DisplayedableStatData(Definitions.CharacterDisplayableStat.Health , Stats.GetHealthPercentage(), Stats.Health.ToString()) });

            //return newHealthPoint;
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

        public void EndTargetPositionMovement() {
            _transformMover.EndMove();
        }

        public void SetTargetPosition(Vector2 newPosition, float percentToPerform = 1) {

            if (newPosition != Vector2.zero) {
                newPosition.y -= GetPlayerLegsHeight();

                SwitchRigidBodyType(RigidbodyType2D.Kinematic, true);
                _transformMover.MoveToPosition(transform, newPosition, 0.15f, // 0.15f is the fastest combat turn
                    () => SwitchRigidBodyType(RigidbodyType2D.Dynamic, true)); 
            }
        }

        public void SetAsClinging(Collider2D wallParent, Vector2 position) {

            _transformMover.EndMove();
            transform.position = position;

            UpdateAnchor(wallParent?.transform, position);

            if (wallParent != null) {
                _state.SetClinging(wallParent);
            } else {
                _state.StopClinging();
            }
        }

        public void SetAsGrabbing(IInteractableBody grabbedBody) => _state.SetAsGrabbing(grabbedBody);

        public void SetAsGrabbed(IInteractableBody anchorParent, Vector2 position) {

            _transformMover.EndMove();
            transform.position = position;

            UpdateAnchor(anchorParent?.GetTransform(), position);

            _state.SetAsGrabbed(anchorParent);
        }

        private void UpdateAnchor(Transform anchorParent, Vector2 position) {

            _anchor.ModifyAnchor(position, anchorParent);
            _cling.connectedBody.simulated = anchorParent != null;
        }

        public bool IsGrabbing() => _state.IsGrabbing();

        public bool IsGrabbed() => _cling.connectedBody.simulated && _state.IsGrabbed();
        
        public IInteractableBody GetGrabbedTarget() {
            return _state.GetGrabbedTarget();
        }

        public void SetForceDir(Vector2 newForceDir) {

            if (newForceDir == Vector2.zero) {
                return;
            }

            ForceDirToSetOnFixedUpdate = newForceDir;
        }

        public void AddForceDir(Vector2 addForceDir) {

            if (addForceDir == Vector2.zero) {
                return;
            }

            ForceDirToAddOnFixedUpdate = addForceDir;
        }

        private void Update() {

            UpdateCharacterState();


            /*if (ForceDirToSetOnFixedUpdate != Vector2.zero) {

                //SwitchRigidBodyType(RigidbodyType2D.Dynamic, false);
                //_transformMover.EndMove();
                _rigidbody.velocity = ForceDirToSetOnFixedUpdate;
                ForceDirToSetOnFixedUpdate = Vector2.zero;
            }*/

            //UpdateAnimatorRigRotation();
        }
        [Min(10)] public float _maxFallingVelocity = 30;
        private Vector2 _fallingVelocity;

        private void FixedUpdate()
        {
            if (ForceDirToSetOnFixedUpdate != Vector2.zero) {

                _rigidbody.velocity = ForceDirToSetOnFixedUpdate;
                ForceDirToSetOnFixedUpdate = Vector2.zero;
            
            } else if (ForceDirToAddOnFixedUpdate != Vector2.zero) {

                _rigidbody.velocity += (ForceDirToAddOnFixedUpdate);
                ForceDirToAddOnFixedUpdate = Vector2.zero;
            
            } else if (_rigidbody.velocity.y < -_maxFallingVelocity) {
            
                _fallingVelocity.Set(_rigidbody.velocity.x, -_maxFallingVelocity);
                _rigidbody.velocity = _fallingVelocity;
            }

            UpdateAnimatorRigRotation();
        }

        public void SetProjectileState(bool isActive) {
            
            if (_bodyDamager == null) {
                return;
            }

            _bodyDamager.gameObject.SetActive(isActive);
        }

        public EffectData GetEffectOverride(Definitions.Effect2DType effect) {

            if (_effectOverrides == null) {
                return null;
            }

            var overrideEffect = _effectOverrides.GetDataByType(effect);
            return overrideEffect;
        }

        private Task SwitchRigidBodyType(RigidbodyType2D bodyType, bool resetVelocity = false) {
            _rigidbody.bodyType = bodyType;

            _legsRigidBody ??= _legsCollider.GetComponent<Rigidbody2D>();
            _legsRigidBody.bodyType = bodyType;

            if (resetVelocity) {
                _rigidbody.velocity = Vector2.zero;
                _legsRigidBody.velocity = Vector2.zero;
            }

            return Task.CompletedTask;
        }

        private void UpdateAnimatorRigRotation() {

            if (_rigAnimator == null) {
                return;
            }

            if (Stats.Health <= 0) {
                _rigAnimator.UpdateRigRotation(Quaternion.LookRotation(Vector3.forward, Vector2.down), 0);
                return;
            }

            var defaultOffsetWhenNoJumping = 
                State.CurrentState is ObjectState.Jumping ? State.IsFacingRight() ? 90 : 270 : -1;
            
            var terrainQuaternion = _state.ReturnForwardDirByTerrainQuaternion();
            /*if (_state.IsJumping()) {
                // aditional 50 to the jump direction, so character will start into rotation when jumping to create the flip effect. 50 is pretty random, 90 will turn the flip to obselete
                terrainQuaternion *= Quaternion.AngleAxis(_state.GetFacingRightAsInt() * 50, new Vector3(0, 0, 1));
            }*/

            _rigAnimator.UpdateRigRotation(terrainQuaternion, defaultOffsetWhenNoJumping);
        }

        private void UpdateCharacterState() {

            var prevState = _state.CurrentState;

            UpdateOverlappingColliders(ref _solidColliders, ref _traversableColliders);

            var hitPoint = Vector2.zero;
            var collDir = Vector2.zero;
            var collLayer = -1;
            var legsPosition = (Vector2)LegsPosition;

            var isInTraversableSurface = false;
            var isInTraversableCrouchArea = false;

            // find specific colliders to priorotize rather than soilds and distance?
            /*if (_solidColliders.Length > 0) {
                var isPlatformSurface = false;
                foreach (var coll in _solidColliders) {
                    isPlatformSurface = HelperFunctions.IsObjectInLayerMask(coll.gameObject.layer, ref _whatIsPlatform);
                    // validate the when isPlatformSurface occurs, the allSurfaces includes the collider.
                    // as moving on platforms in angle might not always register the surface
                    if (!isPlatformSurface || isPlatformSurface && !_state.IsUpwardMovement()) {
                        highestPriorityCollider = coll; break;
                    }
                }
            }*/

            var highestPriorityCollider = _state.GetClingableObject();
            if (highestPriorityCollider != null) {

                hitPoint = highestPriorityCollider.ClosestPoint(legsPosition);
                collDir = (hitPoint - legsPosition).normalized;
                collLayer = highestPriorityCollider.gameObject.layer;

            } else {

                highestPriorityCollider = _solidColliders.FirstOrDefault();
                if (highestPriorityCollider != null) {

                    hitPoint = highestPriorityCollider.ClosestPoint(legsPosition);
                    collDir = (hitPoint - legsPosition).normalized;
                    collLayer = highestPriorityCollider.gameObject.layer;
                
                } else {

                    highestPriorityCollider = _traversableColliders.FirstOrDefault();
                    if (highestPriorityCollider != null) {

                        collLayer = highestPriorityCollider.gameObject.layer;
                        isInTraversableSurface = HelperFunctions.IsObjectInLayerMask(collLayer, ref _whatIsTraversable);
                    }
                }
            }

            var hasGroundBeneathByRayCast = _state.IsGrounded();
            if (!hasGroundBeneathByRayCast && (collDir != Vector2.zero)) {

                var facingDirectionAsInt = _state.GetFacingRightAsInt();
                var forwardDownwardDir = HelperFunctions.RotateVector(-(Vector2.up) * 2, facingDirectionAsInt * GroundCheck_AngleOffset);
                var backwardDownwardDir = HelperFunctions.RotateVector(-(Vector2.up) * 2, facingDirectionAsInt * -GroundCheck_AngleOffset);
                var xOffsetFromCenter = facingDirectionAsInt * LegsRadius * GroundCheck_PosOffset.x;
                var yOffsetFromCenter = LegsRadius * GroundCheck_PosOffset.y;
                var legsForwardPosition = new Vector3(legsPosition.x + xOffsetFromCenter, legsPosition.y + yOffsetFromCenter, 0);
                var legsBackwardPosition = new Vector3(legsPosition.x - xOffsetFromCenter, legsPosition.y + yOffsetFromCenter, 0);

                hasGroundBeneathByRayCast = Physics2D.Raycast(legsForwardPosition, forwardDownwardDir, 4, _whatIsSurface);//_whatIsSolid);
                _debug.DrawRay(legsForwardPosition, forwardDownwardDir, hasGroundBeneathByRayCast ? Color.yellow : Color.grey, hasGroundBeneathByRayCast ? 2 : 1);
                if (!hasGroundBeneathByRayCast) {
                    hasGroundBeneathByRayCast = Physics2D.Raycast(legsPosition, -(Vector2.up) * 2, 4, _whatIsSurface);
                    _debug.DrawRay(legsPosition, -(Vector2.up) * 2, hasGroundBeneathByRayCast ? Color.yellow : Color.grey, hasGroundBeneathByRayCast ? 2 : 1);
                    if (!hasGroundBeneathByRayCast) {
                        hasGroundBeneathByRayCast = Physics2D.Raycast(legsBackwardPosition, backwardDownwardDir, 4, _whatIsSurface);
                        _debug.DrawRay(legsBackwardPosition, backwardDownwardDir, hasGroundBeneathByRayCast ? Color.yellow : Color.grey, hasGroundBeneathByRayCast ? 2 : 1);
                    }
                }
                
                if (hasGroundBeneathByRayCast && !_state.CanMoveOnSurface()) {
                    _state.SetAsLanded();
                }
            }

            if (hasGroundBeneathByRayCast) {

                foreach (var crouchArea in _traversableColliders) {
                    var potentiallyCrouchLayer = crouchArea.gameObject.layer;
                    var isInCrouchSurface = HelperFunctions.IsObjectInLayerMask(potentiallyCrouchLayer, ref _whatIsTraversableCrouch);
                    if (isInCrouchSurface) {
                        isInTraversableCrouchArea = true;
                    }
                }

                if (isInTraversableCrouchArea && _logic.InputLogic.IsNavigationDirValidAsDown(_state.NavigationDir)) {
                    _legsCollider.radius = 0.8f;
                } else if (_legsCollider.radius != 1) {
                    _legsCollider.radius = 1;
                }

            }

            _state.DiagnoseState(hitPoint, collDir, collLayer, legsPosition, _rigidbody.velocity, hasGroundBeneathByRayCast, isInTraversableSurface, isInTraversableCrouchArea);

            var isStateChanged = prevState != _state.CurrentState;
            
            if (isStateChanged) {

                OnStateChanged?.Invoke(_state);

                if (GetHpPercent() <= 0) {
                    _state.SetState(Definitions.ObjectState.Dead);
                }

                TryPlayAnimationClip(_state.CurrentState);

                this.currState = _state.CurrentState;
                this.lastState = prevState;
                // Additional specific states:
                // Resets rigidbody velocity when player has landed
                if (_state.CurrentState == Definitions.ObjectState.Landed && ForceDirToSetOnFixedUpdate != Vector2.zero) {
                    ForceDirToSetOnFixedUpdate = new Vector2(_rigidbody.velocity.x, 0);
                    DoMove(0);
                }
            }

            _modelAnimatorPlayer?.SetPlaySpeed(GetClipSpeedByState(_state.CurrentState));
        }

        // Todo: use extention for this monsteracity
        private void TryPlayAnimationClip(Definitions.ObjectState currentState) {

            var animClip = GetClipByState(currentState);
            
            _modelAnimatorPlayer?.PlayAnimation(animClip);
        }

        private float GetClipSpeedByState(ObjectState currentState) {
            
            if (currentState is not ObjectState.Running /*or ObjectState.Crawling*/) {
                return 1;
            }
            var minClipSpeed = 0.5f;
            var speedGap = 1 - minClipSpeed;
            var speedPer = Mathf.Clamp01(Velocity.magnitude / 20) * speedGap + minClipSpeed; // 20 as the min speed for the lowest level stat

            return speedPer;
        }

        private AnimClipType GetClipByState(ObjectState currentState) {

            return currentState switch {

                ObjectState.Running or ObjectState.TraversalRunning => AnimClipType.Moving,
                ObjectState.Jumping => AnimClipType.Jump,
                ObjectState.Falling => AnimClipType.Fall,
                ObjectState.Grounded => AnimClipType.Init,
                ObjectState.Grabbing or ObjectState.StandingUp => AnimClipType.StandUp,
                ObjectState.Crouching => AnimClipType.Crouch,
                ObjectState.Crawling => AnimClipType.Crawl,
                ObjectState.Stunned => AnimClipType.Stunned,
                ObjectState.LightAttack => AnimClipType.LightAttack,
                ObjectState.MediumAttack => AnimClipType.MediumAttack,
                ObjectState.HeavyAttack => AnimClipType.HeavyAttack,
                ObjectState.AerialAttack => AnimClipType.AerialAttack,
                ObjectState.DashAttack => AnimClipType.DashAttack,
                ObjectState.SpecialAbility => AnimClipType.SpecialAbility,
                ObjectState.Blocking => AnimClipType.Block,
                ObjectState.Dodging => AnimClipType.Dodge,
                ObjectState.AimingJump => AnimClipType.AimJump,
                ObjectState.AimingRope => AnimClipType.AimRope,
                ObjectState.AimingProjectile => AnimClipType.AimProjectile,
                ObjectState.WallClinging  => AnimClipType.WallCling,
                ObjectState.WallClimbing  => AnimClipType.WallClimb,
                ObjectState.RopeClinging => AnimClipType.RopeCling,
                ObjectState.RopeClimbing => AnimClipType.RopeClimb,
                ObjectState.Landed => AnimClipType.Landed,
                ObjectState.Dead => AnimClipType.DropDead,

                _ => AnimClipType.Init,
            };
        }

        private void UpdateOverlappingColliders(ref Collider2D[] solids, ref Collider2D[] traversables) {

            var allColliders = Physics2D.OverlapCircleAll(LegsPosition, LegsRadius + AdditionalOuterRadius, _whatIsSurface)
                .Where(coll => coll.gameObject != gameObject).ToArray();

            IList<Collider2D> traversablesList = allColliders.Where(coll => coll.isTrigger).ToList();
            IList<Collider2D> solidsList = allColliders.Except(traversablesList)
                .OrderBy(coll => Vector2.Distance(coll.ClosestPoint(transform.position), transform.position)).ToList();

            var collidersToTransferFromSolidsToTraversables = new List<Collider2D>();
            foreach (var coll in solidsList) {
                var isPlatformSurface = HelperFunctions.IsObjectInLayerMask(coll.gameObject.layer, ref _whatIsPlatform);
                // validates PlatformSurface, initially platform counts as solid but set as traversable when the player is moving up thru the platform
                if (isPlatformSurface && _state.IsUpwardMovement() && !_state.IsGrounded()) {
                    collidersToTransferFromSolidsToTraversables.Add(coll);
                }
            }

            foreach (var coll in collidersToTransferFromSolidsToTraversables) {
                solidsList.Remove(coll);
                traversablesList.Add(coll);
            }

            var clingableObject = _state.GetClingableObject();
            if (clingableObject != null) {
                HelperFunctions.MoveOrAddItemAsFirst(ref solidsList, ref clingableObject, false);
            }

            solids = solidsList.ToArray();
            traversables = traversablesList.ToArray();
        }

        public int GetSurfaceCollLayer() {

            var closestColl = _solidColliders.FirstOrDefault();
            closestColl ??= _traversableColliders.FirstOrDefault();

            return closestColl != null ? closestColl.gameObject.layer : -1;
        }

        public void SetJumping(float time) {

            _state.SetJumping(time);
        }
        
        public Vector3 IsFreeFalling(float distanceToCheckWhenFreeFalling) {

            if (State.CurrentState is not (Definitions.ObjectState.Jumping or Definitions.ObjectState.Falling or Definitions.ObjectState.Landed) || Velocity.y > 0 || State.IsTouchingAnySurface()) {
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

            return Physics2D.Raycast(startPos, direction, distanceToCheckWhenFreeFalling, _whatIsSurface);
        }

        public void SetNavigationDir(Definitions.NavigationType inputDirection) => State.SetNavigationDir(inputDirection);

        public Definitions.NavigationType GetNavigationDir() => State.NavigationDir;

        public float NegateYDownwardForce() {
            return Mathf.Abs(Physics2D.gravity.y) * _rigidbody.mass * _rigidbody.gravityScale;
        }

        // todo make this should get the data from the CharacterBase, currently each character can revive itself when health is below 100%
        public bool CanPerformSuper() => Stats.GetHealthPercentage() < 1;

        // todo make this should get the data from the CharacterBase, and return the special action properties
        public bool TryPerformSuper() {

            var result = State.IsNotMoving();

            return result;
        }

    }

}