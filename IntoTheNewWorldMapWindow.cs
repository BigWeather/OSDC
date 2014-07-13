#define SCALE_ONLY
#define PARCHMENT

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if Allow_XNA
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
#endif
using GamesLibrary;

namespace IntoTheNewWorld
{
    public class IntoTheNewWorldMapWindow : MapGridWindow<MapTile>
    {
        Dictionary<Command.Slot, Button> _dictCommandButtonsBySlot;
        Dictionary<Command.Slot, Command> _dictCommandsBySlot; // TODO: Build into a new CommandButton class?

        SpriteFont _font = (SpriteFont)FontManager.Instance.getFont(IntoTheNewWorld.Instance.fontName);

        public IntoTheNewWorldMapWindow(MapGrid<MapTile> map, Rectangle bounds, Vector2 tileSizeA, Vector2 cameraPos)
            : base(map, bounds, tileSizeA, cameraPos, IntoTheNewWorld.Instance)
        {
            _dictCommandButtonsBySlot = new Dictionary<Command.Slot, Button>();

            Button commandButton = new Button(new Rectangle(0, 0, 0, 0), true, null, false);
            commandButton.Press += new EventHandler<PressEventArgs>(commandButton_Press);
            _dictCommandButtonsBySlot.Add(Command.Slot.A, commandButton);

            commandButton = new Button(new Rectangle(0, 0, 0, 0), true, null, false);
            commandButton.Press += new EventHandler<PressEventArgs>(commandButton_Press);
            _dictCommandButtonsBySlot.Add(Command.Slot.B, commandButton);

            commandButton = new Button(new Rectangle(0, 0, 0, 0), true, null, false);
            commandButton.Press += new EventHandler<PressEventArgs>(commandButton_Press);
            _dictCommandButtonsBySlot.Add(Command.Slot.X, commandButton);

            commandButton = new Button(new Rectangle(0, 0, 0, 0), true, null, false);
            commandButton.Press += new EventHandler<PressEventArgs>(commandButton_Press);
            _dictCommandButtonsBySlot.Add(Command.Slot.Y, commandButton);

            _dictCommandsBySlot = new Dictionary<Command.Slot, Command>();
        }

        bool _cameraMove = true;

        public override void HandleInput(GameTime gameTime)
        {
            // TODO: Ick, is this the best place to put this?!
            if (IntoTheNewWorld.Instance.players[0].isDead())
                showMessageBox("It appears that your journey has\ncome to an unexpected end!\n" + IntoTheNewWorld.Instance.getScoreString(), exitGameCallback);

            // TODO: Centralize this?
            double milliseconds = gameTime.ElapsedGameTime.TotalMilliseconds;
            Mob player = IntoTheNewWorld.Instance.players[0];

#if WINDOWS
            KeyboardState kbs = Keyboard.GetState();
            MouseState ms = Mouse.GetState();
            //GamePadState gps = GamePad.GetState(PlayerIndex.One); // TODO: Don't hardcode player index!

            // Move the player (and the camera, provided Right Shift isn't pressed)
            if (ms.LeftButton == ButtonState.Pressed)
            {
                Vector2 v3MouseOffset = screenToWorld(IntoTheNewWorld.Instance.mousePos) - player.positionWorld;
                if (!IntoTheNewWorld.Instance.players[0].isDead())
                    player.moveDirection(this.map, v3MouseOffset, milliseconds);

                //if (!kbs.IsKeyDown(Keys.RightShift))
                if (_cameraMove)
                    cameraPos = player.positionWorld;
            }

            // Center the camera
            if (kbs.IsKeyDown(Keys.RightControl))
                cameraPos = player.positionWorld;
            if (ms.MiddleButton == ButtonState.Pressed)
                cameraPos = player.positionWorld;

            //if (kbs.IsKeyDown(Keys.F))
            //{
            //    IntoTheNewWorld.Instance.playerState.food += 1000;
            //    IntoTheNewWorld.Instance.playerState.dead = false;
            //}
            //if (kbs.IsKeyDown(Keys.R))
            //{
            //    // TODO: FIX RESET!
            //    IntoTheNewWorld.Instance.reset();
            //    cameraPos = player.positionWorld;
            //}
            if (kbs.IsKeyDown(Keys.C))
            {
                cameraPos = player.positionWorld;
                this.scale = 1.0f;
                this.rotation = 0.0f;
            }
            //if (kbs.IsKeyDown(Keys.T))
            //{
            //    VariableBundle leftTrader = IntoTheNewWorld.Instance.players[0].state;
            //    VariableBundle rightTrader = new VariableBundle();
            //    rightTrader.setValue("food", IntoTheNewWorld.Instance.weeksToTotalFood(40, 100));
            //    rightTrader.setValue("gold", 600);
            //    rightTrader.setValue("men", 100);
            //    rightTrader.setValue("newworld_goods", 750);
            //    rightTrader.setValue("horses", 35);
            //    rightTrader.setValue("weapons", 10);
            //    rightTrader.setValue("oldworld_goods", 40);
            //    rightTrader.setValue("ships", 0);
            //    TradeWindow tradeWindow = new TradeWindow(this.center, 800, leftTrader, rightTrader, TradeWindow.TradingPartner.Europe, 1, "Cheat cache");
            //    //TradeWindow tradeWindow = new TradeWindow(new Rectangle(200, 25, 800, 570), leftTrader, rightTrader, TradeWindow.TradingPartner.Cache, 1);
            //    IntoTheNewWorld.Instance.Show(tradeWindow);
            //}
#if false
            if (kbs.IsKeyDown(Keys.Q))
                _cameraMove = false;
            if (kbs.IsKeyDown(Keys.W))
                _cameraMove = true;
#endif
#elif WINDOWS_PHONE
            TouchPanel.EnabledGestures = GestureType.FreeDrag | GestureType.Tap;
            if (TouchPanel.IsGestureAvailable)
            {
                //GestureSample gs = TouchPanel.ReadGesture();
                //if (gs.GestureType == GestureType.FreeDrag)
                {
                    TouchCollection tc = TouchPanel.GetState();
                    foreach (TouchLocation tl in tc)
                    {
                        if (tl.State != TouchLocationState.Moved)
                            continue;

                        //Vector2 pos = gs.Position;
                        Vector2 pos = tl.Position;

                        Vector2 v2TouchOffset = screenToWorld(pos) - player.pos;
                        if (!IntoTheNewWorld.Instance.playerState.dead)
                            player.moveDirection(this.map, v2TouchOffset, milliseconds);

                        if (_cameraMove)
                            cameraPos = player.pos;
                    }
                }
            }
#elif XBOX360
            GamePadState gps = GamePad.GetState(PlayerIndex.One);

            // Move the player
            if (!IntoTheNewWorld.Instance.playerState.dead)
            {
                if (gps.ThumbSticks.Left != Vector2.Zero)
                {
                    player.moveDirection(this.map, new Vector2(gps.ThumbSticks.Left.X, -gps.ThumbSticks.Left.Y), milliseconds);

                    if (_cameraMove)
                        cameraPos = player.pos;
                }
            }

            // Move the camera
            if (gps.ThumbSticks.Right != Vector2.Zero)
                cameraPos += (new Vector2(gps.ThumbSticks.Right.X, -gps.ThumbSticks.Right.Y) * 10);
#endif

            base.HandleInput(gameTime);
        }

