using Code.Wakoz.PurrKurr.Screens.Gameplay_Controller.Handlers;
using Code.Wakoz.PurrKurr.Screens.Gameplay_Controller;
using Code.Wakoz.PurrKurr;
using Code.Wakoz.PurrKurr.DataClasses.Characters;
using System;
using Code.Wakoz.PurrKurr.Screens.SceneTransition;
using Code.Wakoz.PurrKurr.DataClasses.Objectives;
using System.Threading.Tasks;
using Code.Wakoz.PurrKurr.Screens.MainMenu;

namespace Code.Core
{
    public class CharacterStateHandler : IBindableHandler
    {
        private GameplayController _gameEvents;
        private Character2DController _character;

        private static int stamStaticSoul = 9;

        public CharacterStateHandler(GameplayController gameEvents) {

            _gameEvents = gameEvents ?? throw new ArgumentNullException(nameof(GameplayController));
        }

        public void Bind() {

            if (_gameEvents == null)
                return;

            _gameEvents.OnNewHero += HandleNewHero;
            _gameEvents.OnHeroDeath += HandleHeroFailed;
        }

        public void Unbind() {

            if (_gameEvents == null)
                return;

            _gameEvents.OnNewHero -= HandleNewHero;
            _gameEvents.OnHeroDeath -= HandleHeroFailed;
        }

        public void Dispose() {

            _gameEvents = null;
        }

        private void HandleNewHero(Character2DController character) {

            _character = character;
        }


        private async void HandleHeroFailed() {
            
            if (_character == null) return;

            var hasActiveChallengeHandler = _gameEvents.Handlers.GetHandler<ObjectivesMissionHandler>().HasActiveMission();
            if (hasActiveChallengeHandler) return;

            _gameEvents.InvokeNewNotification("You lost a soul");

            await Task.Delay(TimeSpan.FromSeconds(2));

            stamStaticSoul--;

            ResetLevelState(() => {

                // todo: ask game mode if the player can be revived
                if (stamStaticSoul <= 0) {
                    SingleController.GetController<MainMenuController>().ShowMenu(true);
                    return;
                }

                var canHeroBeRevived = true;
                if (canHeroBeRevived) {
                    _character.Revive();
                }
            });
        }
        
        private void ResetLevelState(Action actionOnEnd) {
            SingleController.GetController<SceneTransitionController>().PretendLoad(2.5f, $"{stamStaticSoul} Souls left",
                () => {
                    actionOnEnd.Invoke();
                });
        }
    }
}
