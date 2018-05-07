using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Omicron
{
   public class Projectile : Actor
    {
        public float Damage, Rate;
        public int Spread = 0;
        public bool Hurtful = false;

        public Projectile(Texture2D  t) :base (t)
        {
           
        }
        public Projectile(Texture2D t,float damage =1,float rate = 1,int _spread = 0,float Limetime = 5, float Speed = 15) : base(t,Limetime)
        {
            Damage = damage;
            Rate = rate;
            Spread = _spread;
            ThrustSpeed = Speed;
            TimeBeforeBeingDelete = Limetime;
   
          
        }

        
        public override void DeleteThis(Actor a)
        {
            CanCollide = false;
            if (!IsDeath)
            {
               
                if(AnimationSheet.Count > 1)
                currentAnimation = 1;
                TimeDeletion = false;
                lastFrame = -1;
                currentframe = 0;
                rotation -= 1.5708f * 2f;
                Layer = 0f;
                IsDeath = true;
                frameSpeed = 60;
                ps.Velocity = Vector2.Zero;
                return;
            }
            else
            {
                if (CORE.OnSCreen.Contains(this)) CORE.OnSCreen.Remove(this);
                a = null;
            }
           
        }
        
        public override void UpdateAnimations(GameTime dt)
        {

            var a = AnimationSheet[currentAnimation];
           
            lastFrame += dt.ElapsedGameTime.Milliseconds;
            if (lastFrame > frameSpeed)
            {
                lastFrame -= frameSpeed;
                currentframe++;

                lastFrame = 0;
               
                if (currentframe >= a.totalFrames && !IsDeath) currentframe = 0;

            }


            int row = (int)(float)currentframe / a.colums;
            int colum = currentframe % a.colums;
            Rectangle s = new Rectangle(a.Width * colum, a.Height * row, a.Width, a.Height);


            var e = GetAnimations()[currentAnimation];
            var pos = position;//+ new Vector2(0,a.Height/1.2f);
            var f = new Rectangle((int)pos.X, (int)pos.Y, e.Width, e.Height);
            // CORE.spriteBatch.Draw(e.sheet, f, s, Color.White, rotation, Origin, SpriteEffects.None, Layer);
            CORE.spriteBatch.Draw(e.sheet, position, s, color, rotation, Origin, scale, SpriteEffects.None, Layer);

            if (IsDeath && currentframe > a.totalFrames) DeleteThis(this);
        }
        public override void OnCollision(GameTime dt)
        {
            if (!IsONScreen()) return;
                 var sd = new List<Image>();
            sd.AddRange(CORE.gameObjects);
            sd.AddRange(CORE.Actors);
      

            for (int i = 0; i < sd.Count; i++)
            {
                var g = sd[i];

                if (rect.Intersects(g.rect) && sd[i] != this)
                    if (this.CheckCollision(sd[i], this, out Vector2 z))
                    {
                        if (g is Bonus) return;
                        if(g is Actor )
                        {
                            var e = g as Actor;
                            if (parent == e ) continue;
                           
                        }
                        else
                        {
                            CheckforGround(this, z);
                            rotation += 1.5708f * 2f;
                            //DeleteThis(this);
                        }
                       

                    }

            }

        }

        public override void Update(GameTime dt)
        {
         
            this.rotation = (float)Math.Atan2(ps.Velocity.X, -ps.Velocity.Y);
            base.Update(dt);
            if (!IsONScreen() || position.Y > 130 || position.Y < -135 || Math.Abs(position.X) > 75) DeleteThis(this);
            
        }
    }
}