        private enum Visibility { Visible, NotVisible, Unexplored }

        private Visibility getVisibility(Map.MapNode mn, Dictionary<Map.MapNode, bool> dictVisibleByMapNode)
        {
            if (dictVisibleByMapNode == null)
                return Visibility.Unexplored;

            if (mn == null) // TODO: Do I really want to do this?
                return Visibility.Unexplored;

            if (!IntoTheNewWorld.Instance.players[0].seen.ContainsKey(mn))
                return Visibility.Unexplored;

            if (!dictVisibleByMapNode.ContainsKey(mn))
                return Visibility.NotVisible;

            return Visibility.Visible;
        }

        public override void Draw(GameTime gameTime, VariableBundle gameState, SpriteBatch spriteBatch)
        {
            base.Draw(gameTime, gameState, spriteBatch);

            SpriteFont font = _font;

            // Clear any hotspots from the last render.
            this.hotspots = new List<Hotspot>();

            // Disable any command buttons.
            foreach (Button commandButton in _dictCommandButtonsBySlot.Values)
                commandButton.enabled = false;

            // Clear any slotted commands.
            _dictCommandsBySlot.Clear();

            Color cOcean = new Color(0, 148, 255);
            Color cLand = new Color(198, 185, 117);

            //if (this.scale == 1.0f) // TODO:AA: Re-enable
            {
#if PARCHMENT
                spriteBatch.Begin();

                Graphic gParchment = IntoTheNewWorld.Instance.dictGraphics["parchment"];
                Frame fParchment = gParchment.getCurrentFrame(gameTime, gameState);

#if false
                Vector2 v2ULW = screenToWorld(Vector2.Zero);
                Vector2 v2FrameBoundsW = Vector2.Transform(new Vector2(fParchment.bounds.Width, fParchment.bounds.Height), mxTBFYI);

                float xW1 = (v2ULW.X % v2FrameBoundsW.X);
                float xW = -xW1;
                if (xW1 < 0)
                    xW = Math.Abs(xW1) - v2FrameBoundsW.X;

                float yW1 = (v2ULW.Y % v2FrameBoundsW.Y);
                float yW = -yW1;
                if (yW1 < 0)
                    yW = Math.Abs(yW1) - v2FrameBoundsW.Y;

                Vector2 v2FramePositionW = new Vector2(xW, yW);
                v2FramePositionW = v2FramePositionW + v2ULW;
                Vector2 v2FramePositionS = worldToScreen(v2FrameXYW);
#else
                Vector2 v2ULW = screenToWorld(Vector2.Zero);
                //Vector2 v2FrameBoundsW = Vector2.Transform(new Vector2(fParchment.bounds.Width, fParchment.bounds.Height), mxTBFYI);
                Vector2 v2FrameBoundsW = screenToWorld(new Vector2(fParchment.bounds.Width, fParchment.bounds.Height)) - v2ULW;

                float xW = -(v2ULW.X % v2FrameBoundsW.X);
                if (xW > 0)
                    xW -= v2FrameBoundsW.X;
                float yW = -(v2ULW.Y % v2FrameBoundsW.Y);
                if (yW > 0)
                    yW -= v2FrameBoundsW.Y;
                Vector2 v2FramePositionW = new Vector2(xW, yW);

                Vector2 v2FramePositionS = worldToScreen(v2FramePositionW + v2ULW);
#endif

                for (int x = (int)v2FramePositionS.X; x < IntoTheNewWorld.Instance.graphics.PreferredBackBufferWidth; x += fParchment.bounds.Width)
                    for (int y = (int)v2FramePositionS.Y; y < IntoTheNewWorld.Instance.graphics.PreferredBackBufferHeight; y += fParchment.bounds.Height)
                    {
                        gParchment.Draw(fParchment, spriteBatch, new Point(x, y));
                    }

                spriteBatch.End();
#else
                IntoTheNewWorld.Instance.GraphicsDevice.Clear(cLand);
#endif
            }

            Map.MapNode mnPlayer = this.map.getMapNode(IntoTheNewWorld.Instance.players[0].positionWorld);
            List<Map.MapNode> mnsVisible = this.map.getVisibleNodes(mnPlayer, this.map.getTile(mnPlayer).terrain.visibilityRange);
            Dictionary<Map.MapNode, bool> dictVisibleByMapNode = new Dictionary<Map.MapNode, bool>();
            foreach (Map.MapNode mnVisible in mnsVisible)
            {
                if (dictVisibleByMapNode.ContainsKey(mnVisible))
                    continue;

                dictVisibleByMapNode.Add(mnVisible, true);
            }

            Point tileExtents = map.getTileExtents();

            // TODO: Move to MapWindow?
            // Figure out the UL and LR row and column we need to draw.
            Vector2 v2WUL = screenToWorld(Vector2.Zero);
            Vector2 v2WLR = screenToWorld(new Vector2(IntoTheNewWorld.Instance.graphics.PreferredBackBufferWidth, IntoTheNewWorld.Instance.graphics.PreferredBackBufferHeight));
            Vector2 v2WLL = screenToWorld(new Vector2(0, IntoTheNewWorld.Instance.graphics.PreferredBackBufferHeight));
            Vector2 v2WUR = screenToWorld(new Vector2(IntoTheNewWorld.Instance.graphics.PreferredBackBufferWidth, 0));

            Vector2 v2GUL = this.map.worldToGrid(v2WUL, true);
            Vector2 v2GLR = this.map.worldToGrid(v2WLR, true);
            Vector2 v2GLL = this.map.worldToGrid(v2WLL, true);
            Vector2 v2GUR = this.map.worldToGrid(v2WUR, true);

            int startRow, endRow, startColumn, endColumn;
#if false
            // NOTE: When (if) we support rotation this will need to change.
            startRow = Math.Max(0, (int)v2GUL.Y - 1);
            endRow = Math.Min(this.map.height, (int)v2GLR.Y + 1);
            if (startRow > endRow)
            {
                int temp = startRow;
                startRow = endRow;
                endRow = temp;
            }
            startColumn = Math.Max(0, (int)v2GUL.X - 1);
            endColumn = Math.Min(this.map.width, (int)v2GLR.X + 1);
            if (startColumn > endColumn)
            {
                int temp = startColumn;
                startColumn = endColumn;
                endColumn = temp;
            }
#else
            List<int> rows = new List<int>(4);
            rows.Add((int)v2GUL.Y);
            rows.Add((int)v2GLR.Y);
            rows.Add((int)v2GLL.Y);
            rows.Add((int)v2GUR.Y);
            rows.Sort();
            startRow = Math.Max(0, rows[0] - 1);
            endRow = Math.Min(this.map.height, rows[3] + 1);

            List<int> columns = new List<int>(4);
            columns.Add((int)v2GUL.X);
            columns.Add((int)v2GLR.X);
            columns.Add((int)v2GLL.X);
            columns.Add((int)v2GUR.X);
            columns.Sort();
            startColumn = Math.Max(0, columns[0] - 1);
            endColumn = Math.Min(this.map.width, columns[3] + 1);
#endif

            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, null, null, null, null, mxTWorldToScreen);

