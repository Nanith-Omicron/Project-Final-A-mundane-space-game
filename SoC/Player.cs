using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


namespace Omicron
{
    public class Player : Actor
    {
        protected CORE.Axis Horizontal, Vertical;
        
    
       
      
        public Player(String s, Texture2D t,CORE.Axis V, CORE.Axis H) : base(s, t)
        { Vertical = V; Horizontal = H;

            this.ps.MaximumSpeed = 6;
            this.ps.Acceleration = 0;
            this.ps.Aerodynamism = 2;
            this.ThrustSpeed = 15f;
            this.Health = 100;
            this.Exhausted = 6;
            this.DebrisColor = Color.Red;

        
            Health = 10;
            BaseHP = 10;
            ps.bounciness = .3f;
        }

        private float Timer = 0, currentime= 0;


        private MouseState LastMouseState;
        public override void OnCollision(GameTime dt)
        {
            var sd = new List<Image>();
            sd.AddRange(CORE.gameObjects);
            sd.AddRange(CORE.Actors);
            sd.AddRange(CORE.OnSCreen);


            for (int i = 0; i < sd.Count; i++)
            {
                var g = sd[i];

                if (rect.Intersects(g.rect) && sd[i] != this)
                    if (this.CheckCollision(sd[i], this, out Vector2 z))
                    {
                    
                        
                        if(g is Projectile)
                        {
                          
                          Projectile f =  (Projectile)g;
                            if (f.Hurtful)
                            {
                                f.rotation += 1.5708f * 2f;
                                f.ps.Velocity = Vector2.Zero;

                                CreateDebris(f.position);
                                TakeDamage(.1f);
                                f.DeleteThis(f);
                                return;
                            }

                        }
                        else if(g is Bonus)
                        {
                            var x = (Bonus)g;
                            switch (x.bonusType)
                            {
                                case Bonus.BonusType.Hp:
                                    Health+= x.BonusIntensity;
                                    if (Health > 10) Health = 10;
                                    break;
                                case Bonus.BonusType.Speed:
                                    Weapon.Rate -= x.BonusIntensity;
                                    Weapon.ThrustSpeed += x.BonusIntensity;
                                    if (Weapon.Rate <= .05f) Weapon.Rate = .05f;

                                    break;
                                case Bonus.BonusType.Damage:
                                    break;
                                default:
                                    break;
                            }
                            x.DeleteThis(x);
                            return;
                        }
                        else if(g is Actor)
                        {
                            var e = (Actor)g;
                            if (e.Health <= 0) return;
                            CheckforGround(this, z);

                            TakeDamage(1);
                            CreateDebris(g.position);
                           
                            ps.Velocity  = z *100;
                            return;
                        }
                    }

            }

        }
     
        public override void Update(GameTime dt)
        {   
            var amount = CORE.InterpolatedGameTime;
            currentime += (float)dt.ElapsedGameTime.TotalSeconds;

            OnCollision(dt);
            if(currentime > Weapon.Rate)
            {
                Timer++;
                currentime -= Weapon.Rate;
            }

         

            var ActualSpeed = this.ThrustSpeed;
            var d = rotation;

            rotation = 0;
           /* ps.RotationSpeed += Horizontal.Update(amount);
           rotation += ps.RotationSpeed  * amount;
            ps.RotationSpeed -= ps.RotationSpeed * .3f;*/
            d = rotation + 1.5708f; 
            var whereImLooking = new Vector2((float)Math.Cos(d), (float)Math.Sin(d));
            var HorizontalSpeed = whereImLooking *Horizontal.Update(amount);//new Vector2(Horizontal.Update(amount), Vertical.Update(amount));
            var VerticalSpeed = whereImLooking * Vertical.Update(amount);
            LastMouseState = Mouse.GetState();
          
            if ((Mouse.GetState().LeftButton == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Space)) && Timer  >= Weapon.Rate  )
            {
                Shoot(false);
                Timer = 0;
            }

            
         /*   ps.Acceleration += ActualSpeed * amount;

            ps.Speed += dir * ps.Acceleration * amount ;*/

            ps.Velocity += new Vector2((HorizontalSpeed *ActualSpeed ).Y, (VerticalSpeed * ActualSpeed ).Y) * 6;

            if (ps.Speed.Length() > ps.MaximumSpeed)
            {
                var t = ps.MaximumSpeed / ps.Speed.Length();
                ps.Speed*= t;
            }
           ps.Speed -= (ps.Speed *  ps.friction / ( +ps.Aerodynamism ))* amount;

            ps.Velocity -= (ps.Velocity * ps.friction/5) * 60 * amount;
           
                AddPropulsion(dt,.5f);

            

            this.position += ps.Velocity  * amount;
            //  this.position.X = MathHelper.Clamp(this.position.X, 0, CORE.graphics.GraphicsDevice.Viewport.Width - this.Texture.Width);
            var Screen = CORE.graphics.PreferredBackBufferWidth/6;
            this.position.X = MathHelper.Clamp(this.position.X, -Screen, Screen);
            this.position.Y = MathHelper.Clamp(this.position.Y, -125,125 );
            if (this.position.X <= -Screen || this.position.X >= Screen)
            {
                ps.Velocity = -ps.Velocity * (.5f +ps.bounciness);
                ps.Speed = ps.Speed * (1 -ps.bounciness);
            }

            //movement

            UpdateRect();

            ps.Acceleration = 0;
        }
    }
}
