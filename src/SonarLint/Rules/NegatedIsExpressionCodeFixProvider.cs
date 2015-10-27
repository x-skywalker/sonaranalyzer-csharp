﻿/*
 * SonarLint for Visual Studio
 * Copyright (C) 2015 SonarSource
 * sonarqube@googlegroups.com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02
 */

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.CodeAnalysis.CodeActions;
using System.Threading;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace SonarLint.Rules
{
    [ExportCodeFixProvider(LanguageNames.VisualBasic)]
    public class NegatedIsExpressionCodeFixProvider : CodeFixProvider
    {
        internal const string Title = "Replace \"Not...Is...\" with \"IsNot\".";
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(NegatedIsExpression.DiagnosticId);
            }
        }
        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var unary = root.FindNode(diagnosticSpan, getInnermostNodeForTie: true) as UnaryExpressionSyntax;
            if (unary == null)
            {
                return;
            }

            var isExpression = unary.Operand as BinaryExpressionSyntax;
            if (isExpression == null ||
                !isExpression.IsKind(SyntaxKind.IsExpression))
            {
                return;
            }

            context.RegisterCodeFix(
                CodeAction.Create(
                    Title,
                    c => ChangeToIsNot(context.Document, unary, isExpression, c)),
                context.Diagnostics);
        }

        private static async Task<Document> ChangeToIsNot(Document document, UnaryExpressionSyntax unary, BinaryExpressionSyntax isExpression,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken);
            var newRoot = root.ReplaceNode(
                unary,
                SyntaxFactory.BinaryExpression(
                    SyntaxKind.IsNotExpression,
                    isExpression.Left,
                    SyntaxFactory.Token(SyntaxKind.IsNotKeyword).WithTriviaFrom(isExpression.OperatorToken),
                    isExpression.Right));
            return document.WithSyntaxRoot(newRoot);
        }
    }
}
