namespace Skyline.DataMiner.CICD.CSharpAnalysis.Tests
{
    using System.Reflection;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Text;

    internal class SolutionBuilderHelper
    {
        public static Solution CreateSolutionFromSource(string sourceCode)
        {
            ICollection<MetadataReference> defaultReferences = new List<MetadataReference>();
            defaultReferences.Add(MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location));

            var adhocWorkspace = new AdhocWorkspace();

            VersionStamp versionStamp = VersionStamp.Create();

            SolutionId solutionId = SolutionId.CreateNewId("solution");
            SolutionInfo solutionInfo = SolutionInfo.Create(solutionId, versionStamp);

            adhocWorkspace.AddSolution(solutionInfo);

            ProjectId projectId = ProjectId.CreateNewId();

            string projectName = "Project_1";
            CSharpParseOptions parseOptions = new CSharpParseOptions(LanguageVersion.CSharp7_3);
            CompilationOptions compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);

            ProjectInfo projectInfo = ProjectInfo.Create(projectId, versionStamp, projectName, projectName, LanguageNames.CSharp)
                .WithCompilationOptions(compilationOptions)
                .WithParseOptions(parseOptions)
                .WithMetadataReferences(defaultReferences);

            var solution = adhocWorkspace.CurrentSolution;
            solution = solution.AddProject(projectInfo);
            bool result = adhocWorkspace.TryApplyChanges(solution);
            solution = adhocWorkspace.CurrentSolution;

            var sourceText = SourceText.From(sourceCode);
            var doc = adhocWorkspace.AddDocument(projectId, "Class_1.cs", sourceText);
            adhocWorkspace.TryApplyChanges(solution);
            solution = adhocWorkspace.CurrentSolution;

            return solution;
        }

        public static SemanticModel GetSemanticModel(Solution solution)
        {
            var project = solution.Projects.First();

            // Build the project.
            return BuildProject(project);
        }

        private static SemanticModel BuildProject(Project project)
        {
            SemanticModel semanticModel = null;

            var lastTask = project.GetCompilationAsync().ContinueWith(compileTask =>
            {
                var compilation = compileTask.Result;
                var diagnostics = compilation.GetDiagnostics();

                List<Diagnostic> compilationErrors = new List<Diagnostic>();
                foreach (var diagnostic in diagnostics)
                {
                    if (diagnostic.Severity == DiagnosticSeverity.Error)
                    {
                        compilationErrors.Add(diagnostic);
                    }
                }

                if(compilationErrors.Count > 0)
                {
                    throw new ArgumentException("Could not compile project: " + String.Join(Environment.NewLine, compilationErrors));
                }

                var syntaxTree = compilation.SyntaxTrees.First();
                semanticModel = compilation.GetSemanticModel(syntaxTree, true);

                //buildResult.CompilationErrors = compilationErrors;
                //buildResult.Result = compilationErrors.Count == 0;
                //buildResult.Compilation = compilation;
            });

            lastTask.Wait();

            return semanticModel;
        }
    }
}
