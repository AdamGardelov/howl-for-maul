using UnityEngine;
using FrostMaze.Simulation;
namespace FrostMaze
{
    [CreateAssetMenu(menuName = "FrostMaze/Map", fileName = "TestMap")]
    public sealed class MapDefinition : ScriptableObject
    {
        public Scenario Settings = new Scenario();
    }
}
