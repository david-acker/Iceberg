using Iceberg.Map.DependencyMapper.FunctionalTests.Utilities;
using Iceberg.Map.Metadata;
using Microsoft.Extensions.Logging.Abstractions;
using System.Diagnostics.CodeAnalysis;

namespace Iceberg.Map.DependencyMapper.FunctionalTests.Method.Upstream;

[ExcludeFromCodeCoverage]
public sealed class UpstreamMethodDependencyMapperDepthTests
{
    [Fact]
    public async Task EnforcesMaximumMappingDepth()
    {
        // Arrange
        var projectTemplate = new ProjectTemplate
        {
            ProjectName = "Project",
            Documents = new[]
            {
                new DocumentTemplate
                {
                    DocumentName = "Class_A.cs",
                    Text = @"
                        namespace UpstreamMethodMappingTests;

                        public class Class_A : BaseClass_A
                        {
                            private readonly IClass_E _classE;

                            public Class_A(
                                IClass_C classC,
                                IClass_E classE) : base(classC)
                            {
                                _classE = classE;
                            }

                            public override int Method_A()
                            {
                                var a = AbstractMethod_B();
                                var b = ProtectedMethod_D();
                                var c = Method_F();

                                return a + b + c;
                            }

                            private int Method_F() => _classE.Method_E();
                        }
                    "
                },
                new DocumentTemplate
                {
                    DocumentName = "BaseClass_A.cs",
                    Text = @"
                        namespace UpstreamMethodMappingTests;

                        public class BaseClass_A : BaseClass_B
                        {
                            private readonly IClass_C _classC;

                            public BaseClass_A(IClass_C classC) : base()
                            {
                                _classC = classC;
                            }

                            public virtual int Method_A() => 5;

                            public override int AbstractMethod_B()
                            {
                                return _classC.Method_C() + ProtectedMethod_D();
                            }

                            protected int ProtectedMethod_D() => VirtualMethod_B();
                        }
                    "
                },
                new DocumentTemplate
                {
                    DocumentName = "BaseClass_B.cs",
                    Text = @"
                        namespace UpstreamMethodMappingTests;

                        public abstract class BaseClass_B
                        {
                            public abstract int AbstractMethod_B();

                            public virtual int VirtualMethod_B() => 3;
                        }"
                },
                new DocumentTemplate
                {
                    DocumentName = "Class_C.cs",
                    Text = @"
                        namespace UpstreamMethodMappingTests;

                        public interface IClass_C
                        {
                            int Method_C();
                        }

                        public class Class_C : IClass_C
                        {
                            public int Method_C() => PrivateMethod_C() + 1;

                            private int PrivateMethod_C() => 5;
                        }"
                },
                new DocumentTemplate
                {
                    DocumentName = "Class_E.cs",
                    Text = @"
                        namespace UpstreamMethodMappingTests;

                        public interface IClass_E
                        {
                            int Method_E();
                        }

                        public class Class_E: IClass_E
                        {
                            private readonly IClass_C _classC = new Class_C();

                            public int Method_E() => _classC.Method_C();
                        }"
                }
            }
        };

        var expected = new MethodDependencyMap
        {
            {
                new MethodMetadata("UpstreamMethodMappingTests.Class_A.Method_A()", "Class_A.cs"),
                new HashSet<MethodMetadata>
                {
                    new MethodMetadata("UpstreamMethodMappingTests.BaseClass_A.AbstractMethod_B()", "BaseClass_A.cs"),
                    new MethodMetadata("UpstreamMethodMappingTests.BaseClass_A.ProtectedMethod_D()", "BaseClass_A.cs"),
                    new MethodMetadata("UpstreamMethodMappingTests.Class_A.Method_F()", "Class_A.cs")
                }
            },
            {
                new MethodMetadata("UpstreamMethodMappingTests.BaseClass_A.AbstractMethod_B()", "BaseClass_A.cs"),
                new HashSet<MethodMetadata>
                {
                    new MethodMetadata("UpstreamMethodMappingTests.Class_C.Method_C()", "Class_C.cs"),
                    new MethodMetadata("UpstreamMethodMappingTests.BaseClass_A.ProtectedMethod_D()", "BaseClass_A.cs")
                }
            },
            {
                new MethodMetadata("UpstreamMethodMappingTests.BaseClass_A.ProtectedMethod_D()", "BaseClass_A.cs"),
                new HashSet<MethodMetadata>
                {
                    new MethodMetadata("UpstreamMethodMappingTests.BaseClass_B.VirtualMethod_B()", "BaseClass_B.cs"),
                }
            },
            {
                new MethodMetadata("UpstreamMethodMappingTests.Class_A.Method_F()", "Class_A.cs"),
                new HashSet<MethodMetadata>
                {
                    new MethodMetadata("UpstreamMethodMappingTests.Class_E.Method_E()", "Class_E.cs"),
                }
            },
            {
                new MethodMetadata("UpstreamMethodMappingTests.Class_E.Method_E()", "Class_E.cs"),
                new HashSet<MethodMetadata>
                {
                    new MethodMetadata("UpstreamMethodMappingTests.Class_C.Method_C()", "Class_C.cs"),
                }
            },
            {
                new MethodMetadata("UpstreamMethodMappingTests.Class_C.Method_C()", "Class_C.cs"),
                new HashSet<MethodMetadata>
                {
                    new MethodMetadata("UpstreamMethodMappingTests.Class_C.PrivateMethod_C()", "Class_C.cs"),
                }
            }
        };

        int depth = 2;

        var solutionContext = await TestUtilities.SetupMethodSolutionContext(projectTemplate);
        var mapper = new MethodDependencyMapper(NullLoggerFactory.Instance);

        // Act + Assert
        var matchingEntryPoints = await solutionContext.FindMethodEntryPoints("Class_A", "Method_A");
        var selectedEntryPoint = Assert.Single(matchingEntryPoints);

        var actual = await mapper.MapUpstream(solutionContext, selectedEntryPoint, depth);

        TestUtilities.AssertGeneratedMethodDependencyMap(expected, actual);
    }
}

