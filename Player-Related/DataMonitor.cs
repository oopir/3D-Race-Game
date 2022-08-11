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
    class DataMonitor
    {
        IDrive mainCar;
        List<IDrive> bots;
        Texture2D redObj, greenObj;
        SpriteFont font;
        Basic2D map, speedometer;
        Complex2D arrow;
        TimeSpan raceTime;

        /// <summary>
        /// Class constructor.
        /// Initializes all variables and signs up to needed events.
        /// </summary>
        /// <param name="car">references to player's car</param>
        /// <param name="bots">references to player's enemies</param>
        /// <param name="mapTex">texture of the race's track</param>
        public DataMonitor(IDrive car, List<IDrive> bots, Texture2D mapTex)
        {
            mainCar = car;
            this.bots = bots;

            redObj = Game1.contentManager.Load<Texture2D>("monitor/red"); CleanBackground(redObj);
            greenObj = Game1.contentManager.Load<Texture2D>("monitor/green"); CleanBackground(greenObj);
            font = Game1.contentManager.Load<SpriteFont>("myfont");

            Viewport vp = Game1.device.Viewport;

            map = new Basic2D(mapTex,  new Rectangle(vp.Width * 2 / 3, 0, vp.Width / 3, vp.Height / 3));
            map.texture = Game1.CleanColors(map.texture);

            speedometer = new Basic2D(Game1.contentManager.Load<Texture2D>("monitor/speedometer"), new Rectangle(10, vp.Height - vp.Width / 5 - 10, vp.Width / 5, vp.Width / 5));
            CleanBackground(speedometer.texture);
            
            arrow = new Complex2D(Game1.contentManager.Load<Texture2D>("monitor/arrow"),
                                  new Vector2(speedometer.rec.X + speedometer.rec.Width / 2, speedometer.rec.Y + speedometer.rec.Width / 2), 
                                  null, Color.White, mainCar.speed.Length() / 40, Vector2.Zero, Vector2.One, SpriteEffects.None, 0);
            
            arrow.origin = new Vector2(arrow.texture.Width / 2, arrow.texture.Height / 2);
            CleanBackground(arrow.texture);

            Game1.CallUpdate += new Update_Handler(Update);
        }
        private void CleanBackground(Texture2D tex)
        {
            Color[] colorArr = new Color[tex.Width * tex.Height];
            tex.GetData<Color>(colorArr);
            for (int i = 0; i < colorArr.Length; i++)
                if (colorArr[i].R + colorArr[i].G + colorArr[i].B > 700)
                    colorArr[i] = Color.Transparent;
                else
                    colorArr[i] = colorArr[i];
            tex.SetData<Color>(colorArr);
        }

        public void Update(GameTime gameTime)
        {
            arrow.rotation = mainCar.speed.Length() / 40;
            raceTime += new TimeSpan(0, 0, 0, 0, gameTime.ElapsedGameTime.Milliseconds);
        }

        /// <summary>
        /// Draws all of the monitor's components
        /// </summary>
        public void DrawMonitor(GameTime gameTime)
        {
            Game1.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);

            map.Draw();
            speedometer.Draw();
            arrow.Draw();
            foreach (IDrive bot in bots)
                DrawOnMinimap(Track.boundingBoxes[bot.closestBoxRight].Min, true);
            DrawOnMinimap(Track.boundingBoxes[mainCar.closestBoxRight].Min, false);

            string time = raceTime.Minutes + ":" + raceTime.Seconds + "." + raceTime.Milliseconds / 10;
            Game1.spriteBatch.DrawString(font, time, new Vector2(Game1.device.Viewport.Width * 0.05f, Game1.device.Viewport.Height * 0.05f), Color.Snow);
            int lapNum = (mainCar.lap > 0 ? mainCar.lap : 1);
            Game1.spriteBatch.DrawString(font, "Lap: " + lapNum + "/" + GameManager.numOfLaps, new Vector2(Game1.device.Viewport.Width * 0.05f, Game1.device.Viewport.Height * 0.15f), Color.Snow);
            
            Game1.spriteBatch.End();
            Game1.device.BlendState = BlendState.Opaque;
            Game1.device.DepthStencilState = DepthStencilState.Default;
            Game1.device.SamplerStates[0] = SamplerState.LinearWrap; 
        }
        public void DrawOnMinimap(Vector3 pos, bool red)
        {
            Vector2 posOnMap = new Vector2(pos.X / ((float)map.texture.Width / (float)map.rec.Width), -pos.Z / ((float)map.texture.Height / (float)map.rec.Height));
            if (red)
                Game1.spriteBatch.Draw(redObj, new Vector2(map.rec.X, map.rec.Y) + posOnMap, null, Color.White, 0, 
                                       new Vector2(redObj.Width / 2, redObj.Height / 2), 0.15f, SpriteEffects.None, 0);
            else
                Game1.spriteBatch.Draw(greenObj, new Vector2(map.rec.X, map.rec.Y) + posOnMap, null, Color.White, 0,
                                       new Vector2(greenObj.Width / 2, greenObj.Height / 2), 0.35f, SpriteEffects.None, 0);
        }

        public void CalculatePlaces()
        {
            IDrive[] sortedCars = new IDrive[bots.Count + 1];
            for (int i = 0; i < sortedCars.Length - 1; i++)
                sortedCars[i] = bots[i];
            sortedCars[bots.Count] = mainCar;

            for (int i = 0; i < sortedCars.Length; i++)
                for (int k = 0; k < sortedCars.Length - i - 1; k++)
                {
                    bool toSwitch = false;
                    if (sortedCars[k].lap > sortedCars[k + 1].lap)
                        toSwitch = true;
                    else
                        if (sortedCars[k].lap == sortedCars[k + 1].lap && sortedCars[k].closestBoxLeft > sortedCars[k].closestBoxLeft)
                            toSwitch = true;

                    if (toSwitch)
                    {
                        IDrive temp = sortedCars[k];
                        sortedCars[k] = sortedCars[k + 1];
                        sortedCars[k + 1] = temp;
                    }
                }
        }
    }
}