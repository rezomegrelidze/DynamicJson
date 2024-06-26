using System.Text;
using System.Text.Json;

public class DynamicJson
{
    public static dynamic Deserialize(string text)
    {
        var data = Encoding.UTF8.GetBytes(text);

        var reader = new Utf8JsonReader(data);

        reader.Read();
        return reader.TokenType == JsonTokenType.StartArray ? Deserializer.ReadArray(ref reader) : Deserializer.ReadObject(ref reader);
    }

    public static T Deserialize<T>(string text)
    {
        var data = Encoding.UTF8.GetBytes(text);

        var reader = new Utf8JsonReader(data);

        reader.Read();
        return Deserializer.Deserialize<T>(reader);
    }


    public static string Serialize(object obj)
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
