namespace Code.Wakoz.PurrKurr.DataClasses.Enums {
    public class Definitions {

        public enum ActionType {
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
        
        public enum ActionTypeGroup {
            Navigation = 0,
            Action = 1
        }

        public enum PadType {
            Fixed = 0,
            Flexible = 1
        }
        
        public enum SwipeDistanceType {
            Short = 0,
            Medium = 1,
            Long = 2
        }
        
        public enum NavigationType {
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
        
        public enum CharacterAbility {
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
        
        public enum AttackAbility {
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
        }

        public enum AttackProperty {
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
            ApplyPoisonOnHit = 14,
            ApplyPoisonOnBlock = 15,
            ApplyBleedOnHit = 16,
            ApplyBleedOnBlock = 17,
            CriticalChanceOnHit = 18,
        }
        
        public enum CharacterBuff {
            PhysicalResist,
            BlockResist,
            StunResist,
            BleedResist,
            PoisonResist,
            PushbackResist,
            CriticalResist,
        }
        public enum CharacterStatType { // stats that are displayed on the ui
            Health = 0,
            Pawer = 1,
            Supurr = 2,
        }

        public enum Character2DFacingRightType {
            Auto = 0,
            FixedLeft = 1,
            FixedRight = 2,
        }

        public enum CharacterState {
            Spawned,
            UninterruptibleAnimation,
            Dead,
            Grounded,
            Crouching,
            StandingUp,
            Attacking,
            Grabbing,
            Grabbed,
            Blocking,
            Dodging,
            Running, // bodySlam DashAttack
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
            Stunned,
            Landed,
        }

        public enum CollectableTypes {
            RealMoney = 0,
            Gold = 1,
            Bones = 2,
            Exp = 3,
            Health = 4,
            Life = 5,
            Stamina = 6,
            Suppur = 7,
        }

        public enum GameMode {
            Tutorial = 0,
            SinglePlayer = 1,
            OnlineMission = 2,
            PvP = 3,
            TeamRaid = 4
        }
        
        public enum NonePlayableCharacterType {
            Mentor,
            Nemesis,
            Merchandiser, // fly, garbage beetle
            Mapper, // mouse, bird, moll
            Mastery, //Hedgehog 
            QuestGiver,
            HintGiver,
        }
        
        public enum SenseType {
            Sight,
            Sound,
            Smell
        }
        
        public enum FoodType {
            Carnivore,
            Omnivore
        }
        
        public enum MovementType {
            Ground = 0,
            Air = 1,
            Water = 2,
        }
        
        public enum PlayableCharacterType {
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

        public enum ObjectiveTypes {
            ReachLocation, //GetGold 60%
            DefeatFoes,  //GetBones 80%, Hidden for explorers 20%,
            Collect,
            ReachLevel,
            UnlockAbility,
            UseAbility,
            ReachAbilityLevel,
            UnlockCharacter,
        }
        
        public enum AchievementTypes {
            ExploreLocation, //Hidden treasure for explorers : Extra Gold 20%, Extra Bones 20%
            DefeatEveryone,  //Hidden bonus for action thrillers : Extra Gold 20%, Extra Bones 20%
            UnlockCoreAbilities, // attack, block, grab
            BreedNewCharacter
        }
        
        public enum AiGoals {
            Explore,
            Protect,
            Fight
        }

        public enum CharacterDisplayableStat {
            Health, Timer, Pawer, Supurr
        }
    }

}