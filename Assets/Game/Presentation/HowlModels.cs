using System.Collections.Generic;
using UnityEngine;
namespace FrostMaze
{
    // Eight authored bodies, compiled once per map into shared, articulated batches.
    // Spawning copies only a handful of render nodes, never primitive colliders or meshes.
    public sealed class HowlModels
    {
        public sealed class Part { public Mesh Mesh; public int Joint,Material; }
        public sealed class Model { public Part[] Parts;public float GroundExtent,Height; }
        readonly ModelMeshes meshes;readonly bool winter;
        readonly Dictionary<HowlForm,Model> cache=new Dictionary<HowlForm,Model>();
        readonly Dictionary<int,List<CombineInstance>> pieces=new Dictionary<int,List<CombineInstance>>();
        public HowlModels(ModelMeshes meshes,bool winter){this.meshes=meshes;this.winter=winter;}
        public Model Get(HowlForm form)
        {
            if(cache.TryGetValue(form,out var model))return model;
            pieces.Clear();
            switch(form) {
                case HowlForm.Hearthgnawer: Scavenger();break;
                case HowlForm.Thornrunner: Hound();break;
                case HowlForm.Ashling: Swarm();break;
                case HowlForm.Cairnback: Boar();break;
                case HowlForm.HollowWarden: Warden();break;
                case HowlForm.Gloamwing: Moth();break;
                case HowlForm.Gatebreaker: Giant();break;
                case HowlForm.StormHerald: Herald();break;
            }
            // Regional surface details live inside the same already-fitted envelope.
            if(winter&&(form==HowlForm.Hearthgnawer||form==HowlForm.Thornrunner))
                for(int i=0;i<3;i++)Leaf(new Vector3(0,form==HowlForm.Thornrunner?.90f:.74f,-.12f-i*.13f),new Vector3(.28f,.26f,.14f),0,1);
            if(winter&&form==HowlForm.Cairnback)
                for(int side=-1;side<=1;side+=2)Bone(new Vector3(side*.39f,.72f,.15f),Vector3.down,.21f,.075f,1);
            var parts=new List<Part>();float extent=.01f,height=.01f;
            foreach(var pair in pieces) {
                int joint=pair.Key/8,material=pair.Key%8;
                var mesh=meshes.Combine("Howl/"+(winter?"rime":"iron")+"/"+form+"/"+pair.Key,pair.Value);
                parts.Add(new Part{Mesh=mesh,Joint=joint,Material=material});
                // Gait rotates before body scaling. Attack/recoil twist around the vertical axis,
                // allowing tall creatures without overhanging their collision discs.
                float sine=Mathf.Sin((joint>0?24:0)*Mathf.Deg2Rad);
                foreach(var v in mesh.vertices) {
                    height=Mathf.Max(height,v.y);float z=Mathf.Abs(v.z)+Mathf.Abs(v.y)*sine;
                    extent=Mathf.Max(extent,Mathf.Sqrt(v.x*v.x+z*z));
                }
            }
            model=new Model{Parts=parts.ToArray(),GroundExtent=extent,Height=height};cache.Add(form,model);return model;
        }
        public void Warm(){for(int i=0;i<8;i++)Get((HowlForm)i);}
        void Add(Mesh mesh,Vector3 p,Vector3 s,int mat=0,int joint=0,Quaternion? rotation=null)
        {
            int key=joint*8+mat;if(!pieces.TryGetValue(key,out var list)){list=new List<CombineInstance>();pieces.Add(key,list);}
            list.Add(new CombineInstance{mesh=mesh,transform=Matrix4x4.TRS(p,rotation??Quaternion.identity,s)});
        }
        void Flesh(Vector3 p,Vector3 s,int mat=0,int joint=0)=>Add(meshes.Shell,p,s,mat,joint);
        void Stone(Vector3 p,Vector3 s,int mat=1,int joint=0)=>Add(meshes.Shell,p,s,mat,joint);
        void Bone(Vector3 p,Vector3 d,float h,float w,int mat=1,int joint=0)=>Add(meshes.Thorn,p,new Vector3(w,h,w),mat,joint,Quaternion.FromToRotation(Vector3.up,d));
        void Limb(Vector3 a,Vector3 b,float w,int mat=0,int joint=0)=>Add(meshes.Shell,(a+b)*.5f,new Vector3(w,(b-a).magnitude+w*.4f,w),mat,joint,Quaternion.FromToRotation(Vector3.up,b-a));
        void Leaf(Vector3 p,Vector3 s,float tilt,int mat=2,int joint=0)=>Add(meshes.Leaf,p,s,mat,joint,Quaternion.Euler(80,0,tilt));
        void Eyes(Vector3 p,float spread,float size=.075f)
        {
            for(int side=-1;side<=1;side+=2){Flesh(p+new Vector3(side*spread,0,0),new Vector3(size*1.7f,size*1.25f,size*.55f),4);Flesh(p+new Vector3(side*spread,0,.024f),new Vector3(size,size*.65f,size*.4f),3);}
        }
        void Legs(float spread,float front,float rear,float hip,float width)
        {
            for(int side=-1;side<=1;side+=2)for(int i=0;i<2;i++) {
                int joint=(side==(i==0?1:-1))?1:2;float z=i==0?front:rear;
                Limb(new Vector3(side*spread*.7f,hip,z),new Vector3(side*spread,.12f,z+.06f),width,0,joint);
                Flesh(new Vector3(side*spread,.085f,z+.12f),new Vector3(width*1.25f,.15f,width*1.65f),0,joint);
            }
        }
        void Scavenger()
        {
            // Hunched, long-muzzled burrower with a shaggy back and folded ears.
            Flesh(new Vector3(0,.42f,-.15f),new Vector3(.80f,.70f,.88f));
            Flesh(new Vector3(0,.38f,.37f),new Vector3(.52f,.40f,.44f));
            Flesh(new Vector3(0,.28f,.59f),new Vector3(.31f,.22f,.38f),1);
            Flesh(new Vector3(0,.31f,.76f),new Vector3(.21f,.14f,.14f),4);Eyes(new Vector3(0,.47f,.54f),.18f);
            Legs(.33f,.24f,-.36f,.38f,.18f);
            for(int s=-1;s<=1;s+=2)Leaf(new Vector3(s*.26f,.63f,.19f),new Vector3(.30f,.44f,.23f),s*28,0);
            for(int i=0;i<4;i++)Bone(new Vector3(0,.67f,-.04f-i*.15f),new Vector3(0,.6f,-1),.30f,.25f,2);
            Bone(new Vector3(0,.30f,-.56f),new Vector3(.3f,.2f,-1),.35f,.17f,0);
        }
        void Hound()
        {
            // Narrow hunting body, lifted shoulders, hooked thorn mane, trailing tail.
            Flesh(new Vector3(0,.64f,-.18f),new Vector3(.48f,.47f,.97f));Flesh(new Vector3(0,.76f,.24f),new Vector3(.53f,.66f,.51f));
            Flesh(new Vector3(0,.91f,.46f),new Vector3(.39f,.34f,.42f));Flesh(new Vector3(0,.82f,.69f),new Vector3(.29f,.20f,.42f),1);
            Eyes(new Vector3(0,.97f,.63f),.13f);Legs(.25f,.28f,-.45f,.60f,.13f);
            for(int s=-1;s<=1;s+=2){Bone(new Vector3(s*.16f,1.01f,.29f),new Vector3(s*.25f,1,-.25f),.35f,.18f,0);
                for(int i=0;i<3;i++)Bone(new Vector3(s*.22f,.78f-i*.04f,.13f-i*.19f),new Vector3(s*.6f,.9f,-.8f),.32f,.12f,2);}
            Bone(new Vector3(0,.62f,-.62f),new Vector3(.13f,.1f,-1),.56f,.20f,0);
        }
        void Swarm()
        {
            // A low, six-legged bramble mite: split carapace, mandibles and antennae.
            Flesh(new Vector3(0,.28f,-.08f),new Vector3(.94f,.45f,.76f),2);
            for(int s=-1;s<=1;s+=2){Flesh(new Vector3(s*.20f,.36f,-.15f),new Vector3(.39f,.30f,.61f),1);
                for(int i=0;i<3;i++){int j=(i+(s==1?1:0))%2+1;float z=.32f-i*.3f;
                    Limb(new Vector3(s*.28f,.25f,z),new Vector3(s*.59f,.12f,z+.13f),.10f,0,j);}
                Bone(new Vector3(s*.18f,.24f,.44f),new Vector3(-s*.3f,0,1),.28f,.15f,1);
                Bone(new Vector3(s*.17f,.40f,.29f),new Vector3(s*.35f,1,.6f),.35f,.045f,0);}
            Eyes(new Vector3(0,.36f,.42f),.14f,.08f);
        }
        void Boar()
        {
            // Broad boar under overlapping boulders. Tusks, blunt snout, no metal shield.
            Flesh(new Vector3(0,.50f,-.1f),new Vector3(1.06f,.81f,1.05f));
            Flesh(new Vector3(0,.46f,.49f),new Vector3(.77f,.61f,.60f));Flesh(new Vector3(0,.35f,.77f),new Vector3(.48f,.30f,.20f),4);
            Legs(.39f,.30f,-.38f,.47f,.23f);Eyes(new Vector3(0,.60f,.69f),.27f);
            for(int s=-1;s<=1;s+=2){Bone(new Vector3(s*.29f,.30f,.70f),new Vector3(s*.15f,1,.2f),.44f,.19f,1);
                for(int i=0;i<3;i++)Stone(new Vector3(s*.24f,.87f-i*.035f,.12f-i*.30f),new Vector3(.52f,.37f,.46f),1);
                Leaf(new Vector3(s*.36f,.97f,-.30f),new Vector3(.32f,.35f,.20f),s*35,2);}
        }
        void Warden()
        {
            // Tall, hollow bark mask and tattered mantle; two feet and crooked arms.
            Add(meshes.Robe,new Vector3(0,.69f,0),new Vector3(.79f,1.14f,.57f),0);
            Flesh(new Vector3(0,1.32f,.13f),new Vector3(.52f,.64f,.35f),1);Eyes(new Vector3(0,1.38f,.31f),.13f);
            Bone(new Vector3(0,1.01f,.25f),Vector3.down,.20f,.22f,1);
            for(int s=-1;s<=1;s+=2){
                Limb(new Vector3(s*.28f,1.04f,0),new Vector3(s*.45f,.57f,.20f),.15f,0,s==1?1:2);
                Bone(new Vector3(s*.45f,.59f,.20f),Vector3.down,.34f,.19f,0,s==1?1:2);
                Limb(new Vector3(s*.19f,.31f,0),new Vector3(s*.20f,.07f,.07f),.16f,0,s==1?1:2);
                Bone(new Vector3(s*.18f,1.55f,.01f),new Vector3(s*.38f,1,-.12f),.57f,.14f,2);
                Bone(new Vector3(s*.29f,1.78f,0),new Vector3(s,1,.1f),.26f,.07f,1);
                Leaf(new Vector3(s*.31f,.57f,-.16f),new Vector3(.28f,.76f,.19f),s*30,2);}
        }
        void Moth()
        {
            Flesh(new Vector3(0,.15f,0),new Vector3(.27f,.36f,.84f));Flesh(new Vector3(0,.22f,.36f),new Vector3(.34f,.29f,.29f),1);Eyes(new Vector3(0,.24f,.50f),.09f,.06f);
            for(int s=-1;s<=1;s+=2){int j=s<0?1:2;
                Add(meshes.HowlWing(s,true),new Vector3(s*.12f,.10f,0),Vector3.one,1,j);
                Add(meshes.HowlWing(s,true),new Vector3(s*.14f,.12f,0),new Vector3(.94f,1,.93f),2,j);
                for(int vein=0;vein<3;vein++)Limb(new Vector3(s*.24f,.25f,0),new Vector3(s*(1.18f-vein*.19f),.17f,.31f-vein*.45f),.022f,1,j);
                Flesh(new Vector3(s*.60f,.23f,.11f),new Vector3(.27f,.045f,.24f),1,j);
                Bone(new Vector3(s*.09f,.27f,.40f),new Vector3(s*.5f,.2f,1),.44f,.045f,1);}
        }
        void Giant()
        {
            // Upright ram-headed root-and-stone giant, enormous fists and broken crown.
            Flesh(new Vector3(0,.92f,0),new Vector3(1.14f,1.34f,.78f));Stone(new Vector3(0,1.28f,.19f),new Vector3(.88f,.56f,.50f),1);
            Flesh(new Vector3(0,1.78f,.13f),new Vector3(.58f,.61f,.48f),1);Eyes(new Vector3(0,1.82f,.38f),.16f,.09f);
            for(int s=-1;s<=1;s+=2){int j=s<0?1:2;
                Limb(new Vector3(s*.32f,.58f,0),new Vector3(s*.35f,.14f,.05f),.30f,0,j);
                Flesh(new Vector3(s*.36f,.14f,.15f),new Vector3(.40f,.25f,.51f),0,j);
                Limb(new Vector3(s*.51f,1.26f,0),new Vector3(s*.74f,.60f,.16f),.38f,0,j);
                Stone(new Vector3(s*.74f,.55f,.24f),new Vector3(.50f,.47f,.55f),0,j);
                Stone(new Vector3(s*.50f,1.39f,-.06f),new Vector3(.63f,.46f,.59f),1);
                Bone(new Vector3(s*.23f,1.95f,.04f),new Vector3(s*.5f,1,-.24f),.55f,.25f,2);
                Bone(new Vector3(s*.43f,2.20f,-.08f),new Vector3(-s*.3f,.4f,.7f),.30f,.15f,1);}
            Bone(new Vector3(0,1.57f,.34f),Vector3.down,.33f,.26f,2);
        }
        void Herald()
        {
            // Long-necked corvid with a pale antler mask, layered feathers and forked tail.
            Flesh(new Vector3(0,.30f,-.06f),new Vector3(.53f,.71f,.81f));
            Limb(new Vector3(0,.39f,.23f),new Vector3(0,.91f,.37f),.24f);
            Flesh(new Vector3(0,1.05f,.39f),new Vector3(.43f,.47f,.37f),1);Eyes(new Vector3(0,1.11f,.57f),.12f,.065f);
            Bone(new Vector3(0,.98f,.56f),new Vector3(0,-.25f,1),.37f,.20f,1);
            for(int s=-1;s<=1;s+=2){int j=s<0?1:2;
                Add(meshes.HowlWing(s,false),new Vector3(s*.18f,.43f,.01f),Vector3.one,2,j);
                for(int feather=0;feather<5;feather++)Limb(new Vector3(s*(.4f+feather*.12f),.58f,.12f),new Vector3(s*(.73f+feather*.19f),.48f,-.66f+feather*.08f),.038f,1,j);
                Bone(new Vector3(s*.15f,1.21f,.27f),new Vector3(s*.42f,1,-.18f),.54f,.08f,1);
                Bone(new Vector3(s*.28f,1.47f,.24f),new Vector3(s,.7f,0),.26f,.05f,2);
                Leaf(new Vector3(s*.19f,.17f,-.72f),new Vector3(.30f,.87f,.13f),s*15,2);
                Bone(new Vector3(s*.16f,.05f,.13f),new Vector3(0,-.7f,.3f),.28f,.08f,1);}
        }
    }
}
