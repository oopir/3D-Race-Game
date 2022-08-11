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
using System.Windows.Forms;

namespace Race
{
    static class MenuManager
    {
        static Basic2D background, title;

        static List<Texture2D> roads, themes;
        public static int roadInd = 0, themeInd = 0;
        static Rectangle selctionRec, themeRec;
        static bool selectingRoad;

        public static List<Button> MainScreen, SelectionScreen;
        public static List<Button> current;

        static bool pressed = false;

        /// <summary>
        /// Initializes all the static fields of the class in order to get
        /// ready for displaying the menu. All screens are initialized and
        /// the main one is chosen for the current screen.
        /// </summary>
        public static void InitMenues()
        {
            Viewport vp = Game1.device.Viewport;

            background = new Basic2D(Game1.contentManager.Load<Texture2D>("menu/background"), 
                             new Rectangle(0, 0, Game1.device.Viewport.Width, Game1.device.Viewport.Height));
            title = new Basic2D(Game1.contentManager.Load<Texture2D>("menu/title"), new Rectangle(vp.Width / 3, vp.Height / 10, vp.Width / 3, vp.Height / 5));

            MainScreen = new List<Button>();
            MainScreen.Add(new Button(Game1.contentManager.Load<Texture2D>("menu/main/race"), new Rectangle((int)(vp.Width * 0.45f), (int)(title.rec.Y * 1.5f + title.rec.Height), (int)(vp.Width * 0.1f), vp.Height / 12), new OnClick(Play)));
            MainScreen.Add(new Button(Game1.contentManager.Load<Texture2D>("menu/main/choosetrack"), new Rectangle((int)(vp.Width * 0.20f), (int)(vp.Height * 0.45f), (int)(vp.Width * 0.15f), vp.Height / 12), new OnClick(ToRoadScreen)));
            MainScreen.Add(new Button(Game1.contentManager.Load<Texture2D>("menu/main/choosetime"), new Rectangle((int)(vp.Width * 0.65f), (int)(vp.Height * 0.45f), (int)(vp.Width * 0.15f), vp.Height / 12), new OnClick(ToThemeScreen)));
            MainScreen.Add(new Button(Game1.contentManager.Load<Texture2D>("menu/main/host"), new Rectangle((int)(vp.Width * 0.25f), (int)(vp.Height * 0.75f), (int)(vp.Width * 0.15f), vp.Height / 14), new OnClick(ChoseHost)));
            MainScreen.Add(new Button(Game1.contentManager.Load<Texture2D>("menu/main/join"), new Rectangle((int)(vp.Width * 0.60f), (int)(vp.Height * 0.75f), (int)(vp.Width * 0.15f), vp.Height / 14), new OnClick(ChoseJoin)));

            roads = new List<Texture2D>();
            int i = 1;
            while (i > 0)
                try { 
                    roads.Add(Game1.contentManager.Load<Texture2D>("roads/road" + i++));
                    roads[i - 2] = Game1.CleanColors(roads[i - 2]);
                }
                catch (Exception)  { i = 0; }
            
            themes = new List<Texture2D>();
            i = 1;
            while (i > 0)
                try { themes.Add(Game1.contentManager.Load<Texture2D>("skybox" + (i++) + "/SkyBox_Front")); }
                catch (Exception) { i = 0; }

            selctionRec = new Rectangle((int)(vp.Width * 0.3f), (int)(vp.Height * 0.4f), (int)(vp.Width * 0.4f), (int)(vp.Height * 0.4f));

            SelectionScreen = new List<Button>();
            SelectionScreen.Add(new Button(Game1.contentManager.Load<Texture2D>("menu/left"), new Rectangle((int)(vp.Width * 0.20f), (int)(vp.Height * 0.55f), (int)(vp.Width * 0.1f), vp.Height / 12), new OnClick(ArrowPressed)));
            SelectionScreen.Add(new Button(Game1.contentManager.Load<Texture2D>("menu/right"), new Rectangle((int)(vp.Width * 0.70f), (int)(vp.Height * 0.55f), (int)(vp.Width * 0.1f), vp.Height / 12), new OnClick(ArrowPressed)));
            SelectionScreen.Add(new Button(Game1.contentManager.Load<Texture2D>("menu/back"), new Rectangle((int)(vp.Width * 0.45f), (int)(vp.Height * 0.85f), (int)(vp.Width * 0.1f), vp.Height / 12), new OnClick(ToMainScreen)));

            current = MainScreen;
        }

