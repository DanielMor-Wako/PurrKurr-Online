using System;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.Screens.Notifications
{
    public class NotificationsView : View//<NotificationsModel>
    {
        public event Action OnClickedNotification;

        [SerializeField] private NotificationView _notificationViewPrefab;
        [SerializeField] private Transform _parentContainer;

        private Dictionary<string, NotificationView> _notificationViews = new();

        private Queue<NotificationView> _pool = new();

        /// <summary>
        /// Inspector exposed function for toggling hiding
        /// </summary>
        public void ClickedNotification()
            => OnClickedNotification?.Invoke();

        protected override void ModelChanged() { }

        public void Show(string message, string uniqueId, int siblingIndex = 0) {

            var view = GetOrCreateNotificationView();
            SetNotification(view, message, siblingIndex);

            _notificationViews[uniqueId] = view;
        }

        public void Hide(string uniqueId) {

            if (!_notificationViews.TryGetValue(uniqueId, out var view)) {
                return;
            }

            ReturnToPool(view);
        }

        public void HideAll() {

            foreach (var notificationView in _notificationViews) {
                var view = notificationView.Value;

                if (view == null)
                    continue;

                ReturnToPool(view);
            }
        }

        private NotificationView GetOrCreateNotificationView() {

            if (_pool.Count > 0) {
                var reusedView = _pool.Dequeue();
                reusedView.gameObject.SetActive(true);
                return reusedView;
            }

            var newView = Instantiate(_notificationViewPrefab, _parentContainer);

            newView.transform.SetParent(_parentContainer, false);
            newView.gameObject.SetActive(true);

            return newView;
        }

        /// <summary>
        /// Sets view with model and updates view state by the model
        /// </summary>
        /// <param name="notificationView"></param>
        /// <param name="model"></param>
        /// <param name="siblingIndex"></param>
        private void SetNotification(NotificationView notificationView, string message, int siblingIndex) {

            notificationView.SetText(message);

            notificationView.transform.SetSiblingIndex(siblingIndex);

            notificationView.Fader.CanvasTarget.alpha = 0;
            notificationView.Fader.StartTransition(1);

            notificationView.Color.TargetImage.color = Color.black;
            notificationView.Color.StartTransition();

            notificationView.ImageScaler.TargetRectTransform.localScale = notificationView.ImageScaler.MaxScale;
            notificationView.ImageScaler.EndTransition();
        }

        private void ReturnToPool(NotificationView view) {

            view.Fader.StartTransition(0, 
                () => {
                _pool.Enqueue(view);
                view.gameObject.SetActive(false);
            });

        }

    }
}