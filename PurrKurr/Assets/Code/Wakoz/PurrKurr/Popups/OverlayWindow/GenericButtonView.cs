using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Wakoz.PurrKurr.Popups.OverlayWindow {
    public class GenericButtonView : View<GenericButtonModel> {

        public event Action <GenericButtonModel> OnClick;

        [SerializeField] private GenericButtonType _buttonType;
        [SerializeField] private TextMeshProUGUI _text;
        [SerializeField] private Button _button;

        public void ButtonClicked() => OnClick?.Invoke(Model);

        public GenericButtonType GetButtonType() => _buttonType;

        protected override void ModelChanged()
        {
            var hasData = Model.Data != null;

            _text.gameObject.SetActive(hasData);
            _button?.gameObject.SetActive(hasData);

            if (!hasData) {
                return;
            }

            _text?.SetText(Model.Data.ButtonText);
            _button.interactable = Model.IsClickable();
        }

    }
}