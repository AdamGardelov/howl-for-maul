using UnityEngine;
namespace FrostMaze
{
    public sealed partial class TowerView
    {
        Transform kinetic;Quaternion kineticRest;float kineticSpeed;bool kineticSwing;
        void FitSilhouette()
        {
            // Keep even diagonal weapon turns and upgrades inside the actual occupied cell.
            // Normalize horizontal geometry only; the order's height and negative space survive.
            float radius=.47f;var vertices=new System.Collections.Generic.List<Vector3>();
            foreach(var filter in GetComponentsInChildren<MeshFilter>()){
                var matrix=transform.worldToLocalMatrix*filter.transform.localToWorldMatrix;
                filter.sharedMesh.GetVertices(vertices);
                foreach(var vertex in vertices){var p=matrix.MultiplyPoint3x4(vertex);radius=Mathf.Max(radius,new Vector2(p.x,p.z).magnitude);}
            }
            float scale=(livingCrown!=null||creature?.43f:.47f)/radius;
            // Fit the weapon in tower axes. Static pieces receive the same matrix during batching:
            // scaling a slanted piece's local X/Z leaves its tilted local Y sticking outside the cell.
            if(weapon!=null)weapon.localScale=new Vector3(scale,weaponHeight,scale);
            weaponWidth=scale;
        }
        void FactionFooting(Material trim,Material dark,int faction,bool iron)
        {
            if((iron&&faction!=0)||(!iron&&faction!=1)){
                var bed=Part("Foundation",PrimitiveType.Sphere,new Vector3(0,.04f,0),new Vector3(.78f,.07f,.72f),dark);
                bed.GetComponent<MeshFilter>().sharedMesh=game.Models.Shell;return;
            }
            bool stone=!iron&&(faction==0||faction==1);
            var foot=Part("Foundation",stone?PrimitiveType.Cylinder:PrimitiveType.Cube,new Vector3(0,.045f,0),new Vector3(stone?.84f:.64f,.04f,stone?.84f:.60f),dark);
            if(!iron&&faction==1)foot.GetComponent<MeshFilter>().sharedMesh=game.Models.Shell;
            if(iron&&(faction==2||faction==4||faction==7)){foot.GetComponent<MeshFilter>().sharedMesh=game.Models.Column;foot.transform.localScale=new Vector3(.60f,.035f,.60f);}
            if(!iron&&faction==0)Part("Snow drift",PrimitiveType.Sphere,new Vector3(-.20f,.12f,-.14f),new Vector3(.56f,.18f,.47f),trim);
        }
        Transform Motion(string name,Vector3 position,Quaternion rotation,float speed)
        {
            kinetic=new GameObject(name).transform;kinetic.SetParent(weapon??transform,false);kinetic.localPosition=position;
            kinetic.localRotation=kineticRest=rotation;kineticSpeed=speed;return kinetic;
        }
        GameObject Loop(string name,Vector3 p,float diameter,Quaternion rotation,Material material,Transform parent=null)
        {
            var loop=Part(name,PrimitiveType.Sphere,p,Vector3.one*diameter,material,parent);
            loop.GetComponent<MeshFilter>().sharedMesh=game.Models.Halo;loop.transform.localRotation=rotation;return loop;
        }
        void Strut(string name,Vector3 a,Vector3 b,float width,Material material,Transform parent=null)
        {
            var beam=Part(name,PrimitiveType.Cylinder,(a+b)*.5f,new Vector3(width,(b-a).magnitude*.5f,width),material,parent);
            beam.transform.localRotation=Quaternion.FromToRotation(Vector3.up,b-a);
        }
    }
}
