using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Input;
using Mathf = Microsoft.Xna.Framework.MathHelper;
using System.Collections.Generic;
using System;
using System.IO;
using System.Text;

namespace Omicron
{

    /// <summary>
    /// This is the main type for your game.
    /// </summary>-
    public class CORE : Game
    {
       
        SpriteFont main;
        public static List<Actor> EnemyLib = new List<Actor>();
        
        
        public static GraphicsDeviceManager graphics;
        public static SpriteBatch spriteBatch;
        public static List<Image> gameObjects = new List<Image>();
        public static Player MP;

        static Camera camera;
        public static float InterpolatedGameTime = 0;
        public CORE()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        
        int screenHeight, screenWidth;
        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {


            // TODO: Add your initialization logic here
            screenHeight = 800;
            screenWidth = 400;
            graphics.PreferredBackBufferHeight = screenHeight;
            graphics.PreferredBackBufferWidth = screenWidth;
            graphics.IsFullScreen = false;
            graphics.ApplyChanges();

            
            base.Initialize();
        }

        Actor Hyper, Planet, Star, Sun, boss,LastRecord;
        Image HPBar, HPFrame;
        Song Theme;
        Bonus HPB, RateB;
        int _lastrecord = -1;
        List<Actor> Stars = new List<Actor>();
        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            camera = new Camera(GraphicsDevice.Viewport);
            // camera.Offset = Vector2.UnitY * -100;

            HPBar = new Image( Content.Load<Texture2D>("HP_Bar"));
            HPFrame = new Image(Content.Load<Texture2D>("HP_Frame"));


            LastRecord = new Actor(Content.Load<Texture2D>("record"), 999, Actor.Type.Inert);
            LastRecord.CanCollide = false;
            
            var roamingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var filePath = Path.Combine(roamingDirectory, "Omicron");
            Directory.CreateDirectory(filePath);
           

            try
            {
                using (StreamReader sr = new StreamReader(filePath + "\\highscore.txt"))
                {
                    string ln;
                    while ((ln = sr.ReadLine()) != null)
                    {
                        if (ln.Contains("RECORD"))
                        {
                            var ars = ln.Split('-');
                            if (ars.Length > 1)
                                _lastrecord = int.Parse(ars[1]);
                        }
                    }

                }
            }
            catch
            {
                
            }
            


            HPB = new Bonus(Content.Load<Texture2D>("Bonus_1"), 1, Bonus.BonusType.Hp);
            RateB = new Bonus(Content.Load<Texture2D>("Bonus_2"), .03f, Bonus.BonusType.Speed);

            Theme = Content.Load<Song>("theme");
            MediaPlayer.Play(Theme);
            Hyper = new Actor(Content.Load<Texture2D>("hyperdrive"), 999, Actor.Type.Inert);
            Hyper.CanCollide = false;
            Hyper.Layer = 0.01f;


            Sun = new Actor(Content.Load<Texture2D>("Sun"), 999, Actor.Type.Inert);
            Sun.CanCollide = false;
            Sun.Layer = 1.1f;
            Sun.position = new Vector2(15, -190);
            Sun.color = Color.Gray;

            Star = new Actor(Content.Load<Texture2D>("Star"), 999, Actor.Type.Inert);
            Star.CanCollide = false;
            Star.Layer = 1.11f;


            var r = new Random();
            for (int i = 0; i < 500; i++)
            {
                var e = new Actor(Star);
              
                e.position = new Vector2(r.Next(-70, 70), r.Next(-160, 170));
                e.color = Color.Gray;

                if (i % 3 == 0) e.scale = .5f;
                else if (i % 5 == 0) e.scale = 1.2f;

                if (i % 4 == 0)
                {
                    e.color = Color.Beige;
                }
                else if (i % 6 == 0) e.color = Color.LightYellow;
                else if (i % 7 == 0) e.color = Color.White;
                e.color *= .4f;
                e.Eternal = true;
                Stars.Add(e);
            }
            Planet = new Actor(Content.Load<Texture2D>("planet"), 9999, Actor.Type.Inert);
            Planet.CanCollide = false;
            Planet.Layer = 1.1f;
            Planet.scale = 2;
            Planet.position = new Vector2(60, -150);

            VFX.Add(Sun);
            VFX.Add(Planet);


