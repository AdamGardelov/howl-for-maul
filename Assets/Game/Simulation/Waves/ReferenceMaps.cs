using System;
using System.Collections.Generic;
namespace FrostMaze.Simulation
{
    public static class ReferenceMaps
    {
        // Source coordinates are row/column, top-left origin. World Y points toward the map's top.
        static V2 Point(int row,int col,int rows,float cell) => new V2((col+.5f)*cell,(rows-row-.5f)*cell);
        public static Scenario Rimewatch(string text)
        {
            var c=Parse("Rimewatch",text,1,".B","winter");
            V2 P(int r,int col)=>Point(r,col,64,1);
            // Mirrored corridor and shared exit are centred on the map.
            var exit=new V2(32,8.5f);
            c.Lanes=new[]{
                new LaneSpec{Spawn=P(8,9),GroundRoute=new[]{P(24,9),P(40,9),P(48,10),exit},FlightRoute=new[]{P(24,9),exit}},
                new LaneSpec{Spawn=P(8,30),GroundRoute=new[]{P(30,31),P(48,31),exit},FlightRoute=new[]{P(24,30),exit}},
                new LaneSpec{Spawn=P(8,55),GroundRoute=new[]{P(24,55),P(40,55),P(48,54),exit},FlightRoute=new[]{P(24,55),exit}}
            };
            c.BuilderStarts=new[]{P(15,9),P(15,30),P(15,55),P(31,10),P(31,54),P(48,20),P(48,43),P(52,31)};
            c.StartNames=new[]{"Upper left","Upper centre","Upper right","West crossing","East crossing","Lower west","Lower east","Last stand"};
            MirrorLane(c,0,2);
            c.Lanes[1].Spawn=new V2(32,c.Lanes[1].Spawn.Y);
            c.Lanes[1].GroundRoute=new[]{new V2(32,P(30,31).Y),new V2(32,P(48,31).Y),exit};
            c.Lanes[1].FlightRoute=new[]{new V2(32,P(24,30).Y),exit};
            c.BuilderStarts[2]=Mirror(c,c.BuilderStarts[0]);c.BuilderStarts[4]=Mirror(c,c.BuilderStarts[3]);c.BuilderStarts[6]=Mirror(c,c.BuilderStarts[5]);
            c.BuilderStarts[1]=new V2(32,c.BuilderStarts[1].Y);c.BuilderStarts[7]=new V2(32,c.BuilderStarts[7].Y);
            c.SoloBuilderStart=new V2(32,P(48,31).Y);Finish(c,exit);return c;
        }
        public static Scenario Ironfold(string text,bool mirrored=true)
        {
            var c=Parse("Ironfold",text,.5f,".,G","iron");
            V2 P(int r,int col)=>Point(r,col,128,.5f);
            var exit=mirrored?new V2(32,1.25f):P(125,63);
            c.Lanes=new[]{
                new LaneSpec{Spawn=P(2,22),GroundRoute=new[]{P(40,22),P(78,45),P(116,30),exit},FlightRoute=new[]{P(40,22),exit}},
                new LaneSpec{Spawn=P(2,49),GroundRoute=new[]{P(40,56),P(78,64),P(110,63),exit},FlightRoute=new[]{P(40,56),exit}},
                new LaneSpec{Spawn=P(2,79),GroundRoute=new[]{P(40,74),P(78,65),P(110,66),exit},FlightRoute=new[]{P(40,74),exit}},
                new LaneSpec{Spawn=P(2,106),GroundRoute=new[]{P(40,106),P(78,84),P(116,96),exit},FlightRoute=new[]{P(40,106),exit}}
            };
            c.BuilderStarts=new[]{P(15,22),P(15,49),P(15,79),P(15,106),P(78,45),P(78,64),P(78,84),P(116,64)};
            c.StartNames=new[]{"Upper west","Inner west","Inner east","Upper east","West bend","Central spine","East bend","Last stand"};
            // Archived crowd regressions retain the original routes and mask explicitly.
            if(!mirrored){c.SoloBuilderStart=P(110,63);Finish(c,exit);return c;}
            MirrorLane(c,0,3);MirrorLane(c,1,2);
            c.BuilderStarts[3]=Mirror(c,c.BuilderStarts[0]);c.BuilderStarts[2]=Mirror(c,c.BuilderStarts[1]);c.BuilderStarts[6]=Mirror(c,c.BuilderStarts[4]);
            c.BuilderStarts[5]=new V2(32,c.BuilderStarts[5].Y);c.BuilderStarts[7]=new V2(32,c.BuilderStarts[7].Y);
            c.SoloBuilderStart=new V2(32,P(110,63).Y);Finish(c,exit);return c;
        }
        static V2 Mirror(Scenario c,V2 p)=>new V2(c.Width-p.X,p.Y);
        static void MirrorLane(Scenario c,int from,int to){
            var lane=c.Lanes[from];var ground=new V2[lane.GroundRoute.Length];var air=new V2[lane.FlightRoute.Length];
            for(int i=0;i<ground.Length;i++)ground[i]=Mirror(c,lane.GroundRoute[i]);
            for(int i=0;i<air.Length;i++)air[i]=Mirror(c,lane.FlightRoute[i]);
            c.Lanes[to]=new LaneSpec{Spawn=Mirror(c,lane.Spawn),GroundRoute=ground,FlightRoute=air};
        }
        static void Finish(Scenario c,V2 exit)
        {
            if(c.Theme=="iron")RobotFactions.Apply(c);else Factions.Apply(c);CampaignProgression.Apply(c);MaulEconomy.Apply(c);c.Spawn=c.Lanes[0].Spawn;c.GroundRoute=new[]{exit};c.FlightRoute=new[]{exit};c.Validate();
        }
        static Scenario Parse(string name,string text,float cell,string walkable,string theme)
        {
            var rows=text.Replace("\r","").Trim().Split('\n');int width=rows[0].Length;
            foreach(var row in rows)if(row.Length!=width)throw new ArgumentException("Inconsistent layout row width.");
            var c=Scenario.SharedDefense();c.Name=name;c.Width=(int)Math.Ceiling(width*cell);c.Height=(int)Math.Ceiling(rows.Length*cell);
            c.LayoutRows=rows;c.LayoutCellSize=cell;c.WalkableSymbols=walkable;c.Theme=theme;c.SelectableMap=true;
            var blocks=new List<TerrainBlock>();var active=new Dictionary<string,TerrainBlock>();
            // Merge equal horizontal runs on adjacent rows. Preserve every source cell, including thin diagonal rims.
            for(int row=0;row<rows.Length;row++) {
                var next=new Dictionary<string,TerrainBlock>();
                for(int x=0;x<width;) {
                    char kind=rows[row][x];int start=x;while(x<width&&rows[row][x]==kind)x++;
                    if(walkable.IndexOf(kind)>=0)continue;
                    string key=start+":"+x+":"+kind;
                    if(active.TryGetValue(key,out var block)){block.Y=(rows.Length-row-1)*cell;block.Height+=cell;}
                    else {block=new TerrainBlock{X=start*cell,Y=(rows.Length-row-1)*cell,Width=(x-start)*cell,Height=cell,Kind=kind.ToString()};blocks.Add(block);}
                    next[key]=block;
                }
                active=next;
            }
            c.Terrain=blocks.ToArray();return c;
        }
    }
}
