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
    public class Track : Basic3D
    {
        public List<Vector3> outerBoundary;
        public List<Vector3> innerBoundary;
        public static List<BoundingBox> boundingBoxes;

        /// <summary>
        /// Class constructor. Creates the track object from the color matrix.
        /// </summary>
        /// <param name="roadTex">texture of road</param>
        /// <param name="map">color matrix</param>
        public Track(Texture2D roadTex, Color[,] map) : base(roadTex)
        {
            FindBoundaries(map);
            FillRoadBuffers();
            CreateBoundingBoxes();
        }

        /// <summary>
        /// Initializes the innerBoundary and outerBoundary lists
        /// </summary>
        /// <param name="colorMat">color matrix of blueprint</param>
        private void FindBoundaries(Color[,] colorMat)
        {
            List<Vector2> innerBoundary2D = AddCurves(FindInnerBoundary(colorMat, 50), 5);
            List<Vector2> outerBoundary2D = CreateOuterBoundary(innerBoundary2D);
            innerBoundary = new List<Vector3>();
            outerBoundary = new List<Vector3>();
            for (int i = 0; i < innerBoundary2D.Count; i++)
            {
                this.innerBoundary.Add(new Vector3(innerBoundary2D[i].X, 0, -innerBoundary2D[i].Y));
                this.outerBoundary.Add(new Vector3(outerBoundary2D[i].X, 0, -outerBoundary2D[i].Y));
            }
        }

        /// <summary>
        /// Finds all points of the inner road boundary from the data
        /// of the color matrix. 
        /// </summary>
        /// <param name="colorMat">color matrix</param>
        /// <param name="detail">amount of dots to return (sensitivity variable)</param>
        /// <returns></returns>
        private List<Vector2> FindInnerBoundary(Color[,] colorMat, int detail)
        {
            //scanning the color matrix to find the first boundary pixel
            Vector2 point = new Vector2();
            bool pointFound = false;
            for (int i = 0; i < colorMat.GetLength(0) && !pointFound; i++)
                for (int k = 0; k < colorMat.GetLength(1) && !pointFound; k++)
                    if (colorMat[i, k] == Color.Black)
                    {
                        point = new Vector2(i, k);
                        pointFound = true;
                    }

            //finding the rest of the boundary using each point's neighboring pixels
            int detailFactor = 0;
            List<Vector2> innerBoundary = new List<Vector2>();
            do
            {
                colorMat[(int)point.X, (int)point.Y] = Color.Orange;
                if ((detailFactor++) % detail == 0)
                    innerBoundary.Add(point);
                point = FindNeighborInBoundary(colorMat, point);
            }
            while (point != innerBoundary[0] && point != new Vector2());

            return innerBoundary;
        }
        private Vector2 FindNeighborInBoundary(Color[,] colorMat, Vector2 point)
        {
            Vector2 result = new Vector2();
            bool found = false;
            for (int i = (int)point.X - 1; i < point.X + 2 && !found; i++)
                for (int k = (int)point.Y - 1; k < point.Y + 2 && !found; k++)
                    if (colorMat[i, k] == Color.Black && (i != point.X || k != point.Y))
                        if ((colorMat[i - 1, k] == Color.White && colorMat[i + 1, k] == Color.Black) ||
                            (colorMat[i + 1, k] == Color.White && colorMat[i - 1, k] == Color.Black) ||
                            (colorMat[i, k - 1] == Color.White && colorMat[i, k + 1] == Color.Black) ||
                            (colorMat[i, k + 1] == Color.White && colorMat[i, k - 1] == Color.Black))
                        {
                            result = new Vector2(i, k);
                            found = true;
                        }

            return result;
        }

        /// <summary>
        /// Improves the boundary's curves and overall accuracy.
        /// </summary>
        /// <param name="boundary">boundary to change</param>
        /// <param name="detail">sensitivity variable</param>
        /// <returns></returns>
        private List<Vector2> AddCurves(List<Vector2> boundary, int detail)
        {
            //detail should be a number between 0 and 100
            List<Vector2> result = new List<Vector2>();
            for (int i = 0; i < boundary.Count; i++)
            {
                Vector2[] vectors = new Vector2[]
                {
                    boundary[i > 0 ? i - 1 : boundary.Count - 1],
                    boundary[i],
                    boundary[(i + 1) % boundary.Count],
                    boundary[(i + 2) % boundary.Count]
                };
                for (int k = 0; k < 100; k += detail)
                    result.Add(Vector2.CatmullRom(vectors[0], vectors[1], vectors[2], vectors[3], (float)k / (float)100)); 
            }

            return result;
        }

        /// <summary>
        /// Uses the inner boundary to generate the outer boundary points
        /// </summary>
        /// <param name="innerBoundary">inner boundary</param>
        /// <returns>outer boundary</returns>
        private List<Vector2> CreateOuterBoundary(List<Vector2> innerBoundary)
        {
            //  for each point in the inner boundary:
            //      calculate the vector between the point before it and the point after it
            //      calculate the sum of the inner point and a unit vector perpendicular to the one calculated
            //      the given sum is an outer boundary point
            
            List<Vector2> outerBoundary = new List<Vector2>();
            Vector2 firstVector = innerBoundary[1] - innerBoundary[innerBoundary.Count - 1],
                    lastVector = innerBoundary[0] - innerBoundary[innerBoundary.Count - 2];
            for (int i = 0; i < innerBoundary.Count; i++)
            {
                Vector2 boundaryVector;
                if (i == 0)
                    boundaryVector = firstVector;
                else if (i == innerBoundary.Count - 1)
                    boundaryVector = lastVector;
                else 
                    boundaryVector = innerBoundary[i + 1] - innerBoundary[i - 1];

                Vector2 boundaryNormal = new Vector2(boundaryVector.Y, -boundaryVector.X);
                boundaryNormal.Normalize();
                outerBoundary.Add(innerBoundary[i] - boundaryNormal * 25);
            }
            
            return outerBoundary;
        }

        /// <summary>
        /// Uses the innerBoundary and outerBoundary lists to generate the object's
        /// vertices and indices, and then initializes the buffers.
        /// </summary>
        private void FillRoadBuffers()
        {
            List<VPNT> vertices = new List<VPNT>();
            int yValue = 0;

            for (int i = 0; i < innerBoundary.Count; i += 1)
            {
                vertices.Add(new VPNT(innerBoundary[i], new Vector3(0, 1, 0), new Vector2(1, yValue)));
                vertices.Add(new VPNT(outerBoundary[i], new Vector3(0, 1, 0), new Vector2(0, yValue)));
                yValue = yValue ^ 1;
            }
            vertices.Add(new VPNT(innerBoundary[0], new Vector3(0, 1, 0), new Vector2(1, yValue)));
            vertices.Add(new VPNT(outerBoundary[0], new Vector3(0, 1, 0), new Vector2(0, yValue)));

            vertexBuffer = new VertexBuffer(Game1.device, VPNT.VertexDeclaration, vertices.Count, BufferUsage.WriteOnly);
            vertexBuffer.SetData(vertices.ToArray());


            List<short> indices = new List<short>();
            for (int i = 2; i < vertexBuffer.VertexCount; i += 2)
            {
                indices.Add((short)(i + 1)); indices.Add((short)(i - 2)); indices.Add((short)(i - 1));
                indices.Add((short)(i + 1)); indices.Add((short)(i)); indices.Add((short)(i - 2));
            }
            indexBuffer = new IndexBuffer(Game1.device, typeof(short), indices.Count, BufferUsage.WriteOnly);
            indexBuffer.SetData(indices.ToArray());
        }

        /// <summary>
        /// Uses the innerBoundary and outerBoundary lists to generate a list of
        /// BoundingBoxes to cover the road from both sides.
        /// </summary>
        private void CreateBoundingBoxes()
        {
            boundingBoxes = new List<BoundingBox>();
            for (int i = 1; i < innerBoundary.Count; i++)
            {
                boundingBoxes.Add(new BoundingBox(innerBoundary[i - 1], new Vector3(innerBoundary[i].X, 0.35f, innerBoundary[i].Z)));
                boundingBoxes.Add(new BoundingBox(outerBoundary[i - 1], new Vector3(outerBoundary[i].X, 0.35f, outerBoundary[i].Z)));
            }
            boundingBoxes.Add(new BoundingBox(innerBoundary[innerBoundary.Count - 1], new Vector3(innerBoundary[0].X, 0.35f, innerBoundary[0].Z)));
            boundingBoxes.Add(new BoundingBox(outerBoundary[innerBoundary.Count - 1], new Vector3(outerBoundary[0].X, 0.35f, outerBoundary[0].Z)));
        }
    }
}