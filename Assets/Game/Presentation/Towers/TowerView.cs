using UnityEngine;
using System.Collections.Generic;
using FrostMaze.Simulation;
namespace FrostMaze
{
    // All shapes are original, cosmetic and kept inside the simulation footprint.
    public sealed class TowerView : MonoBehaviour
    {
        public string Role { get; private set; }
        public int VisibleLevel { get; private set; }
        Transform weapon;
        Prototype game;
        long observedShot,shotTick=-100;

        GameObject[] tiers=new GameObject[2];
        Material shell,accent,light;
        readonly Dictionary<PrimitiveType,Mesh> silhouettes=new Dictionary<PrimitiveType,Mesh>();
        Mesh Faceted(PrimitiveType kind) {
            if(silhouettes.TryGetValue(kind,out var found))return found;
            var vertices=new List<Vector3>();var triangles=new List<int>();
            float[] heights=kind==PrimitiveType.Cylinder?new[]{-1f,-.78f,.78f,1f}:new[]{-.5f,-.32f,.2f,.44f,.5f};
            float[] radii=kind==PrimitiveType.Cylinder?new[]{.4f,.5f,.5f,.4f}:new[]{0f,.36f,.47f,.27f,0f};
            for(int ring=0;ring<heights.Length-1;ring++)for(int side=0;side<8;side++) {
                float a=side*Mathf.PI/4,b=(side+1)*Mathf.PI/4;
                int n=vertices.Count;
                vertices.Add(new Vector3(Mathf.Cos(a)*radii[ring],heights[ring],Mathf.Sin(a)*radii[ring]));
                vertices.Add(new Vector3(Mathf.Cos(a)*radii[ring+1],heights[ring+1],Mathf.Sin(a)*radii[ring+1]));
                vertices.Add(new Vector3(Mathf.Cos(b)*radii[ring+1],heights[ring+1],Mathf.Sin(b)*radii[ring+1]));
                vertices.Add(new Vector3(Mathf.Cos(b)*radii[ring],heights[ring],Mathf.Sin(b)*radii[ring]));
                triangles.AddRange(new[]{n,n+1,n+2,n,n+2,n+3});
            }
            if(kind==PrimitiveType.Cylinder)for(int end=0;end<2;end++)for(int side=0;side<8;side++) {
                float a=side*Mathf.PI/4,b=(side+1)*Mathf.PI/4,y=end==0?-1:1;int n=vertices.Count;
                vertices.Add(new Vector3(0,y,0));vertices.Add(new Vector3(Mathf.Cos(a)*.4f,y,Mathf.Sin(a)*.4f));vertices.Add(new Vector3(Mathf.Cos(b)*.4f,y,Mathf.Sin(b)*.4f));
                triangles.AddRange(end==0?new[]{n,n+1,n+2}:new[]{n,n+2,n+1});
            }
            var mesh=new Mesh{name="Original faceted tower "+kind};mesh.SetVertices(vertices);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();silhouettes.Add(kind,mesh);return mesh;
        }
        void OnDestroy(){foreach(var mesh in silhouettes.Values)Destroy(mesh);}
        GameObject Part(string name,PrimitiveType kind,Vector3 p,Vector3 scale,Material material,Transform parent=null)
        {
            var o=GameObject.CreatePrimitive(kind);o.name=name;o.transform.SetParent(parent==null?transform:parent,false);
            o.transform.localPosition=p;o.transform.localScale=scale;o.GetComponent<Renderer>().sharedMaterial=material;
            if(kind==PrimitiveType.Cylinder||kind==PrimitiveType.Sphere)o.GetComponent<MeshFilter>().sharedMesh=Faceted(kind);
            var collider=o.GetComponent<Collider>();collider.enabled=false;Destroy(collider);return o;
        }
        public void Initialize(Prototype game,Tower tower,TowerDesign design,int faction)
        {
            this.game=game;
            var shots=game.World.Shots;
            observedShot=shots.Count>0?shots[shots.Count-1].Serial:0;
            var spec=tower.Spec;bool robot=game.World.Config.Theme=="iron";
            var palette=game.TowerPalette(faction);shell=palette[0];accent=palette[1];light=palette[2];
            Role=spec.Damage<=0?"Wall":design!=null&&design.Requires.Length>0?"Champion":!spec.TargetsGround?"Interceptor":spec.SlowFraction>0?"Control":spec.ChainTargets>0?"Relay":spec.SplashRadius>0?"Artillery":"Sentry";
            Part("Foundation",PrimitiveType.Cylinder,new Vector3(0,.1f,0),new Vector3(.94f,.13f,.94f),shell);
            Part("Faction band",PrimitiveType.Cylinder,new Vector3(0,.23f,0),new Vector3(.78f,.035f,.78f),accent);
            if(!robot&&Role!="Wall")for(int side=-1;side<=1;side+=2) {
                Part("Stone buttress",PrimitiveType.Cube,new Vector3(side*.31f,.38f,-.05f),new Vector3(.16f,.43f,.48f),shell);
            }
            if(Role=="Wall") {
                Part("Fortified wall",PrimitiveType.Cube,new Vector3(0,.42f,0),new Vector3(.8f,.5f,.8f),shell);
                for(int x=-1;x<=1;x++)Part("Battlement",PrimitiveType.Cube,new Vector3(x*.27f,.75f,0),new Vector3(.19f,.22f,.7f),accent);
            } else {
                var pivot=new GameObject(Role+" weapon");pivot.transform.SetParent(transform,false);weapon=pivot.transform;
                Part(robot?"Armored torso":"Pedestal",robot?PrimitiveType.Cube:PrimitiveType.Cylinder,new Vector3(0,.6f,0),new Vector3(.5f,.38f,.48f),shell,weapon);
                if(Role=="Artillery") {
                    var barrel=Part("Mortar barrel",PrimitiveType.Cylinder,new Vector3(0,.95f,.14f),new Vector3(.46f,.37f,.46f),accent,weapon);barrel.transform.localRotation=Quaternion.Euler(30,0,0);
                    Part("Mortar aperture",PrimitiveType.Sphere,new Vector3(0,1.27f,.33f),new Vector3(.29f,.07f,.29f),light,weapon);
                } else if(Role=="Interceptor") {
                    for(int side=-1;side<=1;side+=2){var rail=Part("Sky rail",PrimitiveType.Cube,new Vector3(side*.23f,1.06f,.08f),new Vector3(.12f,.75f,.16f),accent,weapon);rail.transform.localRotation=Quaternion.Euler(25,0,0);}
                    Part("Targeting lens",PrimitiveType.Sphere,new Vector3(0,.95f,.29f),Vector3.one*.24f,light,weapon);
                } else if(Role=="Control") {
                    var crystal=Part("Control crystal",PrimitiveType.Cube,new Vector3(0,1.1f,0),Vector3.one*.43f,light,weapon);crystal.transform.localRotation=Quaternion.Euler(0,45,35);
                    for(int side=-1;side<=1;side+=2)Part("Crystal prong",PrimitiveType.Cylinder,new Vector3(side*.29f,.92f,0),new Vector3(.08f,.34f,.08f),accent,weapon);
                } else if(Role=="Relay") {
                    for(int side=-1;side<=1;side+=2){Part("Coil mast",PrimitiveType.Cylinder,new Vector3(side*.24f,.96f,0),new Vector3(.12f,.38f,.12f),accent,weapon);Part("Arc node",PrimitiveType.Sphere,new Vector3(side*.24f,1.38f,0),Vector3.one*.25f,light,weapon);}
                } else {
                    Part(robot?"Helmet":"Runestone crown",robot?PrimitiveType.Sphere:PrimitiveType.Cylinder,new Vector3(0,1.02f,0),robot?new Vector3(.48f,.43f,.42f):new Vector3(.53f,.14f,.53f),accent,weapon);
                    if(!robot)for(int side=-1;side<=1;side+=2)Part("Crown merlon",PrimitiveType.Cube,new Vector3(side*.2f,1.22f,0),new Vector3(.14f,.2f,.36f),shell,weapon);
                    Part("Visor",PrimitiveType.Cube,new Vector3(0,1.04f,.2f),new Vector3(.34f,.1f,.07f),light,weapon);
                    var arm=Part("Arm cannon",PrimitiveType.Cylinder,new Vector3(.28f,.78f,.2f),new Vector3(.22f,.31f,.22f),accent,weapon);arm.transform.localRotation=Quaternion.Euler(90,0,0);
                    if(Role=="Champion")for(int side=-1;side<=1;side+=2){Part("Champion shoulder",PrimitiveType.Cube,new Vector3(side*.3f,1.06f,0),Vector3.one*.3f,accent,weapon);Part("Champion crown",PrimitiveType.Cube,new Vector3(side*.16f,1.38f,0),new Vector3(.1f,.3f,.12f),light,weapon);}
                }
            }
            for(int i=0;i<2;i++)tiers[i]=Part("Upgrade tier "+(i+2),PrimitiveType.Cube,new Vector3((i==0?-1:1)*.31f,.34f,-.35f),new Vector3(.13f,.15f,.1f),light);
            Sync(tower,false);
        }
        public void Sync(Tower tower,bool clearance)
        {
            transform.position=new Vector3(tower.Center.X,0,tower.Center.Y);
            transform.localScale=new Vector3(tower.Spec.Width,clearance?.08f:1,tower.Spec.Height);
            VisibleLevel=tower.Level;for(int i=0;i<2;i++)tiers[i].SetActive(tower.Level>=i+2);
            if(weapon!=null) {
                weapon.localScale=Vector3.one*(1+.08f*(tower.Level-1));
                var shots=game.World.Shots;
                // Only direct shots from this footprint drive its weapon. Chain origins are enemies.
                for(int i=shots.Count-1;i>=0&&shots[i].Serial>observedShot;i--) {
                    var shot=shots[i];
                    if(shot.Chained||V2.Distance(shot.From,tower.Center)>.001f)continue;
                    var direction=shot.To-shot.From;
                    if(direction.Length>.001f)weapon.localRotation=Quaternion.Euler(0,Mathf.Atan2(direction.X,direction.Y)*Mathf.Rad2Deg,0);
                    shotTick=game.World.Tick;break;
                }
                if(shots.Count>0)observedShot=shots[shots.Count-1].Serial;
                // Simulation ticks freeze in pause/setup and naturally follow the speed controls.
                float recoil=Mathf.Clamp01(1-(game.World.Tick-shotTick)/6f)*(Role=="Artillery"?.14f:.09f);
                weapon.localPosition=-(weapon.localRotation*Vector3.forward)*recoil;
            }
        }
    }
}
