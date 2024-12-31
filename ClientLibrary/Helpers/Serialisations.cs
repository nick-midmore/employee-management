using System.Text.Json;

namespace ClientLibrary.Helpers;
public static class Serialisations
{
    public static string SerialiseObj<T>(T model) => JsonSerializer.Serialize(model);
    public static T DeserializeJsonString<T>(string json) => JsonSerializer.Deserialize<T>(json);
    public static IList<T> DeserialiseJsonList<T>(string json) => JsonSerializer.Deserialize<IList<T>>(json);
}
