# Gramma.Parallel
This library emulates a subset of PLINQ for long-running tasks. The reason for introducing such a library is that PLINQ relies on the standard `TaskScheduler` which in turn uses the .NET thread pool. Long running tasks with default settings have the tendency to starve the thread pool. The library creates the worker tasks with `TaskCreationOptions.LongRunning` to overcome the problem.

In order to use the library, open the `Gramma.Parallel` namespace, invoke the `AsLongParallel` extension method to any `IEnumerable<T>`. You can then optionally invoke `WithCancellation` and `WithDegreeOfParallelism` as in standard PLINQ. Currently only `Where` and `Select` are natively supported. The other LINQ methods are available through the default .NET implementations.