            for (int row = startRow; row <= endRow; row++)
                for (int column = startColumn; column <= endColumn; column++)
                {
                    float layer = 1.0f; // 1 = back, 0 = front

                    Map.MapNode mnRC = this.map.getMapNodeG(new Vector2(column, row));
                    if (mnRC == null) // TODO: Do I really want to do this?
                        continue;

                    Visibility visibility = getVisibility(mnRC, dictVisibleByMapNode);
                    if (visibility == Visibility.Unexplored)
                        continue;

                    bool visible = (visibility == Visibility.Visible);

                    Color color = Color.White;
                    //if (!visible)
                    //    color = Color.DarkGray;

                    Point tileXY = map.getULPixel(row, column);

                    MapTile tile = this.map.getTile(new Vector2(column, row));
                    if (tile == null)
                        continue;

                    // Draw the base
                    Terrain terrain = tile.terrain;
                    Graphic graphic;

                    bool drawBases = true;
                    if (this.scale >= 0.5f)
                    {
#if PARCHMENT
                        drawBases = false;
#else
                        drawBases = true;
#endif
                    }

                    if (drawBases)
                    {
                        if (this.hexBased)
                            graphic = IntoTheNewWorld.Instance.dictGraphics["hex"];
                        else
                            graphic = IntoTheNewWorld.Instance.dictGraphics["square"];

                        //Color cBase = (terrain.identifier == "ocean") ? cOcean : cLand;
                        Color cBase = cLand;
                        if (terrain.majorType == Terrain.TerrainMajorType.Ocean)
                        {
                            if (visible)
                                cBase = cOcean;
                            if (this.scale != 1.0f)
                                cBase = cOcean;
                        }
                        else if (terrain.majorType == Terrain.TerrainMajorType.Desert)
                            cBase = Color.Yellow;
                        else if (terrain.majorType == Terrain.TerrainMajorType.Forest)
                            cBase = Color.ForestGreen;
                        else if (terrain.majorType == Terrain.TerrainMajorType.Hills)
                            cBase = Color.Tan;
                        else if (terrain.majorType == Terrain.TerrainMajorType.Lake)
                            cBase = Color.LightBlue;
                        else if (terrain.majorType == Terrain.TerrainMajorType.Mountains)
                            cBase = Color.Gray;
                        else if (terrain.majorType == Terrain.TerrainMajorType.Plains)
                            cBase = cLand;
                        else if (terrain.majorType == Terrain.TerrainMajorType.Swamp)
                            cBase = Color.YellowGreen;

                        graphic.Draw(gameTime, gameState, spriteBatch, new Rectangle(tileXY.X, tileXY.Y, tileExtents.X, tileExtents.Y), new Color(cBase.ToVector3() * color.ToVector3()), 0.0f, Vector2.Zero, layer);
                        //graphic.Draw(gameTime, gameState, spriteBatch, IntoTheNewWorld.Instance, new Rectangle(tileXY.X, tileXY.Y, tileExtents.X, tileExtents.Y));
                    }

                    // Draw the grid, if requested
                    if (this.drawGrid)
                    {
                        layer -= 0.01f;

                        int sideLen;
                        if (this.hexBased)
                            sideLen = 50; // TODO: Remove hardcoded
                        else
                            sideLen = 75; // TODO: Remove hardcoded

                        Graphic gSmallSquare = IntoTheNewWorld.Instance.dictGraphics["small_square"];
                        drawBorder(row, column, gSmallSquare, new Frame(new Rectangle(0, 0, 4, sideLen), new Point(0, 0), 1), Color.Black, Side.All, spriteBatch, layer, gameTime, gameState);
                    }

                    // Draw the terrain on top of the base                    
                    if (this.scale >= 0.5f)
                    {
                        layer -= 0.01f;

                        // TODO: Should probably have a "supplemental" gameState?  Or the ability to set stuff then remove it?
                        gameState.setValue("density", tile.density);
                        if (visible)
                            gameState.setValue("visible", 1);
                        else
                            gameState.setValue("visible", 0);

                        // NOTE: Enable this is ever need to see all squares as if visible, no gray (non-visible) areas.
                        //gameState.setValue("visible", 1);

                        WorldModel.Season season = IntoTheNewWorld.Instance.world.getEffectiveSeason(column, row, IntoTheNewWorld.Instance.date.DayOfYear);
                        string seasonString;
                        if (season == WorldModel.Season.Spring)
                            seasonString = "spring";
                        else if (season == WorldModel.Season.Summer)
                            seasonString = "summer";
                        else if (season == WorldModel.Season.Fall)
                            seasonString = "fall";
                        else
                            seasonString = "winter";
                        gameState.setValue("season", seasonString);

                        graphic = getGraphic(terrain);
                        if (graphic != null)
                        {
                            Frame frameTerrain = graphic.getCurrentFrame(gameTime, gameState);
                            Vector2 v2WFrameBounds = assetToWorld(new Vector2(frameTerrain.bounds.Width, frameTerrain.bounds.Height));
                            Vector2 tileCXY = map.getCenterW(row, column);
                            Rectangle rectTerrain = new Rectangle((int)(tileCXY.X - (v2WFrameBounds.X / 2)), (int)(tileCXY.Y - (v2WFrameBounds.Y / 2)), (int)v2WFrameBounds.X, (int)v2WFrameBounds.Y);
                            if (frameTerrain.anchor.X != 0) // TODO:AA: What's the point of this?
                                rectTerrain = new Rectangle((int)(tileCXY.X - (tileExtents.X / 2)), (int)(tileCXY.Y - (tileExtents.Y / 2)), (int)v2WFrameBounds.X, (int)v2WFrameBounds.Y);
                            graphic.Draw(frameTerrain, spriteBatch, rectTerrain, color, 0.0f, new Vector2(frameTerrain.anchor.X, frameTerrain.anchor.Y), layer);
                        }
                    }

                    // Draw rivers, coasts, and other borders...
                    if ((tile.borders != null) && (this.scale >= 0.5f))
                    {
                        layer -= 0.01f;

                        int visibleOriginal;
                        bool restoreVisible = gameState.getValue("visible", out visibleOriginal);

                        foreach (Side side in tile.borders.Keys)
                        {
                            Map.MapNode mnSide = map.getConnectedNode(mnRC, side);
                            Visibility visibilitySide = getVisibility(mnSide, dictVisibleByMapNode);
                            if (visibilitySide == Visibility.Unexplored)
                                continue;

                            if (visibilitySide == Visibility.NotVisible)
                                gameState.setValue("visible", 0);
                            else if (visibilitySide == Visibility.Visible)
                                gameState.setValue("visible", 1);

                            Graphic gBorder = tile.borders[side];
                            Frame frmBorder = gBorder.getCurrentFrame(gameTime, gameState);

                            drawBorder(row, column, gBorder, frmBorder, color, side, spriteBatch, layer, gameTime, gameState);
                        }

                        if (restoreVisible)
                            gameState.setValue("visible", visibleOriginal);
                    }
                }

