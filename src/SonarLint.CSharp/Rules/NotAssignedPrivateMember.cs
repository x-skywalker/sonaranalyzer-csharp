﻿/*
 * SonarLint for Visual Studio
 * Copyright (C) 2015-2016 SonarSource SA
 * mailto:contact@sonarsource.com
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

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarLint.Common;
using SonarLint.Common.Sqale;
using SonarLint.Helpers;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace SonarLint.Rules.CSharp
{
    using MemberUsage = SyntaxNodeSymbolSemanticModelTuple<SimpleNameSyntax, ISymbol>;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [SqaleConstantRemediation("5min")]
    [SqaleSubCharacteristic(SqaleSubCharacteristic.DataReliability)]
    [Rule(DiagnosticId, RuleSeverity, Title, IsActivatedByDefault)]
    [Tags(Tag.Confusing)]
    public class NotAssignedPrivateMember : SonarDiagnosticAnalyzer
    {
        /*
         CS0649 reports the same on internal fields. So that's wider in scope, but that's not a live Roslyn analyzer,
         the issue only shows up at build time and not during editing.
        */

        internal const string DiagnosticId = "S3459";
        internal const string Title = "Unassigned members should be removed";
        internal const string Description =
            "Fields and auto-properties that are never assigned to hold the default values for their types. They are either pointless code or, more likely, mistakes.";
        internal const string MessageFormat = "Remove unassigned {0} \"{1}\", or set its value.";
        internal const string Category = SonarLint.Common.Category.Reliability;
        internal const Severity RuleSeverity = Severity.Major;
        internal const bool IsActivatedByDefault = true;

        internal static readonly DiagnosticDescriptor Rule =
            new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category,
                RuleSeverity.ToDiagnosticSeverity(), IsActivatedByDefault,
                helpLinkUri: DiagnosticId.GetHelpLink(),
                description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        private static readonly Accessibility maxAccessibility = Accessibility.Private;

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSymbolAction(
                c =>
                {
                    var namedType = (INamedTypeSymbol)c.Symbol;
                    if (!namedType.IsClassOrStruct() ||
                        namedType.ContainingType != null)
                    {
                        return;
                    }

                    var removableDeclarationCollector = new RemovableDeclarationCollector(namedType, c.Compilation);

                    var candidateFields = removableDeclarationCollector.GetRemovableFieldLikeDeclarations(
                        ImmutableHashSet.Create(SyntaxKind.FieldDeclaration), maxAccessibility)
                        .Where(tuple => ((VariableDeclaratorSyntax)tuple.SyntaxNode).Initializer == null);

                    var candidateProperties = removableDeclarationCollector.GetRemovableDeclarations(
                        ImmutableHashSet.Create(SyntaxKind.PropertyDeclaration), maxAccessibility)
                        .Where(tuple => IsAutoPropertyWithNoInitializer((PropertyDeclarationSyntax)tuple.SyntaxNode));

                    var allCandidateMembers = candidateFields.Concat(candidateProperties).ToList();
                    if (!allCandidateMembers.Any())
                    {
                        return;
                    }

                    var usedMembers = GetMemberUsages(removableDeclarationCollector, new HashSet<ISymbol>(allCandidateMembers.Select(t => t.Symbol)));
                    var usedMemberSymbols = new HashSet<ISymbol>(usedMembers.Select(tuple => tuple.Symbol));

                    var assignedMemberSymbols = GetAssignedMemberSymbols(usedMembers);

                    foreach (var candidateMember in allCandidateMembers)
                    {
                        if (!usedMemberSymbols.Contains(candidateMember.Symbol))
                        {
                            /// reported by <see cref="UnusedPrivateMember"/>
                            continue;
                        }

                        if (!assignedMemberSymbols.Contains(candidateMember.Symbol))
                        {
                            var field = candidateMember.SyntaxNode as VariableDeclaratorSyntax;
                            var property = candidateMember.SyntaxNode as PropertyDeclarationSyntax;

                            var memberType = field != null ? "field" : "auto-property";

                            var location = field != null
                                ? field.Identifier.GetLocation()
                                : property.Identifier.GetLocation();

                            c.ReportDiagnosticIfNonGenerated(Diagnostic.Create(Rule, location, memberType, candidateMember.Symbol.Name));
                        }
                    }
                },
                SymbolKind.NamedType);
        }

        private static bool IsAutoPropertyWithNoInitializer(PropertyDeclarationSyntax declaration)
        {
            return declaration.Initializer == null &&
                declaration.AccessorList != null &&
                declaration.AccessorList.Accessors.All(acc => acc.Body == null);
        }

        private static IList<MemberUsage> GetMemberUsages(RemovableDeclarationCollector removableDeclarationCollector,
            HashSet<ISymbol> declaredPrivateSymbols)
        {
            var symbolNames = declaredPrivateSymbols.Select(s => s.Name).ToImmutableHashSet();

            var identifiers = removableDeclarationCollector.ClassDeclarations
                .SelectMany(container => container.SyntaxNode.DescendantNodes()
                    .Where(node =>
                        node.IsKind(SyntaxKind.IdentifierName))
                    .Cast<IdentifierNameSyntax>()
                    .Where(node => symbolNames.Contains(node.Identifier.ValueText))
                    .Select(node =>
                        new MemberUsage
                        {
                            SyntaxNode = node,
                            SemanticModel = container.SemanticModel,
                            Symbol = container.SemanticModel.GetSymbolInfo(node).Symbol
                        }));

            var generic = removableDeclarationCollector.ClassDeclarations
                .SelectMany(container => container.SyntaxNode.DescendantNodes()
                    .Where(node =>
                        node.IsKind(SyntaxKind.GenericName))
                    .Cast<GenericNameSyntax>()
                    .Where(node => symbolNames.Contains(node.Identifier.ValueText))
                    .Select(node =>
                        new MemberUsage
                        {
                            SyntaxNode = node,
                            SemanticModel = container.SemanticModel,
                            Symbol = container.SemanticModel.GetSymbolInfo(node).Symbol
                        }));

            return identifiers.Concat(generic)
                .Where(tuple => tuple.Symbol is IFieldSymbol || tuple.Symbol is IPropertySymbol)
                .ToList();
        }

        private static ISet<ISymbol> GetAssignedMemberSymbols(IList<MemberUsage> memberUsages)
        {
            var assignedMembers = new HashSet<ISymbol>();

            foreach (var memberUsage in memberUsages)
            {
                ExpressionSyntax node = memberUsage.SyntaxNode;
                var memberSymbol = memberUsage.Symbol;

                // handle "this.FieldName"
                var simpleMemberAccess = node.Parent as MemberAccessExpressionSyntax;
                if (simpleMemberAccess != null &&
                    simpleMemberAccess.Name == node)
                {
                    node = simpleMemberAccess;
                }

                // handle "((this.FieldName))"
                node = node.GetSelfOrTopParenthesizedExpression();

                var parentNode = node.Parent;

                if (PreOrPostfixOpSyntaxKinds.Contains(parentNode.Kind()))
                {
                    assignedMembers.Add(memberSymbol);
                    continue;
                }

                var assignment = parentNode as AssignmentExpressionSyntax;
                if (assignment != null)
                {
                    if (assignment.Left == node)
                    {
                        assignedMembers.Add(memberSymbol);
                    }

                    continue;
                }

                var argument = parentNode as ArgumentSyntax;
                if (argument != null)
                {
                    if (!argument.RefOrOutKeyword.IsKind(SyntaxKind.None))
                    {
                        assignedMembers.Add(memberSymbol);
                    }

                    continue;
                }
            }

            return assignedMembers;
        }

        private static readonly ISet<SyntaxKind> PreOrPostfixOpSyntaxKinds = ImmutableHashSet.Create(
            SyntaxKind.PostDecrementExpression,
            SyntaxKind.PostIncrementExpression,
            SyntaxKind.PreDecrementExpression,
            SyntaxKind.PreIncrementExpression);
    }
}
