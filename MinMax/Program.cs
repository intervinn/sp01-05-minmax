using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace MinMax
{
    public class MinMax
    {
        public class Result
        {
            public int Min { get; set; } = int.MaxValue;
            public int Max { get; set; } = int.MinValue;

            public override string ToString()
            {
                return $"{Min} - {Max}";
            }
        }

        public static async Task<Result> DoAsync(IEnumerable<int> ints)
        {
            using var cts = new CancellationTokenSource();
            var indexedInts = ints.AsParallel().Select((value, index) => (value, index));
            Result result = new();
            await Parallel.ForEachAsync(
                indexedInts,
                new ParallelOptions
                {
                    CancellationToken = cts.Token
                },
                async (item, token) =>
                {
                    if (item.index % 2 == 0)
                    {
                        if (item.value < result.Min)
                        {
                            result.Min = item.value;
                        }
                    }
                    else
                    {
                        if (item.value > result.Max)
                        {
                            result.Max = item.value;
                        }
                    }
                });
            return result;
        }

        public static IEnumerable<int> CreateRandomArray(int len, int min = int.MinValue, int max = int.MaxValue)
        {
            Random r = new();
            int[] res = new int[len];
            for (int i = 0; i < len; i++)
            {
                res[i] = r.Next(min, max);
            }
            return res;
        }
    }

    public class MinMaxBenchmark
    {
        private readonly int max = int.MaxValue;
        private readonly int min = int.MinValue;

        [Benchmark]
        public async Task<MinMax.Result> MinMax100()
        {
            return await MinMax.DoAsync(MinMax.CreateRandomArray(100, min, max));
        }

        [Benchmark]
        public async Task<MinMax.Result> MinMax1000()
        {
            return await MinMax.DoAsync(MinMax.CreateRandomArray(1000, min, max));
        }

        [Benchmark]
        public async Task<MinMax.Result> MinMax10000()
        {
            return await MinMax.DoAsync(MinMax.CreateRandomArray(10000, min, max));
        }

        [Benchmark]
        public async Task<MinMax.Result> MinMax10000000()
        {
            return await MinMax.DoAsync(MinMax.CreateRandomArray(10000000, min, max));
        }

        [Benchmark]
        public async Task<MinMax.Result> MinMaxMaxValue()
        {
            return await MinMax.DoAsync(MinMax.CreateRandomArray(int.MaxValue, min, max));
        }
    }

    public class Program
    {
        public static async Task Main()
        {
            BenchmarkRunner.Run<MinMaxBenchmark>();
        }
    }
}