            MP = new Player("MAIN PLAYER", Content.Load<Texture2D>("PSHIP_COLLIDER_I"),Vertical,Horizontal);
            MP.Propulsion = new Actor(Content.Load<Texture2D>("Diesel_Propulsion"),4);
            MP.Propulsion.TimeBeforeBeingDelete = 3;
            MP.Propulsion.CanCollide = false;
            MP.scale = .75f;
            MP.Weapon = new Projectile(Content.Load<Texture2D>("Laser_1_Bound"),2,.4f,Limetime:15,Speed:35);
         
            Animations.animation LaserAnimation1 = new Animations.animation(Content.Load<Texture2D>("Laser_1_Animations"), 2, 2);
            Animations.animation LaserContactAnimation = new Animations.animation(Content.Load<Texture2D>("Laser_1_Contact_Animation"), 3,2);
            MP.Weapon.AnimationSheet.Add(LaserAnimation1);
            MP.Weapon.AnimationSheet.Add(LaserContactAnimation);
          

            Actor[] Metal_Bits = new Actor[5];
            float lifetime = 10;
            for (int i = 0; i < 5; i++)
            {
                Metal_Bits[i] = new Actor(Content.Load<Texture2D>("Metal_Bits_" + (i + 1)), lifetime);
                Metal_Bits[i].CanCollide = false;

            }


            MP.Debris = Metal_Bits;
            MP.Weapon.scale =.75f;

            var x = graphics.GraphicsDevice.Viewport;      
                var OwO = new Actor(Content.Load<Texture2D>("Enemy_1"),AiType:Actor.Type.PopUp,HP:5);
                OwO.Debris = Metal_Bits;
            OwO.scale = .5f;
                OwO.Propulsion = MP.Propulsion;
                OwO.Weapon = new Projectile(Content.Load<Texture2D>("Laser_1_Bound"),1,1.01f, Limetime: 12,Speed: 15);
                OwO.Weapon.Layer = .7f;
            
                OwO.Weapon.AnimationSheet.Add(LaserAnimation1);
                OwO.Weapon.color = Color.Red;
                OwO.Weapon.Hurtful = true;
                OwO.DebrisColor = Color.DarkGreen;
                OwO.Weapon.AnimationSheet.Add(LaserContactAnimation);
                OwO.Weapon.scale = .5f;
                OwO.ps.RotationSpeed = .2f;
                OwO.ps.friction = .7f;
                OwO.ThrustSpeed = 3f;
                OwO.OnTakeDamage += OwO_OnTakeDamage;
            OwO.Exhausted = 2f;
              
            EnemyLib.Add(OwO);


            var OwO2 = new Actor(Content.Load<Texture2D>("Enemy_2"), AiType: Actor.Type.Follow, HP: 15);
            OwO2.Debris = Metal_Bits;
            OwO2.scale = .5f;
            OwO2.Propulsion = MP.Propulsion;
            OwO2.Weapon = new Projectile(Content.Load<Texture2D>("Laser_1_Bound"), 1, .50f, Limetime:12);
            OwO2.Weapon.Layer = .7f;

            OwO2.Weapon.AnimationSheet.Add(LaserAnimation1);
            OwO2.Weapon.color = Color.Red;
            OwO2.Weapon.Hurtful = true;
            OwO2.DebrisColor = Color.Blue;
            OwO2.Weapon.AnimationSheet.Add(LaserContactAnimation);
            OwO2.Weapon.scale = .8f;
            OwO2.ps.RotationSpeed = .2f;
            OwO2.ps.friction = .7f;
            OwO2.ThrustSpeed = 1f;
            OwO2.OnTakeDamage += OwO_OnTakeDamage;
            OwO2.Exhausted = 2f;

            EnemyLib.Add(OwO2);



            var OwO3 = new Actor(Content.Load<Texture2D>("Enemy_3"), AiType: Actor.Type.Laser, HP: 6);
            OwO3.Debris = Metal_Bits;
            OwO3.scale = 1;
            OwO3.Propulsion = MP.Propulsion;
            OwO3.Weapon = new Projectile(Content.Load<Texture2D>("Laser_1_Bound"), 1, .3f, Limetime: 12);
            OwO3.Weapon.Layer = .7f;

            OwO3.Weapon.AnimationSheet.Add(LaserAnimation1);
            OwO3.Weapon.color = Color.Red;
            OwO3.Weapon.Hurtful = true;
            OwO3.DebrisColor = Color.Yellow;
            OwO3.Weapon.AnimationSheet.Add(LaserContactAnimation);
            OwO3.Weapon.scale = .85f;
            OwO3.ps.RotationSpeed = .2f;
            OwO3.ps.friction = .7f;
            OwO3.ThrustSpeed = 1f;
            OwO3.OnTakeDamage += OwO_OnTakeDamage;
            OwO3.Exhausted = 2f;

