using System;
using System.Text;
using System.Text.Json;
using System.IO;
using Xunit;
using Bogus;

public class DynamicJsonTests
{
    [Fact]
    public void Deserialize_SimpleObject_ShouldReturnDynamicObject()
    {
        string json = "{\"name\":\"John\", \"age\":30}";
        dynamic result = DynamicJson.Deserialize(json);

        Assert.Equal("John", (string)result.name);
        Assert.Equal(30, (int)result.age);
    }

    [Fact]
    public void Deserialize_SimpleArray_ShouldReturnDynamicArray()
    {
        string json = "[{\"name\":\"John\"}, {\"name\":\"Jane\"}]";
        dynamic result = DynamicJson.Deserialize(json);

        Assert.Equal("John", (string)result[0].name);
        Assert.Equal("Jane", (string)result[1].name);
    }

    [Fact]
    public void Deserialize_NestedObject_ShouldReturnDynamicObject()
    {
        string json = "{\"person\":{\"name\":\"John\", \"address\":{\"city\":\"New York\"}}}";
        dynamic result = DynamicJson.Deserialize(json);

        Assert.Equal("John", (string)result.person.name);
        Assert.Equal("New York", (string)result.person.address.city);
    }

    [Fact]
    public void Deserialize_EmptyObject_ShouldReturnDynamicObject()
    {
        string json = "{}";
        dynamic result = DynamicJson.Deserialize(json);

        Assert.NotNull(result);
    }

    [Fact]
    public void Deserialize_EmptyArray_ShouldReturnDynamicArray()
    {
        string json = "[]";
        dynamic result = DynamicJson.Deserialize(json);

        Assert.NotNull(result);
    }

    [Fact]
    public void Deserialize_InvalidJson_ShouldThrowException()
    {
        string json = "{name:\"John\""; // Missing closing brace and quotes

        Assert.Throws<JsonException>(() => DynamicJson.Deserialize(json));
    }

    [Fact]
    public void Serialize_SimpleObject_ShouldReturnJsonString()
    {
        var obj = new { name = "John", age = 30 };
        string json = DynamicJson.Serialize(obj);

        Assert.Equal("{\"name\":\"John\",\"age\":30}", json);
    }

    [Fact]
    public void Serialize_NestedObject_ShouldReturnJsonString()
    {
        var obj = new { person = new { name = "John", address = new { city = "New York" } } };
        string json = DynamicJson.Serialize(obj);

        Assert.Equal("{\"person\":{\"name\":\"John\",\"address\":{\"city\":\"New York\"}}}", json);
    }

    [Fact]
    public void Serialize_EmptyObject_ShouldReturnJsonString()
    {
        var obj = new { };
        string json = DynamicJson.Serialize(obj);

        Assert.Equal("{}", json);
    }

    [Fact]
    public void Serialize_NullValue_ShouldReturnJsonString()
    {
        var obj = new { name = (string)null };
        string json = DynamicJson.Serialize(obj);

        Assert.Equal("{\"name\":null}", json);
    }

    [Fact]
    public void Deserialize_GenericType_ShouldReturnTypedObject()
    {
        string json = "{\"name\":\"John\", \"age\":30}";
        var result = DynamicJson.Deserialize<Person>(json);

        Assert.Equal("John", result.Name);
        Assert.Equal(30, result.Age);
    }

    [Fact]
    public void Deserialize_GenericTypeWithExtraFields_ShouldIgnoreExtraFields()
    {
        string json = "{\"name\":\"John\", \"age\":30, \"extra\":\"extra\"}";
        var result = DynamicJson.Deserialize<Person>(json);

        Assert.Equal("John", result.Name);
        Assert.Equal(30, result.Age);
    }

    [Fact]
    public void Deserialize_GenericTypeWithMissingFields_ShouldDefaultToTypeDefaults()
    {
        string json = "{\"name\":\"John\"}";
        var result = DynamicJson.Deserialize<Person>(json);

        Assert.Equal("John", result.Name);
        Assert.Equal(0, result.Age); // default int value
    }

    [Fact]
    public void SerializeLargeObject_Then_Deserialize()
    {
        var length = 100000;
        Faker<Person> faker = new Faker<Person>()
            .RuleFor(o => o.Name, f => f.Person.FullName)
            .RuleFor(o => o.Age, f => f.Random.Number(1, 100));


        var people = faker.Generate(length);

        var serialized = people.Serialize();

        var deserilzed = serialized.Deserialize<List<Person>>();

        Assert.True(deserilzed.Count == length);
    }
}

public class Person
{
    public string Name { get; set; }
    public int Age { get; set; }
}
