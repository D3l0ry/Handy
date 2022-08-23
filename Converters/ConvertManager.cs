﻿ using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Data.SqlClient;

namespace DatabaseManager
{
    public class ConvertManager
    {
        protected readonly Type mr_Type;

        public ConvertManager(Type type)
        {
            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            mr_Type = type;
        }

        /// <summary>
        /// Получение массива объектов из таблицы
        /// </summary>
        /// <param name="dataReader"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual IEnumerable GetObjects(SqlDataReader dataReader)
        {
            IList list = (IList)Activator
                .CreateInstance(typeof(List<>)
                .MakeGenericType(mr_Type));

            while (dataReader.Read())
            {
                object newObject = GetInternalObject(dataReader);

                list.Add(newObject);
            }

            Type enumerableType = typeof(Enumerable);

            object cast = enumerableType
                .GetMethod("Cast")
                .MakeGenericMethod(mr_Type)
                .Invoke(null, new object[] { list });

            IEnumerable result = (IEnumerable)enumerableType
                .GetMethod("ToArray")
                .MakeGenericMethod(mr_Type)
                .Invoke(null, new object[] { cast });

            dataReader.Close();

            return result;
        }

        /// <summary>
        /// Получение объекта из таблицы
        /// </summary>
        /// <param name="dataReader"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual object GetObject(SqlDataReader dataReader)
        {
            if(!dataReader.Read())
            {
                if(mr_Type.IsValueType)
                {
                    return Activator.CreateInstance(mr_Type);
                }

                return null;
            }

            return GetInternalObject(dataReader);
        }

        protected virtual object GetInternalObject(SqlDataReader dataReader)
        {
            if (dataReader.FieldCount == 1)
            {
                return dataReader.GetValue(0);
            }

            object table = Activator.CreateInstance(mr_Type);

            foreach (PropertyInfo currentProperty in mr_Type.GetProperties())
            {
                object readerValue = dataReader[currentProperty.Name];

                if (readerValue is DBNull)
                {
                    continue;
                }

                currentProperty.SetValue(table, readerValue);
            }

            return table;
        }
    }
}