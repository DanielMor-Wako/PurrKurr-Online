using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.Popups.OverlayWindow {

    [DefaultExecutionOrder(12)]
    public class OverlayWindowController : SingleController {

        public event Action onConfirmCallback;
        public event Action onDeclineCallback;
        public event Action onAlternateCallback;

        [SerializeField] private OverlayWindowView _view;
        private OverlayWindowModel _model;

        private List<OverlayWindowData> _pageData;

        public void ShowWindow(List<OverlayWindowData> windowPageData) {

            _pageData = windowPageData;
            var firstPage = windowPageData.FirstOrDefault();
            ShowSinglePage(firstPage);
        }

        private void ShowSinglePage(OverlayWindowData page) {

            ShowWindow(page.Title, page.BodyContent, page.BodyPicture, page.ButtonsRawData);
        }

        public void ShowWindow(string title = null, string bodyContent = null, Sprite bodyPicture = null, List<GenericButtonData> buttons = null) {

            SetModel(title, bodyContent, bodyPicture, buttons);
        }

        public void HideWindow() {

            SetModel();
        }

        private void SetModel(string title = null, string bodyContent = null, Sprite bodyPicture = null, List<GenericButtonData> buttons = null) {

            if (_model != null) {
                _model.UpdateDisplay(title, bodyContent, bodyPicture, buttons);
            } else {
                _model = new OverlayWindowModel(title, bodyContent, bodyPicture, buttons);
                _view.SetModel(_model);
            }
        }

        protected override Task Initialize() {

            _view.InitButtons(true);

            SetModel();

            return Task.CompletedTask;
        }

        protected override void Clean() {

            if (_view == null) {
                return;
            }

            _view.InitButtons(false);
        }

    }
}