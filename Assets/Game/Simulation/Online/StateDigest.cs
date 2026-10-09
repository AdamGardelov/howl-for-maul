using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Reflection;
using System.Linq;
namespace FrostMaze.Simulation.Online
{
    public static class StateDigest
    {
        public static string Scenario(Scenario scenario){using(var m=new MemoryStream())using(var w=new BinaryWriter(m)){Value(w,scenario);return Hash(m);}}
        static void Value(BinaryWriter w,object value){if(value==null){w.Write(false);return;}w.Write(true);var type=value.GetType();
            if(value is string text){w.Write(text);return;}if(value is int i){w.Write(i);return;}if(value is long l){w.Write(l);return;}if(value is float f){w.Write(f);return;}if(value is bool b){w.Write(b);return;}if(type.IsEnum){w.Write(Convert.ToInt32(value));return;}
            if(value is Array array){w.Write(array.Length);foreach(var item in array)Value(w,item);return;}
            foreach(var field in type.GetFields(BindingFlags.Public|BindingFlags.Instance).OrderBy(x=>x.Name,StringComparer.Ordinal)){w.Write(field.Name);Value(w,field.GetValue(value));}
        }
        public static string Of(World world){using(var m=new MemoryStream())using(var w=new BinaryWriter(m)){
            w.Write(world.Tick);w.Write(world.WaveIndex);w.Write(world.Pending);w.Write(world.Killed);w.Write(world.Leaked);
            foreach(var p in world.Players){w.Write(p.Gold);w.Write(p.Wood);w.Write(p.Faction);w.Write(p.UnlockedFactions);Value(w,p.Position);Value(w,p.Destination);Value(w,p.BuildOrder);w.Write(p.HasBuildOrder);w.Write(p.OrderedDesign);foreach(var task in p.Queue)Value(w,task);w.Write(-1);}
            foreach(var tower in world.Grid.Towers){Value(w,tower);w.Write(world.TowerOwner(tower.Id));w.Write(world.SaleRefund(tower.Id));}w.Write(-1);
            foreach(var enemy in world.Enemies)Value(w,enemy);return Hash(m);
        }}
        static string Hash(MemoryStream stream){using(var hash=SHA256.Create())return Convert.ToBase64String(hash.ComputeHash(stream.ToArray()));}
    }
}
