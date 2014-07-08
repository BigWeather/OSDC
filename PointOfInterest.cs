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
    public class PointOfInterest : MapObject
    {
        public int value { get; private set; }
        public string claimMessage { get; private set; }
        public bool claimed { get; private set; }
        public string subject { get; private set; }

        public PointOfInterest(Vector2 pos, int value, string claimMessage, string subject)
            : base(subject, pos)
        {
            this.value = value;
            this.claimMessage = claimMessage;
            this.subject = subject;

            this.claimed = false;

            this.targetActions.Add(IntoTheNewWorld.Instance.actionsByIdentifier["discover"]);
        }

        public int claim()
        {
            if (this.claimed)
                return 0;

            this.claimed = true;

            return this.value;
        }

        public override string getGraphicIdentifier()
        {
            //spriteBatch.DrawString(IntoTheNewWorld.Instance.miramonte, "" + this.value, new Vector2(rectangleDest.Center.X, rectangleDest.Center.Y), Color.White, 0.0f, new Vector2(frame.anchor.X, frame.anchor.Y), 1.0f, spriteEffects, 0);
            return "landmark";
        }

        protected override List<Command> getCommands()
        {
            List<Command> commands = new List<Command>();
            commands.Add(new Command("command_claim", Command.Slot.A, this));
            return commands;
        }
    }
}
