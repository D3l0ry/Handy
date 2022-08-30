﻿using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Data.SqlClient;

namespace System.Linq
{
    public static class DatabaseQueryable
    {
        #region MethodHelper
        private static MethodInfo GetMethodInfo<T1, T2>(Func<T1, T2> func, T1 unused) => func.Method;

        private static MethodInfo GetMethodInfo<T1, T2, T3>(Func<T1, T2, T3> func, T1 unused, T2 unused2) => func.Method;

        private static MethodInfo GetMethodInfo<T1, T2, T3, T4>(Func<T1, T2, T3, T4> func, T1 unused, T2 unused2, T3 unused3) => func.Method;

        private static MethodInfo GetMethodInfo<T1, T2, T3, T4, T5>(Func<T1, T2, T3, T4, T5> func, T1 unused, T2 unused2, T3 unused3, T4 unused4) => func.Method;

        private static MethodInfo GetMethodInfo<T1, T2, T3, T4, T5, T6>(Func<T1, T2, T3, T4, T5, T6> func, T1 unused, T2 unused2, T3 unused3, T4 unused4, T5 unused5) => func.Method;

        private static MethodInfo GetMethodInfo<T1, T2, T3, T4, T5, T6, T7>(Func<T1, T2, T3, T4, T5, T6, T7> func, T1 unused, T2 unused2, T3 unused3, T4 unused4, T5 unused5, T6 unused6) => func.Method;
        #endregion

        internal static void CheckDataValue(this SqlDataReader dataReader, Expression expression)
        {
            if (dataReader is null)
            {
                throw new ArgumentNullException(nameof(dataReader));
            }

            if (expression is null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            MethodCallExpression methodCallExpression = expression as MethodCallExpression;

            if (methodCallExpression is null)
            {
                return;
            }

            string methodName = methodCallExpression.Method.Name;

            switch (methodName)
            {
                case "First" when !dataReader.HasRows:
                    throw new InvalidOperationException("Исходная последовательность пуста");
            }
        }

        public static IDatabaseQueryable<TSource> Where<TSource>(this IDatabaseQueryable<TSource> source, Expression<Func<TSource, bool>> predicate)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (predicate is null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            return ((IDatabaseQueryProvider<TSource>)source.Provider)
                .CreateQuery(
                    Expression.Call(
                        null,
                        GetMethodInfo(Where, source, predicate),
                        new Expression[] { source.Expression, Expression.Quote(predicate) })
                    );
        }

        public static TSource First<TSource>(this IDatabaseQueryable<TSource> source)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return source.Provider.Execute<TSource>(
                Expression.Call(
                    null,
                    GetMethodInfo(First, source),
                    new Expression[] { source.Expression }
                    )
                );
        }

        public static TSource First<TSource>(this IDatabaseQueryable<TSource> source, Expression<Func<TSource, bool>> predicate)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (predicate is null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            return source.Provider.Execute<TSource>(
                Expression.Call(
                    null,
                    GetMethodInfo(First, source, predicate),
                    new Expression[] { source.Expression, Expression.Quote(predicate) }
                    )
                );
        }

        public static TSource FirstOrDefault<TSource>(this IDatabaseQueryable<TSource> source)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return source.Provider.Execute<TSource>(
                Expression.Call(
                    null,
                    GetMethodInfo(FirstOrDefault, source),
                    new Expression[] { source.Expression }
                    )
                );
        }

        public static TSource FirstOrDefault<TSource>(this IDatabaseQueryable<TSource> source, Expression<Func<TSource, bool>> predicate)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (predicate is null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            return source.Provider.Execute<TSource>(
                Expression.Call(
                    null,
                    GetMethodInfo(FirstOrDefault, source, predicate),
                    new Expression[] { source.Expression, Expression.Quote(predicate) }
                    )
                );
        }
    }
}