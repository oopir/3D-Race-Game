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
using V3 = Microsoft.Xna.Framework.Vector3;
using VPNT = Microsoft.Xna.Framework.Graphics.VertexPositionNormalTexture;

namespace Race
{
    class Car : IDrive, IBasic
    {
        public Vector3 position { get; set; }
        public float rot { get; set; }
        BaseKeys keyboard;
        public static event Collision_Handler CheckCollision;
        public int lap { get; set; }

        #region drawing-related
        public static Model model;
        protected Matrix worldMatrix;
        public static List<Texture2D> playerTextures, botTextures;
        #endregion

        #region physics-related
        //changing attributes
        public Vector3 speed { get; set; }
        public float FWR { get; set; }
        public bool drifting { get; set; }
        public float boost { get; set; }
        float driftStart;
        public int closestBoxLeft { get; set; }
        public int closestBoxRight { get; set; }
        public bool inReverse { get; set; }
        public bool collided { get; set; }
        //forces and constants
        static float engineForce = 25000 * 2f;
        static float air_resistance = 0.75f * 1.5f;
        static float wheel_resistance = 22.5f * 1.5f;
        static float breaking_factor = 100000;
        static float cornering_stiffness = 40f;
        static int mass = 1500;
        static float[] gears = new float[6] { 2.6f, 1.78f, 1.30f, 1f, 0.74f, 0.50f };
        #endregion

        /// <summary>
        /// Loads the car model to be used in the game, and makes the adjustments needed
        /// </summary>
        /// <param name="model_name">path of model</param>
        /// <param name="effect">the effect it will be drawn with</param>
        public static void LoadModel(string model_name1, string model_name2, Effect effect)
        {
            model = Game1.contentManager.Load<Model>(model_name1);
            playerTextures = new List<Texture2D>();
            foreach (ModelMesh mesh in model.Meshes)
                foreach (BasicEffect currentEffect in mesh.Effects)
                    playerTextures.Add(currentEffect.Texture);

            foreach (ModelMesh mesh in model.Meshes)
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                    meshPart.Effect = effect.Clone();

            model = Game1.contentManager.Load<Model>(model_name2);
            botTextures = new List<Texture2D>();
            foreach (ModelMesh mesh in model.Meshes)
                foreach (BasicEffect currentEffect in mesh.Effects)
                    botTextures.Add(currentEffect.Texture);

            foreach (ModelMesh mesh in model.Meshes)
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                    meshPart.Effect = effect.Clone();
        }

        /// <summary>
        /// Class constructor. 
        /// Initializes variables and signs up for events.
        /// </summary>
        /// <param name="position">car's initial position</param>
        /// <param name="keys">car's keyboard object</param>
        public Car(V3 position, BaseKeys keys)
        {
            #region initialize variables
            this.position = position;
            this.keyboard = keys;
            if (keyboard is BotKeys)
                keys.initCar(this);
            rot = 0;
            speed = new V3();
            FWR = 0;
            drifting = false;
            boost = 1f;
            lap = 0;
            collided = false;
            worldMatrix = Matrix.CreateRotationY(MathHelper.Pi) *
                          Matrix.CreateRotationY(rot) *
                          Matrix.CreateScale(1.25f) *
                          Matrix.CreateTranslation(position);

            //rest of region finds the closest BoundingBoxes to the car from each side.
            closestBoxLeft = 0; float bestDistanceLeft = V3.Distance(position, (Track.boundingBoxes[0].Min + Track.boundingBoxes[0].Max) / 2);
            for (int i = 1; i < Track.boundingBoxes.Count; i++)
            {
                if (V3.Distance(position, (Track.boundingBoxes[i].Min + Track.boundingBoxes[i].Max) / 2) < bestDistanceLeft)
                {
                    closestBoxLeft = i;
                    bestDistanceLeft = V3.Distance(position, (Track.boundingBoxes[i].Min + Track.boundingBoxes[i].Max) / 2);
                }
            }
            closestBoxRight = (closestBoxLeft + 1) % Track.boundingBoxes.Count; 
            #endregion

            Game1.CallDraw += new Draw_Handler(DrawCar);
            Game1.CallUpdate += new Update_Handler(Update);
            Car.CheckCollision += new Collision_Handler(HandleCollision);
            Game1.CallCollisionEvents += new Initiate_Collision(CallCollisionEvent);
        }

