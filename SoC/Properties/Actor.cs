using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System.Threading.Tasks;
using System.Diagnostics;

namespace Omicron
{

    public class Actor : Image, Ianimable
    {
        public const float SCREENPADDING = 95;


        public delegate void DamageHandler(float f, Actor a = null);
        public event DamageHandler OnTakeDamage = delegate { };



        public enum Type { Inert = 0, PopUp = 1, Follow =2,Laser = 3, Chaser =4, Sider = 5 }
        public Type Brain;
        protected bool _Enabled;
        public bool IsEnabled()
        {
            return _Enabled;
        }
   

        public const float DEF_FRICTION = .5f;
        public const float GRAVITY = 19.8f;
        public Image parent;
        public float Health = 10;
        public double RandomSeed;
        public const float MINIMUMBOUNCINESS = .3f;
        public bool OnGround, Attackable = true;
        public Color color = Color.White;
        public Color[] Flashes;
        public Animations.animation DeathPixel;
        public Color DebrisColor = Color.Red;
        public Actor[] Debris;
        public bool TimeDeletion, Flash = false, ShrinkOVertime = false, UseAnimations = false;
        public float TimeBeforeBeingDelete = 3, FlashTime = .35f;
        protected float TimerSinceAlive = 0;
        public bool IsDeath;
        GameTime InternalClock;
        public Projectile Weapon;
        public struct Physics
        {
            public float Acceleration, friction;
            public Vector2 Velocity;
            public Vector2 Speed;
            public float GravitySpeed, RotationSpeed;
            public float bounciness;
            public float MaximumSpeed;
            public float Aerodynamism;
        }


        public Actor(Texture2D t, float lifeTime = 1, Type AiType = Type.Inert, float HP = 10) : base(t)
        {

            this.TimerSinceAlive = lifeTime;
            this.TimeBeforeBeingDelete = lifeTime;
            RandomSeed = new Random().NextDouble();
            currentime = (float)RandomSeed;
            Timer = -(float)RandomSeed;
            Brain = AiType;
            _Enabled = true;
            BaseHP = HP;
            Health = HP;

        }

        public Actor(Actor a) : base(a.Texture)
        {
            this.RandomSeed = new Random().NextDouble();
            this.Weapon = a.Weapon;
            this.Brain = a.Brain;
            ps.friction = Actor.DEF_FRICTION;
            Debris = a.Debris;
            CanCollide = a.CanCollide;
            Layer = a.Layer;
            scale = a.scale;
            ThrustSpeed = a.ThrustSpeed;
            Health = a.Health;
            Propulsion = a.Propulsion;
            Weapon = a.Weapon;
            DebrisColor = a.DebrisColor;
            Exhausted = a.Exhausted;
            BaseHP = a.Health;
            ded = false;
            _Enabled = true;
        }


        private bool ded = false;public float BaseHP;
        public void TakeDamage(float damage)
        {
            if (_Enabled)
            {
                this.Health -= damage;
                StartCoroutine(Blink(Color.Red, 5f));
              
                if (Health <= 0)
                {
                    color = Color.White;
                    DeathTimer = new Timer(3f, InternalClock);
                    ded = true;
                    CanCollide = false;
                    OnTakeDamage(BaseHP,this);
                }

            }
            else if (this is Player)
            {
                Health -= damage;
            }

        }

        private IEnumerator Blink(Color c,float time)
        {
            var e = color;
            this.color = c;
            float t = 0;
            while (t < time)
            {
                t += InternalClock?.ElapsedGameTime.Milliseconds ?? 0;    
                yield return null;

            }
            this.color = e;

            yield break;
        }
    
