using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
namespace FrostMaze.Simulation.Online
{
    public enum Stage { Lobby, Factions, Lanes, Difficulty, Match }
    public enum Kind { Challenge, Hello, Welcome, Lobby, Ready, Begin, Faction, Lane, Difficulty, Command, Frame, PauseVote, Notice, Leave, Kick, Ping }
    public enum ActionKind { Build, Move, Sell, Upgrade, Launch, Cancel }
    public sealed class Member { public int Id,Faction=-1,Lane=-1,Vote=-1;public string Name="";public bool Ready,Connected=true,PauseVote; }
    public sealed class Order { public ActionKind Kind;public int Player,Design,Target;public float X,Y;public bool Append; }
    public sealed class Packet { public Kind Kind;public int A,B,C;public long Tick;public bool Flag;public string Text="",Extra="";public Member[] Members=Array.Empty<Member>();public Order[] Orders=Array.Empty<Order>(); }
    public static class Protocol
    {
        public const string Version="howl-direct-1";
        public const int MaxBytes=16384;
        public static byte[] Encode(Packet p){using(var m=new MemoryStream()){using(var w=new BinaryWriter(m,Encoding.UTF8,true)){
            w.Write((byte)p.Kind);w.Write(p.A);w.Write(p.B);w.Write(p.C);w.Write(p.Tick);w.Write(p.Flag);w.Write(p.Text??"");w.Write(p.Extra??"");w.Write(p.Members.Length);
            foreach(var q in p.Members){w.Write(q.Id);w.Write(q.Faction);w.Write(q.Lane);w.Write(q.Vote);w.Write(q.Name);w.Write(q.Ready);w.Write(q.Connected);w.Write(q.PauseVote);}
            w.Write(p.Orders.Length);foreach(var q in p.Orders){w.Write((byte)q.Kind);w.Write(q.Player);w.Write(q.Design);w.Write(q.Target);w.Write(q.X);w.Write(q.Y);w.Write(q.Append);}
        }if(m.Length>MaxBytes)throw new InvalidDataException("Packet too large");return m.ToArray();}}
        public static Packet Decode(byte[] data){if(data.Length>MaxBytes)throw new InvalidDataException();using(var m=new MemoryStream(data))using(var r=new BinaryReader(m,Encoding.UTF8)){
            var p=new Packet{Kind=(Kind)r.ReadByte(),A=r.ReadInt32(),B=r.ReadInt32(),C=r.ReadInt32(),Tick=r.ReadInt64(),Flag=r.ReadBoolean(),Text=r.ReadString(),Extra=r.ReadString()};
            if(!Enum.IsDefined(typeof(Kind),p.Kind)||p.Text.Length>2048||p.Extra.Length>2048)throw new InvalidDataException();
            int n=r.ReadInt32();if(n<0||n>4)throw new InvalidDataException();p.Members=new Member[n];
            for(int i=0;i<n;i++)p.Members[i]=new Member{Id=r.ReadInt32(),Faction=r.ReadInt32(),Lane=r.ReadInt32(),Vote=r.ReadInt32(),Name=r.ReadString(),Ready=r.ReadBoolean(),Connected=r.ReadBoolean(),PauseVote=r.ReadBoolean()};
            n=r.ReadInt32();if(n<0||n>64)throw new InvalidDataException();p.Orders=new Order[n];for(int i=0;i<n;i++)p.Orders[i]=new Order{Kind=(ActionKind)r.ReadByte(),Player=r.ReadInt32(),Design=r.ReadInt32(),Target=r.ReadInt32(),X=r.ReadSingle(),Y=r.ReadSingle(),Append=r.ReadBoolean()};
            if(m.Position!=m.Length)throw new InvalidDataException();return p;
        }}
        public static string Nonce(){var bytes=new byte[32];using(var rng=RandomNumberGenerator.Create())rng.GetBytes(bytes);return Convert.ToBase64String(bytes);}
        public static string Proof(string password,string nonce){using(var k=new Rfc2898DeriveBytes(password??"",Encoding.UTF8.GetBytes(nonce),10000,HashAlgorithmName.SHA256))using(var h=new HMACSHA256(k.GetBytes(32)))return Convert.ToBase64String(h.ComputeHash(Encoding.UTF8.GetBytes(Version+nonce)));}
        public static bool Equal(string a,string b){if(a==null||b==null||a.Length!=b.Length)return false;int diff=0;for(int i=0;i<a.Length;i++)diff|=a[i]^b[i];return diff==0;}
    }
}
