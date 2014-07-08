using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IntoTheNewWorld
{
    public class Terrain
    {
        public enum TerrainMajorType { Ocean, Forest, Plains, Desert, Swamp, Hills, Mountains, Lake }
        public enum TerrainMinorType { Ocean, Forest, TropicalForest, EvergreenForest, Plains, Desert, Swamp, Hills, Mountain, Savanna, Tundra }

        public string graphicIdentifier
        {
            get { return _graphicIdentifier; }
        }
        private string _graphicIdentifier;

        public TerrainMajorType majorType { get; private set; }
        public TerrainMinorType minorType { get; private set; }

        public float moveModifier = 1.0f;
        public float forageModifier = 1.0f;
        public int visibilityRange = 1;
        public int height = 0;

        public Terrain(string graphicIdentifier, TerrainMajorType majorType, TerrainMinorType minorType, float moveModifier, float forageModifier, int visibilityRange, int height)
        {
            _graphicIdentifier = graphicIdentifier;
            this.majorType = majorType;
            this.minorType = minorType;
            this.moveModifier = moveModifier;
            this.forageModifier = forageModifier;
            this.visibilityRange = visibilityRange;
            this.height = height;
        }
    }
}
