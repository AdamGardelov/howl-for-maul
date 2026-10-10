using UnityEngine;
namespace FrostMaze
{
    public sealed partial class TowerView
    {
        Transform livingCrown,livingArm;
        int livingKind;
        Vector3 livingCrownRest;
        GameObject TreePart(string name,Vector3 p,Vector3 size,Material material,Transform parent=null)
        {
            var part=Part(name,PrimitiveType.Sphere,p,size,material,parent??weapon);
            if(name=="Sapling trunk"||name=="Old oak trunk"||name=="Briar heartwood"||name=="Sky pine trunk"||name=="Willow heartwood")part.GetComponent<MeshFilter>().sharedMesh=game.Models.Heartwood;
            return part;
        }
        void Bough(string name,Vector3[] points,float width,Material material,Transform parent=null)
        {
            var piece=TreePart(name,Vector3.zero,Vector3.one,material,parent);
            piece.GetComponent<MeshFilter>().sharedMesh=game.Models.CurvedPipe("Rootbound/"+livingKind+"/"+name+"/"+points[0],points,width);
        }
        void Leaf(string name,Vector3 p,Vector3 size,Vector3 angles,Material material,Transform parent)
        {
            var leaf=TreePart(name,p,size,material,parent);leaf.GetComponent<MeshFilter>().sharedMesh=game.Models.Leaf;
            leaf.transform.localRotation=Quaternion.Euler(angles);
        }
        void Thorn(Vector3 root,Vector3 direction,float length,float width,Transform parent)
        {
            var thorn=TreePart("Woody thorn",root,new Vector3(width,length,width),shell,parent);
            thorn.GetComponent<MeshFilter>().sharedMesh=game.Models.Thorn;
            thorn.transform.localRotation=Quaternion.FromToRotation(Vector3.up,direction.normalized);
        }
        void Foliage(Vector3 p,float size,int seed,Material pale)
        {
            // Branches meet their moving foliage; every canopy stays visibly rooted in its trunk.
            Bough("Crown fork "+seed,new[]{new Vector3(0,-.20f,0),new Vector3(p.x*.6f,p.y*.6f,p.z),p},.075f,shell,livingCrown);
            // Open, overlapping leaf fans keep branches visible through a leafy crown.
            TreePart("Mossy leaf heart",p,new Vector3(size*.87f,size*.59f,size*.82f),accent,livingCrown);
            for(int i=0;i<14;i++){
                float angle=(i*137+seed*37)*Mathf.Deg2Rad;float ring=i<4?.16f:.32f;
                var offset=new Vector3(Mathf.Sin(angle)*ring,.12f+(i%3)*.065f,Mathf.Cos(angle)*ring)*size;
                Leaf("Crown leaf",p+offset,new Vector3(size*.72f,size*.86f,size*.70f),new Vector3(48+(i%3)*19,i*137+seed*37,(i%2==0?1:-1)*25),i%3==0?pale:accent,livingCrown);
            }
        }
        void TreeFace(float height,float width,float depth,Material dark,Material pale)
        {
            TreePart("Gnarled face",new Vector3(0,height,depth),new Vector3(width,.34f,.21f),shell);
            for(int side=-1;side<=1;side+=2){
                TreePart("Deep eye hollow",new Vector3(side*width*.25f,height+.055f,depth+.09f),new Vector3(.115f,.083f,.045f),dark);
                TreePart("Amber sap eye",new Vector3(side*width*.25f,height+.053f,depth+.113f),new Vector3(.05f,.042f,.022f),light);
                var brow=TreePart("Heavy bark brow",new Vector3(side*width*.25f,height+.116f,depth+.08f),new Vector3(.15f,.063f,.07f),shell);
                brow.transform.localRotation=Quaternion.Euler(0,0,-side*12);
            }
            TreePart("Bark nose",new Vector3(0,height-.015f,depth+.11f),new Vector3(.09f,.15f,.095f),shell);
            TreePart("Crooked mouth",new Vector3(0,height-.115f,depth+.11f),new Vector3(width*.48f,.057f,.036f),dark);
        }
        void RootboundTower(Material pale,Material dark,int slot)
        {
            livingKind=slot;
            weapon=new GameObject(Role+" weapon").transform;weapon.SetParent(transform,false);
            livingCrown=new GameObject("Living crown").transform;livingCrown.SetParent(weapon,false);
            livingCrown.localPosition=livingCrownRest=new Vector3(0,1.15f,0);
            livingArm=new GameObject("Throwing bough").transform;livingArm.SetParent(weapon,false);livingArm.localPosition=new Vector3(.23f,.73f,0);
            for(int i=0;i<5;i++){
                float a=(i*72+18)*Mathf.Deg2Rad;var dir=new Vector3(Mathf.Sin(a),0,Mathf.Cos(a));
                Bough("Planted root",new[]{dir*.10f+Vector3.up*.38f,dir*.26f+Vector3.up*.17f,dir*.44f+Vector3.up*.055f},.13f,shell,transform);
            }
            if(slot==0){ // Young hazel: forked trunk, broad ears of leaves, a loaded seed hand.
                TreePart("Sapling trunk",new Vector3(0,.70f,0),new Vector3(.39f,.94f,.34f),shell);
                Bough("Sapling fork",new[]{new Vector3(-.09f,.68f,0),new Vector3(-.28f,1.06f,0),new Vector3(-.23f,1.34f,0)},.13f,shell);
                TreeFace(.91f,.33f,.14f,dark,pale);
                Foliage(new Vector3(-.24f,.19f,0),.54f,0,pale);Foliage(new Vector3(.17f,.36f,0),.60f,1,pale);
                TreePart("Seed fist",new Vector3(.035f,.14f,.13f),new Vector3(.22f,.23f,.25f),shell,livingArm);
                TreePart("Loaded acorn",new Vector3(.035f,.30f,.17f),new Vector3(.14f,.19f,.14f),pale,livingArm);
            }else if(slot==1){ // A sleepy old oak, broad enough to shelter the road; no attack.
                TreePart("Old oak trunk",new Vector3(0,.51f,0),new Vector3(.93f,.94f,.72f),shell);
                TreeFace(.67f,.46f,.23f,dark,pale);
                Foliage(new Vector3(-.28f,-.12f,0),.73f,2,pale);Foliage(new Vector3(.28f,-.08f,-.04f),.74f,3,pale);Foliage(new Vector3(0,.13f,0),.71f,4,pale);
                for(int side=-1;side<=1;side+=2)Bough("Folded oak arm",new[]{new Vector3(side*.25f,.69f,0),new Vector3(side*.37f,.47f,.13f),new Vector3(side*.16f,.40f,.27f)},.17f,shell);
                TreePart("Acorn nook",new Vector3(-.19f,.29f,.26f),new Vector3(.12f,.15f,.075f),pale);
            }else if(slot==2){ // An exposed hawthorn crown: hooked wood, sparse leaves and thorn volleys.
                TreePart("Briar heartwood",new Vector3(0,.75f,0),new Vector3(.57f,1.22f,.47f),shell);
                TreeFace(1.07f,.42f,.20f,dark,pale);
                for(int side=-1;side<=1;side+=2){
                    Bough("Briar crown",new[]{new Vector3(side*.10f,-.27f,-.07f),new Vector3(side*.29f,.05f,-.10f),new Vector3(side*.21f,.40f,-.05f),new Vector3(side*.28f,.57f,0)},.12f,shell,livingCrown);
                    Thorn(new Vector3(side*.28f,.56f,0),new Vector3(side*.25f,1,.1f),.29f,.15f,livingCrown);
                    for(int i=0;i<3;i++){
                        var root=new Vector3(side*(i==1?.27f:.24f),.04f+i*.16f,-.07f);
                        Thorn(root,new Vector3(side*.8f,.5f,.15f),.23f-i*.02f,.16f,livingCrown);
                        Leaf("Briar leaf",root+new Vector3(-side*.06f,.03f,-.02f),new Vector3(.21f,.27f,.21f),new Vector3(35,side*35,side*48),i==1?pale:accent,livingCrown);
                    }
                    Thorn(new Vector3(side*.22f,.77f,-.015f),new Vector3(side,.5f,0),.23f,.18f,weapon);
                }
                Bough("Twisting briar vine",new[]{new Vector3(-.19f,.32f,.10f),new Vector3(.12f,.48f,.22f),new Vector3(.24f,.71f,.02f),new Vector3(.13f,.94f,-.16f)},.07f,dark);
                Bough("Heavy throwing forearm",new[]{Vector3.zero,new Vector3(.10f,.08f,.08f),new Vector3(.02f,.33f,.10f)},.18f,shell,livingArm);
                TreePart("Thorn grasp",new Vector3(.02f,.35f,.10f),new Vector3(.20f,.16f,.18f),shell,livingArm);
                for(int i=-1;i<=1;i++)Thorn(new Vector3(.02f+i*.055f,.39f,.10f),new Vector3(i*.30f,1,.1f),i==0?.36f:.27f,.14f,livingArm);
            }else if(slot==3){ // A narrow, upright pine spirit with raised branch fingers.
                TreePart("Sky pine trunk",new Vector3(0,.85f,0),new Vector3(.33f,1.53f,.30f),shell);
                TreeFace(1.12f,.32f,.12f,dark,pale);
                var needles=TreePart("Pine needle crown",new Vector3(0,.24f,-.10f),new Vector3(.70f,.99f,.55f),accent,livingCrown);
                needles.GetComponent<MeshFilter>().sharedMesh=game.Models.PineCrown;
                for(int tier=0;tier<3;tier++)for(int side=-1;side<=1;side+=2){
                    float width=.33f-tier*.085f;float y=.02f+tier*.20f;
                    Bough("Pine bough "+tier,new[]{new Vector3(0,y,-.10f),new Vector3(side*width,y-.11f,-.08f)},.075f,shell,livingCrown);
                    Leaf("Needled pine fan",new Vector3(side*width*.6f,y,-.08f),new Vector3(.37f-tier*.055f,.44f,.28f),new Vector3(16,side*20,side*58),accent,livingCrown);
                }
                Foliage(new Vector3(0,.48f,-.04f),.36f,7,pale);
                Bough("Raised seed arm",new[]{Vector3.zero,new Vector3(.06f,.36f,0),new Vector3(-.03f,.64f,.08f)},.085f,shell,livingArm);
                TreePart("Wingseed cone",new Vector3(-.03f,.68f,.08f),new Vector3(.11f,.24f,.11f),pale,livingArm);
            }else{ // An ancient willow with long leaf curtains and a root beard.
                TreePart("Willow heartwood",new Vector3(0,.76f,0),new Vector3(.49f,1.26f,.43f),shell);
                TreeFace(1.03f,.40f,.19f,dark,pale);
                for(int side=-1;side<=1;side+=2)Bough("Willow shoulder",new[]{new Vector3(side*.15f,.91f,0),new Vector3(side*.37f,1.17f,0),new Vector3(side*.29f,1.45f,-.04f)},.105f,shell);
                Foliage(new Vector3(0,.43f,-.08f),.84f,8,pale);
                for(int i=0;i<7;i++){
                    float a=i*Mathf.PI*2/7;var dir=new Vector3(Mathf.Sin(a),0,Mathf.Cos(a));
                    // Keep the face open while the willow curtains fall down its back and sides.
                    if(dir.z>.55f)continue;
                    Bough("Hanging willow strand",new[]{dir*.14f+Vector3.up*.41f,dir*.38f+Vector3.up*.25f,dir*.40f-Vector3.up*.30f},.035f,shell,livingCrown);
                    for(int j=0;j<3;j++)Leaf("Pendant willow leaf",dir*(.33f+j*.026f)+Vector3.up*(.22f-j*.23f),new Vector3(.22f,.64f,.18f),new Vector3(0,i*360f/7,12),j%2==0?accent:pale,livingCrown);
                }
                for(int i=-1;i<=1;i++)Bough("Root beard",new[]{new Vector3(i*.08f,.85f,.24f),new Vector3(i*.12f,.62f,.26f),new Vector3(i*.09f,.40f,.20f)},.05f,dark);
                Bough("Binding branch",new[]{Vector3.zero,new Vector3(.10f,.18f,.08f),new Vector3(.02f,.40f,.16f)},.10f,shell,livingArm);
                Leaf("Hand sprout",new Vector3(.02f,.41f,.16f),new Vector3(.22f,.28f,.20f),new Vector3(20,0,25),accent,livingArm);
            }
        }
        void AnimateRootbound()
        {
            if(livingCrown==null)return;
            float time=game.World.Tick*FrostMaze.Simulation.World.FixedDelta,phase=time*1.7f+Subject.Id*.83f;
            float throwPose=Subject.Spec.Damage>0?Mathf.Clamp01(1-(game.World.Tick-shotTick)/8f):0;
            livingCrown.localPosition=livingCrownRest+Vector3.up*(Mathf.Sin(phase)*.012f);
            livingCrown.localRotation=Quaternion.Euler(Mathf.Sin(phase*.71f)*1.2f,0,Mathf.Sin(phase)*1.4f);
            livingArm.localRotation=Quaternion.Euler(-throwPose*32+Mathf.Sin(phase+.6f)*2,0,0);
        }
    }
}
