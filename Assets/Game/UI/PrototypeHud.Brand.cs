using UnityEngine;
namespace FrostMaze
{
    public sealed partial class PrototypeHud
    {
        Texture2D brandLogo;
        bool brandLoaded;
        Vector2 menuSettingsScroll;
        // Sample the artwork with a small safety margin; the source PNG keeps its full transparent canvas.
        static readonly Rect BrandUv=new Rect(289f/1774f,41f/887f,1199f/1774f,808f/887f);
        const float BrandAspect=1199f/808f;
        void DrawBrand(Rect bounds)
        {
            if(!brandLoaded){brandLogo=Resources.Load<Texture2D>("Brand/HowlForMaul");brandLoaded=true;}
            if(brandLogo==null){GUI.Label(bounds,"HOWL FOR MAUL",title);return;}
            float width=Mathf.Min(bounds.width,bounds.height*BrandAspect),height=width/BrandAspect;
            var rect=new Rect(bounds.center.x-width*.5f,bounds.center.y-height*.5f,width,height);
            var tint=GUI.color;GUI.color=Color.white;
            GUI.DrawTextureWithTexCoords(rect,brandLogo,BrandUv,true);GUI.color=tint;
        }
        void BrandHeading(float height)
        {
            var rect=GUILayoutUtility.GetRect(1,height,GUILayout.ExpandWidth(true));DrawBrand(rect);
        }
        void LobbyBrandHeading(float width,string context,string flow,string stage)
        {
            var rect=GUILayoutUtility.GetRect(width,130,GUILayout.Width(width));
            DrawBrand(new Rect(rect.x,rect.y,184,124));
            float x=rect.x+202,w=rect.width-202;
            GUI.Label(new Rect(x,rect.y+17,w,22),context,section);
            GUI.Label(new Rect(x,rect.y+42,w,40),flow,small);
            GUI.Label(new Rect(x,rect.y+86,w,32),stage,title);
        }
    }
}
