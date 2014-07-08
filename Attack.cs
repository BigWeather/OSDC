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
    public class AttackAction : GamesLibrary.Action
    {
        public AttackAction()
        {
            this.identifier = "attack";
            this.requiresTarget = true;
            this.rangeMN = 1;
            this.generateNews = true;
            //this.goalsFulfilled = new string[] { "men", "food", "gold", "oldworld_goods", "newworld_goods", "horses", "weapons", "security" }.ToList();
            this.goalsFulfilled = new string[] { }.ToList();
            this.severity = 5;
            this.cooldown = 1;
        }

        public override void execute(MapObject source, MapObject target)
        {
            // TODO: Do center correctly!
            if (IntoTheNewWorld.Instance.isPlayer(source) || IntoTheNewWorld.Instance.isPlayer(target))
            {
                CombatWindow combatWindow = new CombatWindow(new Vector2(1280 / 2, 720 / 2), 0, source, target);
                combatWindow.background = IntoTheNewWorld.Instance.windowBackground;
                combatWindow.decorations = IntoTheNewWorld.Instance.windowDecorations;
                IntoTheNewWorld.Instance.Show(combatWindow);
            }
            else
            {
                string dummy;
                CombatWindow.resolveCombat(source, target, false, out dummy); // NOTE: false doesn't mean the defender won't flee, just that it's not automatic.
            }
        }
    }



    public class BoardBoatAction : GamesLibrary.Action
    {
        public BoardBoatAction()
        {
            this.identifier = "board_boat";
            this.requiresTarget = true;
            this.rangeMN = 1;
            this.generateNews = true;
            //this.goalsFulfilled = new string[] { "discovery", "recognition" }.ToList();
            //this.goalsFulfilled = new string[] { "recognition" }.ToList();
            this.goalsFulfilled = new string[] { }.ToList();
            this.severity = 0;
            this.cooldown = 1;
        }

        public override bool isPossible(MapObject source, MapObject target)
        {
            if (!base.isPossible(source, target))
                return false;

            Explorer explorer = source as Explorer;
            Ship ship = target as Ship;

            return ((ship != null) && ship.parked && (explorer != null) && !explorer.isOnBoat());
        }

        public override void execute(MapObject source, MapObject target)
        {
            Explorer explorer = source as Explorer;
            if (explorer == null)
                return;

            Ship ship = target as Ship;
            if (ship == null)
                return;

            // Remove the boat from the list of parked ships.
            IntoTheNewWorld.Instance.parkedShips.Remove(ship);

            // Note where the explorer came from.
            Map.MapNode explorerMapNode = IntoTheNewWorld.Instance._map.getMapNode(explorer.positionWorld);

            // Put the explorer on the boat.
            explorer.position(ship.positionWorld);

            // Initiate the map node change.
            Map.MapNode boatMapNode = IntoTheNewWorld.Instance._map.getMapNode(ship.positionWorld);
            explorer.mapNodeChanged(explorerMapNode, boatMapNode);
        }
    }



    public class BuildFortAction : GamesLibrary.Action
    {
        public BuildFortAction()
        {
            this.identifier = "build_fort";
            this.requiresTarget = false;
            this.rangeMN = 0;
            this.generateNews = true;
            this.goalsFulfilled = new string[] { "military", "security", "expansion" }.ToList();
            this.severity = 4;
        }

        public override bool isPossible(MapObject source, MapObject target)
        {
            if (!base.isPossible(source, target))
                return false;

            Explorer explorer = source as Explorer;

            return ((explorer != null) && (explorer.captain != null) && (explorer.captain.expertiseArea == Captain.ExpertiseArea.Military));
        }

        public override void execute(MapObject source, MapObject target)
        {
            Explorer explorer = source as Explorer;

            if (explorer != null)
                explorer.captain = null;

            // TODO: Handle Captain doing the order.
        }
    }



    public class BuildMissionAction : GamesLibrary.Action
    {
        public BuildMissionAction()
        {
            this.identifier = "build_mission";
            this.requiresTarget = false;
            this.rangeMN = 0;
            this.generateNews = true;
            this.goalsFulfilled = new string[] { "religion", "expansion" }.ToList();
            this.severity = 3;
        }

        public override bool isPossible(MapObject source, MapObject target)
        {
            if (!base.isPossible(source, target))
                return false;

            Explorer explorer = source as Explorer;

            return ((explorer != null) && (explorer.captain != null) && (explorer.captain.expertiseArea == Captain.ExpertiseArea.Religion));
        }

        public override void execute(MapObject source, MapObject target)
        {
            Explorer explorer = source as Explorer;

            if (explorer != null)
                explorer.captain = null;

            // TODO: Handle Captain doing the order.
        }
    }



    public class BuildSettlementAction : GamesLibrary.Action
    {
        public BuildSettlementAction()
        {
            this.identifier = "build_settlement";
            this.requiresTarget = false;
            this.rangeMN = 0;
            this.generateNews = true;
            this.goalsFulfilled = new string[] { "settlement", "expansion" }.ToList();
            this.severity = 4;
        }

        public override bool isPossible(MapObject source, MapObject target)
        {
            if (!base.isPossible(source, target))
                return false;

            Explorer explorer = source as Explorer;

            return ((explorer != null) && (explorer.captain != null) && (explorer.captain.expertiseArea == Captain.ExpertiseArea.Settlement));
        }

        public override void execute(MapObject source, MapObject target)
        {
            Explorer explorer = source as Explorer;

            if (explorer != null)
                explorer.captain = null;

            // TODO: Handle Captain doing the order.
        }
    }



    public class BuildTradingPostAction : GamesLibrary.Action
    {
        public BuildTradingPostAction()
        {
            this.identifier = "build_tradingpost";
            this.requiresTarget = false;
            this.rangeMN = 0;
            this.generateNews = true;
            this.goalsFulfilled = new string[] { "trade", "expansion" }.ToList();
            this.severity = 2;
        }

        public override bool isPossible(MapObject source, MapObject target)
        {
            if (!base.isPossible(source, target))
                return false;

            Explorer explorer = source as Explorer;

            return ((explorer != null) && (explorer.captain != null) && (explorer.captain.expertiseArea == Captain.ExpertiseArea.Trade));
        }

        public override void execute(MapObject source, MapObject target)
        {
            Explorer explorer = source as Explorer;

            if (explorer != null)
                explorer.captain = null;

            // TODO: Handle Captain doing the order.
        }
    }



    public class DiscoverAction : GamesLibrary.Action
    {
        public DiscoverAction()
        {
            this.identifier = "discover";
            this.requiresTarget = true;
            this.rangeMN = 0;
            this.generateNews = true;
            this.goalsFulfilled = new string[] { "discovery" }.ToList();
            this.severity = 0;
        }

        public override bool isPossible(MapObject source, MapObject target)
        {
            if (!base.isPossible(source, target))
                return false;

            PointOfInterest poi = target as PointOfInterest;
            if (poi == null)
                return false;

            Explorer explorer = source as Explorer;
            if (explorer != null)
                return !explorer.discoveredPOIs.Contains(poi.text) && !IntoTheNewWorld.Instance.recognizedPOIs.Contains(poi.text);

            return true;
        }

        public override void execute(MapObject source, MapObject target)
        {
            PointOfInterest poi = target as PointOfInterest;
            if (target == null)
                return;

            source.state.adjustValue("discovery", poi.claim()); // TODO: Claim needs to be reworked, no longer use internal .claimed variable.
        }
    }



    public class ExploreAction : GamesLibrary.Action
    {
        public ExploreAction()
        {
            this.identifier = "explore";
            this.requiresTarget = false;
            this.rangeMN = 0;
            this.generateNews = false;
            this.goalsFulfilled = new string[] { "discovery", "expansion" }.ToList();
            this.severity = 0;
        }

        public override void execute(MapObject source, MapObject target)
        {
            Mob mob = source as Mob;

            if (mob != null)
                mob.explore();
        }
    }



    public class FleeAction : GamesLibrary.Action
    {
        public FleeAction()
        {
            this.identifier = "flee";
            this.requiresTarget = true;
            this.rangeMN = 1;
            this.generateNews = false;
            this.goalsFulfilled = new string[] { "security" }.ToList();
            this.severity = 0;
        }

        public override void execute(MapObject source, MapObject target)
        {
            Mob mob = source as Mob;
            Mob mobTarget = target as Mob;

            if (mob != null)
                mob.flee(mobTarget);
        }
    }



    public class FollowAction : GamesLibrary.Action
    {
        public FollowAction()
        {
            this.identifier = "follow";
            this.requiresTarget = true;
            this.rangeMN = 1;
            this.generateNews = false;
            this.goalsFulfilled = new string[] { "security", "trade" }.ToList();
            this.severity = 0;
        }

        public override void execute(MapObject source, MapObject target)
        {
            Mob mob = source as Mob;
            Mob mobTarget = target as Mob;

            if (mob != null)
                mob.follow(mobTarget);
        }
    }



    public class LeaveBoatAction : GamesLibrary.Action
    {
        public LeaveBoatAction()
        {
            this.identifier = "leave_boat";
            this.requiresTarget = false;
            this.rangeMN = 0;
            this.generateNews = true;
            //this.goalsFulfilled = new string[] { "discovery" }.ToList();
            this.goalsFulfilled = new string[] { }.ToList();
            this.severity = 0;
            this.cooldown = 1;
        }

        public override bool isPossible(MapObject source, MapObject target)
        {
            if (!base.isPossible(source, target))
                return false;

            Explorer explorer = source as Explorer;
            if (explorer == null)
                return false;

            if (explorer.isDead())
                return false;

            if (!explorer.isOnBoat())
                return false;

            Vector2 v2PlayerPosG = IntoTheNewWorld.Instance._map.worldToGrid(explorer.positionWorld);
            Map.MapNode mapNode = IntoTheNewWorld.Instance._map.getMapNodeG(v2PlayerPosG);
            List<Map.MapNode> connectedMapNodes = IntoTheNewWorld.Instance._map.getConnectedNodes(mapNode);

            // TODO: Instead get walkability?
            return (connectedMapNodes.Any(mapnode => IntoTheNewWorld.Instance._map.getTile(mapnode).terrain.majorType != Terrain.TerrainMajorType.Ocean));
        }

        public override void execute(MapObject source, MapObject target)
        {
            Explorer explorer = source as Explorer;
            if (explorer == null)
                return;

            MapGrid<MapTile> map = IntoTheNewWorld.Instance._map;

            Map.MapNode currentMapNode = map.getMapNode(explorer.positionWorld);
            MapTile currentTile = map.getTile(currentMapNode);

            List<Map.MapNode> connectedNodes = map.getConnectedNodes(currentMapNode);
            float minDistance = float.MaxValue;
            Map.MapNode closestNode = null;
            foreach (Map.MapNode connectedNode in connectedNodes)
            {
                MapTile connectedTile = map.getTile(connectedNode);
                if (connectedTile.terrain.majorType == Terrain.TerrainMajorType.Ocean)
                    continue;

                Vector2 connectedNodePositionG = map.getNodeGridPosition(connectedNode);

                float distance = Vector2.DistanceSquared(explorer.positionWorld, map.gridToWorld(connectedNodePositionG));
                if (distance >= minDistance)
                    continue;

                minDistance = distance;
                closestNode = connectedNode;
            }

            if (closestNode != null)
            {
                Ship ship = new Ship(explorer.positionWorld);
                ship.parked = true;
                ship.owner = explorer;
                IntoTheNewWorld.Instance.parkedShips.Add(ship);

                // TODO: Center the camera?  How?

                Vector2 connectedNodePositionG2 = map.getNodeGridPosition(closestNode);
                explorer.position(map.getCenterW((int)connectedNodePositionG2.Y, (int)connectedNodePositionG2.X));
                explorer.mapNodeChanged(currentMapNode, closestNode);
            }
        }
    }



    public class PatrolAction : GamesLibrary.Action
    {
        public PatrolAction()
        {
            this.identifier = "patrol";
            this.requiresTarget = false;
            this.rangeMN = 0;
            this.generateNews = false;
            this.goalsFulfilled = new string[] { "security" }.ToList();
            this.severity = 0;
        }

        public override void execute(MapObject source, MapObject target)
        {
            Mob mob = source as Mob;

            if (mob != null)
                mob.patrol();
        }
    }



    public class ReturnToEuropeAction : GamesLibrary.Action
    {
        public ReturnToEuropeAction()
        {
            this.identifier = "return_to_europe";
            this.requiresTarget = true;
            this.rangeMN = 0;
            this.generateNews = true;
            this.goalsFulfilled = new string[] { "recognition" }.ToList();
            this.severity = 0;
        }

        public override bool isPossible(MapObject source, MapObject target)
        {
            if (!base.isPossible(source, target))
                return false;

            Explorer explorer = source as Explorer;

            return (explorer != null);
        }

        public override void execute(MapObject source, MapObject target)
        {
            Explorer explorer = source as Explorer;

            // Cash in discovery for fame
            source.state.adjustValue("recognition", source.state.getValue<int>("discovery"));
#if !SIMPLE
            source.state.setValue("discovery", 0);
#endif
        }
    }


    
