﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

using Handy.Converters.Generic;
using Handy.QueryInteractions;

namespace Handy
{
    public class TableManager<Table> : ITableQueryable<Table> where Table : class, new()
    {
        private readonly TableQueryProvider<Table> mr_TableQueryProvider;
        private readonly Expression mr_Expression;

        internal TableManager(ContextOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            mr_TableQueryProvider = new TableQueryProvider<Table>(options);
            mr_Expression = Expression.Constant(this);
        }

        internal TableManager(TableQueryProvider<Table> tableQueryProvider, Expression expression)
        {
            if (tableQueryProvider == null)
            {
                throw new ArgumentNullException(nameof(tableQueryProvider));
            }

            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            mr_TableQueryProvider = tableQueryProvider;
            mr_Expression = expression;
        }

        Expression ITableQueryable.Expression => mr_Expression;

        public ITableQueryProvider<Table> Provider => mr_TableQueryProvider;

        public void Add(Table newElement)
        {
            TablePropertyInformation propertyQueryCreator = mr_TableQueryProvider.Creator.PropertyQueryCreator;

            StringBuilder stringBuilder = new StringBuilder("INSERT INTO ");

            stringBuilder.Append(propertyQueryCreator.GetTableName());
            stringBuilder.Append(' ');
            stringBuilder.Append(propertyQueryCreator.GetTableProperties());
            stringBuilder.Append(" VALUES ");
            stringBuilder.Append(propertyQueryCreator.GetTablePropertiesValue(newElement));
            stringBuilder.Append(';');

            mr_TableQueryProvider.Connection.ExecuteNonQuery(stringBuilder.ToString());
        }

        public IdType AddAndOutputId<IdType>(Table newElement) where IdType : struct
        {
            Type idType = typeof(IdType);

            TablePropertyInformation propertyQueryCreator = mr_TableQueryProvider.Creator.PropertyQueryCreator;

            StringBuilder stringBuilder = new StringBuilder("INSERT INTO ");

            stringBuilder.Append(propertyQueryCreator.GetTableName());
            stringBuilder.Append(' ');
            stringBuilder.Append(propertyQueryCreator.GetTableProperties());
            stringBuilder.Append(" OUTPUT INSERTED.[");
            stringBuilder.Append(propertyQueryCreator.PrimaryKey.Value.Name);
            stringBuilder.Append("]");
            stringBuilder.Append(" VALUES ");
            stringBuilder.Append(propertyQueryCreator.GetTablePropertiesValue(newElement));
            stringBuilder.Append(';');

            DbDataReader dataReader = mr_TableQueryProvider.Connection.ExecuteReader(stringBuilder.ToString());

            ConvertManager convertManager = new ConvertManager(idType);

            return (IdType)convertManager.GetObject(dataReader);
        }

        public void AddRange(IEnumerable<Table> newElements)
        {
            if (newElements is null || newElements.Count() == 0)
            {
                throw new ArgumentNullException(nameof(newElements));
            }

            TablePropertyInformation propertyQueryCreator = mr_TableQueryProvider.Creator.PropertyQueryCreator;

            DbTransaction transaction = mr_TableQueryProvider.Connection.BeginTransaction();

            DbCommand sqlCommand = mr_TableQueryProvider.Connection.CreateCommand();
            sqlCommand.Transaction = transaction;

            try
            {
                StringBuilder stringBuilder = new StringBuilder("INSERT INTO ");

                stringBuilder.Append(propertyQueryCreator.GetTableName());
                stringBuilder.Append(' ');
                stringBuilder.Append(propertyQueryCreator.GetTableProperties());
                stringBuilder.Append(" VALUES ");
                stringBuilder.Append(propertyQueryCreator.GetTablesPropertiesValue(newElements));

                sqlCommand.CommandText = stringBuilder.ToString();
                sqlCommand.ExecuteNonQuery();

                transaction.Commit();
            }
            catch (Exception Ex)
            {
                transaction.Rollback();

                throw Ex;
            }
        }

        public void Update(Table element)
        {
            TablePropertyInformation propertyQueryCreator = mr_TableQueryProvider.Creator.PropertyQueryCreator;

            StringBuilder stringBuilder = new StringBuilder("UPDATE ");

            stringBuilder.Append(propertyQueryCreator.GetTableName());
            stringBuilder.Append(" SET ");
            stringBuilder.Append(propertyQueryCreator.GetTablePropertiesNameAndValue(element));
            stringBuilder.Append(" WHERE ");
            stringBuilder.Append(propertyQueryCreator.GetPropertyName(mr_TableQueryProvider.Creator.PropertyQueryCreator.PrimaryKey));
            stringBuilder.Append(" = ");
            stringBuilder.Append(TablePropertyInformation.ConvertFieldQuery(mr_TableQueryProvider.Creator.PropertyQueryCreator.PrimaryKey.Key.GetValue(element)));
            stringBuilder.Append(';');

            mr_TableQueryProvider.Connection.ExecuteNonQuery(stringBuilder.ToString());
        }

