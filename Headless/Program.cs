using System;
using FrostMaze.Tests;
class Program
{
    static int Main(string[] args)
    {
        if(args.Length>0&&args[0]=="--balance")return BalanceSweep.Run(args.Length>1?args[1]:"balance-results.json",args.Length>2?Enum.Parse<FrostMaze.Simulation.Difficulty>(args[2]):FrostMaze.Simulation.Difficulty.Normal,args.Length>3?int.Parse(args[3]):1);
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
