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
    public class World
    {
        Color[,] map;
        public Vector3 initialPos;
        public Track track;
        Sidewalk sidewalk;
        City city;
        public Skybox skybox;
        Floor floor;
        FinishLine line;

        /// <summary>
        /// Creating all of the world's components 
        /// </summary>
        /// <param name="roadTex">texture to create track with</param>
        /// <param name="sidewalkTex">texture to create sidewalk with</param>
        /// <param name="blueprint">world's outline (track and building)</param>
        /// <param name="buildings">texture of the different buildings</param>
        /// <param name="floorTex">texture to create floor with</param>
        /// <param name="effect">effect to draw all objects with</param>
        public World(Texture2D roadTex, Texture2D sidewalkTex, Texture2D blueprint, Texture2D buildings, Texture2D floorTex, Texture2D finishTex, Effect effect)
        {
            map = getColorMatrix(blueprint);
            floor = new Floor(floorTex, map.GetLength(0), map.GetLength(1));
            track = new Track(roadTex, map);
            initialPos = (track.innerBoundary[0] - track.outerBoundary[0]) / 2 + track.outerBoundary[0];

            sidewalk = new Sidewalk(sidewalkTex, track.innerBoundary, track.outerBoundary);
            city = new City(buildings, map);

            List<Vector3> start = (new Vector3[] {Track.boundingBoxes[0].Min, Track.boundingBoxes[1].Min, Track.boundingBoxes[12].Min, Track.boundingBoxes[13].Min}).ToList();
            line = new FinishLine(finishTex, start);
        }

        /// <summary>
        /// Initializes the skybox instance
        /// This stands aside from the constructor because the skybox needs to recieve
        /// a car to follow, and creation of the car must be after creation of the track
        /// for the car's initial position.
        /// </summary>
        /// <param name="skyboxPath">path to box model</param>
        /// <param name="car">car to follow</param>
        /// <param name="effect">effect to draw skybox with</param>
        public void InitSkybox(string skyboxPath, IBasic car, Effect effect)
        {
            skybox = new Skybox(skyboxPath, car, effect);
        }

        // returns a matrix of the texture's colors
        private Color[,] getColorMatrix(Texture2D blueprint)
        {
            Color[] colorArr = new Color[blueprint.Height * blueprint.Width];
            blueprint.GetData<Color>(colorArr);

            Color[,] colorMat = new Color[blueprint.Width, blueprint.Height];
            for (int i = 0; i < colorMat.GetLength(0); i++)
                for (int j = 0; j < colorMat.GetLength(1); j++)
                    colorMat[i, j] = colorArr[j * colorMat.GetLength(0) + i];

            return colorMat;
        }

        public void BeGone()
        {
            track.BeGone();
            sidewalk.BeGone();
            city.BeGone();
            floor.BeGone();
            skybox.BeGone();
            track = null; sidewalk = null; city = null; floor = null; skybox = null;
        }
    }
}