using System.Collections.Generic;
using System.Threading.Tasks;
using Code.Wakoz.PurrKurr.DataClasses.ScriptableObjectData;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.Logic.GameFlow {

    [DefaultExecutionOrder(10)]
    public class LogicController : SingleController {

        public InputLogic InputLogic;
        public AbilitiesLogic AbilitiesLogic;
        public GameplayLogic GameplayLogic;
        
        private List<SOData> _logics = new();

        protected override Task Initialize() {

            InputLogic = new InputLogic("PlayerInputData");
            _logics.Add( InputLogic );
            
            // Note: AbilitiesLogic relies on input logic to already be instantiated
            AbilitiesLogic = new AbilitiesLogic("AbilitiesData"); // mainly for touch pads data
            _logics.Add( AbilitiesLogic );

            GameplayLogic = new GameplayLogic("GameplayLogicData");
            _logics.Add( GameplayLogic );
            
            return Task.CompletedTask;
        }

        protected override void Clean() {

            InputLogic = null;
            AbilitiesLogic = null;
        }

        /// <summary>
        /// Trying to get a ref by casting a generic as return value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="logicRef"></param>
        /// <returns></returns>
        public T TryGet<T>() where T : SOData {

            foreach (var logic in _logics) {
                
                if (logic.GetType() != typeof(T)) {
                    continue;
                }

                return logic as T;
            }

            return null;
        }
        
    }

}