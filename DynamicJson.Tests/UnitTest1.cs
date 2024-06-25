using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using Xunit;

public class DynamicJsonTests
{
    [Fact]
    public void Deserialize_Dynamic_ReturnsExpectedObject()
    {
        // Arrange
        string json = "{\"Name\":\"John\", \"Age\":30}";

        // Act
        dynamic result = DynamicJson.Deserialize(json);

        // Assert
        Assert.Equal("John", result.Name.ToString());
        Assert.Equal(30, (int)result.Age);
    }

    [Fact]
    public void Deserialize_Generic_ReturnsExpectedObject()
    {
        // Arrange
        string json = "{\"Name\":\"John\", \"Age\":30}";

        // Act
        var result = DynamicJson.Deserialize<Person>(json);

        // Assert
        Assert.Equal("John", result.Name);
        Assert.Equal(30, result.Age);
    }

    [Fact]
    public void Serialize_ReturnsExpectedJson()
    {
        // Arrange
        var person = new Person { Name = "John", Age = 30 };

        // Act
        string json = DynamicJson.Serialize(person);

        // Assert
        Assert.Contains("\"Name\":\"John\"", json);
        Assert.Contains("\"Age\":30", json);
    }

    [Fact]
    public void Deserialize_Dynamic_Array_ReturnsExpectedArray()
    {
        // Arrange
        string json = "[{\"Name\":\"John\", \"Age\":30}, {\"Name\":\"Jane\", \"Age\":25}]";

        // Act
        dynamic result = DynamicJson.Deserialize(json);

        // Assert
        Assert.Equal("John", result[0].Name.ToString());
        Assert.Equal(30, (int)result[0].Age);
        Assert.Equal("Jane", result[1].Name.ToString());
        Assert.Equal(25, (int)result[1].Age);
    }

    [Fact]
    public void Deserialize_Generic_Array_ReturnsExpectedArray()
    {
        // Arrange
        string json = "[{\"Name\":\"John\", \"Age\":30}, {\"Name\":\"Jane\", \"Age\":25}]";

        // Act
        var result = DynamicJson.Deserialize<List<Person>>(json);

        // Assert
        Assert.Equal("John", result[0].Name);
        Assert.Equal(30, result[0].Age);
        Assert.Equal("Jane", result[1].Name);
        Assert.Equal(25, result[1].Age);
    }
}

public class Person
{
    public string Name { get; set; }
    public int Age { get; set; }
}
