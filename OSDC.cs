using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if Allow_XNA
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
#endif
using GamesLibrary;

namespace OSDC
{
    class OSDC : BaseGame, MapConsumerInterface
    {
        public static OSDC Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new OSDC();

                return _instance;
            }
        }
        private static OSDC _instance = null;

        private OSDC() { }

        List<Room> _rooms;
        public class Room
        {
            public string[] plan;
            public string name;
            public string label;
        }

        public enum TileEnum
        {
            OneSquareCircle, MousePointer
        }

        public Dictionary<char, string> dictIdentifierByMapCharacter = new Dictionary<char, string>();
        Vector2 _tilesize = new Vector2(25, 25);
        //public int worldCoordinatesPerFoot = 6;
        //public int feetPerTile = 5;
        //public float worldCoordinatesPerFoot = 0.2f;
        //public int feetPerTile = 5;
        public float worldCoordinatesPerFoot = 12f;
        public int feetPerTile = 5;

        private MapSquare<char> _map;

        public Mob player;
        public List<Mob> mobs;

        //const float _walkingFeetPerSecond = 4.22f; // http://en.wikipedia.org/wiki/Walking
        const float _walkingFeetPerSecond = 16.88f; // made up
        public const float walkingFeetPerMillisecond = _walkingFeetPerSecond / 1000.0f;

        // Overridden methods below...
        public override void InitializeGraphics()
        {
        }

        public override void Initialize()
        {
            initRooms();
            initMap();
            initMobs();
            initTiles();

            Show(new OSDCMapWindow(_map, rectTileSafeArea, _tilesize, player.positionWorld));
        }

        public override void LoadContent()
        {
            // Load textures
            this.loadTexture("OSDC_Tiles", @"OSDC_Tiles");

            // Load fonts
        }

        public override void PostLoadContent()
        {
            //throw new NotImplementedException();
        }

        public override void UnloadContent()
        {
        }

        public override void UpdateGameState(GameTime gameTime)
        {
            double milliseconds = gameTime.ElapsedGameTime.TotalMilliseconds;

            for (int i = 1; i < this.mobs.Count; i++)
            {
                Mob mob = this.mobs[i];
                mob.update(_map, milliseconds);
            }
        }

        protected override Graphic getMousePointerGraphic(Vector2 mousePos)
        {
            return dictGraphics[dictIdentifierByMapCharacter[(char)TileEnum.MousePointer]];
        }
        // Overridden methods above...

        private void initRooms()
        {
            _rooms = new List<Room>();

            Room entry = new Room();
            entry.label = "1";
            entry.name = "Entry";
            entry.plan = new string[] { 
                "XXXXXXXXXXXXXXXXXXXXXXX",
                "XXXX                  X",
                "XXXXDDXXXXXXXXSXXXXXXDX",
                "X p    p XXXXX XXXXXX X",
                "XX      XXXXXX XXXXXX X",
                "X p    p XXXXX XX     X",
                "X        XXXXX XX XXXXX",
                "X p    p XXXXX XX XXXXX",
                "XX      XXXXXXSXXDXXXXX",
                "X p    p XX p   p XXXXX",
                "XXSXAAXXXXX p   p XXXXX",
                "XX X  XXXXX p   p XXXXX",
                "XXDX  XXXXX p   p XXXXX",
                "X  X  XXXXX p   p XXXXX",
                "X  X  XXXXXXXXXXXXXXXXX",
                "XXXXPPXXXXXXXXXXXXXXXXX"
            };

            _rooms.Add(entry);
        }

        private void initMap()
        {
            int rows = _rooms[0].plan.Length;
            int cols = _rooms[0].plan[0].Length;
            char[,] tiles = new char[rows, cols];
            for (int row = 0; row < rows; row++)
                for (int col = 0; col < cols; col++)
                    tiles[row, col] = _rooms[0].plan[row][col];

            bool allowDiagonalPathing = false;
            _map = new MapSquare<char>(tiles, allowDiagonalPathing, new Vector2(this.worldCoordinatesPerFoot * this.feetPerTile), this);
        }

        private void initMobs()
        {
            this.mobs = new List<Mob>();

            float speed = OSDC.walkingFeetPerMillisecond * worldCoordinatesPerFoot;

            this.player = new Mob("PC", _map.gridToWorld(new Vector2(1, 14)), speed);
            this.mobs.Add(this.player);

            Mob king = new Mob("K", _map.gridToWorld(new Vector2(14, 12)), speed);
            king.follow(player);
            this.mobs.Add(king);

            Mob mob = new Mob("k", _map.gridToWorld(new Vector2(4, 1)), speed);
            mob.follow(king);
            this.mobs.Add(mob);

            mob = new Mob("k", _map.gridToWorld(new Vector2(21, 4)), speed);
            mob.flee(player);
            this.mobs.Add(mob);
        }

        private void initTiles()
        {
            Rectangle rectTile = new Rectangle(0, 0, (int)_tilesize.X, (int)_tilesize.Y);
            addGraphic(new GraphicSimple("solid", "OSDC_Tiles", 0, 3, rectTile));
            addGraphic(new GraphicSimple("open", "OSDC_Tiles", 1, 3, rectTile));
            addGraphic(new GraphicSimple("door", "OSDC_Tiles", 0, 2, rectTile));
            addGraphic(new GraphicSimple("pillar", "OSDC_Tiles", 0, 1, rectTile));
            addGraphic(new GraphicSimple("secret_door", "OSDC_Tiles", 1, 2, rectTile));
            addGraphic(new GraphicSimple("portcullis", "OSDC_Tiles", 1, 0, rectTile));
            addGraphic(new GraphicSimple("arch", "OSDC_Tiles", 1, 1, rectTile));
            addGraphic(new GraphicSimple("one_square_circle", "OSDC_Tiles", 2, 1, rectTile, new Point(rectTile.Width / 2, rectTile.Height / 2)));
            addGraphic(new GraphicSimple("mouse_pointer", "OSDC_Tiles", 2, 2, rectTile));
            addGraphic(new GraphicSimple("small_square", "SmallSquare", new Rectangle(0, 0, 2, 2)));

            dictIdentifierByMapCharacter.Add('X', "solid");
            dictIdentifierByMapCharacter.Add(' ', "open");
            dictIdentifierByMapCharacter.Add('D', "door");
            dictIdentifierByMapCharacter.Add('p', "pillar");
            dictIdentifierByMapCharacter.Add('S', "secret_door");
            dictIdentifierByMapCharacter.Add('P', "portcullis");
            dictIdentifierByMapCharacter.Add('A', "arch");
            dictIdentifierByMapCharacter.Add((char)TileEnum.OneSquareCircle, "one_square_circle");
            dictIdentifierByMapCharacter.Add((char)TileEnum.MousePointer, "mouse_pointer");
        }

