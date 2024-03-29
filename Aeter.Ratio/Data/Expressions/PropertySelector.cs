﻿/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
namespace Aeter.Ratio.Expressions
{
    public static class PropertySelector
    {
        public static PropertyInfo GetProperty<T, TProperty>(Expression<Func<T, TProperty>> propertyExpression)
        {
            var memberExpression = RequireMemberExpression(propertyExpression.Body);
            return GetPropertyInfo(memberExpression);
        }

        public static IEnumerable<PropertyInfo> GetPropertyPath<T, TProperty>(Expression<Func<T, TProperty>> propertyExpression)
        {
            var memberExpression = RequireMemberExpression(propertyExpression.Body);
            var properties = new List<PropertyInfo>();
            while (true) {
                properties.Add(GetPropertyInfo(memberExpression));
                if (memberExpression.Expression == null)
                    break;

                if (memberExpression.Expression.NodeType == ExpressionType.Parameter)
                    break;

                memberExpression = RequireMemberExpression(memberExpression.Expression);
            }
            properties.Reverse();
            return properties;
        }

        private static MemberExpression RequireMemberExpression(Expression expression)
        {
            var memberExpression = expression as MemberExpression;
            if (memberExpression == null)
                throw new ArgumentException("Expression does not specify a property");
            return memberExpression;
        }

        private static PropertyInfo GetPropertyInfo(MemberExpression expression)
        {
            var propertyInfo = expression.Member as PropertyInfo;
            if (propertyInfo == null)
                throw new ArgumentException("Expression does not specify a property");
            return propertyInfo;
        }

    }
}
