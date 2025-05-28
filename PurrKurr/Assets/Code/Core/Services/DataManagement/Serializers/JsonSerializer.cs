using Newtonsoft.Json;
using UnityEngine;

namespace Code.Core.Services.DataManagement.Serializers {

    public class JsonSerializer : ISerializer {
        public object Serialize<T>(T data) {
            return JsonUtility.ToJson(data).ToString();
        }

        public T Deserialize<T>(object serializedData) {
            return JsonUtility.FromJson<T>(serializedData as string);
        }
    }

    public class CloudSaveSerializer : ISerializer
    {
        public object Serialize<T>(T data) {
            return data;
        }

        public T Deserialize<T>(object serializedData) {
            return JsonConvert.DeserializeObject<T>(serializedData as string);
        }
    }
}