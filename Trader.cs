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
    public class Priest : IntoTheNewWorldMob
    {
        public Priest(string text, Vector2 pos, float speed) : base(text, pos, speed) 
        {
            // Set up the actions the Trader can do.

            // build mission, convert, demand tribute, flee, follow, move to, resupply, spread news, trade
            this.sourceActions.Add(IntoTheNewWorld.Instance.actionsByIdentifier["flee"]);
            this.sourceActions.Add(IntoTheNewWorld.Instance.actionsByIdentifier["follow"]);
            this.sourceActions.Add(IntoTheNewWorld.Instance.actionsByIdentifier["trade"]);

            // attack, demand tribute, follow, resupply, spread news, trade 
            this.targetActions.Add(IntoTheNewWorld.Instance.actionsByIdentifier["attack"]);
            this.targetActions.Add(IntoTheNewWorld.Instance.actionsByIdentifier["follow"]);
            this.targetActions.Add(IntoTheNewWorld.Instance.actionsByIdentifier["trade"]);

            // wander
            this.defaultActions.Add(IntoTheNewWorld.Instance.actionsByIdentifier["wander"]);
        }

        public override string getGraphicIdentifier()
        {
            return "native_priest";
        }
    }

    public class Trader : IntoTheNewWorldMob
    {
        public Trader(string text, Vector2 pos, float speed) : base(text, pos, speed) 
        {
            // Flees at prospect of 25% losses.
            this.state.setValue("defenderLossFleeingThreshold", 25);

#if false
            this.state.setValue("native_like", 100);
#endif

            // Set up the actions the Trader can do.

            // build trading post, flee, follow, move to, resupply, spread news, trade
            this.sourceActions.Add(IntoTheNewWorld.Instance.actionsByIdentifier["flee"]);
            this.sourceActions.Add(IntoTheNewWorld.Instance.actionsByIdentifier["follow"]);
            this.sourceActions.Add(IntoTheNewWorld.Instance.actionsByIdentifier["trade"]);

            // attack, demand tribute, follow, resupply, spread news, trade 
            this.targetActions.Add(IntoTheNewWorld.Instance.actionsByIdentifier["attack"]);
            this.targetActions.Add(IntoTheNewWorld.Instance.actionsByIdentifier["follow"]);
            this.targetActions.Add(IntoTheNewWorld.Instance.actionsByIdentifier["trade"]);

            // wander
            this.defaultActions.Add(IntoTheNewWorld.Instance.actionsByIdentifier["wander"]);

            this.notesLandmarks = true;
        }

        public override ResolvedAction chooseAction<T>(List<ResolvedAction> actions, VariableBundle gameState, MapGrid<T> map)
        {
            // TODO: Cruise through anything that this Mob may have an interest in and figure out what should get
            //       focus -- maybe a generic routine that evaluates all MapObjects and calls some per-Mob routine
            //       to guage interest, then chooses the highest interest and returns it?  If nothing gets focus
            //       we'll just do some default (wander, explore, patrol) and return, else we stop() and figure out
            //       the course of action...

            foreach (ResolvedAction action in actions)
            {
                MapObject target = action.target;

                // TODO: REMOVE eventually!
                if (!(target is Explorer))
                    continue;

                if ((target != null) && (target is Mob))
                {
                    int native_like = target.state.getValue<int>("native_like"); // TODO: Handle faction stuff better!
                    if ((native_like <= -10) && (action.action is FleeAction))
                        return action;
                    if ((native_like > 40) && (action.action is FollowAction))
                        return action;
                }                
            }

            return base.chooseAction(actions, gameState, map);
        }

        public override string getGraphicIdentifier()
        {
            return "native_trader";
        }
    }
}
