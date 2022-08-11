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
    abstract class BaseKeys
    {
        public abstract bool pressedUp();
        public abstract bool pressedDown();
        public abstract bool pressedLeft();
        public abstract bool pressedRight();
        public abstract bool pressedDrift();
        public virtual void Update() { }
        public virtual void initCar(IDrive car) { }
    }

    class PlayerKeys : BaseKeys
    {
        Keys up, down, left, right, drift;

        public PlayerKeys(Keys up, Keys down, Keys left, Keys right, Keys drift)
        {
            this.up = up;
            this.down = down;
            this.left = left;
            this.right = right;
            this.drift = drift;
        }

        public override bool pressedUp()
        {  
            return Keyboard.GetState().IsKeyDown(up); 
        }
        public override bool pressedDown() 
        { 
            return Keyboard.GetState().IsKeyDown(down); 
        }
        public override bool pressedLeft() 
        { 
            return Keyboard.GetState().IsKeyDown(left); 
        }
        public override bool pressedRight() 
        { 
            return Keyboard.GetState().IsKeyDown(right); 
        }
        public override bool pressedDrift() 
        { 
            return Keyboard.GetState().IsKeyDown(drift); 
        }
    }

    class BotKeys : BaseKeys
    {
        public static string data = "";
        bool up, down, left, right, drift;
        IDrive car;
        float rayAngle, maxSpeed, braveRate;
        int wait = 0;

        public BotKeys(float rayAngle, float maxSpeed, float braveRate)
        {
            up = false; down = false; left = false; right = false; drift = false;
            this.rayAngle = rayAngle;
            this.maxSpeed = maxSpeed;
            this.braveRate = braveRate;
        }

        /// <summary>
        /// Runs the algorithm to decide what the controlled car should
        /// do in order to drive the best she can. 
        /// The car's decisions are made by "pressing" certain keys,
        /// meaning setting a boolean representing the key to "true".
        /// </summary>
        public override void Update()
        {
            // initializing all pressing variables
            up = false; down = false; left = false; right = false; drift = false;

            // creating Ray objects to be aware of anything harmful in the rays' range.
            V3 straightRayVec = V3.Transform(new V3(0, 0, -1), Matrix.CreateRotationY(car.rot));
            Ray leftRay = new Ray(car.position, V3.Transform(straightRayVec, Matrix.CreateRotationY(rayAngle)));
            Ray rightRay = new Ray(car.position, V3.Transform(straightRayVec, Matrix.CreateRotationY(-rayAngle)));
            Ray straightRay = new Ray(car.position, straightRayVec);

            // creating a variable called "distance" to hold the distance from a 
            // possible intersection right in front of the car.
            float? distance = null;
            for (int i = 0; i < Track.boundingBoxes.Count; i++)
            {
                distance = straightRay.Intersects(Track.boundingBoxes[(i + car.closestBoxLeft) % Track.boundingBoxes.Count]);
                if (distance != null)
                    break;
            }

            // finding the points in the game-space where the left ray intersects with 
            // anything and the right ray intersects with anything
            //      the initial points are set to zero; If they stay zero the program can conclude
            //      that the car's rays do not cover both sides of the road, and the car might be 
            //      about to crash into some edge of the road.
            V3 leftHitPoint = V3.Zero, rightHitPoint = V3.Zero;

            for (int i = 0; i < Track.boundingBoxes.Count && leftHitPoint == V3.Zero; i += 2)
                if (leftRay.Intersects(Track.boundingBoxes[(i + car.closestBoxLeft) % Track.boundingBoxes.Count]) != null)
                    leftHitPoint = (Track.boundingBoxes[(i + car.closestBoxLeft) % Track.boundingBoxes.Count].Max +
                                    Track.boundingBoxes[(i + car.closestBoxLeft) % Track.boundingBoxes.Count].Min) / 2;

            for (int i = 0; i < Track.boundingBoxes.Count && rightHitPoint == V3.Zero; i += 2)
                if (rightRay.Intersects(Track.boundingBoxes[(i + car.closestBoxRight) % Track.boundingBoxes.Count]) != null)
                    rightHitPoint = (Track.boundingBoxes[(i + car.closestBoxRight) % Track.boundingBoxes.Count].Max +
                                     Track.boundingBoxes[(i + car.closestBoxRight) % Track.boundingBoxes.Count].Min) / 2;

            // checking if the rays cover both sides of the road, and initializing recovery
            // when the rays don't.
            if (leftHitPoint == V3.Zero)
            {
                down = true;
                if (car.inReverse && car.speed.Length() > 10)
                    right = true;
                if (wait == 0)
                    drift = true;
                wait = 10;
                return;
            }
            if (rightHitPoint == V3.Zero)
            {
                down = true;
                if (car.inReverse && car.speed.Length() > 10)
                    left = true;
                if (wait == 0)
                    drift = true;
                wait = 10;
                return;
            }

            // from this part it is known that the rays cover both sides of the road.
            // next part determines the direction the car should want to go in, and 
            // finally decides which keys to "press".
            V3 movementDir = (rightHitPoint + leftHitPoint) / 2 - car.position;
            float movementDirAngle = (float)Math.Atan2(-movementDir.X, -movementDir.Z);
            while (movementDirAngle < 0)
                movementDirAngle += MathHelper.TwoPi;

            if (wait == 0)
            {
                if (Math.Abs(movementDirAngle - car.rot) > MathHelper.ToRadians(5) && car.speed.Length() > 8)
                {
                    if ((Math.Abs(movementDirAngle - car.rot) > MathHelper.Pi && car.rot - movementDirAngle > 0) ||
                        (Math.Abs(movementDirAngle - car.rot) < MathHelper.Pi && car.rot - movementDirAngle < 0))
                        left = true;
                    if ((Math.Abs(movementDirAngle - car.rot) < MathHelper.Pi && car.rot - movementDirAngle > 0) ||
                        (Math.Abs(movementDirAngle - car.rot) > MathHelper.Pi && car.rot - movementDirAngle < 0))
                        right = true;
                }

                if (distance.GetValueOrDefault() < car.speed.Length() / braveRate)
                    down = true;
                else
                    if (car.speed.Length() < maxSpeed)
                        up = true;
            }
            if (wait > 0) 
                wait--;
        }

        /// <summary>
        /// assigns the keyboard object a reference to the car it controls
        /// </summary>
        /// <param name="car">car connected to the keyboard</param>
        public override void initCar(IDrive car) 
        { 
            this.car = car; 
        }

        public override bool pressedUp() 
        { 
            return up; 
        }
        public override bool pressedDown() 
        { 
            return down; 
        }
        public override bool pressedLeft() 
        { 
            return left; 
        }
        public override bool pressedRight() 
        { 
            return right; 
        }
        public override bool pressedDrift() 
        { 
            return drift; 
        }
    }
}