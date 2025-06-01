using Code.Wakoz.PurrKurr.DataClasses.Characters;
using Code.Wakoz.PurrKurr.Screens.Gameplay_Controller;
using Code.Wakoz.PurrKurr.Screens.MainMenu;
using Code.Wakoz.PurrKurr.Screens.Ui_Controller.Bars;
using Code.Wakoz.PurrKurr.Screens.Ui_Controller.InputDisplay;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.Screens.Ui_Controller
{
    [DefaultExecutionOrder(13)]
    public class UIController : SingleController {

        private UIMenuController _menuController;
        private UiPadsController _padsController;
        private UIBarsController _barsController;

        protected override Task Initialize() {

            _menuController ??= GetController<UIMenuController>() ?? throw new ArgumentNullException("Missing Ui Menu Controller");
            _padsController ??= GetController<UiPadsController>() ?? throw new ArgumentNullException("Missing Ui Pads Controller");
            _barsController ??= GetController<UIBarsController>() ?? throw new ArgumentNullException("Missing Ui Bars Controller");

            return Task.CompletedTask;
        }

        protected override void Clean() {}

        public void InitUiForCharacter(Character2DController hero = null)
        {
            _padsController.Init(hero);
            _barsController.Init(hero);
        }

        public bool TryBindToCharacterController(GameplayController character, InputProcessor inputProcessor) 
        {
            var isPadsInitialized = _padsController.TryBindToCharacter(character);
            var isBarsInitialized = _barsController.TryBindToCharacter(character);

            return isPadsInitialized && isBarsInitialized;
        }

        public void ToggleMenu() {

            if (!Debug.isDebugBuild) {
                GetController<MainMenuController>().ShowMenu(true);
                return;
            }

            _menuController?.Toggle();
        }
    }
}