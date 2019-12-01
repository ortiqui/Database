using System.Linq;

namespace Database.Core.Console
{
    using System;
    using System.Data;
    using System.Data.Common;
    using Database.Core.Enums;

    class Program
    {
        static void Main(string[] args)
        {
            //DbProviderFactoryInitializer.Initialize();

            GetFactoryClasses();

            //DbProviderFactories.RegisterFactory("FirebirdSql.Data.FirebirdClient", FirebirdSql.Data.FirebirdClient.FirebirdClientFactory.Instance);
            foreach (var providerType in (DataProviderType[])Enum.GetValues(typeof(DataProviderType)))
            {
                TestProvider(providerType);
            }
            
            Console.WriteLine("Press a key for exit:");
            Console.ReadLine();
        }

        internal static void GetFactoryClasses()
        {
            Console.WriteLine("All registered DbProviderFactories:");
            var allFactoryClasses = DbProviderFactories.GetFactoryClasses();
            foreach (DataRow row in allFactoryClasses.Rows)
            {
                Console.WriteLine(row[0] + ": " + row[2]);
                Test(row);
            }
        }

        public static void TestProvider(DataProviderType providerType)
        {
            DatabaseCore db = DatabaseCore.Create(providerType, @"Data Source=(localdb)\MSSQLLocalDB;Database=ZKTecoTime;User ID=manuel.ortiz;Password=1910Skynet;Pooling=true;Encrypt=False;TrustServerCertificate=False;Trusted_Connection=Yes;");
            using (var cn = db.CreateOpenConnection())
            {
                cn.Open();
                cn.Close();
                cn.Dispose();
            }

        }

        internal static void Test(DataRow row)
        {
            DbProviderFactory factory = null;
            try
            {
                factory = DbProviderFactories.GetFactory(row);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return;
            }
            
            Console.WriteLine(factory);
        }

        internal static void Test(string provider)
        {
            DbProviderFactory factory = DbProviderFactories.GetFactory("MySql.Data.MySqlClient");
            System.Console.WriteLine(factory);
        }
    }
}
