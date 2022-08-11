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
    class FinishLine : Basic3D
    {
        public FinishLine(Texture2D texture, List<Vector3> vectors)
            : base(texture)
        {
            List<VPNT> vertices = new List<VPNT>();
            Vector3 normal = new Vector3(0, 1, 0);
            Vector3 boost = new Vector3(0, 0.01f, 0);
            vertices.Add(new VPNT(vectors[0] + boost, normal, new Vector2(0, 0)));
            vertices.Add(new VPNT(vectors[1] + boost, normal, new Vector2(0, 1)));
            vertices.Add(new VPNT(vectors[2] + boost, normal, new Vector2(1, 0)));
            vertices.Add(new VPNT(vectors[3] + boost, normal, new Vector2(1, 1)));

            List<short> indices = (new short[] { 0, 1, 2, 2, 1, 3 }).ToList();

            vertexBuffer = new VertexBuffer(Game1.device, VPNT.VertexDeclaration, vertices.Count, BufferUsage.WriteOnly);
            vertexBuffer.SetData(vertices.ToArray());
            indexBuffer = new IndexBuffer(Game1.device, typeof(short), indices.Count, BufferUsage.WriteOnly);
            indexBuffer.SetData(indices.ToArray());
        }
    }
}