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
    // TODO: This needs to move to WorldBuilder (but we'll have to rip out the IntoTheNewWorld references).
    public class River
    {
        public List<Point> points
        {
            get { return _points; }
        }
        private List<Point> _points;

        public Vector2 v2SourceLocation { get; private set; }
        public Vector2 v2DeltaLocation { get; private set; }
        public Side sourceSide { get; private set; }
        public Side deltaSide { get; private set; }

        public int length { get; private set; }

        public River()
        {
            _points = new List<Point>();
        }

        public River(int[] pointStrip)
        {
            _points = new List<Point>();
            for (int i = 0; i < pointStrip.Length; i += 2)
                //_points.Add(new Point(pointStrip[i + 1], pointStrip[i]));
                _points.Add(new Point(pointStrip[i], pointStrip[i + 1]));
        }

        public River(List<Point> points)
        {
            _points = points;
        }

        public void etchToMap(MapTile[,] tiles)
        {
            if (tiles == null)
                return;

            // TODO: Handle square grid!

            List<Point> points = _points;
            //bool useHexes = this.hexBased;
            bool useHexes = true;
            int start = 0;
            int end = points.Count - 1;
            //start = 0;
            //end = 24;
            this.length = 0;
            for (int i = start; i < end; i++)
            {
                Point p1 = points[i];
                Point p2 = points[i + 1];

                // If square we halve (round down) each column entry...
                if (!useHexes)
                {
                    // Square reduction can result in same point...
                    if (((p1.X / 2) == (p2.X / 2)) && (p1.Y == p2.Y))
                        continue;
                }

                // Determine the tile and side...
                int row = p1.Y;
                int column = (p1.X - (((row % 2) == 0) ? 0 : 1)) / 2;
                bool evenRow = ((row % 2) == 0);
                Side side = Side.West;
                if (p1.X == p2.X)
                {
                    if (p1.Y > p2.Y)
                    {
                        // South to north, needs to be on west side of tile...
                        side = Side.West;
                        row--;
                        if (!evenRow)
                            column++;
                    }
                    else
                    {
                        // North to south, needs to be on east side of left tile...
                        side = Side.East;
                        column--;
                    }
                }
                else if (p1.Y == p2.Y)
                {
                    if (p1.X > p2.X)
                    {
                        // East to west, needs to be on south side of left tile...
                        if (!useHexes)
                            side = Side.South;
                        else
                        {
                            if ((p1.X % 2) == (row % 2))
                                side = Side.SouthWest;
                            else
                                side = Side.SouthEast;
                        }
                        row--;
                        if (evenRow)
                            column--;
                    }
                    else
                    {
                        // West to east, needs to be on north side of tile...
                        if (!useHexes)
                            side = Side.North;
                        else
                        {
                            if ((p1.X % 2) != (row % 2))
                                side = Side.NorthEast;
                            else
                                side = Side.NorthWest;
                        }
                    }
                }
                else
                    ; // Shouldn't happen!

                // TODO: Special code for letting a normal river overwrite a river's start, if exists.

                Graphic gRiver = IntoTheNewWorld.Instance.dictGraphics["river"];
                v2DeltaLocation = new Vector2(column, row);
                deltaSide = side;

                if (i == start)
                {
                    Graphic gRiverSource;
                    if (IntoTheNewWorld.Instance.dictGraphics.TryGetValue("river_source", out gRiverSource))
                        gRiver = gRiverSource;

                    v2SourceLocation = new Vector2(column, row);
                    sourceSide = side;
                }

                tiles[row, column].addBorder(side, gRiver);
                this.length++;
            }
        }
    }
}
