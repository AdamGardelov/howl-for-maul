using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Authentication;
namespace FrostMaze
{
    // Display service account notices without logging player identifiers or notification text.
    public static class RelayNotices
    {
        static readonly List<Notification> pending=new List<Notification>();
        public static bool Available=>pending.Count>0;
        public static string Text=>string.Join("\n\n",pending.Select(n=>n.Message+"\nCase: "+n.CaseId+" · Player: "+n.PlayerId));
        static string Key(string player)=>"howl.online.notice."+player;
        public static async Task Fetch(){
            var auth=AuthenticationService.Instance;
            if(long.TryParse(auth.LastNotificationDate,out var latest)&&latest>ReadDate(auth.PlayerId))Set(await auth.GetNotificationsAsync());
        }
        static long ReadDate(string player)=>long.TryParse(PlayerPrefs.GetString(Key(player),"0"),out var value)?value:0;
        public static void Set(IEnumerable<Notification> notices){pending.Clear();if(notices!=null)pending.AddRange(notices.Where(n=>!long.TryParse(n.CreatedAt,out var date)||date>ReadDate(n.PlayerId)));}
        public static void Acknowledge(){foreach(var notice in pending)if(long.TryParse(notice.CreatedAt,out var date))PlayerPrefs.SetString(Key(notice.PlayerId),Math.Max(date,ReadDate(notice.PlayerId)).ToString());PlayerPrefs.Save();pending.Clear();}
    }
}
