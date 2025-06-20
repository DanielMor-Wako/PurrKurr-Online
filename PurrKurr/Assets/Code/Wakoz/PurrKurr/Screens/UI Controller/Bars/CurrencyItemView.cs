using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Wakoz.PurrKurr.Screens.Ui_Controller.Bars
{
    public class CurrencyItemView : View//<CurrencyModel>
    {

        [SerializeField] private Transform _container;
        [SerializeField] private TextMeshProUGUI _text;
        [SerializeField] private Image _image;

        public void UpdateAmount(int amount) {

            _container = _container ?? throw new ArgumentNullException("Missing _container ref");
            _text = _text ?? throw new ArgumentNullException("Missing _healthText ref");

            _container.gameObject.SetActive(amount > 0);
            if (amount <= 0) return;

            var amountText = amount.ToString();
            if (_text.text == amountText) {
                return;
            }

            _text.SetText(amountText);
        }

        public void UpdateSprite(Sprite newSprite) {

            _image = _image ?? throw new ArgumentNullException("Missing _timerFiller ref");

            if (_image.sprite != newSprite) {
                _image.sprite = newSprite;
            }
        }

        protected override void ModelChanged() { }

    }
}