#if false
    public class SailAction : GamesLibrary.Action
    {
        public SailAction()
        {
            this.identifier = "sail";
            this.requiresTarget = false;
            this.rangeMN = 0;
            this.generateNews = false;
            this.goalsFulfilled = new string[] { "discovery", "recognition" }.ToList();
            this.severity = 0;
        }

        public override bool isPossible(MapObject source, MapObject target)
        {
            if (!base.isPossible(source, target))
                return false;

            Explorer explorer = source as Explorer;

            return ((explorer != null) && explorer.isOnBoat());
        }

        public override void execute(MapObject source, MapObject target)
        {
            Mob mob = source as Mob;

            if (mob != null)
                mob.explore();
        }
    }
#endif



    public class SpawnExplorerAction : GamesLibrary.Action
    {
        public SpawnExplorerAction()
        {
            this.identifier = "spawn_explorer";
            this.requiresTarget = false;
            this.rangeMN = 0;
            this.generateNews = true;
            this.goalsFulfilled = new string[] { "discovery" }.ToList();
            this.severity = 0;
            this.cooldown = 10;
        }

        public override void execute(MapObject source, MapObject target)
        {
            IntoTheNewWorld.Instance.explorers.Add(new Explorer("Explorer_Spawned_" + IntoTheNewWorld.Instance.days, source.positionWorld, IntoTheNewWorld.Instance.players[0].speed));
        }
    }



    public class TradeAction : GamesLibrary.Action
    {
        public TradeAction()
        {
            this.identifier = "trade";
            this.requiresTarget = true;
            this.rangeMN = 1;
            this.generateNews = false;
            this.goalsFulfilled = new string[] { "food", "gold", "oldworld_goods", "newworld_goods", "horses", "weapons" }.ToList();
            this.severity = 0;
            this.cooldown = 1;
        }

        public override void execute(MapObject source, MapObject target)
        {
            // TODO: Do.
        }
    }



    public class WanderAction : GamesLibrary.Action
    {
        public WanderAction()
        {
            this.identifier = "wander";
            this.requiresTarget = false;
            this.rangeMN = 0;
            this.generateNews = false;
            this.goalsFulfilled = new string[] { }.ToList();
            this.severity = 0;
        }

        public override void execute(MapObject source, MapObject target)
        {
            Mob mob = source as Mob;

            if (mob != null)
                mob.wander();
        }
    }
}