            spriteBatch.End();



            //spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, null, null, null, null, mxTScaleOnly);
#if SCALE_ONLY
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, mxTScaleOnly); // TODO:AA: Re-enable
#else
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, mxTWorldToScreen); // TODO:AA: testing
#endif

            // Add map objects (towns, parked boats, etc.)
            List<MapObject> mapObjects = new List<MapObject>();
            for (int row = startRow; row <= endRow; row++)
                for (int col = startColumn; col <= endColumn; col++)
                {
                    Map.MapNode mn = this.map.getMapNode(map.gridToWorld(new Vector2(col, row)));
                    if (mn == null) // TODO: Do I really want to do this?
                        continue;

                    if (!IntoTheNewWorld.Instance.players[0].seen.ContainsKey(mn))
                        continue;

                    MapTile mapTile = this.map.getTile(new Vector2(col, row));

                    if (mapTile.mapObjects == null)
                        continue;

                    mapObjects.AddRange(mapTile.mapObjects);
                }

            // Add Europe
            mapObjects.Add(IntoTheNewWorld.Instance.europe);

            // Add PoIs
            foreach (PointOfInterest poi in IntoTheNewWorld.Instance.PoIs)
            {
                if (poi.claimed)
                    continue;

                mapObjects.Add(poi);
            }

            // Add caches
            mapObjects.AddRange(IntoTheNewWorld.Instance.caches.Cast<MapObject>());

            // Add ships
            mapObjects.AddRange(IntoTheNewWorld.Instance.parkedShips.Cast<MapObject>());

            // Add NPCs
            mapObjects.AddRange(IntoTheNewWorld.Instance.NPCs.Cast<MapObject>());

            // Add cities
            //foreach (City city in IntoTheNewWorld.Instance.cities)
            //    mapObjects.Add(new MapObject(IntoTheNewWorld.Instance.dictGraphics["native_city"], city.pos));

            Explorer player = IntoTheNewWorld.Instance.players[0];
            mapObjects.Add(player);

            Hotspot cmdHotspot = null;
            MapObject cmdMapObject = null;
            Vector2 cmdV2SPos = Vector2.Zero;
            Color cmdColor = Color.White;

            // Render map objects (unless zoomed out)
            int zorder = 0;
            foreach (MapObject mapObject in mapObjects)
            {
                // TODO:AA: Support multiple scales?
                //if (this.scale != 1.0f)
                //    continue;

                Map.MapNode mn = map.getMapNode(mapObject.positionWorld);
                if (mn == null) // TODO: Do I really want to do this?
                    continue;

                if (!IntoTheNewWorld.Instance.players[0].seen.ContainsKey(mn))
                    continue;

                Color color = Color.White;
                if (!dictVisibleByMapNode.ContainsKey(mn))
                {
                    //if (mapObject.needsLoS) // DRAW
                    //    continue;
                    continue;

                    color = Color.DarkGray;
                }

                // Get the screen position of the map object.
                Vector2 v2SPos = worldToScreen(mapObject.positionWorld);

                MapObjectRendered mapObjectRendered = this.getMapObjectRendered(mapObject);

                mapObjectRendered.graphic = IntoTheNewWorld.Instance.dictGraphics[mapObject.getGraphicIdentifier()];

                // Draw the map object.
#if SCALE_ONLY
                mapObjectRendered.Draw(gameTime, gameState, spriteBatch, v2SPos, IntoTheNewWorld.Instance);
#else
                mapObjectRendered.Draw(gameTime, gameState, spriteBatch, mapObject.positionWorld, IntoTheNewWorld.Instance);
#endif

                // Add a hotspot for the map object to the window, if it has a hotspot.
                Hotspot hotspot = mapObjectRendered.hotspot;
                if (hotspot != null)
                {
                    hotspot.zorder = zorder;
                    zorder++;
                    this.hotspots.Add(hotspot);
                }

                if (this.primaryFocusHotspot == hotspot)
                {
                    cmdHotspot = hotspot;
                    cmdMapObject = mapObject;
                    cmdV2SPos = v2SPos;
                    cmdColor = color;
                }
            }

            spriteBatch.End();

            // TODO:AA: Support multiple scales?
            if ((cmdHotspot != null) && (!IntoTheNewWorld.Instance.players[0].isDead()))
            {
                Hotspot hotspot = cmdHotspot;
                MapObject mapObject = cmdMapObject;
                Vector2 v2SPos = cmdV2SPos;
                Color color = cmdColor;

                List<Command> commands = mapObject.getCommands(IntoTheNewWorld.Instance._map, IntoTheNewWorld.Instance.players[0], 1);

                if ((commands != null) && (commands.Count > 0))
                {
                    Graphic gCommandBG = IntoTheNewWorld.Instance.dictGraphics["command"];
                    Frame fCommandBG = gCommandBG.getCurrentFrame(gameTime, gameState);
                    Frame frame = fCommandBG;

                    foreach (Command command in commands)
                    {
                        if (_dictCommandsBySlot.ContainsKey(command.slot))
                            continue;

                        _dictCommandsBySlot.Add(command.slot, command);

                        Vector2 v2SCommandPos = new Vector2(v2SPos.X, v2SPos.Y);

                        int offset = 50;
                        if (command.slot == Command.Slot.A)
                            v2SCommandPos += new Vector2(0, offset);
                        else if (command.slot == Command.Slot.B)
                            v2SCommandPos += new Vector2(offset, 0);
                        else if (command.slot == Command.Slot.Y)
                            v2SCommandPos += new Vector2(0, -offset);
                        else if (command.slot == Command.Slot.X)
                            v2SCommandPos += new Vector2(-offset, 0);

                        // Draw the background.
                        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, mxTScaleOnly);
                        gCommandBG.Draw(fCommandBG, spriteBatch, new Rectangle((int)v2SCommandPos.X, (int)v2SCommandPos.Y, fCommandBG.bounds.Width, fCommandBG.bounds.Height), color, 0.0f, new Vector2(frame.anchor.X, frame.anchor.Y), 0.1f);
                        spriteBatch.End();

                        // Draw the command.
                        Graphic gCommand = IntoTheNewWorld.Instance.dictGraphics[command.identifier];
                        Frame fCommand = gCommand.getCurrentFrame(gameTime, gameState);

                        // Update the command button in that slot.
                        Button commandButton = _dictCommandButtonsBySlot[command.slot];
                        Rectangle commandButtonBounds = new Rectangle((int)v2SCommandPos.X, (int)v2SCommandPos.Y, fCommand.bounds.Width, fCommand.bounds.Height);
                        commandButtonBounds.Offset(-fCommand.anchor.X, -fCommand.anchor.Y);
                        commandButton.bounds = commandButtonBounds;
                        commandButton.enabled = true;
                        commandButton.graphic = gCommand;
                        commandButton.parentHotspot = hotspot;
                        commandButton.Draw(gameTime, gameState, spriteBatch, IntoTheNewWorld.Instance, mxTScaleOnly);
                        commandButton.zorder = int.MaxValue; // commands should always be a higher zorder than anything else

                        this.hotspots.Add(commandButton);
                    }
                }
            }

            // Render UI
            spriteBatch.Begin();
            Rectangle tsa = IntoTheNewWorld.Instance.rectTileSafeArea;