            EnemyLib.Add(OwO3);


            var OwO4 = new Actor(Content.Load<Texture2D>("Enemy_4"), AiType: Actor.Type.Chaser, HP: 3);
            OwO4.Debris = Metal_Bits;
            OwO4.scale = .75f;
            OwO4.Propulsion = MP.Propulsion;
            OwO4.Weapon = new Projectile(Content.Load<Texture2D>("Laser_1_Bound"), 1, .96f, Limetime: 12);
            OwO4.Weapon.Layer = .7f;

            OwO4.Weapon.AnimationSheet.Add(LaserAnimation1);
            OwO4.Weapon.color = Color.Red;
            OwO4.Weapon.Hurtful = true;
            OwO4.DebrisColor = Color.Red;
            OwO4.Weapon.AnimationSheet.Add(LaserContactAnimation);
            OwO4.Weapon.scale = 1f;
            OwO4.ps.RotationSpeed = .2f;
            OwO4.ps.friction = .7f;
            OwO4.ThrustSpeed = .1f;
            OwO4.OnTakeDamage += OwO_OnTakeDamage;
            OwO4.Exhausted = 2f;

            EnemyLib.Add(OwO4);

            var OwO5 = new Actor(Content.Load<Texture2D>("Enemy_5"), AiType: Actor.Type.Sider, HP: 3);
            OwO5.Debris = Metal_Bits;
            OwO5.scale = .75f;
            OwO5.Propulsion = MP.Propulsion;
            OwO5.Weapon = new Projectile(Content.Load<Texture2D>("Laser_1_Bound"), 1,.5f,_spread: 90, Limetime: 15,Speed: 7);
            OwO5.Weapon.Layer = .7f;

            OwO5.Weapon.AnimationSheet.Add(LaserAnimation1);
            OwO5.Weapon.color = Color.Red;
            OwO5.Weapon.Hurtful = true;
            OwO5.DebrisColor = Color.Purple;
            OwO5.Weapon.AnimationSheet.Add(LaserContactAnimation);
            OwO5.Weapon.scale = .5f;
            OwO5.ps.RotationSpeed = .2f;
            OwO5.ps.friction = .7f;
            OwO5.ThrustSpeed = .1f;
            OwO5.OnTakeDamage += OwO_OnTakeDamage;
            OwO5.Exhausted = 2f;

            EnemyLib.Add(OwO5);



