using System;
namespace FrostMaze.Simulation
{
    // Immutable end-of-wave income, not wallet differences (players can spend during combat).
    public sealed class WaveSummary
    {
        public int WaveNumber { get; }
        public int Killed { get; }
        public int Leaked { get; }
        public bool Cleared { get; }
        public int TeamGold { get; }
        readonly int[] playerGold;
        internal WaveSummary(int waveNumber,int killed,int leaked,bool cleared,int[] gold)
        {
            WaveNumber=waveNumber;Killed=killed;Leaked=leaked;Cleared=cleared;
            playerGold=(int[])gold.Clone();foreach(int amount in playerGold)TeamGold+=amount;
        }
        public int GoldForPlayer(int player)
        {
            if(player<0||player>=playerGold.Length)throw new ArgumentOutOfRangeException(nameof(player));
            return playerGold[player];
        }
    }
}