        /// <summary>
        /// Checks if the mouse was pressed, and when it is checks if it is
        /// located on any of the screen's buttons. If it recognizes a pressed
        /// button, it activates its respone function.
        /// </summary>
        public static void Update()
        {
            if (Mouse.GetState().LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Released)
                pressed = false;
            else
                if (!pressed)
                {
                    Rectangle mouseRec = new Rectangle(Mouse.GetState().X, Mouse.GetState().Y, 2, 2);
                    foreach (Button botton in current)
                        if (botton.rec.Intersects(mouseRec))
                            botton.action();
                    pressed = true;
                }
        }

        /// <summary>
        /// Draws all of the current screen's components to the screen.
        /// </summary>
        public static void Draw()
        {
            Game1.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);
            background.Draw();
            title.Draw();
            foreach (Button button in current)
                button.Draw();
            if (current == SelectionScreen)
                Game1.spriteBatch.Draw(selectingRoad ? roads[roadInd] : themes[themeInd], selctionRec, Color.White);
            Game1.spriteBatch.End();
            Game1.device.BlendState = BlendState.Opaque;
            Game1.device.DepthStencilState = DepthStencilState.Default;
            Game1.device.SamplerStates[0] = SamplerState.LinearWrap; 
        }
        
        // Response-function for a button. Starts the game
        public static void Play()
        {
            Game1.gameState = GameState.loading;
        }
        // Response-function for a button, Returns to menu from a race
        public static void MenuAgain(bool disconnection)
        {
            if (Game1.gameState != GameState.menu)
            {
                Game1.gameState = GameState.menu;
                current = MainScreen;
                if (disconnection)
                    MessageBox.Show("There was a connection problem. You will return to the menu.");
            }
        }
        // Response-function for a button. Switches to main screen
        public static void ToMainScreen()
        {
            current = MainScreen;
        }
        // Response-function for a button. Switches to road screen
        public static void ToRoadScreen()
        {
            current = SelectionScreen;
            selectingRoad = true;
        }
        // Response-function for a button. Switches to theme screen
        public static void ToThemeScreen()
        {
            current = SelectionScreen;
            selectingRoad = false;
        }
        // Response-function for a button. Reacts to pressing an arrow
        public static void ArrowPressed()
        {
            if (selectingRoad)
            {
                if (Mouse.GetState().X < Game1.device.Viewport.Width / 2)
                    roadInd = (roadInd + roads.Count - 1) % roads.Count;
                else
                    roadInd = (roadInd + 1) % roads.Count;
            }
            else
            {
                if (Mouse.GetState().X < Game1.device.Viewport.Width / 2)
                    themeInd = (themeInd + themes.Count - 1) % themes.Count;
                else
                    themeInd = (themeInd + 1) % themes.Count;
            }
        }
        // Response-function for a button. Starts online-host game
        public static void ChoseHost()
        {
            Game1.gameType = GameType.onlineHost;
            Play();
        }
        // response-function for a button, Starts online-client game
        public static void ChoseJoin()
        {
            Game1.gameType = GameType.onlineJoin;
            Play();
        }
    }

    class Button : Basic2D
    {
        public OnClick action;

        /// <summary>
        /// Initializes the Button's variables
        /// </summary>
        /// <param name="texture">button's texture</param>
        /// <param name="destRec">button's destination rectangle</param>
        /// <param name="action">reference to button's response function</param>
        public Button(Texture2D texture, Rectangle destRec, OnClick action)
            :base (texture, destRec)
        {
            this.action = action;
        }
    }
}