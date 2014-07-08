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

// TODO: Move to GamesLibrary eventually?
namespace IntoTheNewWorld
{
    public class WorldBuilderWindow : MapGridWindow<float>
    {
        private float _waterLevel;

        public WorldBuilderWindow(MapGrid<float> map, Rectangle bounds, Vector2 tileSize, Vector2 cameraPos, float waterLevel)
            : base(map, bounds, tileSize, cameraPos, IntoTheNewWorld.Instance)
        {
            _waterLevel = waterLevel;
        }

        public override void HandleInput(GameTime gameTime)
        {
            // TODO: Centralize this?
            double milliseconds = gameTime.ElapsedGameTime.TotalMilliseconds;
            Mob player = IntoTheNewWorld.Instance.players[0];

#if WINDOWS
            KeyboardState kbs = Keyboard.GetState();
            MouseState ms = Mouse.GetState();

            // Center the camera
            if (kbs.IsKeyDown(Keys.RightControl))
                cameraPos = player.positionWorld;
            if (ms.MiddleButton == ButtonState.Pressed)
                cameraPos = player.positionWorld;

            if (kbs.IsKeyDown(Keys.C))
            {
                cameraPos = player.positionWorld;
            }
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
                GestureSample gs = TouchPanel.ReadGesture();
                if (gs.GestureType == GestureType.FreeDrag)
                {
                    Vector2 v3TouchOffset = screenToWorld(gs.Position) - player.pos;
                    if (!IntoTheNewWorld.Instance.playerState.dead)
                        player.moveDirection(this.map, v3TouchOffset, milliseconds);
                }
                //if (gs
            }
#elif XBOX360
            GamePadState gps = GamePad.GetState(PlayerIndex.One);

            // Move the player
            if (!IntoTheNewWorld.Instance.playerState.dead)
                player.moveDirection(this.map, new Vector2(gps.ThumbSticks.Left.X, -gps.ThumbSticks.Left.Y), milliseconds);

            // Move the camera
            cameraPos += (new Vector2(gps.ThumbSticks.Right.X, -gps.ThumbSticks.Right.Y) * 10);
#endif

            base.HandleInput(gameTime);
        }

        public override void Draw(GameTime gameTime, VariableBundle gameState, SpriteBatch spriteBatch)
        {
            base.Draw(gameTime, gameState, spriteBatch);

            // Figure out the UL and LR row and column we need to draw.
            Vector2 v3ULWorld = screenToWorld(Vector2.Zero);
            Vector2 v3LRWorld = screenToWorld(new Vector2(IntoTheNewWorld.Instance.graphics.PreferredBackBufferWidth, IntoTheNewWorld.Instance.graphics.PreferredBackBufferHeight));

            Vector2 v3ULGrid = this.map.worldToGrid(v3ULWorld, true);
            Vector2 v3LRGrid = this.map.worldToGrid(v3LRWorld, true);

            Point tileExtents = map.getTileExtents();

            Color cOcean = new Color(0, 148, 255);
            Color cLand = new Color(198, 185, 117);

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, mxTWorldToScreen);

            for (int row = (int)v3ULGrid.Y; row <= (int)v3LRGrid.Y; row++)
                for (int col = (int)v3ULGrid.X; col <= (int)v3LRGrid.X; col++)
                {
                    Vector2 v3loc = this.map.gridToWorld(new Vector2(col, row));
                    Map.MapNode mn = this.map.getMapNode(v3loc);
                    if (mn == null) // TODO: Do I really want to do this?
                        continue;

                    Point tileXY = map.getULPixel(row, col);

                    float tile = this.map.getTile(new Vector2(col, row));

                    // Draw the base
                    Graphic graphic;
                    if (this.hexBased)
                        graphic = IntoTheNewWorld.Instance.dictGraphics["hex"];
                    else
                        graphic = IntoTheNewWorld.Instance.dictGraphics["square"];
                    Color cBase = (tile <= _waterLevel) ? cOcean : cLand;
                    if (cBase == cOcean)
                        tile = 1.0f;
#if OLD_TEXTURE
                    spriteBatch.Draw(IntoTheNewWorld.Instance.getTexture(graphic), new Rectangle(tileXY.X, tileXY.Y, tileExtents.X, tileExtents.Y), graphic.getCurrentFrame(gameTime, gameState).bounds, new Color(cBase.ToVector3() * new Vector3(tile)));
#else
                    graphic.Draw(gameTime, gameState, spriteBatch, new Rectangle(tileXY.X, tileXY.Y, tileExtents.X, tileExtents.Y), new Color(cBase.ToVector3() * new Vector3(tile)));
#endif
                }

            spriteBatch.End();
        }
    }
}
