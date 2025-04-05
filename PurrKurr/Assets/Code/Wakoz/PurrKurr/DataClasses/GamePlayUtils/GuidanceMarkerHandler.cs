using Code.Wakoz.PurrKurr.DataClasses.GameCore.CollectableItems;
using Code.Wakoz.PurrKurr.Screens.Gameplay_Controller;
using Code.Wakoz.PurrKurr.Screens.Gameplay_Controller.Handlers;

namespace Code.Wakoz.PurrKurr.DataClasses.GamePlayUtils
{
    public class GuidanceMarkerHandler : IBindableHandler
    {
        private GameplayController _gameEvents;
        private UiGuidanceController _controller;

        public GuidanceMarkerHandler(GameplayController gameEvents, UiGuidanceController controller) {
            _gameEvents = gameEvents;
            _controller = controller;
        }

        public void Bind() {

            if (_gameEvents == null)
                return;

            _gameEvents.OnHideGuidanceMarker += HideMarker;
            _gameEvents.OnShowGuidanceMarker += ShowMarker;
        }

        public void Unbind() {

            if (_gameEvents == null)
                return;

            _gameEvents.OnHideGuidanceMarker -= HideMarker;
            _gameEvents.OnShowGuidanceMarker -= ShowMarker;
        }

        public void Dispose() {

            _gameEvents = null;
        }

        private void HideMarker() 
            => _controller.DeactivateMarker();

        private void ShowMarker(ObjectiveMarker marker) 
            => _controller.ActivateMarker(marker);
    }
}