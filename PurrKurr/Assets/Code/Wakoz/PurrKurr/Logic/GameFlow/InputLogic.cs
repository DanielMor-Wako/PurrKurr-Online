using Code.Wakoz.PurrKurr.DataClasses.Enums;
using Code.Wakoz.PurrKurr.DataClasses.ScriptableObjectData;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.Logic.GameFlow {
    public sealed class InputLogic : SOData<PlayerInputDataSO>  {
        public InputLogic(string assetName) : base(assetName) { }

        private float _minSwipeDirectionToBeLeftOrRight;
        private float _minSwipeDirectionToBeUpOrDown;
        
        protected override void Init() {

            _minSwipeDirectionToBeLeftOrRight = Data.MinSwipeDirectionToBeLeftOrRight;
            _minSwipeDirectionToBeUpOrDown = Data.MinSwipeDirectionToBeUpOrDown;
        }
        
        public float MinSwipeVelocity => Data.MinSwipeVelocity;
        public float GetSwipeDistanceShort() => Data.MinSwipeDistance;
        public float GetSwipeDistanceMedium() => Data.MediumSwipeDistance;
        public float GetSwipeDistanceLong() => Data.LongSwipeDistance;

        public float GetSwipeDistance(Definitions.SwipeDistanceType swipeDistance) {
            return swipeDistance switch {
                Definitions.SwipeDistanceType.Short => GetSwipeDistanceShort(),
                Definitions.SwipeDistanceType.Medium => GetSwipeDistanceMedium(),
                _ => GetSwipeDistanceLong()
            };
        }

        public int GetNavigationDirAsFacingRightInt(Definitions.NavigationType navigationDirection) =>
            IsNavigationDirValidAsLeft(navigationDirection) ? -1 
            : IsNavigationDirValidAsRight(navigationDirection) ? 1 
            : 0;

        public bool IsNavigationDirValidAsDown(Definitions.NavigationType navigationDirection) =>
            navigationDirection is Definitions.NavigationType.Down or Definitions.NavigationType.DownLeft or Definitions.NavigationType.DownRight;
        
        public bool IsNavigationDirValidAsUp(Definitions.NavigationType navigationDirection) =>
            navigationDirection is Definitions.NavigationType.Up or Definitions.NavigationType.UpLeft or Definitions.NavigationType.UpRight;

        public bool IsNavigationDirValidAsRight(Definitions.NavigationType navigationDirection) =>
            navigationDirection is Definitions.NavigationType.UpRight or Definitions.NavigationType.Right or Definitions.NavigationType.DownRight;

        public bool IsNavigationDirValidAsLeft(Definitions.NavigationType navigationDirection) =>
            navigationDirection is Definitions.NavigationType.UpLeft or Definitions.NavigationType.Left or Definitions.NavigationType.DownLeft;

        public bool IsInputDirectionValidAsDown(float inputValue) =>
            inputValue < 0 && inputValue < -_minSwipeDirectionToBeUpOrDown;
        public bool IsInputDirectionValidAsUp(float inputValue) =>
            inputValue > 0 && inputValue > _minSwipeDirectionToBeUpOrDown;
        public bool IsInputDirectionValidAsRight(float inputValue) =>
            inputValue > 0 && inputValue > _minSwipeDirectionToBeLeftOrRight;
        public bool IsInputDirectionValidAsLeft(float inputValue) =>
            inputValue < 0 && inputValue < -_minSwipeDirectionToBeLeftOrRight;

        public Definitions.NavigationType GetInputDirection(Vector2 inputValue) {
            
            var hasLeft = IsInputDirectionValidAsLeft(inputValue.x);
            var hasRight = !hasLeft && IsInputDirectionValidAsRight(inputValue.x);
            var hasDown = IsInputDirectionValidAsDown(inputValue.y);
            var hasUp = !hasDown && IsInputDirectionValidAsUp(inputValue.y);

            switch (hasLeft || hasRight || hasUp || hasDown) {
                
                case var _ when hasLeft: {
                    if (hasUp) {
                        return Definitions.NavigationType.UpLeft;
                    } else if (hasDown) {
                        return Definitions.NavigationType.DownLeft;
                    } else {
                        return Definitions.NavigationType.Left;
                    }
                }
                
                case var _ when hasRight: {
                    if (hasUp) {
                        return Definitions.NavigationType.UpRight;
                    } else if (hasDown) {
                        return Definitions.NavigationType.DownRight;
                    } else {
                        return Definitions.NavigationType.Right;
                    }
                }
                    
                default: {
                    if (hasUp) {
                        return Definitions.NavigationType.Up;
                    } else if (hasDown) {
                        return Definitions.NavigationType.Down;
                    }
                    break;
                }
            }

            return Definitions.NavigationType.None;
        }

        

    }
}