        /// <summary>
        /// draws the car model
        /// </summary>
        /// <param name="effect">effect to draw with</param>
        public void DrawCar(Effect effect)
        {
            worldMatrix = Matrix.CreateRotationY(MathHelper.Pi) *
                          Matrix.CreateRotationY(rot) *
                          Matrix.CreateScale(1.25f) *
                          Matrix.CreateTranslation(position);

            Matrix[] meshTransforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(meshTransforms);
            meshTransforms[4] = Matrix.CreateRotationY(FWR) * meshTransforms[4]; // displaying front wheel rotation
            meshTransforms[10] = Matrix.CreateRotationY(FWR) * meshTransforms[10]; // displaying front wheel rotation
            int i = 0;
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (Effect currentEffect in mesh.Effects)
                {
                    currentEffect.CurrentTechnique = currentEffect.Techniques["Textured"];

                    currentEffect.Parameters["xWorld"].SetValue(meshTransforms[mesh.ParentBone.Index] * worldMatrix);
                    currentEffect.Parameters["xView"].SetValue(Cameras.viewMatrix);
                    currentEffect.Parameters["xProjection"].SetValue(Cameras.projectionMatrix);
                    if (keyboard is PlayerKeys)
                        currentEffect.Parameters["xTexture"].SetValue(playerTextures[i++]);
                    else
                        currentEffect.Parameters["xTexture"].SetValue(botTextures[i++]);
                    currentEffect.Parameters["xEnableLighting"].SetValue(true);
                    currentEffect.Parameters["xLightDirection"].SetValue(Cameras.lightDirection);
                    currentEffect.Parameters["xAmbient"].SetValue(0.5f);
                }
                mesh.Draw();
            }
        }

        /// <summary>
        /// updates the car's keyboard, applies the physics to its movement, 
        /// checks for collision with other cars and finally updates the car's
        /// world matrix.
        /// </summary>
        /// <param name="gameTime">GameTime object</param>
        public virtual void Update(GameTime gameTime)
        {
            keyboard.Update();

            ApplyPhysicsToCar(gameTime);

            worldMatrix = Matrix.CreateRotationY(MathHelper.Pi) *
                          Matrix.CreateRotationY(rot) *
                          Matrix.CreateScale(1.25f) *
                          Matrix.CreateTranslation(position);

            collided = false;
        }

        public void CallCollisionEvent()
        {
            Car.CheckCollision(this);
        }

