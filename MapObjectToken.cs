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
            SpriteEffects spriteEffects = SpriteEffects.None;
            float layerDepth = 0.0f;

            // Token frame.
            Graphic gToken = baseGame.dictGraphics["token"];
#if OLD_TEXTURE
            Texture2D t2dToken = baseGame.getTexture(gToken);
#endif
            Frame fToken = gToken.getCurrentFrame(gameTime, gameState);

            // Content frame.
            Frame frame = this.graphic.getCurrentFrame(gameTime, gameState);

            // Draw the token.
#if OLD_TEXTURE
            spriteBatch.Draw(t2dToken, new Rectangle((int)position.X, (int)position.Y, fToken.bounds.Width, fToken.bounds.Height), fToken.bounds, color, 0.0f, new Vector2(frame.anchor.X, frame.anchor.Y), spriteEffects, layerDepth + 0.1f);
#else
            gToken.Draw(gameTime, gameState, spriteBatch, new Rectangle((int)position.X, (int)position.Y, fToken.bounds.Width, fToken.bounds.Height), color, 0.0f, new Vector2(frame.anchor.X, frame.anchor.Y), layerDepth);
#endif

            base.Draw(gameTime, gameState, spriteBatch, position, baseGame);
        }
    }
}
