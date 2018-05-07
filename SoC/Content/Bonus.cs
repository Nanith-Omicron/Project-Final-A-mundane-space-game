using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
namespace Omicron
{
    class Bonus :Actor
    {
        public enum BonusType
        {
            Hp = 1, Speed = 2, Damage = 3
        }
        public BonusType bonusType;
        public float BonusIntensity = 1;
        public Bonus(Texture2D t, float BsIntensity, BonusType type) : base(t)
        {
            Eternal = true;
            BonusIntensity = BsIntensity;
            bonusType = type;
        }
        public Bonus(Bonus b):base(b.Texture)
        {
            this.Eternal = true;
            this.BonusIntensity = b.BonusIntensity;
            this.bonusType = b.bonusType;
            this.CanCollide = true;
            this.scale = .5f;
            
        }
    }
}