        /// <summary>
        /// managing all the physics implementation
        /// </summary>
        /// <param name="gameTime">GameTime object</param>
        protected void ApplyPhysicsToCar(GameTime gameTime)
        {
            float speedDir = (float)Math.Atan2(-speed.X, -speed.Z);
            if (speedDir < 0) speedDir += MathHelper.TwoPi;

            // using the speed vector and the rotation variable to determine if the car 
            // is in reverse mode or not
            if (speed.Length() == 0)
                inReverse = false;
            else
            {
                float movementAngle = (float)Math.Atan2(-speed.X, -speed.Z);
                if (movementAngle < 0) movementAngle += MathHelper.TwoPi;
                if (movementAngle > MathHelper.TwoPi) movementAngle -= MathHelper.TwoPi;
                if (Math.Abs(movementAngle - rot) > MathHelper.ToRadians(160) && Math.Abs(movementAngle - rot) < MathHelper.ToRadians(200))
                    inReverse = true;
                else
                    inReverse = false;
            }

            //checking keyboard input and calculating net force (no collision)
            HandleSteering();
            HandleDrifting(gameTime);
            V3 NetForce; float rotChange123;
            HandleForces(gameTime, out NetForce, out rotChange123);
            V3 firstCollision = HandleCollisions(gameTime);
            if (!(keyboard is PlayerKeys && Keyboard.GetState().IsKeyDown(Keys.A)))
                NetForce += firstCollision;

            //applying the net force to calculate change in position
            V3 acceleration = NetForce / mass;
            speed += acceleration * (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (speed.Length() < 1f)
                speed = new V3(0);
            V3 deltaPos = speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            position += deltaPos;

            //updating the car rotation
            float turnRadius = (2) / (float)Math.Sin(FWR) * 6; // first # is car length, second # is to control sensitivity
            float rotChange = deltaPos.Length() / turnRadius;
            if (rotChange123 != 0)
                rot += rotChange123;
            else
                rot += rotChange;

            rot %= MathHelper.TwoPi;
            while (rot < 0) rot += MathHelper.TwoPi;
        }
        private void HandleSteering()
        {
            float turningAngle = 0.02f;
            float maxAngle = Math.Min(MathHelper.PiOver4 * 0.55f, 0.8f / (speed.Length() / 15));

            // updating the FWR according to the pressed keys. If no keys are affecting
            // the FWR, the wheels are attracted back to their initial position.
            if (keyboard.pressedRight())
                FWR = Math.Max(-maxAngle, FWR - turningAngle);
            else
                if (keyboard.pressedLeft())
                    FWR = Math.Min(maxAngle, FWR + turningAngle);
                else
                    if (FWR > 0)
                        FWR = Math.Max(0, FWR - turningAngle * 5);
                    else
                        if (FWR < 0)
                            FWR = Math.Min(0, FWR + turningAngle * 5);
            Math.Round(FWR, 4);
            if (Math.Abs(FWR) < 0.01f)
                FWR = 0;
        }
        private void HandleDrifting(GameTime gameTime)
        {
            // this function was written in case continuous drifting would
            // need to result in a bonus boost.
            if (FWR != 0 && keyboard.pressedDrift())
            {
                if (!drifting)
                {
                    drifting = true;
                    driftStart = 0;
                    boost = 2;
                }
                else
                    driftStart += gameTime.ElapsedGameTime.Milliseconds;
            }
            else
                if (drifting)
                {
                    boost = (speed.Length() > 80) ? 1 : (driftStart + 500) / 1000f * 5;
                    drifting = false;
                }
                else
                    boost = MathHelper.Lerp(boost, 1, 0.03f); 
        }
        private void HandleForces(GameTime gt, out V3 NetForce, out float rotChange)
        {
            float speedDir = (float)Math.Atan2(-speed.X, -speed.Z);

            // air resistance and wheel resistance
            V3 airRes = air_resistance * -speed * speed.Length();
            V3 wheelRes = wheel_resistance * -speed;

            // engine/breaking force
            V3 movementForce = new V3();
            if (keyboard.pressedUp())
                movementForce = V3.Transform(new V3(0, 0, -1), Matrix.CreateRotationY(rot + FWR)) *
                                engineForce * gears[(int)Math.Min(gears.Length - 1, (int)(speed.Length() / 20))];
            else if (keyboard.pressedDown() &&  !(inReverse && speed.Length() > 30))
                movementForce = V3.Transform(new V3(0, 0, 1), Matrix.CreateRotationY(rot)) * breaking_factor;

            // curving in low speed
            rotChange = 0;
            if (speed.Length() > 00 && !keyboard.pressedDrift())
            {
                // low-speed turning
                float turnRadius = (2) / (float)Math.Sin(FWR) * 6; // first # is car length, second # is to control sensitivity
                rotChange = (speed.Length() / turnRadius) * (float)gt.ElapsedGameTime.TotalSeconds;
                if (inReverse)
                    rotChange *= -1;
                speed = V3.Transform(speed, Matrix.CreateRotationY(rotChange));
            }

            // friction caused by turning
            V3 frictionForce = new V3();
            if (speed.Length() > 3)
            {
                V3 frictionDir = -speed;
                frictionDir.Normalize();
                float frontFrictionMagnitude = cornering_stiffness * (float)Math.Abs(Math.Sin((rot + FWR) - speedDir)) * mass / 2 * 10;
                float rearFrictionMagnitude = cornering_stiffness * (float)Math.Abs(Math.Sin((rot) - speedDir)) * mass / 2 * 10;
                if (keyboard.pressedDrift())
                    rearFrictionMagnitude = 0;
                frictionForce = (frontFrictionMagnitude + rearFrictionMagnitude) * frictionDir;
            }

            NetForce = airRes + wheelRes + movementForce + frictionForce;
        }

        private V3 HandleCollisions(GameTime gameTime)
        {
            // initializes variables before using the CheckIntersection method
            float speedDir = (float)Math.Atan2(-speed.X, -speed.Z);
            if (speedDir < 0) speedDir += MathHelper.TwoPi;            
            V3 collisionResult = new V3();
            BoundingSphere sphere = model.Meshes[0].BoundingSphere;
            sphere.Center = this.position;
            sphere.Radius *= 1.25f;
            
            // checking for intersection between the car and the closest part of each 
            // sidewalk to it.
            collisionResult += CheckIntersection(gameTime, speedDir, sphere, closestBoxLeft);
            collisionResult += CheckIntersection(gameTime, speedDir, sphere, closestBoxRight);

            // updating the closest parts of the sidewalks to the car.
            closestBoxLeft = UpdateClosestBox(closestBoxLeft);
            closestBoxRight = UpdateClosestBox(closestBoxRight);

            return collisionResult;
        }
        private V3 CheckIntersection(GameTime gameTime, float speedDir, BoundingSphere sphere, int boxIndex)
        {
            V3 collisionResult = V3.Zero;
            if (sphere.Intersects(Track.boundingBoxes[boxIndex]))
            {
                // caltulating a vector to represent the sidewalk's direction and normal
                V3 sidewalkVec = Track.boundingBoxes[boxIndex].Min - Track.boundingBoxes[boxIndex].Max;
                V3 sidewalkNormal = new V3(sidewalkVec.Z, 0, -sidewalkVec.X);
                sidewalkNormal.Normalize();
                if (boxIndex % 2 == 0)
                    sidewalkNormal *= -1;
                // determining the angle of the normal with the -Z axis
                float normalAngle = (float)Math.Atan2(-sidewalkNormal.X, -sidewalkNormal.Z) - MathHelper.Pi;
                while (normalAngle > MathHelper.TwoPi) 
                    normalAngle -= MathHelper.TwoPi;
                if (normalAngle < 0)
                    normalAngle += MathHelper.TwoPi;
                // determining the angle between the car's speed vector and the normal
                float angleBetweenSpeedAndNormal = Math.Abs((speedDir - normalAngle)) % MathHelper.Pi /* % MathHelper.PiOver2 */;
                while (angleBetweenSpeedAndNormal > MathHelper.PiOver2)
                    angleBetweenSpeedAndNormal = MathHelper.Pi - angleBetweenSpeedAndNormal;
                //calculating final result
                collisionResult = sidewalkNormal * speed.Length() * 1.6f * (float)Math.Abs(Math.Cos(angleBetweenSpeedAndNormal)) * 
                                  mass / (float)gameTime.ElapsedGameTime.TotalSeconds;
                //next two lines are in case drifting will result in boost
                drifting = false;
                boost = 1;
            }
            return collisionResult;
        }
        private int UpdateClosestBox(int closestBox)
        {
            // for each closestBox of a sidewalk, this function checks if the parts 
            // next to it are now closer, and updates the closestBox if so.
            int prev = (closestBox - 2 + Track.boundingBoxes.Count) % Track.boundingBoxes.Count;
            int next = (closestBox + 2) % Track.boundingBoxes.Count;
            if (V3.Distance(position, (Track.boundingBoxes[prev].Min + Track.boundingBoxes[prev].Max) / 2) <
                V3.Distance(position, (Track.boundingBoxes[closestBox].Min + Track.boundingBoxes[closestBox].Max) / 2))
                closestBox = prev;
            if (V3.Distance(position, (Track.boundingBoxes[next].Min + Track.boundingBoxes[next].Max) / 2) <
                V3.Distance(position, (Track.boundingBoxes[closestBox].Min + Track.boundingBoxes[closestBox].Max) / 2))
                closestBox = next;
            return closestBox;
        }
        void HandleCollision(IDrive other)
        {
            // handles collisions of two cars

            if (other == this || this.collided)
                return;

            BoundingSphere mySphere = model.Meshes[0].BoundingSphere,
                           otherSphere = model.Meshes[0].BoundingSphere;
            mySphere.Radius *= 0.9f;
            otherSphere.Radius *= 0.9f;
            mySphere.Center = this.position;
            otherSphere.Center = other.position;
            if (mySphere.Intersects(otherSphere))
            {
                V3 temp = this.speed;
                this.speed = other.speed;
                other.speed = temp;
                this.collided = true;
                other.collided = true;
            }
        }

        public void BeGone()
        {
            Game1.CallDraw -= this.DrawCar;
            Game1.CallUpdate -= this.Update;
            Car.CheckCollision -= this.HandleCollision;
            Game1.CallCollisionEvents -= this.CallCollisionEvent;
        }
    }
}