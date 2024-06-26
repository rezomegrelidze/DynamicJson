using System.Collections;
using System.Dynamic;
using System.Reflection;
using System.Text.Json;

internal static class Serializer{
    public static void SerializeObject(Utf8JsonWriter writer, object obj)
    {
        if (obj is IList arr)
        {
            WriteArray(writer, arr);
        }
        else if (obj is bool)
        {
            WriteBoolean(writer, (bool)obj);
        }
        else if (obj is string str)
        {
            WriteString(writer, str);
        }
        else if (obj is double or int or decimal or float or short
                 or byte or ulong or uint or ushort or sbyte)
        {
            WriteNumber(writer, obj);
        }
        else if (obj is null)
        {
            WriteNull(writer);
        }
        else
        {
            WriteObject(writer, obj);
        }
    }

    private static void WriteObject(Utf8JsonWriter writer, object o)
    {
        writer.WriteStartObject();

        if (o is ExpandoObject)
        {
            var dict = o as IDictionary<string, object>;

            foreach (var pair in dict)
            {
                WriteProperty(writer,pair.Key,pair.Value);
            }
        }
        else
        {
            var type = o.GetType();
            var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (var property in properties)
            {
                WriteProperty(writer, property, o);
            }
        }

        writer.WriteEndObject();
    }

    private static void WriteProperty(Utf8JsonWriter writer, PropertyInfo property, object o)
    {
        writer.WritePropertyName(property.Name);
        var val = property.GetValue(o, null);
        SerializeObject(writer,val);
    }

    private static void WriteProperty(Utf8JsonWriter writer, string property, object value)
    {
        writer.WritePropertyName(property);
        SerializeObject(writer,value);
    }

    private static void WriteBoolean(Utf8JsonWriter writer, bool b)
    {
        writer.WriteBooleanValue(b);
    }

    private static void WriteNull(Utf8JsonWriter writer)
    {
        writer.WriteNullValue();
    }

    private static void WriteNumber(Utf8JsonWriter writer, object o)
    {
        switch (o)
        {
            case decimal dec:
                writer.WriteNumberValue(dec);
                break;
            case double d:
                writer.WriteNumberValue(d);
                break;
            case int n:
                writer.WriteNumberValue(n);
                break;
            case float f:
                writer.WriteNumberValue(f);
                break;
            case byte b:
                writer.WriteNumberValue(b);
                break;
            case sbyte sb:
                writer.WriteNumberValue(sb);
                break;
            case short s:
                writer.WriteNumberValue(s);
                break;
            case ushort u:
                writer.WriteNumberValue(u);
                break;
            case uint ui:
                writer.WriteNumberValue(ui);
                break;
            case long l:
                writer.WriteNumberValue(l);
                break;
            case ulong ul:
                writer.WriteNumberValue(ul);
                break;
        }
    }

    private static void WriteString(Utf8JsonWriter writer, string str)
    {
        writer.WriteStringValue(str);
    }

    private static void WriteArray(Utf8JsonWriter writer, IEnumerable arr)
    {
        writer.WriteStartArray();
        foreach (var item in arr)
        {
            SerializeObject(writer,item);
        }
        writer.WriteEndArray();
    }
}