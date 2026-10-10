using UnityEngine;
namespace FrostMaze
{
    public sealed partial class TowerView
    {
        enum CreatureForm { Owl,Bear,Deer,Toad,Crane,Newt,Coalcrab,Phoenix,Ape,Jay,Ram,Heron,Roc,
            Beetle,Stagbeetle,Bombardier,Mantis,Dragonfly,Scorpion,Colossus,
            Fox,Wisps,Runestone,Serpent,Moth,Seer,Sphinx,Gust,Hare,Hawk,Chime,Gryphon,
            Tortoise,StoneRam,StoneApe,Cairn,Ibex,StoneBear,Ox,
            Wisp,Walker,Umbrella,BellSpirit,MoonMoth,Ancestor,Lantern,
            Hatchling,CoilDrake,Spitter,Ramjaw,Wyvern,TwinDrake,Wyrm,Crab,Snail,Hydra,Kingfisher,Jelly,Shellback }
        static readonly CreatureForm[][] winterForms={
            new[]{CreatureForm.Owl,CreatureForm.Bear,CreatureForm.Deer,CreatureForm.Toad,CreatureForm.Crane},
            null,
            new[]{CreatureForm.Newt,CreatureForm.Coalcrab,CreatureForm.Toad,CreatureForm.Phoenix,CreatureForm.Ape},
            new[]{CreatureForm.Jay,CreatureForm.Ram,CreatureForm.Deer,CreatureForm.Heron,CreatureForm.Roc}
        };
        static readonly CreatureForm[][] ironForms={null,
            new[]{CreatureForm.Beetle,CreatureForm.Stagbeetle,CreatureForm.Bombardier,CreatureForm.Mantis,CreatureForm.Dragonfly,CreatureForm.Scorpion,CreatureForm.Colossus},
            new[]{CreatureForm.Fox,CreatureForm.Wisps,CreatureForm.Runestone,CreatureForm.Serpent,CreatureForm.Moth,CreatureForm.Seer,CreatureForm.Sphinx},
            new[]{CreatureForm.Owl,CreatureForm.Gust,CreatureForm.Hare,CreatureForm.Crane,CreatureForm.Hawk,CreatureForm.Chime,CreatureForm.Gryphon},
            new[]{CreatureForm.Tortoise,CreatureForm.StoneRam,CreatureForm.StoneApe,CreatureForm.Cairn,CreatureForm.Ibex,CreatureForm.StoneBear,CreatureForm.Ox},
            new[]{CreatureForm.Wisp,CreatureForm.Walker,CreatureForm.Umbrella,CreatureForm.BellSpirit,CreatureForm.MoonMoth,CreatureForm.Ancestor,CreatureForm.Lantern},
            new[]{CreatureForm.Hatchling,CreatureForm.CoilDrake,CreatureForm.Spitter,CreatureForm.Ramjaw,CreatureForm.Wyvern,CreatureForm.TwinDrake,CreatureForm.Wyrm},
            new[]{CreatureForm.Crab,CreatureForm.Snail,CreatureForm.Hydra,CreatureForm.Toad,CreatureForm.Kingfisher,CreatureForm.Jelly,CreatureForm.Shellback}
        };
        Transform creatureHead,creatureGesture;
        Vector3 creatureHeadRest;
        bool creature,creatureStone;
        int creatureFaction,creatureSlot;
        public string CreatureIdentity {get;private set;}
        Material ivory,shadow;
        GameObject Flesh(string name,Vector3 p,Vector3 size,Material material,Transform parent=null)
        {
            var part=Part(name,PrimitiveType.Sphere,p,size,material,parent??weapon);
            if(creatureStone&&material==shell)part.GetComponent<MeshFilter>().sharedMesh=game.Models.Armor;
            return part;
        }
        void Limb(string name,Vector3 a,Vector3 b,float width,Material material,Transform parent=null)
        {
            var piece=Flesh(name,(a+b)*.5f,new Vector3(width,(b-a).magnitude+width*.45f,width),material,parent);
            piece.transform.localRotation=Quaternion.FromToRotation(Vector3.up,b-a);
        }
        void Curl(string name,Vector3[] points,float width,Material material,Transform parent=null)
        {
            var part=Flesh(name,Vector3.zero,Vector3.one,material,parent);
            part.GetComponent<MeshFilter>().sharedMesh=game.Models.CurvedPipe("Creature/"+game.World.Config.Theme+"/"+Subject.Design+"/"+name,points,width);
        }
        void Feather(string name,Vector3 p,Vector3 size,float tilt,Material material,Transform parent=null)
        {
            var piece=Flesh(name,p,size,material,parent);piece.GetComponent<MeshFilter>().sharedMesh=game.Models.Leaf;
            piece.transform.localRotation=Quaternion.Euler(12,0,tilt);
        }
        void Horn(string name,Vector3 p,Vector3 direction,float length,float width,Material material,Transform parent=null)
        {
            var part=Flesh(name,p,new Vector3(width,length,width),material,parent);
            part.GetComponent<MeshFilter>().sharedMesh=game.Models.Thorn;part.transform.localRotation=Quaternion.FromToRotation(Vector3.up,direction);
        }
        void Face(Vector3 p,Vector3 size,bool beak=false,float muzzle=.13f)
        {
            creatureHead.localPosition=creatureHeadRest=p;
            Flesh("Expressive head",Vector3.zero,size,shell,creatureHead);
            for(int side=-1;side<=1;side+=2){
                var eye=new Vector3(side*size.x*.24f,size.y*.06f,size.z*.43f);
                Flesh("Eye socket",eye,new Vector3(.11f,.11f,.055f),shadow,creatureHead);
                Flesh("Living eye",eye+Vector3.forward*.028f,new Vector3(.049f,.055f,.022f),light,creatureHead);
                Limb("Brow",eye+new Vector3(-.055f,.083f,0),eye+new Vector3(.055f,.062f,0),.06f,ivory,creatureHead);
            }
            if(beak)Horn("Beak",new Vector3(0,-.065f,size.z*.40f),new Vector3(0,-.15f,1),muzzle,.18f,ivory,creatureHead);
            else if(muzzle>0){
                Flesh("Muzzle",new Vector3(0,-size.y*.17f,size.z*.41f),new Vector3(size.x*.65f,size.y*.35f,muzzle*2),ivory,creatureHead);
                Flesh("Nose",new Vector3(0,-size.y*.11f,size.z*.40f+muzzle*.88f),new Vector3(size.x*.21f,.065f,.05f),shadow,creatureHead);
            }
        }
        void Feet(float y,float spread,float depth,bool four=true)
        {
            for(int side=-1;side<=1;side+=2)for(int end=0;end<(four?2:1);end++){
                float z=four?(end==0?-depth:depth):.08f;
                Limb("Planted leg",new Vector3(side*spread,y,z),new Vector3(side*(spread+.03f),.16f,z+.035f),.19f,shell);
                Flesh("Paw",new Vector3(side*(spread+.03f),.12f,z+.08f),new Vector3(.25f,.19f,.30f),ivory);
            }
        }
        void Bird(CreatureForm form)
        {
            bool longNeck=form==CreatureForm.Crane||form==CreatureForm.Heron;
            bool royal=form==CreatureForm.Roc||form==CreatureForm.Phoenix;
            float h=longNeck?1.28f:royal?1.00f:.76f;
            Feet(longNeck?.73f:.38f,longNeck?.13f:.20f,0,false);
            Flesh("Bird breast",new Vector3(0,longNeck?.80f:.53f,0),new Vector3(royal?.58f:.48f,royal?.69f:.57f,.47f),shell);
            if(longNeck)Curl("Long curved neck",new[]{new Vector3(0,.69f,0),new Vector3(-.13f,1.01f,.03f),new Vector3(0,1.28f,.08f)},.17f,shell);
            Face(new Vector3(0,h,.14f),new Vector3(longNeck?.31f:.45f,longNeck?.32f:.40f,.35f),true,longNeck?.38f:.20f);
            for(int side=-1;side<=1;side+=2){
                float flare=royal?70:form==CreatureForm.Hawk?48:25;
                for(int i=0;i<(royal?4:3);i++)Feather("Flight feather",new Vector3(side*(.20f+i*.065f),.58f+i*.10f,-.12f),new Vector3(.29f,royal?.79f:.49f,.20f),-side*flare, i%2==0?accent:ivory,creatureGesture);
                if(form==CreatureForm.Owl)Feather("Owl ear",new Vector3(side*.17f,.18f,-.02f),new Vector3(.17f,.32f,.14f),-side*20,accent,creatureHead);
            }
            if(form!=CreatureForm.Owl)for(int i=-1;i<=1;i++)Feather("Swept crest",new Vector3(i*.07f,.23f,-.04f),new Vector3(.12f,.37f,.14f),i*14,accent,creatureHead);
            for(int i=-1;i<=1;i++)Feather("Tail plume",new Vector3(i*.09f,.43f,-.31f),new Vector3(.21f,royal?.76f:.43f,.18f),i*28,accent,creatureGesture);
        }
        void Quadruped(CreatureForm form)
        {
            bool deer=form==CreatureForm.Deer||form==CreatureForm.Ibex;
            bool small=form==CreatureForm.Fox||form==CreatureForm.Hare;
            bool ram=form==CreatureForm.Ram||form==CreatureForm.StoneRam||form==CreatureForm.Ox;
            bool stoneRam=form==CreatureForm.StoneRam;
            float belly=deer?.64f:stoneRam?.66f:small?.44f:.39f;
            Feet(belly,deer?.18f:.25f,deer?.20f:.16f);
            Flesh("Creature body",new Vector3(0,belly,-.05f),new Vector3(deer||stoneRam?.47f:small?.50f:.76f,deer||stoneRam?.47f:.61f,deer?.66f:.72f),shell);
            Face(new Vector3(0,deer?1.02f:stoneRam?1.13f:small?.82f:.71f,.21f),new Vector3(deer?.33f:small?.39f:.52f,.40f,.38f),false,small?.17f:.15f);
            if(stoneRam)Limb("Ram chest",new Vector3(0,.58f,.13f),new Vector3(0,1.09f,.19f),.29f,shell);
            if(deer)Limb("Upright neck",new Vector3(0,.61f,.13f),new Vector3(0,1.00f,.18f),.24f,shell);
            for(int side=-1;side<=1;side+=2){
                if(ram){
                    Curl("Curling horn "+side,new[]{new Vector3(side*.18f,.13f,0),new Vector3(side*(stoneRam?.46f:.36f),stoneRam?.39f:.20f,-.05f),new Vector3(side*(stoneRam?.47f:.35f),-.10f,.05f),new Vector3(side*.25f,-.06f,.13f)},form==CreatureForm.Ox?.18f:.13f,ivory,creatureHead);
                }else if(deer){
                    Horn("High antler",new Vector3(side*.13f,.14f,-.02f),new Vector3(side*.22f,1,-.17f),form==CreatureForm.Ibex?.63f:.44f,.17f,ivory,creatureHead);
                    if(form==CreatureForm.Deer)for(int i=0;i<2;i++)Horn("Antler tine",new Vector3(side*(.15f+i*.04f),.27f+i*.12f,-.04f),new Vector3(side*.8f,.8f,0),.23f,.10f,accent,creatureHead);
                }else if(small)Feather("Long ear",new Vector3(side*.16f,.24f,-.03f),new Vector3(.24f,form==CreatureForm.Hare?.70f:.36f,.16f),side*14,ivory,creatureHead);
                else Flesh("Round ear",new Vector3(side*.22f,.15f,0),new Vector3(.20f,.21f,.16f),accent,creatureHead);
            }
            if(small)Curl("Full tail",new[]{new Vector3(0,.41f,-.28f),new Vector3(-.27f,.43f,-.40f),new Vector3(-.31f,.77f,-.22f)},form==CreatureForm.Fox?.24f:.14f,accent,creatureGesture);
            if(form==CreatureForm.Bear){creatureHead.localPosition=creatureHeadRest=new Vector3(0,.49f,.28f);Flesh("Snow blanket",new Vector3(0,.64f,-.10f),new Vector3(.72f,.24f,.61f),ivory);}
            if(creatureStone)for(int i=0;i<3;i++)Flesh("Moss bed",new Vector3((i-1)*.19f,belly+.26f,-.10f),new Vector3(.28f,.12f,.26f),accent);
        }
        void Amphibian(CreatureForm form)
        {
            bool newt=form==CreatureForm.Newt;
            Feet(newt?.32f:.22f,newt?.25f:.33f,.16f);
            Flesh("Round belly",new Vector3(0,newt?.48f:.32f,-.02f),new Vector3(newt?.48f:.95f,newt?.62f:.45f,newt?.80f:.70f),shell);
            Face(new Vector3(0,newt?.68f:.53f,.25f),new Vector3(newt?.36f:.72f,.27f,.36f),false,0);
            if(newt){Curl("Newt tail",new[]{new Vector3(0,.29f,-.34f),new Vector3(.21f,.27f,-.46f),new Vector3(.36f,.52f,-.27f)},.17f,accent,creatureGesture);
                for(int i=0;i<3;i++)Feather("Ember sail",new Vector3(0,.64f,-.11f-i*.11f),new Vector3(.22f,.37f,.17f),0,accent,creatureGesture);
            }else{
                Flesh("Wide mouth",new Vector3(0,-.065f,.19f),new Vector3(.52f,.065f,.05f),shadow,creatureHead);
                for(int side=-1;side<=1;side+=2){
                    Flesh("Raised toad eye",new Vector3(side*.25f,.18f,.08f),new Vector3(.22f,.24f,.25f),accent,creatureHead);
                    Flesh("Toad pupil",new Vector3(side*.25f,.18f,.21f),new Vector3(.10f,.095f,.023f),light,creatureHead);
                    Flesh("Folded haunch",new Vector3(side*.38f,.28f,-.10f),new Vector3(.35f,.44f,.50f),ivory);
                }
                for(int i=0;i<3;i++)Flesh("Back wart",new Vector3((i-1)*.20f,.54f,-.15f),new Vector3(.15f,.18f,.15f),ivory);
            }
        }
        void Ape(bool stone)
        {
            Feet(.45f,.22f,0,false);
            Flesh("Broad shoulders",new Vector3(0,.92f,0),new Vector3(.82f,.70f,.53f),shell);
            Flesh("Heavy belly",new Vector3(0,.61f,.04f),new Vector3(.63f,.63f,.50f),ivory);
            Face(new Vector3(0,1.28f,.12f),new Vector3(.44f,.44f,.39f));
            for(int side=-1;side<=1;side+=2){
                Limb("Long arm",new Vector3(side*.30f,1.05f,0),new Vector3(side*.43f,.41f,.16f),.27f,shell,creatureGesture);
                Flesh("Ground fist",new Vector3(side*.43f,.28f,.17f),new Vector3(.32f,.32f,.34f),accent,creatureGesture);
            }
            if(stone)Flesh("Held boulder",new Vector3(.39f,.54f,.24f),new Vector3(.39f,.37f,.33f),ivory,creatureGesture);
            else for(int i=-1;i<=1;i++)Horn("Ash crown",new Vector3(i*.16f,1.43f,-.08f),new Vector3(i*.2f,1,0),.33f,.20f,accent);
        }
        void Insect(CreatureForm form)
        {
            bool mantis=form==CreatureForm.Mantis,fly=form==CreatureForm.Dragonfly,scorpion=form==CreatureForm.Scorpion;
            float y=mantis?.84f:fly?.99f:.47f;
            Flesh("Chitin abdomen",new Vector3(0,y,-.15f),new Vector3(mantis?.31f:fly?.24f:.71f,mantis?.76f:fly?.74f:.61f,.64f),shell);
            for(int side=-1;side<=1;side+=2)for(int i=0;i<3;i++){
                var a=new Vector3(side*.22f,y-.1f,.15f-i*.18f);var b=new Vector3(side*.40f,y-.22f,.19f-i*.20f);var c=new Vector3(side*.45f,.10f,.25f-i*.24f);
                Limb("Insect knee",a,b,.09f,ivory);Limb("Insect foot",b,c,.07f,shell);
            }
            Face(new Vector3(0,mantis?1.33f:fly?1.48f:.61f,.24f),new Vector3(.31f,.28f,.29f),false,0);
            for(int side=-1;side<=1;side+=2){
                Horn("Small mandible",new Vector3(side*.095f,-.10f,.11f),new Vector3(-side*.4f,-.2f,1),.21f,.09f,ivory,creatureHead);
                Horn("Feelers",new Vector3(side*.10f,.13f,0),new Vector3(side*.4f,1,.2f),.31f,.065f,accent,creatureHead);
                if(mantis){Limb("Mantis forearm",new Vector3(side*.16f,1.05f,.16f),new Vector3(side*.35f,1.16f,.37f),.12f,accent,creatureGesture);Horn("Mantis hook",new Vector3(side*.35f,1.16f,.37f),new Vector3(-side*.4f,-1,0),.36f,.14f,ivory,creatureGesture);}
                else if(fly){for(int i=0;i<2;i++)Feather("Glass wing",new Vector3(side*.30f,1.10f-i*.26f,0),new Vector3(.27f,.85f,.10f),-side*(i==0?60:100),ivory,creatureGesture);}
                else if(form==CreatureForm.Stagbeetle||form==CreatureForm.Colossus)Horn("Great mandible",new Vector3(side*.17f,.65f,.21f),new Vector3(side*.18f,1,.22f),form==CreatureForm.Colossus?.89f:.55f,.18f,ivory,creatureGesture);
                else if(scorpion){Flesh("Scorpion claw",new Vector3(side*.36f,.48f,.32f),new Vector3(.29f,.26f,.34f),accent,creatureGesture);}
                else Flesh("Split wing case",new Vector3(side*.18f,.66f,-.14f),new Vector3(.35f,.25f,.60f),accent);
            }
            if(scorpion){Curl("Arched sting",new[]{new Vector3(0,.40f,-.39f),new Vector3(0,.89f,-.38f),new Vector3(0,1.25f,-.12f),new Vector3(0,1.11f,.15f)},.15f,shell,creatureGesture);Horn("Sting",new Vector3(0,1.11f,.15f),Vector3.down,.24f,.17f,light,creatureGesture);}
            if(form==CreatureForm.Bombardier)for(int i=0;i<5;i++)Flesh("Burr pod",new Vector3(Mathf.Sin(i*1.3f)*.25f,.84f+(i%2)*.11f,Mathf.Cos(i*1.3f)*.25f-.10f),new Vector3(.30f,.35f,.30f),ivory);
            if(form==CreatureForm.Colossus){Flesh("Brood mantle",new Vector3(0,.75f,-.13f),new Vector3(.85f,.57f,.70f),accent);Horn("Crown horn",new Vector3(0,.87f,.18f),new Vector3(0,1,.16f),.72f,.28f,ivory);}
        }
        void CreatureTower(Material trim,Material dark,int faction,bool iron)
        {
            creature=true;creatureFaction=faction;creatureSlot=iron?Subject.Design%7:Subject.Design%5;ivory=trim;shadow=dark;creatureStone=iron&&faction==4;
            var form=(iron?ironForms:winterForms)[faction][creatureSlot];CreatureIdentity=form.ToString();
            weapon=new GameObject(Role+" weapon").transform;weapon.SetParent(transform,false);
            creatureHead=new GameObject("Living head").transform;creatureHead.SetParent(weapon,false);
            creatureGesture=new GameObject("Living gesture").transform;creatureGesture.SetParent(weapon,false);
            switch(form){
                case CreatureForm.Owl:case CreatureForm.Crane:case CreatureForm.Jay:case CreatureForm.Heron:case CreatureForm.Roc:case CreatureForm.Phoenix:case CreatureForm.Hawk:case CreatureForm.Kingfisher:Bird(form);break;
                case CreatureForm.Bear:case CreatureForm.Deer:case CreatureForm.Ram:case CreatureForm.Fox:case CreatureForm.Hare:case CreatureForm.StoneRam:case CreatureForm.Ibex:case CreatureForm.StoneBear:case CreatureForm.Ox:Quadruped(form);break;
                case CreatureForm.Newt:case CreatureForm.Toad:Amphibian(form);break;
                case CreatureForm.Ape:case CreatureForm.StoneApe:Ape(creatureStone);break;
                case CreatureForm.Beetle:case CreatureForm.Stagbeetle:case CreatureForm.Bombardier:case CreatureForm.Mantis:case CreatureForm.Dragonfly:case CreatureForm.Scorpion:case CreatureForm.Colossus:Insect(form);break;
                default:MythicCreature(form);break;
            }
        }
        void AnimateCreature()
        {
            if(!creature)return;
            float time=game.World.Tick*FrostMaze.Simulation.World.FixedDelta,phase=time*1.9f+Subject.Id*.71f;
            float attack=Mathf.Clamp01(1-(game.World.Tick-shotTick)/8f);
            creatureHead.localPosition=creatureHeadRest+Vector3.up*(Mathf.Sin(phase)*.009f);
            creatureHead.localRotation=Quaternion.Euler(Mathf.Sin(phase)*1.6f-attack*6,Mathf.Sin(phase*.47f)*2,0);
            creatureGesture.localRotation=Quaternion.Euler(-attack*5,0,Mathf.Sin(phase*.7f)*1.4f);
        }
    }
}
