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
    public class IntoTheNewWorldCache : Cache
    {
        public bool isLoot { get; private set; }
        private int _dayCreated = 0;

        public IntoTheNewWorldCache(Vector2 positionWorld, MapObject owner, bool isLoot)
            : this(positionWorld, owner, isLoot, -1) { }

        public IntoTheNewWorldCache(Vector2 positionWorld, MapObject owner, bool isLoot, int dayCreated)
            : base("", positionWorld, owner)
        {
            this.isLoot = isLoot;
            _dayCreated = dayCreated;

            string text;
            if (this.isLoot)
                text = "loot";
            else
                text = "cache";
            if ((owner != null) && !string.IsNullOrEmpty(owner.text))
                text += " (" + owner.text + ")";
            this.text = text;
        }

        public override string getGraphicIdentifier()
        {
            return "cache";
        }

        protected override List<Command> getCommands()
        {
            List<Command> commands = new List<Command>();
            commands.Add(new Command("command_trade", Command.Slot.A, this));
            return commands;
        }

        // TODO: Move to Cache?
        public string[] getAcceptedItems()
        {
            return getAcceptedItems(this.isLoot);
        }

        // TODO: Move to Cache?
        public static string[] getAcceptedItems(bool isLoot)
        {
            if (isLoot)
                return new string[] { "food", "gold", "oldworld_goods", "newworld_goods", "horses", "weapons" };

            return new string[] { "gold", "oldworld_goods", "newworld_goods", "weapons" };
        }

        // TODO: Move to Cache?
        public bool isEmpty()
        {
            List<string> acceptedItems = getAcceptedItems().ToList();
            foreach (string acceptedItem in acceptedItems)
            {
                if (this.state.getValue<int>(acceptedItem) > 0)
                    return false;
            }
            return true;
        }
    }
}
