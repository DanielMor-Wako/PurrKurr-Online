using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Code.Wakoz.PurrKurr.DataClasses.ScriptableObjectData;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.Logic.GameFlow {

    [DefaultExecutionOrder(10)]
    public sealed class LogicController : SingleController {

        public InputLogic InputLogic => TryGet<InputLogic>() as InputLogic;
        public AbilitiesLogic AbilitiesLogic => TryGet<AbilitiesLogic>() as AbilitiesLogic;
        public GameplayLogic GameplayLogic => TryGet<GameplayLogic>() as GameplayLogic;

        private readonly Dictionary<Type, ScriptableAsset> _logics = new();

        private const string DefaultAssetPathPrefix = "DataManagement/GameConfig/";

        /// <summary>
        /// Trying to get a ref by casting a generic as return value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="logicRef"></param>
        /// <returns></returns>
        public ScriptableAsset TryGet<T>() where T : ScriptableAsset {

            if (!_logics.TryGetValue(typeof(T), out var asset)) {
                return null;
            }

            return asset;
        }

        /// <summary>
        /// Init logics for player input, character abilities, gameplay settings
        /// Note: AbilitiesLogic relies on input logic to already be instantiated
        /// </summary>
        /// <returns></returns>
        /// Todo: Inject logics instead of direct ref
        protected override Task Initialize() {

            AddLogic<InputLogic>("PlayerInputData", DefaultAssetPathPrefix);
            
            AddLogic<AbilitiesLogic>("AbilitiesData", DefaultAssetPathPrefix);

            AddLogic<GameplayLogic>("GameplayLogicData", DefaultAssetPathPrefix);

            return Task.CompletedTask;
        }

        protected override void Clean() {

            foreach(var asset in _logics.Values) {
                asset?.Unload();
            }

            _logics.Clear();
        }

        private void AddLogic<T>(string assetName, string assetPathPrefix) where T : ScriptableAsset, new() {

            var logic = (ScriptableAsset)Activator.CreateInstance(typeof(T), assetName, assetPathPrefix) ?? throw new ArgumentNullException(typeof(T).ToString());
            _logics[typeof(T)] = logic;
        }

    }

}