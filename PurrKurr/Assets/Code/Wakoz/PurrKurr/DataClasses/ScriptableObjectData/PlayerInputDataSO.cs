using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.ScriptableObjectData {

    [CreateAssetMenu(fileName = "PlayerInputData", menuName = "Data/Player Input")]
    public class PlayerInputDataSO : ScriptableObject {

        [Header("General Touch Settings")]

        [Tooltip("the minimum distance of a swipe")]
        [SerializeField] private float _minSwipeDistance = 70f;

        [Tooltip("the minimum distance of a medium swipe")]
        [SerializeField] private float _mediumSwipeDistance = 140f;

        [Tooltip("the minimum distance of a long swipe")]
        [SerializeField] private float _longSwipeDistance = 180;
        
        [Header("Movement Settings")]
        [Tooltip("the direction of the swipe, uses the value to determine left and right")]
        [Range(0,1)]
        [SerializeField] private float _minSwipeDirectionToBeLeftOrRight = 0.45f;
        [Tooltip("the direction of the swipe, uses the value to determine up and down")]
        [Range(0,1)]
        [SerializeField] private float _minSwipeDirectionToBeUpOrDown = 0.45f;
        [Tooltip("the minimum velocity of a swipe")]
        [SerializeField] private float _minSwipeVelocity = 0;


        public float MinSwipeVelocity => _minSwipeVelocity;
        public float MinSwipeDistance => _minSwipeDistance;
        public float MediumSwipeDistance => _mediumSwipeDistance;
        public float LongSwipeDistance => _longSwipeDistance;
        public float MinSwipeDirectionToBeLeftOrRight => _minSwipeDirectionToBeLeftOrRight;
        public float MinSwipeDirectionToBeUpOrDown => _minSwipeDirectionToBeUpOrDown;

    }

}