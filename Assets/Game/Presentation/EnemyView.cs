using FrostMaze.Simulation;
using UnityEngine;

namespace FrostMaze
{
    // Original silhouettes. All transforms are cosmetic; navigation owns the collision disc.
    public sealed class EnemyView : MonoBehaviour
    {
        Transform body, leftWing, rightWing;
        GameObject slowHalo;
        Renderer core;
        Material normalMaterial, siegeMaterial, hitMaterial;
        public Enemy Subject { get; private set; }
        float observedHealth;long hitUntil=-1;
        bool flying;
        float radius;
        ModelMeshes meshes;
        readonly System.Collections.Generic.List<Transform> feet=new System.Collections.Generic.List<Transform>();

        public void Initialize(Enemy enemy, Material normal, Material siege, Material shell, Material frost, Material hit, ModelMeshes models)
        {
            meshes=models;Subject=enemy;observedHealth=enemy.Spec.Health;hitMaterial=hit;
            flying = enemy.Spec.Flying;
            radius = enemy.Spec.Radius;
            normalMaterial = normal; siegeMaterial = siege;
            body = new GameObject(flying ? "Winged drifter" : "Armored crawler").transform;
            body.SetParent(transform, false);
            body.localScale = Vector3.one * radius * 2;
            Part("Carapace", PrimitiveType.Sphere, body, new Vector3(0, .12f, 0), new Vector3(.82f, .56f, .98f), shell);
            core = Part("Signal crest", PrimitiveType.Sphere, body, new Vector3(0, .32f, .02f), new Vector3(.7f, .44f, .76f), normal).GetComponent<Renderer>();
            Part("Face visor", PrimitiveType.Cube, body, new Vector3(0, .17f, .48f), new Vector3(.56f, .12f, .1f), normal);
            if (flying)
            {
                leftWing = Wing(-1, normal);
                rightWing = Wing(1, normal);
                Part("Tail fin", PrimitiveType.Cube, body, new Vector3(0, .16f, -.66f), new Vector3(.16f, .35f, .5f), normal);
            }
            else
            {
                for (int side = -1; side <= 1; side += 2)
                    for (int leg = -1; leg <= 1; leg += 2)
                        feet.Add(Part("Crawler foot", PrimitiveType.Sphere, body, new Vector3(side * .36f, -.17f, leg * .3f), new Vector3(.23f, .25f, .42f), shell).transform);
            }
            if(!flying&&(enemy.Spec.Damage>=30||enemy.Spec.Speed<=1.6f)) {
                Part("Siege shield",PrimitiveType.Cube,body,new Vector3(0,.32f,.46f),new Vector3(.9f,.65f,.17f),shell);
                Part("Siege beacon",PrimitiveType.Cylinder,body,new Vector3(0,.72f,0),new Vector3(.25f,.17f,.25f),normal);
            } else if(!flying&&enemy.Spec.Speed>=2.6f) {
                for(int side=-1;side<=1;side+=2)Part("Runner fin",PrimitiveType.Cube,body,new Vector3(side*.25f,.28f,-.35f),new Vector3(.12f,.4f,.65f),normal);
            }
            slowHalo = Part("Frost status", PrimitiveType.Cylinder, transform, Vector3.zero, new Vector3(radius * 2.4f, .018f, radius * 2.4f), frost);
            slowHalo.SetActive(false);
        }

        Transform Wing(int side, Material material)
        {
            var pivot = new GameObject(side < 0 ? "Left wing" : "Right wing").transform;
            pivot.SetParent(body, false);
            pivot.localPosition = new Vector3(side * .3f, .12f, 0);
            var vane = Part("Wing vane", PrimitiveType.Cube, pivot, Vector3.zero, new Vector3(.9f,1,.85f), material);
            vane.GetComponent<MeshFilter>().sharedMesh=meshes.Wing(side);
            vane.transform.localRotation = Quaternion.Euler(0, side * 20, 0);
            return pivot;
        }

        GameObject Part(string name, PrimitiveType type, Transform parent, Vector3 position, Vector3 scale, Material material)
        {
            var part = GameObject.CreatePrimitive(type);
            part.name = name;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = position;
            part.transform.localScale = scale;
            var collider = part.GetComponent<Collider>();
            collider.enabled = false;
            Destroy(collider);
            part.GetComponent<Renderer>().sharedMaterial = material;
            if(type==PrimitiveType.Sphere)part.GetComponent<MeshFilter>().sharedMesh=meshes.Shell;
            if(type==PrimitiveType.Cylinder)part.GetComponent<MeshFilter>().sharedMesh=meshes.Column;
            return part;
        }

        public void Sync(Enemy enemy, long tick)
        {
            float phase = tick * World.FixedDelta * 7f + enemy.Id * .73f;
            transform.position = new Vector3(enemy.Position.X, flying ? 1.7f : radius * .65f, enemy.Position.Y);
            var direction = enemy.Velocity.Length > .03f ? enemy.Velocity : enemy.IntendedDirection;
            if (direction.Length > .03f)
                transform.rotation = Quaternion.LookRotation(new Vector3(direction.X, 0, direction.Y));
            body.localPosition = new Vector3(0, flying ? Mathf.Sin(phase) * .08f : 0, 0);
            if (flying)
            {
                float flap = Mathf.Sin(phase) * 22f;
                leftWing.localRotation = Quaternion.Euler(0, 0, flap);
                rightWing.localRotation = Quaternion.Euler(0, 0, -flap);
            }
            for(int i=0;i<feet.Count;i++)feet[i].localRotation=Quaternion.Euler(Mathf.Sin(phase*1.7f+i*Mathf.PI)*Mathf.Min(1,enemy.Velocity.Length)*24,0,i<2?-22:22);
            if(enemy.Health<observedHealth)hitUntil=tick+3;
            observedHealth=enemy.Health;
            core.sharedMaterial = tick<hitUntil ? hitMaterial : enemy.Blocked ? siegeMaterial : normalMaterial;
            slowHalo.SetActive(enemy.SlowRemaining > 0);
            // Status marker follows flight height so it cannot be mistaken for a ground unit.
            slowHalo.transform.localPosition = new Vector3(0, flying ? -.3f : -radius * .65f + .025f, 0);
        }
    }
}
