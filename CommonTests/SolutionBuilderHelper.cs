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
            ICollection<MetadataReference> defaultReferences = new List<MetadataReference>
            {
                MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location)
            };

            var adhocWorkspace = new AdhocWorkspace();

            VersionStamp versionStamp = VersionStamp.Create();

            SolutionId solutionId = SolutionId.CreateNewId("solution");
            SolutionInfo solutionInfo = SolutionInfo.Create(solutionId, versionStamp);

            adhocWorkspace.AddSolution(solutionInfo);

            string projectName = "Project_1";
            ProjectId projectId = PrepareProject(versionStamp, defaultReferences, projectName, out ProjectInfo projectInfo);

            AddProjectToSolution(adhocWorkspace, projectInfo);

            AddCSharpFileToProject(sourceCode, adhocWorkspace, projectId);
            return adhocWorkspace.CurrentSolution;
        }

        public static Solution CreateSolutionFromSource(string sourceCode, string precompile)
        {
            ICollection<MetadataReference> defaultReferences = new List<MetadataReference>
            {
                MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location)
            };

            var adhocWorkspace = new AdhocWorkspace();

            VersionStamp versionStamp = VersionStamp.Create();

            SolutionId solutionId = SolutionId.CreateNewId("solution");
            SolutionInfo solutionInfo = SolutionInfo.Create(solutionId, versionStamp);

            adhocWorkspace.AddSolution(solutionInfo);

            string projectName = "Project_1";
            ProjectId projectId = PrepareProject(versionStamp, defaultReferences, projectName, out ProjectInfo projectInfo);

            projectName = "Project_Precompile";
            ProjectId precompileProjectId = PrepareProject(versionStamp, defaultReferences, projectName, out ProjectInfo precompileProjectInfo);

            // Add link between project & precompile project
            projectInfo = projectInfo.WithProjectReferences(new[] { new ProjectReference(precompileProjectId) });

            AddProjectToSolution(adhocWorkspace, precompileProjectInfo);
            AddProjectToSolution(adhocWorkspace, projectInfo);

            AddCSharpFileToProject(precompile, adhocWorkspace, precompileProjectId);
            AddCSharpFileToProject(sourceCode, adhocWorkspace, projectId);
            return adhocWorkspace.CurrentSolution;
        }

        private static void AddCSharpFileToProject(string sourceCode, AdhocWorkspace adhocWorkspace, ProjectId projectId)
        {
            var sourceText = SourceText.From(sourceCode);
            adhocWorkspace.AddDocument(projectId, "Class_1.cs", sourceText);
            adhocWorkspace.TryApplyChanges(adhocWorkspace.CurrentSolution);
        }

        private static void AddProjectToSolution(AdhocWorkspace adhocWorkspace, ProjectInfo projectInfo)
        {
            var solution = adhocWorkspace.CurrentSolution.AddProject(projectInfo);
            adhocWorkspace.TryApplyChanges(solution);
        }

        private static ProjectId PrepareProject(VersionStamp versionStamp, ICollection<MetadataReference> defaultReferences, string projectName, out ProjectInfo projectInfo)
        {
            ProjectId projectId = ProjectId.CreateNewId();

            CSharpParseOptions parseOptions = new CSharpParseOptions(LanguageVersion.CSharp7_3);
            CompilationOptions compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);

            projectInfo = ProjectInfo.Create(projectId, versionStamp, projectName, projectName, LanguageNames.CSharp)
                                     .WithCompilationOptions(compilationOptions)
                                     .WithParseOptions(parseOptions)
                                     .WithMetadataReferences(defaultReferences);
            return projectId;
        }

        public static SemanticModel? GetSemanticModel(Solution solution)
        {
            var project = solution.Projects.First();

            // Build the project.
            return BuildProject(project);
        }

        public static SemanticModel? BuildProject(Project project)
        {
            SemanticModel? semanticModel = null;

            var lastTask = project.GetCompilationAsync().ContinueWith(compileTask =>
            {
                var compilation = compileTask.Result;
                if (compilation == null)
                {
                    return;
                }
                var diagnostics = compilation.GetDiagnostics();

                List<Diagnostic> compilationErrors = new List<Diagnostic>();
                foreach (var diagnostic in diagnostics)
                {
                    if (diagnostic.Severity == DiagnosticSeverity.Error)
                    {
                        compilationErrors.Add(diagnostic);
                    }
                }

                if (compilationErrors.Count > 0)
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
