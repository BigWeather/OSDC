#define HEX

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
#if Allow_XNA
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
#if WINDOWS_PHONE
using Microsoft.Xna.Framework.Input.Touch;
#endif
#endif
using GamesLibrary;

namespace IntoTheNewWorld
{
    // State for each square
    // Objects that can occur on terrain
    // Background color for terrain
    // Blended movement
    // Density varying graphics, visiblity, etc.
    // Large, random map
    // Unhappy with Graphic being primary touchpoint along with all the ["ocean"] type crap
    // Handle on boat, parked boat, etc. better

    // Cool seeds:
    // 1103239772 -- awesome river around (10,65)

    public class IntoTheNewWorld : BaseGame, MapConsumerInterface
    {
        public List<River> rivers;

        private Dictionary<char, Terrain> _dictTerrainByMapCharacter = new Dictionary<char, Terrain>();
#if HEX
        //private Vector2 _tilesize = new Vector2(87, 75);
        private Vector2 _tilesizeA = new Vector2(87, 75);
#else
        private Vector2 _tilesize = new Vector2(75);
#endif
        //public int worldCoordinatesPerMile = 15;
        public int worldCoordinatesPerMile = 45;
        public int milesPerTile = 5;

        public Vector2 v2WorldCoordinatesPerTile;

#if HEX
        public MapHex<MapTile> _map;
#else
        public MapSquare<MapTile> _map;
#endif
        public List<Explorer> players;

        public Europe europe;

        public List<Mob> NPCs;
        public List<City> cities;
        public List<PointOfInterest> PoIs;
        public List<Explorer> explorers;
        public List<IntoTheNewWorldCache> caches;
        public List<Ship> parkedShips;

        public HashSet<string> recognizedPOIs = new HashSet<string>();

        public Dictionary<string, GamesLibrary.Action> actionsByIdentifier;

        public int days;
        private int elapsed = 0;
        private int msperday = 1000;
        public DateTime date = new DateTime(1492, 8, 3);

        public WorldModel world;

        internal int foodperday = 10;
        internal int forageperday = 4;

        private Vector2 oldPlayerPos = Vector2.Zero;

        public const float milesPerMillisecond = 0.01f;

        private Vector2 v2GPlayerStart = new Vector2(45, 12);

        public int mapSeed = -1;

        public static IntoTheNewWorld Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new IntoTheNewWorld();

