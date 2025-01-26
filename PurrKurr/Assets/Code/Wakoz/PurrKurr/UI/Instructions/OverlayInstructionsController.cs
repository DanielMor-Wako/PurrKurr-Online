using Code.Wakoz.PurrKurr.DataClasses.GameCore.Detection;
using Code.Wakoz.PurrKurr.DataClasses.GameCore.OverlayWindowTrigger;
using Code.Wakoz.PurrKurr.Screens.Gameplay_Controller;
using System.Threading.Tasks;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.UI.Instructions
{

    [DefaultExecutionOrder(15)]
    public sealed class OverlayInstructionsController : SingleController
    {
        [SerializeField] private OverlayInstructionsView _view;

        private OverlayInstructionsModel _model;

        protected override void Clean()
        {
            Unbind();

            DeactivateAllInstructions();
        }

        protected override Task Initialize()
        {
            Bind();

            _model = new OverlayInstructionsModel();
            _view.SetModel(_model);

            return Task.CompletedTask;
        }

        private void Unbind()
        {
            var gameplayEvents = GetController<GameplayController>();
            if (gameplayEvents != null)
            {
                gameplayEvents.OnHeroEnterDetectionZone -= HandleDetectionEnter;
                gameplayEvents.OnHeroExitDetectionZone -= HandleDetectionExit;
                //gameplayEvents.OnHeroConditionCheck += OnGameEventConditionCheck;
                gameplayEvents.OnHeroConditionMet -= OnGameEventConditionMet;
            }
        }

        private void Bind()
        {
            var gameplayEvents = GetController<GameplayController>();
            if (gameplayEvents != null)
            {
                gameplayEvents.OnHeroEnterDetectionZone += HandleDetectionEnter;
                gameplayEvents.OnHeroExitDetectionZone += HandleDetectionExit;
                //gameplayEvents.OnHeroConditionCheck += OnGameEventConditionCheck;
                gameplayEvents.OnHeroConditionMet += OnGameEventConditionMet;
            }
        }

        private void HandleDetectionEnter(DetectionZoneTrigger zone)
        {
            if (zone is not OverlayWindowTrigger)
                return;

            StartInstructionAnimation(zone as OverlayWindowTrigger);
        }

        private void StartInstructionAnimation(OverlayWindowTrigger windowTrigger, int pageIndex = 0)
        {
            if (windowTrigger == null)
                return;

            var clickAnimationData = windowTrigger.GetAnimationData(pageIndex);
            if (clickAnimationData == null)
                return;

            for (int i = 0; i < clickAnimationData.Count; i++)
            {
                var animation = clickAnimationData[i];
                if (animation == null)
                    continue;

                var target = animation?.CursorTarget;
                if (target == null)
                    continue;

                Debug.Log($"Starting animation of target {target.name} - Angle: {animation.SwipeAngle} -> HoldSwipe: {animation.IsHoldSwipe}");
                ActivateInstruction(new OverlayInstructionModel(animation));
            }
        }

        private void OnGameEventConditionMet(DetectionZoneTrigger zone)
        {
            DeactivateAllInstructions();
        }

        private void HandleDetectionExit(DetectionZoneTrigger zone)
        {
            DeactivateAllInstructions();
        }

        private void ActivateInstruction(OverlayInstructionModel instruction)
        {
            _model.StartAnimation(instruction);
        }

        private void DeactivateAllInstructions()
        {
            _model.StopAnimations();
        }

    }
}