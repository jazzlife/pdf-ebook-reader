using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;

namespace BookReader.Base.ViewModel
{
    public static class MvvmUtils
    {
        /// <summary>
        /// Get the property name from a lambda expression like () => PropertyName
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static string GetPropertyName<T>(Expression<Func<T>> expression)
        {
            if (expression.NodeType == ExpressionType.Lambda)
            {
                var memberEx = expression.Body as MemberExpression;
                if (memberEx != null &&
                    memberEx.Member.MemberType == MemberTypes.Property)
                {
                    return memberEx.Member.Name;
                }
            }
            throw new ArgumentException("Argument must be a lambda expression like () => PropertyName");
        }

    }
}
