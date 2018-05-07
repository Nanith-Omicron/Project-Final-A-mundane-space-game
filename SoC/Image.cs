using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
namespace Omicron
{
    public class Image
    {

        
        public string name;
        public string path;
        public float Alpha = 1;
        public bool CanCollide = true;
        public Matrix transform;
        public Texture2D Texture;
        public Vector2 position;
        public float rotation, scale, Layer = .5f;
        public Vector2 Origin;
        public Color[] colorData;
        public int Height, Width;
        public Rectangle rect;
      
        public Image(Texture2D t, string Name = "null")
        {
            Texture = t;
            this.position = Vector2.Zero;
            this.rotation = 0;
            this.Alpha = 1;
            Origin = new Vector2(t.Width / 2, t.Height / 2);
            this.scale = 1;
            GeneratePixelData(t, out colorData);
            this.name = "";
            
          rect = new Rectangle
            {
                X = (int)position.X,
                Y = (int)position.Y,
                Height = (int)Texture.Height,
                Width = (int)Texture.Width
            };
        }
      

        static void GeneratePixelData(Texture2D t,out Color[] c)
        {          
           c = new Color[t.Width * t.Height];
            t.GetData(c);
        }
    
        public bool CheckCollision(Image a, Image b, out Vector2 Direction)
        {
            
            Matrix AB = a.transform * Matrix.Invert(b.transform);

            Vector2 stepX = Vector2.TransformNormal(Vector2.UnitX, AB);
            Vector2 stepY = Vector2.TransformNormal(Vector2.UnitY, AB);       
            var outD = b.position - a.position;
            outD.Normalize();
            Direction = outD;
            var yPosinB = Vector2.Transform(Vector2.Zero, AB);
            for (int yA = 0; yA < a.Width; yA++)
            {
                Vector2 posInB = yPosinB;
                for (int xA = 0; xA < a.Height; xA++)
                {      
                    int xB = (int)posInB.X; 
                    int yB = (int)posInB.Y; 
                    if (xB < 0 || xB >= b.Width || yB < 0 || yB >= b.Height) continue;
                    Color cA = a.colorData[xA + yA * a.Width]; Color cB = b.colorData[xB + yB * b.Width];
                    if (cA.A != 0 && cB.A != 0) return true;
                    posInB += stepY;
                }
                yPosinB += stepX;
            }
            return false;

        }


        private List<IEnumerator> _coroutines = new List<IEnumerator>();
       protected void UpdateCoroutines()
        {
            var coroutinesToUpd = _coroutines.ToList();
            coroutinesToUpd.ForEach((coroutine) =>
            {
                if (!coroutine.MoveNext())
                    _coroutines.Remove(coroutine);
            });
        }


        public void StartCoroutine(IEnumerator coroutine)
        {
            _coroutines.Add(coroutine);
        }

        public void UpdateRect()
        {
            Width = Texture.Width;
            Height = Texture.Height;

            if (rotation < 0)
            {
                rotation = MathHelper.TwoPi - Math.Abs(rotation);
            }
            else if (rotation > MathHelper.TwoPi)
            {
                rotation = rotation - MathHelper.TwoPi;
            }
            this.Origin = new Vector2(Width / 2, Height / 2);
            transform =
                Matrix.CreateTranslation(new Vector3(-Origin, 0.0f)) 
                * Matrix.CreateRotationZ(rotation) * 
            Matrix.CreateScale(scale)  
                
                * Matrix.CreateTranslation(new Vector3(position, 0))   
            ;
            var e = new Rectangle
            {
                X = 0,
                Y = 0,
                Height = (int)Texture.Height,
                Width = (int)Texture.Width
            };
            rect = BoundingCollision(e);
        }

        public Rectangle BoundingCollision(Rectangle rekt)
        {
            Vector2 Lt = Vector2.Transform( new Vector2(rekt.Left, rekt.Top), transform);
            Vector2 Rt = Vector2.Transform(new Vector2(rekt.Right, rekt.Top), transform);
            Vector2 Lb = Vector2.Transform(new Vector2(rekt.Left, rekt.Bottom), transform);
            Vector2 Rb = Vector2.Transform(new Vector2(rekt.Right, rekt.Bottom), transform);


            Vector2 min = Vector2.Min(Vector2.Min(Lt, Rt), Vector2.Min(Lb, Rb));
            Vector2 max = Vector2.Max(Vector2.Max(Lt, Rt), Vector2.Max(Lb, Rb));
            
            return new Rectangle((int)min.X, (int)min.Y, (int)(max.X - min.X), (int)(max.Y - min.Y));
        }

    
      


    }
}
