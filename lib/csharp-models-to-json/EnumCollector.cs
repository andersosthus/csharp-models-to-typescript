using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
 
namespace CSharpModelsToJson
{
    public class Enum
    {
        public string Identifier { get; set; }
        public Dictionary<string, object> Values { get; set; }
    }

    public class EnumCollector: CSharpSyntaxWalker
    {
        public readonly List<Enum> Enums = new();

        public override void VisitEnumDeclaration(EnumDeclarationSyntax node)
        {
            var values = new Dictionary<string, object>();

            foreach (var member in node.Members) {
                values[member.Identifier.ToString()] = member.EqualsValue?.Value.ToString();
            }

            Enums.Add(new Enum
            {
                Identifier = node.Identifier.ToString(),
                Values = values
            });
        }
    }
}
