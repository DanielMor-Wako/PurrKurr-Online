using Code.Wakoz.PurrKurr.DataClasses.GameCore.Detection;
using Code.Wakoz.PurrKurr.Screens.Gameplay_Controller;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.Popups.OverlayWindow
{

    [DefaultExecutionOrder(15)]
    public class OverlayWindowController : SingleController
    {
        [SerializeField] private OverlayWindowView _view;
        private OverlayWindowModel _model;

        private List<OverlayWindowData> _pageData;
        private int _pageIndex = 0;

        private Queue<List<OverlayWindowData>> _WindowQueue = new();

        private void ShowWindow(List<OverlayWindowData> windowPageData) {
            _pageData = windowPageData;

            _pageIndex = 0;
            var firstPage = windowPageData.FirstOrDefault();
            if (firstPage == null) {
                return;
            }
            ShowSinglePage(firstPage);
        }

        private void ShowWindow(string title = null, string bodyContent = null, Sprite bodyPicture = null, List<GenericButtonData> buttons = null, string pageCount = null) {
            SetModel(title, bodyContent, bodyPicture, buttons, pageCount);
        }

        private void HideWindow() {
            SetModel();
        }

        private void ShowNextPage(bool hideWindowAfterLastPage) {
            if (_model is null || !hideWindowAfterLastPage)
                return;

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
            BindGameEvents();
            BindViewEvents();

            SetModel();

            return Task.CompletedTask;
        }

        protected override void Clean() {
            UnbindGameEvents();
            UnbindViewEvents();
        }

        private void BindViewEvents() {
            _view.InitButtons(true);

            _view.OnConfirmClicked += HandleCloseWindow;
            _view.OnCloseClicked += HandleCloseWindow;
            _view.OnNextPageClicked += HandleNextPage;
            _view.OnPreviousPageClicked += HandlePreviousPage;
        }

        private void UnbindViewEvents() {
            if (_view == null) {
                return;
            }

            _view.InitButtons(false);
            _view.OnConfirmClicked -= HandleCloseWindow;
            _view.OnCloseClicked -= HandleCloseWindow;
            _view.OnNextPageClicked -= HandleNextPage;
            _view.OnPreviousPageClicked -= HandlePreviousPage;
        }

        private void UnbindGameEvents() {
            var gameplayEvents = GetController<GameplayController>();
            if (gameplayEvents != null) {
                gameplayEvents.OnHeroEnterDetectionZone -= HandleDetectionEnter;
                gameplayEvents.OnHeroExitDetectionZone -= HandleDetectionExit;
                //gameplayEvents.OnHeroConditionCheck += OnGameEventConditionCheck;
                gameplayEvents.OnHeroConditionMet -= OnGameEventConditionMet;
            }

        }

        private void BindGameEvents() {
            var gameplayEvents = GetController<GameplayController>();
            if (gameplayEvents != null) {
                gameplayEvents.OnHeroEnterDetectionZone += HandleDetectionEnter;
                gameplayEvents.OnHeroExitDetectionZone += HandleDetectionExit;
                //gameplayEvents.OnHeroConditionCheck += OnGameEventConditionCheck;
                gameplayEvents.OnHeroConditionMet += OnGameEventConditionMet;
            }
        }

        private void HandleDetectionEnter(DetectionZoneTrigger trigger) {

            var newWindow = trigger.GetWindowData();
            if (newWindow == null || newWindow.Count == 0)
                return;

            ShowOrEnqueueWindow(newWindow);
        }

        private void ShowOrEnqueueWindow(List<OverlayWindowData> newWindow) {

            if (_WindowQueue.Count == 0) {
                ShowWindow(newWindow);
                return;
            }

            _WindowQueue.Enqueue(newWindow);
        }

        private void HideOrDequeueWindow(List<OverlayWindowData> newWindow) {

            if (newWindow == _pageData) {
                if (_WindowQueue.Count > 0) {
                    ShowWindow(_WindowQueue.Dequeue());
                    return;
                }
                HideWindow();
            }
        }

        private void HandleDetectionExit(DetectionZoneTrigger trigger) {

            var newWindow = trigger.GetWindowData();
            if (newWindow == null || newWindow.Count == 0)
                return;
            
            HideOrDequeueWindow(newWindow);
        }

        private void OnGameEventConditionMet(DetectionZoneTrigger trigger) {

            ShowNextPage(true);
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