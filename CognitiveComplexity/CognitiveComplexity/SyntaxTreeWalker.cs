using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace CognitiveComplexity
{
    // if parent is a block or another syntax listed here we are nested
    // keep count of current nesting 
    // nesting is additive for each nested structure but not a return / break

    // todo: handle ternaries

    internal class SyntaxTreeWalker : CSharpSyntaxWalker
    {
        private int _complexity = 0;
        private List<ExpressionSyntax> _previouslyVisitedExpressions = new List<ExpressionSyntax>();

        public SyntaxTreeWalker() : base(Microsoft.CodeAnalysis.SyntaxWalkerDepth.Node)
        {
        }

        public int GetComplexity(CSharpSyntaxNode node)
        {
            this._previouslyVisitedExpressions.Clear();
            this.Visit(node);
            return _complexity;
        }

        public override void VisitSwitchStatement(SwitchStatementSyntax node)
        {
            this.IncrementForNesting(node);
            _complexity++;
            base.VisitSwitchStatement(node);
        }

        public override void VisitIfStatement(IfStatementSyntax node)
        {
            // todo: may need to handle else if / else
            this.IncrementForNesting(node);
            _complexity++;
            base.VisitIfStatement(node);
        }

        public override void VisitConditionalExpression(ConditionalExpressionSyntax node)
        {
            this.IncrementForNesting(node);
            _complexity++;
            base.VisitConditionalExpression(node);
        }

        public override void VisitForStatement(ForStatementSyntax node)
        {
            this.IncrementForNesting(node);
            _complexity++;
            base.VisitForStatement(node);
        }

        public override void VisitForEachStatement(ForEachStatementSyntax node)
        {
            this.IncrementForNesting(node);
            _complexity++;
            base.VisitForEachStatement(node);
        }

        public override void VisitWhileStatement(WhileStatementSyntax node)
        {
            this.IncrementForNesting(node);
            _complexity++;
            base.VisitWhileStatement(node);
        }

        public override void VisitDoStatement(DoStatementSyntax node)
        {
            this.IncrementForNesting(node);
            _complexity++;
            base.VisitDoStatement(node);
        }

        public override void VisitCatchDeclaration(CatchDeclarationSyntax node)
        {
            // get the try statement since catches are always in trystatement
            var tryStatement = node.FirstAncestorOrSelf<TryStatementSyntax>();
            this.IncrementForNesting(tryStatement);
            _complexity++;
            base.VisitCatchDeclaration(node);
        }

        public override void VisitBreakStatement(BreakStatementSyntax node)
        {
            // this one is tricky for switches.
            // need to make sure break is not in switch statement
            if (!(node.Parent is SwitchSectionSyntax))
            {
                // do not incremement for break since it does not result in new structure
                _complexity++;
            }
            else if (true)
            {
                // nothing 
            }
            base.VisitBreakStatement(node);
        }

        public override void VisitContinueStatement(ContinueStatementSyntax node)
        {
            // do not incremement for continue since it does not result in new structure
            _complexity++;
            base.VisitContinueStatement(node);
        }

        public override void VisitBinaryExpression(BinaryExpressionSyntax node)
        {
            if (!_previouslyVisitedExpressions.Contains(node) &&
                node.Kind() == SyntaxKind.LogicalOrExpression ||
                node.Kind() == SyntaxKind.LogicalAndExpression)
            {
                var flattenedExpressions = node.FlattenBinaryExpression(_previouslyVisitedExpressions).Where(n => n != null);
                BinaryExpressionSyntax prev = null;
                foreach (var currentExpression in flattenedExpressions)
                {
                    if (prev == null || prev.Kind() != currentExpression.Kind())
                    {
                        _complexity++;
                    }
                    prev = currentExpression;
                }
            }
            base.VisitBinaryExpression(node);
        }


        private void IncrementForNesting(CSharpSyntaxNode node)
        {
            var nestingLevel = node.Ancestors().Count(n =>
            {
                return n is IfStatementSyntax||
                       n is ForStatementSyntax ||
                       n is ForEachStatementSyntax ||
                       n is SwitchStatementSyntax ||
                       n is WhileStatementSyntax ||
                       n is DoStatementSyntax ||
                       n is CatchClauseSyntax ||
                       n is LambdaExpressionSyntax;
            });
            _complexity += nestingLevel;
        }
    }
}
