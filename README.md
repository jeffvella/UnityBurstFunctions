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
        
* View them like normal jobs in the Burst Inspector

[[https://i.imgur.com/Euj2xUd.jpg|alt=Shows up in burst inspector]]
