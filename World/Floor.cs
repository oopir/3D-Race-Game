using System;
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
    class Floor : Basic3D
    {
        /// <summary>
        /// Class constructor.
        /// Creates the floor grid of the world
        /// </summary>
        /// <param name="floorTex">texture of a floor</param>
        /// <param name="width">selected width of floor</param>
        /// <param name="length">selected length of floor</param>
        public Floor(Texture2D floorTex, int width, int length) : base(floorTex)
        {
            List<VPNT> vertices = new List<VPNT>();
            List<short> indices = new List<short>();
            
            // creating the vertices grid
            int yValue = 0;
            for (int z = - 50; z <= length + 50; z += 50)
            {
                int xValue = 0;
                for (int x = -50; x <= width + 50; x += 50)
                {
                    vertices.Add(new VPNT(new Vector3(x, 0, -z), new Vector3(0, 1, 0), new Vector2(xValue, yValue)));
                    xValue = xValue ^ 1;
                }
                yValue =  yValue ^ 1;
            }

            // creating the indices list to match the vertices grid
            for (int z = 0; z < length / 50 + 2; z++)
                for (int x = 0; x < width / 50 + 2; x++)
                {
                    indices.Add((short)(z * (width / 50 + 2 + 1) + x));
                    indices.Add((short)((z + 1) * (width / 50 + 2 + 1) + x));
                    indices.Add((short)(z * (width / 50 + 2 + 1) + (x + 1)));

                    indices.Add((short)((z + 1) * (width / 50 + 2 + 1) + x));
                    indices.Add((short)((z + 1) * (width / 50 + 2 + 1) + (x + 1)));
                    indices.Add((short)(z * (width / 50 + 2 + 1) + (x + 1)));
                }

            // creating the buffers
            vertexBuffer = new VertexBuffer(Game1.device, VPNT.VertexDeclaration, vertices.Count, BufferUsage.WriteOnly);
            vertexBuffer.SetData(vertices.ToArray());
            indexBuffer = new IndexBuffer(Game1.device, typeof(short), indices.Count, BufferUsage.WriteOnly);
            indexBuffer.SetData(indices.ToArray());
        }

        /// <summary>
        /// Draws the object using the given effect
        /// </summary>
        /// <param name="effect">effect to draw object with</param>
        protected override void Draw(Effect effect)
        {
            effect.CurrentTechnique = Game1.effect.Techniques["Textured"];
            effect.Parameters["xTexture"].SetValue(texture);
            effect.Parameters["xWorld"].SetValue(Matrix.CreateTranslation(new Vector3(0, -0.1f, 0))); //reason for override
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
    }
}