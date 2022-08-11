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
    public class Skybox
    {
        public static List<Texture2D> textures1, textures2;
        public static Model model1, model2;
        IBasic car;
        bool using1;

        /// <summary>
        /// Class constructor.
        /// Loads the model, initializes the IBasic variable and signs to the d
        /// CallDraw event.
        /// </summary>
        /// <param name="model_path">path to model</param>
        /// <param name="car">object to always move with</param>
        /// <param name="effect">effect to draw with (for model loading)</param>
        public Skybox(string model_path, IBasic car, Effect effect)
        {
            if (model_path == "skybox1/skybox2")
                using1 = true;
            else
                using1 = false;

            if (using1)
            {
                if (model1 == null)
                {
                    model1 = Game1.contentManager.Load<Model>(model_path);
                    textures1 = new List<Texture2D>();
                    foreach (ModelMesh mesh in model1.Meshes)
                        foreach (BasicEffect currentEffect in mesh.Effects)
                            textures1.Add(currentEffect.Texture);

                    foreach (ModelMesh mesh in model1.Meshes)
                        foreach (ModelMeshPart meshPart in mesh.MeshParts)
                            meshPart.Effect = effect.Clone();
                }
            }
            else
            {
                if (model2 == null)
                {
                    model2 = Game1.contentManager.Load<Model>(model_path);
                    textures2 = new List<Texture2D>();
                    foreach (ModelMesh mesh in model2.Meshes)
                        foreach (BasicEffect currentEffect in mesh.Effects)
                            textures2.Add(currentEffect.Texture);

                    foreach (ModelMesh mesh in model2.Meshes)
                        foreach (ModelMeshPart meshPart in mesh.MeshParts)
                            meshPart.Effect = effect.Clone();
                }
            }
            
            this.car = car;

            Game1.CallDraw += new Draw_Handler(DrawSkybox);
        }

        /// <summary>
        /// Draws the object
        /// </summary>
        /// <param name="effect">effec to draw wth</param>
        public void DrawSkybox(Effect effect)
        {
            Model toDraw = (using1 ? model1 : model2);
            Matrix[] xwingTransforms = new Matrix[toDraw.Bones.Count];
            toDraw.CopyAbsoluteBoneTransformsTo(xwingTransforms);
            int i = 0;
            foreach (ModelMesh mesh in toDraw.Meshes)
            {
                foreach (Effect currentEffect in mesh.Effects)
                {
                    currentEffect.CurrentTechnique = currentEffect.Techniques["Textured"];
                    currentEffect.Parameters["xWorld"].SetValue(xwingTransforms[mesh.ParentBone.Index] * 
                                                                Matrix.CreateScale(40) * 
                                                                Matrix.CreateTranslation(car.position));
                    currentEffect.Parameters["xView"].SetValue(Cameras.viewMatrix);
                    currentEffect.Parameters["xProjection"].SetValue(Cameras.projectionMatrix);
                    currentEffect.Parameters["xTexture"].SetValue(using1 ? textures1[i++] : textures2[i++]);
                }
                mesh.Draw();
            }
        }

        public void BeGone()
        {
            Game1.CallDraw -= this.DrawSkybox;
        }
    }
}