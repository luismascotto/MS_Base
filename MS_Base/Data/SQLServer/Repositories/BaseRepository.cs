using Microsoft.EntityFrameworkCore;
using MS_Base.Data.SQLServer.Contexts;
using MS_Base.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace MS_Base.Data.SQLServer.Repositories
{
    public class BaseRepository : IDisposable
    {

        public const int DEFAULT_TIMEOUT = 10;

        protected DatabaseDbContext dbContext;
        private bool Disposed;

        private DbTransaction _dbTransaction = null;

        public BaseRepository(DatabaseDbContext _dbContext)
        {
            dbContext = _dbContext;
        }

        protected List<T> ExecuteProc<T>(string procName, List<ExecuteProcParam> procParams, int procTimeout = DEFAULT_TIMEOUT) where T : class
        {
            List<T> result = new List<T>();
            using (DbCommand command = CreateCommand(dbContext, procName, procParams, procTimeout, CommandType.StoredProcedure))
            {
                OpenDbConnection();
                using (DbDataReader reader = command.ExecuteReader(CommandBehavior.SingleResult))
                {
                    while (reader.Read())
                    {
                        T obj = MapToClass<T>(reader);
                        result.Add(obj);
                    }
                }
            }

            return result;
        }

        protected DbDataReader ExecuteProc(string procName, List<ExecuteProcParam> procParams, int procTimeout = DEFAULT_TIMEOUT)
        {
            DbDataReader result;
            using (DbCommand command = CreateCommand(dbContext, procName, procParams, procTimeout, CommandType.StoredProcedure))
            {
                OpenDbConnection();
                result = command.ExecuteReader(CommandBehavior.CloseConnection);
            }
            return result;
        }

        protected object ExecuteProcScalar(string procName, List<ExecuteProcParam> procParams, int procTimeout = DEFAULT_TIMEOUT)
        {
            object result;
            using (DbCommand command = CreateCommand(dbContext, procName, procParams, procTimeout, CommandType.StoredProcedure))
            {
                OpenDbConnection();
                result = command.ExecuteScalar();
            }
            return result;
        }

        protected DbDataReader ExecuteQueryFromFile(string SQL, List<ExecuteProcParam> procParams, int procTimeout = DEFAULT_TIMEOUT)
        {
            DbDataReader result;
            using (DbCommand command = CreateCommand(dbContext, SQL, procParams, procTimeout))
            {
                OpenDbConnection();
                result = command.ExecuteReader(CommandBehavior.CloseConnection);
            }
            return result;
        }

        protected List<T> ExecuteQueryFromFile<T>(string SQL, List<ExecuteProcParam> procParams, int procTimeout = DEFAULT_TIMEOUT) where T : class
        {
            List<T> result = new List<T>();
            using (DbCommand command = CreateCommand(dbContext, SQL, procParams, procTimeout))
            {
                OpenDbConnection();
                using (DbDataReader reader = command.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    while (reader.Read())
                    {
                        T obj = MapToClass<T>(reader);
                        result.Add(obj);
                    }
                }
            }

            return result;
        }

        protected List<SqlParameter> ExecuteProcNonQueryOutput(string procName, List<ExecuteProcParam> procParams, int procTimeout = DEFAULT_TIMEOUT)
        {
            List<SqlParameter> result = new List<SqlParameter>();
            using (DbCommand command = CreateCommand(dbContext, procName, procParams, procTimeout, CommandType.StoredProcedure))
            {
                OpenDbConnection();
                command.ExecuteNonQuery();
                if (procParams != null)
                {
                    procParams.ForEach(p =>
                    {
                        if (p.direction == ParameterDirection.Output)
                        {
                            result.Add(new SqlParameter(p.name, command.Parameters[p.name].Value));
                        }
                    });
                }
            }
                
            return result;
        }

        protected bool ExecuteProcNonQuery(string procName, List<ExecuteProcParam> procParams, int procTimeout = DEFAULT_TIMEOUT)
        {
            bool result = true;
            ExecuteProcNonQueryOutput(procName, procParams, procTimeout);
            return result;
        }

        protected DbCommand CreateCommand(DbContext context, string commandName, List<ExecuteProcParam> procParams, int procTimeout,
            CommandType commandType = CommandType.Text)
        {
            DbCommand command = context.Database.GetDbConnection().CreateCommand();
            command.CommandTimeout = procTimeout;
            if (commandType != CommandType.StoredProcedure)
            {
                command.CommandText = getDataBaseQuery(commandName);
            }
            else
            {
                command.CommandText = commandName;
            }
            command.CommandType = commandType;
            if (procParams != null)
            {
                procParams.ForEach(p =>
                {
                    SqlParameter paramItem = new SqlParameter(p.name, p.dbType)
                    {
                        TypeName = p.typeName,
                        Value = (p.getValue<object>() != null) ? p.getValue<object>() : DBNull.Value,
                        Direction = p.direction
                    };
                    command.Parameters.Add(paramItem);
                });
            }
            return command;
        }

        protected string getDataBaseQuery(string queryName)
        {
            return QueryAdhocManager.GetQuery(queryName, $"{AppContext.BaseDirectory}\\SQL");
        }

        /// <summary>
        /// Mapeia a linha atual do DataReader para uma classe do tipo T
        /// </summary>
        /// <typeparam name="T">Tipo do objeto a ser retornado</typeparam>
        /// <param name="reader">DataReader com dados a serem convertidos</param>
        /// <returns>Retorna objeto com dados preenchidos</returns>
        private T MapToClass<T>(DbDataReader reader) where T : class
        {
            // Cria instância do objeto que será retornado
            T returnedObject = Activator.CreateInstance<T>();

            // Recupera todas as propriedades do objeto que será preenchido
            List<PropertyInfo> modelProperties = returnedObject.GetType().GetProperties().OrderBy(p => p.MetadataToken).ToList();

            // Percorre todos os campos do DataReader
            for (int i = 0; i < reader.FieldCount; i++)
            {
                var fieldName = reader.GetName(i);

                // Recupera propriedade do objeto que possui o mesmo nome do campo do DataReader
                //var prop = modelProperties.Where(p => p.Name == fieldName).FirstOrDefault();
                //Alterado para string.Equals para não termos problema com campo vchCampo no model e vchcampo no retorno da consulta
                var prop = modelProperties.Where(p => string.Equals(p.Name, fieldName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

                if (prop == null)
                {
                    throw new Exception($"O campo [{fieldName}] não foi encontrado no objeto mapeado {returnedObject.GetType().ToString()}");
                }

                // Recupera o valor da coluna 
                var value = reader.GetValue(i);

                // Verifica se o tipo de dados da propriedade é Nullable
                if (prop.PropertyType.GetTypeInfo().IsGenericType && prop.PropertyType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
                {
                    // Caso o valor da coluna seja null ou DBNull, preenche propriedade com o valor null
                    if (value == null || reader.IsDBNull(i))
                    {
                        prop.SetValue(returnedObject, null, null);
                    }
                    else
                    {
                        // Converte o valor retornado pelo DataReader (não nullable) para o tipo nullable da propriedade
                        // Não é possível armazenar o valor de um campo int (DataReader) em um campo int? do objeto, por exemplo. É necessário fazer a conversão.
                        prop.SetValue(returnedObject, Convert.ChangeType(value, Nullable.GetUnderlyingType(prop.PropertyType)), null);
                    }
                }
                else
                {
                    // Caso seja uma string e o retorno tenha sido nulo
                    if (prop.PropertyType.FullName == "System.String" && (value == null || reader.IsDBNull(i)))
                    {
                        prop.SetValue(returnedObject, null, null);
                    }
                    else
                    {
                        // Caso a propriedade não seja Nullable, armazena o valor do campo na respectiva propriedade do objeto
                        prop.SetValue(returnedObject, Convert.ChangeType(value, prop.PropertyType), null);
                    }
                }
            }

            return returnedObject;
        }

        public string[] getNames(Type model)
        {
            List<string> ret = new List<string>();

            List<PropertyInfo> modelProperties = model.GetProperties().OrderBy(p => p.MetadataToken).ToList();
            modelProperties.ForEach(p => ret.Add(p.Name));

            return ret.ToArray();
        }

        public void Dispose()
        {
            if (dbContext != null)
            {
                dbContext.Dispose();
            }

            if (!Disposed)
            {
                Disposed = true;
            }
        }

        protected List<T> ExecuteProcWithOutputParameters<T>(string procName, ref List<ExecuteProcParam> procParams, int procTimeout = DEFAULT_TIMEOUT) where T : class
        {
            List<T> result = new List<T>();
            using (DbCommand command = CreateCommand(dbContext, procName, procParams, procTimeout, CommandType.StoredProcedure))
            {
                OpenDbConnection();
                using (DbDataReader reader = command.ExecuteReader(CommandBehavior.SingleResult))
                {
                    while (reader.Read())
                    {
                        T obj = MapToClass<T>(reader);
                        result.Add(obj);
                    }
                }

                //Varrer parâmetros output e setar seu valor
                if (procParams != null)
                {
                    procParams.ForEach(p =>
                    {
                        if (p.direction == ParameterDirection.Output)
                        {
                            p.setValue(command.Parameters[p.name].Value);
                        }
                    });
                }
            }


            return result;
        }

        protected void ExecuteNonQueryFromFile(string SQL, List<ExecuteProcParam> procParams, int procTimeout = DEFAULT_TIMEOUT)
        {
            using (DbCommand command = CreateCommand(dbContext, SQL, procParams, procTimeout)) {
                OpenDbConnection();
                command.ExecuteNonQuery();
            }
        }

        protected void OpenDbConnection()
        {
            DbConnection dbConnection = dbContext.Database.GetDbConnection();

            if (dbConnection != null && dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
        }
        
		protected void ExecuteNonQueryOutputFromFile(string procName, ref List<ExecuteProcParam> procParams, int procTimeout = DEFAULT_TIMEOUT)
        {
            using (DbCommand command = CreateCommand(dbContext, procName, procParams, procTimeout))
            {
                OpenDbConnection();
                command.ExecuteNonQuery();
                if (procParams != null)
                {
                    procParams.ForEach(p =>
                    {
                        if (p.direction == ParameterDirection.Output)
                        {
                            p.setValue(command.Parameters[p.name].Value);
                        }
                    });
                }
            }
        }

        public void OpenDbTransaction()
        {
            //Em transação, fecha qualquer outra conexão previamente aberta para funcionar corretamente
            DbConnection dbConnection = dbContext.Database.GetDbConnection();

            if (dbConnection != null && dbConnection.State == ConnectionState.Open)
            {
                dbConnection.Close();
            }
            if (dbConnection != null && dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
                _dbTransaction = dbConnection.BeginTransaction();
            }
        }

        public void CommitDbTransaction()
        {
            
            DbConnection dbConnection = dbContext.Database.GetDbConnection();
            if (dbConnection != null && dbConnection.State == ConnectionState.Open && _dbTransaction != null)
            {
                _dbTransaction.Commit();
                _dbTransaction.Dispose();
                dbConnection.Close();
            }
        }

        public void RollbackDbTransaction()
        {
            DbConnection dbConnection = dbContext.Database.GetDbConnection();
            if (dbConnection != null && dbConnection.State == ConnectionState.Open && _dbTransaction != null)
            {
                _dbTransaction.Rollback();
                _dbTransaction.Dispose();
                _dbTransaction = null;
                dbConnection.Close();
            }
        }

        protected void ExecuteTransactionNonQueryFromFile(string SQL, List<ExecuteProcParam> procParams, int procTimeout = DEFAULT_TIMEOUT)
        {
            using (DbCommand command = CreateCommand(dbContext, SQL, procParams, procTimeout))
            {
                command.Transaction = _dbTransaction;
                command.ExecuteNonQuery();
            }
        }

        protected DbDataReader ExecuteTransactionQueryFromFile(string SQL, List<ExecuteProcParam> procParams, int procTimeout = DEFAULT_TIMEOUT)
        {
            DbDataReader result;
            using (DbCommand command = CreateCommand(dbContext, SQL, procParams, procTimeout))
            {
                command.Transaction = _dbTransaction;
                result = command.ExecuteReader(CommandBehavior.Default);
            }
            return result;
        }


        protected object ExecuteQueryFromFileScalar(string SQL, List<ExecuteProcParam> procParams, int procTimeout = DEFAULT_TIMEOUT)
        {
            object result;
            using (DbCommand command = CreateCommand(dbContext, SQL, procParams, procTimeout, CommandType.Text))
            {
                OpenDbConnection();
                result = command.ExecuteScalar();
            }
            return result;
        }


    }

    //----------------------------------------------------------------------

    public class ExecuteProcParam
    {
        public string name { get; set; }
        public string typeName { get; set; }
        public ParameterDirection direction { get; set; }

        private object _value;

        public void setValue<T>(T value)
        {
            _value = value;
            switch (typeof(T).Name)
            {
                case "Int64":
                    dbType = SqlDbType.BigInt;
                    break;
                case "Boolean":
                    dbType = SqlDbType.Bit;
                    break;
                case "Date":
                    dbType = SqlDbType.Date;
                    break;
                case "DateTime":
                    dbType = SqlDbType.DateTime;
                    break;
                case "DateTimeOffset":
                    dbType = SqlDbType.DateTimeOffset;
                    break;
                case "Decimal":
                    dbType = SqlDbType.Decimal;
                    break;
                case "Double":
                    dbType = SqlDbType.Float;
                    break;
                case "Int32":
                case "UInt32":
                    dbType = SqlDbType.Int;
                    break;
                case "String":
                    dbType = SqlDbType.NVarChar;
                    break;
                case "Single":
                    dbType = SqlDbType.Real;
                    break;
                case "Int16":
                    dbType = SqlDbType.SmallInt;
                    break;
                case "TimeSpan":
                    dbType = SqlDbType.Time;
                    break;
                case "Byte":
                    dbType = SqlDbType.TinyInt;
                    break;
                case "Guid":
                    dbType = SqlDbType.UniqueIdentifier;
                    break;
                case "Byte[]":
                    dbType = SqlDbType.VarBinary;
                    break;
                case "Char[]":
                    dbType = SqlDbType.VarBinary;
                    break;
                case "Object":
                    dbType = SqlDbType.Variant;
                    break;
                case "Xml":
                    dbType = SqlDbType.Xml;
                    break;
                case "DataTable":
                    dbType = SqlDbType.Structured;
                    break;
            }
        }

        public T getValue<T>()
        {
            return (T)_value;
        }

        public SqlDbType dbType { get; private set; }

        public ExecuteProcParam(string _name = "")
        {
            name = _name;
            direction = ParameterDirection.Input;
            _value = DBNull.Value;
        }

    }
}
