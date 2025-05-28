using Code.Core.Services.DataManagement.Serializers;
using Code.Core.Services.DataManagement.Storage;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using Unity.Services.Core;

namespace Code.Core.Services.DataManagement {

    public class SaveManager {

        private IStorage _storage;
        private ISerializer _serializer;

        public SaveManager(IStorage storage, ISerializer serialization) {
            _storage = storage;
            _serializer = serialization;
        }

        public async Task SaveAsync(string key, object data) {
            var serializedData = _serializer.Serialize(data);
            await _storage.Save( key, serializedData);
        }

        public async Task SaveAsync(string key, params (string key, object value)[] values) {
            var serializedData = _serializer.Serialize(values);
            await _storage.Save(key, serializedData);
        }

        public async Task SavePublicPlayerDataAsync(params (string key, object value)[] values) {
            await _storage.SavePublicScope(values);
        }

        public async Task SavePrivatePlayerDataAsync(params (string key, object value)[] values) {
            await _storage.SavePlayerScope(values);
        }

        public async Task<IEnumerable<string>> GetAllKeys() {
            return await _storage.GetAllKeys();
        }

        public async Task<T> LoadAsync<T>(string keys) {
            var serializedData = await LoadAsyncRaw<T>(keys);
            return _serializer.Deserialize<T>(serializedData);
        }

        public async Task<T> LoadAsyncRaw<T>(string keys) {
            return await _storage.Load<T>(keys);
        }

        public async Task<IEnumerable<T>> LoadAsyncRaw<T>(params string[] keys) {
            return await _storage.Load<T>(keys);
        }

        public async Task DeleteAllAsync() {
            await _storage.DeleteAllAsync();
        }
    }
}