using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Omicron
{
    class Timer
    {

       public float ElapsedTime = 0;
        private GameTime gm;
        protected float TargetedTime = 0;
        bool done = false;
        public bool IsTerminated()
        {

            return ElapsedTime >= TargetedTime;
            
        }
        public delegate void TimerHandler();
        public event TimerHandler OnTimerFinish;
        public void Update()
        {

            var e = (float)gm.ElapsedGameTime.TotalSeconds;
            if (!(ElapsedTime >= TargetedTime))           
                ElapsedTime += e;
            else if (!done)
            {
                done = true;
                OnTimerFinish();
            }
            
    
            
        }
        public Timer( float TimerInSecond, GameTime gameTime)
        {
            gm = gameTime;
            TargetedTime = TimerInSecond;

        }
    }
}
