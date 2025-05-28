using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Services.CloudSave;
using Unity.Services.CloudSave.Internal;
using Unity.Services.CloudSave.Models.Data.Player;
using UnityEngine;

namespace Code.Core.Services.DataManagement.Storage
{
    public class CloudSaveStorage : IStorage
    {

        public CloudSaveStorage() {
            
        }
/*
        public async Task<T> LoadAsync<T>(string groupid, string id) {

            var query = await _client.Value.Player.LoadAsync(new HashSet<string> { key });
            return query.TryGetValue(key, out var value) ? value.Value.GetAs<T>() : default;

            return result;
        }

        public async Task SaveAsync(string groupid, string id, string data) {

        }*/


        private readonly Lazy<IDataService> _client = new(() => CloudSaveService.Instance.Data);

        public async Task Save(string key, object value) {

            try {
                var data = new Dictionary<string, object> { { key, value } };
                await _client.Value.Player.SaveAsync(data);
                // await Call(_client.Value.Player.SaveAsync(data));
                Debug.Log("Data saved");
            }
            catch (Exception e) {
                Debug.LogError($"Save failed: {e.Message}");
            }
        }

        public async Task Save(params (string key, object value)[] values) {

            try {
                var data = values.ToDictionary(item => item.key, item => item.value);
                await _client.Value.Player.SaveAsync(data);
                Debug.Log("Data saved");
            }
            catch (Exception e) {
                Debug.LogError($"Save failed: {e.Message}");
            }
        }

        public async Task SavePublicScope(params (string key, object value)[] values) {
            
            var publicData = values.ToDictionary(item => item.key, item => item.value);
            await _client.Value.Player.SaveAsync(
                publicData, new Unity.Services.CloudSave.Models.Data.Player.SaveOptions(
                        new PublicWriteAccessClassOptions()));
        }

        public async Task SavePlayerScope( params (string key, object value)[] values) {

            var privatecData = values.ToDictionary(item => item.key, item => item.value);
            await _client.Value.Player.SaveAsync(
                privatecData, new Unity.Services.CloudSave.Models.Data.Player.SaveOptions(
                        new DefaultWriteAccessClassOptions()));
        }

        public async Task<T> Load<T>(string key) {
            
            var querypublic = await _client.Value.Player.LoadAsync(new HashSet<string> { key },
                new LoadOptions(new PublicReadAccessClassOptions()));

            if (querypublic.TryGetValue(key, out var value)) {
                return value.Value.GetAs<T>();
            }

            var queryprivate = await _client.Value.Player.LoadAsync(new HashSet<string> { key },
                new LoadOptions(new DefaultReadAccessClassOptions()));

            return queryprivate.TryGetValue(key, out var pvalue) ? pvalue.Value.GetAs<T>() : default;
        }

        public async Task<IEnumerable<T>> Load<T>(params string[] keys) {
            var query = await _client.Value.Player.LoadAsync(keys.ToHashSet());

            return keys.Select(k =>
            {
                if (query.TryGetValue(k, out var value)) {
                    return value != null ? value.Value.GetAs<T>() : default;
                }

                return default;
            });
        }

        public async Task<IEnumerable<string>> GetAllKeys() {

            var allPublicKeys = await _client.Value.Player.ListAllKeysAsync(
                new ListAllKeysOptions(new PublicReadAccessClassOptions()));

            Debug.Log($"Public Keys {allPublicKeys.Count}");

            var allPrivateKeys = await _client.Value.Player.ListAllKeysAsync();

            Debug.Log($"Private Keys: {(string.Join(", ", allPrivateKeys))}");

            List<string> allKeys = new List<string>();

            //StringBuilder keysProgress = new();

            foreach (var itemKey in allPublicKeys) {
                //keysProgress.Append($"{itemKey.Key} | ");
                allKeys.Add(itemKey.Key);
            }
            //Debug.Log($"Public Keys: {keysProgress}");
            //keysProgress = new();
            foreach (var itemKey in allPrivateKeys) {
                //keysProgress.Append($"{itemKey.Key} | ");
                allKeys.Add(itemKey.Key);
            }
            //Debug.Log($"Private Keys: {keysProgress}");
            return allKeys;
        }

        public async Task<List<T>> LoadAllAsync<T>() {
            var query = await _client.Value.Player.LoadAllAsync();
            return query.Values.Select(v => v.Value.GetAs<T>()).ToList();
        }

        // Delete public player data
        public async Task Delete(string key) {
            await _client.Value.Player.DeleteAsync(key, 
                new Unity.Services.CloudSave.Models.Data.Player.DeleteOptions(
                    new PublicWriteAccessClassOptions()));
        }

        public async Task DeleteAllAsync() {
            
            // Delete private player data
            await Call(_client.Value.Player.DeleteAllAsync());

            var allPublicKeys = await _client.Value.Player.ListAllKeysAsync(
                new ListAllKeysOptions(new PublicReadAccessClassOptions()));

            // Delete each public key individually
            foreach (var publicData in allPublicKeys) {
                await Call(Delete(publicData.Key));
            }

        }

        private static async Task Call(Task action) {
            try {
                await action;
            }
            catch (CloudSaveValidationException) {
                throw;
            }
            catch (CloudSaveRateLimitedException) {
                throw;
            }
            catch (CloudSaveException) {
                throw;
            }
        }

        private static T Deserialize<T>(string data) {
            if (typeof(T) == typeof(string))
                return (T)(object)data;
            return JsonConvert.DeserializeObject<T>(data);
        }
    }
}