            MP.ps.friction = Actor.DEF_FRICTION;
           main = Content.Load<SpriteFont>("main");
            var t = Content.Load<Texture2D>("ground");

     
            MP.position = new Vector2(0, 0);
            MP.Layer = .6f;
            Animator.Add(MP);
            GenerateEnemy(EnemyLib[0]);
           
        }
        public void GenerateEnemy(Actor x )
        {
            var e = new Actor(x);
            e.position = Vector2.UnitY * -150;
            e.OnTakeDamage += OwO_OnTakeDamage;
            Actors.Add(e);
        }

     
        public Actor GenerateEnemy(Actor x, Vector2 pos)
        {
            var e = new Actor(x);
            e.position =pos;
            e.OnTakeDamage += OwO_OnTakeDamage;
            Actors.Add(e);
            return e;
        }
        float score, enemys;
        private void OwO_OnTakeDamage(float f, Actor a = null)
        {
            var q = new Random().Next(0,100);


            enemys += f;
            s += f * .1f;
            if (q > 65 - f)
                {
                    var h = new Random().Next(0, 4);
                    if(h-f/20 > 2)
                    {
                        var e = new Bonus(RateB);
                        e.ps.friction = 0;
                        if (a != null) e.position = a.position;
                        e.scale = .25f;
                        e.ps.Velocity = Vector2.UnitY * 5;
                        e.Layer = .9f;
                        CORE.Actors.Add(e);
                    }
                    else
                    {
                        var e = new Bonus(HPB);
                        e.ps.friction = 0;
                        if (a != null) e.position = a.position;
                        e.scale = .25f;
                        e.ps.Velocity = Vector2.UnitY * 5;
                        e.Layer = .9f;
                        CORE.Actors.Add(e);
                    }
                 
                }
            
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        public void Debug()
        {
            
        }
        float s = 0,s2 =0, s3 =0, s4=0,s5 = 0;
        bool s4sp = false; float BeforeQuit = 0;bool save = false;
        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        /// 
        protected override void Update(GameTime gameTime)
        {
            /*  if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                  Exit();*/

            // TODO: Add your update logic here
            if (MP.Health <= 0 || gameTime.TotalGameTime.TotalSeconds > 201)
            {
                float qt = 0;
                if (!save)
                {
                    var roamingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    var filePath = Path.Combine(roamingDirectory, "Omicron");
                    Directory.CreateDirectory(filePath);
                    string ga = "RECORD-" + (int)gameTime.TotalGameTime.TotalSeconds + Environment.NewLine + "SCORE-" + score;

                    try
                    { 
                        using (StreamReader sr = new StreamReader(filePath + "\\highscore.txt"))
                        {
                            string ln;
                            while ((ln = sr.ReadLine()) != null)
                            {
                                if (ln.Contains("RECORD"))
                                {
                                   var ars = ln.Split('-');
                                    if(ars.Length > 0)
                                        if(int.Parse(ars[1]) > (int)gameTime.TotalGameTime.TotalSeconds)
                                            ga = "RECORD-" + ars[1] + Environment.NewLine + "SCORE -" + score;
                                }
                            }
                          
                        }
                    }
                    catch
                    {
           
                       
                    }
                    qt += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                    InterpolatedGameTime = MathHelper.Clamp(qt / 200, 0, 1);
                   
                    
                    File.WriteAllText(filePath + "\\highscore.txt", ga.ToString());

                    save = true;
                }

                BeforeQuit += InterpolatedGameTime;
                if (BeforeQuit > 10) Exit();
                MediaPlayer.Stop();
                return;
            } 
            var e = new Random();
            var sec = gameTime.TotalGameTime.TotalSeconds;
            if (sec > 193)
            foreach (var item in Stars)
            {
                item.ps.Speed = .4f * Vector2.UnitY;
                if (item.position.Y > 170)
                    item.position.Y = -160;
            }
            if (gameTime.TotalGameTime.Milliseconds % 2 == 0)
            {
                var gt = new Actor(Hyper);
                gt.ThrustSpeed = 10;
                gt.ps.friction = 0;
                gt.ps.Velocity = Vector2.UnitY * (15 + e.Next(12) * 2); 
               gt.position = new Vector2(e.Next(-60, 60), -140);
                gt.color = Color.Gray;
                gt.Layer = 1;
                gt.scale = .5f;
                    VFX.Add(gt);

            }
           
            Sun.ps.Speed = Vector2.UnitY * .001f;
            Planet.ps.Speed = Vector2.UnitY* .008f;
            Planet.color = Color.Gray;
            if(sec < 192)
            {
                s += CORE.InterpolatedGameTime;
                s2 += CORE.InterpolatedGameTime;
                s3 += CORE.InterpolatedGameTime;
                s4 += CORE.InterpolatedGameTime;
                s5 += CORE.InterpolatedGameTime;
            }


            if (_lastrecord > -1 && sec > _lastrecord -2f)
            {
                LastRecord.position = new Vector2(49, -188);
                VFX.Add(LastRecord);
                _lastrecord = -1;
            }
            else if(sec > _lastrecord)
            {
                LastRecord.ps.Speed = Vector2.UnitY/2;
            }

               

            float TimeRequiredForSpawm = 8, trfs2 = 25, trfs3 = 60, trsf4 = 10, trsf5 =12;

            if (sec > 168) TimeRequiredForSpawm = 25f;
            else
            if (sec > 161) TimeRequiredForSpawm = .6f;
            else
            if (sec > 158) TimeRequiredForSpawm = 30;
            else
            if (sec > 146) TimeRequiredForSpawm = 1.5f;
            else if (sec > 114)
                TimeRequiredForSpawm = 20;
            else
             if (sec > 104)
                TimeRequiredForSpawm = 5;
            else
             if (sec > 60)
            {
                TimeRequiredForSpawm = 50;
            }
            else if (sec > 55)
            {
                TimeRequiredForSpawm = 13;
            }
            else if (sec > 25) TimeRequiredForSpawm = 17;
            else if (sec > 23) TimeRequiredForSpawm = 5;
            else if (sec > 9) TimeRequiredForSpawm = 14f;
            else if (sec > 6) TimeRequiredForSpawm = 3f;


            if (s > TimeRequiredForSpawm && (sec < 30 || sec > 55 ) && (sec < 81 || sec > 101) && (sec < 125|| sec >145))
            {
               

                var a =GenerateEnemy(EnemyLib[0], new Vector2(e.Next(-50,50), e.Next(-189, -180)));
                if (gameTime.TotalGameTime.TotalSeconds > 6 && (sec < 55 || sec > 104) && sec < 161f)
                {
                    var b = GenerateEnemy(EnemyLib[0], new Vector2(-a.position.X,a.position.Y));
                       
                }
                s = 0;
            }

            if (sec > 135) trfs3 = 15;
            else
            if (sec > 114) trfs3 = 5;
            else if (sec > 56) trfs3 = 45;
            else if (sec > 46) trfs3 = 15;
            if(s2 > trfs3)
            {
                if ((sec > 35.3  && sec < 55)   || sec > 115 && sec < 140)
                {
                    
                    var vel = new Vector2((float)e.NextDouble(), (float)e.NextDouble());
                    var nwqx = e.Next(-105, 4);
                    var Dir = e.Next(0, 2);
                  if(Dir > 0)
                    {
                        var c = GenerateEnemy(EnemyLib[1], new Vector2(-80 , nwqx));
                        c.ThrustSpeed = 1;
                      /*  if (sec >35)
                        {
                            var d = GenerateEnemy(EnemyLib[1], new Vector2(80, nwqx +25 * Dir));
                            d.ThrustSpeed = -1;
                        }*/
                    }
                    else
                    {
                        var c = GenerateEnemy(EnemyLib[1], new Vector2(80, nwqx));
                        c.ThrustSpeed = -1;
                       if( sec > 55)
                        {
                            var d = GenerateEnemy(EnemyLib[1], new Vector2(-80, nwqx + 25* Dir));
                           d.ThrustSpeed = 1;
                        }
                    }

                    s2 = 0;
                }

             
            }



            if (sec > 172) trfs2 = 15f;
            if (sec > 73) trfs2 =36f;
            else  if (sec > 69) trfs2 = 3.6f;
            else if (sec > 65) trfs2 = 25f;
            else if (sec > 60) trfs2 = 3.5f;
            else if (sec > 55) trfs2 = 12.5f;
          
      
            if (s3 > trfs2)
            {
                if (sec > 40 && (sec <76|| sec > 172))
                {
                    var a = GenerateEnemy(EnemyLib[2], new Vector2(e.Next(-60, 60), -180));
                }
  
                s3 = 0;
            }

            if (sec > 100) trsf5 = 9;
            else if (sec > 90) trsf5 = 2;
            else if (sec > 87) trsf5 = 3;
            if (sec > 79 && sec < 98 || sec > 125 && sec < 139)
            if(s5 > trsf5)
            {
                var x = 1;
                var Dir = e.Next(0, 3);
                if (Dir > 1) x *= -1;
                var a = GenerateEnemy(EnemyLib[4], new Vector2(x * 78, e.Next(-60, 60)));
                s5 = 0;

            }
            if (sec > 35 && sec < 36 || (sec > 80 && sec < 81) || (sec > 123 && sec < 124))
            {
                
                GC.Collect();
               GC.WaitForPendingFinalizers();
            }


            if (sec > 124) trsf4 = 2;
            else if (sec > 114) trsf4 = 10;
            else
          if (sec > 104) trsf4 = 45;
            else
          if (sec > 36) trsf4 = 15;
            else
            if (sec > 29) trsf4 = 2;
            else
            if (sec > 23) trsf4 = 3;
            if(sec > 16 && (sec < 33 || sec > 55) && (sec < 69) ||  sec > 103)
            {
                if(s4 > trsf4)
                {
                    var x = -50;
               
                    if (s4sp) x *= -1;
                    var a = GenerateEnemy(EnemyLib[3], new Vector2(x, e.Next(-186, -180)));
                    s4sp = !s4sp;
                    s4 = 0;
                }
             
            }




            score = (int)sec * 5 + enemys* 10;
            float elapsedTime = 0;
            elapsedTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
           InterpolatedGameTime = MathHelper.Clamp(elapsedTime / 200, 0, 1);


            if (sec < 200)
                MP.Update(gameTime);
            else MP.ps.Speed += Vector2.UnitY;

          
            camera.Update(Vector2.Zero );
           
            for (int i = 0; i < VFX.Count; i++)
            {
                if (VFX[i] != null) VFX[i].Update(gameTime);
            }
            for (int i = 0; i < Stars.Count; i++)
            {
                if (Stars[i] != null) Stars[i].Update(gameTime);
            }
            for (int i = 0; i < OnSCreen.Count; i++)
            {
                if (OnSCreen[i] != null) OnSCreen[i].Update(gameTime);
            }
            for (int i = 0; i < Actors.Count; i++)
            {
                if (Actors[i] != null)
                { Actors[i].Update(gameTime);
        
                } 
            }
     
           


            
            base.Update(gameTime);
        }
       public static List<Actor> VFX = new List<Actor>();
        public static List<Actor> Actors = new List<Actor>();
        public static List<Projectile> OnSCreen = new List<Projectile>();
        //  float Horizontal = 0, Vertical = 0;
        public Axis Horizontal = new Axis { Value = 0, Positive = Keys.A, Negative = Keys.D },Vertical = new Axis { Value = 0,Positive = Keys.W,Negative = Keys.S};

        public string Direction;
  
        [System.Serializable]
        public class Axis
        {
            public float Value;
            public float Update(float UpdateTime)
            {

                var A = Positive;
                var B = Negative;
                var q = Keyboard.GetState();
                if (q.IsKeyDown(A)) Value = MathHelper.Lerp(Value, -1, UpdateTime);
                if (q.IsKeyDown(B)) Value = MathHelper.Lerp(Value, 1, UpdateTime);
                if (q.IsKeyUp(A) && q.IsKeyUp(B)) Value = MathHelper.Lerp(Value, 0, UpdateTime);
                return Value;
            }
            public Keys Positive, Negative;
        }

        Color co;
        List<Ianimable> Animator = new List<Ianimable>();
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            if (MP.Health > 0)
                co = new Color(0, 0, 55, 1);
            else co = Color.Gray;
            GraphicsDevice.Clear(co);
            // TODO: Add your drawing code here
            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend,SamplerState.PointClamp,null,null,null,camera.GetTransform());
      
