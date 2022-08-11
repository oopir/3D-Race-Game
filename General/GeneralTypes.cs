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
using V3 = Microsoft.Xna.Framework.Vector3;
using VPNT = Microsoft.Xna.Framework.Graphics.VertexPositionNormalTexture;

namespace Race
{
    public enum GameState { menu, loading, pre, play, pause, post, disconnected }
    public enum GameType { classic, onlineHost, onlineJoin }

    public delegate void Draw_Handler(Effect effect);
    public delegate void Update_Handler(GameTime gameTime);
    public delegate void Collision_Handler(IDrive car);
    public delegate void Initiate_Collision();
    public delegate void OnClick();

    public interface IBasic
    {
        V3 position { get; set; }
    }

    public interface IDrive
    {
        V3 position { get; set; }
        float rot { get; set; }
        V3 speed { get; set; }
        float FWR { get; set; }
        bool drifting { get; set; }
        float boost { get; set; }
        int closestBoxLeft { get; set; }
        int closestBoxRight { get; set; }
        bool inReverse { get; set; }
        int lap { get; set; }
        bool collided { get; set; }
    }
}