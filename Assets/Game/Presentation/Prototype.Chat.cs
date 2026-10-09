using UnityEngine;
using FrostMaze.Simulation;
namespace FrostMaze
{
    public sealed partial class Prototype
    {
        public bool ChatOpen {get;private set;}
        public int ChatOpenedFrame {get;private set;}=-1;
        int chatCapturedFrame=-1;
        World inspectedResult;
        public bool ChatAvailable=>Net!=null&&Net.IsConnected&&OnlineGame.Current!=null&&!OnlineGame.Current.LocalOnly;
        public Rect ChatTriggerRect=>new Rect(ChatAvailable&&LobbyOpen?Screen.width-166*UiScale:20*UiScale,Screen.height-(LobbyOpen?48:282)*UiScale,146*UiScale,28*UiScale);
        public bool ChatCapturesInput=>ChatAvailable&&(ChatOpen||chatCapturedFrame==Time.frameCount||Input.GetKeyDown(KeyCode.Return)||Input.GetKeyDown(KeyCode.KeypadEnter));
        public bool ResultOpen=>World!=null&&World.Finished&&!SetupOpen&&!MenuOpen&&!LobbyOpen&&!ReferenceEquals(inspectedResult,World);
        public void InspectResult(){inspectedResult=World;}
        public void ShowResult(){inspectedResult=null;}
        public void OpenChat(){if(!ChatAvailable)return;ChatOpen=true;ChatOpenedFrame=chatCapturedFrame=Time.frameCount;HasHover=false;}
        public void CloseChat(){ChatOpen=false;chatCapturedFrame=Time.frameCount;}
        void ReadChatInput() {
            if(!ChatAvailable){ChatOpen=false;return;}
            if(ChatOpen&&Input.GetKeyDown(KeyCode.Escape)){CloseChat();return;}
            if(!ChatOpen&&(Input.GetKeyDown(KeyCode.Return)||Input.GetKeyDown(KeyCode.KeypadEnter)))OpenChat();
        }
    }
}
