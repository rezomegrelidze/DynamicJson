using System.Text;
using System.Text.Json;

public static class DynamicJson
{
    /// <summary>
    /// Deserializes the json string to a C# dynamic object
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    /// <exception cref="JsonException"></exception>
    public static dynamic Deserialize(this string text)
    {
        try
        {
            var data = Encoding.UTF8.GetBytes(text);

            var reader = new Utf8JsonReader(data);

            reader.Read();
            return reader.TokenType == JsonTokenType.StartArray ? Deserializer.ReadArray(ref reader) : Deserializer.ReadObject(ref reader);
        }
        catch (Exception ex)
        {
            throw new JsonException($"Invalid json encountered - Message: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Deserializes the JSON string to the given type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="text"></param>
    /// <returns></returns>
    public static T Deserialize<T>(this string text)
    {
        var data = Encoding.UTF8.GetBytes(text);

        var reader = new Utf8JsonReader(data);

        reader.Read();
        return Deserializer.Deserialize<T>(reader);
    }

    /// <summary>
    /// Serializes the object to a JSON string
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static string Serialize(this object obj)
    {
       using MemoryStream ms = new MemoryStream();
        var writer = new Utf8JsonWriter(ms);

        Serializer.SerializeObject(writer, obj);
        writer.Flush();
        var data = ms.ToArray();
        var json = Encoding.UTF8.GetString(data);

       return json;
    }
}