        public void Delete(Table element)
        {
            TablePropertyInformation propertyQueryCreator = mr_TableQueryProvider.Creator.PropertyQueryCreator;

            StringBuilder stringBuilder = new StringBuilder("DELETE FROM ");

            stringBuilder.Append(propertyQueryCreator.GetTableName());
            stringBuilder.Append(" WHERE ");
            stringBuilder.Append(propertyQueryCreator.GetPropertyName(mr_TableQueryProvider.Creator.PropertyQueryCreator.PrimaryKey));
            stringBuilder.Append(" = ");
            stringBuilder.Append(TablePropertyInformation.ConvertFieldQuery(mr_TableQueryProvider.Creator.PropertyQueryCreator.PrimaryKey.Key.GetValue(element)));
            stringBuilder.Append(';');

            mr_TableQueryProvider.Connection.ExecuteNonQuery(stringBuilder.ToString());
        }

        public void Delete(Expression<Func<Table, bool>> expression)
        {
            StringBuilder stringBuilder = new StringBuilder($"DELETE FROM ");

            string tableName = mr_TableQueryProvider.Creator.PropertyQueryCreator.GetTableName();

            string expressionTranslator = mr_TableQueryProvider.ExpressionTranslatorBuilder
                .CreateInstance(mr_TableQueryProvider.Creator)
                .ToString(expression);

            stringBuilder.Append(tableName);
            stringBuilder.Append(" WHERE ");
            stringBuilder.Append(expressionTranslator);

            mr_TableQueryProvider.Connection.ExecuteNonQuery(stringBuilder.ToString());
        }

        public void DeleteRange(IEnumerable<Table> removedElements)
        {
            if (removedElements is null || removedElements.Count() == 0)
            {
                throw new ArgumentNullException(nameof(removedElements));
            }

            TablePropertyInformation propertyQueryCreator = mr_TableQueryProvider.Creator.PropertyQueryCreator;

            DbTransaction transaction = mr_TableQueryProvider.Connection.BeginTransaction();

            DbCommand sqlCommand = mr_TableQueryProvider.Connection.CreateCommand();
            sqlCommand.Transaction = transaction;

            try
            {
                foreach (Table currentElement in removedElements)
                {
                    StringBuilder stringBuilder = new StringBuilder("DELETE FROM ");

                    stringBuilder.Append(propertyQueryCreator.GetTableName());
                    stringBuilder.Append(" WHERE ");
                    stringBuilder.Append(propertyQueryCreator.GetPropertyName(mr_TableQueryProvider.Creator.PropertyQueryCreator.PrimaryKey));
                    stringBuilder.Append(" = ");
                    stringBuilder.Append(TablePropertyInformation.ConvertFieldQuery(mr_TableQueryProvider.Creator.PropertyQueryCreator.PrimaryKey.Key.GetValue(currentElement)));
                    stringBuilder.Append(';');

                    sqlCommand.CommandText = stringBuilder.ToString();

                    sqlCommand.ExecuteNonQuery();
                }

                transaction.Commit();
            }
            catch (Exception Ex)
            {
                transaction.Rollback();

                throw Ex;
            }
        }

        public void DeleteAll()
        {
            TablePropertyInformation propertyQueryCreator = mr_TableQueryProvider.Creator.PropertyQueryCreator;

            string createElementQuery = $"DELETE FROM {propertyQueryCreator.GetTableName()};";

            mr_TableQueryProvider.Connection.ExecuteNonQuery(createElementQuery);
        }

        public IEnumerable<Table> FromSql(string query)
        {
            DbConnection connection = mr_TableQueryProvider.Connection;

            TableConvertManager<Table> tableConvertManager = connection.GetTableConverter<Table>();

            DbDataReader dataReader = connection.ExecuteReader(query);

            IEnumerable<Table> value = tableConvertManager.GetObjectsEnumerable(dataReader);

            return value;
        }

        public IEnumerator<Table> GetEnumerator() => mr_TableQueryProvider.Execute(mr_Expression).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => mr_TableQueryProvider.Execute(mr_Expression).GetEnumerator();
    }
}