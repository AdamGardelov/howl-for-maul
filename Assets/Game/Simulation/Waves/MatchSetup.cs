using System;
namespace FrostMaze.Simulation
{
    [Serializable] public sealed class LaneSpec
    {
        public V2 Spawn;
        public V2[] GroundRoute, FlightRoute;
    }
    [Serializable] public sealed class TerrainBlock
    {
        public float X,Y,Width,Height;
        public string Kind="#";
        public V2 Center => new V2(X+Width*.5f,Y+Height*.5f);
        public V2 Half => new V2(Width*.5f,Height*.5f);
    }
    public enum Difficulty { Relaxed, Normal, Hard }
    public sealed class MatchOptions
    {
        public int[] Factions={0,0,0,0};
        public int PlayerCount=1;
        public Difficulty Difficulty=Difficulty.Normal;
        public int[] StartingPositions={0,1,2,3};
        public void Validate(int startCount=4)
        {
            if(PlayerCount<1||PlayerCount>4||!Enum.IsDefined(typeof(Difficulty),Difficulty)) throw new ArgumentException("Invalid match settings.");
            if(Factions==null||Factions.Length<PlayerCount)throw new ArgumentException("Missing faction selections.");
            if(StartingPositions==null||StartingPositions.Length<PlayerCount)throw new ArgumentException("Missing starting positions.");
            for(int i=0;i<PlayerCount;i++) {
                if(StartingPositions[i]<0||StartingPositions[i]>=startCount)throw new ArgumentException("Invalid starting position.");
                for(int j=0;j<i;j++)if(StartingPositions[i]==StartingPositions[j])throw new ArgumentException("Starting positions must be unique.");
            }
        }
    }
    public sealed class BuildTask { public float X,Y; public int Design; }
    public sealed class PlayerState
    {
        public readonly System.Collections.Generic.Queue<BuildTask> Queue=new System.Collections.Generic.Queue<BuildTask>();
        public int Faction;
        public int Gold, SelectedDesign, OrderedDesign;
        public V2 Position,Destination,BuildOrder;
        public bool HasBuildOrder;
        public string Notice="Builder ready.";
    }
}
