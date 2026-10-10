#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEditor.SceneManagement;
using FrostMaze.Simulation;
namespace FrostMaze.Tests
{
    public sealed class ElementalFactionTests
    {
        [Test]
        public void PackagedThemesAndNamesMatchFactoriesWithoutChangingRoles()
        {
            var themes=new HashSet<string>();int count=0;
            foreach(bool iron in new[]{false,true}){
                var factory=MapCases.Load(iron);var packaged=Resources.Load<MapDefinition>(iron?"Ironfold":"Rimewatch").Settings;
                for(int f=0;f<factory.Factions.Length;f++){
                    Assert.That(themes.Add(packaged.Factions[f].Name),Is.True);
                    Assert.That(packaged.Factions[f].Name,Is.EqualTo(factory.Factions[f].Name));
                    Assert.That(packaged.Factions[f].Description,Is.EqualTo(factory.Factions[f].Description));
                    Assert.That(packaged.Factions[f].Designs,Is.EqualTo(factory.Factions[f].Designs));
                }
                for(int i=0;i<factory.Catalog.Length;i++){
                    var a=packaged.Catalog[i];var b=factory.Catalog[i];count++;
                    Assert.That(a.Name,Is.EqualTo(b.Name));Assert.That(a.Cost,Is.EqualTo(b.Cost));Assert.That(a.Refund,Is.EqualTo(b.Refund));
                    Assert.That(a.WoodCost,Is.EqualTo(b.WoodCost));Assert.That(a.Requires,Is.EqualTo(b.Requires));
                    Assert.That(JsonUtility.ToJson(a.Spec),Is.EqualTo(JsonUtility.ToJson(b.Spec)));
                }
            }
            Assert.That(count,Is.EqualTo(76));Assert.That(themes,Is.EquivalentTo(new[]{"Earth","Water","Fire","Air","Tech","Magic","Nature","Ice","Storm","Beasts","Dragons","Spirits"}));
        }
        [UnityTest]
        public IEnumerator LivingRosterAttacksAndIdleFreezeAndStayInsideCells()
        {
            EditorSceneManager.OpenScene("Assets/Game/Maps/MazeLab.unity");yield return new EnterPlayMode();yield return null;int checkedCount=0;
            foreach(string map in new[]{"Rimewatch","Ironfold"}){
                var game=Object.FindFirstObjectByType<Prototype>();
                if(game.Map.name!=map){game.ChooseMap(Resources.Load<MapDefinition>(map));yield return null;yield return null;game=Object.FindFirstObjectByType<Prototype>();}
                game.StartMatch();game.Paused=true;var w=game.World;int gold=w.Gold;
                for(int f=0;f<w.Config.Factions.Length;f++){
                    if(map=="Rimewatch"&&f==1||map=="Ironfold"&&f==0)continue;
                    var forms=new HashSet<string>();
                    foreach(int d in w.Config.Factions[f].Designs){
                        var def=w.Config.Catalog[d];var tower=new Tower{Id=500+d,Design=d,Name=def.Name,Spec=def.Spec,Health=def.Spec.Health,Level=3};
                        var root=new GameObject("Creature articulation audit");var view=root.AddComponent<TowerView>();view.Initialize(game,tower,def,f,true);
                        try{
                            Assert.That(forms.Add(view.CreatureIdentity),Is.True,"Repeated body form inside "+w.Config.Factions[f].Name);
                            var head=root.transform.Find(view.Role+" weapon/Living head");Assert.That(head,Is.Not.Null);view.Sync(tower,false);var idle=head.localRotation;var idlePosition=head.localPosition;
                            for(int i=0;i<18;i++)w.Step();view.Sync(tower,false);Assert.That(Quaternion.Angle(idle,head.localRotation)>.03f||Vector3.Distance(idlePosition,head.localPosition)>.0001f,Is.True,def.Name+" did not animate");
                            if(def.Spec.Damage>0){
                                var before=head.localRotation;w.Shots.Add(new ShotEvent{Serial=10000+d,Design=d,From=tower.Center,To=tower.Center+new V2(1,1)});view.Sync(tower,false);
                                Assert.That(Quaternion.Angle(before,head.localRotation),Is.GreaterThan(4),"Creature did not respond to its shot");
                            }
                            for(int pose=0;pose<10;pose++){
                                foreach(var filter in root.GetComponentsInChildren<MeshFilter>())if(filter.GetComponent<Renderer>().enabled){
                                    var m=root.transform.worldToLocalMatrix*filter.transform.localToWorldMatrix;
                                    foreach(var vertex in filter.sharedMesh.vertices){var p=m.MultiplyPoint3x4(vertex);Assert.That(new Vector2(p.x,p.z).magnitude,Is.LessThanOrEqualTo(.501f),def.Name+" animated out of its cell");}
                                }
                                w.Step();view.Sync(tower,false);
                            }
                            var frozen=head.localRotation;var place=head.localPosition;yield return null;view.Sync(tower,false);
                            Assert.That(head.localRotation,Is.EqualTo(frozen));Assert.That(head.localPosition,Is.EqualTo(place));
                            Assert.That(root.GetComponentsInChildren<Collider>().Length,Is.Zero);checkedCount++;
                        }finally{root.SetActive(false);Object.Destroy(root);w.Shots.Clear();}
                        yield return null;
                    }
                }
                Assert.That(w.Gold,Is.EqualTo(gold));Assert.That(w.Grid.Towers.Count,Is.Zero);
            }
            Assert.That(checkedCount,Is.EqualTo(64));yield return new ExitPlayMode();
        }
    }
}
#endif
