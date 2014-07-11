using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if Allow_XNA
using Microsoft.Xna.Framework;
#endif
using GamesLibrary;

namespace IntoTheNewWorld
{
    public class Explorer : IntoTheNewWorldMob
    {
        public Dictionary<Map.MapNode, bool> visited = new Dictionary<Map.MapNode, bool>();
        public Dictionary<Map.MapNode, bool> seen = new Dictionary<Map.MapNode, bool>();

        public HashSet<string> discoveredPOIs = new HashSet<string>();

        public Captain captain { get; set; }

        public Explorer(string text, Vector2 pos, float speed)
            : base(text, pos, speed) 
        {
            System.Random rnd = IntoTheNewWorld.Instance.rnd;
            int men = rnd.Next(100, 200);
            int food = IntoTheNewWorld.Instance.weeksToTotalFood(30, men);
            int horses = men * rnd.Next(1, 3);
            int weapons = men * rnd.Next(1, 3);
            int oldworld_goods = rnd.Next(1000, 5000);
            this.state.setValue("men", men);
            this.state.setValue("food", food);
            this.state.setValue("horses", horses);
            this.state.setValue("weapons", weapons);
            this.state.setValue("ships", 0);
            this.state.setValue("oldworld_goods", oldworld_goods);

#if false
            // TODO: Remove!
            int native_like = rnd.Next(-100, 100);
            this.state.setValue("native_like", native_like);
            this.state.setValue("native_like", -100); // TODO: REMOVE!
#endif

            // Set up the actions the Explorer can do.

            // attack, board boat, build fort, build mission, built settlement, build trading post,
            // demand tribute, discover, flee, follow, journal (player only), leave boat, move to,
            // raze, resupply, return to Europe, spread news, trade 
            this.sourceActions.Add(IntoTheNewWorld.Instance.actionsByIdentifier["attack"]);
            this.sourceActions.Add(IntoTheNewWorld.Instance.actionsByIdentifier["board_boat"]);
            this.sourceActions.Add(IntoTheNewWorld.Instance.actionsByIdentifier["build_fort"]);
            this.sourceActions.Add(IntoTheNewWorld.Instance.actionsByIdentifier["build_mission"]);
            this.sourceActions.Add(IntoTheNewWorld.Instance.actionsByIdentifier["build_settlement"]);
            this.sourceActions.Add(IntoTheNewWorld.Instance.actionsByIdentifier["build_tradingpost"]);
            this.sourceActions.Add(IntoTheNewWorld.Instance.actionsByIdentifier["discover"]);
            this.sourceActions.Add(IntoTheNewWorld.Instance.actionsByIdentifier["flee"]);
            this.sourceActions.Add(IntoTheNewWorld.Instance.actionsByIdentifier["follow"]);
            this.sourceActions.Add(IntoTheNewWorld.Instance.actionsByIdentifier["leave_boat"]);
            this.sourceActions.Add(IntoTheNewWorld.Instance.actionsByIdentifier["return_to_europe"]);
            //this.sourceActions.Add(IntoTheNewWorld.Instance.actionsByIdentifier["sail"]);
            this.sourceActions.Add(IntoTheNewWorld.Instance.actionsByIdentifier["trade"]);

            // attack, flee, follow, spread news, trade
            this.targetActions.Add(IntoTheNewWorld.Instance.actionsByIdentifier["attack"]);
            this.targetActions.Add(IntoTheNewWorld.Instance.actionsByIdentifier["flee"]);
            this.targetActions.Add(IntoTheNewWorld.Instance.actionsByIdentifier["follow"]);
            this.targetActions.Add(IntoTheNewWorld.Instance.actionsByIdentifier["trade"]);

            // explore
            this.defaultActions.Add(IntoTheNewWorld.Instance.actionsByIdentifier["explore"]);

            // Create a temporary, random Captain.
            // TODO: Eventually will do through better means!
            Captain captain = new Captain("random_captain", pos, speed, (Captain.ExpertiseArea)rnd.Next(4), Captain.ExpertiseLevel.Apprentice);
            //addCaptain(captain);
            this.captain = captain;
            captain.explorer = this;

            this.notesLandmarks = true;
        }

#if false
        public void addCaptain(Captain captain)
        {
            if (captain == null)
                return;

            captain.explorer = this;

            switch (captain.expertiseArea)
            {
                case Captain.ExpertiseArea.Military:
                    this.sourceActions.Add(IntoTheNewWorld.Instance.actionsByIdentifier["build_fort"]);
                    break;
                case Captain.ExpertiseArea.Religion:
                    this.sourceActions.Add(IntoTheNewWorld.Instance.actionsByIdentifier["build_mission"]);
                    break;
                case Captain.ExpertiseArea.Settlement:
                    this.sourceActions.Add(IntoTheNewWorld.Instance.actionsByIdentifier["build_settlement"]);
                    break;
                case Captain.ExpertiseArea.Trade:
                    this.sourceActions.Add(IntoTheNewWorld.Instance.actionsByIdentifier["build_tradingpost"]);
                    break;
                default:
                    break;
            }
        }
#endif

        protected override List<string> getNeeds<T>(VariableBundle gameState, MapGrid<T> map)
        {
            this.needs = base.getNeeds<T>(gameState, map);

            if (this.state.getValue<int>("discovery") > 0)
            {
                if (!this.needs.Contains("recognition"))
                    this.needs.Add("recognition");
            }

            return this.needs;
        }

