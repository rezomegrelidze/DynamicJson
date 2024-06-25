using System.Collections;
using System.Dynamic;
using System.Reflection;
using System.Text.Json;
using System.Numerics;

internal static class Deserializer
{
    public static dynamic ReadArray(Utf8JsonReader reader)
    {
        var array = new List<dynamic?>();
        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
        {
            if (reader.TokenType == JsonTokenType.StartArray)
                array.Add(ReadArray(reader));
            else if (reader.TokenType == JsonTokenType.StartObject)
                array.Add(ReadObject(reader));
            else if (reader.TokenType == JsonTokenType.String)
                array.Add(ReadString(reader));
            else if (reader.TokenType == JsonTokenType.False)
                array.Add(false);
            else if (reader.TokenType == JsonTokenType.True)
                array.Add(true);
            else if (reader.TokenType == JsonTokenType.Number)
                array.Add(reader.GetDouble());
            else if (reader.TokenType == JsonTokenType.Null)
                array.Add(null);
        }

        return array;
    }

    static string ReadString(Utf8JsonReader reader)
    {
        return reader.GetString();
    }

    static void ReadProperty(Utf8JsonReader reader, IDictionary<string, object> dict)
    {
        var propertyName = reader.GetString();
        reader.Read();

        if (reader.TokenType == JsonTokenType.True)
            dict[propertyName] = true;
        else if (reader.TokenType == JsonTokenType.False)
            dict[propertyName] = false;
        else if (reader.TokenType == JsonTokenType.Null)
            dict[propertyName] = null;
        else if (reader.TokenType == JsonTokenType.Number)
            dict[propertyName] = reader.GetDouble();
        else if (reader.TokenType == JsonTokenType.String)
            dict[propertyName] = reader.GetString();
        else if (reader.TokenType == JsonTokenType.StartArray)
            dict[propertyName] = ReadArray(reader);
        else if (reader.TokenType == JsonTokenType.StartObject)
            dict[propertyName] = ReadObject(reader);
    }

    public static dynamic ReadObject(Utf8JsonReader reader)
    {
        var expandoObject = new ExpandoObject();
        var dict = expandoObject as IDictionary<string, object>;
        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
        {
            if (reader.TokenType == JsonTokenType.PropertyName)
                ReadProperty(reader, dict);
        }

        return expandoObject;
    }

    internal static T Deserialize<T>(Utf8JsonReader reader)
    {
        try
        {
            var type = typeof(T);
            if (type.IsAssignableTo(typeof(IList)) && reader.TokenType == JsonTokenType.StartArray)
            {
                return ReadArray<T>(reader);
            }

            return ReadObject<T>(reader);

        }
        catch(Exception ex)
        {
            throw new JsonException($"Failed to deserialize type {typeof(T)}",ex);
        }
    }

    private static T ReadObject<T>(Utf8JsonReader reader)
    {
        return (T) ReadObject(reader, typeof(T));
    }

    private static object? ReadObject(Utf8JsonReader reader, Type type)
    {
        var instance = Activator.CreateInstance(type);
        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
        {
            if (reader.TokenType == JsonTokenType.PropertyName)
                ReadProperty(reader, instance);
        }

        return instance;
    }

    private static void ReadProperty(Utf8JsonReader reader, object? instance)
    {
        var type = instance.GetType();
        var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);

        var propertyName = reader.GetString();

        var property =
            properties.FirstOrDefault(x => string.Equals(x.Name, propertyName, StringComparison.OrdinalIgnoreCase));

        if (property == null)
            throw new JsonException($"Type {type} doesn't contain property with name {propertyName}");

        reader.Read();

