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
    public class Warrior : IntoTheNewWorldMob
    {
        public Warrior(string text, Vector2 pos, float speed) : base(text, pos, speed) 
        {
            // Flees at prospect of 60% losses.
            this.state.setValue("defenderLossFleeingThreshold", 60);

#if false
            this.state.setValue("native_like", 100);
#endif

            // Set up the actions the Warrior can do.

            // attack, build fort, demand tribute, flee, follow, join, move to, raze,
            // resupply, spread news, trade
            this.sourceActions.Add(IntoTheNewWorld.Instance.actionsByIdentifier["attack"]);
            this.sourceActions.Add(IntoTheNewWorld.Instance.actionsByIdentifier["flee"]);
            this.sourceActions.Add(IntoTheNewWorld.Instance.actionsByIdentifier["follow"]);

            // attack, demand tribute, flee, follow, join, resupply, spread news, trade
            this.targetActions.Add(IntoTheNewWorld.Instance.actionsByIdentifier["attack"]);
            this.targetActions.Add(IntoTheNewWorld.Instance.actionsByIdentifier["flee"]);
            this.targetActions.Add(IntoTheNewWorld.Instance.actionsByIdentifier["follow"]);

            // patrol
            this.defaultActions.Add(IntoTheNewWorld.Instance.actionsByIdentifier["patrol"]);
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
                    if ((native_like <= -40) && ((action.action is AttackAction) || (action.action is FleeAction) || (action.action is FollowAction)))
                    {
                        Mob focusMob = target as Mob;

                        int odds = CombatWindow.getOdds(this, focusMob);
                        if (((odds >= 70) || (native_like == -100)) && (action.action is AttackAction))
                            return action;
                        else
                        {
                            if ((odds >= 40) && (action.action is FollowAction))
                                return action;
                            else if (action.action is FleeAction)
                                return action;
                        }
                    }
                    if ((native_like <= 40) && (action.action is FollowAction))
                        return action;
                }
            }

            return base.chooseAction(actions, gameState, map);
        }

        public override string getGraphicIdentifier()
        {
            return "native_warrior";
        }
    }
}
