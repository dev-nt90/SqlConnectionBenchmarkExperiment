using Microsoft.Data.SqlClient;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Running;

namespace SqlConnectionBenchmark
{
	
	[SimpleJob(RunStrategy.Throughput, launchCount: 1, warmupCount: 5, invocationCount: 50)]
	public class SqlQueryBenchmark
	{
		private readonly string _connectionString = @"Server=(local)\SQLSERVER2019_D;Initial Catalog=Sandbox;Integrated Security=True;TrustServerCertificate=true";
		private readonly string complexQuery = @"
WITH CTE_Sales AS (
    SELECT
        SalesPersonID,
        SUM(SalesAmount) AS TotalSales,
        COUNT(*) AS NumberOfSales
    FROM
        SalesTable
    GROUP BY
        SalesPersonID
),
CTE_Rank AS (
    SELECT
        SalesPersonID,
        TotalSales,
        NumberOfSales,
        RANK() OVER (ORDER BY TotalSales DESC) AS SalesRank
    FROM
        CTE_Sales
)
SELECT
    CTE_Rank.SalesPersonID,
    CTE_Rank.TotalSales,
    CTE_Rank.NumberOfSales,
    CTE_Rank.SalesRank,
    (SELECT COUNT(*) FROM SalesTable WHERE SalesAmount > 1000) AS HighValueSalesCount,
    AVG(TotalSales) OVER() AS AverageSalesAmount,
    CASE
        WHEN TotalSales > 10000 THEN 'High Performer'
        ELSE 'Regular Performer'
    END AS PerformanceCategory
FROM
    CTE_Rank
WHERE
    SalesRank <= 10
ORDER BY
    TotalSales DESC;
";

		private SqlConnection _connection;

		[GlobalSetup(Targets = new[] { nameof(ReusedConnectionQuery), nameof(ReusedConnectionComplexQuery) })]
		public void Setup()
		{
			_connection = new SqlConnection(_connectionString);
			_connection.Open();
		}

		[Benchmark]
		public void ReusedConnectionQuery()
		{
			using var command = new SqlCommand("SELECT * FROM dbo.Submissions", _connection);
			using var reader = command.ExecuteReader();
			while (reader.Read())
			{
				// Process each row (no-op)
			}
		}

		[Benchmark]
		public void NewConnectionEveryTimeQuery()
		{
			using var connection = new SqlConnection(_connectionString);
			connection.Open();
			using var command = new SqlCommand("SELECT * FROM dbo.Submissions", connection);
			using var reader = command.ExecuteReader();
			while (reader.Read())
			{
				// Process each row (no-op)
			}
		}

		[Benchmark]
		public void ReusedConnectionComplexQuery()
		{
			using var command = new SqlCommand(complexQuery, _connection);
			using var reader = command.ExecuteReader();
			while (reader.Read())
			{
				// Process each row (no-op)
			}
		}

		[Benchmark]
		public void NewConnectionComplexQuery()
		{
			using var connection = new SqlConnection(_connectionString);
			connection.Open();
			using var command = new SqlCommand(complexQuery, connection);
			using var reader = command.ExecuteReader();
			while (reader.Read())
			{
				// Process each row (no-op)
			}
		}

		[GlobalCleanup(Target = nameof(ReusedConnectionQuery))]
		
		public void ReusedConnectionQueryCleanup()
		{
			_connection.Close();
			_connection.Dispose();
		}

		[GlobalCleanup(Target = nameof(ReusedConnectionComplexQuery))]
		public void ReusedConnectionComplexQueryCleanup()
		{
			_connection.Close();
			_connection.Dispose();
		}
	}


}
