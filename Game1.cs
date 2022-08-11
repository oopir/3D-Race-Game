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
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        #region general
        GraphicsDeviceManager graphics;
        public static SpriteBatch spriteBatch;
        public static GraphicsDevice device;
        public static Effect effect;
        public static ContentManager contentManager;
        public static Random rnd = new Random();
        public static event Draw_Handler CallDraw;
        public static event Update_Handler CallUpdate;
        public static event Initiate_Collision CallCollisionEvents;
        public static event Update_Handler MsgUpdate;
        public static GameState gameState = GameState.menu;
        public static GameType gameType = GameType.classic;
        public static KeyboardState pks = Keyboard.GetState();
        Thread GameLoader;
        Texture2D loadingTex;
        #endregion

        public static World world;
        Car mainCar;
        List<IDrive> bots;
        DataMonitor monitor;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            contentManager = Content;
        }

        protected override void Initialize()
        {
            //graphics.IsFullScreen = true;
            //graphics.ApplyChanges();
            //graphics.PreferredBackBufferWidth = 700;
            //graphics.PreferredBackBufferHeight = 500;
            graphics.PreferredBackBufferWidth = 1100;
            graphics.PreferredBackBufferHeight = 600;
            graphics.ApplyChanges();
            device = graphics.GraphicsDevice;
            MenuManager.InitMenues();
            loadingTex = contentManager.Load<Texture2D>("menu/loading");
            this.IsMouseVisible = true;
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(device);
            GameLoader = new Thread(new ThreadStart(FirstLoad));
            GameLoader.Priority = ThreadPriority.AboveNormal;
            GameLoader.Start();
        }
        void FirstLoad()
        {
            Cameras.InitCameras();
            effect = Content.Load<Effect>("effects");
            Car.LoadModel("car1/car", "car2/car", effect);
        }
        void LastLoad(int road, int skybox)
        {
            #region reset world, load world, load main car
            ResetWorld();
            if (gameType != GameType.classic)
            {
                road = 1;
                skybox = 1;
            }
            world = new World(Content.Load<Texture2D>("world/road"),
                                      Content.Load<Texture2D>("world/sidewalk"),
                                      Content.Load<Texture2D>("roads/road" + road),
                                      Content.Load<Texture2D>("world/buildings"),
                                      Content.Load<Texture2D>("world/floor"),
                                      Content.Load<Texture2D>("world/finish"),
                                      effect);
            bots = new List<IDrive>(); 
            #endregion
            Client client = null;
            switch(gameType)
            {
                case GameType.classic:
                    mainCar = new Car(world.initialPos, new PlayerKeys(Keys.Up, Keys.Down, Keys.Left, Keys.Right, Keys.D));
                    //mainCar = new Car(world.initialPos, "carmesh/car", new BotKeys(MathHelper.ToRadians(45), 80, 2.5f), effect);
                    bots.Add(new Car(world.initialPos, new BotKeys(MathHelper.ToRadians(60), 90, 1.5f)));
                    bots.Add(new Car(world.initialPos, new BotKeys(MathHelper.ToRadians(60), 80, 2)));
                    //bots.Add(new Car(world.initialPos, new BotKeys(MathHelper.ToRadians(45), 80, 2.5f)));
                    break;

                case GameType.onlineJoin:
                    client = new Client();
                    mainCar = new OnlineCar(client, world.initialPos, new PlayerKeys(Keys.Up, Keys.Down, Keys.Left, Keys.Right, Keys.D));
                    bots.Add(new OnlineBot(client, world.initialPos, null));
                    break;

                case GameType.onlineHost:
                    Server server = new Server();
                    client = new Client(server.connectedClient);
                    mainCar = new OnlineCar(client, world.initialPos, new PlayerKeys(Keys.Up, Keys.Down, Keys.Left, Keys.Right, Keys.D));
                    bots.Add(new OnlineBot(client, world.initialPos, null));
                    break;
            }

            world.InitSkybox("skybox" + skybox + "/skybox2", mainCar, effect);
            GameManager.InitGame(2, mainCar, bots);
            monitor = new DataMonitor(mainCar, bots, Content.Load<Texture2D>("roads/road" + road));
            SyncWithOnlinePartner(client);
            gameState = GameState.pre;
        }
        void ResetWorld()
        {
            if (world != null)
            {
                world.BeGone();
                Track.boundingBoxes = null;
            }

            if (mainCar != null)
                mainCar.BeGone();

            if (bots != null)
                foreach (IDrive b in bots)
                    (b as Car).BeGone();
        }
        void SyncWithOnlinePartner(Client c)
        {
            if (c == null) 
                return;
            c.GiveHandshake();
            c.RecieveHandShake();
        }

        protected override void UnloadContent()
        {
            
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.P) && !pks.IsKeyDown(Keys.P))
            {
                if (gameState == GameState.play)
                    gameState = GameState.pause;
                else if (gameState == GameState.pause)
                    gameState = GameState.play;
            }
            switch (gameState)
            {
                case GameState.menu:
                    MenuManager.Update();
                    break;

                case GameState.loading:
                    if (!GameLoader.IsAlive)
                        LastLoad(MenuManager.roadInd + 1, MenuManager.themeInd + 1);
                    break;

                case GameState.pre:
                    if (MsgUpdate != null)
                        MsgUpdate(gameTime);
                    GameManager.UpdatePre();
                    Cameras.UpdateCameras(mainCar.position, Quaternion.CreateFromRotationMatrix(Matrix.CreateRotationY(mainCar.rot)));
                    break;

                case GameState.post:
                     if (MsgUpdate != null)
                        MsgUpdate(gameTime);
                    GameManager.UpdatePost();
                    if (CallUpdate != null)
                        CallUpdate(gameTime);
                    Cameras.UpdateCameras(mainCar.position, Quaternion.CreateFromRotationMatrix(Matrix.CreateRotationY(mainCar.rot)));
                    break;

                case GameState.play: 
                    if (MsgUpdate != null)
                        MsgUpdate(gameTime);
                    if (CallCollisionEvents != null)
                        CallCollisionEvents();
                    if (CallUpdate != null)
                        CallUpdate(gameTime);
                    GameManager.Update();
                    Cameras.UpdateCameras(mainCar.position, Quaternion.CreateFromRotationMatrix(Matrix.CreateRotationY(mainCar.rot)));
                    #region god camera
                    //if (Keyboard.GetState().IsKeyDown(Keys.LeftControl))
                    //{
                    //    if (Keyboard.GetState().IsKeyDown(Keys.Up))
                    //        Cameras.camRotation += Quaternion.CreateFromAxisAngle(new Vector3(1, 0, 0), 0.005f);
                    //    if (Keyboard.GetState().IsKeyDown(Keys.Down))
                    //        Cameras.camRotation += Quaternion.CreateFromAxisAngle(new Vector3(1, 0, 0), -0.005f);
                    //    if (Keyboard.GetState().IsKeyDown(Keys.Left))
                    //        Cameras.camRotation += Quaternion.CreateFromAxisAngle(new Vector3(0, 1, 0), 0.005f);
                    //    if (Keyboard.GetState().IsKeyDown(Keys.Right))
                    //        Cameras.camRotation += Quaternion.CreateFromAxisAngle(new Vector3(0, 1, 0), -0.005f);
                    //}
                    //else
                    //{
                    //    if (Keyboard.GetState().IsKeyDown(Keys.Up))
                    //        Cameras.camPosition += new Vector3(0, 0, -2);
                    //    if (Keyboard.GetState().IsKeyDown(Keys.Down))
                    //        Cameras.camPosition += new Vector3(0, 0, 2);
                    //    if (Keyboard.GetState().IsKeyDown(Keys.Left))
                    //        Cameras.camPosition += new Vector3(-2, 0, 0);
                    //    if (Keyboard.GetState().IsKeyDown(Keys.Right))
                    //        Cameras.camPosition += new Vector3(2, 0, 0);
                    //    if (Keyboard.GetState().IsKeyDown(Keys.Z))
                    //        Cameras.camPosition += new Vector3(0, 2, 0);
                    //    if (Keyboard.GetState().IsKeyDown(Keys.X))
                    //        Cameras.camPosition += new Vector3(0, -2, 0);
                    //}
                    //Cameras.UpdateCameras(); 
                #endregion
                    break;

                case GameState.pause:
                    break;
            }

            pks = Keyboard.GetState();
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            switch (gameState)
            {
                case GameState.menu:
                    MenuManager.Draw(); 
                    break;

                case GameState.loading:
                    spriteBatch.Begin();
                    spriteBatch.Draw(loadingTex, new Rectangle(0, 0, device.Viewport.Width, device.Viewport.Height), Color.White);
                    spriteBatch.End();
                    break;

                case GameState.pause:
                case GameState.pre:
                case GameState.post:
                    
                case GameState.play:
                    spriteBatch.Begin();
                    if (CallDraw != null)
                        CallDraw(effect);
                    spriteBatch.End();
                    monitor.DrawMonitor(gameTime);
                    break;
            }

            base.Draw(gameTime);
        }

        public static Texture2D CleanColors(Texture2D tex)
        {
            Texture2D newTex = new Texture2D(device, tex.Width, tex.Height);
            Color[] colorArr = new Color[tex.Width * tex.Height];
            tex.GetData<Color>(colorArr);
            for (int i = 0; i < colorArr.Length; i++)
                if (colorArr[i].R + colorArr[i].G + colorArr[i].B > 50)
                    colorArr[i] = Color.Transparent;
                else
                    colorArr[i] = Color.Black;
            newTex.SetData<Color>(colorArr);
            return newTex;
        }
    }
}