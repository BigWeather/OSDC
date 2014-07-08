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
    public class IntoTheNewWorldMob : Mob // TODO: Temporary class?
    {
        public IntoTheNewWorldMob(string text, Vector2 pos, float speed) : base(text, pos, speed) { }

        public override void mapNodeChanged(Map.MapNode oldMapNode, Map.MapNode newMapNode)
        {
            base.mapNodeChanged(oldMapNode, newMapNode);

            MapGrid<MapTile> map = IntoTheNewWorld.Instance._map;
            MapTile newTile = map.getTile(newMapNode);

            this.visibleMapNodes = map.getVisibleNodes(newMapNode, newTile.terrain.visibilityRange);
        }

        public void eat()
        {
            // Only Explorers need to eat.
            // TODO: Eventually other Europeans too, right?  Assume all Natives get along just fine with foraging?
            if (!(this is Explorer))
                return;

            // Get the current map node, tile, and terrain information.
            Map.MapNode mn = IntoTheNewWorld.Instance._map.getMapNode(this.positionWorld);
            MapTile tile = IntoTheNewWorld.Instance._map.getTile(mn);
            Terrain terrain = tile.terrain;

            int men = this.state.getValue<int>("men");
            int foodForagedPerMan = (int)(IntoTheNewWorld.Instance.forageperday * terrain.forageModifier);
            int foodEatenPerMan = IntoTheNewWorld.Instance.foodperday;

            // Forage first, then eat.
            this.state.adjustValue("food", men * foodForagedPerMan);
            this.state.adjustValue("food", -(men * foodEatenPerMan));

            //this.state.adjustValue("food", (int)(IntoTheNewWorld.Instance.forageperday * terrain.forageModifier));
            //this.state.adjustValue("food", -IntoTheNewWorld.Instance.foodperday);

            if (this.state.getValue<int>("food") <= 0)
            {
                this.state.setValue("food", 0);
                //playerState.dead = true;
            }
        }

        protected override List<string> getNeeds<T>(VariableBundle gameState, MapGrid<T> map)
        {
            // Should these be in their own routines?
            this.needs = base.getNeeds(gameState, map);

            // Only Explorers need to eat.
            // TODO: Eventually other Europeans too, right?  Assume all Natives get along just fine with foraging?
            if (this is Explorer)
            {
                // If fewer than two weeks food (under worst circumstances -- no foraging) are left then
                // signal need for food.
                int men = this.state.getValue<int>("men");
                int food = this.state.getValue<int>("food");
                if (food < IntoTheNewWorld.Instance.weeksToTotalFood(2, men))
                {
                    if (!this.needs.Contains("food"))
                        this.needs.Add("food");
                }
            }

            return this.needs;
        }

        protected override List<Command> getCommands()
        {
            List<Command> commands = new List<Command>();
            commands.Add(new Command("command_trade", Command.Slot.A, this));
            commands.Add(new Command("command_fight", Command.Slot.X, this));
            return commands;
        }

        /// <summary>
        /// Returns whether or not the Mob is dead.
        /// </summary>
        /// <returns></returns>
        public override bool isDead()
        {
            if (this.state.getValue<int>("men") <= 0)
                return true;

            if (this.state.getValue<int>("food") <= 0)
                return true;

            return base.isDead();
        }

        public override List<MapObject> getActionTargets()
        {
            return IntoTheNewWorld.Instance.getActionTargets(this);
        }
    }

}
