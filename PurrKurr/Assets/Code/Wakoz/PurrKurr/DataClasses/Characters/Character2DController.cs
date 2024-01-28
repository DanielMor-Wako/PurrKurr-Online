using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Code.Wakoz.PurrKurr.DataClasses.Enums;
using Code.Wakoz.PurrKurr.DataClasses.GameCore;
using Code.Wakoz.PurrKurr.Logic.GameFlow;
using Code.Wakoz.PurrKurr.DataClasses.GameCore.Anchors;
using Code.Wakoz.Utils.GraphicUtils.TransformUtils;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.Characters {

    [DefaultExecutionOrder(15)]
    public class Character2DController : Controller, IInteractableBody {

        public event Action<List<DisplayedableStatData>> OnUpdatedStats;
        public event Action<Character2DState> OnStateChanged;

        [SerializeField] private CharacterStats _stats;
        [SerializeField] private Rigidbody2D _rigidbody;
        [SerializeField] private CircleCollider2D _legsCollider;
        [SerializeField] private WheelJoint2D _legs;
        [SerializeField] private HingeJoint2D _cling;
        [SerializeField] private JointMotor2D _motor;
        [SerializeField] private TransformMover _transformMover;
        [SerializeField] private CharacterSenses _senses;
        [SerializeField] private Transform _bodyDamager;
        [SerializeField] private Character2DRig _rigAnimator;
        private SpriteRenderer _sprite;

        private Rigidbody2D _legsRigidBody;

        private Vector3 NewPositionToSetOnFixedUpdte;
        private Vector2 ForceDirToSetOnFixedUpdate;
        
        private bool SetForceOnFixedUpdate;
        private bool SetPositionOnFixedUpdate;

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
        
        private Collider2D[] _solidObjectsColliders;
        private Collider2D[] _solidOutRadiusObjectsColliders;

        public Collider2D[] NearbyInteractables() {
 
            var _nearbyInteractions = _senses.NearbyInteractables();

            if (_nearbyInteractions == null || _legsCollider == null) {
                return null;
            }
            var potentialInteractions = _nearbyInteractions.Where(potentialFoe => potentialFoe != _legsCollider);
            List<Collider2D> validInteractables = new();

            foreach (var interaction in potentialInteractions) {
            
                var objPosition = interaction.transform.position;
                var dirFromCharacterToFoe = (objPosition - LegsPosition).normalized;

                var blockingObjectsInAttackDirection = Physics2D.Raycast(LegsPosition,
                    dirFromCharacterToFoe,
                    Vector2.Distance(objPosition, LegsPosition), _whatIsSolid);

                if (blockingObjectsInAttackDirection.collider != null) {
                    continue;
                }
            
                validInteractables.Add(interaction);
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

        public Vector3 LegsPosition => _legsCollider.transform.position;
        public float LegsRadius => _legsCollider.radius;
        
        public void SetMinLevel() => UpdateStats(0);

        public void SetMaxLevel() => UpdateStats(_stats.MaxLevel);

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

            _anchor = gameObject.AddComponent<AnchorHandler>();
            _cling.connectedBody = SingleController.GetController<AnchorsController>().GetAnchorRigidbody(_anchor);
            
            return Task.CompletedTask;
        }

        protected override void Clean() {

        }

        /*
        private void DiagnoseInputNavigation(ActionInput actionInput, bool ended) {

            if (actionInput.ActionGroupType != Definitions.ActionTypeGroup.Navigation) { 
                return;
            }

            if (ended) {
                DoMove(0);
                return;
            }

            var movementDir = _logic.InputLogic.GetInputDirection(actionInput.NormalizedDirection);

            switch (movementDir) {
                case Definitions.NavigationType.Right:
                    DoMove(-_stats.RunSpeed);
                    break;

                case Definitions.NavigationType.DownRight:
                    DoMove(-_stats.SprintSpeed);
                    break;

                case Definitions.NavigationType.Left:
                    DoMove(_stats.RunSpeed);
                    break;

                case Definitions.NavigationType.DownLeft:
                    DoMove(_stats.SprintSpeed);
                    break;
                
                default:
                    DoMove(0);
                    break;
            }

            if (movementDir == Definitions.NavigationType.None) {
                return;
            }
            
            // Air-borne movement
            var rigidbodyVelocity = _rigidbody.velocity;
            
            if (_characterState.State == Definitions.CharacterState.Falling && rigidbodyVelocity.y < -1f) {
                switch (movementDir) {
                    
                    case Definitions.NavigationType.Right or Definitions.NavigationType.DownRight
                        when (rigidbodyVelocity.x < _stats.AirborneMaxSpeed):
                        
                        SetForceOnFixedUpdate = true;
                        ForceDirToSetOnFixedUpdate = new Vector2(rigidbodyVelocity.x + _stats.AirborneSpeed, rigidbodyVelocity.y);
                        DoMove(0);
                        
                        break;
                    
                    case Definitions.NavigationType.Left or Definitions.NavigationType.DownLeft
                        when rigidbodyVelocity.x > -_stats.AirborneMaxSpeed:
                        
                        SetForceOnFixedUpdate = true;
                        ForceDirToSetOnFixedUpdate = new Vector2(rigidbodyVelocity.x - _stats.AirborneSpeed, rigidbodyVelocity.y);
                        DoMove(0);

                        break;
                }
                
            }
        }

        private void DiagnoseInputAction(ActionInput actionInput, bool started, bool ended) {

            if (actionInput.ActionGroupType != Definitions.ActionTypeGroup.Action) {
                return;
            }

            switch (actionInput.ActionType) {
                case Definitions.ActionType.Jump:
                    if (!ended && IsGrounded() && !_characterState.IsJumping()) {
                        _characterState.SetJumping(Time.time + .2f);
                        //SetPositionOnFixedUpdate = true;
                        SetForceOnFixedUpdate = true;
                        ForceDirToSetOnFixedUpdate = new Vector2(_rigidbody.velocity.x, _stats.JumpForce);
                    } else if (ended && _rigidbody.velocity.y > 0 && _characterState.ConsideredAsJumpingAndNotFalling()) {
                        
                        if (!SetForceOnFixedUpdate) {
                            SetForceOnFixedUpdate = true;
                            ForceDirToSetOnFixedUpdate = new Vector2(_rigidbody.velocity.x, 0);
                        } else if (SetForceOnFixedUpdate) {
                            SetForceOnFixedUpdate = false;
                        }
                    }
                    break;

                case Definitions.ActionType.Attack:
                    if (_nearbyCharacters == null || _nearbyCharacters.Length < 2) {
                        return;
                    }

                    Collider2D closestFoeColl = null;
                    foreach (var potentialFoe in _nearbyCharacters) {
                        if (potentialFoe == null || potentialFoe == _legsCollider) {
                            continue;
                        }

                        if (closestFoeColl == null || ((potentialFoe.transform.position - _legsCollider.transform.position).magnitude < (closestFoeColl.transform.position - _legsCollider.transform.position).magnitude)) {
                            closestFoeColl = potentialFoe;
                        }
                    }

                    if (closestFoeColl == null || SetForceOnFixedUpdate) {
                        return;
                    }
                    
                    if (started && !SetForceOnFixedUpdate) {
                        //NewFoePositionToSetOnFixedUpdte = closestFoeColl.transform.position;
                        var dir = (NewPositionToSetOnFixedUpdte - _legsCollider.transform.position);
                        Debug.DrawLine(_legsCollider.transform.position, NewPositionToSetOnFixedUpdte, Color.green, 4);
                        ForceDirToSetOnFixedUpdate = dir.normalized * 15;
                        SetForceOnFixedUpdate = true;
                        var foeRigidBody = closestFoeColl.GetComponent<Rigidbody2D>();
                        if (foeRigidBody != null) {
                            foeRigidBody.velocity = ForceDirToSetOnFixedUpdate;
                            Debug.DrawRay(NewPositionToSetOnFixedUpdte, ForceDirToSetOnFixedUpdate, Color.red, 7);
                        }
                    }
                    
                    break;

                case Definitions.ActionType.Block:
                    break;

                case Definitions.ActionType.Grab:
                    break;

                case Definitions.ActionType.Projectile:
                    break;

                case Definitions.ActionType.Rope:
                    break;

                case Definitions.ActionType.Special:
                    break;

                //default:
                    //throw new ArgumentOutOfRangeException();
            }
        }
        */

        private float GetPlayerLegsHeight() => _legs.anchor.y;

        public void DoMove(float speed) {
            if (speed != 0) {
                FaceCharacterTowardsPoint(speed < 0);
            }
            _motor.motorSpeed = speed;
            _legs.motor = _motor;
        }
        
        public void FaceCharacterTowardsPoint(bool facingRight) {
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

            var newHealthPoint = Mathf.Clamp(Stats.Health - damage, 0, Stats.MaxHealth);
            Stats.Health = newHealthPoint;

            if (newHealthPoint != 0) {
                //Debug.Log("HP " + Stats.Health);
            } else {
                //Debug.Log("Dead");
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

        public void SetTargetPosition(Vector2 position) {
            SetNewPosition(position);
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
            
            SetForceOnFixedUpdate = newForceDir != Vector2.zero;
        }

        public void SetNewPosition(Vector2 newPosition) {

            if (newPosition != Vector2.zero) {
                newPosition.y -= GetPlayerLegsHeight();
            }
            NewPositionToSetOnFixedUpdte = newPosition;
        }
        
        private void Update() {

            UpdateCharacterState();
        }

        private void FixedUpdate() {

            
            if (NewPositionToSetOnFixedUpdte != Vector3.zero) {

                SwitchRigidBodyType(RigidbodyType2D.Kinematic, 0, true);
                _transformMover.MoveToPosition(this.transform, NewPositionToSetOnFixedUpdte, 0.15f); // 0.15f is the fastest combat turn
                SwitchRigidBodyType(RigidbodyType2D.Dynamic, 0.15f, true);
                _characterState.SetAnimating(Time.time + (Stats.AttackDurationInMilliseconds * 0.001f));
                NewPositionToSetOnFixedUpdte = Vector3.zero;
                
            } else if (ForceDirToSetOnFixedUpdate != Vector2.zero) {

                SwitchRigidBodyType(RigidbodyType2D.Dynamic, 0, false);
                _transformMover.EndMove();
                _rigidbody.velocity = ForceDirToSetOnFixedUpdate;
                ForceDirToSetOnFixedUpdate = Vector2.zero;
                SetForceOnFixedUpdate = false;
            }
            
            UpdateAnimatorRigRotation();

        }
        
        public void SetProjectileState(bool isActive) {
            
            if (_bodyDamager == null) {
                return;
            }

            _bodyDamager.gameObject.SetActive(isActive);
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
            _rigAnimator.UpdateRigRotation(isAlive, _characterState.ReturnForwardDirByTerrainQuaternion());

            /*var isAlive = Stats.Health > 0;
            var facingRightAngle = _characterState.IsFacingRight() ? 180 : 0;
            var offsetQuaternion = Quaternion.Euler(0f, 0f, isAlive ? QuaterminionOffsetAngle + facingRightAngle : 0);
            var newRotation = Quaternion.Lerp(_rigAnimator.gameObject.transform.rotation,
                _characterState.ReturnForwardDirByTerrainQuaternion() * offsetQuaternion,
                Time.deltaTime * 10f);

            _rigAnimator.gameObject.transform.rotation = newRotation;*/
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
            
            //Debug.DrawRay(hitPoint, collDir * 5, Color.white, 1f);
            //Debug.Log("dir "+ collDir+", go = "+i.name);
            _characterState.DiagnoseState(hitPoint, collDir, outerRadiusCollDir, _rigidbody.velocity);

            var shouldCallStateChange = prevState != _characterState.CurrentState ;// || _characterState.CurrentState == Definitions.CharacterState.Grounded;
            
            if (shouldCallStateChange) {

                OnStateChanged?.Invoke(_characterState);

                if (_characterState.CurrentState == Definitions.CharacterState.Landed) {
                    SetForceOnFixedUpdate = true;
                    ForceDirToSetOnFixedUpdate = new Vector2(_rigidbody.velocity.x, 0);
                    DoMove(0);
                }
            }
        }

        public void SetJumping(float time) => _characterState.SetJumping(time);
        
        public Vector3 IsFreeFalling(float distanceToCheckWhenFreeFalling) {

            if (State.CurrentState is not (Definitions.CharacterState.Jumping or Definitions.CharacterState.Falling or Definitions.CharacterState.Landed) || Velocity.y > 0 || State.IsTouchingAnySurface()) {
                //Debug.LogWarning("Is not free falling because state is "+State.State);
                return Vector3.zero;
            }
            
            var hit = FreeFallRayCast(LegsPosition,  Velocity.normalized, distanceToCheckWhenFreeFalling);
            //Debug.DrawRay(legsPosition, Velocity.normalized, Color.black, 3);
            if (hit.collider != null) {
                //Debug.DrawLine(legsPosition, hit.point, Color.white, 2);
                return hit.point;
            }

            var newAngleToCheck = Velocity;
            newAngleToCheck.y -= 10;
            hit = FreeFallRayCast(LegsPosition, newAngleToCheck , distanceToCheckWhenFreeFalling);
            //Debug.DrawRay(legsPosition, newAngleToCheck, Color.magenta, 3);
            if (hit.collider != null) {
                //Debug.DrawLine(legsPosition, hit.point, Color.white, 2);
                return hit.point;
            }
            
            newAngleToCheck = Velocity;
            newAngleToCheck.y -= 10*2;
            hit = FreeFallRayCast(LegsPosition, newAngleToCheck , distanceToCheckWhenFreeFalling);
            //Debug.DrawRay(legsPosition, newAngleToCheck, Color.cyan, 3);
            if (hit.collider != null) {
                //Debug.DrawLine(legsPosition, hit.point, Color.white, 2);
                return hit.point;
            }
            
            return Vector3.zero;
        }

        private RaycastHit2D FreeFallRayCast(Vector3 startPos, Vector3 direction, float distanceToCheckWhenFreeFalling) {

            return Physics2D.Raycast(startPos, direction, distanceToCheckWhenFreeFalling,_whatIsSolid);
        }

        public void SetNavigationDir(Definitions.NavigationType inputDirection) => State.SetNavigationDir(inputDirection);

        public Definitions.NavigationType GetNavigationDir() => State.NavigationDir;

    }

}