#if false
            // TODO: Do more elegantly!
            //int maxFame = 10 * (this.map.width * this.map.height);
            spriteBatch.DrawString(font, "Food remaining: " + IntoTheNewWorld.Instance.playerState.food, new Vector2(tsa.Left + 10, tsa.Top + 10), Color.White);
            spriteBatch.DrawString(font, "Eaten per day: " + IntoTheNewWorld.Instance.foodperday, new Vector2(tsa.Left + 10, tsa.Top + 25), Color.White);
            spriteBatch.DrawString(font, "Foraged per day: " + (int)(IntoTheNewWorld.Instance.forageperday * this.map.getTile(mnPlayer).terrain.forageModifier), new Vector2(tsa.Left + 10, tsa.Top + 40), Color.White);
#if WINDOWS
            spriteBatch.DrawString(font, "Left mouse button moves, death at 0 food, go to east side of map for food and to convert discovery to fame, 'F' for food, 'R' resets, 'C' centers view, 'Insert' and 'Delete' zooms.", new Vector2(tsa.Left + 10, tsa.Bottom - 25), Color.White);
#elif WINDOWS_PHONE
            spriteBatch.DrawString(font, "Touch drag moves, death at 0 food, go to east side of map for food and to convert discovery to fame, pinch zooms.", new Vector2(tsa.Left + 10, tsa.Bottom - 25), Color.White);
