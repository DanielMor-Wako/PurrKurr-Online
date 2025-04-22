using Code.Wakoz.PurrKurr.DataClasses.Characters;
using Code.Wakoz.PurrKurr.DataClasses.Enums;
using Code.Wakoz.PurrKurr.DataClasses.GameCore.CollectableItems;
using Code.Wakoz.PurrKurr.DataClasses.GameCore.Detection;
using Code.Wakoz.PurrKurr.Screens.Gameplay_Controller;
using Code.Wakoz.PurrKurr.Screens.Gameplay_Controller.Handlers;

namespace Code.Wakoz.PurrKurr.DataClasses.Objectives
{
    public class CharacterStatsHandler : IBindableHandler
    {
        private GameplayController _gameEvents;
        private Character2DController _character;

        public CharacterStatsHandler(GameplayController gameEvents) {

            _gameEvents = gameEvents;
        }

        public void Bind() {

            if (_gameEvents == null)
                return;

            _gameEvents.OnNewHero += HandleNewHero;
            _gameEvents.OnHeroEnterDetectionZone += HandleHeroEnterDetectionZone;
        }

        public void Unbind() {

            if (_gameEvents == null)
                return;

            _gameEvents.OnNewHero -= HandleNewHero;
            _gameEvents.OnHeroEnterDetectionZone -= HandleHeroEnterDetectionZone;
        }

        public void Dispose() {

            _gameEvents = null;
        }

        private void HandleNewHero(Character2DController character) {

            _character = character;
        }

        private void HandleHeroEnterDetectionZone(DetectionZoneTrigger zone) {

            if (_character == null) {
                return;
            }

            if (zone is not CollectableItemController) {
                return;
            }

            var collectableItem = zone as CollectableItemController;
            var (id, quantity) = collectableItem.GetData();
            var ability = GetAbility(id);
            if (ability == Definitions.ActionType.Empty) {
                return;
            }

            if (_character.Stats.TryUnlockAbility(ability)) {
                _character.RefreshStats();
            }
        }

        private Definitions.ActionType GetAbility(string id) 
            => id switch {

            "AbilityMovement" => Definitions.ActionType.Movement,
            "AbilityJump" => Definitions.ActionType.Jump,
            "AbilityAttack" => Definitions.ActionType.Attack,
            "AbilityBlock" => Definitions.ActionType.Block,
            "AbilityGrab" => Definitions.ActionType.Grab,
            "AbilityProjectile" => Definitions.ActionType.Projectile,
            "AbilityRope" => Definitions.ActionType.Rope,
            "AbilitySpecial" => Definitions.ActionType.Special,
            _ => Definitions.ActionType.Empty
        };
    }
}