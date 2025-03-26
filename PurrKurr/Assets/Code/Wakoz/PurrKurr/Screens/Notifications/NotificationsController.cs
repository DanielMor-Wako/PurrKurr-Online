using Code.Wakoz.PurrKurr.Screens.Gameplay_Controller;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.Screens.Notifications
{

    [DefaultExecutionOrder(15)]
    public sealed class NotificationsController : SingleController
    {
        [SerializeField] private NotificationsView _view;

        private Queue<NotificationData> _notifications = new Queue<NotificationData>();
        private Coroutine _notificationCoroutine;

        protected override void Clean() {

            Unbind();
            DeactivateAll();
        }

        protected override Task Initialize() {

            Bind();
            return Task.CompletedTask;
        }

        private void Unbind() {

            var gameplayEvents = GetController<GameplayController>();
            if (gameplayEvents != null) {
                gameplayEvents.OnNewNotification -= HandleNewNotification;
            }
        }

        private void Bind() {

            var gameplayEvents = GetController<GameplayController>();
            if (gameplayEvents != null) {
                gameplayEvents.OnNewNotification += HandleNewNotification;
            }
        }

        private void HandleNewNotification(NotificationData data) {

            _notifications.Enqueue(data);

            _notificationCoroutine ??= StartCoroutine(ProcessNotifications());
        }

        private IEnumerator ProcessNotifications() {

            var index = -1;

            while (_notifications.Count > 0) {

                var currentNotification = _notifications.Dequeue();

                StartCoroutine(DisplayAndHide(currentNotification, ++index));

                yield return new WaitForSeconds(0.5f);
            }

            _notificationCoroutine = null;
        }

        private IEnumerator DisplayAndHide(NotificationData data, int siblingIndex = 0) {

            var message = data.Message;
            var uniqueId = Guid.NewGuid().ToString();

            _view.Show(message, uniqueId, siblingIndex);

            yield return new WaitForSeconds(data.Duration);

            _view.Hide(uniqueId);
        }

        private void DeactivateAll() {

            if (_notificationCoroutine != null) {
                StopCoroutine(_notificationCoroutine);
                _notificationCoroutine = null;
            }
        }

        [ContextMenu("Test Notifications")]
        public void TestNotifications() {

            HandleNewNotification(new NotificationData("Mission Complete", 3));
            HandleNewNotification(new NotificationData("Time 0:12", 2));
            HandleNewNotification(new NotificationData("Smashed 5 Foes", 5));
            HandleNewNotification(new NotificationData("Remained Food 89 %", 3));
        }
    }
}