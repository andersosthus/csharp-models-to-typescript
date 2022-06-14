using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;

namespace CSharpModelsToJson.Tests
{
    public class ModelCollectorTest
    {
        [Fact]
        public void BasicInheritance_ReturnsInheritedClass()
        {
            var tree = CSharpSyntaxTree.ParseText(@"
                public class A : B, C, D
                {
                    public void AMember()
                    {
                    }
                }"
            );

            var root = (CompilationUnitSyntax)tree.GetRoot();

            var modelCollector = new ModelCollector();
            modelCollector.VisitClassDeclaration(root.DescendantNodes().OfType<ClassDeclarationSyntax>().First());

            Assert.NotNull(modelCollector.Models);
            Assert.Equal(new[] { "B", "C", "D" }, modelCollector.Models.First().BaseClasses);
        }

        [Fact]
        public void InterfaceImport_ReturnsSyntaxClassFromInterface()
        {
            var tree = CSharpSyntaxTree.ParseText(@"
                public interface IPhoneNumber {
                    string Label { get; set; }
                    string Number { get; set; }
                    int MyProperty { get; set; }
                }

                public interface IPoint
                {
                   // Property signatures:
                   int x
                   {
                      get;
                      set;
                   }

                   int y
                   {
                      get;
                      set;
                   }
                }


                public class X {
                    public IPhoneNumber test { get; set; }
                    public IPoint test2 { get; set; }
                }"
            );

            var root = (CompilationUnitSyntax)tree.GetRoot();

            var modelCollector = new ModelCollector();
            modelCollector.Visit(root);

            Assert.NotNull(modelCollector.Models);
            Assert.Equal(3, modelCollector.Models.Count);
            Assert.Equal(3, modelCollector.Models.First().Properties.Count());
        }


        [Fact]
        public void TypedInheritance_ReturnsInheritance()
        {
            var tree = CSharpSyntaxTree.ParseText(@"
                public class A : IController<Controller>
                {
                    public void AMember()
                    {
                    }
                }"
            );

            var root = (CompilationUnitSyntax)tree.GetRoot();

            var modelCollector = new ModelCollector();
            modelCollector.VisitClassDeclaration(root.DescendantNodes().OfType<ClassDeclarationSyntax>().First());

            Assert.NotNull(modelCollector.Models);
            Assert.Equal(new[] { "IController<Controller>" }, modelCollector.Models.First().BaseClasses);
        }

        [Fact]
        public void AccessibilityRespected_ReturnsPublicOnly()
        {
            var tree = CSharpSyntaxTree.ParseText(@"
                public class A : IController<Controller>
                {
                    const int A_Constant = 0;

                    private string B { get; set }

                    static string C { get; set }

                    public string Included { get; set }

                    public void AMember() 
                    { 
                    }
                }"
            );

            var root = (CompilationUnitSyntax)tree.GetRoot();

            var modelCollector = new ModelCollector();
            modelCollector.VisitClassDeclaration(root.DescendantNodes().OfType<ClassDeclarationSyntax>().First());

            Assert.NotNull(modelCollector.Models);
            Assert.NotNull(modelCollector.Models.First().Properties);
            Assert.Single(modelCollector.Models.First().Properties);
        }

        [Fact]
        public void IgnoresJsonIgnored_ReturnsOnlyNotIgnored()
        {
            var tree = CSharpSyntaxTree.ParseText(@"
                public class A : IController<Controller>
                {
                    const int A_Constant = 0;

                    private string B { get; set }

                    static string C { get; set }

                    public string Included { get; set }

                    [JsonIgnore]
                    public string Ignored { get; set; }

                    public void AMember() 
                    { 
                    }
                }"
            );

            var root = (CompilationUnitSyntax)tree.GetRoot();

            var modelCollector = new ModelCollector();
            modelCollector.VisitClassDeclaration(root.DescendantNodes().OfType<ClassDeclarationSyntax>().First());

            Assert.NotNull(modelCollector.Models);
            Assert.NotNull(modelCollector.Models.First().Properties);
            Assert.Single(modelCollector.Models.First().Properties);
        }

        [Fact]
        public void DictionaryInheritance_ReturnsIndexAccessor()
        {
            var tree = CSharpSyntaxTree.ParseText(@"public class A : Dictionary<string, string> { }");

            var root = (CompilationUnitSyntax)tree.GetRoot();

            var modelCollector = new ModelCollector();
            modelCollector.VisitClassDeclaration(root.DescendantNodes().OfType<ClassDeclarationSyntax>().First());
            
            Assert.NotNull(modelCollector.Models);
            Assert.NotNull(modelCollector.Models.First().BaseClasses);
            Assert.Equal(new[] { "Dictionary<string, string>" }, modelCollector.Models.First().BaseClasses);
        }
    }
}