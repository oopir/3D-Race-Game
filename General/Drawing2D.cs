using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using VPNT = Microsoft.Xna.Framework.Graphics.VertexPositionNormalTexture;

namespace Race
{
    class Basic2D
    {
        public Texture2D texture;
        public Rectangle rec;

        /// <summary>
        /// Class constructor. Initializes class variables
        /// </summary>
        /// <param name="texture">texture of object</param>
        /// <param name="rec">destination rectangle of object</param>
        public Basic2D(Texture2D texture, Rectangle rec)
        {
            this.texture = texture;
            this.rec = rec;
        }

        /// <summary>
        /// draws the object into the screen.
        /// </summary>
        public void Draw()
        {
            Game1.spriteBatch.Draw(texture, rec, Color.White);
        }
    }

    class Complex2D
    {
        public Texture2D texture;
        public Vector2 position;
        Rectangle? sourceRec;
        Color color;
        public float rotation;
        public Vector2 origin;
        Vector2 scale;
        SpriteEffects effect;
        float layer;

        /// <summary>
        /// Class Constructor. Initializes all variables.
        /// </summary>
        /// <param name="texture">object's texture to draw</param>
        /// <param name="position">position to draw the object in</param>
        /// <param name="sourceRec">a rectangle describing which part of the texture to draw</param>
        /// <param name="color">color to multiply the texture's colors with before drawing</param>
        /// <param name="rotation">an angle to rotate the texture in before drawing</param>
        /// <param name="origin">the pixel of the texture that will be drawn in the point "position"</param>
        /// <param name="scale">the amount ot straching/shrinking of the picture in every axis</param>
        /// <param name="effect">a SpriteEffect object</param>
        /// <param name="layer">layer to draw the texture in</param>
        public Complex2D(Texture2D texture, Vector2 position, Rectangle? sourceRec, Color color, float rotation,
                         Vector2 origin, Vector2 scale, SpriteEffects effect, float layer)
        {
            this.texture = texture;
            this.position = position;
            this.sourceRec = sourceRec;
            this.color = color;
            this.rotation = rotation;
            this.origin = origin;
            this.scale = scale;
            this.effect = effect;
            this.layer = layer;
            if (origin == new Vector2(-1))
                origin = new Vector2(texture.Width / 2, texture.Height / 2);
        }

        /// <summary>
        /// draws the object according to its variables
        /// </summary>
        public void Draw()
        {
            Game1.spriteBatch.Draw(texture, position, sourceRec, color, rotation, origin, scale, effect, layer);
        }

    }

    class Temporary2D : Complex2D
    {
        TimeSpan timespan;
        int seconds;
        public bool displayed = true;

        public Temporary2D(int seconds, Texture2D texture, Vector2 position, Rectangle? sourceRec, Color color, float rotation,
                     Vector2 origin, Vector2 scale, SpriteEffects effect, float layer)
            : base(texture, position, sourceRec, color, rotation, origin, scale, effect, layer)
        {
            this.seconds = seconds;
            Game1.CallDraw += Draw;
            Game1.MsgUpdate += Update;
            timespan = new TimeSpan(0);
        }

        public void Update(GameTime gameTime)
        {
            timespan += new TimeSpan(0, 0, 0, 0, (int)gameTime.ElapsedGameTime.TotalMilliseconds);

        }

        public void Draw(Effect effect)
        {
            if (timespan.Seconds > seconds - 1)
            {
                displayed = false;
                Game1.CallDraw -= Draw;
                Game1.MsgUpdate -= Update;
            }
            else
                base.Draw();
        }
    }
}