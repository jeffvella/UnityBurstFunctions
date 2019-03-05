# UnityBurstFunctions
Pattern and helpers for writing Unity Burst jobs as generic Functions/Actions.

Write burst compiled jobs with easy arguments and return values.

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


#### Pass by reference ('unmanaged' types only)

Interface:

		public interface IBurstRefAction<T1, T2, T3, T4, T5> : IBurstOperation
		{
			void Execute(ref T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);
		}

Implementation:

        [BurstCompile]
        public struct TryGetContact : IBurstRefAction<NativeManifold, RigidTransform, NativeHull, RigidTransform, NativeHull>
        {
            public void Execute(ref NativeManifold manifold, RigidTransform t1, NativeHull hull1, RigidTransform t2, NativeHull hull2)
            {
                HullIntersection.NativeHullHullContact(ref manifold, t1, hull1, t2, hull2);
            }

            public static bool Invoke(out NativeManifold result, RigidTransform t1, NativeHull hull1, RigidTransform t2, NativeHull hull2)
            {
                // Can only allocate as 'temp' within Burst Jobs 
                result = new NativeManifold(Allocator.Persistent); 

                BurstRefAction<TryGetContact, NativeManifold, RigidTransform, NativeHull, RigidTransform, NativeHull>.Run(Instance, ref result, t1, hull1, t2, hull2);
                return result.Length > 0;
            }

            public static TryGetContact Instance { get; } = new TryGetContact();
        }

Calling Example:

		var burstResult = HullOperations.TryGetContact.Invoke(out NativeManifold manifold, t1, hull1, t2, hull2);