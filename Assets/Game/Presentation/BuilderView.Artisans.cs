using UnityEngine;
namespace FrostMaze
{
    public sealed partial class BuilderView
    {
        bool GroundedArtisan=>robot&&(faction==0||faction==1||faction==3);

        void CompactArtisan()
        {
            // Shrink around the ground, not the floating root. The owner ring remains full size.
            // Include the authored tools when choosing the envelope before sharing body meshes.
            float radius=0,height=0;
            foreach(var filter in GetComponentsInChildren<MeshFilter>()){
                if(!filter.gameObject.activeInHierarchy||filter.transform==ring)continue;
                var matrix=transform.worldToLocalMatrix*filter.transform.localToWorldMatrix;
                foreach(var v in filter.sharedMesh.vertices){var p=matrix.MultiplyPoint3x4(v);
                    radius=Mathf.Max(radius,new Vector2(p.x,p.z).magnitude);height=Mathf.Max(height,p.y+1.06f);
                }
            }
            float scale=Mathf.Min(.80f,.47f/Mathf.Max(radius,.01f),1.52f/Mathf.Max(height,.01f));
            foreach(Transform child in transform){
                if(!child.gameObject.activeSelf||child==ring)continue;
                var p=child.localPosition;child.localPosition=new Vector3(p.x*scale,(p.y+1.06f)*scale-1.06f,p.z*scale);child.localScale*=scale;
            }
        }
        void ArtisanLegs(float spacing,float width,bool boots)
        {
            var m=game.Models;
            for(int side=-1;side<=1;side+=2){
                var leg=Joint(side<0?"Left stride":"Right stride",new Vector3(side*spacing,-.52f,0));
                Part("Articulated shin",m.Column,new Vector3(0,-.20f,0),new Vector3(width*.68f,.19f,width*.68f),palette[4],leg);
                Part("Knee guard",m.Armor,new Vector3(0,-.08f,.06f),new Vector3(width,.17f,.19f),palette[0],leg);
                Part("Grounded boot",m.BeveledBox,new Vector3(0,-.43f,.075f),new Vector3(width,.22f,boots?.36f:.28f),palette[4],leg);
                Part("Toe cap",m.Armor,new Vector3(0,-.425f,.18f),new Vector3(width,.15f,.15f),palette[3],leg);
                if(side<0)leftBoot=leg;else rightBoot=leg;
            }
        }
        void ArtisanArms(float spread,float shoulder,bool cloth)
        {
            var m=game.Models;
            for(int side=-1;side<=1;side+=2){
                var arm=Joint(side<0?"Left arm":"Right arm",new Vector3(side*spread,shoulder,0));
                Part("Working shoulder",cloth?m.Robe:m.Armor,Vector3.zero,new Vector3(.22f,.21f,.27f),cloth?palette[1]:palette[0],arm);
                Part("Artisan sleeve",cloth?m.Robe:m.Column,new Vector3(0,-.19f,.025f),new Vector3(.16f,cloth?.36f:.16f,.18f),cloth?palette[1]:palette[4],arm);
                Part("Working glove",m.Armor,new Vector3(0,-.37f,.06f),new Vector3(.18f,.18f,.19f),palette[3],arm);
                if(side<0)leftArm=arm;else rightArm=arm;
            }
        }
        void BuildPulseMechanic()
        {
            var m=game.Models;
            Part("Chassis",m.Armor,new Vector3(0,-.20f,0),new Vector3(.42f,.61f,.36f),palette[0]);
            Part("Faction tabard",m.Robe,new Vector3(0,-.36f,.14f),new Vector3(.38f,.49f,.20f),palette[1]);
            Part("Mechanic breastplate",m.Armor,new Vector3(0,-.07f,.22f),new Vector3(.32f,.30f,.23f),palette[3]);
            Part("Belt buckle",m.BeveledBox,new Vector3(0,-.39f,.235f),new Vector3(.13f,.09f,.04f),palette[4]);
            ArtisanLegs(.135f,.19f,false);ArtisanArms(.26f,-.04f,false);
            Part("Enamel helmet",m.Armor,new Vector3(0,.25f,0),new Vector3(.40f,.34f,.36f),palette[0]);
            Part("Helmet visor",m.BeveledBox,new Vector3(0,.27f,.18f),new Vector3(.31f,.105f,.04f),palette[4]);
            Part("Signal eye",m.BeveledBox,new Vector3(0,.28f,.205f),new Vector3(.21f,.038f,.02f),palette[2]);
            Part("Helmet brow",m.BeveledBox,new Vector3(0,.36f,.135f),new Vector3(.39f,.065f,.15f),palette[3]);
            Part("Signal aerial",m.Column,new Vector3(-.16f,.46f,-.09f),new Vector3(.038f,.16f,.038f),palette[3]);
            Part("Signal cap",m.Shell,new Vector3(-.16f,.62f,-.09f),Vector3.one*.075f,palette[2]);
            Part("Tool roll",m.Column,new Vector3(0,-.20f,-.235f),new Vector3(.30f,.25f,.17f),palette[4]);
            Part("Precision driver",m.Column,new Vector3(0,-.41f,.23f),new Vector3(.105f,.24f,.105f),palette[3],rightArm).transform.localRotation=Quaternion.Euler(70,0,0);
            Part("Driver tip",m.Crystal,new Vector3(0,-.32f,.46f),new Vector3(.05f,.17f,.05f),palette[2],rightArm).transform.localRotation=Quaternion.Euler(70,0,0);
            Part("Folded plans",m.BeveledBox,new Vector3(0,-.34f,.12f),new Vector3(.25f,.07f,.25f),palette[1],leftArm);
        }
        void BuildBlastSmith()
        {
            var m=game.Models;
            Part("Chassis",m.Shell,new Vector3(0,-.25f,0),new Vector3(.60f,.55f,.44f),palette[0]);
            Part("Smith apron",m.Robe,new Vector3(0,-.34f,.21f),new Vector3(.48f,.61f,.20f),palette[4]);
            Part("Faction apron band",m.BeveledBox,new Vector3(0,-.12f,.27f),new Vector3(.37f,.11f,.055f),palette[1]);
            ArtisanLegs(.18f,.25f,true);ArtisanArms(.33f,-.11f,false);
            Part("Copper helmet",m.Shell,new Vector3(0,.20f,.01f),new Vector3(.51f,.39f,.43f),palette[3]);
            Part("Smith face shield",m.Armor,new Vector3(0,.16f,.195f),new Vector3(.37f,.31f,.09f),palette[0]);
            for(int side=-1;side<=1;side+=2){
                Part("Dark goggle",m.Shell,new Vector3(side*.105f,.215f,.25f),new Vector3(.15f,.12f,.05f),palette[4]);
                Part("Goggle glint",m.Shell,new Vector3(side*.105f,.22f,.278f),new Vector3(.05f,.045f,.018f),palette[2]);
                Part("Pressure cylinder",m.Armor,new Vector3(side*.14f,-.10f,-.29f),new Vector3(.22f,.57f,.25f),palette[3]);
                Part("Pressure valve",m.Halo,new Vector3(side*.14f,.20f,-.29f),Vector3.one*.17f,palette[1]);
            }
            Part("Hammer haft",m.Column,new Vector3(0,-.40f,.12f),new Vector3(.065f,.26f,.065f),palette[4],rightArm);
            Part("Smith hammer",m.BeveledBox,new Vector3(0,-.16f,.12f),new Vector3(.36f,.18f,.22f),palette[3],rightArm);
            for(int side=-1;side<=1;side+=2)Part("Hammer face",m.BeveledBox,new Vector3(side*.18f,-.16f,.12f),new Vector3(.04f,.20f,.23f),palette[0],rightArm);
            Part("Workpiece tongs",m.BeveledBox,new Vector3(0,-.43f,.19f),new Vector3(.07f,.08f,.31f),palette[4],leftArm);
            Part("Copper rivet",m.Shell,new Vector3(0,-.42f,.36f),Vector3.one*.115f,palette[1],leftArm);
        }
        void BuildHorizonSurveyor()
        {
            var m=game.Models;
            Part("Chassis",m.Robe,new Vector3(0,-.25f,0),new Vector3(.45f,.64f,.36f),palette[0]);
            Part("Faction survey cloak",m.Robe,new Vector3(0,-.26f,-.11f),new Vector3(.61f,.78f,.47f),palette[1]);
            Part("Surveyor vest",m.Armor,new Vector3(0,-.08f,.23f),new Vector3(.32f,.34f,.22f),palette[0]);
            ArtisanLegs(.14f,.19f,true);ArtisanArms(.29f,-.07f,true);
            Part("Survey hood",m.Shell,new Vector3(0,.27f,-.015f),new Vector3(.40f,.43f,.40f),palette[1]);
            Part("Shadowed face",m.Armor,new Vector3(0,.24f,.175f),new Vector3(.27f,.23f,.075f),palette[4]);
            Part("Sun brow",m.BeveledBox,new Vector3(0,.37f,.13f),new Vector3(.39f,.06f,.20f),palette[3]);
            Part("Survey monocle",m.Halo,new Vector3(.075f,.28f,.221f),Vector3.one*.115f,palette[3]).transform.localRotation=Quaternion.Euler(90,0,0);
            Part("Survey glass",m.Shell,new Vector3(.075f,.28f,.228f),new Vector3(.065f,.065f,.018f),palette[2]);
            for(int side=-1;side<=1;side+=2){
                var leg=Part("Packed survey tripod",m.Column,new Vector3(side*.12f,-.10f,-.30f),new Vector3(.055f,.47f,.055f),palette[3]);leg.transform.localRotation=Quaternion.Euler(0,0,side*7);
            }
            Part("Field satchel",m.BeveledBox,new Vector3(-.20f,-.42f,-.18f),new Vector3(.23f,.25f,.23f),palette[4]);
            Part("Hand scope",m.Column,new Vector3(0,-.35f,.22f),new Vector3(.12f,.22f,.12f),palette[3],rightArm).transform.localRotation=Quaternion.Euler(80,0,0);
            Part("Scope aperture",m.Shell,new Vector3(0,-.31f,.445f),new Vector3(.09f,.09f,.025f),palette[2],rightArm);
            Part("Survey tablet",m.BeveledBox,new Vector3(0,-.35f,.13f),new Vector3(.27f,.07f,.30f),palette[0],leftArm);
            Part("Tablet brass edge",m.BeveledBox,new Vector3(0,-.305f,.13f),new Vector3(.22f,.02f,.25f),palette[3],leftArm);
        }
    }
}
