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
    static class GameManager
    {
        public static int numOfLaps;
        public static bool isReallyNewLap = true;
        public static List<bool> isReallyNewForBots = new List<bool>();
        public static Temporary2D ready, go, post;
        public static IDrive car;
        public static List<IDrive> bots;

        public static void InitGame(int numOfLaps, IDrive car, List<IDrive> bots)
        {
            GameManager.numOfLaps = numOfLaps;
            GameManager.car = car;
            GameManager.bots = bots;
            LocateCars();
            for (int i = 0; i < bots.Count; i++)
                isReallyNewForBots.Add(false);
            Viewport vp = Game1.device.Viewport;
            ready = new Temporary2D(2, Game1.contentManager.Load<Texture2D>("msg/ready"), new Vector2(vp.Width * 0.35f, vp.Height * 0.1f), null,
                                    Color.White, 0, new Vector2(-1), new Vector2(1), SpriteEffects.None, 0);
        }

        /// <summary>
        /// Assigns initial positions to all the cars in the race
        /// before it begins.
        /// </summary>
        /// <param name="car">player's car</param>
        /// <param name="bots">player's enemies</param>
        public static void LocateCars()
        {
            List<Vector3> innerBoundary = Game1.world.track.innerBoundary,
                          outerBoundary = Game1.world.track.outerBoundary;

            Vector3 line = (innerBoundary[0] - outerBoundary[0]);
            switch (Game1.gameType)
            {
                case GameType.classic:
                    car.position = outerBoundary[0] + line / 2;
                    for (int i = 0; i < bots.Count; i++)
                    {
                        line = (innerBoundary[(i / 2 + 1) * 5] - outerBoundary[(i / 2 + 1) * 5]);
                        bots[i].position = outerBoundary[(i + 1) * 5] + line / 2;
                        bots[i].rot = (float)Math.Atan2(-line.X, -line.Z) + MathHelper.PiOver2;
                    }
                    break;

                case GameType.onlineHost:
                    car.position = outerBoundary[0] + line * 2 / 3;
                    bots[0].position = outerBoundary[0] + line / 3;
                    bots[0].rot = (float)Math.Atan2(-line.X, -line.Z) + MathHelper.PiOver2;
                    break;

                case GameType.onlineJoin:
                    car.position = outerBoundary[0] + line / 3;
                    bots[0].position = outerBoundary[0] + line * 2 / 3;
                    bots[0].rot = (float)Math.Atan2(-line.X, -line.Z) + MathHelper.PiOver2;
                    break;
            }
            

        }

        /// <summary>
        /// Checks which lap each car is on.
        /// </summary>
        public static void Update()
        {
            if (car.closestBoxRight == 0 && car.closestBoxLeft == 1)
            {
                if (isReallyNewLap)
                {
                    if (!car.inReverse)
                    {
                        car.lap++;
                        if (car.lap > numOfLaps)
                            EndRace();
                    }
                    else
                        car.lap--;

                    isReallyNewLap = false;
                }
            }
            else
                isReallyNewLap = true;

            for (int i = 0; i < bots.Count; i++)
            {
                if (bots[i].closestBoxRight == 0 && bots[i].closestBoxLeft == 1)
                {
                    if (isReallyNewForBots[i])
                    {
                        if (!bots[i].inReverse)
                        {
                            bots[i].lap++;
                            if (bots[i].lap > numOfLaps)
                                EndRace();
                        }
                        else
                            bots[i].lap--;

                        isReallyNewForBots[i] = false;
                    }
                }
                else
                    isReallyNewForBots[i] = true;    
            }
        }

        /// <summary>
        /// Checks if the game should move to "play" mode
        /// </summary>
        public static void UpdatePre()
        {
            if (!ready.displayed)
            {
                Viewport vp = Game1.device.Viewport;
                go = new Temporary2D(2, Game1.contentManager.Load<Texture2D>("msg/go"), new Vector2(vp.Width * 0.4f, vp.Height * 0.1f), null,
                                Color.White, 0, new Vector2(-1), new Vector2(2), SpriteEffects.None, 0);
                Game1.gameState = GameState.play;
            }
        }

        /// <summary>
        /// checks if the game should move to "menu" mode
        /// </summary>
        public static void UpdatePost()
        {
            if (!post.displayed)
                MenuManager.MenuAgain(false);
        }

        /// <summary>
        /// moves the game to "post" mode
        /// </summary>
        public static void EndRace()
        {
            Viewport vp = Game1.device.Viewport;
            if (DidIWin())
                post = new Temporary2D(2, Game1.contentManager.Load<Texture2D>("msg/win"), new Vector2(vp.Width * 0.4f, vp.Height * 0.1f), null,
                                Color.White, 0, new Vector2(-1), new Vector2(2), SpriteEffects.None, 0);
            else
                post = new Temporary2D(2, Game1.contentManager.Load<Texture2D>("msg/lose"), new Vector2(vp.Width * 0.4f, vp.Height * 0.1f), null,
                            Color.White, 0, new Vector2(-1), new Vector2(2), SpriteEffects.None, 0);
            Game1.gameState = GameState.post;
        }
        static bool DidIWin()
        {
            for (int i = 0; i < bots.Count; i++)
            {
                if (bots[i].closestBoxLeft > 1 && bots[i].lap >= car.lap)
                    return false;
            }
            return true;
        }
    }
}