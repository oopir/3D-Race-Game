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

namespace Race
{
    static class Cameras
    {
        public static Matrix viewMatrix;
        public static Matrix projectionMatrix;
        public static Quaternion camRotation;
        public static Vector3 camPosition;
        static bool reverse = false;

        public static Vector3 lightDirection = new Vector3(3, -2, 5);

        /// <summary>
        /// assigning initial values to all the class' variables.
        /// </summary>
        public static void InitCameras()
        {
            viewMatrix = Matrix.CreateLookAt(Vector3.Zero, Vector3.Zero, new Vector3(0, 1, 0));
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, Game1.device.Viewport.AspectRatio, 0.2f, 2000f);
            camRotation = Quaternion.Identity;
            camPosition = Vector3.Zero;

            lightDirection.Normalize();
        }

        /// <summary>
        /// updating the camera's view matrix and projection matrix according to 
        /// the given position and rotation of the object it follows.
        /// </summary>
        /// <param name="objectPosition">position of object to follow</param>
        /// <param name="objectRotation">rotation of object to follow</param>
        public static void UpdateCameras(Vector3 objectPosition, Quaternion objectRotation)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.R))
            {
                // updating the camera to reverse mode
                reverse = true;
                camRotation = objectRotation;
                camPosition = objectPosition + new Vector3(0, 2, 0);
                viewMatrix = Matrix.CreateLookAt(camPosition,
                                                 camPosition + Vector3.Transform(new Vector3(0, 0, 1), Matrix.CreateFromQuaternion(camRotation)), 
                                                 new Vector3(0, 1, 0));
            }
            else
            {
                // updating the camera to regular mode
                if (!reverse)
                    camRotation = Quaternion.Lerp(camRotation, objectRotation, 0.075f);
                else
                {
                    reverse = false;
                    camRotation = objectRotation;
                }
                camPosition = objectPosition + Vector3.Transform(new Vector3(0, 4, 15), Matrix.CreateFromQuaternion(camRotation));
                //camPosition = objectPosition + Vector3.Transform(new Vector3(0, 6, 15), Matrix.CreateFromQuaternion(camRotation));
                viewMatrix = Matrix.CreateLookAt(camPosition, objectPosition + new Vector3(0, 3, 0), new Vector3(0, 1, 0));
            }
            
        }

        /// <summary>
        /// updates the camera when in "god mode"; only for debugging!
        /// </summary>
        public static void UpdateCameras()
        {
            viewMatrix = Matrix.CreateLookAt(camPosition - Vector3.Transform(new Vector3(0, 0, -1), Matrix.CreateFromQuaternion(camRotation)),
                                             camPosition, 
                                             new Vector3(0, 1, 0));
        }
    }
}