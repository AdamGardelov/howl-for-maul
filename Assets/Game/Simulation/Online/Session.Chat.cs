using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
namespace FrostMaze.Simulation.Online
{
    public sealed class ChatLine
    {
        public readonly long Sequence;public readonly int PlayerId,Slot;public readonly string Name,Text;
        public ChatLine(long sequence,int id,int slot,string name,string text){Sequence=sequence;PlayerId=id;Slot=slot;Name=name;Text=text;}
    }
    public sealed partial class Session
    {
        public const int ChatLimit=240,ChatHistoryLimit=64;
        readonly List<ChatLine> chat=new List<ChatLine>();
        readonly HashSet<int> mutedChat=new HashSet<int>();
        readonly Dictionary<int,ChatAllowance> chatAllowances=new Dictionary<int,ChatAllowance>();
        sealed class ChatAllowance {public double Tokens=3,At;}
        long chatSequence;
        public IReadOnlyList<ChatLine> Chat=>chat;
        public bool ChatMuted(int id)=>mutedChat.Contains(id);
        public void MuteChat(int id,bool mute){if(mute)mutedChat.Add(id);else mutedChat.Remove(id);}
        public static string CleanChat(string text) {
            var value=new StringBuilder();bool space=false;
            foreach(char c in text??"") {
                if(char.IsWhiteSpace(c)){space=value.Length>0;continue;}
                if(char.IsControl(c)||char.GetUnicodeCategory(c)==UnicodeCategory.Format)continue;
                if(space&&value.Length<ChatLimit)value.Append(' ');space=false;
                if(value.Length>=ChatLimit)break;value.Append(c);
            }
            if(value.Length>0&&char.IsHighSurrogate(value[value.Length-1]))value.Length--;
            return value.ToString().Trim();
        }
        public bool SendChat(string text){text=CleanChat(text);if(!IsConnected||text.Length==0)return false;Send(new Packet{Kind=Kind.Chat,Text=text});return true;}
        void PublishChat(Member member,string text) {
            text=CleanChat(text);if(text.Length==0)return;
            if(!chatAllowances.TryGetValue(member.Id,out var allowance)){allowance=new ChatAllowance{At=clock};chatAllowances.Add(member.Id,allowance);}
            allowance.Tokens=Math.Min(3,allowance.Tokens+(clock-allowance.At)/1.5);allowance.At=clock;
            if(allowance.Tokens<1){var notice=new Packet{Kind=Kind.Notice,Text="Chat is moving too quickly. Wait a moment before sending again."};if(member.Id==LocalId)Notice=notice.Text;else peers.Find(p=>p.Id==member.Id)?.Link.Send(notice);return;}
            allowance.Tokens--;
            // Identity and sequence come exclusively from the authenticated host-side member.
            var line=new ChatLine(++chatSequence,member.Id,members.IndexOf(member),member.Name,text);StoreChat(line);Broadcast(ChatPacket(line));
        }
        static Packet ChatPacket(ChatLine line)=>new Packet{Kind=Kind.Chat,A=line.PlayerId,B=line.Slot,Tick=line.Sequence,Extra=line.Name,Text=line.Text};
        void ReceiveChat(Packet p) {
            if(p.Tick<=chatSequence||p.B<0||p.B>3)return;
            string text=CleanChat(p.Text);if(text.Length==0)return;
            chatSequence=p.Tick;StoreChat(new ChatLine(p.Tick,p.A,p.B,Clean(p.Extra),text));
        }
        void StoreChat(ChatLine line){if(chat.Count==ChatHistoryLimit)chat.RemoveAt(0);chat.Add(line);}
    }
}