#if false
        public float getEdgeCost<R>(R requester, Map.MapNode start, Map.MapNode end)
        {
            return 1.0f;
        }
#endif

        public float getCost<R>(R requester, Map.MapNode start, Map.MapNode end)
        {
            char tile = _map.getTile(end);

            if (tile == 'X')
                return float.MaxValue;
            if (tile == 'D')
            {
                if (requester.Equals(this.player))
                    return 1.0f;
                return float.MaxValue;
            }
            //if (tile == 'p')
            //    return 0.25f;

            return 1.0f;
        }

        public float getMovementModifier<R>(R requester, Map.MapNode node)
        {
            char tile = _map.getTile(node);

            if (tile == ' ')
                return 1.0f;
            else if (tile == 'D')
                return 0.5f;
            else if (tile == 'p')
                return 0.75f;

            //return 0.0f;
            return 1.0f;
        }

        public int getOpacity(Map.MapNode node)
        {
            return 1;
        }
    }






    public class OSDCMapWindow : MapGridWindow<char>
    {
        public OSDCMapWindow(MapGrid<char> map, Rectangle bounds, Vector2 tileSize, Vector2 cameraPos)
            : base(map, bounds, tileSize, cameraPos, OSDC.Instance) { }

        public override void HandleInput(GameTime gameTime)
        {
            // TODO: Centralize this?
            double milliseconds = gameTime.ElapsedGameTime.TotalMilliseconds;
            Mob player = OSDC.Instance.player;

            KeyboardState kbs = Keyboard.GetState();
            MouseState ms = Mouse.GetState();

            // Move the player (and the camera, provided Right Shift isn't pressed)
            if (ms.LeftButton == ButtonState.Pressed)
            {
                Vector2 v3MouseOffset = screenToWorld(OSDC.Instance.mousePos) - player.positionWorld;
                player.moveDirection(this.map, v3MouseOffset, milliseconds);

                if (!kbs.IsKeyDown(Keys.RightShift))
                    cameraPos = player.positionWorld;
            }

            // Center the camera
            if (kbs.IsKeyDown(Keys.RightControl))
                cameraPos = player.positionWorld;
            if (ms.MiddleButton == ButtonState.Pressed)
                cameraPos = player.positionWorld;

            base.HandleInput(gameTime);
        }

        public override void Draw(GameTime gameTime, VariableBundle gameState, SpriteBatch spriteBatch)
        {
            Graphic tile;
            Vector2 _worldCoordinatesPerTile = this.map.worldCoordinatesPerTile;

            Dictionary<string, Graphic> _graphics = OSDC.Instance.dictGraphics;

            Point tileExtents = map.getTileExtents();

            //spriteBatch.Begin(sortMode: SpriteSortMode.Immediate, blendState: BlendState.AlphaBlend, samplerState: null, depthStencilState: null, rasterizerState: null, effect: null, transformMatrix: mxT); 
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, null, mxTWorldToScreen);
            //spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None, mxT);
            Vector2 v3MapDims = this.map.getDimensions();
            for (int x = 0; x < (int)v3MapDims.X; x++)
                for (int y = 0; y < (int)v3MapDims.Y; y++)
                {
                    char cTile = this.map.getTile(new Vector2(x, y));
                    if (cTile == 'X')
                        continue;

                    Point tileXY = map.getULPixel(y, x);

                    tile = _graphics[OSDC.Instance.dictIdentifierByMapCharacter[' ']];
                    //spriteBatch.Draw(OSDC.Instance.getTexture(tile), new Rectangle((int)(x * _worldCoordinatesPerTile.X), (int)(y * _worldCoordinatesPerTile.Y), (int)_worldCoordinatesPerTile.X, (int)_worldCoordinatesPerTile.Y), tile.getCurrentFrame(gameTime, gameState).bounds, Color.White);
#if OLD_TEXTURE
                    spriteBatch.Draw(OSDC.Instance.getTexture(tile), new Rectangle(tileXY.X, tileXY.Y, tileExtents.X, tileExtents.Y), tile.getCurrentFrame(gameTime, gameState).bounds, Color.White);
#else
                    tile.Draw(gameTime, gameState, spriteBatch, new Rectangle(tileXY.X, tileXY.Y, tileExtents.X, tileExtents.Y));
#endif
                    if (cTile == ' ')
                        continue;
                    tile = _graphics[OSDC.Instance.dictIdentifierByMapCharacter[cTile]];
                    if (tile != null)
                    {
                        //spriteBatch.Draw(OSDC.Instance.getTexture(tile), new Rectangle((int)(x * _worldCoordinatesPerTile.X), (int)(y * _worldCoordinatesPerTile.Y), (int)_worldCoordinatesPerTile.X, (int)_worldCoordinatesPerTile.Y), tile.getCurrentFrame(gameTime, gameState).bounds, Color.White);
#if OLD_TEXTURE
                        spriteBatch.Draw(OSDC.Instance.getTexture(tile), new Rectangle(tileXY.X, tileXY.Y, tileExtents.X, tileExtents.Y), tile.getCurrentFrame(gameTime, gameState).bounds, Color.White);
#else
                        tile.Draw(gameTime, gameState, spriteBatch, new Rectangle(tileXY.X, tileXY.Y, tileExtents.X, tileExtents.Y));
#endif
                    }
                }

            // Draw path
            Graphic gSmallSquare = OSDC.Instance.dictGraphics["small_square"];
            foreach (Mob mob in OSDC.Instance.mobs)
            {
                if (mob.path == null)
                    continue;

                foreach (Vector2 v3Path in mob.path)
#if OLD_TEXTURE
                    spriteBatch.Draw(OSDC.Instance.tx2dSmallSquare, new Rectangle((int)((v3Path.X * _worldCoordinatesPerTile.X) + (_worldCoordinatesPerTile.X / 4.0f)), (int)((v3Path.Y * _worldCoordinatesPerTile.Y) + (_worldCoordinatesPerTile.Y / 4.0f)), (int)(_worldCoordinatesPerTile.X / 2.0f), (int)(_worldCoordinatesPerTile.Y / 2.0f)), Color.Red);
#else
                    gSmallSquare.Draw(gameTime, gameState, spriteBatch, new Rectangle((int)((v3Path.X * _worldCoordinatesPerTile.X) + (_worldCoordinatesPerTile.X / 4.0f)), (int)((v3Path.Y * _worldCoordinatesPerTile.Y) + (_worldCoordinatesPerTile.Y / 4.0f)), (int)(_worldCoordinatesPerTile.X / 2.0f), (int)(_worldCoordinatesPerTile.Y / 2.0f)), Color.Red);
#endif
                }

#if true
            // Draw mobs (transformed)
            foreach (Mob mob in OSDC.Instance.mobs)
            {
                tile = _graphics[OSDC.Instance.dictIdentifierByMapCharacter[(char)OSDC.TileEnum.OneSquareCircle]];
                Frame frame = tile.getCurrentFrame(gameTime, gameState);
                Rectangle bounds = frame.bounds;
                Point anchor = frame.anchor;
                //Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2);
                Vector2 origin = new Vector2(anchor.X, anchor.Y);

                Vector2 v3Mob = mob.positionWorld;

#if OLD_TEXTURE
                spriteBatch.Draw(OSDC.Instance.getTexture(tile),
                    new Rectangle((int)v3Mob.X, (int)v3Mob.Y, (int)_worldCoordinatesPerTile.X, (int)_worldCoordinatesPerTile.Y),
                    bounds,
                    Color.White, (float)mob.facing, origin, SpriteEffects.None, 0);
#else
                tile.Draw(gameTime, gameState, spriteBatch, new Rectangle((int)v3Mob.X, (int)v3Mob.Y, (int)_worldCoordinatesPerTile.X, (int)_worldCoordinatesPerTile.Y), Color.White, (float)mob.facing, origin, 0);
#endif
                //spriteBatch.Draw(OSDC.Instance.getTexture(tile),
                //    new Rectangle((int)v3Mob.X, (int)v3Mob.Y, (int)_worldCoordinatesPerTile, (int)_worldCoordinatesPerTile),
                //    bounds,
                //    Color.White);
            }
#endif
            spriteBatch.End();

#if false
            // Draw mobs (non-transformed)
            spriteBatch.Begin();
            foreach (Mob mob in _mobs)
            {
                tile = _tiles[(char)TileEnum.OneSquareCircle];
                Vector2 origin = new Vector2(tile.rectContentPos.Width / 2, tile.rectContentPos.Height / 2);

                Vector2 v3Mob = mob.pos;
                v3Mob = worldToScreen(v3Mob);

                spriteBatch.Draw(tile.texture,
                    new Rectangle((int)v3Mob.X, (int)v3Mob.Y, _tileSize, _tileSize),
                    tile.rectContentPos,
                    Color.White, (float)mob.facing, origin, SpriteEffects.None, 0);
            }
            spriteBatch.End();
#endif

            // Draw mob labels
            spriteBatch.Begin();
            foreach (Mob mob in OSDC.Instance.mobs)
            {
                Vector2 v3MobScreen = worldToScreen(mob.positionWorld);
                string text = mob.text;
                Vector2 v2Text = OSDC.Instance.miramonte.MeasureString(text);
                spriteBatch.DrawString(OSDC.Instance.miramonte, text, new Vector2(v3MobScreen.X - (v2Text.X / 2), v3MobScreen.Y - (v2Text.Y / 2)), Color.CornflowerBlue);
            }
            spriteBatch.End();

            base.Draw(gameTime, gameState, spriteBatch);
        }
    }
}
