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
    /// <summary>
    /// Represents an Explorer's Captains, which give passive bonuses while in the expedition
    /// but can also be "spent" to build structures, etc.
    /// </summary>
    public class Captain : IntoTheNewWorldMob
    {
        /// <summary>
        /// Indicates the area of expertise of the captain.  This determines the passive bonuses
        /// and what happens when they are "spent".
        /// </summary>
        public enum ExpertiseArea { Military, Religion, Trade, Settlement }

        /// <summary>
        /// Level of expertise of the captain.  Determines the effectiveness of passive bonuses
        /// and their "spend" action.
        /// </summary>
        public enum ExpertiseLevel { Novice, Apprentice, Journeyman, Expert, Master }

        /// <summary>
        /// Area of expertise of the captain.
        /// </summary>
        public ExpertiseArea expertiseArea { get; private set; }

        /// <summary>
        /// Level of expertise of the captain.
        /// </summary>
        public ExpertiseLevel expertiseLevel { get; private set; }

        /// <summary>
        /// Explorer that the captain is serving.
        /// </summary>
        public Explorer explorer { get; set; }

        public Captain(string text, Vector2 pos, float speed, ExpertiseArea expertiseArea, ExpertiseLevel expertiseLevel)
            : base(text, pos, speed)
        {
            this.expertiseArea = expertiseArea;
            this.expertiseLevel = expertiseLevel;
        }
    }
}
