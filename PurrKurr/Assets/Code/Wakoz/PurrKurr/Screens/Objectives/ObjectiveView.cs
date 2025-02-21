using Code.Wakoz.PurrKurr.AnimatorBridge;
using Code.Wakoz.PurrKurr.Views;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Wakoz.PurrKurr.Screens.Objectives
{
    public class ObjectiveView : View<ObjectiveModel>
    {
        [Header("Content Description")]
        [SerializeField] private TextMeshProUGUI _contentText;

        [Header("Counter")]
        [SerializeField] private TextMeshProUGUI _counterField;

        [Header("Icon")]
        [SerializeField] private Image _iconImage;
        [SerializeField] private ImageFillerView _iconImageFiller;
        [SerializeField] private RectTransformScalerView _iconImageScaler;

        [Header("MultiState")]
        [SerializeField] private MultiStateView _states;

        [Header("Fader")]
        [SerializeField] private CanvasGroupFaderView _fader;

        public ImageFillerView ImageFiller => _iconImageFiller;
        public RectTransformScalerView ImageScaler => _iconImageScaler;
        public CanvasGroupFaderView Fader => _fader;

        protected override void ModelChanged()
        {
            UpdateView();
        }

        private void UpdateView()
        {
            var data = Model.InterfaceData;

            var isComplete = data.IsComplete();

            _contentText.SetText(data.GetObjectiveDescription());

            var requiredQuantity = data.GetRequiredQuantity();

            _counterField.gameObject.SetActive(requiredQuantity > 1);
            UpdateCounterProgression(data.GetCurrentQuantity(), requiredQuantity);

            UpdateState(Convert.ToInt32(isComplete));
        }

        private void UpdateCounterProgression(int step, int total)
        {
            _counterField.SetText($"{step} / {total}");
        }

        /// <summary>
        /// Sets a new state using a bool parsing. boolean as False => 0 , and True => 1.
        /// </summary>
        /// <param name="newstate"></param>
        private void UpdateState(int newstate)
        {
            if (_states == null)
                return;

            _states.ChangeState(newstate);
        }

    }
}