﻿using System;
using System.Data;
using System.Diagnostics;
using System.Reflection;

using Handy.Converters;
using Handy.Converters.Generic;
using Handy.Extensions;

using Microsoft.Data.SqlClient;

namespace Handy
{
    internal static class SqlConnectionExtensions
    {
        private static TResult ConvertReader<TResult>(this SqlConnection sqlConnection, SqlDataReader dataReader)
        {
            if (dataReader == null)
            {
                throw new ArgumentNullException(nameof(dataReader));
            }

            Type resultType = typeof(TResult);
            Type elementType = resultType.GetElementType() ?? resultType;
            TableAttribute tableAttribute = elementType.GetCustomAttribute<TableAttribute>();

            ConvertManager convertManager;

            if (tableAttribute != null)
            {
                convertManager = sqlConnection.GetTableConverter(elementType);
            }
            else
            {
                convertManager = new ConvertManager(elementType);
            }

            if (resultType.IsArray)
            {
                return (TResult)convertManager.GetObjects(dataReader);
            }

            return (TResult)convertManager.GetObject(dataReader);
        }

        public static SqlCommand CreateProcedureCommand(this SqlConnection sqlConnection, string procedureName)
        {
            SqlCommand dataCommand = sqlConnection.CreateCommand();

            dataCommand.CommandType = CommandType.StoredProcedure;
            dataCommand.CommandText = procedureName;

            return dataCommand;
        }

        public static void ExecuteNonQuery(this SqlConnection sqlConnection, string query)
        {
            if (sqlConnection == null)
            {
                throw new ArgumentNullException(nameof(sqlConnection));
            }

            if (string.IsNullOrWhiteSpace(query))
            {
                throw new ArgumentNullException(nameof(query));
            }

            using (SqlCommand sqlCommand = sqlConnection.CreateCommand())
            {
                sqlCommand.CommandText = query;

                sqlCommand.ExecuteNonQuery();
            }
        }

        public static SqlDataReader ExecuteReader(this SqlConnection sqlConnection, string query)
        {
            if (sqlConnection == null)
            {
                throw new ArgumentNullException(nameof(sqlConnection));
            }

            if (string.IsNullOrWhiteSpace(query))
            {
                throw new ArgumentNullException(nameof(query));
            }

            using (SqlCommand sqlCommand = sqlConnection.CreateCommand())
            {
                sqlCommand.CommandText = query;

                return sqlCommand.ExecuteReader();
            }
        }

        /// <summary>
        /// Метод для вызова процедуры из базы данных.
        /// Если процедура имеет принимаемые аргументы, то ExecuteProcedure должен обязательно вызываться в методе
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sqlConnection"></param>
        /// <param name="procedureName"></param>
        /// <param name="arguments">Аргументы, которые передаются в процедуру. Аргументы должны идти в порядке параметров метода</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static T ExecuteProcedure<T>(this SqlConnection sqlConnection, string procedureName, params object[] arguments)
        {
            if (sqlConnection == null)
            {
                throw new ArgumentNullException(nameof(sqlConnection));
            }

            if (string.IsNullOrWhiteSpace(procedureName))
            {
                throw new ArgumentNullException(nameof(procedureName));
            }

            SqlCommand dataCommand = sqlConnection.CreateProcedureCommand(procedureName);

            StackFrame stackFrame = new StackFrame(2);
            MethodBase callingMethod = stackFrame.GetMethod();

            dataCommand.AddArguments(arguments, stackFrame, callingMethod);

            SqlDataReader dataReader = dataCommand.ExecuteReader();

            T result = sqlConnection.ConvertReader<T>(dataReader);

            dataCommand.Dispose();
            dataReader.Close();

            return result;
        }

        public static TableConvertManager<Table> GetTableConverter<Table>(this SqlConnection connection) where Table : class, new() => new TableConvertManager<Table>(connection);

        public static TableConvertManager GetTableConverter(this SqlConnection connection, Type tableType)
        {
            if (connection == null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            if (tableType == null)
            {
                throw new ArgumentNullException(nameof(tableType));
            }

            return new TableConvertManager(tableType, connection);
        }
    }
}