        protected override List<string> getWants<T>(VariableBundle gameState, MapGrid<T> map)
        {
            this.wants = base.getWants(gameState, map);

            if (!this.wants.Contains("gold"))
                this.wants.Add("gold");
            if (!this.wants.Contains("discovery"))
                this.wants.Add("discovery");

            // Cash in discovery once it gets to be too much risk.
            // TODO: Eventually have level of risk vary by explorer.
            if (this.state.getValue<int>("discovery") > 0)
            {
                if (!this.wants.Contains("recognition"))
                    this.wants.Add("recognition");
            }

            return this.wants;
        }

        public override bool isPlayer()
        {
            return IntoTheNewWorld.Instance.players.Contains(this);
        }

        public override void mapNodeChanged(Map.MapNode oldMapNode, Map.MapNode newMapNode)
        {
            base.mapNodeChanged(oldMapNode, newMapNode);

            MapGrid<MapTile> map = IntoTheNewWorld.Instance._map;

            MapTile oldTile = (MapTile)(map.getTile(oldMapNode));
            MapTile newTile = map.getTile(newMapNode);

            Vector2 oldGridPosition = map.getNodeGridPosition(oldMapNode);
            Vector2 newGridPosition = map.getNodeGridPosition(newMapNode);

            Dictionary<string, Graphic> dictGraphics = IntoTheNewWorld.Instance.dictGraphics;

            // Potentially give seen, visited, and discovery credit to the visible nodes.
            foreach (Map.MapNode mnc in this.visibleMapNodes)
            {
                if (this.seen.ContainsKey(mnc))
                    continue;

                this.seen[mnc] = true;

                if (map.getTile(mnc).terrain.majorType != Terrain.TerrainMajorType.Ocean)
                {
                    this.state.adjustValue("discovery", 10);

                    WindowEffect effect = new WindowEffect();
                    effect.graphic = IntoTheNewWorld.Instance.dictGraphics["discovery"];
                    effect.msDuration = 1000;
                    effect.v2Start = IntoTheNewWorld.Instance.mapWindow.center;
                    effect.v2End = new Vector2(IntoTheNewWorld.Instance.mapWindow.bounds.Width, 0);

                    //IntoTheNewWorld.Instance.mapWindow.effects.Add(effect);
                }

                if (mnc != newMapNode)
                    continue;

                if (this.visited.ContainsKey(mnc))
                    continue;

                this.visited[mnc] = true;

                // Small chance of an undiscovered flora / fauna...
                if (IntoTheNewWorld.Instance.rnd.Next(0, 100) >= 5)
                    continue;

                // TODO:
            }
        }

        public override string getGraphicIdentifier()
        {
#if false
            // Flip the boat around depending on pointer facing.
            if ((gMapObject.identifier == "sailing_boats") && (mapObject.pos.X < screenToWorld(IntoTheNewWorld.Instance.mousePos).X) && IntoTheNewWorld.Instance.playerState.onBoat && (this.primaryFocusHotspot != mapObjectRendered))
                spriteEffects = SpriteEffects.FlipHorizontally;
            spriteEffects = SpriteEffects.None;
#endif

            // Add player graphic (could be on boat, on foot, or dead)
            if (isDead())
                return "dead";
            else if (isOnBoat())
                    return "sailing_boats";

            return "player_foot";
        }

        // TODO: This method should go away, commands instead derived from the available actions.
        protected override List<Command> getCommands()
        {
            List<Command> commands = new List<Command>();

            if (IntoTheNewWorld.Instance.players[0] != this)
            {
                commands.Add(new Command("command_trade", Command.Slot.A, this));
                commands.Add(new Command("command_fight", Command.Slot.X, this));
            }
            else if (!isDead())
            {
                if (isOnBoat())
                {
                    Vector2 v2PlayerPosG = IntoTheNewWorld.Instance._map.worldToGrid(IntoTheNewWorld.Instance.players[0].positionWorld);
                    Map.MapNode mapNode = IntoTheNewWorld.Instance._map.getMapNodeG(v2PlayerPosG);
                    List<Map.MapNode> connectedMapNodes = IntoTheNewWorld.Instance._map.getConnectedNodes(mapNode);

                    // TODO: Instead get walkability?
                    if (connectedMapNodes.Any(mapnode => IntoTheNewWorld.Instance._map.getTile(mapnode).terrain.majorType != Terrain.TerrainMajorType.Ocean))
                        commands.Add(new Command("command_disembark", Command.Slot.A, this));

#if false
                    if (v2PlayerPosG.X == (IntoTheNewWorld.Instance._map.width - 1))
                        commands.Add(new Command("command_europe", Command.Slot.A, this));
#endif
                }
                else
                    commands.Add(new Command("command_cache", Command.Slot.A, this));

                commands.Add(new Command("command_journal", Command.Slot.Y, this));
            }
            return commands;
        }

        public bool isOnBoat()
        {
            Vector2 v2PosG = IntoTheNewWorld.Instance._map.worldToGrid(this.positionWorld);
            Map.MapNode mapNode = IntoTheNewWorld.Instance._map.getMapNodeG(v2PosG);
            return (IntoTheNewWorld.Instance._map.getTile(mapNode).terrain.majorType == Terrain.TerrainMajorType.Ocean);
        }
    }
}
