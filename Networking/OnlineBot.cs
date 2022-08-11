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
using System.Windows.Forms;

namespace Race
{
    class OnlineBot : Car
    {
        Client client;

        public OnlineBot(Client c, V3 position, BaseKeys keys)
            : base(position, keys)
        {
            client = c;
            client.PositionProcessing = new Data_Handler(this.DataReceived);
        }

        public override void Update(GameTime gameTime)
        {
            client.RecieveData();
            worldMatrix = Matrix.CreateRotationY(MathHelper.Pi) *
                          Matrix.CreateRotationY(rot) *
                          Matrix.CreateScale(1.25f) *
                          Matrix.CreateTranslation(position);
            collided = false;
        }

        public void DataReceived(byte[] data)
        {
            position = new V3(BitConverter.ToSingle(data, 0), 
                              BitConverter.ToSingle(data, 4), 
                              BitConverter.ToSingle(data, 8));
            rot = BitConverter.ToSingle(data, 12);

            worldMatrix = Matrix.CreateRotationY(MathHelper.Pi) *
                          Matrix.CreateRotationY(rot) *
                          Matrix.CreateScale(1.25f) *
                          Matrix.CreateTranslation(position);
        }
    }
}