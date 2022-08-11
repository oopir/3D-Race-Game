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
    class OnlineCar : Car
    {
        Client client;

        public OnlineCar(Client c, V3 position, BaseKeys keys)
            : base(position, keys)
        {
            client = c;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            byte[] toSend = new byte[16];

            byte[] aFloat = BitConverter.GetBytes(position.X);
            for (int i = 0; i < 4; i++) 
                toSend[i] = aFloat[i];
            aFloat = BitConverter.GetBytes(position.Y);
            for (int i = 0; i < 4; i++)
                toSend[i + 4] = aFloat[i];
            aFloat = BitConverter.GetBytes(position.Z);
            for (int i = 0; i < 4; i++)
                toSend[i + 8] = aFloat[i];
            aFloat = BitConverter.GetBytes(rot);
            for (int i = 0; i < 4; i++)
                toSend[i + 12] = aFloat[i];

            client.SendData(toSend);
        }
    }
}