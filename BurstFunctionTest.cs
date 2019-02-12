using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Assets.UnityXBurst.Helpers;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;
using Debug = UnityEngine.Debug;


namespace Assets
{
    public class BurstFunctionTest : MonoBehaviour
    {
        public unsafe void Start()
        {
            var timer = new Timer();

            for (int i = 0; i < 1000; i++)
            {
                if (i == 20) timer.IsEnabled = true;

                using (var input1 = new NativeArray<int>(100000, Allocator.TempJob))
                using (var input2 = new NativeArray<int>(100000, Allocator.TempJob))
                {
                    timer.Restart();
                    timer.Stop();
                    timer.Save("Timer No Work Baseline");

                    var sw1 = new Stopwatch();
                    sw1.Start();
                    sw1.Stop();
                    timer.Timings.Add(("StopWatch No Work Baseline", sw1.Elapsed));

                    // -----------------------------------
                    // Standard C# method.

            
                    var instance = new MyFunctions.AddIntegersCSharp();
                    timer.Restart();
                    var result0 = instance.Execute(5, 7);
                    timer.Stop();
                    timer.Save("AddIntegers(PureC#)");

                    // -----------------------------------
                    // Normal Burst Job construction.

                    int intResult1 = 0;
                    var addIntegersJobWithResult = new MyFunctions.AddIntegersJobWithResult
                    {
                        arg1 = 5,
                        arg2 = 7,
                        resultPtr = &intResult1
                    };
                    timer.Restart();
                    addIntegersJobWithResult.Schedule().Complete();
                    timer.Stop();
                    timer.Save("AddIntegersJob(NonBurst): with result");

                    // -----------------------------------
                    // Normal Burst job without a return result;

                    var addIntegersJob = new MyFunctions.AddIntegersJob
                    {
                        arg1 = 5,
                        arg2 = 7,
                    };
                    timer.Restart();
                    addIntegersJob.Schedule().Complete();
                    timer.Stop();
                    timer.Save("AddIntegersJob(NonBurst): no result");

                    // -----------------------------------
                    // Function in burst, maintains references, uses GetAddress/CopyPtrToStructure.

                    timer.Restart();
                    var intResult3 = MyFunctions.AddIntegersFuncX2.Invoke(5, 7);
                    timer.Stop();
                    timer.Save("AddIntegersFunc(Burst):");

                    // -----------------------------------
                    // Uses no jobs code, just pure c# performing the same logic.

                    var instanceArr = new MyFunctions.ArrayTestCSharp();
                    timer.Restart();
                    instanceArr.Execute(input1, input2);
                    timer.Stop();
                    timer.Save("ArrayTest(PureC#)");

                    // -----------------------------------
                    // Uses the Function Job

                    NativeArray<int> arrResult;
                    timer.Restart();
                    arrResult = MyFunctions.ArrayTestFunc.Invoke(input1, input2);
                    timer.Stop();
                    timer.Save("ArrayTestFunc(Burst)");

                    // -----------------------------------
                    // Uses a standard Job construction, same as Function version except it has no result.

                    var compiledArr = new MyFunctions.ArrayTestJob
                    {
                        arg1 = input1,
                        arg2 = input2,
                    };
                    timer.Restart();
                    compiledArr.Schedule().Complete();
                    timer.Stop();
                    timer.Save("ArrayTestJob(Burst):ScheduleComplete");

                    // -----------------------------------
                    // Uses a standard Job construction fired with Run(); returns no result.

                    compiledArr = new MyFunctions.ArrayTestJob
                    {
                        arg1 = input1,
                        arg2 = input2,
                    };
                    timer.Restart();
                    compiledArr.Run();
                    timer.Stop();
                    timer.Save("ArrayTestJob(Burst):Run");

                }

            }

            timer.LogAverages();

        }
    }

    public class Timer : IEnumerable<(string, TimeSpan)>
    {
        public Stopwatch Stopwatch = new Stopwatch();

        public List<(string, TimeSpan)> Timings = new List<(string, TimeSpan)>();

        public void Start() => Stopwatch.Start();

        public void Restart() => Stopwatch.Restart();

        public void Stop() => Stopwatch.Stop();

        public TimeSpan Elapsed => Stopwatch.Elapsed;

        public void Save(string name)
        {
            if (IsEnabled)
            {
                Timings.Add((name, Stopwatch.Elapsed));
            }
        }

        public void Save(string name, TimeSpan time)
        {
            if (IsEnabled)
            {
                Timings.Add((name, time));
            }
        }

        public void LogAverages()
        {
            foreach (var group in Timings.GroupBy(t => t.Item1))
            {
                Debug.Log($"{group.Key}: {group.Average(t => t.Item2.TotalMilliseconds):N8} ms");
            }
        }

        public bool IsEnabled { get; set; }

        public IEnumerator<(string, TimeSpan)> GetEnumerator() => Timings.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public class MyFunctions
    {
        public struct AddIntegersCSharp
        {
            public int Execute(int a, int b)
            {
                return a + b;
            }
        }

        [BurstCompile]
        public struct AddIntegersJob : IJob
        {
            public int arg1;
            public int arg2;

            public void Execute()
            {
                arg1 = arg1 + arg2;
            }
        }

        [BurstCompile]
        public struct AddIntegersJobWithResult : IJob
        {
            public int arg1;
            public int arg2;

            [NativeDisableUnsafePtrRestriction]
            public unsafe int* resultPtr;

            public unsafe void Execute()
            {
                ref var result = ref *resultPtr;
                result = arg1 + arg2;
            }
        }

        [BurstCompile]
        public struct ArrayTestJob : IJob
        {
            public NativeArray<int> arg1;
            public NativeArray<int> arg2;

            public void Execute()
            {
                for (int i = 0; i < 100000; i++)
                {
                    arg1[i] = i;
                }
                for (int i = 0; i < 100000; i++)
                {
                    arg2[i] = arg1[i];
                }
            }
        }

        [BurstCompile]
        public struct AddIntegersFuncX2 : IBurstFunction<int, int, int>
        {
            public int Execute(int a, int b)
            {
                return a + b;
            }

            public static int Invoke(int a, int b)
            {
                return BurstFunction<AddIntegersFuncX2, int, int, int>.Run(Instance, a, b);
            }

            private static readonly AddIntegersFuncX2 Instance = new AddIntegersFuncX2();
        }


        [BurstCompile]
        public struct ArrayTestFunc : IBurstFunction<NativeArray<int>, NativeArray<int>, NativeArray<int>>
        {
            public NativeArray<int> Execute(NativeArray<int> arg1, NativeArray<int> arg2)
            {
                for (int i = 0; i < 100000; i++)
                {
                    arg1[i] = i;
                }
                for (int i = 0; i < 100000; i++)
                {
                    arg2[i] = arg1[i];
                }
                return arg2;
            }

            public static NativeArray<int> Invoke(NativeArray<int> a, NativeArray<int> b)
            {
                return BurstFunction<ArrayTestFunc, NativeArray<int>, NativeArray<int>, NativeArray<int>>.Run(Instance, a, b);
            }

            public static ArrayTestFunc Instance { get; } = new ArrayTestFunc();
        }

        public struct ArrayTestCSharp
        {
            public NativeArray<int> Execute(NativeArray<int> arg1, NativeArray<int> arg2)
            {
                for (int i = 0; i < 100000; i++)
                {
                    arg1[i] = i;
                }
                for (int i = 0; i < 100000; i++)
                {
                    arg2[i] = arg1[i];
                }
                return arg2;
            }
        }
    }



}
