﻿using Iceberg.Map.DependencyMapper.Context;
using Iceberg.Map.DependencyMapper.FunctionalTests.Utilities;
using Iceberg.Map.Metadata;
using Microsoft.Extensions.Logging.Abstractions;
using System.Diagnostics.CodeAnalysis;

namespace Iceberg.Map.DependencyMapper.FunctionalTests.Method.Downstream;

// TODO: Clean up class and method names in test code samples.
[ExcludeFromCodeCoverage]
public sealed class DownstreamMethodDependencyMapperTests
{
    [Theory]
    [InlineData("public")]
    [InlineData("protected")]
    [InlineData("private")]
    public async Task InvokedMethodDeclaredOnClass(string accessModifier)
    {
        // Arrange
        var projectTemplate = new ProjectTemplate
        {
            ProjectName = "Project",
            Documents = new[]
            {
                new DocumentTemplate
                {
                    DocumentName = "MyClass.cs",
                    Text = $@"
                        public class MyClass
                        {{
                            public int OtherMethod()
                            {{
                                return EntryPoint();
                            }}

                            {accessModifier} int EntryPoint()
                            {{
                                return 1;
                            }}
                        }}
                    "
                }
            }
        };

        var expected = new MethodDependencyMap
        {
            {
                new MethodMetadata("MyClass.EntryPoint()", "MyClass.cs"),
                new HashSet<MethodMetadata>
                {
                    new MethodMetadata("MyClass.OtherMethod()", "MyClass.cs")
                }
            },
            {
                new MethodMetadata("MyClass.OtherMethod()", "MyClass.cs"),
                new HashSet<MethodMetadata>()
            }
        };

        var solutionContext = await TestUtilities.SetupMethodSolutionContext(projectTemplate);
        var mapper = new MethodDependencyMapper(NullLoggerFactory.Instance);

        // Act + Assert
        var matchingEntryPoints = await solutionContext.FindMethodEntryPoints("MyClass", "EntryPoint");
        var selectedEntryPoint = Assert.Single(matchingEntryPoints);

        var actual = await mapper.MapDownstream(solutionContext, new[] { selectedEntryPoint });

        TestUtilities.AssertGeneratedMethodDependencyMap(expected, actual);
    }

    [Theory]
    [InlineData("public")]
    [InlineData("protected")]
    public async Task InvokedMethodDeclaredOnBaseClass(string accessModifier)
    {
        // Arrange
        var projectTemplate = new ProjectTemplate
        {
            ProjectName = "Project",
            Documents = new[]
            {
                new DocumentTemplate
                {
                    DocumentName = "DerivedClass.cs",
                    Text = @"
                        public class DerivedClass : BaseClass
                        {
                            public void EntryPoint()
                            {
                                InheritedMethod();
                            }
                        }
                    "
                },
                new DocumentTemplate
                {
                    DocumentName = "BaseClass.cs",
                    Text = $@"
                        public class BaseClass
                        {{
                            {accessModifier} void InheritedMethod()
                            {{
                            }}
                        }}
                    "
                }
            }
        };

        var expected = new MethodDependencyMap
        {
            {
                new MethodMetadata("DerivedClass.EntryPoint()", "DerivedClass.cs"),
                new HashSet<MethodMetadata>()
            },
            {
                new MethodMetadata("BaseClass.InheritedMethod()", "BaseClass.cs"),
                new HashSet<MethodMetadata>
                {
                    new MethodMetadata("DerivedClass.EntryPoint()", "DerivedClass.cs")
                }
            }
        };

        var solutionContext = await TestUtilities.SetupMethodSolutionContext(projectTemplate);
        var mapper = new MethodDependencyMapper(NullLoggerFactory.Instance);

        // Act + Assert
        var matchingEntryPoints = await solutionContext.FindMethodEntryPoints("BaseClass", "InheritedMethod");
        var selectedEntryPoint = Assert.Single(matchingEntryPoints);

        var actual = await mapper.MapDownstream(solutionContext, new[] { selectedEntryPoint });

        TestUtilities.AssertGeneratedMethodDependencyMap(expected, actual);
    }

