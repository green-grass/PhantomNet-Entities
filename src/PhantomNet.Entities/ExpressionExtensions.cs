﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace System.Linq.Expressions
{
    // Coppied from EntityFrameworkCore
    // https://github.com/aspnet/EntityFrameworkCore/blob/dev/src/Microsoft.EntityFrameworkCore/Extensions/Internal/ExpressionExtensions.cs
    internal static class ExpressionExtensions
    {
        public static PropertyInfo GetPropertyAccess(this LambdaExpression propertyAccessExpression)
        {
            Debug.Assert(propertyAccessExpression.Parameters.Count == 1);

            var parameterExpression = propertyAccessExpression.Parameters.Single();
            var propertyInfo = parameterExpression.MatchSimplePropertyAccess(propertyAccessExpression.Body);

            if (propertyInfo == null)
            {
                throw new ArgumentException(
                    /* // TODO:: CoreStrings.InvalidPropertyExpression(propertyAccessExpression),
                    nameof(propertyAccessExpression)*/);
            }

            var declaringType = propertyInfo.DeclaringType;
            var parameterType = parameterExpression.Type;

            if (declaringType != null
                && declaringType != parameterType
                && declaringType.GetTypeInfo().IsInterface
                && declaringType.IsAssignableFrom(parameterType))
            {
                var propertyGetter = propertyInfo.GetGetMethod(true);
                var interfaceMapping = parameterType.GetTypeInfo().GetRuntimeInterfaceMap(declaringType);
                var index = Array.FindIndex(interfaceMapping.InterfaceMethods, p => p == propertyGetter);
                var targetMethod = interfaceMapping.TargetMethods[index];
                foreach (var runtimeProperty in parameterType.GetRuntimeProperties())
                {
                    if (targetMethod == runtimeProperty.GetGetMethod(true))
                    {
                        return runtimeProperty;
                    }
                }
            }

            return propertyInfo;
        }

        public static IReadOnlyList<PropertyInfo> GetPropertyAccessList(this LambdaExpression propertyAccessExpression)
        {
            Debug.Assert(propertyAccessExpression.Parameters.Count == 1);

            var propertyPaths
                = MatchPropertyAccessList(propertyAccessExpression, (p, e) => e.MatchSimplePropertyAccess(p));

            if (propertyPaths == null)
            {
                throw new ArgumentException(
                    /* // TODO:: CoreStrings.InvalidPropertiesExpression(propertyAccessExpression),
                    nameof(propertyAccessExpression)*/);
            }

            return propertyPaths;
        }

        private static IReadOnlyList<PropertyInfo> MatchPropertyAccessList(
            this LambdaExpression lambdaExpression, Func<Expression, Expression, PropertyInfo> propertyMatcher)
        {
            Debug.Assert(lambdaExpression.Body != null);

            var newExpression
                = RemoveConvert(lambdaExpression.Body) as NewExpression;

            var parameterExpression
                = lambdaExpression.Parameters.Single();

            if (newExpression != null)
            {
                var propertyInfos
                    = newExpression
                        .Arguments
                        .Select(a => propertyMatcher(a, parameterExpression))
                        .Where(p => p != null)
                        .ToList();

                return propertyInfos.Count != newExpression.Arguments.Count ? null : propertyInfos;
            }

            var propertyPath
                = propertyMatcher(lambdaExpression.Body, parameterExpression);

            return propertyPath != null ? new[] { propertyPath } : null;
        }

        private static PropertyInfo MatchSimplePropertyAccess(
            this Expression parameterExpression, Expression propertyAccessExpression)
        {
            var propertyInfos = MatchPropertyAccess(parameterExpression, propertyAccessExpression);

            return (propertyInfos != null) && (propertyInfos.Length == 1) ? propertyInfos[0] : null;
        }

        public static PropertyInfo[] GetComplexPropertyAccess(this LambdaExpression propertyAccessExpression)
        {
            Debug.Assert(propertyAccessExpression.Parameters.Count == 1);

            var propertyPath
                = propertyAccessExpression
                    .Parameters
                    .Single()
                    .MatchPropertyAccess(propertyAccessExpression.Body);

            if (propertyPath == null)
            {
                throw new ArgumentException(
                    /* // TODO:: CoreStrings.InvalidComplexPropertyExpression(propertyAccessExpression)*/);
            }

            return propertyPath;
        }

        private static PropertyInfo[] MatchPropertyAccess(
            this Expression parameterExpression, Expression propertyAccessExpression)
        {
            var propertyInfos = new List<PropertyInfo>();

            MemberExpression memberExpression;

            do
            {
                memberExpression = RemoveConvert(propertyAccessExpression) as MemberExpression;

                var propertyInfo = memberExpression?.Member as PropertyInfo;

                if (propertyInfo == null)
                {
                    return null;
                }

                propertyInfos.Insert(0, propertyInfo);

                propertyAccessExpression = memberExpression.Expression;
            }
            while (memberExpression.Expression.RemoveConvert() != parameterExpression);

            return propertyInfos.ToArray();
        }

        public static Expression RemoveConvert(this Expression expression)
        {
            while ((expression != null)
                   && ((expression.NodeType == ExpressionType.Convert)
                       || (expression.NodeType == ExpressionType.ConvertChecked)))
            {
                expression = RemoveConvert(((UnaryExpression)expression).Operand);
            }

            return expression;
        }

        public static TExpression GetRootExpression<TExpression>(this Expression expression)
            where TExpression : Expression
        {
            MemberExpression memberExpression;
            while ((memberExpression = expression as MemberExpression) != null)
            {
                expression = memberExpression.Expression;
            }

            return expression as TExpression;
        }

        public static bool IsLogicalOperation(this Expression expression)
        {
            // TODO:: Check.NotNull(expression, nameof(expression));

            return (expression.NodeType == ExpressionType.AndAlso)
                   || (expression.NodeType == ExpressionType.OrElse);
        }

        public static bool IsComparisonOperation(this Expression expression)
        {
            // TODO:: Check.NotNull(expression, nameof(expression));

            return (expression.NodeType == ExpressionType.Equal)
                   || (expression.NodeType == ExpressionType.NotEqual)
                   || (expression.NodeType == ExpressionType.LessThan)
                   || (expression.NodeType == ExpressionType.LessThanOrEqual)
                   || (expression.NodeType == ExpressionType.GreaterThan)
                   || (expression.NodeType == ExpressionType.GreaterThanOrEqual)
                   || (expression.NodeType == ExpressionType.Not);
        }
    }
}
