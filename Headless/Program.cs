using System;
using FrostMaze.Tests;
class Program
{
    static int Main(string[] args)
    {
        if(args.Length>0&&args[0]=="--navigation-benchmark")return NavigationBenchmark.Run();
        if(args.Length>0&&args[0]=="--network-check")return NetworkChecks.Run();
        if(args.Length>0&&args[0]=="--network-peer")return NetworkChecks.Peer(int.Parse(args[1]));
        if(args.Length>0&&args[0]=="--balance") {
            try { return BalanceSweep.Run(args.Length>1?args[1]:"balance-results.json",args.Length>2?Enum.Parse<FrostMaze.Simulation.Difficulty>(args[2]):FrostMaze.Simulation.Difficulty.Normal,args.Length>3?int.Parse(args[3]):1,args.Length>4?args[4]:"coverage",args.Length>5&&args[5]=="mixed",args.Length>6?args[6]:"",args.Length>7?int.Parse(args[7]):-1,args.Length>8?Array.ConvertAll(args[8].Split(','),int.Parse):null); }
            catch(Exception e) when(e is ArgumentException||e is FormatException||e is OverflowException) {
                Console.Error.WriteLine("Invalid balance request: "+e.Message);return 2;
            }
        }
        int failed = 0;
        foreach (var test in SimulationCases.All)
        {
            try
            {
                var watch = System.Diagnostics.Stopwatch.StartNew();
                test.Run();
                Console.WriteLine("PASS  " + test.Name + " (" + watch.ElapsedMilliseconds + " ms)");
            }
            catch (Exception e) { failed++; Console.WriteLine("FAIL  " + test.Name + ": " + e.Message); }
        }
        Console.WriteLine($"{SimulationCases.All.Length - failed}/{SimulationCases.All.Length} passed");
        return failed == 0 ? 0 : 1;
    }
}
