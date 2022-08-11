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
    class City : Basic3D
    {
        Vector2[] buildingCoords;
        int[] buildingHeights;

        /// <summary>
        /// Class constructor.
        /// Uses the color matrix to find all the buildings from the planning
        /// texture and creates the vertex and index buffers with that information.
        /// </summary>
        /// <param name="buildingsTex">texture containing different buildings</param>
        /// <param name="map">color matrix of the blueprint texture</param>
        public City(Texture2D buildingsTex, Color[,] map) : base(buildingsTex)
        {
            // initializing random building heights and building's coordinates in texture
            buildingHeights = new int[6];
            buildingCoords = new Vector2[6];
            for (int i = 0; i < buildingCoords.Length; i++)
            {
                buildingCoords[i] = new Vector2(i / (float)(buildingCoords.Length), (i + 1) / (float)(buildingCoords.Length));
                buildingHeights[i] = Game1.rnd.Next(3, 7) * 10;
            }

            // initializing buffers
            List<VPNT> vertices = new List<VPNT>();
            List<short> indices = new List<short>();

            List<Vector3> nextBuilding = FindBuilding(map);
            while (nextBuilding.Count > 0)
            {
                // creating the indicies to match the next building's vertices
                for (int i = 0; i < 8; i += 2)
                {
                    indices.Add((short)(vertices.Count + (i + 2) % 8));
                    indices.Add((short)(vertices.Count + (i + 1) % 8));
                    indices.Add((short)(vertices.Count + i));

                    indices.Add((short)(vertices.Count + (i + 2) % 8));
                    indices.Add((short)(vertices.Count + (i + 3) % 8));
                    indices.Add((short)(vertices.Count + (i + 1) % 8));
                }

                // creating the building's vertices
                int type = Game1.rnd.Next(buildingHeights.Length);
                AddBuildingVertices(Game1.rnd.Next(buildingHeights.Length), nextBuilding, ref vertices);

                nextBuilding = FindBuilding(map);
            }

            vertexBuffer = new VertexBuffer(Game1.device, VPNT.VertexDeclaration, vertices.Count, BufferUsage.WriteOnly);
            vertexBuffer.SetData(vertices.ToArray());
            indexBuffer = new IndexBuffer(Game1.device, typeof(short), indices.Count, BufferUsage.WriteOnly);
            indexBuffer.SetData(indices.ToArray());
        }

        /// <summary>
        /// Adds a new building to a vertices list that will be used to 
        /// create the vertex buffer.
        /// </summary>
        /// <param name="type">the building's type (matters for height and texture coordinates)</param>
        /// <param name="vectors">building's coordinates in blueprint</param>
        /// <param name="vertices">the vertices list to which the new vertices are added</param>
        private void AddBuildingVertices(int type, List<Vector3> vectors, ref List<VPNT> vertices)
        {
            Vector3 normal = new Vector3(1, 0, -1);
            vertices.Add(new VPNT(vectors[0], normal, new Vector2(buildingCoords[type].X, 0)));
            vertices.Add(new VPNT(vectors[0] + new Vector3(0, buildingHeights[type], 0), normal, new Vector2(buildingCoords[type].Y, 0)));
            vertices.Add(new VPNT(vectors[1], normal, new Vector2(buildingCoords[type].X, 1)));
            vertices.Add(new VPNT(vectors[1] + new Vector3(0, buildingHeights[type], 0), normal, new Vector2(buildingCoords[type].Y, 1)));
            vertices.Add(new VPNT(vectors[2], normal, new Vector2(buildingCoords[type].X, 0)));
            vertices.Add(new VPNT(vectors[2] + new Vector3(0, buildingHeights[type], 0), normal, new Vector2(buildingCoords[type].Y, 0)));
            vertices.Add(new VPNT(vectors[3], normal, new Vector2(buildingCoords[type].X, 1)));
            vertices.Add(new VPNT(vectors[3] + new Vector3(0, buildingHeights[type], 0), normal, new Vector2(buildingCoords[type].Y, 1)));
        }

        /// <summary>
        /// finds a building in the color matrix, if one is still left
        /// </summary>
        /// <param name="map">color matrix</param>
        /// <returns>list of building's coordinates in matrix</returns>
        private List<Vector3> FindBuilding(Color[,] map)
        {
            //scanning the color matrix to find the first boundary pixel
            Vector2 point = new Vector2();
            bool pointFound = false;
            for (int i = 0; i < map.GetLength(0) && !pointFound; i++)
                for (int k = 0; k < map.GetLength(1) && !pointFound; k++)
                    if (map[i, k] == Color.Red)
                    {
                        point = new Vector2(i, k);
                        pointFound = true;
                    }
            if (!pointFound)
                return new List<Vector3>();

            //finding the rest of the boundary using each point's neighboring pixels
            List<Vector2> shape = new List<Vector2>();
            do
            {
                map[(int)point.X, (int)point.Y] = Color.Orange;
                shape.Add(point);
                point = FindNeighborInBoundary(map, point);
            }
            while (point != shape[0] && point != new Vector2());

            //finding the shape's corners
            List<Vector3> corners = new List<Vector3>();
            corners.Add(new Vector3(shape[0].X, 0, -shape[0].Y));
            for (int i = 1; i < shape.Count - 1; i++)
                if (!Vector2.Equals(shape[i] - shape[i - 1], shape[i + 1] - shape[i]))
                    corners.Add(new Vector3(shape[i].X, 0, -shape[i].Y));

            return corners;
        }
        private Vector2 FindNeighborInBoundary(Color[,] colorMat, Vector2 point)
        {
            Vector2 result = new Vector2();
            bool found = false;

            for (int i = (int)point.X - 1; i < point.X + 2 && !found; i++)
                for (int k = (int)point.Y - 1; k < point.Y + 2 && !found; k++)
                    if (colorMat[i, k] == Color.Red && (i != point.X || k != point.Y))
                        if (Math.Abs(i - point.X) + Math.Abs(k - point.Y) == 1) //no diagonals
                            //if ((colorMat[i - 1, k] == Color.White && colorMat[i + 1, k] == Color.Red) ||
                            //    (colorMat[i + 1, k] == Color.White && colorMat[i - 1, k] == Color.Red) ||
                            //    (colorMat[i, k - 1] == Color.White && colorMat[i, k + 1] == Color.Red) ||
                            //    (colorMat[i, k + 1] == Color.White && colorMat[i, k - 1] == Color.Red))
                            {
                                result = new Vector2(i, k);
                                found = true;
                            }

            return result;
        }
    }
}