    [Fact]
    public async Task MethodCalledOnInstanceImplementingInterface()
    {
        // Arrange
        var projectTemplate = new ProjectTemplate
        {
            ProjectName = "Project",
            Documents = new[]
            {
                new DocumentTemplate
                {
                    DocumentName = "ConcreteService.cs",
                    Text = @"
                        public interface IService
                        {
                            int ServiceMethod();
                        }

                        public class ConcreteService : IService
                        {
                            public int ServiceMethod()
                            {
                                return 1;
                            }
                        }
                    "
                },
                new DocumentTemplate
                {
                    DocumentName = "ConsumingService.cs",
                    Text = @"
                        public class ConsumingService
                        {
                            private readonly IService _service;

                            public ConsumingService(IService service)
                            {
                                _service = service;
                            }

                            public int EntryPoint()
                            {
                                return _service.ServiceMethod();
                            }
                        }
                    "
                }
            }
        };

        var expected = new MethodDependencyMap
        {
            {
                new MethodMetadata("ConsumingService.EntryPoint()", "ConsumingService.cs"),
                new HashSet<MethodMetadata>()
            },
            {
                new MethodMetadata("ConcreteService.ServiceMethod()", "ConcreteService.cs"),
                new HashSet<MethodMetadata>
                {
                    new MethodMetadata("ConsumingService.EntryPoint()", "ConsumingService.cs")
                }
            }
        };

        var solutionContext = await TestUtilities.SetupMethodSolutionContext(projectTemplate);
        var mapper = new MethodDependencyMapper(NullLoggerFactory.Instance);

        // Act + Assert
        var matchingEntryPoints = await solutionContext.FindMethodEntryPoints("ConcreteService", "ServiceMethod");
        var selectedEntryPoint = Assert.Single(matchingEntryPoints);

        var actual = await mapper.MapDownstream(solutionContext, new[] { selectedEntryPoint });

        TestUtilities.AssertGeneratedMethodDependencyMap(expected, actual);
    }

