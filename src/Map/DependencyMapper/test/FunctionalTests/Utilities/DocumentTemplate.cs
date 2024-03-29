﻿using System.Diagnostics.CodeAnalysis;

namespace Iceberg.Map.DependencyMapper.FunctionalTests.Utilities;

[ExcludeFromCodeCoverage]
public sealed class DocumentTemplate
{
    public string DocumentName { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
}
