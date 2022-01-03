# GhostFields.MonitorSlim
Up to 2x faster Monitor class for .Net, usefull for small and hot critical code sections.

Results :

BenchmarkDotNet=v0.13.1, OS=Windows 10.0.19042.1110 (20H2/October2020Update)
Intel Core i9-9900K CPU 3.60GHz (Coffee Lake), 1 CPU, 16 logical and 8 physical cores
.NET SDK=6.0.100
  [Host]     : .NET 6.0.0 (6.0.21.52210), X64 RyuJIT  [AttachedDebugger]
  DefaultJob : .NET 6.0.0 (6.0.21.52210), X64 RyuJIT


|       Method |      Mean |     Error |    StdDev |
|------------- |----------:|----------:|----------:|
| lock() | 13.315 ns | 0.0245 ns | 0.0229 ns |
| MonitorSlim.Enter / Exit |  6.114 ns | 0.0858 ns | 0.0803 ns |
