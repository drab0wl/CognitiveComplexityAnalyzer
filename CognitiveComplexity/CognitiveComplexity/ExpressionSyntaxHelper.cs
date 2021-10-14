using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using System.Linq;

namespace CognitiveComplexity
{
    internal static class ExpressionSyntaxExtensions
    {
        public static IEnumerable<BinaryExpressionSyntax> FlattenBinaryExpression(this ExpressionSyntax expression, ICollection<ExpressionSyntax> ignoredExpressions)
        {
            if (expression != null &&
                !ignoredExpressions.Contains(expression) &&
                expression.Kind() == SyntaxKind.LogicalAndExpression ||
                expression.Kind() == SyntaxKind.LogicalOrExpression)
            {
                ignoredExpressions.Add(expression);
                var binaryExpression = expression as BinaryExpressionSyntax;
                if (binaryExpression != null)
                {
                    var left = SkipParenthesizedExpression(binaryExpression.Left);
                    var right = SkipParenthesizedExpression(binaryExpression.Right);
                    return FlattenBinaryExpression(left, ignoredExpressions).Concat(new[] { binaryExpression }).Concat(FlattenBinaryExpression(right, ignoredExpressions));
                }
            }
            return Enumerable.Empty<BinaryExpressionSyntax>();
        }

        private static ExpressionSyntax SkipParenthesizedExpression(ExpressionSyntax expressionSyntax)
        {
            ExpressionSyntax result = expressionSyntax;
            while (result.Kind() == SyntaxKind.ParenthesizedExpression)
            {
                result = ((ParenthesizedExpressionSyntax)result).Expression;
            }
            return result;
        }
    }
}
