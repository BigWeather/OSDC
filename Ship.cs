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
    public class Ship : MapObject // TODO: Eventually want a vehicle class with a "driver" that can be AI...
    {
        public bool parked { get; set; }
        public Explorer owner { get; set; }

        public Ship(Vector2 positionWorld) : base("ship", positionWorld) 
        {
            this.targetActions.Add(IntoTheNewWorld.Instance.actionsByIdentifier["board_boat"]);

            //this.landmark = true;
            this.landmark = false;
        }

        public override string getGraphicIdentifier()
        {
            if (this.parked)
                return "parked_boats";

            return "sailing_boats";
        }

        protected override List<Command> getCommands()
        {
            List<Command> commands = new List<Command>();
            if (!this.parked) // TODO: Check to be sure we are near land!
                commands.Add(new Command("command_disembark", Command.Slot.A, this));
            else
                commands.Add(new Command("command_embark", Command.Slot.A, this));
            return commands;
        }
    }
}
