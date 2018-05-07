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
   public interface Ianimable
    {


        Animations.animation[] GetAnimations();
        int CurrentAnimation();
        Image GetImage();
        void UpdateAnimations( GameTime dt);
    }


    public class Animations
    {
        public struct animation
        {
            public Texture2D sheet;
            public int colums, row;
            public int totalFrames;
            public int Width, Height;
     
           
            public animation(Texture2D Sheet, int Rows, int Collumns)
            {
                this.row = Rows;
                this.colums = Collumns;
                this.sheet = Sheet;
                Width = sheet.Width / colums;
                Height = sheet.Height / row;
                totalFrames = row * colums;
           

            }

        }


    
     



    }
}
