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
        [Params(10000)]
        public int Max;
        [Params(-10000)]
        public int Min;
        [Params(100_000, 1_000_000)] // от миллиарда подвисает комп + долго считает
        public int N;

        public IEnumerable<int> Data;

        [GlobalSetup]
        public void Setup()
        {
            Data = MinMax.CreateRandomArray(N, Min, Max);
        }

        [Benchmark]
        public MinMax.Result Sync()
        {
            return MinMax.DoSync(Data);
        }

        [Benchmark]
        public MinMax.Result PLINQ()
        {
            return MinMax.DoPLINQ(Data);
        }

        [Benchmark]
        public MinMax.Result Parallel()
        {
            return MinMax.DoParallel(Data);
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