using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Slack_Gpt_Source_Generator;

[Generator]
public class GptToolSourceGenerator : IIncrementalGenerator
{
    public const string InternalParameterClassName = "InternalCallingParameters";
    
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Register our source generator
        var classDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                static (s, _) => IsSyntaxTargetForGeneration(s),
                static (ctx, _) => GetSemanticTargetForGeneration(ctx))
            .Where(static m => m is not null)!;

        // Combine the selected class declarations with the Compilation
        IncrementalValueProvider<(Compilation, ImmutableArray<ClassDeclarationSyntax>)> compilationAndClasses
            = context.CompilationProvider.Combine(classDeclarations.Collect());

        // Register the source generator
        context.RegisterSourceOutput(compilationAndClasses,
            (spc, source) => Execute(source.Item1, source.Item2, spc));
    }

    // Check if the syntax node is a class declaration that might be a candidate
    private static bool IsSyntaxTargetForGeneration(SyntaxNode node)
    {
        return node is ClassDeclarationSyntax;
    }

    // Get the class declaration syntax node
    private static ClassDeclarationSyntax GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
    {
        var classSyntax = (ClassDeclarationSyntax)context.Node;

        // We need to check if the class implements IExpressionGptTool
        // We'll do this in the main Execute method to have access to symbols
        return classSyntax;
    }

    private void Execute(Compilation compilation, ImmutableArray<ClassDeclarationSyntax> classes,
        SourceProductionContext context)
    {
        if (classes.IsDefaultOrEmpty)
            return;

        foreach (var classSyntax in classes.Distinct())
        {
            var semanticModel = compilation.GetSemanticModel(classSyntax.SyntaxTree);
            var classSymbol = ModelExtensions.GetDeclaredSymbol(semanticModel, classSyntax) as INamedTypeSymbol;
            if (classSymbol == null)
                continue;

            // Check if class implements IExpressionGptTool
            if (!ImplementsInterface(classSymbol, "IExpressionGptTool"))
                continue;

            // Generate the partial class code
            var classSource = GeneratePartialClass(compilation, classSymbol, context);
            if (!string.IsNullOrEmpty(classSource))
            {
                // Add the generated source code to the compilation
                context.AddSource($"{classSymbol.Name}_Generated.cs", SourceText.From(classSource, Encoding.UTF8));
            }
        }
    }

    private string GeneratePartialClass(Compilation compilation, INamedTypeSymbol classSymbol,
        SourceProductionContext context)
    {
        // Check if the class inherits from BaseGptTool
        var baseGptToolSymbol = compilation.GetTypeByMetadataName("Slack_GPT_Socket.GptApi.BaseGptTool");
        if (baseGptToolSymbol == null)
        {
            // Handle error if BaseGptTool is not found
            context.ReportDiagnostic(Diagnostic.Create(
                new DiagnosticDescriptor(
                    "GPT001",
                    "BaseGptTool not found",
                    "The base type 'BaseGptTool' was not found in the compilation.",
                    "GptToolGenerator",
                    DiagnosticSeverity.Error,
                    true),
                Location.None));
            return null;
        }

        if (!InheritsFrom(classSymbol, baseGptToolSymbol))
            return null;

        // Find the method 'CallExpression' to analyze
        var methodSymbol = classSymbol.GetMembers().OfType<IMethodSymbol>()
            .FirstOrDefault(m => m.Name == "CallExpression");

        if (methodSymbol == null)
            return null;

        // Generate the parameters code
        var parametersCode = GenerateParametersCode(methodSymbol);

        // Generate the class source code
        var namespaceName = classSymbol.ContainingNamespace.ToDisplayString();
        var classSource = $$"""
                            using System.Collections.Generic;
                            using Slack_GPT_Socket.GptApi;
                            using Newtonsoft.Json;
                            
                            namespace {{namespaceName}}
                            {
                                public partial class {{classSymbol.Name}}
                                {
                                    public override string Name => "{{classSymbol.Name.ToLowerInvariant()}}";
                                    public override string Description => "{{GetMethodDescription(methodSymbol)}}";
                            
                                    public {{classSymbol.Name}}()
                                    {
                            {{parametersCode}}
                                    }
                                    
                            {{GenerateMethodCallFromString(methodSymbol)}}
                                    
                            {{GenerateInternalClassWithParameters(methodSymbol)}}
                                }
                            }
                            """;
        return classSource;
    }

    private string GenerateParametersCode(IMethodSymbol methodSymbol)
    {
        var sb = new StringBuilder();

        sb.AppendLine("Parameters = new GptToolParameter");
        sb.AppendLine("{");
        sb.AppendLine("    Type = PropertyType.Object,");
        sb.AppendLine("    Properties = new Dictionary<string, GptToolParameter>");
        sb.AppendLine("    {");

        var index = 0;
        foreach (var parameter in methodSymbol.Parameters)
        {
            var type = GetPropertyType(parameter.Type);
            var description = GetParameterDescription(methodSymbol, parameter.Name);

            sb.AppendLine($"        [\"{parameter.Name}\"] = new GptToolParameter");
            sb.AppendLine("        {");
            sb.AppendLine($"            Type = PropertyType.{type},");
            sb.AppendLine($"            Description = \"{description}\",");
            sb.AppendLine($"            Index = {index++},");


            if (IsEnumType(parameter.Type))
            {
                var enumValues = GetEnumValues(parameter.Type);
                sb.AppendLine(
                    $"            Enum = new List<string> {{ {string.Join(", ", enumValues.Select(v => $"\"{v}\""))} }},");
            }

            sb.AppendLine(
                $"            Required = {(!IsOptionalOrExplicitNullable(parameter)).ToString().ToLowerInvariant()}");
            sb.AppendLine("        },");
        }

        sb.AppendLine("    }");
        sb.AppendLine("};");

        PrependStringBuilder(sb, 3);

        return sb.ToString();
    }

    private static void PrependStringBuilder(StringBuilder sb, int indent, int indentSize = 4)
    {
        var lines = sb.ToString().Split('\n').Select(l => new string(' ', indentSize * indent) + l).ToArray();
        sb.Clear();
        for (var i = 0; i < lines.Length - 1; i++)
        {
            var line = lines[i];
            if (i == lines.Length - 2)
                sb.Append(line);
            else
                sb.AppendLine(line);
        }
    }

    private string GenerateInternalClassWithParameters(IMethodSymbol methodSymbol)
    {
        var sb = new StringBuilder();

        sb.AppendLine($$"""
                      public struct {{InternalParameterClassName}}
                      {
                      """);

        foreach (var parameter in methodSymbol.Parameters)
        {
            var type = parameter.Type.ToDisplayString();

            sb.AppendLine($"    public {type} {parameter.Name} {{ get; set; }}");
        }

        sb.AppendLine("}");

        PrependStringBuilder(sb, 2);

        return sb.ToString();
    }

    private string GenerateMethodCallFromString(IMethodSymbol methodSymbol)
    {
        // Generate the method call code, the parameters class is just for easier access to the parameters
        // The method call must use the actual parameters
        var sb = new StringBuilder();
        
        sb.AppendLine($"public override async Task<CallExpressionResult> CallExpressionInternal(string jsonParameters, Func<string, Type, object> deserialize)");
        sb.AppendLine("{");
        sb.AppendLine($"    var parameters = ({InternalParameterClassName})deserialize(jsonParameters, typeof({InternalParameterClassName}));");
        sb.AppendLine("    return await CallExpression(");
        foreach (var parameter in methodSymbol.Parameters)
        {
            sb.Append($"        parameters.{parameter.Name}");
            if (!SymbolEqualityComparer.Default.Equals(parameter, methodSymbol.Parameters.Last()))
                sb.AppendLine(",");
        }

        sb.AppendLine();
        sb.AppendLine("    );");
        sb.AppendLine("}");
        
        PrependStringBuilder(sb, 2);
        
        return sb.ToString();
    }

    private bool IsOptionalOrExplicitNullable(IParameterSymbol parameter)
    {
        return parameter.IsOptional || ((INamedTypeSymbol)parameter.Type).ConstructedFrom.SpecialType ==
            SpecialType.System_Nullable_T;
    }

    private bool ImplementsInterface(INamedTypeSymbol classSymbol, string interfaceName)
    {
        return classSymbol.AllInterfaces.Any(i => i.Name == interfaceName);
    }

    private bool InheritsFrom(INamedTypeSymbol classSymbol, INamedTypeSymbol baseTypeSymbol)
    {
        var baseType = classSymbol.BaseType;

        while (baseType != null)
        {
            if (SymbolEqualityComparer.Default.Equals(baseType, baseTypeSymbol))
                return true;

            baseType = baseType.BaseType;
        }

        return false;
    }

    private bool IsEnumType(ITypeSymbol typeSymbol)
    {
        if (typeSymbol is INamedTypeSymbol namedTypeSymbol &&
            namedTypeSymbol.ConstructedFrom.SpecialType == SpecialType.System_Nullable_T)
            typeSymbol = namedTypeSymbol.TypeArguments[0];
        
        return typeSymbol.TypeKind == TypeKind.Enum;
    }

    private IEnumerable<string> GetEnumValues(ITypeSymbol typeSymbol)
    {
        // If type is nullable, get the underlying type
        if (typeSymbol is INamedTypeSymbol namedTypeSymbol &&
            namedTypeSymbol.ConstructedFrom.SpecialType == SpecialType.System_Nullable_T)
            typeSymbol = namedTypeSymbol.TypeArguments[0];
        
        var enumMembers = typeSymbol.GetMembers().OfType<IFieldSymbol>().Where(f => f.IsConst && f.HasConstantValue)
            .Select(f => f.Name);

        return enumMembers;
    }

    private string GetPropertyType(ITypeSymbol typeSymbol)
    {
        // If type is nullable, get the underlying type
        if (typeSymbol is INamedTypeSymbol namedTypeSymbol &&
            namedTypeSymbol.ConstructedFrom.SpecialType == SpecialType.System_Nullable_T)
            typeSymbol = namedTypeSymbol.TypeArguments[0];

        switch (typeSymbol.SpecialType)
        {
            case SpecialType.System_String:
                return "String";
            case SpecialType.System_Int32:
            case SpecialType.System_Int64:
            case SpecialType.System_Double:
            case SpecialType.System_Single:
            case SpecialType.System_Decimal:
                return "Number";
            case SpecialType.System_Boolean:
                return "Boolean";
            default:
                if (typeSymbol.TypeKind == TypeKind.Enum)
                    return "String"; // Enums are represented as strings in the parameters
                if (typeSymbol.TypeKind == TypeKind.Array)
                    return "Array";
                return "Object"; // Default to Object
        }
    }
    
    private string GetParameterDescription(IMethodSymbol methodSymbol, string parameterName)
    {
        var syntaxReference = methodSymbol.DeclaringSyntaxReferences.FirstOrDefault();
        if (syntaxReference == null) return string.Empty;

        var syntax = syntaxReference.GetSyntax() as MethodDeclarationSyntax;
        if (syntax == null) return string.Empty;

        var triviaList = syntax.GetLeadingTrivia();

        var xmlTrivia = triviaList.FirstOrDefault(trivia =>
            trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia) ||
            trivia.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia));

        if (xmlTrivia == default) return string.Empty;

        var structure = xmlTrivia.GetStructure() as DocumentationCommentTriviaSyntax;
        if (structure == null) return string.Empty;

        var paramElements = structure.Content.OfType<XmlElementSyntax>()
            .Where(element =>
                element.StartTag.Name.LocalName.Text == "param");

        foreach (var param in paramElements)
        {
            var paramText = string.Concat(param.Content
                .OfType<XmlTextSyntax>()
                .SelectMany(textSyntax => textSyntax.TextTokens)
                .Where(token => token.Kind() == SyntaxKind.XmlTextLiteralToken)
                .Select(token => token.ValueText));

            var nameAttribute = param.StartTag.Attributes
                .OfType<XmlNameAttributeSyntax>()
                .FirstOrDefault(attr => attr.Name.LocalName.Text == "name");

            if (nameAttribute != null && nameAttribute.Identifier.Identifier.Text == parameterName)
                return NormalizeWhiteSpaceForLoop(paramText.Trim());
        }

        return string.Empty;
    }

    private string GetMethodDescription(IMethodSymbol methodSymbol)
    {
        var syntaxReference = methodSymbol.DeclaringSyntaxReferences.FirstOrDefault();
        if (syntaxReference == null) return string.Empty;

        var syntax = syntaxReference.GetSyntax() as MethodDeclarationSyntax;
        if (syntax == null) return string.Empty;

        var triviaList = syntax.GetLeadingTrivia();

        var xmlTrivia = triviaList.FirstOrDefault(trivia =>
            trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia) ||
            trivia.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia));

        if (xmlTrivia == default) return string.Empty;

        var structure = xmlTrivia.GetStructure() as DocumentationCommentTriviaSyntax;
        if (structure == null) return string.Empty;

        var summaryElement = structure.Content.OfType<XmlElementSyntax>()
            .FirstOrDefault(element =>
                element.StartTag.Name.LocalName.Text == "summary");

        if (summaryElement != null)
        {
            // Extract the text content within <summary> tags without leading slashes or extra whitespaces
            var summaryText = string.Concat(summaryElement.Content
                .OfType<XmlTextSyntax>()
                .SelectMany(textSyntax => textSyntax.TextTokens)
                .Where(token => token.Kind() == SyntaxKind.XmlTextLiteralToken)
                .Select(token => token.ValueText));

            summaryText = summaryText.Trim();

            return NormalizeWhiteSpaceForLoop(summaryText);
        }

        return string.Empty;
    }

    public static string NormalizeWhiteSpaceForLoop(string input)
    {
        int len = input.Length,
            index = 0,
            i = 0;
        var src = input.ToCharArray();
        var skip = false;
        char ch;
        for (; i < len; i++)
        {
            ch = src[i];
            switch (ch)
            {
                case '\u0020':
                case '\u00A0':
                case '\u1680':
                case '\u2000':
                case '\u2001':
                case '\u2002':
                case '\u2003':
                case '\u2004':
                case '\u2005':
                case '\u2006':
                case '\u2007':
                case '\u2008':
                case '\u2009':
                case '\u200A':
                case '\u202F':
                case '\u205F':
                case '\u3000':
                case '\u2028':
                case '\u2029':
                case '\u0009':
                case '\u000A':
                case '\u000B':
                case '\u000C':
                case '\u000D':
                case '\u0085':
                    if (skip) continue;
                    src[index++] = ch;
                    skip = true;
                    continue;
                default:
                    skip = false;
                    src[index++] = ch;
                    continue;
            }
        }

        return new string(src, 0, index);
    }
}