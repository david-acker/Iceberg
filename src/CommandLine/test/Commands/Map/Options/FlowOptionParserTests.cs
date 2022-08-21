using Iceberg.CommandLine.Commands.Map.Options;
using System.Diagnostics.CodeAnalysis;

namespace Iceberg.CommandLine.Tests.Commands.Map.Options;

[ExcludeFromCodeCoverage]
public class FlowOptionParserTests
{
    [Theory]
    [InlineData("downstream", MappingFlow.Downstream)]
    [InlineData("DOWNSTREAM", MappingFlow.Downstream)]
    [InlineData("d", MappingFlow.Downstream)]
    [InlineData("D", MappingFlow.Downstream)]
    [InlineData("upstream", MappingFlow.Upstream)]
    [InlineData("UPSTREAM", MappingFlow.Upstream)]
    [InlineData("u", MappingFlow.Upstream)]
    [InlineData("U", MappingFlow.Upstream)]
    public void Parse(string flowName, MappingFlow expectedMappingFlow)
    {
        // Arrange
        var flowOptionParser = new FlowOptionParser();

        // Act
        var parseResult = flowOptionParser.Parse(new[] { flowName });

        // Assert
        Assert.False(parseResult.HasError);
        Assert.Equal(expectedMappingFlow, parseResult.Value);
    }

    [Fact]
    public void Parse_UnrecognizedFlowName_Fails()
    {
        // Arrange
        var flowName = "unknown";
        var flowOptionParser = new FlowOptionParser();

        // Act
        var parseResult = flowOptionParser.Parse(new[] { flowName });

        // Assert
        Assert.True(parseResult.HasError);
        Assert.Equal("Unrecognized mapping flow name. Please provide a valid flow name: downstream/d, upstream/u", parseResult.ErrorMessage);
    }
}
