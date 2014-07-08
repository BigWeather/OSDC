using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if Allow_XNA
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endif
using GamesLibrary;

namespace IntoTheNewWorld
{
    public class City : MapObject
    {
        private string[] _resourcesMerge = new string[] { "men", "food", "ships", "gold", "oldworld_goods", "newworld_goods", "horses", "weapons" };

        public int size
        {
            get { return _size; }
        }
        private int _size;

        public List<Mob> units
        {
            get { return _units; }
        }
        private List<Mob> _units;

        bool _active = false;

        public City(Vector2 pos, int size)
            : base("city", pos)
        {
            _size = size;

            _units = new List<Mob>();

            int men = 1000 * _size;

            this.state.setValue("food", IntoTheNewWorld.Instance.weeksToTotalFood(52, men));
            this.state.setValue("gold", 500 * _size);
            this.state.setValue("men", men);
            this.state.setValue("newworld_goods", 750 * _size);

            this.landmark = true;
        }

        private bool isActive()
        {
            //if (Vector2.Distance(positionWorld, IntoTheNewWorld.Instance.players[0].positionWorld) < 1000) // TODO: hard-coded needs to go...
            if (inRange(IntoTheNewWorld.Instance._map, IntoTheNewWorld.Instance.players[0], 4))
                return true;

            return false;
        }

        // AIInterface methods
        #region AIInterface_methods
        public override void process<T>(VariableBundle gameState, MapGrid<T> map)
        {
            base.process(gameState, map);

            // Get rid of any dead units.
            _units = _units.Where(unit => !unit.isDead()).ToList();

            bool active = isActive();

            // If the city isn't active we still need to process returning units.
            if (!active)
            {
                List<Mob> unreturnedUnits = new List<Mob>();
                foreach (Mob unit in _units)
                {
                    //if (Vector2.Distance(unit.positionWorld, this.positionWorld) < 100) // TODO: hard-coded needs to go...
                    if (inRange(IntoTheNewWorld.Instance._map, unit, 0))
                    {
                        this.state.merge(unit.state, _resourcesMerge.ToList());
                        continue;
                    }

                    // Unit needs to return home.
                    //unit.moveTo(this.pos);

                    unreturnedUnits.Add(unit);
                }
                _units = unreturnedUnits;
            }

            if (active == _active)
                return;

            // State change...
            _active = active;

            if (_active)
            {
                // Newly active, spawn some natives.
                List<Mob> newUnits = new List<Mob>();
                for (int i = _units.Count; i < Math.Max(1, _size / 3); i++)
                {
                    // Let's spawn either a trader or warrior, depending on how much they like the player (somewhat...).
                    bool trader = false;
                    int roll = IntoTheNewWorld.Instance.rnd.Next(-100, 100);
                    if (roll < IntoTheNewWorld.Instance.players[0].state.getValue<int>("native_like"))
                        trader = true;

                    // TODO: Fix the fact that the player can kill the warrior and always have traders up thereafter...
                    Mob unit = null;
                    //if ((i % 2) == 0)
                    if (!trader)
                    {
                        unit = new Warrior("Native warriors", this.positionWorld, IntoTheNewWorld.milesPerMillisecond * IntoTheNewWorld.Instance.worldCoordinatesPerMile);

                        int men = 100 * this.size;
                        int food = IntoTheNewWorld.Instance.weeksToTotalFood(4, men);
                        int horses = men;
                        int weapons = men;
                        this.state.moveValue("men", men, unit.state);
                        this.state.moveValue("food", food, unit.state);
                        this.state.moveValue("horses", horses, unit.state);
                        this.state.moveValue("weapons", weapons, unit.state);
                    }
                    else
                    {
                        unit = new Trader("Native traders", this.positionWorld, IntoTheNewWorld.milesPerMillisecond * IntoTheNewWorld.Instance.worldCoordinatesPerMile);

                        // TODO: Do trade goods...
                        int men = this.size;
                        int food = IntoTheNewWorld.Instance.weeksToTotalFood(12, men);
                        int gold = 10 * this.size;
                        int newworld_goods = 100 * this.size;
                        this.state.moveValue("men", men, unit.state);
                        this.state.moveValue("food", food, unit.state);
                        this.state.moveValue("gold", gold, unit.state);
                        this.state.moveValue("newworld_goods", newworld_goods, unit.state);
                        //if (IntoTheNewWorld.Instance.rnd.Next(100) < 20)
                        //    this.state.moveValue("rare_good", 10 * this.size, unit.state);
                    }
                    newUnits.Add(unit);
                }
                _units.AddRange(newUnits);

                // Have the units stop whatever they were doing and they'll refigure out their behavior.
                // TODO: Ok to comment out?  Just seems like that the existing units shouldn't have to change
                //       their behavior, right?
#if OLD_AI
                foreach (Mob unit in _units)
                    unit.stop();
#else
                // City is active again, we need to have any units that were moving to the city to despawn
                // to stop and re-evaluate.
                foreach (Mob unit in _units)
                {
                    if (unit.behavior == Mob.Behavior.MoveTo)
                        unit.stop();
                }
#endif
            }
            else
            {
                // Newly inactive, have the units return home.
                foreach (Mob unit in _units)
                    unit.moveTo(this.positionWorld);
            }
        }
        #endregion

        public override string getGraphicIdentifier()
        {
            return "native_city";
        }

        protected override List<Command> getCommands()
        {
            List<Command> commands = new List<Command>();
            commands.Add(new Command("command_trade", Command.Slot.A, this));
            commands.Add(new Command("command_fight", Command.Slot.X, this));
            return commands;
        }
    }
}
