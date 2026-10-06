#if UNITY_EDITOR
using NUnit.Framework;
namespace FrostMaze.Tests
{
    public sealed class UnityTests
    {
        public static System.Collections.IEnumerable Cases {get{foreach(var test in SimulationCases.All)yield return new TestCaseData(test).SetName(test.Name);}}
        [TestCaseSource(nameof(Cases))] public void Simulation(Case test){test.Run();}
    }
}
#endif
