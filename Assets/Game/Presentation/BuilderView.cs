using System.Collections.Generic;
using UnityEngine;
using FrostMaze.Simulation;
namespace FrostMaze
{
    // Cosmetic bodies follow authoritative builder positions; never add physics or movement rules.
    public sealed partial class BuilderView : MonoBehaviour
    {
        Prototype game;
        int faction=-1,owner=-1;
        bool robot,positioned;
        long lastTick=-1;
        V2 previous;
        int observedTowerId;
        long gestureUntil=-1;
        V2 workTarget;
        public bool Constructing=>lastTick<gestureUntil;
        public bool Walking {get;private set;}
        Transform leftBoot,rightBoot,leftArm,rightArm,staff,ring;
        Renderer ownerBadge;
        Material[] palette;
        public int VisibleFaction=>faction;
        public string Identity {get;private set;}
        GameObject Part(string name,Mesh mesh,Vector3 position,Vector3 scale,Material material,Transform parent=null)
        {
            var part=new GameObject(name);part.transform.SetParent(parent==null?transform:parent,false);
            part.transform.localPosition=position;part.transform.localScale=scale;
            part.AddComponent<MeshFilter>().sharedMesh=mesh;part.AddComponent<MeshRenderer>().sharedMaterial=material;return part;
        }
        Transform Joint(string name,Vector3 position)
        {
            var joint=new GameObject(name).transform;joint.SetParent(transform,false);joint.localPosition=position;return joint;
        }
        public void Configure(Prototype prototype,int selectedFaction)
        {
            if(game==prototype&&faction==selectedFaction)return;
            game=prototype;faction=selectedFaction;robot=game.World.Config.Theme=="iron";palette=game.TowerPalette(faction);
            foreach(Transform child in transform){child.gameObject.SetActive(false);Destroy(child.gameObject);}
            leftBoot=rightBoot=leftArm=rightArm=staff=null;lastTick=-1;positioned=false;owner=-1;
            Identity=(robot?"Forge artisan ":"Warden ")+game.World.Config.Factions[faction].Name;
            ring=Part("Ownership ring",game.Models.OwnerRing,new Vector3(0,-1.06f,0),Vector3.one,game.OwnerMaterial(0)).transform;
            ownerBadge=ring.GetComponent<Renderer>();ownerBadge.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
            if(robot)BuildArtisan();else BuildWarden();
            for(int side=-1;side<=1;side+=2) {
                var seal=Part("Hearthwright seal",game.Models.BeveledBox,new Vector3(side*.052f,-.07f,.325f),new Vector3(.035f,.16f,.025f),palette[3]);
                seal.transform.localRotation=Quaternion.Euler(0,0,side*27);
            }
            if(robot)CompactArtisan();
            string key="Builder/"+game.World.Config.Theme+"/"+faction;
            Batch(transform,key+"/body");
            foreach(var joint in new[]{leftArm,rightArm,leftBoot,rightBoot,staff})if(joint!=null)Batch(joint,key+"/"+joint.name);
        }
        void BuildWarden()
        {
            var m=game.Models;
            Part("Faction mantle",m.Robe,new Vector3(0,-.25f,-.035f),new Vector3(.69f,.94f,.65f),palette[1]);
            Part("Mantle hem",m.Robe,new Vector3(0,-.62f,-.035f),new Vector3(.69f,.12f,.65f),palette[3]);
            Part("Chest armor",m.Armor,new Vector3(0,-.04f,.14f),new Vector3(.44f,.43f,.25f),palette[0]);
            Part("Belt",m.Column,new Vector3(0,-.31f,0),new Vector3(.50f,.037f,.49f),palette[4]);
            Part("Belt clasp",m.BeveledBox,new Vector3(0,-.30f,.245f),new Vector3(.11f,.105f,.05f),palette[3]);
            Part("Hood",m.Shell,new Vector3(0,.31f,0),new Vector3(.45f,.47f,.44f),palette[4]);
            Part("Hood brow",m.BeveledBox,new Vector3(0,.35f,.18f),new Vector3(.33f,.09f,.14f),palette[0]);
            Part("Face shadow",m.BeveledBox,new Vector3(0,.24f,.195f),new Vector3(.26f,.20f,.06f),palette[4]);
            for(int side=-1;side<=1;side+=2) {
                Part("Warden eye",m.Crystal,new Vector3(side*.062f,.275f,.232f),new Vector3(.048f,.043f,.028f),palette[2]);
                Part("Shoulder armor",m.Armor,new Vector3(side*.30f,.025f,0),new Vector3(.27f,.23f,.34f),palette[0]);
                var arm=Joint(side<0?"Left arm":"Right arm",new Vector3(side*.31f,-.04f,0));
                Part("Sleeve",m.Robe,new Vector3(0,-.18f,0),new Vector3(.21f,.43f,.23f),palette[1],arm);
                Part("Glove",m.Armor,new Vector3(0,-.38f,.07f),new Vector3(.17f,.18f,.18f),palette[4],arm);
                var leg=Joint(side<0?"Left stride":"Right stride",new Vector3(side*.16f,-.63f,0));
                Part("Boot",m.BeveledBox,new Vector3(0,-.28f,.06f),new Vector3(.22f,.28f,.34f),palette[4],leg);
                Part("Boot cuff",m.Armor,new Vector3(0,-.12f,0),new Vector3(.22f,.19f,.22f),palette[0],leg);
                if(side<0){leftArm=arm;leftBoot=leg;}else{rightArm=arm;rightBoot=leg;}
            }
            Part("Field satchel",m.BeveledBox,new Vector3(-.30f,-.33f,-.16f),new Vector3(.23f,.31f,.26f),palette[4]);
            Part("Satchel clasp",m.BeveledBox,new Vector3(-.31f,-.27f,-.30f),new Vector3(.10f,.07f,.025f),palette[3]);
            staff=Joint("Staff grip",new Vector3(.42f,-.35f,.12f));
            Part("Staff",m.Column,new Vector3(0,.3f,0),new Vector3(.068f,.66f,.068f),palette[4],staff);
            Part("Staff collar",m.Column,new Vector3(0,.77f,0),new Vector3(.19f,.065f,.19f),palette[3],staff);
            Part("Faction staff crystal",faction==1?m.Shell:m.Crystal,new Vector3(0,.98f,0),new Vector3(.25f,.4f,.25f),palette[1],staff);
            Part("Staff heart",m.Crystal,new Vector3(0,1.0f,.09f),new Vector3(.10f,.24f,.07f),palette[2],staff);
            if(faction==0) {
                for(int side=-1;side<=1;side+=2)Part("Frost collar",m.Crystal,new Vector3(side*.20f,.16f,-.05f),new Vector3(.16f,.34f,.22f),palette[3]);
            } else if(faction==1) {
                for(int side=-1;side<=1;side+=2){var antler=Part("Root antler",m.Column,new Vector3(side*.19f,.54f,-.02f),new Vector3(.065f,.20f,.065f),palette[3]);antler.transform.localRotation=Quaternion.Euler(0,0,-side*24);Part("Antler branch",m.Crystal,new Vector3(side*.27f,.62f,-.02f),new Vector3(.13f,.22f,.08f),palette[1]);}
            } else if(faction==2) {
                Part("Forge mask",m.Armor,new Vector3(0,.22f,.22f),new Vector3(.25f,.24f,.1f),palette[3]);
                for(int i=-1;i<=1;i++)Part("Mask vent",m.BeveledBox,new Vector3(i*.065f,.21f,.278f),new Vector3(.026f,.12f,.024f),palette[2]);
                Part("Ember pack",m.Column,new Vector3(0,-.10f,-.31f),new Vector3(.32f,.28f,.24f),palette[0]);
            } else {
                for(int side=-1;side<=1;side+=2)Part("Conductor fork",m.Crystal,new Vector3(side*.12f,.96f,0),new Vector3(.1f,.42f,.1f),palette[3],staff);
                Part("Storm crest",m.Crystal,new Vector3(0,.62f,-.045f),new Vector3(.12f,.36f,.25f),palette[1]);
            }
        }
        void BuildArtisan()
        {
            if(faction==0){BuildPulseMechanic();return;}
            if(faction==1){BuildBlastSmith();return;}
            if(faction==3){BuildHorizonSurveyor();return;}
            var m=game.Models;
            Part("Chassis",m.Armor,new Vector3(0,-.20f,0),new Vector3(.66f,.62f,.60f),palette[0]);
            Part("Faction canopy",m.Shell,new Vector3(0,.27f,0),new Vector3(.51f,.44f,.48f),palette[1]);
            Part("Visor recess",m.BeveledBox,new Vector3(0,.27f,.205f),new Vector3(.37f,.17f,.075f),palette[4]);
            Part("Artisan visor",m.BeveledBox,new Vector3(0,.28f,.25f),new Vector3(.29f,.065f,.03f),palette[2]);
            Part("Chest insignia",m.Crystal,new Vector3(0,-.05f,.29f),new Vector3(.16f,.21f,.065f),palette[2]);
            Part("Tool backpack",m.BeveledBox,new Vector3(0,-.04f,-.34f),new Vector3(.48f,.53f,.26f),palette[4]);
            for(int i=-1;i<=1;i++)Part("Backpack vent",m.BeveledBox,new Vector3(i*.1f,-.07f,-.48f),new Vector3(.035f,.25f,.025f),palette[3]);
            for(int side=-1;side<=1;side+=2) {
                Part("Faction stabilizer",m.Wing(side),new Vector3(side*.21f,-.39f,-.02f),new Vector3(.43f,.9f,.63f),palette[1]);
                Part("Hover engine",m.Column,new Vector3(side*.29f,-.53f,0),new Vector3(.25f,.17f,.25f),palette[4]);
                Part("Engine glow",m.Crystal,new Vector3(side*.29f,-.77f,0),new Vector3(.15f,.27f,.15f),palette[2]);
                var arm=Joint(side<0?"Left arm":"Right arm",new Vector3(side*.38f,.02f,0));
                Part("Arm joint",m.Shell,Vector3.zero,Vector3.one*.21f,palette[3],arm);
                Part("Tool arm",m.BeveledBox,new Vector3(0,-.24f,.05f),new Vector3(.17f,.38f,.21f),palette[0],arm);
                Part("Tool cuff",m.Column,new Vector3(0,-.43f,.05f),new Vector3(.25f,.065f,.25f),palette[1],arm);
                for(int finger=-1;finger<=1;finger+=2)Part("Gripper",m.BeveledBox,new Vector3(finger*.075f,-.54f,.10f),new Vector3(.047f,.16f,.12f),palette[3],arm);
                if(side<0)leftArm=arm;else rightArm=arm;
            }
            // Each faction has a different readable tool/crest silhouette, not only a recolor.
            if(faction==2)for(int side=-1;side<=1;side+=2){
                var crown=Part("Ivory signal petal",m.Crystal,new Vector3(side*.30f,.49f,-.07f),new Vector3(.18f,.7f,.22f),palette[3]);crown.transform.localRotation=Quaternion.Euler(0,0,-side*22);
            }else if(faction==4){
                var halo=Part("Gravity artisan halo",m.Halo,new Vector3(0,.40f,-.20f),Vector3.one*.84f,palette[3]);halo.transform.localRotation=Quaternion.Euler(70,0,15);
            }else if(faction==5){
                Part("Salvage crate",m.BeveledBox,new Vector3(0,.08f,-.39f),new Vector3(.65f,.72f,.24f),palette[0]);
                for(int side=-1;side<=1;side+=2){var tool=Part("Carried repair wrench",m.BeveledBox,new Vector3(side*.20f,.53f,-.31f),new Vector3(.12f,.54f,.12f),palette[3]);tool.transform.localRotation=Quaternion.Euler(0,0,side*15);}
            }else if(faction==6)for(int side=-1;side<=1;side+=2){
                var crest=Part("Drake horn",m.Crystal,new Vector3(side*.21f,.58f,-.04f),new Vector3(.16f,.52f,.22f),palette[3]);crest.transform.localRotation=Quaternion.Euler(-20,0,-side*23);
            }else{
                Part("Tide pearl",m.Shell,new Vector3(0,.56f,-.16f),Vector3.one*.34f,palette[3]);
                var shell=Part("Pearlkeeper crest",m.Bell,new Vector3(0,.25f,-.19f),new Vector3(.77f,.28f,.75f),palette[3]);shell.transform.localRotation=Quaternion.Euler(65,0,0);
            }
            Part("Faction tool pack",faction==7?m.Column:m.BeveledBox,new Vector3(faction%2==0?-.25f:.25f,-.09f,-.43f),new Vector3(.21f,.40f,.19f),palette[1]);
        }
        void Batch(Transform group,string key)
        {
            var materials=new List<Material>();var parts=new List<List<CombineInstance>>();
            foreach(Transform part in group) {
                if(!part.gameObject.activeSelf||part==ring)continue;
                var filter=part.GetComponent<MeshFilter>();var renderer=part.GetComponent<MeshRenderer>();if(filter==null||renderer==null||!renderer.enabled)continue;
                int index=materials.IndexOf(renderer.sharedMaterial);if(index<0){index=materials.Count;materials.Add(renderer.sharedMaterial);parts.Add(new List<CombineInstance>());}
                parts[index].Add(new CombineInstance{mesh=filter.sharedMesh,transform=Matrix4x4.TRS(part.localPosition,part.localRotation,part.localScale)});renderer.enabled=false;
            }
            for(int i=0;i<materials.Count;i++)Part("Combined builder geometry "+i,game.Models.Combine(key+"/"+i,parts[i]),Vector3.zero,Vector3.one,materials[i],group);
        }
        public void Sync(V2 position,long tick,int player)
        {
            bool reset=!positioned||owner!=player||tick<lastTick;
            if(owner!=player){owner=player;ownerBadge.sharedMaterial=game.OwnerMaterial(player);}
            var movement=reset?default:position-previous;
            float phase=tick*World.FixedDelta*9;
            transform.position=new Vector3(position.X,1.1f+(robot&&!GroundedArtisan?Mathf.Sin(phase*.45f)*.035f:0),position.Y);
            ring.localPosition=new Vector3(0,.04f-transform.position.y,0);
            if(reset||tick!=lastTick) {
                bool walking=!reset&&movement.Length>.001f&&movement.Length<3;
                Walking=walking;
                int newest=reset?0:observedTowerId;
                foreach(var tower in game.World.Grid.Towers) {
                    newest=Mathf.Max(newest,tower.Id);
                    if(!reset&&tower.Id>observedTowerId&&game.World.TowerOwner(tower.Id)==player) {
                        gestureUntil=tick+14;workTarget=tower.Center;
                    }
                }
                observedTowerId=newest;
                if(reset){transform.rotation=Quaternion.identity;gestureUntil=-1;}
                else {
                    var facing=tick<gestureUntil?workTarget-position:movement;
                    if(facing.Length>.001f&&(walking||tick<gestureUntil)) {
                        float elapsed=Mathf.Clamp(tick-lastTick,1,6)*World.FixedDelta;
                        transform.rotation=Quaternion.RotateTowards(transform.rotation,Quaternion.LookRotation(new Vector3(facing.X,0,facing.Y)),480*elapsed);
                    }
                }
                float step=walking?Mathf.Sin(phase):0;
                if(leftBoot!=null){leftBoot.localRotation=Quaternion.Euler(step*23,0,0);rightBoot.localRotation=Quaternion.Euler(-step*23,0,0);}
                leftArm.localRotation=Quaternion.Euler((robot?8:0)-step*12,0,robot?-8:0);
                rightArm.localRotation=Quaternion.Euler((robot?8:0)+step*8,0,robot?8:0);
                float work=tick<gestureUntil?Mathf.Sin(Mathf.Clamp01((gestureUntil-tick)/14f)*Mathf.PI):0;
                rightArm.localRotation*=Quaternion.Euler(-work*68,0,-work*12);
                leftArm.localRotation*=Quaternion.Euler(-work*32,0,work*9);
                if(staff!=null)staff.localRotation=Quaternion.Euler(step*4-work*48,0,0);
                lastTick=tick;
            }
            // Retain the authoritative position used to determine the next actual movement step.
            previous=position;positioned=true;
        }
    }
}
