using UnityEngine;
namespace FrostMaze
{
    public sealed partial class TowerView
    {
        void ShellCreature(CreatureForm form)
        {
            bool crab=form==CreatureForm.Crab||form==CreatureForm.Coalcrab,snail=form==CreatureForm.Snail;
            Feet(.32f,crab?.32f:.25f,.18f);
            Flesh("Living shell",new Vector3(0,.52f,-.06f),new Vector3(.86f,snail?1.00f:.58f,.75f),shell);
            if(snail){
                for(int i=0;i<3;i++)Loop("Spiral shell whorl",new Vector3(0,.66f+i*.055f,-.08f+i*.02f),.67f-i*.20f,Quaternion.Euler(90,0,0),ivory,weapon);
                Face(new Vector3(0,.35f,.36f),new Vector3(.34f,.22f,.26f),false,0);
                for(int side=-1;side<=1;side+=2){Limb("Eye stalk",new Vector3(side*.10f,.08f,.06f),new Vector3(side*.19f,.42f,.09f),.065f,accent,creatureHead);Flesh("Stalk pearl",new Vector3(side*.19f,.43f,.09f),Vector3.one*.13f,light,creatureHead);}
            }else{
                Face(new Vector3(0,.56f,.32f),new Vector3(crab?.45f:.33f,.20f,.29f),false,0);
                for(int side=-1;side<=1;side+=2){
                    if(crab){
                        Limb("Crab eye stalk",new Vector3(side*.13f,.56f,.29f),new Vector3(side*.20f,.89f,.28f),.08f,shell);
                        Flesh("Crab watchful eye",new Vector3(side*.20f,.90f,.29f),new Vector3(.14f,.16f,.13f),light);
                        Limb("Claw arm",new Vector3(side*.25f,.39f,.20f),new Vector3(side*.43f,.66f,.25f),.15f,ivory,creatureGesture);
                        Flesh("Great pincer",new Vector3(side*.43f,.72f,.25f),new Vector3(.29f,.39f,.32f),accent,creatureGesture);
                        Horn("Pincer thumb",new Vector3(side*.34f,.62f,.39f),new Vector3(0,1,.1f),.27f,.13f,ivory,creatureGesture);
                    }else for(int i=0;i<3;i++)Flesh("Shell plate",new Vector3(side*.20f,.75f,-.25f+i*.19f),new Vector3(.31f,.17f,.25f),ivory);
                }
                if(form==CreatureForm.Shellback)for(int i=-1;i<=1;i++)Horn("Old shell ridge",new Vector3(i*.20f,.79f,-.07f),new Vector3(i*.2f,1,0),.49f,.24f,accent);
                if(form==CreatureForm.Coalcrab)for(int i=-1;i<=1;i++)Flesh("Banked coal",new Vector3(i*.23f,.76f,-.08f),new Vector3(.26f,.27f,.35f),accent);
            }
        }
        void Spirit(CreatureForm form)
        {
            bool tall=form==CreatureForm.Walker||form==CreatureForm.Ancestor||form==CreatureForm.Seer;
            float y=tall?1.28f:form==CreatureForm.Wisp?.64f:1.03f;
            var robe=Flesh("Spirit mantle",new Vector3(0,tall?.70f:.49f,0),new Vector3(tall?.47f:.66f,tall?1.13f:.68f,.46f),shell);
            robe.GetComponent<MeshFilter>().sharedMesh=game.Models.Robe;
            Face(new Vector3(0,y,.12f),new Vector3(.38f,.43f,.21f),false,.045f);
            Flesh("Carved face mask",new Vector3(0,-.015f,.075f),new Vector3(.34f,.38f,.12f),ivory,creatureHead);
            for(int side=-1;side<=1;side+=2){
                Flesh("Mask slit",new Vector3(side*.08f,.04f,.144f),new Vector3(.095f,.037f,.025f),shadow,creatureHead);
                Flesh("Kindled mask eye",new Vector3(side*.08f,.04f,.160f),new Vector3(.038f,.027f,.01f),light,creatureHead);
                Feather("Trailing robe",new Vector3(side*.22f,.32f,-.10f),new Vector3(.22f,.53f,.17f),-side*23,accent,creatureGesture);
            }
            if(form==CreatureForm.Umbrella){
                Limb("Umbrella staff",new Vector3(.21f,.23f,0),new Vector3(.21f,1.54f,0),.06f,ivory);
                var canopy=Flesh("Rain canopy",new Vector3(.04f,1.43f,0),new Vector3(1.08f,.44f,.94f),accent);canopy.GetComponent<MeshFilter>().sharedMesh=game.Models.Bell;
                for(int i=0;i<6;i++){float a=i*Mathf.PI/3;Horn("Canopy tassel",new Vector3(Mathf.Sin(a)*.45f,1.27f,Mathf.Cos(a)*.4f),Vector3.down,.17f,.06f,ivory);}
            }else if(form==CreatureForm.BellSpirit){
                var bell=Flesh("Carried wardbell",new Vector3(.22f,.62f,.25f),new Vector3(.43f,.49f,.39f),ivory,creatureGesture);bell.GetComponent<MeshFilter>().sharedMesh=game.Models.Bell;
                Loop("Bell handle",new Vector3(.22f,.93f,.25f),.27f,Quaternion.Euler(90,0,0),accent,creatureGesture);
            }else if(form==CreatureForm.Ancestor){
                for(int side=-1;side<=1;side+=2){Horn("Ancestor antler",new Vector3(side*.14f,.19f,0),new Vector3(side*.22f,1,0),.57f,.14f,ivory,creatureHead);
                    Horn("Ancestor tine",new Vector3(side*.21f,.44f,0),new Vector3(side,.65f,0),.28f,.10f,accent,creatureHead);}
            }else if(form==CreatureForm.Walker||form==CreatureForm.Seer){
                Limb("Wayfarer staff",new Vector3(.33f,.10f,0),new Vector3(.33f,1.55f,0),.07f,ivory,creatureGesture);
                Flesh("Guiding lantern",new Vector3(.33f,1.48f,.02f),new Vector3(.28f,.38f,.25f),accent,creatureGesture);
                Flesh("Lantern light",new Vector3(.33f,1.49f,.15f),new Vector3(.11f,.20f,.02f),light,creatureGesture);
                if(form==CreatureForm.Seer){var hat=Flesh("Seer's pointed hood",new Vector3(0,.29f,-.06f),new Vector3(.60f,.64f,.48f),accent,creatureHead);hat.GetComponent<MeshFilter>().sharedMesh=game.Models.Crystal;}
            }else if(form==CreatureForm.Lantern){
                for(int side=-1;side<=1;side+=2){Curl("Lantern arch "+side,new[]{new Vector3(side*.36f,.25f,0),new Vector3(side*.45f,1.36f,-.05f),new Vector3(side*.16f,1.63f,0)},.10f,ivory);
                    Flesh("Floating lantern",new Vector3(side*.36f,.95f,.16f),new Vector3(.24f,.38f,.24f),accent,creatureGesture);}
                Loop("Elder moon",new Vector3(0,1.73f,0),.41f,Quaternion.Euler(90,0,0),ivory,weapon);
            }else if(form==CreatureForm.Wisp)Curl("Wisp curl",new[]{new Vector3(0,.11f,0),new Vector3(-.24f,.23f,-.10f),new Vector3(-.28f,.66f,-.06f)},.11f,accent,creatureGesture);
        }
        void Moth(bool moon)
        {
            Flesh("Moth body",new Vector3(0,.83f,0),new Vector3(.27f,.72f,.26f),shell);
            Face(new Vector3(0,1.25f,.06f),new Vector3(.28f,.28f,.26f),false,.04f);
            for(int side=-1;side<=1;side+=2){
                for(int i=0;i<2;i++){
                    Feather("Broad moth wing",new Vector3(side*(i==0?.29f:.23f),i==0?1.10f:.61f,0),new Vector3(moon?.76f:.58f,i==0?.98f:.63f,.19f),-side*(i==0?46:112),i==0?ivory:accent,creatureGesture);
                    Flesh("Wing eyespot",new Vector3(side*.35f,.98f,.09f),new Vector3(.15f,.20f,.025f),accent,creatureGesture);
                }
                Horn("Moth antenna",new Vector3(side*.075f,.10f,0),new Vector3(side*.45f,1,0),.28f,.045f,ivory,creatureHead);
            }
            if(moon)for(int side=-1;side<=1;side+=2)Feather("Luna tail",new Vector3(side*.27f,.28f,0),new Vector3(.15f,.60f,.12f),-side*12,ivory,creatureGesture);
        }
        void Dragon(CreatureForm form)
        {
            bool twin=form==CreatureForm.TwinDrake,winged=form==CreatureForm.Wyvern||form==CreatureForm.Wyrm;
            if(form==CreatureForm.CoilDrake){
                for(int i=0;i<3;i++)Loop("Coiled drake",new Vector3(0,.24f+i*.16f,0),.78f-i*.15f,Quaternion.Euler(i*7,0,0),shell,weapon);
                Face(new Vector3(.06f,.90f,.17f),new Vector3(.43f,.40f,.48f),false,.24f);
            }else{
                Feet(.36f,.24f,.18f);
                Flesh("Drake belly",new Vector3(0,form==CreatureForm.Spitter?.43f:.60f,-.08f),new Vector3(form==CreatureForm.Spitter?.83f:.58f,form==CreatureForm.Ramjaw?.92f:.63f,.62f),shell);
                Face(new Vector3(twin?-.17f:0,form==CreatureForm.Ramjaw?1.18f:.95f,.18f),new Vector3(.39f,.38f,.39f),false,form==CreatureForm.Spitter?.31f:.19f);
                if(twin){Flesh("Second drake neck",new Vector3(.24f,.94f,.03f),new Vector3(.23f,.70f,.26f),shell);Flesh("Second drake head",new Vector3(.22f,1.28f,.18f),new Vector3(.38f,.32f,.44f),ivory);Flesh("Second eye",new Vector3(.29f,1.31f,.36f),Vector3.one*.07f,light);}
                Curl("Drake tail",new[]{new Vector3(0,.29f,-.31f),new Vector3(.27f,.27f,-.42f),new Vector3(.39f,.52f,-.23f)},.17f,accent,creatureGesture);
            }
            for(int side=-1;side<=1;side+=2){
                Horn("Drake horn",new Vector3(side*.14f,.12f,-.07f),new Vector3(side*.20f,1,-.3f),form==CreatureForm.Wyrm?.52f:.27f,.13f,ivory,creatureHead);
                if(winged){
                    var wing=Flesh("Membrane wing",new Vector3(side*.16f,.85f,-.14f),new Vector3(.58f,1,.62f),accent,creatureGesture);wing.GetComponent<MeshFilter>().sharedMesh=game.Models.Wing(side);wing.transform.localRotation=Quaternion.Euler(-30,0,side*(form==CreatureForm.Wyrm?65:42));
                    Horn("Wing finger",new Vector3(side*.18f,.82f,-.17f),new Vector3(side*.6f,1,-.15f),.70f,.08f,ivory,creatureGesture);
                }
            }
            if(form==CreatureForm.Ramjaw)Flesh("Battering jaw",new Vector3(0,-.14f,.32f),new Vector3(.57f,.27f,.36f),ivory,creatureHead);
            for(int i=0;i<3;i++)Horn("Dorsal crest",new Vector3(0,.72f-i*.11f,-.15f-i*.10f),new Vector3(0,1,-.25f),.28f,.16f,accent);
        }
        void MythicCreature(CreatureForm form)
        {
            switch(form){
                case CreatureForm.Crab:case CreatureForm.Coalcrab:case CreatureForm.Tortoise:case CreatureForm.Snail:case CreatureForm.Shellback:ShellCreature(form);return;
                case CreatureForm.Wisp:case CreatureForm.Walker:case CreatureForm.Umbrella:case CreatureForm.BellSpirit:case CreatureForm.Ancestor:case CreatureForm.Lantern:case CreatureForm.Seer:Spirit(form);return;
                case CreatureForm.Moth:case CreatureForm.MoonMoth:Moth(form==CreatureForm.MoonMoth);return;
                case CreatureForm.Hatchling:case CreatureForm.CoilDrake:case CreatureForm.Spitter:case CreatureForm.Ramjaw:case CreatureForm.Wyvern:case CreatureForm.TwinDrake:case CreatureForm.Wyrm:Dragon(form);return;
                case CreatureForm.Sphinx:case CreatureForm.Gryphon:
                    Quadruped(CreatureForm.Fox);
                    for(int side=-1;side<=1;side+=2)for(int i=0;i<4;i++)Feather("Guardian wing",new Vector3(side*(.21f+i*.075f),.94f+i*.11f,-.14f),new Vector3(.28f,.81f,.18f),-side*38,i%2==0?accent:ivory,creatureGesture);
                    if(form==CreatureForm.Gryphon)Horn("Gryphon beak",new Vector3(0,0,.29f),Vector3.forward,.24f,.18f,ivory,creatureHead);
                    else Loop("Astral crown",new Vector3(0,1.28f,0),.46f,Quaternion.Euler(90,0,0),ivory,weapon);
                    return;
                case CreatureForm.Serpent:case CreatureForm.Hydra:
                    for(int i=0;i<3;i++)Loop("Serpent coil",new Vector3(0,.20f+i*.13f,0),.76f-i*.10f,Quaternion.identity,shell,weapon);
                    Curl("Rising neck",new[]{new Vector3(0,.38f,-.16f),new Vector3(-.13f,.86f,-.16f),new Vector3(0,1.26f,.10f)},.20f,accent);
                    Face(new Vector3(0,1.27f,.13f),new Vector3(.34f,.28f,.42f),false,0);
                    if(form==CreatureForm.Hydra)for(int side=-1;side<=1;side+=2){Curl("Hydra neck "+side,new[]{new Vector3(side*.15f,.4f,0),new Vector3(side*.38f,.8f,-.05f),new Vector3(side*.30f,1.06f,.16f)},.14f,shell,creatureGesture);Flesh("Hydra head",new Vector3(side*.30f,1.07f,.18f),new Vector3(.25f,.24f,.31f),ivory,creatureGesture);}
                    else for(int side=-1;side<=1;side+=2)Feather("Serpent hood",new Vector3(side*.20f,1.11f,-.07f),new Vector3(.35f,.68f,.18f),-side*22,ivory);
                    return;
                case CreatureForm.Runestone:case CreatureForm.Cairn:
                    for(int i=0;i<3;i++){var stone=Flesh("Floating stone",new Vector3(i%2==0?-.035f:.05f,.27f+i*.36f,0),new Vector3(.65f-i*.12f,.35f,.61f-i*.10f),i==1?accent:shell);stone.transform.localRotation=Quaternion.Euler(0,i*37,i%2==0?12:-10);}
                    Face(new Vector3(0,1.25f,.04f),new Vector3(.36f,.34f,.30f),false,.065f);
                    if(form==CreatureForm.Runestone){for(int side=-1;side<=1;side+=2)Crystal("Satellite rune",new Vector3(side*.33f,.91f,0),new Vector3(.19f,.54f,.19f),ivory,creatureGesture);}
                    else for(int i=-1;i<=1;i++)Flesh("Moss cap",new Vector3(i*.11f,1.45f,-.03f),new Vector3(.22f,.14f,.25f),accent);
                    return;
                case CreatureForm.Jelly:
                    var cap=Flesh("Jelly bell",new Vector3(0,1.06f,0),new Vector3(.85f,.63f,.73f),ivory);cap.GetComponent<MeshFilter>().sharedMesh=game.Models.Bell;
                    Face(new Vector3(0,.91f,.22f),new Vector3(.34f,.27f,.20f),false,.035f);
                    for(int i=0;i<7;i++){float a=i*Mathf.PI*2/7;var d=new Vector3(Mathf.Sin(a),0,Mathf.Cos(a));Curl("Jelly ribbon "+i,new[]{d*.25f+Vector3.up*.85f,d*.33f+Vector3.up*.52f,d*.20f+Vector3.up*.19f},.055f,accent,creatureGesture);}
                    return;
                case CreatureForm.Wisps:case CreatureForm.Gust:case CreatureForm.Chime:
                    for(int i=0;i<3;i++){
                        float x=(i-1)*.26f,y=.60f+(i%2)*.48f;
                        if(form==CreatureForm.Gust)Loop("Wind spiral",new Vector3(0,.30f+i*.29f,0),.72f-i*.16f,Quaternion.Euler(i*12,0,i*9),i%2==0?ivory:accent,creatureGesture);
                        else{Limb("Spirit stem",new Vector3(x,.12f,0),new Vector3(x,y,0),.055f,ivory);Flesh("Orbiting familiar",new Vector3(x,y,0),new Vector3(.29f,.34f,.27f),accent,creatureGesture);Flesh("Familiar eye",new Vector3(x,y,.145f),Vector3.one*.065f,light,creatureGesture);}
                    }
                    Face(new Vector3(0,form==CreatureForm.Chime?1.45f:1.25f,0),new Vector3(.30f,.30f,.27f),false,.045f);
                    if(form==CreatureForm.Chime)for(int side=-1;side<=1;side+=2)Feather("Wind vane",new Vector3(side*.24f,1.25f,0),new Vector3(.34f,.71f,.14f),-side*64,ivory,creatureGesture);
                    return;
            }
        }
    }
}
