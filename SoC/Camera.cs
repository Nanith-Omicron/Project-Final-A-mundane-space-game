using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace Omicron
{
    public class Camera
    {
        private Matrix Transform;
        public Matrix GetTransform()
        {
            return Transform;
        }

        private Vector2 Target;
        public Vector2 Offset = Vector2.Zero;
        private Viewport viewport;

        private float zoom =3, rotation = 0;

        public Camera(Viewport v)
        {
            viewport = v;
        }
        public void Update(Vector2 pos)
        {
            Target = pos;
            var e = Matrix.CreateTranslation(new Vector3(-Target.X - Offset.X, -Target.Y - Offset.Y, 0))
                * Matrix.CreateRotationZ(rotation)
                * Matrix.CreateScale(new Vector3(zoom, zoom, 0))
                * Matrix.CreateTranslation(new Vector3(viewport.Width / 2, viewport.Height / 2, 0));

            Transform = Matrix.Lerp(Transform, e, CORE.InterpolatedGameTime * 5);
                
        }
        

    }
}
