// See https://aka.ms/new-console-template for more information
using BenchmarkDotNet.Running;
using MonitorSlim;

Console.WriteLine("2x faster monitor - Gabriel RABHI 2021");

BenchmarkRunner.Run<BenchMonitor>();

BenchmarkRunner.Run<BenchAverage>();