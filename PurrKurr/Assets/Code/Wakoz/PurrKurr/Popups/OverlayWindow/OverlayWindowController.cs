using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.Popups.OverlayWindow {

    [DefaultExecutionOrder(12)]
    public class OverlayWindowController : SingleController {

        [SerializeField] private OverlayWindowView _view;
        private OverlayWindowModel _model;

        private List<OverlayWindowData> _pageData;
        private int _pageIndex = 0;

        public void ShowWindow(List<OverlayWindowData> windowPageData) {

            _pageData = windowPageData;

            _pageIndex = 0;
            var firstPage = windowPageData.FirstOrDefault();
            if (firstPage == null) {
                Debug.LogWarning("No data for window popup");
                HideWindow();
                return;
            }
            ShowSinglePage(firstPage);
        }

        public void ShowWindow(string title = null, string bodyContent = null, Sprite bodyPicture = null, List<GenericButtonData> buttons = null, string pageCount = null) {

            SetModel(title, bodyContent, bodyPicture, buttons, pageCount);
        }

        public void HideWindow() {

            SetModel();
        }

        public void ShowNextPage(bool hideWindowAfterLastPage) {

            if (_model is null) {
                return;
            }

            if (!hideWindowAfterLastPage) {
                return;
            }

            if (_pageIndex >= _pageData.Count - 1) {

                if (hideWindowAfterLastPage) {
                    HideWindow();
                }
                return;
            }

            HandleNextPage();
        }

        private void SetModel(string title = null, string bodyContent = null, Sprite bodyPicture = null, List<GenericButtonData> buttons = null, string pageCount = null) {

            if (_model != null) {
                _model.UpdateDisplay(title, bodyContent, bodyPicture, buttons, pageCount);
            } else {
                _model = new OverlayWindowModel(title, bodyContent, bodyPicture, buttons, pageCount);
                _view.SetModel(_model);
            }
        }

        protected override Task Initialize() {

            _view.InitButtons(true);
            _view.OnConfirmClicked += HandleCloseWindow;
            _view.OnCloseClicked += HandleCloseWindow;
            _view.OnNextPageClicked += HandleNextPage;
            _view.OnPreviousPageClicked += HandlePreviousPage;

            SetModel();

            return Task.CompletedTask;
        }

        protected override void Clean() {

            if (_view == null) {
                return;
            }

            _view.InitButtons(false);
            _view.OnConfirmClicked -= HandleCloseWindow;
            _view.OnCloseClicked -= HandleCloseWindow;
            _view.OnNextPageClicked -= HandleNextPage;
            _view.OnPreviousPageClicked -= HandlePreviousPage;

        }

        private void ShowSinglePage(OverlayWindowData page) {

            ShowWindow(page.Title, page.BodyContent, page.BodyPicture, page.ButtonsRawData, page.PageCount);
        }

        private void HandleCloseWindow() {
            HideWindow();
        }

        private void HandleNextPage() {

            if (_pageIndex >= _pageData.Count - 1) {
                return;
            }

            _pageIndex++;
            ChangePage(_pageIndex);
        }

        private void HandlePreviousPage() {

            if (_pageIndex <= 0) {
                return;
            }

            _pageIndex--;
            ChangePage(_pageIndex);
        }

        private void ChangePage(int newIndex) {

            var page = _pageData[newIndex];
            ShowSinglePage(page);
        }

    }
}