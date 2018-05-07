using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
namespace Omicron
{
    class Tile : Image
    {
        public Tile(Vector2 pos,Texture2D t) :base(t)
        {
            this.position = pos;
            this.Texture = t;
            Weight = 1;

        }
       public float Weight = 5;
    }
}