#elif XBOX360
            spriteBatch.DrawString(font, "Left stick moves, death at 0 food, go to east side of map for food and to convert discovery to fame.", new Vector2(tsa.Left + 10, tsa.Bottom - 25), Color.White);
#endif
#endif

            int itemXGutter = 20;
            int itemX = tsa.X + 10;
            int itemY = tsa.Y + 10;
            //string[] items = { "food", "gold" };
            string[] items = { "men", "food", "ships", "gold", "oldworld_goods", "newworld_goods", "horses", "weapons" };
            int itemIdx = 0;
            foreach (string item in items)
            {
                Graphic gItem = IntoTheNewWorld.Instance.dictGraphics["trade_" + item];
                Frame fItem = gItem.getCurrentFrame(gameTime, gameState);
                gItem.Draw(fItem, spriteBatch, new Point(itemX, itemY));
                int itemQuantity = IntoTheNewWorld.Instance.players[0].state.getValue<int>(item);
                string sItem = "" + itemQuantity;
                if (item == "food")
                    sItem = IntoTheNewWorld.Instance.totalFoodToWeeks(itemQuantity, IntoTheNewWorld.Instance.players[0].state.getValue<int>("men")) + "w";
                Vector2 v2ItemSize = font.MeasureString(sItem);
                spriteBatch.DrawString(font, sItem, new Vector2(itemX + (fItem.bounds.Width / 2) - ((int)v2ItemSize.X / 2), itemY + fItem.bounds.Height), Color.White);
                itemX += (fItem.bounds.Width + itemXGutter);

                if (itemIdx == ((items.Length / 2) - 1))
                {
                    itemX = tsa.X + 10;
                    itemY = tsa.Y + 10 + fItem.bounds.Height + (int)v2ItemSize.Y + itemXGutter;
                }

                itemIdx++;
            }

            Graphic gDays = IntoTheNewWorld.Instance.dictGraphics["days"];
            Frame fDays = gDays.getCurrentFrame(gameTime, gameState);
            gDays.Draw(fDays, spriteBatch, new Point(tsa.X + (tsa.Width / 2) - (fDays.bounds.Width / 2), tsa.Y + 10));
            string sDays = "" + IntoTheNewWorld.Instance.days;
            string sDate = "" + IntoTheNewWorld.Instance.date.ToString("d");
            Vector2 v2DaysSize = font.MeasureString(sDays);
            Vector2 v2DateSize = font.MeasureString(sDate);
            //spriteBatch.DrawString(font, sDays, new Vector2(tsa.X + (tsa.Width / 2) - ((int)v2DaysSize.X / 2), tsa.Y + 10 + fDays.bounds.Height), Color.White);
            spriteBatch.DrawString(font, sDate, new Vector2(tsa.X + (tsa.Width / 2) - ((int)v2DateSize.X / 2), tsa.Y + 10 + fDays.bounds.Height), Color.White);

            int iconY = tsa.Y + 10;
            int iconXGutter = 20;
            int iconRightX = tsa.X + tsa.Width - 10;

            //string[] icons = { "discovery" };
            string[] icons = { "recognition", "discovery", "relations" };

            int iconLeftX = iconRightX;
            for (int i = 0; i < icons.Length; i++)
            {
                Graphic gIcon = IntoTheNewWorld.Instance.dictGraphics[icons[i]];
                Frame fIcon = gIcon.getCurrentFrame(gameTime, gameState);
                iconLeftX -= fIcon.bounds.Width;
                gIcon.Draw(fIcon, spriteBatch, new Point(iconLeftX, iconY));

                int textCenter = iconLeftX + (fIcon.bounds.Width / 2);
                string sIcon = "";
                if (icons[i] == "recongition")
                    sIcon += IntoTheNewWorld.Instance.players[0].state.getValue<int>("recognition");
                else if (icons[i] == "discovery")
                    sIcon += IntoTheNewWorld.Instance.players[0].state.getValue<int>("discovery");
                else if (icons[i] == "relations")
                    sIcon += IntoTheNewWorld.Instance.players[0].state.getValue<int>("native_like");
                Vector2 v2TextSize = font.MeasureString(sIcon);
                spriteBatch.DrawString(font, sIcon, new Vector2(textCenter - (v2TextSize.X / 2), iconY + fIcon.bounds.Height), Color.White);

                iconLeftX -= iconXGutter;
            }

            // TODO: Need to, based on the intended lat, long span of the world, decide on number of tiles and make sure that they are in
            //       correct ratio to each other.
    
            //spriteBatch.DrawString(font, "Discovery: " + IntoTheNewWorld.Instance.playerState.discovery /* + " (" + (int)(((float)IntoTheNewWorld.Instance.playerState.discovery / (float)maxFame) * 100) + "%)" */, new Vector2(tsa.Right - 200 + 10, tsa.Top + 10), Color.White);
            //spriteBatch.DrawString(font, "Fame: " + IntoTheNewWorld.Instance.playerState.fame /* + " (" + (int)(((float)IntoTheNewWorld.Instance.playerState.fame / (float)maxFame) * 100) + "%)" */, new Vector2(tsa.Right - 200 + 10, tsa.Top + 25), Color.White);
            //spriteBatch.DrawString(font, "(+10 each tile uncovered)", new Vector2(tsa.Right - 200 + 10, tsa.Top + 40), Color.White);
            //spriteBatch.DrawString(font, "Native faction (-100 to 100): " + IntoTheNewWorld.Instance.players[0].state.getValue<int>("native_like"), new Vector2(tsa.Left + (tsa.Width / 2), tsa.Bottom - 25), Color.White);
            Vector2 v2GPlayerPos = map.worldToGrid(player.positionWorld);
            WorldModel.WorldLocation worldLocation = IntoTheNewWorld.Instance.world.getWorldLocation((int)v2GPlayerPos.X, (int)v2GPlayerPos.Y);
            string latitudeString = "0";
            if (worldLocation.latitude < 0)
                latitudeString = (Math.Abs(worldLocation.latitude) + "S");
            else if (worldLocation.latitude > 0)
                latitudeString = (worldLocation.latitude + "N");
            string longitudeString = "0";
            if (worldLocation.longitude < 0)
                longitudeString = (Math.Abs(worldLocation.longitude) + "W");
            else if (worldLocation.longitude > 0)
                longitudeString = (worldLocation.longitude + "E");
            spriteBatch.DrawString(font, "(r: " + v2GPlayerPos.Y + ", c: " + v2GPlayerPos.X + "), (" + latitudeString + ", " + longitudeString + "), seed: " + IntoTheNewWorld.Instance.mapSeed + ", fps: " + IntoTheNewWorld.Instance.frameRate, new Vector2(tsa.Left + 10, tsa.Bottom - 25), Color.White);

