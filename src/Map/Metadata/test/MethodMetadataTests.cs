namespace Iceberg.Map.Metadata.Tests;

public class MethodMetadataTests
{
    [Fact]
    public void MethodName_ReturnsMethodName()
    {
        // Arrange
        var displayName = "Iceberg.Map.CalculationService.Calculate<int>(int firstInput, int secondInput)";
        var methodMetadata = new MethodMetadata(displayName, string.Empty);

        var expected = "Calculate";

        // Act
        var actual = methodMetadata.MethodName;

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ClassName_ReturnsClassName()
    {
        // Arrange
        var displayName = "Iceberg.Map.CalculationService.Calculate<int>(int firstInput, int secondInput)";
        var methodMetadata = new MethodMetadata(displayName, string.Empty);

        var expected = "CalculationService";

        // Act
        var actual = methodMetadata.ClassName;

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void NamespaceComponents_ReturnsNamespaceComponents()
    {
        // Arrange
        var displayName = "Iceberg.Map.CalculationService.Calculate<int>(int firstInput, int secondInput)";
        var methodMetadata = new MethodMetadata(displayName, string.Empty);

        var expected = new[] { "Iceberg", "Map" };

        // Act
        var actual = methodMetadata.NamespaceComponents;

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TypeParameters_ReturnsTypeParameters()
    {
        // Arrange
        var displayName = "Iceberg.Map.CalculationService.Calculate<string, string>(int, int)";
        var methodMetadata = new MethodMetadata(displayName, string.Empty);

        var expected = new[] { "string", "string" };

        // Act
        var actual = methodMetadata.TypeParameters;

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TypeParameters_HasNoTypeParameters_ReturnsEmptyArray()
    {
        // Arrange
        var displayName = "Iceberg.Map.CalculationService.Calculate(int, int)";
        var methodMetadata = new MethodMetadata(displayName, string.Empty);

        // Act
        var actual = methodMetadata.TypeParameters;

        // Assert
        Assert.Empty(actual);
    }

    [Fact]
    public void Parameters_HasParameters_ReturnsParameters()
    {
        // Arrange
        var displayName = "Iceberg.Map.CalculationService.Calculate<string, string>(int, int)";
        var methodMetadata = new MethodMetadata(displayName, string.Empty);

        var expected = new[] { "int", "int" };

        // Act
        var actual = methodMetadata.Parameters;

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Parameters_HasNoParameters_ReturnsEmptyArray()
    {
        // Arrange
        var displayName = "Iceberg.Map.CalculationService.Calculate<int, int>()";
        var methodMetadata = new MethodMetadata(displayName, string.Empty);

        // Act
        var actual = methodMetadata.Parameters;

        // Assert
        Assert.Empty(actual);
    }
}