                return _instance;
            }
        }
        private static IntoTheNewWorld _instance = null;

        public WindowDecorations windowDecorations
        {
            get
            {
                if (_windowDecorations == null)
                    initWindowDecorationsAndBackground();

                return _windowDecorations;
            }
        }
        private WindowDecorations _windowDecorations = null;

        public WindowBackground windowBackground
        {
            get
            {
                if (_windowBackground == null)
                    initWindowDecorationsAndBackground();

                return _windowBackground;
            }
        }
        private WindowBackground _windowBackground = null;

        public bool forceUpdateGameState = false;

        private IntoTheNewWorld() { }

        public bool isPlayer(MapObject mo)
        {
            if (!(mo is Explorer))
                return false;

            if (this.players == null)
                return false;

            return this.players.Contains((Explorer)mo);
        }

        // Overridden methods below...
        public override void InitializeGraphics()
        {
        }

        private List<Animation> getDensityAnimations(int row, int col, Rectangle rectTile, int med, int high)
        {
            List<Animation> animations = new List<Animation>();
            animations.Add(new Animation(new Condition("density", VariableType.Integer, Operation.LessThan, "" + med), Animation.Behavior.Static, new Frame(row, col, rectTile)));
            animations.Add(new Animation(new Condition("density", VariableType.Integer, Operation.LessThan, "" + high), Animation.Behavior.Static, new Frame(row, col + 1, rectTile)));
            animations.Add(new Animation(new Condition("density", VariableType.Integer, Operation.GreatherThanEqual, "" + high), Animation.Behavior.Static, new Frame(row, col + 2, rectTile)));

            return animations;
        }

        private List<Animation> getVisibleAndDensityAnimations(int row, int col, Rectangle rectTile, Point anchor, int med, int high)
        {
            return getVisibleAndDensityAnimations(row, col, rectTile, anchor, med, high, null);
        }

        private List<Animation> getVisibleAndDensityAnimations(int row, int col, Rectangle rectTile, Point anchor, int med, int high, List<Condition> additionalConditions)
        {
            List<Animation> animations = new List<Animation>();

            // non-visible high density
            List<Condition> conditionsNVHD = new List<Condition>();
            conditionsNVHD.Add(new Condition("visible", VariableType.Integer, Operation.Equals, "0"));
            conditionsNVHD.Add(new Condition("density", VariableType.Integer, Operation.GreatherThanEqual, "" + high));
            if (additionalConditions != null)
                conditionsNVHD.AddRange(additionalConditions);
            animations.Add(new Animation(conditionsNVHD, Animation.Behavior.Static, new Frame(row, col, rectTile, anchor, int.MaxValue)));

            // visible high density
            List<Condition> conditionsVHD = new List<Condition>();
            conditionsVHD.Add(new Condition("visible", VariableType.Integer, Operation.Equals, "1"));
            conditionsVHD.Add(new Condition("density", VariableType.Integer, Operation.GreatherThanEqual, "" + high));
            if (additionalConditions != null)
                conditionsVHD.AddRange(additionalConditions);
            animations.Add(new Animation(conditionsVHD, Animation.Behavior.Static, new Frame(row, col + 1, rectTile, anchor, int.MaxValue)));

            List<string> betweenNVMD = new List<string>();
            betweenNVMD.Add("" + med);
            betweenNVMD.Add("" + (high - 1));

            // non-visible medium density
            List<Condition> conditionsNVMD = new List<Condition>();
            conditionsNVMD.Add(new Condition("visible", VariableType.Integer, Operation.Equals, "0"));
            conditionsNVMD.Add(new Condition("density", VariableType.Integer, Operation.InclusiveBetween, betweenNVMD));
            if (additionalConditions != null)
                conditionsNVMD.AddRange(additionalConditions);
            animations.Add(new Animation(conditionsNVMD, Animation.Behavior.Static, new Frame(row, col + 2, rectTile, anchor, int.MaxValue)));

            // visible medium density
            List<Condition> conditionsVMD = new List<Condition>();
            conditionsVMD.Add(new Condition("visible", VariableType.Integer, Operation.Equals, "1"));
            conditionsVMD.Add(new Condition("density", VariableType.Integer, Operation.InclusiveBetween, betweenNVMD));
            if (additionalConditions != null)
                conditionsVMD.AddRange(additionalConditions);
            animations.Add(new Animation(conditionsVMD, Animation.Behavior.Static, new Frame(row, col + 3, rectTile, anchor, int.MaxValue)));

            // non-visible low density
            List<Condition> conditionsNVLD = new List<Condition>();
            conditionsNVLD.Add(new Condition("visible", VariableType.Integer, Operation.Equals, "0"));
            conditionsNVLD.Add(new Condition("density", VariableType.Integer, Operation.LessThan, "" + med));
            if (additionalConditions != null)
                conditionsNVLD.AddRange(additionalConditions);
            animations.Add(new Animation(conditionsNVLD, Animation.Behavior.Static, new Frame(row, col + 4, rectTile, anchor, int.MaxValue)));

            // visible low density
            List<Condition> conditionsVLD = new List<Condition>();
            conditionsVLD.Add(new Condition("visible", VariableType.Integer, Operation.Equals, "1"));
            conditionsVLD.Add(new Condition("density", VariableType.Integer, Operation.LessThan, "" + med));
            if (additionalConditions != null)
                conditionsVLD.AddRange(additionalConditions);
            animations.Add(new Animation(conditionsVLD, Animation.Behavior.Static, new Frame(row, col + 5, rectTile, anchor, int.MaxValue)));

            return animations;
        }

        private void loadTiles()
        {
            // TODO: Eventually (soon) we want to treat all forests (forest, tropical_forest, evergreen_forest) as the "same"
            //       for the purpose of density, etc. and use Conditions (based on latitude, probably) to pick the correct one.

            //Rectangle rectTile = new Rectangle(0, 0, (int)_tilesize.X, (int)_tilesize.Y);
            Rectangle rectTile = new Rectangle(0, 0, 75, 75);
            Rectangle hexTile = new Rectangle(0, 0, 87, 100);
            Point hexAnchor = new Point(0, 0);
#if HEX
            int med = 3; // 3
            int high = 5; // 6
#else
            int med = 5;
            int high = 8;
#endif
            addGraphic(new Graphic("plains", "ItNW_Terrain", getVisibleAndDensityAnimations(1, 0, hexTile, hexAnchor, med, high)));
            addGraphic(new Graphic("forest", "ItNW_Terrain", getVisibleAndDensityAnimations(0, 0, hexTile, hexAnchor, med, high, new Condition[] { new Condition("season", VariableType.String, Operation.Equals, new string[] { "summer" }.ToList()) }.ToList())));
            this.dictGraphics["forest"].animations.AddRange(getVisibleAndDensityAnimations(1, 6, hexTile, hexAnchor, med, high, new Condition[] { new Condition("season", VariableType.String, Operation.Equals, new string[] { "fall" }.ToList()) }.ToList()));
            this.dictGraphics["forest"].animations.AddRange(getVisibleAndDensityAnimations(5, 8, hexTile, hexAnchor, med, high, new Condition[] { new Condition("season", VariableType.String, Operation.Equals, new string[] { "winter" }.ToList()) }.ToList()));
            this.dictGraphics["forest"].animations.AddRange(getVisibleAndDensityAnimations(6, 8, hexTile, hexAnchor, med, high, new Condition[] { new Condition("season", VariableType.String, Operation.Equals, new string[] { "spring" }.ToList()) }.ToList()));
            addGraphic(new Graphic("ocean", "ItNW_Terrain", getVisibleAndDensityAnimations(2, 0, hexTile, hexAnchor, med, high)));
            addGraphic(new Graphic("desert", "ItNW_Terrain", getVisibleAndDensityAnimations(2, 6, hexTile, hexAnchor, med, high)));
            addGraphic(new Graphic("mountains", "ItNW_Terrain", getVisibleAndDensityAnimations(0, 6, hexTile, hexAnchor, med, high)));
            addGraphic(new Graphic("hills", "ItNW_Terrain", getVisibleAndDensityAnimations(6, 0, hexTile, hexAnchor, med, high)));
            addGraphic(new Graphic("evergreen_forest", "ItNW_Terrain", getVisibleAndDensityAnimations(4, 0, hexTile, hexAnchor, med, high)));
            addGraphic(new Graphic("swamp", "ItNW_Terrain", getVisibleAndDensityAnimations(5, 0, hexTile, hexAnchor, med, high)));
            addGraphic(new Graphic("tropical_forest", "ItNW_Terrain", getVisibleAndDensityAnimations(3, 0, hexTile, hexAnchor, med, high)));
            addGraphic(new Graphic("savanna", "ItNW_Terrain", getVisibleAndDensityAnimations(3, 6, hexTile, hexAnchor, med, high)));
            addGraphic(new Graphic("tundra", "ItNW_Terrain", getVisibleAndDensityAnimations(4, 6, hexTile, hexAnchor, med, high)));
            //addGraphic(new GraphicSimple("native_city", "PassageIndies_Tiles", 7, 7, rectTile, new Point(rectTile.Width / 2, rectTile.Height / 2)));
            //addGraphic(new GraphicSimple("nonvisible", "PassageIndies_Tiles", 0, 6, rectTile));
            addGraphic(new GraphicSimple("one_square_circle", "PassageIndies_Tiles", new Rectangle(0, 160, 27, 27), new Point(27 / 2, 27 / 2)));
#if HEX
            //addGraphic(new GraphicSimple("river", "PassageIndies_Tiles", new Rectangle(124, 386, 20, 50)));
            List<Animation> animationsRiver = new List<Animation>();
            animationsRiver.Add(new Animation(new Condition("visible", VariableType.Integer, Operation.Equals, "0"), Animation.Behavior.Static, new Frame(new Rectangle(124, 386, 20, 50), new Point(0, 0), int.MaxValue)));
            animationsRiver.Add(new Animation(new Condition("visible", VariableType.Integer, Operation.Equals, "1"), Animation.Behavior.Static, new Frame(new Rectangle(124, 333, 20, 50), new Point(0, 0), int.MaxValue)));
            addGraphic(new Graphic("river", "PassageIndies_Tiles", animationsRiver));

            //addGraphic(new GraphicSimple("river_source", "PassageIndies_Tiles", new Rectangle(147, 386, 20, 50)));
            List<Animation> animationsRiverSource = new List<Animation>();
            animationsRiverSource.Add(new Animation(new Condition("visible", VariableType.Integer, Operation.Equals, "0"), Animation.Behavior.Static, new Frame(new Rectangle(147, 386, 20, 50), new Point(0, 0), int.MaxValue)));
            animationsRiverSource.Add(new Animation(new Condition("visible", VariableType.Integer, Operation.Equals, "1"), Animation.Behavior.Static, new Frame(new Rectangle(147, 333, 20, 50), new Point(0, 0), int.MaxValue)));
            addGraphic(new Graphic("river_source", "PassageIndies_Tiles", animationsRiverSource));

            //addGraphic(new GraphicSimple("coastline", "PassageIndies_Tiles", new Rectangle(170, 386, 20, 50)));
            List<Animation> animationsCoastline = new List<Animation>();
            animationsCoastline.Add(new Animation(new Condition("visible", VariableType.Integer, Operation.Equals, "0"), Animation.Behavior.Static, new Frame(new Rectangle(170, 386, 20, 50), new Point(0, 0), int.MaxValue)));
            animationsCoastline.Add(new Animation(new Condition("visible", VariableType.Integer, Operation.Equals, "1"), Animation.Behavior.Static, new Frame(new Rectangle(170, 333, 20, 50), new Point(0, 0), int.MaxValue)));
            addGraphic(new Graphic("coastline", "PassageIndies_Tiles", animationsCoastline));
#else
            addGraphic(new GraphicSimple("river", "PassageIndies_Tiles", new Rectangle(100, 386, 20, 75)));
            addGraphic(new GraphicSimple("coastline", "PassageIndies_Tiles", new Rectangle(170, 386, 20, 50)));
#endif
            Point tokenAnchor = new Point(18, 18);

            addGraphic(new GraphicSimple("token", "PassageIndies_Tiles", new Rectangle(831, 172, 37, 41), tokenAnchor));

            addGraphic(new GraphicSimple("player_foot", "PassageIndies_Tiles", new Rectangle(1125, 179, 35, 35), tokenAnchor));
            addGraphic(new GraphicSimple("native_warrior", "PassageIndies_Tiles", new Rectangle(1125, 216, 35, 35), tokenAnchor));
            addGraphic(new GraphicSimple("fauna", "PassageIndies_Tiles", new Rectangle(903, 216, 35, 35), tokenAnchor));
            addGraphic(new GraphicSimple("flora", "PassageIndies_Tiles", new Rectangle(940, 216, 35, 35), tokenAnchor));
            addGraphic(new GraphicSimple("landmark", "PassageIndies_Tiles", new Rectangle(977, 216, 35, 35), tokenAnchor));
            addGraphic(new GraphicSimple("native_city", "PassageIndies_Tiles", new Rectangle(1014, 216, 35, 35), tokenAnchor));
            addGraphic(new GraphicSimple("dead", "PassageIndies_Tiles", new Rectangle(1014, 179, 35, 35), tokenAnchor));
            addGraphic(new GraphicSimple("native_trader", "PassageIndies_Tiles", new Rectangle(1162, 216, 35, 35), tokenAnchor));
            addGraphic(new GraphicSimple("cache", "PassageIndies_Tiles", new Rectangle(903, 179, 35, 35), tokenAnchor));

            addGraphic(new GraphicSimple("command_trade", "PassageIndies_Tiles", new Rectangle(1088, 179, 35, 35), tokenAnchor));
            addGraphic(new GraphicSimple("command_claim", "PassageIndies_Tiles", new Rectangle(940, 179, 35, 35), tokenAnchor));
            addGraphic(new GraphicSimple("command_fight", "PassageIndies_Tiles", new Rectangle(1051, 179, 35, 35), tokenAnchor));
            addGraphic(new GraphicSimple("command_embark", "PassageIndies_Tiles", new Rectangle(977, 179, 35, 35), tokenAnchor));
            addGraphic(new GraphicSimple("command_disembark", "PassageIndies_Tiles", new Rectangle(977, 179, 35, 35), tokenAnchor));
            addGraphic(new GraphicSimple("command_europe", "PassageIndies_Tiles", new Rectangle(1162, 179, 35, 35), tokenAnchor));
            addGraphic(new GraphicSimple("command_cache", "PassageIndies_Tiles", new Rectangle(903, 179, 35, 35), tokenAnchor));
            addGraphic(new GraphicSimple("command_journal", "PassageIndies_Tiles", new Rectangle(1162, 142, 35, 35), tokenAnchor));
            addGraphic(new GraphicSimple("command_build_tradingpost", "PassageIndies_Tiles", new Rectangle(1125, 142, 35, 35), tokenAnchor));
            addGraphic(new GraphicSimple("command_build_fort", "PassageIndies_Tiles", new Rectangle(1088, 105, 35, 35), tokenAnchor));
            addGraphic(new GraphicSimple("command_build_town", "PassageIndies_Tiles", new Rectangle(1125, 105, 35, 35), tokenAnchor));
            addGraphic(new GraphicSimple("command_build_mission", "PassageIndies_Tiles", new Rectangle(1088, 142, 35, 35), tokenAnchor));

            //addGraphic(new GraphicSimple("aztec_trader", "PassageIndies_Tiles", new Rectangle(341, 164, 14, 35), new Point(7, 35 - 3)));
            //addGraphic(new GraphicSimple("mouse_pointer", "PassageIndies_Tiles", new Rectangle(0, 160, 27, 27)));
            addGraphic(new GraphicSimple("mouse_pointer", "PassageIndies_Tiles", new Rectangle(847, 616, 27, 35)));
            addGraphic(new GraphicSimple("food", "PassageIndies_Tiles", new Rectangle(277, 113, 49, 24)));
            //addGraphic(new GraphicSimple("days", "PassageIndies_Tiles", new Rectangle(350, 111, 30, 31)));
            //addGraphic(new GraphicSimple("days", "ItNW_Trade", new Rectangle(317, 85, 40, 40)));
            addGraphic(new GraphicSimple("days", "ItNW_Trade", new Rectangle(359, 85, 40, 40)));
            addGraphic(new GraphicSimple("recognition", "ItNW_Trade", new Rectangle(233, 85, 40, 40)));
            //addGraphic(new GraphicSimple("fame", "PassageIndies_Tiles", new Rectangle(322, 82, 24, 23)));
            addGraphic(new GraphicSimple("discovery", "ItNW_Trade", new Rectangle(191, 85, 40, 40)));
            addGraphic(new GraphicSimple("relations", "ItNW_Trade", new Rectangle(275, 85, 40, 40)));
            addGraphic(new GraphicSimple("hex", "PassageIndies_Tiles", new Rectangle(360, 337, 87, 100)));
            addGraphic(new GraphicSimple("square", "PassageIndies_Tiles", 7, 5, rectTile));
            addGraphic(new GraphicSimple("poi_frame", "PassageIndies_Tiles", new Rectangle(505, 256, 71, 71), new Point(0 - 2, 71 + 2)));
            addGraphic(new GraphicSimple("poi_riversource", "PassageIndies_Tiles", new Rectangle(663, 258, 69, 69), new Point(0 - 3, 69 + 3)));
            addGraphic(new GraphicSimple("poi_riverdelta", "PassageIndies_Tiles", new Rectangle(734, 258, 69, 69), new Point(0 - 3, 69 + 3)));
            addGraphic(new GraphicSimple("poi_tallpeak", "PassageIndies_Tiles", new Rectangle(805, 258, 69, 69), new Point(0 - 3, 69 + 3)));
            //addGraphic(new GraphicSimple("parked_boats", "PassageIndies_Tiles", 8, 6, rectTile, new Point(rectTile.Width / 2, rectTile.Height / 2)));
            //List<Frame> frames = new List<Frame>();
            //frames.Add(new Frame(7, 10, rectTile, new Point(rectTile.Width / 2, rectTile.Height / 2), 500));
            //frames.Add(new Frame(8, 6, rectTile, new Point(rectTile.Width / 2, rectTile.Height / 2), 500));
            //frames.Add(new Frame(7, 11, rectTile, new Point(rectTile.Width / 2, rectTile.Height / 2), 500));
            //List<Animation> animations = new List<Animation>();
            //animations.Add(new Animation(new List<Condition>(), Animation.Behavior.RandomDifferent, frames));
            //Graphic gParkedBoats = new Graphic("parked_boats", "PassageIndies_Tiles", animations);
            //addGraphic(gParkedBoats);
            addGraphic(new GraphicSimple("parked_boats", "PassageIndies_Tiles", new Rectangle(1051, 216, 35, 35), tokenAnchor));
            addGraphic(new GraphicSimple("sailing_boats", "PassageIndies_Tiles", new Rectangle(1088, 216, 35, 35), tokenAnchor));

            //Point commandAnchor = new Point(35, 35);
            //addGraphic(new GraphicSimple("command", "PassageIndies_Tiles", new Rectangle(1051, 485, 70, 70), commandAnchor));
            Point commandAnchor = new Point(18, 18);
            addGraphic(new GraphicSimple("command", "PassageIndies_Tiles", new Rectangle(829, 216, 37, 37), commandAnchor));

            addGraphic(new GraphicSimple("parchment", "PassageIndies_Tiles", new Rectangle(964, 280, 200, 200)));

            // Trade window graphics...
            // TODO: Should this not be elsewhere?
            addGraphic(new GraphicSimple("trade_background", "ItNW_Trade", new Rectangle(14, 105, 767, 468)));
            addGraphic(new GraphicSimple("trade_ltr_arrow_tail", "ItNW_Trade", new Rectangle(20, 1, 2, 40)));
            addGraphic(new GraphicSimple("trade_ltr_arrow_body", "ItNW_Trade", new Rectangle(24, 1, 100, 40)));
            addGraphic(new GraphicSimple("trade_ltr_arrow_head", "ItNW_Trade", new Rectangle(126, 1, 21, 40)));
            addGraphic(new GraphicSimple("trade_rtl_arrow_tail", "ItNW_Trade", new Rectangle(126, 43, 2, 40)));
            addGraphic(new GraphicSimple("trade_rtl_arrow_body", "ItNW_Trade", new Rectangle(24, 43, 100, 40)));
            addGraphic(new GraphicSimple("trade_rtl_arrow_head", "ItNW_Trade", new Rectangle(1, 43, 21, 40)));
            addGraphic(new GraphicSimple("trade_gold", "ItNW_Trade", new Rectangle(191, 1, 40, 40)));
            addGraphic(new GraphicSimple("trade_food", "ItNW_Trade", new Rectangle(233, 1, 40, 40)));
            addGraphic(new GraphicSimple("trade_ships", "ItNW_Trade", new Rectangle(275, 1, 40, 40)));
            addGraphic(new GraphicSimple("trade_oldworld_goods", "ItNW_Trade", new Rectangle(149, 43, 40, 40)));
            addGraphic(new GraphicSimple("trade_newworld_goods", "ItNW_Trade", new Rectangle(191, 43, 40, 40)));
            addGraphic(new GraphicSimple("trade_men", "ItNW_Trade", new Rectangle(233, 43, 40, 40)));
            addGraphic(new GraphicSimple("trade_weapons", "ItNW_Trade", new Rectangle(275, 43, 40, 40)));
            addGraphic(new GraphicSimple("trade_horses", "ItNW_Trade", new Rectangle(317, 1, 40, 40)));
            addGraphic(new GraphicSimple("trade_onepixel", "ItNW_Trade", new Rectangle(1, 105, 1, 1)));
            addGraphic(new GraphicSimple("trade_locked", "ItNW_Trade", new Rectangle(41, 85, 18, 18)));
            addGraphic(new GraphicSimple("trade_desired", "ItNW_Trade", new Rectangle(61, 85, 18, 18)));
            addGraphic(new GraphicSimple("trade_unlocked", "ItNW_Trade", new Rectangle(81, 85, 18, 18)));
            addGraphic(new GraphicSimple("trade_upgreen", "ItNW_Trade", new Rectangle(101, 85, 22, 40)));
            addGraphic(new GraphicSimple("trade_downred", "ItNW_Trade", new Rectangle(125, 85, 22, 40)));

            addGraphic(new GraphicSimple("window_NW", "ItNW_Trade", new Rectangle(1, 108, 20, 20)));
            addGraphic(new GraphicSimple("window_NE", "ItNW_Trade", new Rectangle(45, 108, 20, 20)));
            addGraphic(new GraphicSimple("window_SE", "ItNW_Trade", new Rectangle(45, 152, 20, 20)));
            addGraphic(new GraphicSimple("window_SW", "ItNW_Trade", new Rectangle(1, 152, 20, 20)));
            addGraphic(new GraphicSimple("window_N", "ItNW_Trade", new Rectangle(23, 108, 20, 7)));
            addGraphic(new GraphicSimple("window_E", "ItNW_Trade", new Rectangle(58, 130, 7, 20)));
            addGraphic(new GraphicSimple("window_S", "ItNW_Trade", new Rectangle(23, 165, 20, 7)));
            addGraphic(new GraphicSimple("window_W", "ItNW_Trade", new Rectangle(1, 130, 7, 20)));

            addGraphic(new GraphicSimple("small_square", "SmallSquare", new Rectangle(0, 0, 2, 2)));

            //Graphic[] graphics = this.dictGraphics.Values.ToArray();
            //FileStream fs = File.Open("bfy.xml", FileMode.Create);
            //XmlSerializer xs = new XmlSerializer(typeof(GraphicSimple));
            //foreach (Graphic graphic in graphics)
            //    xs.Serialize(fs, graphic);
            //fs.Close();
        }

        private void initTerrain()
        {
            _dictTerrainByMapCharacter.Add('O', new Terrain("ocean", Terrain.TerrainMajorType.Ocean, Terrain.TerrainMinorType.Ocean, 1.5f, 0.0f, 2, 0));
            _dictTerrainByMapCharacter.Add('P', new Terrain("plains", Terrain.TerrainMajorType.Plains, Terrain.TerrainMinorType.Plains, 1.0f, 1.0f, 2, 0));
            _dictTerrainByMapCharacter.Add('F', new Terrain("forest", Terrain.TerrainMajorType.Forest, Terrain.TerrainMinorType.Forest, 0.5f, 1.5f, 1, 1));
            _dictTerrainByMapCharacter.Add('D', new Terrain("desert", Terrain.TerrainMajorType.Desert, Terrain.TerrainMinorType.Desert, 0.75f, 0.25f, 2, 0));
            _dictTerrainByMapCharacter.Add('M', new Terrain("mountains", Terrain.TerrainMajorType.Mountains, Terrain.TerrainMinorType.Mountain, 0.25f, 0.75f, 3, 3)); // same vis as hills, but higher...
            _dictTerrainByMapCharacter.Add('H', new Terrain("hills", Terrain.TerrainMajorType.Hills, Terrain.TerrainMinorType.Hills, 0.5f, 1.25f, 3, 2));
            _dictTerrainByMapCharacter.Add('E', new Terrain("evergreen_forest", Terrain.TerrainMajorType.Forest, Terrain.TerrainMinorType.EvergreenForest, 0.5f, 1.5f, 1, 1));
            _dictTerrainByMapCharacter.Add('S', new Terrain("swamp", Terrain.TerrainMajorType.Swamp, Terrain.TerrainMinorType.Swamp, 0.3f, 1.25f, 1, 1));
            _dictTerrainByMapCharacter.Add('J', new Terrain("tropical_forest", Terrain.TerrainMajorType.Forest, Terrain.TerrainMinorType.TropicalForest, 0.4f, 1.5f, 1, 1));
            _dictTerrainByMapCharacter.Add('A', new Terrain("savanna", Terrain.TerrainMajorType.Plains, Terrain.TerrainMinorType.Savanna, 1.0f, 1.5f, 2, 0));
            _dictTerrainByMapCharacter.Add('T', new Terrain("tundra", Terrain.TerrainMajorType.Plains, Terrain.TerrainMinorType.Tundra, 1.0f, 0.25f, 2, 0));
        }

        private void initWindowDecorationsAndBackground()
        {
            _windowDecorations = new WindowDecorations();
            _windowDecorations.cornerNW = IntoTheNewWorld.Instance.dictGraphics["window_NW"];
            _windowDecorations.cornerNE = IntoTheNewWorld.Instance.dictGraphics["window_NE"];
            _windowDecorations.cornerSE = IntoTheNewWorld.Instance.dictGraphics["window_SE"];
            _windowDecorations.cornerSW = IntoTheNewWorld.Instance.dictGraphics["window_SW"];
            _windowDecorations.sideN = IntoTheNewWorld.Instance.dictGraphics["window_N"];
            _windowDecorations.sideE = IntoTheNewWorld.Instance.dictGraphics["window_E"];
            _windowDecorations.sideS = IntoTheNewWorld.Instance.dictGraphics["window_S"];
            _windowDecorations.sideW = IntoTheNewWorld.Instance.dictGraphics["window_W"];

            _windowBackground = new WindowBackground();
            _windowBackground.background = IntoTheNewWorld.Instance.dictGraphics["parchment"];
            _windowBackground.style = WindowBackground.Style.Tiled;
        }

        private MapTile[,] buildWorld(int worldWidth, int worldHeight, out Vector2 v2GHighestPoint)
        {
            this.world = new WorldModel(worldWidth, worldHeight, new Rectangle(-150, 80, 120, 160), 23.5f, 365);

            // TODO: Eventually have parameters for size, islands vs. continents, etc.

            this.v2GPlayerStart = new Vector2(worldWidth - 1, worldHeight / 4);
            WorldBuilder wb = new WorldBuilder(worldWidth, worldHeight);
            float[,] heightMap = wb.generateHeightMap(out this.mapSeed);
            MapTile[,] tiles = new MapTile[worldHeight, worldWidth];
            float highestPoint = float.MinValue;
            v2GHighestPoint = new Vector2(0, 0);
            for (int row = 0; row < worldHeight; row++)
                for (int column = 0; column < worldWidth; column++)
                {
                    if (heightMap[row, column] > highestPoint)
                    {
                        highestPoint = heightMap[row, column];
                        v2GHighestPoint = new Vector2(column, row);
                    }

#if false
                    bool polar = ((row < (worldHeight * 0.125)) || (row > (worldHeight * 0.875)));
                    bool temperate = (!polar && ((row < (worldHeight * 0.4)) || (row > (worldHeight * 0.6))));
                    bool tropical = (!polar && !temperate);
#else
                    WorldModel.Zone zone = this.world.getZone(column, row);
                    bool polar = (zone == WorldModel.Zone.Polar);
                    bool temperate = (zone == WorldModel.Zone.Temperate);
                    bool tropical = (zone == WorldModel.Zone.Tropical);
#endif

                    Terrain terrain;
                    if (heightMap[row, column] > 0.6)
                        terrain = _dictTerrainByMapCharacter['M'];
                    else if (heightMap[row, column] > 0.4)
                        terrain = _dictTerrainByMapCharacter['H'];
                    else if (heightMap[row, column] > 0.2)
                    {
                        if (polar)
                            terrain = _dictTerrainByMapCharacter['E'];
                        else if (temperate)
                            terrain = _dictTerrainByMapCharacter['F'];
                        else
                            terrain = _dictTerrainByMapCharacter['J'];
                    }
                    else if (heightMap[row, column] > 0.01)
                    {
                        if (polar)
                            terrain = _dictTerrainByMapCharacter['T'];
                        else if (temperate)
                            terrain = _dictTerrainByMapCharacter['P'];
                        else
                            terrain = _dictTerrainByMapCharacter['A'];
                    }
                    else if (heightMap[row, column] == 0.0f)
                        terrain = _dictTerrainByMapCharacter['O'];
                    else
                    {
                        if (polar)
                            terrain = _dictTerrainByMapCharacter['S']; // TODO: Do wet taiga.
                        else if (temperate)
                            terrain = _dictTerrainByMapCharacter['S'];
                        else
                            terrain = _dictTerrainByMapCharacter['S']; // TODO: Do mangrove swamp.
                    }

                    MapTile mapTile = new MapTile();
                    mapTile.terrain = terrain;

                    tiles[row, column] = mapTile;
                }

            // Density and etch coastline
            for (int row = 0; row < worldHeight; row++)
                for (int column = 0; column < worldWidth; column++)
                {
                    tiles[row, column].density = getDensity(row, column, tiles);
                    etchCoastline(row, column, tiles);
                }

            // Etch rivers
            List<List<Point>> rivers2 = wb.getRivers(heightMap);
            this.rivers = new List<River>();
            if (rivers2 != null)
            {
                foreach (List<Point> river2 in rivers2)
                {
                    River river = new River(river2);
                    river.etchToMap(tiles);
                    rivers.Add(river);
                }
            }

            return tiles;
        }

        private void setActions(GamesLibrary.Action[] actions)
        {
            this.actionsByIdentifier = new Dictionary<string, GamesLibrary.Action>();

            foreach (GamesLibrary.Action action in actions)
                this.actionsByIdentifier.Add(action.identifier, action);
        }

        public override void Initialize()
        {
            float wcpt = this.worldCoordinatesPerMile * this.milesPerTile;
#if HEX
            this.v2WorldCoordinatesPerTile = new Vector2((float)(87.0f / 75.0f) * wcpt, wcpt);
#else
            this.v2WorldCoordinatesPerTile = new Vector2(wcpt);
#endif

            // Load the tile graphics
            loadTiles();

            // Initialize the terrain characteristics
            initTerrain();

            // TODO: After the world is built, or as it is built, try to figure out contiguous
            //       areas via A* flood fill.  Basically from every seed node, if no region assigned,
            //       flood fill and assign each MapNode a region.  Also accumulate a count per region
            //       so that NPC explorers will know when they've discovered entirely a region and will
            //       be able to hop on their boat and sail to another.  Region identification will be
            //       the trigger for the AI knowing to seek out the boat (when regions are different)
            //       or not (when they are the same) when doing a moveTo.

            // Build the random world
            int worldWidth = 120;
            int worldHeight = 80;
            worldWidth = 60;
            worldHeight = 40;
            Vector2 v2GHighestPoint;
            MapTile[,] tiles = buildWorld(worldWidth, worldHeight, out v2GHighestPoint);

            float sideLen = (float)(50.0f / 75.0f) * wcpt;
#if HEX
            _map = new MapHex<MapTile>(tiles, sideLen, this.v2WorldCoordinatesPerTile, this);
#else
            _map = new MapSquare<MapTile>(tiles, true, this.v2WorldCoordinatesPerTile, this);
#endif

            // Initialize the actions
            setActions(new GamesLibrary.Action[] { 
                new AttackAction(), 
                new BoardBoatAction(),
                new BuildFortAction(), 
                new BuildMissionAction(), 
                new BuildSettlementAction(), 
                new BuildTradingPostAction(),
                //new ConvertAction(),
                //new DemandTributeAction(),
                new DiscoverAction(),
                new ExploreAction(),
                new FleeAction(),
                new FollowAction(),
                //new GiftAction(),
                new LeaveBoatAction(),
                //new LootAction(),
                //new JoinAction(),
                //new JournalAction(),
                //new MoveToAction(),
                new PatrolAction(),
                //new RazeAction(),
                //new ResupplyAction(),
                new ReturnToEuropeAction(),
#if false
                new SailAction(),
#endif
                //new SpawnExplorerAction(),
                //new SpawnMissionaryAction(),
                //new SpawnSettlerAction(),
                //new SpawnTraderAction(),
                //new SpawnWarriorAction(),
                //new SplitAction(),
                //new SpreadNewsAction(),
                new TradeAction(), 
                new WanderAction()
            });

            // Initialize the player 
            players = new List<Explorer>();
            players.Add(new Explorer("Expedition", _map.gridToWorld(v2GPlayerStart), IntoTheNewWorld.milesPerMillisecond * this.worldCoordinatesPerMile));
            players[0].update(_map, 0); // TODO: Ugh, better way?!
            int men = 200;
            int food = weeksToTotalFood(30, men);
            int horses = men * 2;
            int weapons = men * 2;
            players[0].state.setValue("men", men);
            players[0].state.setValue("food", food);
            players[0].state.setValue("horses", horses);
            players[0].state.setValue("weapons", weapons);
            players[0].state.setValue("ships", 3);
            players[0].state.setValue("oldworld_goods", 3000);

#if false
            players[0].state.setValue("native_like", 0);
#endif

#if false
            MapHex<float> wbMap = new MapHex<float>(heightMap, sideLen, this.v2WorldCoordinatesPerTile, this);
            WorldBuilderWindow wbWindow = new WorldBuilderWindow(wbMap, rectTileSafeArea, _tilesize, players[0].pos, 0.0f);
            wbWindow.scrollSpeed = 10;
            wbWindow.allowScaling = true;
            wbWindow.scaleMin = 0.1f;
            wbWindow.scaleMax = 0.1f;
            //Show(wbWindow);
#endif

            // Add PoIs
            PoIs = new List<PointOfInterest>();
            //int riverSegmentLen = 5;
            //foreach (River river in this.rivers)
            //{
            //    if (river.length < riverSegmentLen)
            //        continue;

            //    int points = (river.length / riverSegmentLen) * 50;

            //    // TODO: Put PoI on edge correctly
            //    PoIs.Add(new PointOfInterest(dictGraphics["poi_riversource"], _map.gridToWorld(river.v2SourceLocation), points));
            //    PoIs.Add(new PointOfInterest(dictGraphics["poi_riverdelta"], _map.gridToWorld(river.v2DeltaLocation), points));
            //}
            if (this.rivers.Count > 0)
            {
                River longestRiver = this.rivers[0];
                foreach (River river in this.rivers)
                {
                    if (river.length <= longestRiver.length)
                        continue;

                    longestRiver = river;
                }
                int riverSegmentLen = 5;
                int points = ((longestRiver.length / riverSegmentLen) + 1) * 50;
                // TODO: Put PoI on edge correctly
                //PoIs.Add(new PointOfInterest(dictGraphics["poi_riversource"], _map.gridToWorld(longestRiver.v2SourceLocation), points));
                Vector2 v2EdgeW = _map.getSideW(new MapGrid<MapTile>.RowColumn((int)longestRiver.v2SourceLocation.Y, (int)longestRiver.v2SourceLocation.X), longestRiver.sourceSide);
                PoIs.Add(new PointOfInterest(v2EdgeW, points, "You have discovered the source of the longest river!", "source of the longest river"));
                // TODO: Put PoI on edge correctly
                //PoIs.Add(new PointOfInterest(dictGraphics["poi_riverdelta"], _map.gridToWorld(longestRiver.v2DeltaLocation), points));
                v2EdgeW = _map.getSideW(new MapGrid<MapTile>.RowColumn((int)longestRiver.v2DeltaLocation.Y, (int)longestRiver.v2DeltaLocation.X), longestRiver.deltaSide);
                PoIs.Add(new PointOfInterest(v2EdgeW, points, "You have discovered the delta of the longest river!", "delta of the longest river"));
            }

            //PoIs.Add(new PointOfInterest(dictGraphics["poi_tallpeak"], _map.gridToWorld(v2GHighestPoint), 100));
            PoIs.Add(new PointOfInterest(_map.gridToWorld(v2GHighestPoint), 100, "You have discovered the highest peak!", "highest peak"));

            // TODO: Remove this to get the PoIs back...
            //PoIs.Clear();

            // Initialize some cities (test only)

            // Test only, intialize some NPCs
            NPCs = new List<Mob>();

            // Put out an Explorer...
            float speed = IntoTheNewWorld.milesPerMillisecond * this.worldCoordinatesPerMile;
            explorers = new List<Explorer>();
            //explorers.Add(new Explorer("Juan", _map.gridToWorld(getLandTile(tiles, this.rnd)), speed));
            //explorers.Add(new Explorer("John", _map.gridToWorld(getLandTile(tiles, this.rnd)), speed));
            //explorers.Add(new Explorer("Jean", _map.gridToWorld(getLandTile(tiles, this.rnd)), speed));

            cities = new List<City>();
            int numCities = (int)((float)_map.width * (float)_map.height * 0.20 * 0.01); // TODO:AA: Was 50.
            for (int i = 0; i < numCities; i++)
            {
                Vector2 v2GPos = getLandTile(tiles, this.rnd);
                Vector2 v2WPos = _map.gridToWorld(v2GPos);
                //if ((i % 2) == 0)
                //    NPCs.Add(new Warrior("native_warrior_" + i, v2WPos, this.milesPerMillisecond * this.worldCoordinatesPerMile));
                //else
                //    NPCs.Add(new Trader("native_trader_" + i, v2WPos, this.milesPerMillisecond * this.worldCoordinatesPerMile));
                City city = new City(v2WPos, this.rnd.Next(1, 9));
                cities.Add(city);
                MapTile mt = _map.getTile(v2GPos);
                mt.addMapObject(city);
            }

            this.caches = new List<IntoTheNewWorldCache>();
            this.parkedShips = new List<Ship>();

            Vector2 v2GEurope = new Vector2(v2GPlayerStart.X, v2GPlayerStart.Y - 1);
            _map.getTile(v2GEurope).terrain = _dictTerrainByMapCharacter['P'];
            etchCoastline((int)v2GEurope.Y, (int)v2GEurope.X, tiles);
            this.europe = new Europe("europe", _map.gridToWorld(new Vector2(v2GPlayerStart.X, v2GPlayerStart.Y - 1)));
        }

        private Vector2 getLandTile(MapTile[,] tiles, System.Random rnd)
        {
            bool valid = false;
            int x = 0;
            int y = 0;
            while (!valid)
            {
                x = rnd.Next(tiles.GetLength(1));
                y = rnd.Next(tiles.GetLength(0));
                if (tiles[y, x].terrain == _dictTerrainByMapCharacter['O'])
                    continue;

                valid = true;
            }

            return new Vector2(x, y);
        }

        public override void LoadContent()
        {
            // Load textures
            this.loadTexture("PassageIndies_Tiles", @"PassageIndies_Tiles");
            this.loadTexture("ItNW_Terrain", @"ItNW_Terrain");
            this.loadTexture("ItNW_Trade", @"ItNW_Trade");

            // Load fonts
        }

        public IntoTheNewWorldMapWindow mapWindow = null; // TODO: Better place for this?

        /// <summary>
        /// Called after loading of the content, last thing done before the game transitions to the 
        /// handle input / draw loop.
        /// </summary>
        public override void PostLoadContent()
        {
            mapWindow = new IntoTheNewWorldMapWindow(_map, this.rectTileSafeArea, _tilesizeA, this.players[0].positionWorld);
            mapWindow.scrollSpeed = 10;
            mapWindow.allowScaling = false; // TODO:AA: Was true.
            mapWindow.allowRotation = false; // TODO:AA: Was true.
            mapWindow.scaleMin = 0.1f;
            mapWindow.scaleMax = 3.0f;
            mapWindow.drawGrid = false;
            Show(mapWindow);

            mapWindow.showMessageBox("Welcome to Into the New World!\n\nSail forth and discover the New World!\nDiscover as much as possible and return\nto Europe to end the game, or die\nin the attempt...");
        }

        public override void UnloadContent()
        {
        }

        public override void UpdateGameState(GameTime gameTime)
        {
            // If the player has not moved then don't advance time.
            // TODO: Exception will be when we have commands that allow advance of time w/o moving, like camping.
            if ((players[0].positionWorld == oldPlayerPos) && !this.forceUpdateGameState)
                return;
            this.forceUpdateGameState = false;

            // Current position is now our old position.
            oldPlayerPos = players[0].positionWorld;

            double milliseconds = gameTime.ElapsedGameTime.TotalMilliseconds;

            //// Get the current map node, tile, and terrain information.
            //Map.MapNode mn = _map.getMapNode(oldPlayerPos);
            //MapTile tile = _map.getTile(mn);
            //Terrain terrain = tile.terrain;

            // Advance the calendar, if necessary, and consume food.
            //elapsed += gameTime.ElapsedGameTime.Milliseconds;
            elapsed += (int)milliseconds;
            int olddays = days;
            days = elapsed / msperday;
            if (olddays != days)
            {
                gameState.setValue("day_of_year", (days % 365));
                //gameState.setValue("day", days);
                gameState.setValue("turn", days);

                date = date.AddDays(1);

                IntoTheNewWorld.Instance.players[0].eat();
                foreach (Mob NPC in this.NPCs)
                {
                    if (!(NPC is IntoTheNewWorldMob))
                        continue;
                    ((IntoTheNewWorldMob)NPC).eat();
                }
            }
            //if (players[0].state.getValue<int>("men") <= 0)
            //    playerState.dead = true;

            // Handle dead NPCs (due to lack of food or lack of men)...
            List<Mob> deadNPCs = this.NPCs.Where(mob => mob.isDead()).ToList();
            foreach (Mob deadNPC in deadNPCs)
            {
                // Create a cache object that can be traded with.
                IntoTheNewWorldCache loot = new IntoTheNewWorldCache(deadNPC.positionWorld, deadNPC, true);
                loot.state.merge(deadNPC.state, loot.getAcceptedItems().ToList());
                this.caches.Add(loot);
            }

            // Clean up empty caches...
            // NOTE: We do this after dead NPCs because deadNPCs may create caches that end up empty...
            this.caches = this.caches.Where(cache => !cache.isEmpty()).ToList();

            // Have the NPCs (natives, other explorers, etc.) do their AI.
            this.NPCs.Clear();
            foreach (City city in this.cities)
            {
                city.process(this.gameState, _map);
                this.NPCs.AddRange(city.units);
            }
            this.NPCs.AddRange(explorers.Where(mob => !mob.isDead()).Cast<Mob>());

            foreach (Mob mob in this.NPCs)
            {
                mob.process(this.gameState, _map);

                // TODO: Do update in process?
                mob.update(_map, milliseconds);
            }

            europe.process(this.gameState, _map);
        }

        protected override Graphic getMousePointerGraphic(Vector2 mousePos)
        {
            return dictGraphics["mouse_pointer"];
        }
        // Overridden methods above...

        private bool isEdgeBlocked<R>(R requester, Map.MapNode start, Map.MapNode end)
        {
            // TODO: Square grid!

            if (start == end)
                return false;

            if ((start == null) || (end == null))
                return true;

            MapTile mtStart = _map.getTile(start);
            MapTile mtEnd = _map.getTile(end);

            // If neither tile has borders then no worries, can't be blocked.
            // TODO: Right?
            if ((mtStart.borders == null) && (mtEnd.borders == null))
                return false;

            Vector2 v2Start = _map.getNodeGridPosition(start);
            Vector2 v2End = _map.getNodeGridPosition(end);
            Vector2 v2Diff = v2End - v2Start;
            Vector2 v2Target = Vector2.Zero;
            
            bool oddRow = ((v2Start.Y % 2) != 0);

            //s:east, e:west
            if (v2Diff == Vector2.UnitX)
            {
                if (isBlockingBorderType(mtStart.getBorder(Side.East)))
                    return true;
                if (isBlockingBorderType(mtEnd.getBorder(Side.West)))
                    return true;
            }
            //s:west, e:east
            if (v2Diff == -Vector2.UnitX)
            {
                if (isBlockingBorderType(mtStart.getBorder(Side.West)))
                    return true;
                if (isBlockingBorderType(mtEnd.getBorder(Side.East)))
                    return true;
            }
            //s:northwest, e:southeast
            if (oddRow)
                v2Target = -Vector2.UnitY;
            else
                v2Target = -Vector2.UnitY + -Vector2.UnitX;
            if (v2Diff == v2Target)
            {
                if (isBlockingBorderType(mtStart.getBorder(Side.NorthWest)))
                    return true;
                if (isBlockingBorderType(mtEnd.getBorder(Side.SouthEast)))
                    return true;
            }
            //s:northeast, e:southwest
            if (oddRow)
                v2Target = -Vector2.UnitY + Vector2.UnitX;
            else
                v2Target = -Vector2.UnitY;
            if (v2Diff == v2Target)
            {
                if (isBlockingBorderType(mtStart.getBorder(Side.NorthEast)))
                    return true;
                if (isBlockingBorderType(mtEnd.getBorder(Side.SouthWest)))
                    return true;
            }
            //s:southwest, e:northeast
            if (oddRow)
                v2Target = Vector2.UnitY;
            else
                v2Target = Vector2.UnitY + -Vector2.UnitX;
            if (v2Diff == v2Target)
            {
                if (isBlockingBorderType(mtStart.getBorder(Side.SouthWest)))
                    return true;
                if (isBlockingBorderType(mtEnd.getBorder(Side.NorthEast)))
                    return true;
            }
            //s:southeast, e:northwest
            if (oddRow)
                v2Target = Vector2.UnitY + Vector2.UnitX;
            else
                v2Target = Vector2.UnitY;
            if (v2Diff == v2Target)
            {
                if (isBlockingBorderType(mtStart.getBorder(Side.SouthEast)))
                    return true;
                if (isBlockingBorderType(mtEnd.getBorder(Side.NorthWest)))
                    return true;
            }

            return false;
        }

        private bool isBlockingBorderType(Graphic graphic)
        {
            if (graphic == null)
                return false;

            if (graphic.identifier == "river")
                return true;

            // Let the river source be walkable, else it's annoying.
            //if (graphic.identifier == "river_source")
            //    return true;

            return false;
        }

        private bool isWalkableOcean<R>(MapTile tile, R requester)
        {
            Explorer explorer = requester as Explorer;
            return ((explorer != null) && explorer.isOnBoat());
        }

        private bool canUseBoat<R>(Ship ship, R requester)
        {
            Explorer explorer = requester as Explorer;
            return ((explorer != null) && (explorer.isPlayer() || (ship.owner == explorer)));
        }

        public float getEdgeCost<R>(R requester, Map.MapNode start, Map.MapNode end)
        {
            MapTile tile = _map.getTile(end);
            Terrain terrain = tile.terrain;
            Terrain ocean = _dictTerrainByMapCharacter['O'];

#if false
            MapTile tileStart = _map.getTile(start);
            Terrain terrainStart = tileStart.terrain;

            if ((terrain == ocean) && !isWalkableOcean(tile, requester))
            {
                // Does the tile have a boat the requester can use?
                List<Ship> tileShips = this.parkedShips.Where(ship => canUseBoat(ship, requester) && (_map.getTile(_map.getMapNode(ship.positionWorld)) == tile)).ToList();
                if (tileShips.Count <= 0)
                    return float.MaxValue;
            }
#else
            if ((terrain == ocean) && !isWalkableOcean(tile, requester))
                return float.MaxValue;
#endif

            if (terrain != ocean)
            {
                Explorer explorer = requester as Explorer;
                if ((explorer != null) && explorer.isOnBoat())
                    return float.MaxValue;
            }

            // Check edge blocks (e.g., rivers).
            if (isEdgeBlocked(requester, start, end))
                return float.MaxValue;

            return 1.0f;
        }

        public float getNodeCost<R>(R requester, Map.MapNode node)
        {
            MapTile tile = _map.getTile(node);
            Terrain terrain = tile.terrain;

            if (terrain.moveModifier == 0.0f)
                return float.MaxValue;

            return 2.0f - terrain.moveModifier; // TODO: This is WRONG!  Since low move modifiers are DIFFICULT terrain in this game we want the inverse, so the cost is high.  For now do a simple 2-x.
        }

        public float getCost<R>(R requester, Map.MapNode start, Map.MapNode end)
        {
            float edgeCost = getEdgeCost(requester, start, end);
            if (edgeCost == float.MaxValue)
                return float.MaxValue;

            float nodeCost = getNodeCost(requester, end);
            if (nodeCost == float.MaxValue)
                return float.MaxValue;

            return edgeCost + nodeCost;
        }

