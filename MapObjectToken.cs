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
    public class MapObjectToken : MapObjectRendered
    {
        public MapObjectToken(MapObject mapObject) : base(mapObject) { }

        public override void Draw(GameTime gameTime, VariableBundle gameState, SpriteBatch spriteBatch, Vector2 position, BaseGame baseGame)
        {
            Color color = Color.White;
            float layerDepth = 0.0f;

            // Token frame.
            Graphic gToken = baseGame.dictGraphics["token"];
            Frame fToken = gToken.getCurrentFrame(gameTime, gameState);

            // Content frame.
            Frame frame = this.graphic.getCurrentFrame(gameTime, gameState);

            // Draw the token.
            gToken.Draw(gameTime, gameState, spriteBatch, new Rectangle((int)position.X, (int)position.Y, fToken.bounds.Width, fToken.bounds.Height), color, 0.0f, new Vector2(frame.anchor.X, frame.anchor.Y), layerDepth + 0.1f);

            base.Draw(gameTime, gameState, spriteBatch, position, baseGame);
        }
    }
}
