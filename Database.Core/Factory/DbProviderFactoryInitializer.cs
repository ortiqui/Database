namespace Database.Core.Factory
{
    #region Usings

    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Configuration;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using Database.Core.Attribute;
    using Database.Core.Enums;
    using Database.Core.Utils;

    #endregion

    public static class DbProviderFactoryInitializer
    {
        #region Campos

        private const string SystemDataSectionName = "system.data";

        private const string DbProviderFactoriesTable = "DbProviderFactories";

        private const string InvariantNameCellName = "InvariantName";

        private static DataSet systemData;

        #endregion

        #region Métodos

        public static void Initialize()
        {
            LoadSectionSystemData();
            //Ensure table exists
            if (systemData.Tables.IndexOf(DbProviderFactoriesTable) > -1)
            {
                ////remove existing provider factory
                //if (systemData.Tables[systemData.Tables.IndexOf("DbProviderFactories")].Rows.Find("System.Data.SQLite") != null)
                //{
                //    systemData.Tables[systemData.Tables.IndexOf("DbProviderFactories")].Rows.Remove(
                //        systemData.Tables[systemData.Tables.IndexOf("DbProviderFactories")].Rows.Find("System.Data.SQLite")
                //    );
                //}
                // Remove existing provider factories
                systemData.Tables[systemData.Tables.IndexOf("DbProviderFactories")].Rows.Clear();
            }
            else
            {
                systemData.Tables.Add(DbProviderFactoriesTable);
            }

            List<DataProviderType> dbTypes = ((DataProviderType[])Enum.GetValues(typeof(DataProviderType))).ToList();
            foreach (DataProviderType dbType in dbTypes)
            {
                ProviderAttribute pi = dbType.GetAttribute<ProviderAttribute>();
                if (pi == null)
                {
                    continue;
                }

                systemData.RegisterDbProvider(pi.Invariant, pi.Description, pi.Name, pi.FactoryType, null);
            }

            ////Add provider factory with our assembly in it.
            //systemData.Tables[systemData.Tables.IndexOf("DbProviderFactories")].Rows.Add("SQLite Data Provider"
            //    , ".NET Framework Data Provider for SQLite"
            //    , "System.Data.SQLite"
            //    , "System.Data.SQLite.SQLiteFactory, System.Data.SQLite"
            //);
        }

        internal static DataSet LoadSectionSystemData()
        {
            systemData = ConfigurationManager.GetSection(SystemDataSectionName) as DataSet;
            if (systemData == null)
            {
                throw new ArgumentNullException(nameof(systemData));
            }

            return systemData;
        }

        internal static bool RegisterDbProvider(this DataSet ds, string invariant, string description, string name, string typeFactory, string assemblyFQN)
        {
            try
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    if (row[InvariantNameCellName].ToString() == invariant)
                    {
                        return true;
                    }
                }

                Type assemblyType = Type.GetType(!string.IsNullOrEmpty(assemblyFQN) ? assemblyFQN : typeFactory);
                if (assemblyType != null)
                {
                    Assembly ass = Assembly.GetAssembly(assemblyType);
                    assemblyFQN = ass.FullName;
                    ds.Tables[0].Rows.Add(name, description, invariant, $"{assemblyFQN}");
                    return true;
                }

                return false;
            }
            catch (Exception e)
            {
                Debug.Print(e.Message);
            }

            return false;
        }

        #endregion
    }
}