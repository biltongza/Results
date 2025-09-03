using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using Results.Lib.Analyzers;

namespace Results.Lib
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class CheckAllResultTypes : DiagnosticAnalyzer
    {
        private const string RuleId = "R1000";
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(_rule);

        private static readonly DiagnosticDescriptor _rule = new DiagnosticDescriptor(
            RuleId,
            "Switch expression does not handle all possible result types",
            "Switch expression does not handle the result {0} result type",
            "Usage",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);


        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterCompilationStartAction(AnalyzeCompilationStart);
        }

        private void AnalyzeCompilationStart(CompilationStartAnalysisContext context)
        {
            INamedTypeSymbol resultInterfaceTypeSymbol = context.Compilation.GetTypeByMetadataName(WellKnownTypeNames.IResult);
            INamedTypeSymbol genericResult2TypeSymbol = context.Compilation.GetTypeByMetadataName(WellKnownTypeNames.GenericResult2);
            context.RegisterOperationAction((operationContext) =>
            {
                var switchExpression = (ISwitchExpressionOperation)operationContext.Operation;
                if (!ShouldDiagnose(switchExpression, resultInterfaceTypeSymbol))
                {
                    return;
                }
                AnalyzeSwitchExpression(operationContext);
            }, OperationKind.SwitchExpression);
        }

        private void AnalyzeSwitchExpression(OperationAnalysisContext context)
        {
            ISwitchExpressionOperation switchExpression = (ISwitchExpressionOperation)context.Operation;

            var resultType = switchExpression.Value.Type;

            INamedTypeSymbol sourceSymbol;

            if(switchExpression.Value is IMemberReferenceOperation memberReferenceOperation)
            {
                sourceSymbol = memberReferenceOperation.Member.ContainingType;
            }
            else
            {
                sourceSymbol = switchExpression.Type as INamedTypeSymbol;
            }

            var returnTypes = sourceSymbol.TypeArguments;

            foreach(var returnType in returnTypes)
            {
                var matchedArm = switchExpression.Arms.Any(arm => SymbolEqualityComparer.Default.Equals(arm.Pattern.NarrowedType, returnType));
                if(!matchedArm)
                {
                    context.ReportDiagnostic(Diagnostic.Create(_rule, context.Operation.Syntax.GetLocation(), returnType));
                }
            }
        }

        private bool ShouldDiagnose(
            ISwitchExpressionOperation operation,
            INamedTypeSymbol resultInterfaceTypeSymbol
        ) => SymbolEqualityComparer.Default.Equals(operation.Value.Type, resultInterfaceTypeSymbol);
    }
}
