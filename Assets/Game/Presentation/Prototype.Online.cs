using UnityEngine;
using FrostMaze.Simulation;
using FrostMaze.Simulation.Online;
namespace FrostMaze
{
    public sealed partial class Prototype
    {
        public Session Net=>OnlineGame.Current==null?null:OnlineGame.Current.Session;
        public bool NetworkMatch=>Net!=null&&Net.World!=null;
        public bool LobbyOpen=>OnlineGame.Current!=null&&!NetworkMatch;
        public void BeginSolo(){var lobby=OnlineGame.Create();lobby.Host(Map.name,"Player 1","",0,true);}
        public void AdoptOnlineWorld(World world){ClearUnitViews();World=world;SetupOpen=MenuOpen=DetailsOpen=false;matchStarted=true;accumulator=0;ClearInteraction();View.GetComponent<RtsCamera>().FocusPoint(World.BuilderPosition);}
        public void LeaveOnline(){OnlineGame.Current?.Leave();SetupOpen=true;MenuOpen=false;Paused=true;matchStarted=false;}
        public void VotePause(){if(NetworkMatch)Net.Send(new Packet{Kind=Kind.PauseVote});else Paused=!Paused;}
        public void Issue(Order order){if(NetworkMatch)Net.Submit(order);}
        public void ChooseFaction(int faction){if(NetworkMatch){Issue(new Order{Kind=ActionKind.ChooseFaction,Target=faction});return;}World.ChooseFaction(faction,out string message);Notice=message;}
        public void UpgradeTower(int id){if(NetworkMatch){Issue(new Order{Kind=ActionKind.Upgrade,Target=id});return;}World.Upgrade(id,out string message);Notice=message;}
        public bool SellTower(float x,float y){if(NetworkMatch){Issue(new Order{Kind=ActionKind.Sell,X=x,Y=y});return true;}return World.Sell(x,y);}
        public void MoveTo(V2 position){if(NetworkMatch){Issue(new Order{Kind=ActionKind.Move,X=position.X,Y=position.Y});return;}World.MoveBuilder(position);}
    }
}
