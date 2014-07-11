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
    public class Europe : MapObject
    {
        public Europe(string text, Vector2 pos)
            : base(text, pos)
        {
            //this.sourceActions.Add(IntoTheNewWorld.Instance.actionsByIdentifier["spawn_explorer"]);

            this.targetActions.Add(IntoTheNewWorld.Instance.actionsByIdentifier["return_to_europe"]);

            this.landmark = true;
        }

        public override string getGraphicIdentifier()
        {
            //spriteBatch.DrawString(IntoTheNewWorld.Instance.miramonte, "" + this.value, new Vector2(rectangleDest.Center.X, rectangleDest.Center.Y), Color.White, 0.0f, new Vector2(frame.anchor.X, frame.anchor.Y), 1.0f, spriteEffects, 0);
            return "command_build_town";
        }

        protected override List<Command> getCommands()
        {
            List<Command> commands = new List<Command>();
            commands.Add(new Command("command_europe", Command.Slot.A, this));
            return commands;
        }

        protected override List<string> getWants<T>(VariableBundle gameState, MapGrid<T> map)
        {
            this.wants = base.getWants<T>(gameState, map);

            if (!this.wants.Contains("discovery"))
                this.wants.Add("discovery");

            return this.wants;
        }
    }
}
