using UnityEngine;

namespace Code.Wakoz.PurrKurr.Screens.Ui_Controller.Bars
{
    public class UICurrenciesView : View//<UICurrenciesModel>
    {

        [SerializeField] private CurrencyItemView[] _currencies;

        protected override void ModelChanged() { }

        public void UpdateView() {

            for (var i = 0; i < _currencies.Length; i++) {
                var currencyView = _currencies[i];
                currencyView.UpdateAmount(0);
            }
        }

    }
}
