using System.Collections.Generic;
using System.Threading.Tasks;

namespace Code.Core.Services.DataManagement.Storage {

    public interface IStorage {
        //Task SaveAsync(string groupId, string id, string data);
        //Task<string> LoadAsync(string groupId, string id);

        Task Save(string key, object value);

        Task Save(params (string key, object value)[] values);

        Task SavePublicScope(params (string key, object value)[] values);

        Task SavePlayerScope(params (string key, object value)[] values);

        Task<IEnumerable<string>> GetAllKeys();

        Task<T> Load<T>(string key);

        Task<IEnumerable<T>> Load<T>(params string[] keys);

        Task Delete(string key);

        Task DeleteAllAsync();
    }
}