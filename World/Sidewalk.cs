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
    class Sidewalk : Basic3D
    {
        /// <summary>
        /// creatin the sidewalks to go along the road's sides
        /// </summary>
        /// <param name="tex">texture of sidewalk-part</param>
        /// <param name="innerBoundary">list of dots along the road''s inner boundary</param>
        /// <param name="outerBoundary">list of dots along the road''s outer boundary</param>
        public Sidewalk(Texture2D tex, List<Vector3> innerBoundary, List<Vector3> outerBoundary) : base(tex)
        {
            List<VPNT> vertices = new List<VPNT>();
            List<short> indices = new List<short>();

            // creating the vertices of one sidewalk
            int yValue = 0;
            for (int i = 0; i < innerBoundary.Count; i++)
            {
                Vector3 roadNormal = innerBoundary[i] - outerBoundary[i];
                vertices.Add(new VPNT(innerBoundary[i], new Vector3(1, 0, 0), new Vector2(1, yValue)));
                vertices.Add(new VPNT(innerBoundary[i] + new Vector3(0, 0.5f, 0), new Vector3(0.5f, 0.5f, 0), new Vector2(0.9f, yValue)));
                vertices.Add(new VPNT(innerBoundary[i] + roadNormal / 5 + new Vector3(0, 0.5f, 0), new Vector3(0, 1, 0), new Vector2(0, yValue)));

                yValue = yValue ^ 1;
            }
            vertices.Add(new VPNT(vertices[0].Position, vertices[0].Normal, new Vector2(1, yValue)));
            vertices.Add(new VPNT(vertices[1].Position, vertices[1].Normal, new Vector2(0.9f, yValue)));
            vertices.Add(new VPNT(vertices[2].Position, vertices[2].Normal, new Vector2(0, yValue)));

            // creating the indices of one sidewalk
            for (short i = 0; i < vertices.Count - 3; i += 3)
            {
                indices.Add((short)(i + 0)); indices.Add((short)(i + 3)); indices.Add((short)(i + 1));
                indices.Add((short)(i + 3)); indices.Add((short)(i + 4)); indices.Add((short)(i + 1));
                indices.Add((short)(i + 1)); indices.Add((short)(i + 4)); indices.Add((short)(i + 2));
                indices.Add((short)(i + 4)); indices.Add((short)(i + 5)); indices.Add((short)(i + 2));
            }

            // creating the vertices of the other side of the sidewalk
            yValue = 0;
            for (int i = 0; i < outerBoundary.Count; i++)
            {
                Vector3 roadNormal = outerBoundary[i] - innerBoundary[i];
                vertices.Add(new VPNT(outerBoundary[i] + roadNormal / 5 + new Vector3(0, 0.5f, 0), new Vector3(0, 1, 0), new Vector2(0, yValue)));
                vertices.Add(new VPNT(outerBoundary[i] + new Vector3(0, 0.5f, 0), new Vector3(0.5f, 0.5f, 0), new Vector2(0.9f, yValue)));
                vertices.Add(new VPNT(outerBoundary[i], new Vector3(1, 0, 0), new Vector2(1, yValue)));

                yValue = yValue ^ 1;
            }
            int offset = (vertices.Count + 3) / 2;
            vertices.Add(new VPNT(vertices[offset].Position, vertices[offset].Normal, new Vector2(1, yValue)));
            vertices.Add(new VPNT(vertices[offset + 1].Position, vertices[offset + 1].Normal, new Vector2(0.9f, yValue)));
            vertices.Add(new VPNT(vertices[offset + 2].Position, vertices[offset + 2].Normal, new Vector2(0, yValue)));

            // creating the indices of the other side of the sidewalk
            for (short i = (short)(vertices.Count / 2); i < vertices.Count - 3; i += 3)
            {
                indices.Add((short)(i + 0)); indices.Add((short)(i + 3)); indices.Add((short)(i + 1));
                indices.Add((short)(i + 3)); indices.Add((short)(i + 4)); indices.Add((short)(i + 1));
                indices.Add((short)(i + 1)); indices.Add((short)(i + 4)); indices.Add((short)(i + 2));
                indices.Add((short)(i + 4)); indices.Add((short)(i + 5)); indices.Add((short)(i + 2));
            }

            // initializing the bufers
            vertexBuffer = new VertexBuffer(Game1.device, VPNT.VertexDeclaration, vertices.Count, BufferUsage.WriteOnly);
            vertexBuffer.SetData(vertices.ToArray());
            indexBuffer = new IndexBuffer(Game1.device, typeof(short), indices.Count, BufferUsage.WriteOnly);
            indexBuffer.SetData(indices.ToArray());

        }
    }
}