        public static void CheckforGround(Actor a, Vector2 z)
        {
            
                var x = a.ps.Velocity.Length() * z;
                a.ps.Velocity += x;
                a.ps.Speed = Vector2.Zero;           
        }
        public void Shoot(bool hurt = true)
        {
            if (position.Y < -136) return;
            var d = (rotation + +1.5708f);//(rotation + 90) * (float)(Math.PI / 180);
            var r = new Random();
            var px = new Projectile(Weapon.Texture, _spread: Weapon.Spread, Limetime: Weapon.TimeBeforeBeingDelete,Speed: Weapon.ThrustSpeed);
            var fg = (r.Next(-px.Spread , px.Spread)) * 0.0174533f + d;
            var whereIamLooking  = new Vector2((float)Math.Cos(fg), (float)Math.Sin(fg));
            
          
            px.Damage = Weapon.Damage;
            px.Spread = Weapon.Spread;
            px.parent = this;
            if (Weapon.AnimationSheet.Count > 0) px.AnimationSheet = Weapon.AnimationSheet;
           
         
            TimerSinceAlive = Weapon.TimeBeforeBeingDelete;

            px.color = Color.White;
            px.position = this.position + whereIamLooking * .4f;
            px.TimeBeforeBeingDelete = Weapon.TimeBeforeBeingDelete;


            px.UseAnimations = true;

            px.Hurtful = Weapon.Hurtful;
            px.TimeDeletion = true;
            px.rotation = fg - 1.5708f;
      
            px.scale = Weapon.scale;

            px.ps.Velocity = -whereIamLooking * px.ThrustSpeed + ps.Velocity * .05f;
          
            CORE.OnSCreen.Add(px);
        }
        protected float currentime;
        public bool IsONScreen()
        {

              var Xpos = CORE.graphics.PreferredBackBufferWidth / 6 + SCREENPADDING;
              var Ypos = 125 + SCREENPADDING;
              return position.X > -Xpos && position.Y < Xpos && position.Y > -Ypos && position.Y < Ypos;
            
        }
        float InternalTimer = 0; bool side = false;
        public virtual void AI( float aggroDistance = 3)
        {
            InternalTimer += CORE.InterpolatedGameTime; 
            if (ded) return;
            var d = rotation + 1.5708f; ;
            var pos = CORE.MP.position - position;
            pos.Normalize();
            var whereImLooking = new Vector2((float)Math.Cos(d), (float)Math.Sin(d));
            switch (Brain)
            {
                case Type.Inert:
                    break;
                case Type.PopUp:
                    if (InternalTimer < .25f)
                    {
                        this.rotation = 2 * 1.5708f;
                        this.ps.Speed = Vector2.UnitY * 50f;
                    }

                    else if (InternalTimer < 15)
                    {
                        ps.Speed = Vector2.Zero;

                        if (InternalTimer >10)
                        {
                            currentime += CORE.InterpolatedGameTime;
                          
                            this.rotation = MathHelper.Lerp(rotation, ((float)Math.Atan2(pos.Y, pos.X)) + 1 * 1.5708f, .8f * CORE.InterpolatedGameTime);
                            if (currentime > Weapon.Rate)
                            {
                                Timer++;
                                currentime -= Weapon.Rate;
                            }
                            if (Timer >= Weapon.Rate)
                            {
                                Shoot(true);
                                Timer = 0;
                            }

                        }

                    }
                    else
                    {

                        this.ps.Speed = -whereImLooking * 9;
                    }
                    var hte = MathHelper.Clamp(ps.Velocity.Length() / 4, 0, 1.3f);

                    AddPropulsion(InternalClock, hte);

                    break;
                case Type.Follow:
                    currentime += CORE.InterpolatedGameTime;
                    this.rotation += CORE.InterpolatedGameTime * .6f * ThrustSpeed;
          

                    this.ps.Speed =  Vector2.UnitX * ThrustSpeed;
                    if (currentime > Weapon.Rate)
                    {
                        Timer++;
                        currentime -= Weapon.Rate;
                    }
                    if (Timer >= Weapon.Rate)
                    {
                        Shoot(true);
                        Timer = 0;
                    }
                    break;
                case Type.Laser:
                    currentime += CORE.InterpolatedGameTime;
                    if (InternalTimer < 2)
                    {
                        this.rotation = 2 * 1.5708f;
                        this.ps.Speed = Vector2.UnitY * 5f;
                    }
                    else if(InternalTimer < 15)
                    {
                        this.rotation = 2 * 1.5708f;
                        var ee = Vector2.UnitX * (CORE.MP.position.X - position.X);
                        ee.Normalize();
                        ps.Speed = ee;

                        if (currentime > Weapon.Rate)
                        {
                            Timer++;
                            currentime -= Weapon.Rate;
                        }
                        if (Timer >= Weapon.Rate)
                        {
                            Shoot(true);
                            Timer = 0;
                        }
                    }
                    else
                    {

                        this.ps.Speed = whereImLooking *11;
                    }
                    break;
                case Type.Chaser:
                    currentime += CORE.InterpolatedGameTime;
                    if (InternalTimer < 2)
                    {
                        this.rotation = 2 * 1.5708f;
                        this.ps.Speed = Vector2.UnitY * 5f;
                    }
                    else if (InternalTimer < 6)
                    {
                        this.rotation = MathHelper.Lerp(rotation, ((float)Math.Atan2(pos.Y, pos.X)) + 1 * 1.5708f, 1f * CORE.InterpolatedGameTime);

                        var ee = Vector2.UnitX * (CORE.MP.position.X - position.X);
                        ee.Normalize();
                        ps.Speed = ee;
                        this.ps.Speed = -whereImLooking * 6;
                     
                    }
                    else if (InternalTimer < 7)
                    {
                        if (currentime > Weapon.Rate)
                        {
                            Timer++;
                            currentime -= Weapon.Rate;
                        }
                        if (Timer >= Weapon.Rate)
                        {
                            Shoot(true);
                            Timer = 0;
                        }
                    }
                    else
                    {

                        this.ps.Speed = whereImLooking * 17;
                    }

                    break;
                case Type.Sider:
                    currentime += CORE.InterpolatedGameTime;
                    if (InternalTimer < 3)
                    {
                        this.rotation = 3 * 1.5708f;
                        this.ps.Speed = Vector2.UnitX *ThrustSpeed;
                    }
                    else if (InternalTimer < 5)
                    {
                        this.rotation = 3 * 1.5708f;
                        this.ps.Speed = -Vector2.UnitX * ThrustSpeed;
                    }
                
                    else if (InternalTimer < 7)
                    {
                        this.rotation = this.rotation = MathHelper.Lerp(rotation, ((float)Math.Atan2(pos.Y, pos.X)) + 1 * 1.5708f + CORE.InterpolatedGameTime, 1f * CORE.InterpolatedGameTime);

                        var ee = Vector2.UnitX * (CORE.MP.position.X - position.X);
                        ee.Normalize();
                        ps.Speed = ee;
                     

                    }
                    else if (InternalTimer < 13)
                    {

                    }
                    else if (InternalTimer < 22)
                    {
                        this.ps.Speed = -Vector2.UnitX * ThrustSpeed;
                        if (currentime > Weapon.Rate)
                        {
                            Timer++;
                            currentime -= Weapon.Rate;
                        }
                        if (Timer >= Weapon.Rate)
                        {
                            Shoot(true);
                            Timer = 0;
                        }
                    }
                    else
                    {

                        this.ps.Speed = whereImLooking * 17;
                    }

                    break;
                default:
                    break;
            }
          
        

        }
   

