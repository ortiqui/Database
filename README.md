# Database

Database agnostic ADO.NET wrapper + lambda-based SQL query builder for .NET.

## Proyectos

| Proyecto | Tipo | Target | Descripción |
|---|---|---|---|
| `Database.Core` | Class Library | .NET Framework 4.7.2 | Wrapper ADO.NET agnóstico al proveedor |
| `Database.Core.Console` | Console App | .NET Framework 4.7.2 | Harness de prueba / demo |
| `LambdaSql` | Class Library | .NET Framework 4.0 | Generador de SQL fluido y fuertemente tipado |
| `LambdaSql.Analyzers` | Roslyn Analyzer | .NET Standard 1.3 | Analyzer que fuerza inmutabilidad en tiempo de compilación |

## Database.Core

Wrapper alrededor de `DbProviderFactory` que abstrae la conexión, comandos, transacciones y parámetros. Soporta 6 proveedores:

- SQL Server
- SQLite
- MySQL (via MySqlConnector)
- PostgreSQL (via Npgsql)
- Access (OLE DB)
- Oracle (ODP)

### Uso básico

```csharp
using (var db = DatabaseCore.Create(
    DataProviderType.SqlServer,
    "Server=(localdb)\\MSSQLLocalDB;Database=Demo;Trusted_Connection=True;"))
{
    db.OpenConnection();
    var result = db.ExecuteScalar("SELECT COUNT(*) FROM Products");
}
```

## LambdaSql

Generador de consultas SQL `SELECT` mediante una API fluida con expresiones lambda. Diseñado para usarse con micro-ORMs como Dapper.

### Uso básico

```csharp
var query = SqlSelect<Order>.From()
    .Where(SqlFilter<Order>.From(o => o.Status).EqualTo("Shipped"))
    .OrderBy(o => o.OrderDate);

Console.WriteLine(query.ParametricSql);
// SELECT [Order].[Id], [Order].[Status], [Order].[OrderDate]
// FROM [Order]
// WHERE [Order].[Status] = @p0
// ORDER BY [Order].[OrderDate]
```

### Características

- SELECT, DISTINCT, WHERE, HAVING, GROUP BY, ORDER BY
- INNER / LEFT / RIGHT / FULL JOIN
- TOP (SQL Server, via extension)
- Subconsultas
- Filtros parametrizados o raw
- Aliases de tabla y campo
- Funciones de agregación (MIN, MAX, AVG, SUM, COUNT)
- Totalmente inmutable — cada método retorna una nueva instancia

## LambdaSql.Analyzers

Analyzer de Roslyn que genera un error en compilación (`LSql1000`) cuando se invoca un método de `SqlSelect<T>` o `SqlFilter<T>` sin asignar su valor de retorno.

```csharp
// Error LSql1000: el valor de retorno se ignora
query.Where(...);
```

## Dependencias

```
Database.Core.Console
    └── Database.Core
            ├── LambdaSql
            └── LambdaSql.Analyzers
```

## Patrones de diseño

| Patrón | Ubicación |
|---|---|
| Abstract Factory | `DbProviderFactoryProvider` |
| Factory Method | `DatabaseCore.Create()`, `SqlFilter.From()` |
| Immutable Object / Builder | `SqlSelect<T>`, `SqlSelectInfo` |
| Fluent Interface | `SqlSelect<T>`, `SqlFilter<T>` |
| Strategy / Plugin | `IMetadataProvider` |
| Roslyn Analyzer | `ImmutableAnalyzer` |

## Requisitos

- .NET Framework 4.7.2 (Database.Core, Database.Core.Console)
- .NET Framework 4.0 (LambdaSql)
- .NET Standard 1.3 (LambdaSql.Analyzers)
- Visual Studio 2019 o superior

## Créditos

LambdaSql fue publicado originalmente como [NuGet package](https://www.nuget.org/packages/LambdaSql/) por [Serg046](https://github.com/Serg046/LambdaSql).
