using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Wakoz.PurrKurr.Popups.OverlayWindow {
    public class OverlayWindowView : View<OverlayWindowModel> {

        public event Action OnConfirmClicked;
        public event Action OnCloseClicked;
        public event Action OnNextPageClicked;
        public event Action OnPreviousPageClicked;

        [Header("Container")]
        [SerializeField] private GameObject container;

        [Header("Header")]
        [SerializeField] private Transform headerArea;
        [SerializeField] private TextMeshProUGUI titleField;

        [Header("Content")]
        [SerializeField] private Transform contentArea;
        [SerializeField] private TextMeshProUGUI contentText;
        [Space()]
        [SerializeField] private Transform iconContainer;
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI iconText;

        [Header("Footer")]
        [SerializeField] private Transform footerArea;
        [SerializeField] private List<GenericButtonView> _buttonViews;
        [SerializeField] private GameObject _pageArea;
        [SerializeField] private TextMeshProUGUI _pageCountText;

        private Dictionary<GenericButtonType, GenericButtonModel> _buttonModels = new();

        private void UpdateContainerView(bool isActive) {
            container.SetActive(isActive);
        }

        private void Close() {

            UpdateContainerView(false);
        }

        public void CloseWindow() => Close();

        public void InitButtons(bool initEventInsteadOfClear) {
            
            if (_buttonViews == null || _buttonViews.Count < 1) {
                return;
            }

            foreach (var button in _buttonViews) {

                if (button == null) {
                    continue;
                }

                
                if (initEventInsteadOfClear) {

                    var buttonType = button.GetButtonType();
                    var _mode = new GenericButtonModel(new GenericButtonData(buttonType, null));
                    button.SetModel(_mode);
                    
                    _buttonModels.Add(buttonType, _mode);

                    button.OnClick += OnButtonClicked;
                } else {
                    button.OnClick -= OnButtonClicked;
                }
            }
        }

        private void OnButtonClicked(GenericButtonModel model) {

            var buttonType = model.Data.ButtonType;

            Debug.Log($"{buttonType} was pressed");

            if (TryInvokeNextOrBackButton(buttonType)) {
                return;
            }

            if (Model != null && Model.ButtonsRawData != null) {

                foreach (var button in Model.ButtonsRawData) {

                    if (button == null || button.ButtonType != model.Data.ButtonType) {
                        continue;
                    }

                    try {
                        button.ClickedAction?.Invoke();
                    }
                    finally {
                    }
                }
            }

            if (buttonType is GenericButtonType.Close) {
                OnCloseClicked?.Invoke();
            }

            if (buttonType is GenericButtonType.Confirm) {
                OnConfirmClicked?.Invoke();
            }

        }

        private bool TryInvokeNextOrBackButton(GenericButtonType type) {
            
            if (type is GenericButtonType.Next) {

                OnNextPageClicked?.Invoke();
                return true;

            } else if (type is GenericButtonType.Back) {

                OnPreviousPageClicked?.Invoke();
                return true;

            }

            return false;

        }

        protected override void ModelChanged() {

            UpdateView();
        }

        public void UpdateView() {

            if (!Model.IsActiveWindow()) {
                CloseWindow();
            }

            UpdateTitleView();

            UpdateBodyView();

            UpdateFooter();

            UpdateContainerView(true);
        }

        private void UpdateFooter() {

            var hasFooter = Model.HasFooter;
            footerArea.gameObject.SetActive(hasFooter);

            UpdatePageCountText();

            if (!hasFooter) {
                return;
            }

            UpdateButtonsView();

        }

        private void UpdateButtonsView() {

            var buttonData = Model.ButtonsRawData;

            if (_buttonModels == null) {
                return;
            }

            foreach (var buttonEntry in _buttonModels) {

                var buttonType = buttonEntry.Key;
                var button = buttonEntry.Value;

                if (_buttonModels.TryGetValue(buttonType, out var buttonRawData)) {
                    var data = GetButtonDataByType(buttonData, buttonType);
                    UpdateButton(button, data);
                }
            }
        }

        private void UpdatePageCountText() {

            var hasPageCount = !string.IsNullOrEmpty(Model.PageCount);

            _pageArea.SetActive(hasPageCount);

            if (!hasPageCount) {
                return;
            }

            _pageCountText.SetText(Model.PageCount);
        }

        private static GenericButtonData GetButtonDataByType(List<GenericButtonData> buttonData, GenericButtonType typeTypeMatch) {

            foreach (var data in buttonData) {

                if (data.ButtonType != typeTypeMatch) {
                    continue;
                }

                return data;
            }

            return null;
        }

        private void UpdateButton(GenericButtonModel button, GenericButtonData data) {

            button.UpdateData(data);
        }

        private void UpdateBodyView() {

            var hasBody = Model.HasBody;
            contentArea.gameObject.SetActive(hasBody);

            if (!hasBody) {
                return;
            }

            if (Model.HasBodyContent) {
                contentText.gameObject.SetActive(true);
                contentText.text = Model.BodyContent;
            } else {
                contentText.gameObject.SetActive(false);
            }

            if (Model.HasBodyPicture) {
                iconContainer.gameObject.SetActive(true);
                iconImage.sprite = Model.BodyPicture;
            } else {
                iconContainer.gameObject.SetActive(false);
            }

            //iconText.text = hasBody && Model.HasBodyPicture ? Model.BodyContent : "";
        }

        private void UpdateTitleView() {

            headerArea.gameObject.SetActive(Model.HasTitle);
            titleField.text = Model.HasTitle ? Model.Title : "";
        }

    }
}