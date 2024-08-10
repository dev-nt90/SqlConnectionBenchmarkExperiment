using BenchmarkDotNet.Running;

namespace SqlConnectionBenchmark
{
	internal class Program
	{
		static void Main(string[] args)
		{
			var summary = BenchmarkRunner.Run<SqlQueryBenchmark>();
			Console.WriteLine(summary);
		}
	}
}
