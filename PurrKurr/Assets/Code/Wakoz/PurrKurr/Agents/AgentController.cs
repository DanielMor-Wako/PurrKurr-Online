using Code.Wakoz.PurrKurr.DataClasses.Characters;
using Code.Wakoz.PurrKurr.DataClasses.Enums;
using Code.Wakoz.PurrKurr.Views;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.Agents
{

    [DefaultExecutionOrder(15)]
    public class AgentController : Controller
    {
        public static event Action<AgentController> OnRegisterAgent;
        public static event Action<AgentController> OnDeregisterAgent;

        [SerializeField] private List<GoalData> _goals;

        [SerializeField] private Character2DController _controller;

        [SerializeField] private MultiStateView _state;

        [SerializeField] private bool _isInitialized;

        public void SetCurrentGoal(Definitions.AgentGoal goal)
        {
            _isInitialized = true;

            if (_state == null)
                return;

            _state.gameObject.SetActive(_isInitialized);
            _state.ChangeState((int)goal);
        }

        public List<GoalData> Goals => _goals;

        public Character2DController CharacterController => _controller;

        protected override void Clean() {}

        protected override Task Initialize()
        {
            _controller ??= GetComponent<Character2DController>();

            return Task.CompletedTask;
        }

        private void OnEnable()
        {
            _state.gameObject.SetActive(_isInitialized);
            OnRegisterAgent?.Invoke(this);
        }

        private void OnDisable() => OnDeregisterAgent?.Invoke(this);

    }
}