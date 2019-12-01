using System.Reflection;

namespace Database.Core
{
    #region Usings

    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data;
    using System.Linq;
    using System.Data.Linq;
    using Database.Core.Utils;
    using Database.Core.Enums;
    using Database.Core.Factory;
    using Database.Core.Attribute;

    #endregion

    #region Delegates

    public delegate string DbProviderNameLoader();
    public delegate string DbConnectionStringLoader();

    #endregion

    /// <summary>
    /// Agnostic database type artifact using <see cref="DbProviderFactory"/>
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public class DatabaseCore : IDisposable
    {
        #region Campos

        private const bool Use = true;

        private const bool DontUse = false;

        private static bool UseAttributeInfo = DontUse;

        protected internal static Dictionary<Type, DbType> DbTypes;

        private DbConnectionStringBuilder connectionStringBuilder;

        private readonly string providerName;
        
        private DbProviderFactory providerFactory;

        private IDbConnection connection;

        private IDbConnection connectionCommand;

        private IDbTransaction transaction;
        
        private IDbTransaction transactionCommand;

        private IDbCommand command;

        private IDataReader reader;

        private IDbDataAdapter adapter;

        #endregion

        #region Constructores

        /// <summary>
        /// Initializes the <see cref="DatabaseCore"/> class.
        /// </summary>
        static DatabaseCore()
        {
            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseCore" /> class.
        /// </summary>
        /// <param name="providerType">Type of the provider.</param>
        /// <param name="connectionString">The connection string.</param>
        protected DatabaseCore(DataProviderType providerType, string connectionString)
        {
            this.providerFactory = DbProviderFactoryProvider.GetDbProviderFactory(providerType, useAtrributeInfo: UseAttributeInfo);
            this.providerName = providerType.GetAttributeValue<ProviderAttribute>(p => p.Name)?.ToString();
            this.connectionStringBuilder = this.providerFactory.CreateConnectionStringBuilder() ?? throw new ArgumentNullException(nameof(this.connectionStringBuilder));
            this.connectionStringBuilder.ConnectionString = connectionString;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseCore" /> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        protected DatabaseCore(string connectionString)
        {
            this.providerFactory = DbProviderFactoryProvider.GetDbProviderFactory(connectionString, useAtrributeInfo: UseAttributeInfo);
            this.providerName = DbProviderFactoryProvider.GetProviderType(this.providerFactory).GetAttributeValue<ProviderAttribute>(p => p.Name)?.ToString();
            this.connectionStringBuilder = this.providerFactory.CreateConnectionStringBuilder() ?? throw new ArgumentNullException(nameof(this.connectionStringBuilder));
            this.connectionStringBuilder.ConnectionString = connectionString;
        }

        #endregion

        #region Propiedades

        protected internal IDbConnection Connection => this.connection;

        protected internal IDbConnection ConnectionCommand => this.connectionCommand;

        protected internal IDbTransaction Transaction => this.transaction;

        protected internal IDbTransaction TransactionCommand => this.transactionCommand;

        protected internal IDbCommand Command => this.command;

        protected internal IDataReader Reader => this.reader;

        protected internal IDbDataAdapter Adapter => this.adapter;

        #endregion

        #region Factory

        public static DatabaseCore Create(DataProviderType providerType, string connectionString)
        {
            return new DatabaseCore(providerType, connectionString);
        }

        //public static DatabaseCore Create(string connectionString, string providerName)
        //{
        //    return new DatabaseCore(() => connectionString, () => providerName);
        //}

        //public static DatabaseCore Create(DbConnectionStringLoader connectionStringLoader, DbProviderNameLoader dbProviderNameLoader)
        //{
        //    return new DatabaseCore(connectionStringLoader, dbProviderNameLoader);
        //}

        #endregion

        #region Connection

        public virtual IDbConnection CreateConnection()
        {
            this.connection = this.providerFactory.CreateConnection();
            if (this.connection == null)
            {
                throw new ArgumentNullException(nameof(this.connection));
            }

            this.connection.ConnectionString = this.connectionStringBuilder.ConnectionString;
            return this.connection;
        }

        public virtual IDbConnection CreateOpenConnection()
        {
            this.OpenConnection(createConnection: true);
            return this.connection;
        }

        public virtual IDbConnection OpenConnection(bool createConnection = false)
        {
            if (this.connection == null)
            {
                if (!createConnection)
                {
                    throw new ArgumentNullException(nameof(this.connection));
                }

                this.CreateConnection();
            }

            this.connection.Open();
            return this.connection;
        }

        public virtual void CloseConnection()
        {
            if (this.connection == null)
            {
                return;
            }

            if (this.connection.State == ConnectionState.Open)
            {
                this.connection.Close();
            }

            this.Dispose(this.connection);
        }

        #endregion

        #region Command

        public virtual IDbCommand CreateCommand(bool createConnection = false)
        {
            if (this.connection == null)
            {
                if (!createConnection)
                {
                    throw new ArgumentNullException(nameof(this.connection));
                }

                this.CreateConnection();
            }

            // TODO Manu DbCommandBuilder
            DbCommandBuilder commandBuilder = this.providerFactory.CreateCommandBuilder() ?? throw new ArgumentNullException("commandBuilder");
            commandBuilder.DataAdapter = this.CreateAdapter() as DbDataAdapter;
            
            this.command = this.providerFactory.CreateCommand();
            if (this.command != null && createConnection)
            {
                this.command.Connection = this.connection;
            }
            
            return this.command;
        }

        public virtual IDbCommand CreateCommand(string commandText, IDbConnection connectionCmd, IDbTransaction transactionCmd)
        {
            if (connectionCmd != null)
            {
                this.connectionCommand = connectionCmd;
                this.CreateCommand();
                if (this.command != null)
                {
                    this.command.Connection = this.connectionCommand;
                }
            }
            else
            {
                this.command = this.CreateCommand(createConnection: true);
            }

            if (transactionCmd == null)
            {
                // TODO Manu Revisar IsolationLevel
                this.transaction = this.CreateTransaction(IsolationLevel.Snapshot);
            }
            else
            {
                this.transactionCommand = transactionCmd;
            }

            this.command = this.CreateCommand();
            this.command.CommandType = CommandType.Text;
            this.command.CommandText = commandText;
            this.command.Transaction = this.transactionCommand ?? this.transaction;
            return this.command;
        }

        public virtual IDbCommand CreateStoredProcCommand(string procName, IDbConnection connectionSP, IDbTransaction transactionSP)
        {
            if (connectionSP != null)
            {
                this.connectionCommand = connectionSP;
                this.command = this.connectionCommand.CreateCommand();
            }
            else
            {
                this.command = this.CreateCommand();
            }

            if (transactionSP == null)
            {
                // TODO Manu Revisar IsolationLevel
                this.transaction = this.CreateTransaction(IsolationLevel.Snapshot);
            }
            else
            {
                this.transactionCommand = transactionSP;
            }

            this.command.CommandType = CommandType.StoredProcedure;
            this.command.CommandText = procName;
            this.command.Transaction = this.transactionCommand ?? this.transaction;
            return this.command;
        }

        #endregion

        #region DataAdapter

        public virtual IDataAdapter CreateAdapter()
        {
            this.adapter = this.providerFactory.CreateDataAdapter();
            return this.adapter;
        }

        #endregion

        #region Transaction

        public virtual IDbTransaction CreateTransaction(IsolationLevel isolationLevel, bool createConnection = false)
        {
            if (this.connection == null)
            {
                if (!createConnection)
                {
                    throw new ArgumentNullException(nameof(this.connection));
                }

                this.CreateConnection();
            }

            this.transaction = this.connection.BeginTransaction(isolationLevel);
            return this.transaction;
        }

        public virtual IDbTransaction CreateTransaction(IsolationLevel isolationLevel, IDbConnection connectionTran)
        {
            if (connectionTran == null)
            {
                this.CreateConnection();
            }
            else
            {
                this.connection = connectionTran;
            }

            this.CreateTransaction(isolationLevel, createConnection: connectionTran == null);
            return this.transaction;
        }

        public virtual void CommitTransaction()
        {
            this.CommitTransaction(this.transaction);
        }

        public virtual void CommitTransaction(IDbTransaction transactionToCommit)
        {
            transactionToCommit?.Commit();
        }

        public virtual void RollbackTransaction()
        {
            this.RollbackTransaction(this.transaction);
        }

        public virtual void RollbackTransaction(IDbTransaction transactionToRollback)
        {
            transactionToRollback?.Rollback();
        }

        #endregion

        #region DML



        #endregion

        #region DDL



        #endregion

        #region Parameters

        public virtual IDataParameter CreateParameter(ParameterDefinition definition)
        {
            IDataParameter parameter = this.command.CreateParameter();
            parameter.ParameterName = definition.Name;
            parameter.Value = definition.Value;
            parameter.DbType = definition.Type;
            return parameter;
        }

        public virtual IDataParameter CreateParameter(string name, object value, ParameterDirection direction)
        {
            ParameterDefinition definition = new ParameterDefinition(name, value, direction);
            IDataParameter parameter = this.CreateParameter(definition);
            return parameter;
        }

        public virtual IEnumerable<IDataParameter> CreateParameters(IEnumerable<string> parameterNames, IEnumerable<object> parameterValues)
        {
            return parameterNames.Select((parameter, index) => CreateParameter(parameter, parameterValues.ElementAt(index), ParameterDirection.Input));
        }

        #endregion

        #region Util

        protected internal static void Initialize()
        {
            DbProviderFactoryInitializer.Initialize();
            LoadDbTypeMappings();
        }

        /// <summary>
        /// Loads the database type mappings.
        /// </summary>
        internal static void LoadDbTypeMappings()
        {
            DbTypes = new Dictionary<Type, DbType>
            {
                [typeof(byte)] = DbType.Byte,
                [typeof(sbyte)] = DbType.SByte,
                [typeof(short)] = DbType.Int16,
                [typeof(ushort)] = DbType.UInt16,
                [typeof(int)] = DbType.Int32,
                [typeof(uint)] = DbType.UInt32,
                [typeof(long)] = DbType.Int64,
                [typeof(ulong)] = DbType.UInt64,
                [typeof(float)] = DbType.Single,
                [typeof(double)] = DbType.Double,
                [typeof(decimal)] = DbType.Decimal,
                [typeof(bool)] = DbType.Boolean,
                [typeof(string)] = DbType.String,
                [typeof(char)] = DbType.StringFixedLength,
                [typeof(Guid)] = DbType.Guid,
                [typeof(DateTime)] = DbType.DateTime,
                [typeof(DateTimeOffset)] = DbType.DateTimeOffset,
                [typeof(byte[])] = DbType.Binary,
                [typeof(byte?)] = DbType.Byte,
                [typeof(sbyte?)] = DbType.SByte,
                [typeof(short?)] = DbType.Int16,
                [typeof(ushort?)] = DbType.UInt16,
                [typeof(int?)] = DbType.Int32,
                [typeof(uint?)] = DbType.UInt32,
                [typeof(long?)] = DbType.Int64,
                [typeof(ulong?)] = DbType.UInt64,
                [typeof(float?)] = DbType.Single,
                [typeof(double?)] = DbType.Double,
                [typeof(decimal?)] = DbType.Decimal,
                [typeof(bool?)] = DbType.Boolean,
                [typeof(char?)] = DbType.StringFixedLength,
                [typeof(Guid?)] = DbType.Guid,
                [typeof(DateTime?)] = DbType.DateTime,
                [typeof(DateTimeOffset?)] = DbType.DateTimeOffset,
                [typeof(Binary)] = DbType.Binary
            };
        }

        #endregion

        #region Dispose

        /// <summary>
        /// Disposes the specified disposable.
        /// </summary>
        /// <param name="disposable">The disposable.</param>
        protected void Dispose(IDisposable disposable)
        {
            if (disposable == null)
            {
                return;
            }

            disposable.Dispose();
            disposable = null;
        }

        /// <summary>
        /// Releases the unmanaged resources.
        /// </summary>
        private void ReleaseUnmanagedResources()
        {
            // TODO release unmanaged resources here
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            ReleaseUnmanagedResources();
            if (disposing)
            {
                this.Dispose(this.command);
                this.Dispose(this.transaction);
                this.Dispose(this.transactionCommand);
                this.Dispose(this.reader);
                this.Dispose(this.connectionCommand);
                this.Dispose(this.connection);
            }
        }

        /// <summary>
        /// Realiza tareas definidas por la aplicación asociadas a la liberación o al restablecimiento de recursos no administrados.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="DatabaseCore"/> class.
        /// </summary>
        ~DatabaseCore()
        {
            Dispose(false);
        }

        #endregion
    }
}