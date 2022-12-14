using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace Handy
{
    /// <summary>
    /// Абстрактный класс для работы с базой данных и ее таблицами
    /// </summary>
    public abstract class DatabaseContext : IDisposable
    {
        private readonly ContextOptions mr_Options;
        private readonly Dictionary<Type, IQueryable> mr_Tables;

        protected DatabaseContext()
        {
            ContextOptionsBuilder optionsBuilder = new ContextOptionsBuilder();

            OnConfigure(optionsBuilder);

            mr_Options = optionsBuilder.Build();
            mr_Tables = new Dictionary<Type, IQueryable>();

            mr_Options.Connection.ConnectionString = mr_Options.ConnectionString;
            mr_Options.Connection.Open();
        }

        protected DatabaseContext(string connection)
        {
            ContextOptionsBuilder optionsBuilder = new ContextOptionsBuilder();
            optionsBuilder.UseConnectionString(connection);

            OnConfigure(optionsBuilder);

            mr_Options = optionsBuilder.Build();
            mr_Tables = new Dictionary<Type, IQueryable>();

            mr_Options.Connection.ConnectionString = mr_Options.ConnectionString;
            mr_Options.Connection.Open();
        }

        public DbConnection Connection => mr_Options.Connection;

        /// <summary>
        /// Вызывается при инициализации и до момента подключения к бд
        /// </summary>
        /// <param name="options"></param>
        protected abstract void OnConfigure(ContextOptionsBuilder options);

        /// <summary>
        /// Метод для вызова процедур базы данных.
        /// Если процедура имеет принимаемые аргументы, то ExecuteProcedure должен обязательно вызываться в методе
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="procedure">Имя хранимой процедуры</param>
        /// <param name="arguments">Аргументы, которые передаются в процедуру. Аргументы должны идти в порядке параметров метода</param>
        /// <returns></returns>
        protected virtual T ExecuteProcedure<T>(string procedure, params object[] arguments) => mr_Options.Connection.ExecuteProcedure<T>(procedure, arguments);

        /// <summary>
        /// Получает объект TableManager с указанным типом, который определяет модель таблицы из базы данных
        /// </summary>
        /// <typeparam name="Table">Тип, определяющий модель таблицы из базы данных</typeparam>
        /// <returns></returns>
        protected TableManager<Table> GetTable<Table>() where Table : class, new()
        {
            Type tableType = typeof(Table);
            bool tryGet = mr_Tables.TryGetValue(tableType, out IQueryable selectedTable);

            if (tryGet)
            {
                return (TableManager<Table>)selectedTable;
            }

            TableManager<Table> newTable = new TableManager<Table>(mr_Options);

            mr_Tables.Add(tableType, newTable);

            return newTable;
        }

        public void Dispose()
        {
            mr_Tables.Clear();
            mr_Options.Connection.Close();
        }
    }
}