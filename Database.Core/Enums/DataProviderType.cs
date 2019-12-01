namespace Database.Core.Enums
{
    #region Usings

    using Database.Core.Attribute;

    #endregion
    
    public enum DataProviderType
    {
        [Provider(
            name: "SqlClient Data Provider",
            invariant: "System.Data.SqlClient",
            factoryType: "System.Data.SqlClient.SqlClientFactory, System.Data",
            description: ".Net Framework Data Provider for SqlServer")]
        SqlServer = 1,

        [Provider(
            name: "SQLite Data Provider",
            invariant: "System.Data.SQLite",
            factoryType: "System.Data.SQLite.SQLiteFactory, System.Data.SQLite",
            description: ".Net Framework Data Provider for SQLite")]
        SqLite = 2,

        [Provider(
            name: "MySQL Data Provider",
            invariant: "MySql.Data.MySqlClient",
            factoryType: "MySql.Data.MySqlClient.MySqlClientFactory, MySql.Data",
            description: ".Net Framework Data Provider for MySQL")]
        MySql = 3,

        [Provider(
            name: "Npgsql Data Provider",
            invariant: "Npgsql",
            factoryType: "Npgsql.NpgsqlFactory, Npgsql",
            description: ".NET Framework Data Provider for PostgreSQL")]
        PostgreSql = 4,

        [Provider(
            name: "Npgsql Data Provider",
            invariant: "System.Data.OleDb",
            factoryType: "System.Data.OleDbFactory, System.Data",
            description: ".NET Framework Data Provider for OLE DB")]
        Access = 5,

        [Provider(
            name: "Oracle Data Provider for .NET",
            invariant: "Oracle.DataAccess.Client",
            factoryType: "Oracle.DataAccess.Client.OracleClientFactory,Oracle.DataAccess",
            description: ".NET Framework Oracle Data Provider for .NET")]
        Oracle = 6
    }
}