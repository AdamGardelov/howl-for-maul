using System.Collections;
using UnityEngine;
namespace FrostMaze
{
    // Created only by --howl-smoke-test. Normal games never create this component.
    public sealed class StandaloneSmokeExit : MonoBehaviour
    {
        public int Result;
        IEnumerator Start()
        {
            yield return null;
            yield return null;
            Debug.Log("HOWL_SMOKE_EXIT normal-frame result="+Result);
            Application.Quit(Result);
        }
    }
}
