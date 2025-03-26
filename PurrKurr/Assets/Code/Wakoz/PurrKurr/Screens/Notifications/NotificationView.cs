using Code.Wakoz.PurrKurr.Views;
using TMPro;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.Screens.Notifications
{
    public class NotificationView : View
    {
        [SerializeField] private TextMeshProUGUI _text;
        [SerializeField] private CanvasGroupFaderView _fader;
        [SerializeField] private ImageColorChangerView _color;
        [SerializeField] private RectTransformScalerView _scaler;

        public CanvasGroupFaderView Fader => _fader;
        public ImageColorChangerView Color => _color;
        public RectTransformScalerView ImageScaler => _scaler;

        public void SetText(string text) => _text.SetText(text);

        protected override void ModelChanged() {}
        
    }
}