        if (reader.TokenType == JsonTokenType.True && property.PropertyType == typeof(bool))
        {
            property.SetValue(instance, true);
        }
        else if (reader.TokenType == JsonTokenType.False && property.PropertyType == typeof(bool))
        {
            property.SetValue(instance, false);
        }
        else if (reader.TokenType == JsonTokenType.Null && property.PropertyType.IsClass)
        {

            property.SetValue(instance, null);
        }
        else if (reader.TokenType == JsonTokenType.Number && property.PropertyType.IsValueType)
        {
            var numberType = typeof(INumber<>).MakeGenericType(property.PropertyType);
            if (property.PropertyType.IsAssignableTo(numberType))
            {
                dynamic value = null;
                if (property.PropertyType == typeof(int))
                    value = reader.GetInt32();
                else if (property.PropertyType == typeof(decimal))
                    value = reader.GetDecimal();
                else if (property.PropertyType == typeof(byte))
                    value = reader.GetInt32();
                else if (property.PropertyType == typeof(double))
                    value = reader.GetDouble();
                else if (property.PropertyType == typeof(float))
                    value = reader.GetSingle();
                else if (property.PropertyType == typeof(byte))
                    value = reader.GetByte();
                else if (property.PropertyType == typeof(long))
                    value = reader.GetInt64();
                else if (property.PropertyType == typeof(uint))
                    value = reader.GetUInt32();
                else if (property.PropertyType == typeof(ulong))
                    value = reader.GetUInt64();
                else if (property.PropertyType == typeof(uint))
                    value = reader.GetUInt32();
                else if (property.PropertyType == typeof(short))
                    value = reader.GetInt16();
                else if (property.PropertyType == typeof(ushort))
                    value = reader.GetUInt16();

                property.SetValue(instance, value);
            }
        }
        else if (reader.TokenType == JsonTokenType.String && property.PropertyType == typeof(string))
            property.SetValue(instance, reader.GetString());
        else if (reader.TokenType == JsonTokenType.StartArray && property.PropertyType.IsAssignableTo(typeof(IList)))
            property.SetValue(instance, ReadArray(reader, property.PropertyType));
        else if (reader.TokenType == JsonTokenType.StartObject)
            property.SetValue(instance, ReadObject(reader, property.PropertyType));
    }

    private static object? ReadArray(Utf8JsonReader reader, Type type)
    {
        var instance = Activator.CreateInstance(type);
        IList array = (instance as IList)!;
        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
        {
            if (reader.TokenType == JsonTokenType.StartArray)
                array.Add(ReadArray(reader, type));
            else if (reader.TokenType == JsonTokenType.StartObject)
                array.Add(ReadObject(reader, type));
            else if (reader.TokenType == JsonTokenType.String)
                array.Add(ReadString(reader));
            else if (reader.TokenType == JsonTokenType.False)
                array.Add(false);
            else if (reader.TokenType == JsonTokenType.True)
                array.Add(true);
            else if (reader.TokenType == JsonTokenType.Number)
            {
                if (type.IsGenericType)
                {
                    var numberType = type.GetGenericArguments()[0];

                    if (numberType == typeof(int))
                        array.Add(reader.GetInt32());
                    else if (numberType == typeof(decimal))
                        array.Add(reader.GetDecimal());
                    else if (numberType == typeof(byte))
                        array.Add(reader.GetInt32());
                    else if (numberType == typeof(double))
                        array.Add(reader.GetDouble());
                    else if (numberType == typeof(float))
                        array.Add(reader.GetSingle());
                    else if (numberType == typeof(byte))
                        array.Add(reader.GetByte());
                    else if (numberType == typeof(long))
                        array.Add(reader.GetInt64());
                    else if (numberType == typeof(uint))
                        array.Add(reader.GetUInt32());
                    else if (numberType == typeof(ulong))
                        array.Add(reader.GetUInt64());
                    else if (numberType == typeof(uint))
                        array.Add(reader.GetUInt32());
                    else if (numberType == typeof(short))
                        array.Add(reader.GetInt16());
                    else if (numberType == typeof(ushort))
                        array.Add(reader.GetUInt16());
                    else throw new Exception($"Unexpected number type! {numberType}");
                }
                else
                {
                    array.Add(reader.GetDouble());
                }
            }
            else if (reader.TokenType == JsonTokenType.Null)
                array.Add(null);
        }

        return array;
    }

    private static T ReadArray<T>(Utf8JsonReader reader)
    {
        return (T) ReadArray(reader, typeof(T));
    }
}
