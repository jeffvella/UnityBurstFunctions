# UnityBurstFunctions
Pattern and helpers for writing Unity Burst jobs as generic Functions/Actions.

* Write burst compiled jobs with easy arguments and return values.

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
        
then call it like a normal function.

     var intResult3 = MyFunctions.AddIntegersFuncX2.Invoke(5, 7);
        
#### View as normal in the Burst Inspector

<img src="https://i.imgur.com/Euj2xUd.jpg" target="_blank" />

#### Still reasonably fast.

<img src="https://i.imgur.com/y844kBw.jpg" target="_blank" />

#### Pass complicated arguments

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
