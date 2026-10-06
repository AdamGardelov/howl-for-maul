using System;
using FrostMaze.Tests;
class Program
{
    static int Main()
    {
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