        public static void AddGravity(Actor a)
        {
            a.ps.GravitySpeed += GRAVITY * CORE.InterpolatedGameTime;
            a.ps.Velocity += new Vector2(0, a.ps.GravitySpeed) * CORE.InterpolatedGameTime * (1 + a.ps.friction);
        }
        public float Exhausted = 13f; public Actor Propulsion;

        int  NumberOfSide = 7;
        int _CacheNumberOfSide = 0;
        public void CreateDebris(Vector2 pos, float Number = 2, float Intensity = 1)
        {
            if (Debris == null) return;
            var r = new Random();
            for (int i = 0; i < Number; i++)
            {
                _CacheNumberOfSide++;
                var e = r.Next(0,4);
                var x = new Vector2(r.Next(-1, 1), r.Next(-1, 1));
                var s = new Actor(Debris[e].Texture, Debris[e].TimeBeforeBeingDelete)
                {

                    color = DebrisColor,
                    CanCollide = false,
                    position = pos + x * r.Next(1, 10) / 10,
                    TimeDeletion = true,
                    ShrinkOVertime = false,
                    TimeBeforeBeingDelete = 5,
                  
                   
                };
                if (i % 2 == 0) s.color = Color.Gray;
                var f = (pos - position);
                f.Normalize();
                s.ps.friction = .5f;
                s.ps.Velocity = ((f * 7+ x * 2 )* 1.25f) * Intensity + ps.Velocity;
                s.rotation = (float)(r.NextDouble() * Math.PI);
                s.scale = 1f;
                s.UpdateRekt = false;

                CORE.VFX.Add(s);
                

            }
            if (_CacheNumberOfSide > NumberOfSide) _CacheNumberOfSide = 0;

        }
        public void AddPropulsion( GameTime dt,float size = 1)
        {
            var d = rotation + 1.5708f;;
          
            var whereImLooking = new Vector2((float)Math.Cos(d), (float)Math.Sin(d));
            bool coolide = true;
            var dir = -whereImLooking;
            float speed = 9;
            for (int i = 0; i < 1; i++)
            {
                Random r = new Random();
                var gf = new Vector2(-dir.Y, dir.X);
                try
                {
                    var s = new Actor(Propulsion.Texture, Propulsion.TimeBeforeBeingDelete)
                    {

                        color = Color.WhiteSmoke,
                        CanCollide = coolide,
                        position = this.position - dir * Exhausted + r.Next(-5, 5) * gf,
                        TimeBeforeBeingDelete = Propulsion.TimeBeforeBeingDelete,
                        TimeDeletion = true,
                        ShrinkOVertime = true
                    };
                    s.ps.Velocity += -dir * speed * dir.Length() * r.Next(7, 12) / 10 + ps.Velocity * .017f;

                    s.scale = scale * (.15f * size * r.Next(5, 15) / 10);
                    s.CanCollide = false;
                    s.UpdateRekt = false;
                    CORE.VFX.Add(s);
                }
                catch (Exception)
                {

                    Propulsion = CORE.MP.Propulsion;
                }
            

             
            }
            if (this is Player)
            {
                for (int i = 0; i < 1; i++)
                {
                    Random r = new Random();
                    var gf = new Vector2(-dir.Y, dir.X);

                    var s = new Actor(Propulsion.Texture, Propulsion.TimeBeforeBeingDelete)
                    {

                        color = Color.OrangeRed,
                        CanCollide = coolide,
                        position = this.position - dir * Exhausted + r.Next(-5, 5) * gf,
                        TimeDeletion = true,
                        ShrinkOVertime = true
                    };

                    s.ps.Velocity += -dir * speed * dir.Length() * r.Next(7, 12) / 10 + ps.Velocity * .017f;

                    s.scale = .2f * size * r.Next(5, 15) / 10;

                    CORE.VFX.Add(s);
                }
            
                for (int i = 0; i < 1; i++)
                {
                    Random r = new Random();
                    var gf = new Vector2(-dir.Y, dir.X);

                    var s = new Actor(Propulsion.Texture, Propulsion.TimeBeforeBeingDelete)
                    {

                        color = Color.LightGoldenrodYellow,
                        CanCollide = coolide,
                        position = this.position - dir * Exhausted + r.Next(-5, 5) * gf,
                        TimeDeletion = true,
                        ShrinkOVertime = true
                    };

                    s.ps.Velocity += -dir * speed * dir.Length() * r.Next(7, 12) / 10 + ps.Velocity * .017f;

                    s.scale = .2f * size * r.Next(5, 15) / 10;

                    CORE.VFX.Add(s);
                }
                for (int i = 0; i < 1; i++)
                {
                    Random r = new Random();
                    var gf = new Vector2(-dir.Y, dir.X);

                    var s = new Actor(Propulsion.Texture, Propulsion.TimeBeforeBeingDelete)
                    {

                        color = Color.Orange,
                        CanCollide = coolide,
                        position = this.position - dir * Exhausted + r.Next(-5, 5) * gf,
                        TimeDeletion = true,
                        ShrinkOVertime = true
                    };

                    s.ps.Velocity += -dir * speed * dir.Length() * r.Next(7, 12) / 10 + ps.Velocity * .017f;

                    s.scale = .2f * size * r.Next(5, 15) / 10;

                    CORE.VFX.Add(s);
                }
            }    
            lastFrame += dt.ElapsedGameTime.Milliseconds;
            if (lastFrame > frameSpeed)
            {
                lastFrame -= frameSpeed;
                currentframe++;

                lastFrame = 0;
                if (currentframe >= 10)
                {

                    currentframe = 0;
                }

            }

        }
        //ANIMATIONS

