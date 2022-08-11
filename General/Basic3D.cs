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
    public abstract class Basic3D
    {
        protected Texture2D texture;
        protected VertexBuffer vertexBuffer;
        protected IndexBuffer indexBuffer;

        /// <summary>
        /// assigns the object a texture and signs it to the drawing event
        /// </summary>
        /// <param name="texture">object's texture</param>
        protected Basic3D(Texture2D texture)
        {
            this.texture = texture;
            Game1.CallDraw += new Draw_Handler(Draw);
        }

        /// <summary>
        /// draws the object into the screen
        /// </summary>
        /// <param name="effect">effect to draw object with</param>
        protected virtual void Draw(Effect effect)
        {
            effect.CurrentTechnique = Game1.effect.Techniques["Textured"];
            effect.Parameters["xTexture"].SetValue(texture);
            effect.Parameters["xWorld"].SetValue(Matrix.Identity);
            effect.Parameters["xView"].SetValue(Cameras.viewMatrix);
            effect.Parameters["xProjection"].SetValue(Cameras.projectionMatrix);
            effect.Parameters["xEnableLighting"].SetValue(true);
            effect.Parameters["xLightDirection"].SetValue(Cameras.lightDirection);
            effect.Parameters["xAmbient"].SetValue(0.5f);

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                Game1.device.SetVertexBuffer(vertexBuffer);
                Game1.device.Indices = indexBuffer;
                Game1.device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertexBuffer.VertexCount, 0, indexBuffer.IndexCount);
            }
        }

        public void BeGone()
        {
            Game1.CallDraw -= this.Draw;
        }
    }
}