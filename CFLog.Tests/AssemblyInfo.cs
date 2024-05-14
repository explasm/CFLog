//[assembly: CollectionBehavior(CollectionBehavior.CollectionPerAssembly)]
using System.Diagnostics.CodeAnalysis;

[assembly: CollectionBehavior(DisableTestParallelization = true)]
[assembly: SuppressMessage("xUnit", "xUnit1031", Justification = "Parallelization is disabled")]