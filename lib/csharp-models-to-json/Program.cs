using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ganss.IO;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace CSharpModelsToJson
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile(args[0], true, true)
                .Build();

            var includes = new List<string>();
            var excludes = new List<string>();

            config.Bind("include", includes);
            config.Bind("exclude", excludes);

            var files = GetFileNames(includes, excludes);
            var parsedFiles = files.Select(ParseFile).ToList();

            Console.WriteLine(JsonConvert.SerializeObject(parsedFiles));
        }

        private static IEnumerable<string> GetFileNames(IEnumerable<string> includes, IEnumerable<string> excludes) {
            var fileNames = ExpandGlobPatterns(includes).ToList();

            foreach (var path in ExpandGlobPatterns(excludes)) {
                fileNames.Remove(path);
            }

            return fileNames;
        }

        private static List<string> ExpandGlobPatterns(IEnumerable<string> globPatterns) {
            var fileNames = new List<string>();

            foreach (var paths in globPatterns.Select(pattern => Glob.Expand(pattern)))
            {
                fileNames.AddRange(paths.Select(path => path.FullName));
            }

            return fileNames;
        }

        private static File ParseFile(string path) {
            var source = System.IO.File.ReadAllText(path);
            var tree = CSharpSyntaxTree.ParseText(source);
            var root = (CompilationUnitSyntax) tree.GetRoot();
 
            var modelCollector = new ModelCollector();
            var enumCollector = new EnumCollector();

            modelCollector.Visit(root);
            enumCollector.Visit(root);

            return new File {
                FileName = Path.GetFullPath(path),
                Models = modelCollector.Models,
                Enums = enumCollector.Enums
            };
        }
    }
    
    internal class File
    {
        public string FileName { get; set; }
        public IEnumerable<Model> Models { get; set; }
        public IEnumerable<Enum> Enums { get; set; }
    }
}