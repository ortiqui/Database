using System.Collections.Generic;
using System.Reflection;

namespace Database.Core.Factory
{
    #region Usings

    using System;
    using System.Data.Common;
    using System.Data.OleDb;
    using System.Data.SqlClient;
    using System.Data.SQLite;
    using Database.Core.Utils;
    using MySql.Data.MySqlClient;
    using Npgsql;
    using Oracle.DataAccess.Client;
    using Database.Core.Attribute;
    using Database.Core.Enums;
    using System.Linq;

    #endregion

    public static class DbProviderFactoryProvider
    {
        #region Campos

        private const string InstanceMemberName = "Instance";

        #endregion

        #region Métodos

        public static DbProviderFactory GetDbProviderFactory(string dbProviderFactoryTypename, string assemblyName)
        {
            var instance = ReflectionUtils.GetStaticProperty(dbProviderFactoryTypename, InstanceMemberName);
            if (instance == null)
            {
                var assembly = ReflectionUtils.LoadAssembly(assemblyName);
                if (assembly != null)
                {
                    instance = ReflectionUtils.GetStaticProperty(dbProviderFactoryTypename, InstanceMemberName);
                }
            }

            if (instance == null)
            {
                throw new InvalidOperationException($"Unable to retrieve DbProviderFactory for ''{dbProviderFactoryTypename}''");
            }

            return instance as DbProviderFactory;
        }

        public static DbProviderFactory GetDbProviderFactory(DataProviderType providerType, bool useAtrributeInfo)
        {
            DbProviderFactory providerFactory;
            ProviderAttribute attrib = providerType.GetAttribute<ProviderAttribute>();
            string factoryTypeName = attrib?.FactoryType;
            string assemblyName = attrib?.Invariant;
            if (useAtrributeInfo)
            {
                if (string.IsNullOrEmpty(factoryTypeName) && string.IsNullOrEmpty(assemblyName))
                {
                    throw new NotSupportedException($"Unsuported provider factory for {factoryTypeName} and {assemblyName}");
                }

                providerFactory = GetDbProviderFactory(factoryTypeName, assemblyName);
                if (providerFactory == null)
                {
                    throw new NotSupportedException($"Unsuported provider factory for {factoryTypeName} and {assemblyName}");
                }
            }
            else
            {
                switch (providerType)
                {
                    case DataProviderType.Access:
                        providerFactory = OleDbFactory.Instance;
                        break;
                    case DataProviderType.MySql:
                        providerFactory = MySqlClientFactory.Instance;
                        break;
                    case DataProviderType.Oracle:
                        providerFactory = OracleClientFactory.Instance;
                        break;
                    case DataProviderType.PostgreSql:
                        providerFactory = NpgsqlFactory.Instance;
                        break;
                    case DataProviderType.SqLite:
                        providerFactory = SQLiteFactory.Instance;
                        break;
                    case DataProviderType.SqlServer:
                        providerFactory = SqlClientFactory.Instance;
                        break;
                    default:
                        throw new NotSupportedException($"Unsuported provider type {providerType.ToString()}");
                }
            }

            return providerFactory;

            //            if (providerType == DataProviderType.SqlServer)
            //                return SqlClientFactory.Instance; // this library has a ref to SqlClient so this works

            //            if (providerType == DataProviderType.SqLite)
            //            {
            //#if NETFULL
            //        return GetDbProviderFactory("System.Data.SQLite.SQLiteFactory", "System.Data.SQLite");
            //#else
            //                return GetDbProviderFactory("Microsoft.Data.Sqlite.SqliteFactory", "Microsoft.Data.Sqlite");
            //#endif
            //            }
            //            if (providerType == DataProviderType.MySql)
            //                return GetDbProviderFactory("MySql.Data.MySqlClient.MySqlClientFactory", "MySql.Data");
            //            if (providerType == DataProviderType.PostgreSql)
            //                return GetDbProviderFactory("Npgsql.NpgsqlFactory", "Npgsql");
            //#if NETFULL
            //    if (type == DataAccessProviderTypes.OleDb)
            //        return System.Data.OleDb.OleDbFactory.Instance;
            //    if (type == DataAccessProviderTypes.SqlServerCompact)
            //        return DbProviderFactories.GetFactory("System.Data.SqlServerCe.4.0");                
            //#endif

            //            throw new NotSupportedException($"Unsuported provider factory for type {providerType.ToString()}");
        }

        public static DbProviderFactory GetDbProviderFactory(string connectionString, bool useAtrributeInfo)
        {
            /*From these examples you can say that you can define
             Oracle DB by Data Source substring, then tell the 
             difference between MySql and SQL Server by string 
             Uid and User Id respectively and so on.*/

        }

        public static DataProviderType GetProviderType(DbProviderFactory providerFactory)
        {
            string name = providerFactory.GetType().Name;
            List<DataProviderType> providers = ((DataProviderType[])Enum.GetValues(typeof(DataProviderType))).ToList();
            DataProviderType providerType = providers.SingleOrDefault(e =>
                e.GetAttributeValue<ProviderAttribute>(p => p.FactoryType).ToString()
                    .Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries)[0] == name);
            return providerType;
        }

        #endregion
    }
}