    [Fact]
    public async Task MethodCalledRecursively()
    {
        // Arrange
        var projectTemplate = new ProjectTemplate
        {
            ProjectName = "Project",
            Documents = new[]
            {
                new DocumentTemplate
                {
                    DocumentName = "MyClass.cs",
                    Text = @"
                        public class MyClass
                        {
                            public int Fibonacci(int n, int a, int b)
                            {
                                if (n == 0)
                                    return a;
                                if (n == 1)
                                    return b;

                                return Fibonacci(n - 1, b, a + b);
                            }
                        }
                    "
                }
            }
        };

        var expected = new MethodDependencyMap
        {
            {
                new MethodMetadata("MyClass.Fibonacci(int, int, int)", "MyClass.cs"),
                new HashSet<MethodMetadata>()
            }
        };

        var solutionContext = await TestUtilities.SetupMethodSolutionContext(projectTemplate);
        var mapper = new MethodDependencyMapper(NullLoggerFactory.Instance);

        // Act + Assert
        var matchingEntryPoints = await solutionContext.FindMethodEntryPoints("MyClass", "Fibonacci");
        var selectedEntryPoint = Assert.Single(matchingEntryPoints);

        var actual = await mapper.MapDownstream(solutionContext, new[] { selectedEntryPoint });

        TestUtilities.AssertGeneratedMethodDependencyMap(expected, actual);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task StaticMethodCalled(bool isClassStatic)
    {
        // Arrange
        var projectTemplate = new ProjectTemplate
        {
            ProjectName = "Project",
            Documents = new[]
            {
                new DocumentTemplate
                {
                    DocumentName = "MyClass.cs",
                    Text = @"
                        public class MyClass
                        {
                            public int MyMethod()
                            {
                                return ClassWithStaticMethod.StaticMethod();
                            }
                        }
                    "
                },
                new DocumentTemplate
                {
                    DocumentName = "ClassWithStaticMethod.cs",
                    Text = $@"
                        public {(isClassStatic ? "static " : "")}class ClassWithStaticMethod
                        {{
                            public static int StaticMethod()
                            {{
                                return 1;
                            }}
                        }}
                    "
                }
            }
        };

        var expected = new MethodDependencyMap
        {
            {
                new MethodMetadata("MyClass.MyMethod()", "MyClass.cs"),
                new HashSet<MethodMetadata>()
            },
            {
                new MethodMetadata("ClassWithStaticMethod.StaticMethod()", "ClassWithStaticMethod.cs"),
                new HashSet<MethodMetadata>
                {
                    new MethodMetadata("MyClass.MyMethod()", "MyClass.cs")
                }
            }
        };

        var solutionContext = await TestUtilities.SetupMethodSolutionContext(projectTemplate);
        var mapper = new MethodDependencyMapper(NullLoggerFactory.Instance);

        // Act + Assert
        var matchingEntryPoints = await solutionContext.FindMethodEntryPoints("ClassWithStaticMethod", "StaticMethod");
        var selectedEntryPoint = Assert.Single(matchingEntryPoints);

        var actual = await mapper.MapDownstream(solutionContext, new[] { selectedEntryPoint });

        TestUtilities.AssertGeneratedMethodDependencyMap(expected, actual);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task OverriddenVirtualMethodOnBaseClass(bool isAbstract)
    {
        // Arrange
        var projectTemplate = new ProjectTemplate
        {
            ProjectName = "Project",
            Documents = new[]
            {
                new DocumentTemplate
                {
                    DocumentName = "DerivedClass.cs",
                    Text = @"
                        public class DerivedClass : BaseClass
                        {
                            public override int VirtualMethod()
                            {
                                return 2;
                            }
                        }
                    "
                },
                new DocumentTemplate
                {
                    DocumentName = "BaseClass.cs",
                    Text = $@"
                        public {(isAbstract ? "abstract ": "")}class BaseClass
                        {{
                            public virtual int VirtualMethod()
                            {{
                                return 1;
                            }}
                        }}
                    "
                }
            }
        };

        var expected = new MethodDependencyMap
        {
            {
                new MethodMetadata("DerivedClass.VirtualMethod()", "DerivedClass.cs"),
                new HashSet<MethodMetadata>()
            }
        };

        var solutionContext = await TestUtilities.SetupMethodSolutionContext(projectTemplate);
        var mapper = new MethodDependencyMapper(NullLoggerFactory.Instance);

        // Act + Assert
        var matchingEntryPoints = await solutionContext.FindMethodEntryPoints("DerivedClass", "VirtualMethod");
        var selectedEntryPoint = Assert.Single(matchingEntryPoints);

        var actual = await mapper.MapDownstream(solutionContext, new[] { selectedEntryPoint });

        TestUtilities.AssertGeneratedMethodDependencyMap(expected, actual);
    }

    [Fact]
    public async Task OverriddenVirtualMethodCallingBaseImplementation()
    {
        // Arrange
        var projectTemplate = new ProjectTemplate
        {
            ProjectName = "Project",
            Documents = new[]
            {
                new DocumentTemplate
                {
                    DocumentName = "DerivedClass.cs",
                    Text = @"
                        public class DerivedClass : BaseClass
                        {
                            public override int VirtualMethod()
                            {
                                return base.VirtualMethod() + 1;
                            }
                        }
                    "
                },
                new DocumentTemplate
                {
                    DocumentName = "BaseClass.cs",
                    Text = @"
                        public class BaseClass
                        {
                            public virtual int VirtualMethod()
                            {
                                return 1;
                            }
                        }
                    "
                }
            }
        };

        var expected = new MethodDependencyMap
        {
            {
                new MethodMetadata("DerivedClass.VirtualMethod()", "DerivedClass.cs"),
                new HashSet<MethodMetadata>()
            },
            {
                new MethodMetadata("BaseClass.VirtualMethod()", "BaseClass.cs"),
                new HashSet<MethodMetadata>
                {
                    new MethodMetadata("DerivedClass.VirtualMethod()", "DerivedClass.cs")
                }
            },
        };

        var solutionContext = await TestUtilities.SetupMethodSolutionContext(projectTemplate);
        var mapper = new MethodDependencyMapper(NullLoggerFactory.Instance);

        // Act + Assert
        var matchingEntryPoints = await solutionContext.FindMethodEntryPoints("BaseClass", "VirtualMethod");
        var selectedEntryPoint = Assert.Single(matchingEntryPoints);

        var actual = await mapper.MapDownstream(solutionContext, new[] { selectedEntryPoint });

        TestUtilities.AssertGeneratedMethodDependencyMap(expected, actual);
    }

    [Fact]
    public async Task OverriddenAbstractMethodOnBaseClass()
    {
        // Arrange
        var projectTemplate = new ProjectTemplate
        {
            ProjectName = "Project",
            Documents = new[]
            {
                new DocumentTemplate
                {
                    DocumentName = "DerivedClass.cs",
                    Text = @"
                        public class DerivedClass : AbstractClass
                        {
                            public override int AbstractMethod()
                            {
                                return 1;
                            }
                        }
                    "
                },
                new DocumentTemplate
                {
                    DocumentName = "AbstractClass.cs",
                    Text = @"
                        public abstract class AbstractClass
                        {
                            public abstract int AbstractMethod();
                        }
                    "
                }
            }
        };

        var expected = new MethodDependencyMap
        {
            {
                new MethodMetadata("DerivedClass.AbstractMethod()", "DerivedClass.cs"),
                new HashSet<MethodMetadata>()
            }
        };

        var solutionContext = await TestUtilities.SetupMethodSolutionContext(projectTemplate);
        var mapper = new MethodDependencyMapper(NullLoggerFactory.Instance);

        // Act + Assert
        var matchingEntryPoints = await solutionContext.FindMethodEntryPoints("DerivedClass", "AbstractMethod");
        var selectedEntryPoint = Assert.Single(matchingEntryPoints);

        var actual = await mapper.MapDownstream(solutionContext, new[] { selectedEntryPoint });

        TestUtilities.AssertGeneratedMethodDependencyMap(expected, actual);
    }

    [Fact]
    public async Task MethodReferenceNotInvoked()
    {
        // Arrange
        var projectTemplate = new ProjectTemplate
        {
            ProjectName = "Project",
            Documents = new[]
            {
                new DocumentTemplate
                {
                    DocumentName = "MyClass.cs",
                    Text = @"
                        using System;

                        public class MyClass
                        {
                            public void MyMethod()
                            {
                                Func<int, int> methodReference = MyOtherMethod;

                                var result = methodReference(1);
                            }

                            public int MyOtherMethod(int a)
                            {
                                return a;
                            }
                        }
                    "
                }
            }
        };

        var expected = new MethodDependencyMap
        {
            {
                new MethodMetadata("MyClass.MyOtherMethod(int)", "MyClass.cs"),
                new HashSet<MethodMetadata>
                {
                    new MethodMetadata("MyClass.MyMethod()", "MyClass.cs")
                }
            },
            {
                new MethodMetadata("MyClass.MyMethod()", "MyClass.cs"),
                new HashSet<MethodMetadata>()
            }
        };

        var solutionContext = await TestUtilities.SetupMethodSolutionContext(projectTemplate);
        var mapper = new MethodDependencyMapper(NullLoggerFactory.Instance);

        // Act + Assert
        var matchingEntryPoints = await solutionContext.FindMethodEntryPoints("MyClass", "MyOtherMethod");
        var selectedEntryPoint = Assert.Single(matchingEntryPoints);

        var actual = await mapper.MapDownstream(solutionContext, new[] { selectedEntryPoint });

        TestUtilities.AssertGeneratedMethodDependencyMap(expected, actual);
    }

    [Fact(Skip = "Overload Resolution Issue")]
    public async Task MethodReferencePassedAsArgument()
    {
        // Arrange
        var projectTemplate = new ProjectTemplate
        {
            ProjectName = "Project",
            Documents = new[]
            {
                new DocumentTemplate
                {
                    DocumentName = "MainClass.cs",
                    Text = @"
                        public class MainClass
                        {
                            private readonly ConsumingClass _consumingClass = new ConsumingClass();
                            private readonly ConsumedClass _consumedClass = new ConsumedClass();

                            public int EntryPoint()
                            {
                                return _consumingClass.ConsumingMethod(_consumedClass.Method);
                            }
                        }
                    "
                },
                new DocumentTemplate
                {
                    DocumentName = "ConsumingClass.cs",
                    Text = @"
                        public class ConsumingClass
                        {
                            public int ConsumingMethod(Func<int, int> func)
                            {
                                return func(10);
                            }
                        }
                    "
                },
                new DocumentTemplate
                {
                    DocumentName = "ConsumedClass.cs",
                    Text = @"
                        public class ConsumedClass
                        {
                            public int Method(int x)
                            {
                                return x;
                            }

                            public int Method(int x, int y)
                            {
                                return x + y;
                            }
                        }
                    "
                }
            }
        };

        var expected = new MethodDependencyMap
        {
            {
                new MethodMetadata("MyClass.EntryPoint()", "MyClass.cs"),
                new HashSet<MethodMetadata>
                {
                    new MethodMetadata("MyClass.OtherMethod", "MyClass.cs")
                }
            },
            {
                new MethodMetadata("MyClass.OtherMethod()", "MyClass.cs"),
                new HashSet<MethodMetadata>()
            }
        };

        var solutionContext = await TestUtilities.SetupMethodSolutionContext(projectTemplate);
        var mapper = new MethodDependencyMapper(NullLoggerFactory.Instance);

        // Act + Assert
        var matchingEntryPoints = await solutionContext.FindMethodEntryPoints("MainClass", "EntryPoint");
        var selectedEntryPoint = Assert.Single(matchingEntryPoints);

        var actual = await mapper.MapDownstream(solutionContext, new[] { selectedEntryPoint });

        TestUtilities.AssertGeneratedMethodDependencyMap(expected, actual);
    }
}