            //spriteBatch.Begin();

            foreach (var item in gameObjects)
            {   
               if(item != null ) spriteBatch.Draw(item.Texture, item.position,null, Color.White, item.rotation, item.Origin,item.scale,SpriteEffects.None, item.Layer);
            }

            foreach (var item in Actors)
                if (item != null && item.IsONScreen())
                    if (item.UseAnimations) item.UpdateAnimations(gameTime);
                    else spriteBatch.Draw(item.Texture, item.position, null, item.color, item.rotation, item.Origin, MathHelper.Clamp(item.scale, 0, item.scale), SpriteEffects.None, item.Layer);


            foreach(var item in Stars)
            {
                if (item != null && item.IsONScreen())
                    spriteBatch.Draw(item.Texture, item.position, null, item.color, item.rotation, item.Origin, MathHelper.Clamp(item.scale, 0, item.scale), SpriteEffects.None, item.Layer);

            }
            foreach (var item in VFX)
                if (item != null && item.IsONScreen())
                    spriteBatch.Draw(item.Texture, item.position, null, item.color, item.rotation, item.Origin, MathHelper.Clamp(item.scale, 0, item.scale), SpriteEffects.None, item.Layer);




            for (int i = 0; i < OnSCreen.Count; i++)
            {

                var item = OnSCreen[i];
                if (item != null && item.IsONScreen())
                {
                    if (item.UseAnimations) item.UpdateAnimations(gameTime);
                    else spriteBatch.Draw(item.Texture, item.position, null, item.color, item.rotation, item.Origin, MathHelper.Clamp(item.scale, 0, item.scale), SpriteEffects.None,item.Layer);

                }


            }
            var HpRatio = MP.Health / (10);
            spriteBatch.Draw(HPBar.Texture, new Vector2(46, 72 ), color:Color.Red,scale: new Vector2(1,.94f*HpRatio));
            spriteBatch.Draw(HPFrame.Texture, new Vector2(46, 70), Color.White);
            

            spriteBatch.Draw(MP.Texture, MP.position, null, new Color(1, 1, 1, MP.Alpha),MP.rotation,MP.Origin, MP.scale, SpriteEffects.None, .8f);
            string numberofzero = "0000";
                if (score >= 10000) numberofzero = "";
            else if (score >= 1000) numberofzero = "0";
            else if (score >= 100) numberofzero = "00";
                  else
            if (score > 10) numberofzero = "000";

        

      
            spriteBatch.DrawString(main, numberofzero + ((int)score).ToString(), new Vector2(-66,116), Color.White,0,Vector2.Zero,1,0,0);
           // spriteBatch.DrawString(main, ((int)MP.Health).ToString(), new Vector2(-66,96), Color.White,0,Vector2.Zero,1,0,0);


            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