#if false
        public float getCost<R>(R requester, Map.MapNode start, Map.MapNode end)
        {
            MapTile tile = _map.getTile(end);
            Terrain terrain = tile.terrain;
            Terrain ocean = _dictTerrainByMapCharacter['O'];

            MapTile tileStart = _map.getTile(start);
            Terrain terrainStart = tileStart.terrain;

            if ((terrain == ocean) && !isWalkableOcean(tile, requester))
            {
                // Does the tile have a boat the requester can use?
                List<Ship> tileShips = this.parkedShips.Where(ship => canUseBoat(ship, requester) && (_map.getTile(_map.getMapNode(ship.positionWorld)) == tile)).ToList();
                if (tileShips.Count <= 0)
                    return float.MaxValue;
            }

            if (terrain != ocean)
            {
                Explorer explorer = requester as Explorer;
                if ((explorer != null) && explorer.isOnBoat())
                    return float.MaxValue;
            }

            if (terrain.moveModifier == 0.0f)
                return float.MaxValue;

            // Check edge blocks (e.g., rivers).
            if (isEdgeBlocked(requester, start, end))
                return float.MaxValue;

            //return 1.0f;
            return terrain.moveModifier;
        }
#endif

        public float getMovementModifier<R>(R requester, Map.MapNode node)
        {
            MapTile tile = _map.getTile(node);
            Terrain terrain = tile.terrain;
            Terrain ocean = _dictTerrainByMapCharacter['O'];

            if ((terrain == ocean) && !isWalkableOcean(tile, requester))
                return 0.0f;

            if (terrain != ocean)
            {
                Explorer explorer = requester as Explorer;
                if ((explorer != null) && explorer.isOnBoat())
                    return 0.0f;
            }

            return terrain.moveModifier;
        }

        public int getOpacity(Map.MapNode node)
        {
            return _map.getTile(node).terrain.height;
        }

        // TODO: Handle better later, like by having all state in one object I can just poof away.
        // TODO: This is broken now.
        internal void reset()
        {
            Map.MapNode oldMapNode = _map.getMapNode(players[0].positionWorld);

            days = 0;
            date = new DateTime(1492, 8, 3); 
            elapsed = 0;
            oldPlayerPos = Vector2.Zero;
            players[0].position(_map.gridToWorld(v2GPlayerStart));

            Map.MapNode newMapNode = _map.getMapNode(players[0].positionWorld);

            players[0].mapNodeChanged(oldMapNode, newMapNode);
        }

        private bool validateY(float y, Dictionary<float, List<int>> dictYs, int ycol)
        {
            List<int> cols;
            if (dictYs.TryGetValue(y, out cols))
            {
                foreach (int col in cols)
                {
                    if (Math.Abs(col - ycol) > 1)
                        continue;

                    return false;
                }
            }
            else
            {
                cols = new List<int>();
                dictYs.Add(y, cols);
            }
            cols.Add(ycol);

            return true;
        }

        private int getDensity(int row, int col, MapTile[,] mapTiles)
        {
#if OLD
            int density = 0;

            Terrain terrain = mapTiles[row, col].terrain;
            for (int r = row - 1; r <= row + 1; r++)
                for (int c = col - 1; c <= col + 1; c++)
                {
                    // Don't count ourselves.
                    if ((r == row) && (c == col))
                        continue;

                    if ((r < 0) || (c < 0) ||
                        (r >= mapTiles.GetLength(0)) || (c >= mapTiles.GetLength(1)))
                        continue;

                    Terrain terrainCheck = mapTiles[r, c].terrain;
                    if (terrainCheck != terrain)
                        continue;

                    density++;
                }

            return density;
#endif
            List<MapHex<Terrain>.RowColumn> connectedRowColumns = MapHex<Terrain>.getConnectedRowColumns(row, col, mapTiles.GetLength(0), mapTiles.GetLength(1));
            return connectedRowColumns.Count(i => (mapTiles[i.row, i.column].terrain.majorType == mapTiles[row, col].terrain.majorType));
        }

        private void etchCoastline(int row, int col, MapTile[,] mapTiles)
        {
            Terrain ocean = _dictTerrainByMapCharacter['O'];
            MapTile mt = mapTiles[row, col];

            if (mt.terrain == ocean)
                return;

            Graphic gCoastline = IntoTheNewWorld.Instance.dictGraphics["coastline"];
            bool oddRow = ((row % 2) != 0);
            int height = mapTiles.GetLength(0);
            int width = mapTiles.GetLength(1);

            //west
            if (col > 0)
            {
                if (mapTiles[row, col - 1].terrain == ocean)
                    mt.addBorder(Side.West, gCoastline);
            }
            //east
            if (col < (width - 1))
            {
                if (mapTiles[row, col + 1].terrain == ocean)
                    mt.addBorder(Side.East, gCoastline);
            }
            //northwest
            if (row > 0)
            {
                bool adjOcean = false;
                if (oddRow)
                    adjOcean = (mapTiles[row - 1, col].terrain == ocean);
                else
                {
                    if (col > 0)
                        adjOcean = (mapTiles[row - 1, col - 1].terrain == ocean);
                }
                if (adjOcean)
                    mt.addBorder(Side.NorthWest, gCoastline);
            }
            //northeast
            if (row > 0)
            {
                bool adjOcean = false;
                if (oddRow)
                {
                    if (col < (width - 1))
                        adjOcean = (mapTiles[row - 1, col + 1].terrain == ocean);
                }
                else
                    adjOcean = (mapTiles[row - 1, col].terrain == ocean);
                if (adjOcean)
                    mt.addBorder(Side.NorthEast, gCoastline);
            }
            //southwest
            if (row < (height - 1))
            {
                bool adjOcean = false;
                if (oddRow)
                    adjOcean = (mapTiles[row + 1, col].terrain == ocean);
                else
                {
                    if (col > 0)
                        adjOcean = (mapTiles[row + 1, col - 1].terrain == ocean);
                }
                if (adjOcean)
                    mt.addBorder(Side.SouthWest, gCoastline);
            }
            //southeast
            if (row < (height - 1))
            {
                bool adjOcean = false;
                if (oddRow)
                {
                    if (col < (width - 1))
                        adjOcean = (mapTiles[row + 1, col + 1].terrain == ocean);
                }
                else
                    adjOcean = (mapTiles[row + 1, col].terrain == ocean);
                if (adjOcean)
                    mt.addBorder(Side.SouthEast, gCoastline);
            }

            // TODO: Handle SQUARE as well.
        }

        public int totalFoodToWeeks(int totalFood, int men)
        {
            if (men == 0)
                return 0;

            return (totalFood / (men * this.foodperday * 7));
        }

        public int weeksToTotalFood(int desiredWeeks, int men)
        {
            return (men * this.foodperday * 7 * desiredWeeks);
        }

        public string getScoreString()
        {
            int score = 0;
            int discovery = players[0].state.getValue<int>("discovery");
            int recognition = players[0].state.getValue<int>("recognition");
            int native_like = players[0].state.getValue<int>("native_like");
            int gold = players[0].state.getValue<int>("gold");
            int newworld_goods = players[0].state.getValue<int>("newworld_goods");

            string s = "\nDuring your journey you discovered:\n";
            if ((discovery + recognition) < 1000)
            {
                s += " * a small portion of the New World\n";
                score += 10;
            }
            else if ((discovery + recognition) < 2000)
            {
                s += " * a respectable portion of the New World\n";
                score += 20;
            }
            else if ((discovery + recognition) < 3000)
            {
                s += " * a large portion of the New World\n";
                score += 35;
            }
            else
            {
                s += " * most of the New World\n";
                score += 50;
            }

            foreach (PointOfInterest poi in this.PoIs)
            {
                if (poi.claimed)
                {
                    s += " * " + poi.subject + "\n";
                    score += 10;
                }
            }

            if (newworld_goods == 0)
                ;
            else if (newworld_goods < 2500)
            {
                s += " * some New World goods (" + newworld_goods + ")\n";
                score += 5;
            }
            else
            {
                s += " * many New World goods (" + newworld_goods + ")\n";
                score += 10;
            }

            s += "\n";

            if (recognition > 0)
            {
                s += "As you returned to Europe you received\nrecognition for these discoveries.\n";
                score += (score / 3);
            }
            else
                s += "As you failed to return to Europe your\nfate and efforts will not be recognized\nfor years to come.\n";

            s += "\n";

            if (recognition > 0)
            {
                if (native_like < -50)
                {
                    s += "The crown expresses its displeasure at your\ntreatment of the Natives.\n";
                    score += (native_like / 5);
                }
                else if (native_like > 50)
                {
                    s += "The crown commends you for treating the\nNatives with respect.\n";
                    score += (native_like / 5);
                }

                s += "The royal court is ";
                if (gold < 500)
                    s += "very unhappy";
                else if (gold < 1000)
                    s += "displeased";
                else if (gold < 2000)
                    s += "satisfied";
                else if (gold < 4000)
                    s += "pleased";
                else
                    s += "ecstatic";
                s += " with the\namount of gold (" + gold + ") you brought back with you.\n";
                score += (gold / 100);

                s += "\n";
            }

            string title = "";
            if (score < 40)
                title = "novice";
            else if (score < 80)
                title = "apprentice";
            else if (score < 120)
                title = "journeyman";
            else if (score < 160)
                title = "expert";
            else
                title = "master";

            s += "Your score of " + score + " ranks you as a\n" + title + " explorer.";

            return s;
        }

        public List<MapObject> getActionTargets(MapObject source)
        {
            List<MapObject> targets = new List<MapObject>();

            // TODO: This is crap, obviously -- we'll want a routine that goes through the visible nodes as well as tacks on
            //       fixed nodes (known cities, forts, trading posts, missions) and parked ships.
            targets.AddRange(this.caches.Cast<MapObject>());
            targets.AddRange(this.cities.Cast<MapObject>());
            targets.AddRange(this.NPCs.Where(npc => !((npc is Explorer) && ((Explorer)npc).isOnBoat())).Cast<MapObject>());
            targets.AddRange(this.parkedShips.Cast<MapObject>());
            targets.AddRange(this.players.Where(player => !player.isOnBoat()).Cast<MapObject>());
            targets.AddRange(this.PoIs.Cast<MapObject>());
            targets.Add(this.europe);

            // Exclude the source.
            targets = targets.Where(mo => mo != source).ToList();

            // Exclude anything that isn't visible or a landmark
            Mob mob = source as Mob;
            if (mob != null)
                targets = targets.Where(mo => mob.visibleMapNodes.Contains(_map.getMapNode(mo.positionWorld)) ||
                    (mob.notesLandmarks && mob.notedLandmarks.Contains(mo))).ToList();

            return targets;
        }
    }
}
