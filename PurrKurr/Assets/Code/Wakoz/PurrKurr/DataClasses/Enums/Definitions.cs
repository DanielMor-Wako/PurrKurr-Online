namespace Code.Wakoz.PurrKurr.DataClasses.Enums
{
    public class Definitions
    {

        public enum ActionType : byte
        {
            Empty = 0,
            Movement = 1,
            Jump = 2,
            Attack = 3, // Attack light
            Block = 4,
            Grab = 5,
            Projectile = 6,
            Rope = 7,
            Special = 8
        }

        public enum ActionTypeGroup : byte
        {
            Navigation = 0,
            Action = 1
        }

        public enum PadType : byte
        {
            Fixed = 0,
            Flexible = 1
        }

        public enum SwipeDistanceType : byte
        {
            Short = 0,
            Medium = 1,
            Long = 2
        }

        public enum NavigationType : byte
        {
            None = 0,
            Up = 1,
            Down = 2,
            Left = 3,
            Right = 4,
            UpRight = 5,
            UpLeft = 6,
            DownRight = 7,
            DownLeft = 8
        }

        public enum CharacterAbility : byte
        {
            None = 0,
            Crouch = 1,
            StandUp = 2,
            Dodge = 3,
            AimJump = 4,
            WallCling = 5,
            WallRun = 6, // used in combo with colliders on "Traversable" layer
            RopeCling = 7,
            AirGlide = 8,
        }

        public enum AttackAbility : byte
        {
            LightAttackAlsoDefaultAttack = 0,
            MediumAttack = 1, // attack with up key
            HeavyAttack = 2, // attack with down key
            RollAttack = 3, // attack while running
            AerialAttack = 4, // attack while jumping or falling
            LightGrabAlsoDefaultGrab = 5, // anti-block and anti-dodge
            MediumGrab = 6, // anti-attack (Cannot be stunned but can be dodged)
            HeavyGrab = 7, // anti-grabbers (massive damage to other grabbers)
            AerialGrab = 8, // grab while jumping or falling , and release grabbed foe in groundsmash
            LightBlock = 9, // absorbs damage but can be grabbed
            None = 255
        }

        public enum AttackProperty : byte
        {
            StunResist = 0,
            StunOnHit = 1,
            StunOnBlock = 2,
            PenetrateDodge = 3,
            PushBackOnHit = 4,
            PushBackOnBlock = 5,
            PushUpOnHit = 6,
            PushUpOnBlock = 7,
            PushDiagonalOnHit = 8,
            PushDiagonalOnBlock = 9,
            StunOnGrabber = 10,
            MultiTargetOnSurfaceHit = 11,
            GrabToGroundSmash = 12,
            GrabAndPickUp = 13,
            PushDownOnHit = 14,
            PushDownOnBlock = 15,
            ApplyPoisonOnHit = 16,
            ApplyPoisonOnBlock = 17,
            ApplyBleedOnHit = 18,
            ApplyBleedOnBlock = 19,
            CriticalChanceOnHit = 20,
            StunAttacker = 21
        }

        public enum CharacterBuff : byte
        {
            PhysicalResist,
            BlockResist,
            StunResist,
            BleedResist,
            PoisonResist,
            PushbackResist,
            CriticalResist,
        }

        // stats displayed on the ui
        public enum CharacterStatType : byte
        { 
            Health = 0,
            Pawer = 1,
            Supurr = 2,
        }

        public enum Character2DFacingRightType : byte
        {
            Auto = 0,
            FixedLeft = 1,
            FixedRight = 2,
        }

        // InteractableState?
        public enum ObjectState : byte
        {
            Alive,
            UninterruptibleAnimation,
            Dead,
            Grounded,
            Crouching,
            StandingUp,
            Attacking,
            Grabbing,
            Grabbed,
            Blocking,
            InterruptibleAnimation,
            Running,
            Jumping,
            Falling,
            AerialJumping,
            WallClinging,
            WallClimbing,
            TraversalRunning,
            TraversalClinging,
            RopeClinging,
            RopeClimbing,
            AirGliding,
            AimingRope,
            AimingProjectile,
            AimingJump,
            Stunned,
            Landed,
            Dodging,
            Crawling
        }

        public enum Effect2DType
        {
            None = -1,
            GainHp = 0,
            GainTimer = 1,
            GainPawer = 2,
            LosePawer = 3,
            GainSupurr = 4,
            LoseSupurr = 5,
            TrailOnGround = 6,
            TrailInAir = 7,
            TrailInWater = 8,
            BlockActive = 9,
            DodgeActive = 10,
            ImpactNoDamage = 11,
            ImpactLight = 12,
            ImpactMed = 13,
            ImpactHeavy = 14,
            ImpactFinalBlow = 15,
            ImpactCritical = 16,
            ImpactOnBlock = 17,
            ReflectDamage = 18,
            JumpActive = 19,
            ChargingJump = 20,
            DustCloud = 21,
            Landed = 22,
            Resisted = 23,
            Bleeding = 24,
            FireBurn = 28,
            Poisoned = 25,
            IceFreeze = 26,
            Electrified = 27,
            Stunned = 28,
            DetectedInteractible = 29,
        }

        public enum CollectableTypes : byte
        {
            RealMoney = 0,
            Gold = 1,
            Bones = 2,
            Essence = 3,
            Exp = 3,
            Health = 4,
            Life = 5,
            Pawer = 6,
            Suppur = 7,
        }

        public enum GameMode : byte
        {
            Tutorial = 0,
            SinglePlayer = 1,
            OnlineMission = 2,
            PvP = 3,
            TeamRaid = 4
        }

        public enum NonePlayableCharacterType : byte
        {
            Mentor,
            Nemesis,
            Merchandiser, // fly, garbage beetle
            Mapper, // mouse, bird, moll
            Mastery, //Hedgehog 
            QuestGiver,
            HintGiver,
        }

        public enum SenseType : byte
        {
            Sight,
            Sound,
            Smell
        }

        public enum FoodType : byte
        {
            Carnivore,
            Omnivore
        }

        public enum MovementType : byte
        {
            Ground = 0,
            Air = 1,
            Water = 2,
        }

        public enum PlayableCharacterType : byte
        {
            Cat,
            Mouse,
            Rat,
            Frog,
            MamaBird,
            BirdBaby,
            Rabbit,
            Monkey,
            Bear,
            Scorpion,
            Spider,
            Worm,
            Bat,
            Snake,
            Eagle,
            Hedgehog
        }

        public enum ObjectiveTypes : byte
        {
            ReachLocation, //GetGold 60%
            DefeatFoes,  //GetBones 80%, Hidden for explorers 20%,
            Collect,
            ReachLevel,
            UnlockAbility,
            UseAbility,
            ReachAbilityLevel,
            UnlockCharacter,
        }

        public enum AchievementTypes : byte
        {
            ExploreLocation, //Hidden treasure for explorers : Extra Gold 20%, Extra Bones 20%
            DefeatEveryone,  //Hidden bonus for action thrillers : Extra Gold 20%, Extra Bones 20%
            UnlockCoreAbilities, // attack, block, grab
            BreedNewCharacter
        }

        public enum AgentGoal : byte
        {
            Explore = 0,
            Protect = 1,
            Fight = 2,
            Run = 3
        }

        public enum GoalCondition : byte
        {
            IsHpPercentAboveRange01 = 0,
            HasNearbyAttackers = 1,
            HasNearbyConsumeables = 2,
        }

        public enum CharacterDisplayableStat : byte
        {
            Health, Timer, Pawer, Supurr
        }
    }

}