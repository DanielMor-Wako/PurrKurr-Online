using Code.Core.Services.DataManagement.Serializers;
using System;
using System.Collections.Generic;

namespace Code.Core.DataClassFactory {

    public class DataClassesFactory {

        private static Dictionary<string, Type> _registeredInstances;
        private ISerializer _serializer;

        public DataClassesFactory(ISerializer serialization) {

            _serializer = serialization;

            _registeredInstances = new Dictionary<string, Type>();
        }

        public void Register(string key, Type type) {
            _registeredInstances[key] = type;
        }

        public Dictionary<string, Type> GetRegisteredInstances()
            => _registeredInstances;

        public IEnumerable<string> GetRegisteredKeys() 
            => _registeredInstances.Keys;

        public IEnumerable<Type> GetRegisteredValues() 
            => _registeredInstances.Values;

        public Type GetTypeOfKey(string key) => 
            _registeredInstances.TryGetValue(key, out var type) ? type 
            : throw new KeyNotFoundException($"The key '{key}' does not exist in the registered instance creation methods.");

        public string GetKeyOfType<T>() {
            foreach (var key in _registeredInstances) {
                if (key.Value.GetType() == typeof(T)) {
                    return key.Key;
                }
            }
            return null;
        }
    }
}