        public int lastFrame, frameSpeed = 20;
        public List<Animations.animation> AnimationSheet = new List<Animations.animation>();
        public int currentAnimation;

        public Image GetImage()
        {
            return this;
        }
        public int CurrentAnimation()
        {
            return currentAnimation;
        }
        public Animations.animation[] GetAnimations()
        {
            return AnimationSheet.ToArray();
        }


       public int currentframe;

        public virtual void DeleteThis(Actor a)
        {
            CanCollide = false;
            this._Enabled = false;
            
            if (CORE.VFX.Contains(this)) CORE.VFX.Remove(this);
            if (CORE.Actors.Contains(this)) CORE.Actors.Remove(this);
            
            
            a = null;
           
         
        }
        
        public virtual void UpdateAnimations(GameTime dt)
        {
            if (!_Enabled) return;


            var a = AnimationSheet[currentAnimation];
            
            lastFrame += dt.ElapsedGameTime.Milliseconds;
            if (lastFrame > frameSpeed)
            {
                lastFrame -= frameSpeed;
                currentframe++;
                
                lastFrame = 0;
                if (currentframe >= a.totalFrames) currentframe = 0;
             
            }
           

            int row = (int)(float)currentframe / a.colums;
            int colum = currentframe % a.colums;
            Rectangle s = new Rectangle(a.Width * colum, a.Height * row, a.Width, a.Height);

            
            var e = GetAnimations()[currentAnimation];
            var pos = position;//+ new Vector2(0,a.Height/1.2f);
            var f = new Rectangle((int)pos.X, (int)pos.Y, e.Width, e.Height);
            CORE.spriteBatch.Draw(e.sheet, position, s, color, rotation, Origin, scale,SpriteEffects.None,Layer);

        }
        public virtual void OnCollision(GameTime dt)
        {
            if (!CanCollide) return;
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
                        var e = this is Bonus;
                        if (g is Projectile && Attackable && !e  )
                        {
                            
                            Projectile f = (Projectile)g;
                            if (!f.Hurtful && f.parent!= this)
                            {
                                f.rotation += 1.5708f * 2f;
                                f.Layer = .5f;
                                TakeDamage(f.Damage);
                                CreateDebris(f.position);
                                f.DeleteThis(f);
                            }



                        }
                        else
                        {
                          //  CheckforGround(this, z);

                        }
                    }

            }

        }
        public bool UpdateRekt = true;
        public virtual void Update(GameTime dt)
        {

            if (!_Enabled) return;
            InternalClock = dt;
            var NextTime = InternalClock.ElapsedGameTime.Seconds;
                UpdateRect();
            UpdateCoroutines();
            if(IsONScreen())
         AI();
            if (CanCollide) OnCollision(dt);
           
            if (Flash) GoFlash();
            if (TimeDeletion)
            {
                TimerSinceAlive -= CORE.InterpolatedGameTime;
                if(ShrinkOVertime) this.scale -= CORE.InterpolatedGameTime * 0.05f;
                if(color.A > 0)
                    
                this.color.A-= (byte)2;
                if (TimerSinceAlive <= 0)
                {

                  
                    DeleteThis(this);
                }
            }

          
            float amount = CORE.InterpolatedGameTime;
            this.position += ps.Velocity * amount;
       
            ps.Velocity += ps.Speed * amount;
            ps.Velocity += ps.Velocity * ps.Acceleration / 100 * amount;
            ps.Velocity -= ps.Velocity * ps.friction / 3 * amount;
            if (ps.Speed.Length() > ps.MaximumSpeed) ps.Speed *= ps.MaximumSpeed / ps.Speed.Length();




            //movement
            //this.position += ps.Velocity * amount;

            var screen = CORE.graphics.PreferredBackBufferWidth / 6;
            this.position = Vector2.Lerp(position, position + ps.Velocity, amount);
          //  this.position.X = MathHelper.Clamp(this.position.X, -screen, screen);


            ps.Acceleration = 0;
            if (!IsONScreen() || Math.Abs(position.Y) > 200 || Math.Abs(position.X) > 100) if(!Eternal) DeleteThis(this);
            if (ded) OnDeath(NextTime);








        }
       public bool Eternal = false;
        float dedtimer = 0,debristimer = 0;
        int colorOfDed = 0;
        public virtual void OnDeath(float NextTime)
        {


       
    
            var a = (360 / NumberOfSide) * _CacheNumberOfSide;

            DeathTimer.OnTimerFinish += E_OnTimerFinish;
            dedtimer += CORE.InterpolatedGameTime;
            debristimer += CORE.InterpolatedGameTime;
            if (!DeathTimer.IsTerminated())

                if (NextTime % 10 == 0)
                {

                    var size = 1f;
                    var e = new Random();
        
                    var coolide = false;
                    if(dedtimer > .2)
                    {
                        for (int i = 0; i < 1; i++)
                        {
                            Random r = new Random();
                            var dir = new Vector2(e.Next(-10, 10), e.Next(-10, 10)) * .5f;
                            var col = Color.White;
                            switch (colorOfDed)
                            {
                               
                                case 1: col=  Color.White;break;
                                case 2: col = Color.Orange; break;
                                case 3: col = Color.OrangeRed; break;
                                case 4: col = Color.Yellow; colorOfDed = 0; break;
                                default:
                                    col = Color.White;

                                    break;
                            }
                        /*    try
                            {
                                var s = new Actor(Propulsion.Texture, Propulsion.TimeBeforeBeingDelete)
                                {

                                    color = col,
                                    CanCollide = coolide,
                                    position = this.position + dir,
                                    TimeDeletion = true,
                                    ShrinkOVertime = true
                                                        ,
                                    Layer = .1f
                                };
                                s.scale = .2f * size * r.Next(5, 15) / 10;

                                CORE.VFX.Add(s);
                            }
                            catch (Exception)
                            {

                                Propulsion.Texture = CORE.MP.Propulsion.Texture;
                            }*/
                    

                           
                        }
                        colorOfDed++;
                        dedtimer = 0;
                    }
         
                 if(DeathTimer.ElapsedTime > 2)
                    {
                        var aw = new Vector2((float)Math.Cos(a), (float)Math.Sin(a));
                        if (debristimer > .3f)
                        {
                            CreateDebris(position + aw * .1f, 1, .4f);
                            debristimer = 0;
                        }
                     
                    }
               
                    
                }
            DeathTimer.Update();
            this.rotation += MathHelper.ToRadians(4);
            if (DeathTimer.IsTerminated()) DeleteThis(this);

        }
        Timer DeathTimer;
        private void E_OnTimerFinish()
        {
            DeleteThis(this);
        }
        
        private float Timer; private int currentFlash = 0;
        public void GoFlash()
        {
            Timer += CORE.InterpolatedGameTime;
            if(Timer > FlashTime)
            {
                currentFlash++;
                if (currentFlash > Flashes.Length-1) currentFlash = 0;
                color = Flashes[currentFlash];
               
                Timer = 0;
            }


        }
        public Actor(String s, Texture2D t) : base(t,s) { }
      
        public float ThrustSpeed = 20;
        public Physics ps;
        
        public int Orientation = 0;

        public Vector2 Direction = Vector2.Zero;


    }
}