#if false
            Curve curveX = new Curve();
            Curve curveY = new Curve();
            for (int i = 0; i <= 5; i++)
            {
                Vector2 v2SPlayerPos = this.worldToScreen(player.positionWorld);
                curveX.Keys.Add(new CurveKey(0.0f + (0.2f * i), v2SPlayerPos.X + (50 * i)));
                curveY.Keys.Add(new CurveKey(0.0f + (0.2f * i), v2SPlayerPos.Y - (25 * i)));
            }
            string scurve = "iiiiiiiiiiii";
            for (int i = 0; i < scurve.Length; i++)
                spriteBatch.DrawString(font, "" + scurve[i], new Vector2(curveX.Evaluate((float)i / (float)(scurve.Length - 1)), curveY.Evaluate((float)i / (float)(scurve.Length - 1))), Color.Red);
#endif

            //Graphic gForest = IntoTheNewWorld.Instance.dictGraphics["forest"];
            //Frame fForest = gForest.getCurrentFrame(gameTime, gameState);
            //spriteBatch.Draw(IntoTheNewWorld.Instance.getTexture(gForest), new Rectangle(10, 10, fForest.bounds.Width, fForest.bounds.Height), fForest.bounds, Color.White);

            spriteBatch.End();
        }

        void commandButton_Press(object sender, PressEventArgs e)
        {
            Command.Slot slot = _dictCommandButtonsBySlot.Keys.First(key => _dictCommandButtonsBySlot[key] == sender);
            Command command = _dictCommandsBySlot[slot];

            // TODO: Should we have a separate handler for "command_cache" as well as a "command_loot"?
            if ((command.identifier == "command_trade") || (command.identifier == "command_cache"))
            {
                VariableBundle leftTrader = IntoTheNewWorld.Instance.players[0].state;
                VariableBundle rightTrader = command.owner.state;
                TradeWindow.TradingPartner tradingPartner = TradeWindow.TradingPartner.Native;
                if (command.owner is Explorer)
                    tradingPartner = TradeWindow.TradingPartner.Explorer;
                else if (command.owner is IntoTheNewWorldCache)
                    tradingPartner = (((IntoTheNewWorldCache)command.owner).isLoot ? TradeWindow.TradingPartner.Loot : TradeWindow.TradingPartner.Cache);
                //string ownerIdentifier = command.owner.getGraphicIdentifier();
                //if (command.owner is Mob)
                //    ownerIdentifier = ((Mob)command.owner).text;
                //else if (command.owner is IntoTheNewWorldCache)
                //    ownerIdentifier = (((IntoTheNewWorldCache)command.owner).isLoot ? "loot" : "cache");
                string ownerIdentifier = command.owner.text;

                // TODO: Is there a better way?  How will we allow NPC caches with code like this?!
                if (command.identifier == "command_cache")
                {
                    IntoTheNewWorldCache cache = new IntoTheNewWorldCache(command.owner.positionWorld, command.owner, false);
                    IntoTheNewWorld.Instance.caches.Add(cache);
                    rightTrader = cache.state;
                    tradingPartner = TradeWindow.TradingPartner.Cache;
                    //ownerIdentifier = cache.getGraphicIdentifier();
                    ownerIdentifier = cache.text;
                }

                // TODO: Method call for minimum food?
                VariableBundle rightTraderCopy = rightTrader.copy();
                if (command.owner is City)
                    rightTraderCopy.setValue("food", IntoTheNewWorld.Instance.weeksToTotalFood(20, rightTrader.getValue<int>("men")));
                else if (command.owner is Explorer)
                    rightTraderCopy.setValue("food", IntoTheNewWorld.Instance.weeksToTotalFood(26, rightTrader.getValue<int>("men")));
                else if (command.owner is IntoTheNewWorldMob)
                    rightTraderCopy.setValue("food", IntoTheNewWorld.Instance.weeksToTotalFood(10, rightTrader.getValue<int>("men")));

                TradeWindow tradeWindow = new TradeWindow(this.center, 800, leftTrader, rightTrader, tradingPartner, 1, IntoTheNewWorld.Instance.players[0].text, ownerIdentifier);
                tradeWindow.background = IntoTheNewWorld.Instance.windowBackground;
                tradeWindow.decorations = IntoTheNewWorld.Instance.windowDecorations;

                //TradeWindow tradeWindow = new TradeWindow(new Rectangle(200, 25, 800, 570), leftTrader, rightTrader, TradeWindow.TradingPartner.Cache, 1);
                IntoTheNewWorld.Instance.Show(tradeWindow);
            }
            else if (command.identifier == "command_claim")
            {
                PointOfInterest poi = (PointOfInterest)command.owner;

                showMessageBox(poi.claimMessage);

                new ResolvedAction(IntoTheNewWorld.Instance.actionsByIdentifier["discover"], IntoTheNewWorld.Instance.players[0], poi).execute(-1);
            }
            else if (command.identifier == "command_embark")
            {
                Ship ship = IntoTheNewWorld.Instance.parkedShips.FirstOrDefault(s => s.positionWorld == command.owner.positionWorld);
                if ((ship != default(Ship)) && ship.parked)
                    new ResolvedAction(IntoTheNewWorld.Instance.actionsByIdentifier["board_boat"], IntoTheNewWorld.Instance.players[0], ship).execute(-1);
            }
            else if (command.identifier == "command_disembark")
                new ResolvedAction(IntoTheNewWorld.Instance.actionsByIdentifier["leave_boat"], IntoTheNewWorld.Instance.players[0], null).execute(-1);
            else if (command.identifier == "command_fight")
            {
                CombatWindow combatWindow = new CombatWindow(this.center, 0, IntoTheNewWorld.Instance.players[0], command.owner);
                combatWindow.background = IntoTheNewWorld.Instance.windowBackground;
                combatWindow.decorations = IntoTheNewWorld.Instance.windowDecorations;
                IntoTheNewWorld.Instance.Show(combatWindow);
            }
            else if (command.identifier == "command_europe")
            {
                if (IntoTheNewWorld.Instance.players[0].state.getValue<int>("discovery") == 0)
                    showMessageBox("But you've only just left Europe!\n\nTake the time to discover the New World\nand then return to Europe for hard-earned\nrecognition and to end the game.");
                else
                {
                    new ResolvedAction(IntoTheNewWorld.Instance.actionsByIdentifier["return_to_europe"], IntoTheNewWorld.Instance.players[0], null).execute(-1);

                    showMessageBox("You have returned to Europe!\n\nWith great fanfare you present your\ndiscoveries to the royal court.\n" + IntoTheNewWorld.Instance.getScoreString(), exitGameCallback);
                }
            }
            else if (command.identifier == "command_journal")
                showMessageBox("This is your journal!");
        }

        void exitGameCallback(object sender, PressEventArgs e)
        {
            IntoTheNewWorld.Instance.quit();
        }

        public void showMessageBox(string text)
        {
            showMessageBox(text, null);
        }

        public void showMessageBox(string text, EventHandler<PressEventArgs> OnOK)
        {
            MessageBox messageBox = new MessageBox(text, _font, this.center, IntoTheNewWorld.Instance, OnOK);
            messageBox.background = IntoTheNewWorld.Instance.windowBackground;
            messageBox.decorations = IntoTheNewWorld.Instance.windowDecorations;
            IntoTheNewWorld.Instance.Show(messageBox);
        }

        protected override MapObjectRendered createMapObjectRendered(MapObject mapObject)
        {
            return new MapObjectToken(mapObject);
        }

        private Graphic getGraphic(Terrain terrain)
        {
            //return IntoTheNewWorld.Instance.dictGraphics[terrain.identifier];

            Graphic graphic;
            IntoTheNewWorld.Instance.dictGraphics.TryGetValue(terrain.graphicIdentifier, out graphic);
            return graphic;
        }

        private void drawBorder(int row, int column, Graphic texture, Frame frame, Color color, Side side, SpriteBatch spriteBatch, float layer, GameTime gameTime, VariableBundle gameState)
        {
            MapGrid<MapTile> map = this.map;
            bool useHexes = this.hexBased;

            //int sideLen = map.getTileSideLength();
            int radius = map.getTileRadius();
            int radiusA = (int)worldToAsset(new Vector2(radius, 0.0f)).X;

            int numSides = 6;
            if (!useHexes)
                numSides = 4;

            for (int i = 0; i < numSides; i++)
            {
                if (side != Side.All)
                {
                    if (!useHexes)
                    {
                        if ((side == Side.East) && (i != 0))
                            continue;
                        else if ((side == Side.South) && (i != 1))
                            continue;
                        else if ((side == Side.West) && (i != 2))
                            continue;
                        else if ((side == Side.North) && (i != 3))
                            continue;
                    }
                    else
                    {
                        if ((side == Side.East) && (i != 0))
                            continue;
                        else if ((side == Side.SouthEast) && (i != 1))
                            continue;
                        else if ((side == Side.SouthWest) && (i != 2))
                            continue;
                        else if ((side == Side.West) && (i != 3))
                            continue;
                        else if ((side == Side.NorthWest) && (i != 4))
                            continue;
                        else if ((side == Side.NorthEast) && (i != 5))
                            continue;
                    }
                }

                // Calculate the destination rectangle.  This is in world coordinates.
                Vector2 rcenterWorldPosition = map.getCenterW(row, column);
                Vector2 v2WFrame = assetToWorld(new Vector2(frame.bounds.Width, frame.bounds.Height));
                Rectangle rectangleDest = new Rectangle((int)rcenterWorldPosition.X, (int)rcenterWorldPosition.Y, (int)v2WFrame.X, /*this.map.getTileSideLength()*/ (int)v2WFrame.Y);

                // Calculate the radians for the rotation.
                double degrees = i * (360.0 / (double)numSides);
                double radians = (Math.PI * degrees) / 180.0;

                // Calculate the origin of the rotation.  This is in TEXTURE coordinates.
                Vector2 origin = new Vector2(-(float)radiusA + ((float)frame.bounds.Width / 2.0f), (float)frame.bounds.Height / 2.0f);

                texture.Draw(frame, spriteBatch, rectangleDest, color, (float)radians, origin, layer);
            }
        }
    }
}
