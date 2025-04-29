using System.Collections.Generic;
using System.Threading.Tasks;
using Code.Wakoz.PurrKurr.DataClasses.ScriptableObjectData;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.Logic.GameFlow {

    [DefaultExecutionOrder(10)]
    public sealed class LogicController : SingleController {

        public InputLogic InputLogic;
        public AbilitiesLogic AbilitiesLogic;
        public GameplayLogic GameplayLogic;

        private const string DefaultAssetPathPrefix = "DataManagement/GameConfig/";

        private List<ScriptableAsset> _logics = new();

        /// <summary>
        /// Todo: Inject logics instead of direct ref
        /// Note: AbilitiesLogic relies on input logic to already be instantiated
        /// </summary>
        /// <returns></returns>
        protected override Task Initialize() {

            InputLogic = new InputLogic("PlayerInputData", DefaultAssetPathPrefix);
            _logics.Add( InputLogic );
            
            AbilitiesLogic = new AbilitiesLogic("AbilitiesData", DefaultAssetPathPrefix);
            _logics.Add( AbilitiesLogic );

            GameplayLogic = new GameplayLogic("GameplayLogicData", DefaultAssetPathPrefix);
            _logics.Add( GameplayLogic );
            
            return Task.CompletedTask;
        }

        protected override void Clean() {

            InputLogic = null;
            AbilitiesLogic = null;
            GameplayLogic = null;
        }

        /// <summary>
        /// Trying to get a ref by casting a generic as return value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="logicRef"></param>
        /// <returns></returns>
        public T TryGet<T>() where T : ScriptableAsset {

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