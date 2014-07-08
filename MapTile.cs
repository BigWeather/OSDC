using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GamesLibrary;

namespace IntoTheNewWorld
{
    public class MapTile
    {
        public Terrain terrain;
        public List<MapObject> mapObjects { get; private set; }
        public int density;
        public Dictionary<Side, Graphic> borders
        {
            get { return _borders; }
        }
        private Dictionary<Side, Graphic> _borders;

        public MapTile()
        {
        }

        public void addMapObject(MapObject mapObject)
        {
            if (this.mapObjects == null)
                this.mapObjects = new List<MapObject>();

            this.mapObjects.Add(mapObject);
        }

        public void addBorder(Side side, Graphic graphic)
        {
            if (_borders == null)
                _borders = new Dictionary<Side, Graphic>();

            if (_borders.ContainsKey(side))
                return;

            _borders.Add(side, graphic);
        }

        public void addBorders(List<Side> sides, Graphic graphic)
        {
            if (_borders == null)
                _borders = new Dictionary<Side, Graphic>();

            foreach (Side side in sides)
            {
                if (_borders.ContainsKey(side))
                    continue;

                _borders.Add(side, graphic);
            }
        }

        public Graphic getBorder(Side side)
        {
            if (_borders == null)
                return null;

            Graphic graphic;
            if (!_borders.TryGetValue(side, out graphic))
                return null;

            return graphic;
        }
    }
}
