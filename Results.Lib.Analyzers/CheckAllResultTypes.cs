using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
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
            "Switch expression does not handle type {0}",
            "Usage",
            DiagnosticSeverity.Error,
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
                if (!ShouldDiagnose(switchExpression, resultInterfaceTypeSymbol, genericResult2TypeSymbol))
                {
                    return;
                }

                operationContext.ReportDiagnostic(Diagnostic.Create(_rule, operationContext.Operation.Syntax.GetLocation(), resultInterfaceTypeSymbol.MetadataName));
            }, OperationKind.SwitchExpression);
        }

        private void AnalyzeSwitchExpression(OperationAnalysisContext context)
        {
            ISwitchExpressionOperation switchExpression = (ISwitchExpressionOperation)context.Operation;

            if (switchExpression.Value.Type.MetadataName != "Results.Lib.IResult")
            {
                return;
            }

            var accessor = switchExpression.Value as IMemberReferenceOperation;
            if (accessor == null)
            {
                return;
            }

            if (accessor.Member.MetadataName != "Results.Lib.Result`2")
            {
                return;
            }

            var t = accessor.SemanticModel.GetTypeInfo(accessor.Syntax);

            // we should be a result type now!
            //var typeParams = t.Type.;
            // foreach (var typeParam in typeParams)
            // {
            //     if (!switchExpression.Arms.Any(x => x.Pattern.NarrowedType.MetadataName.Equals(typeParam.MetadataName)))
            //     {
            //         context.ReportDiagnostic(Diagnostic.Create(_rule, context.Operation.Syntax.GetLocation(), typeParam.MetadataName));
            //     }
            // }
        }

        private bool ShouldDiagnose(
            ISwitchExpressionOperation operation,
            INamedTypeSymbol resultInterfaceTypeSymbol,
            INamedTypeSymbol genericResultsTypeSymbol
            )
        {
            if (!SymbolEqualityComparer.Default.Equals(operation.Value.Type, resultInterfaceTypeSymbol))
            {
                return false;
            }
            if (operation.Value is IMemberReferenceOperation memberReferenceOperation
                && SymbolEqualityComparer.Default.Equals(memberReferenceOperation.Member., genericResultsTypeSymbol))
            {
                return true;
            }
            return false;
        }
    }
}
