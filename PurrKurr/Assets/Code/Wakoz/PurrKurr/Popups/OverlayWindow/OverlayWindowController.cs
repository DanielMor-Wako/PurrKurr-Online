using System;
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

            AddPageNavigationButtons(ref _pageData);
            _pageIndex = 0;
            var firstPage = windowPageData.FirstOrDefault();
            ShowSinglePage(firstPage);
        }

        private void AddPageNavigationButtons(ref List<OverlayWindowData> pageData) {

            var pageCount = _pageData.Count;

            if (_pageData == null || pageCount < 1) {
                return;
            }


            for (int i = 0; i < pageCount; i++) {

                var page = _pageData[i];
                page.PageCount = $"{i + 1} / {pageCount}";

                if (i < pageCount - 1) {
                    page.ButtonsRawData.Add(new GenericButtonData(GenericButtonType.Next, "->"));
                }

                if (i > 0) {
                    page.ButtonsRawData.Add(new GenericButtonData(GenericButtonType.Back, "<-"));
                }

            }

        }

        public void ShowWindow(string title = null, string bodyContent = null, Sprite bodyPicture = null, List<GenericButtonData> buttons = null, string pageCount = null) {

            SetModel(title, bodyContent, bodyPicture, buttons, pageCount);
        }

        public void HideWindow() {

            SetModel();
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
            _view.OnNextPageClicked -= HandleNextPage;
            _view.OnPreviousPageClicked -= HandlePreviousPage;

        }

        private void ShowSinglePage(OverlayWindowData page) {

            ShowWindow(page.Title, page.BodyContent, page.BodyPicture, page.ButtonsRawData, page.PageCount);
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