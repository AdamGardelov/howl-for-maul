namespace FrostMaze.Simulation
{
    // Presentation identities are separate from roster IDs, prices and combat tuning.
    public static class FactionIdentity
    {
        static readonly string[] WinterThemes=new[]{"Ice","Nature","Fire","Storm"};
        static readonly string[] WinterOrders=new[]{"Rime Covenant","Rootbound","Ember Assembly","Stormcallers"};
        static readonly string[] WinterFlavors=new[]{"Snow creatures and aurora spirits. Slow the front line and shatter clustered ground enemies.","Ancient tree guardians. Seed volleys, durable oaks and binding roots; Skybough guards the air.","Creatures born around the village hearth. Rapid attacks and expensive bombardment reward compact defenses.","Mountain wildlife carrying thunder. Affordable mazes, precise air defense and chaining bolts."};
        static readonly string[][] WinterNames={
            new[]{"Snowcap Owl","Slumberbear","Rimehart","Hailtoad","Aurora Crane"},
            new[]{"Seedling Warden","Oldbark","Briar Elder","Skybough","Worldroot"},
            new[]{"Cinder Newt","Coalback","Furnace Toad","Flarewing","Ash Elder"},
            new[]{"Spark Jay","Grounding Ram","Thunderhart","Storm Heron","Tempest Roc"}
        };
        static readonly string[] IronThemes=new[]{"Tech","Beasts","Magic","Air","Earth","Spirits","Dragons","Water"};
        static readonly string[] IronOrders=new[]{"Pulse Foundry","Shellbrood","Starweavers","Skyward Guild","Stonewake","Lantern Court","Cinderwing Brood","Tidekin"};
        static readonly string[] IronFlavors=new[]{"Copper instruments from the refuge workshop.","Armored insects with powerful jaws and natural weapons.","Enchanted familiars and living constellations.","Wind birds and creatures of the high passes.","Ancient mountain creatures with moss and mineral hearts.","Gentle masked spirits guarding the road home.","Young drakes and old wyrms from the warm mountain caves.","Spring creatures, shells and living water."};
        static readonly string[][] IronNames={
            new[]{"Copper Lantern","Ironhand Press","Shearwheel","Surveyor Lens","Flare Beacon","Cooling Vessel","Hearthbell"},
            new[]{"Pebble Beetle","Staghorn","Burrback","Reed Mantis","Glasswing","Briar Scorpion","Brood Colossus"},
            new[]{"Runefox","Wispkeeper","Moonstone","Spellcoil","Starmoth","Wayseer","Astral Sphinx"},
            new[]{"Breeze Owl","Gust Dancer","Cloudhare","Reed Crane","Gale Hawk","Windchime","Sky Gryphon"},
            new[]{"Pebbleback","Anchor Ram","Boulderhand","Cairn Elder","Crag Ibex","Mossbear","Mountain Ox"},
            new[]{"Hearth Wisp","Lantern Walker","Rainkeeper","Bell Keeper","Moon Moth","Antler Guide","Elder Lantern"},
            new[]{"Hatchling","Frostcoil","Kiln Spitter","Ramjaw","Dusk Wyvern","Twinflame","Hearth Wyrm"},
            new[]{"Brook Crab","Pearl Snail","Reed Hydra","Pool Toad","River Skimmer","Lantern Jelly","Ancient Shellback"}
        };
        static readonly string[] WinterBrief={"Snow wildlife and aurora spirits","Ancient trees and thorn guardians","Creatures of the village hearth","Mountain wildlife carrying thunder"};
        static readonly string[] IronBrief={"Copper craft from the refuge","Armored insects and chitin","Runic familiars and living stars","Wind birds and high-pass spirits","Stone beasts with living moss","Masked guardians of the road","Young drakes and ancient wyrms","Shell creatures and living water"};
        public static string Brief(string theme,int faction)=>(theme=="iron"?IronBrief:WinterBrief)[faction];
        public static string Order(string theme,int faction)=>(theme=="iron"?IronOrders:WinterOrders)[faction];
        public static string Theme(string theme,int faction)=>(theme=="iron"?IronThemes:WinterThemes)[faction];
        public static void Apply(Scenario c,bool iron)
        {
            var themes=iron?IronThemes:WinterThemes;var names=iron?IronNames:WinterNames;
            for(int f=0;f<c.Factions.Length;f++){
                c.Factions[f].Name=themes[f];
                c.Factions[f].Description=iron?IronFlavors[f]+" "+c.Factions[f].Description:WinterFlavors[f];
                for(int slot=0;slot<c.Factions[f].Designs.Length;slot++)c.Catalog[c.Factions[f].Designs[slot]].Name=names[f][slot];
            }
            if(!iron){c.Catalog[15].Description="Ground + air · quick spark volleys";c.Catalog[19].Description="Ground + air · heavy thunder strikes";}
        }
    }
}
