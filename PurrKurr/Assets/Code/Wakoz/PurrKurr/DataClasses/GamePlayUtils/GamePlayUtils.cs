using Code.Wakoz.PurrKurr.DataClasses.Enums;
using Code.Wakoz.PurrKurr.Views;
using System.Threading.Tasks;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.GamePlayUtils {

    [DefaultExecutionOrder(12)]
    public class GamePlayUtils : SingleController {

        [SerializeField] private MultiStateView _cursor;
        [SerializeField] private MultiStateView _angle;
        [SerializeField] private Transform _trajectory;

        protected override void Clean() { }

        protected override Task Initialize() {
            return Task.CompletedTask;
        }

        public void ActivateUtil(Definitions.ActionType actionType, Vector2 position, Quaternion quaternion, bool hasHitData) => 
            UpdateUtilByActionType(actionType, true, position, quaternion, hasHitData);

        public void DeactivateUtil(Definitions.ActionType actionType) =>
            UpdateUtilByActionType(actionType, false, Vector2.zero, Quaternion.identity);

        private void UpdateUtilByActionType(Definitions.ActionType actionType, bool isActive, Vector2 position, Quaternion quaternion, bool hasHitData = false) {
            
            switch (actionType) {

                case Definitions.ActionType.Rope:
                    UpdateUtil(_cursor?.transform, isActive, position, quaternion);
                    UpdateUtilState(_cursor, hasHitData);
                    break;

                case Definitions.ActionType.Projectile:
                    UpdateUtil(_angle?.transform, isActive, position, quaternion);
                    UpdateUtilState(_angle, hasHitData);
                    break;

                case Definitions.ActionType.Jump:
                    UpdateUtil(_trajectory, isActive, position, quaternion);
                    break;

                default:
                    break;
            }

        }

        private void UpdateUtil(Transform utilTransform, bool isActive, Vector2 position, Quaternion quaternion) {
            
            if (utilTransform == null) {
                return;
            }

            if (isActive) {
                utilTransform.transform.SetPositionAndRotation(position, quaternion);
            }

            utilTransform.gameObject.SetActive(isActive);
        }

        private void UpdateUtilState(MultiStateView utilState, bool isActive) {

            if (utilState == null) {
                return;
            }

            utilState.ChangeState(isActive ? 1 : 0);
        }
    }
}