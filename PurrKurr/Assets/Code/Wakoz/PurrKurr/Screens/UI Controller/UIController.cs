﻿using Code.Wakoz.PurrKurr.DataClasses.Characters;
using Code.Wakoz.PurrKurr.Screens.Gameplay_Controller;
using Code.Wakoz.PurrKurr.Screens.Ui_Controller.Bars;
using Code.Wakoz.PurrKurr.Screens.Ui_Controller.InputDisplay;
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

            _menuController ??= SingleController.GetController<UIMenuController>();
            _padsController ??= SingleController.GetController<UiPadsController>();
            _barsController ??= SingleController.GetController<UIBarsController>();

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

        public void ToggleMenu() => _menuController?.Toggle();
    }
}