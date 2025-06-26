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



        public static Result DoSync(IEnumerable<int> ints)
        {
            Result result = new();
            var i = 0;
            foreach (var v in ints)
            {
                if (i % 2 == 0)
                {
                    if (v < result.Min)
                    {
                        lock (result)
                        {
                            result.Max = v;
                        }
                    }
                } else
                {
                    if (v > result.Max)
                    {
                        lock (result)
                        {
                            result.Min = v;
                        }
                    }
                }
                i++;
            }
            return result;
        }

        public static Result DoPLINQ(IEnumerable<int> ints)
        {
            var query = ints.AsParallel();
            return new Result()
            {
                Max = query.Where((i, v) => i % 2 != 0).Max(),
                Min = query.Where((i, v) => i % 2 == 0).Min()
            };
        }

        public static Result DoParallel(IEnumerable<int> ints)
        {
            Result result = new();
            Parallel.ForEach(ints, (v, state, i) =>
            {
                if (i % 2 == 0)
                {
                    if (v < result.Min)
                    {
                        lock (result)
                        {
                            result.Min = v;
                        }
                    }
                }
                else
                {
                    if (v > result.Max)
                    {
                        lock (result)
                        {
                            result.Max = v;
                        }
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
        private readonly int _max = int.MaxValue;
        private readonly int _min = int.MinValue;

        [Benchmark]
        public MinMax.Result Sync1mil()
        {
            return MinMax.DoSync(MinMax.CreateRandomArray(1_000_000, _min, _max));
        }

        [Benchmark]
        public MinMax.Result Sync2bil()
        {
            return MinMax.DoSync(MinMax.CreateRandomArray(2_000_000_000, _min, _max));
        }

        [Benchmark]
        public MinMax.Result PLinq1mil()
        {
            return MinMax.DoPLINQ(MinMax.CreateRandomArray(1_000_000, _min, _max));
        }

        [Benchmark]
        public MinMax.Result Plinq2bil()
        {
            return MinMax.DoPLINQ(MinMax.CreateRandomArray(2_000_000_000, _min, _max));
        }

        [Benchmark]
        public MinMax.Result Parallel1mil()
        {
            return MinMax.DoParallel(MinMax.CreateRandomArray(1_000_000, _min, _max));
        }

        [Benchmark]
        public MinMax.Result Parallel2bil()
        {
            return MinMax.DoParallel(MinMax.CreateRandomArray(2_000_000_000, _min, _max));
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