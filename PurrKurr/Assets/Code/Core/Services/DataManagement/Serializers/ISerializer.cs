namespace Code.Core.Services.DataManagement.Serializers {

    public interface ISerializer {
        object Serialize<T>(T data);
        T Deserialize<T>(object serializedData);
    }
}