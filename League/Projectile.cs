using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace LeagueEngine {
    /// <summary>
    /// A missile created by an attacking unit
    /// </summary>
    public class Projectile : GameObject {
        /// <summary>
        /// The attacking unit
        /// </summary>
        public Unit Attacker;

        /// <summary>
        /// The attacked unit
        /// </summary>
        public Unit Target;

        /// <summary>
        /// The damage of the projectile
        /// </summary>
        public int Damage = 1;

        /// <summary>
        /// The speed of the projectile in units/second
        /// </summary>
        public float Speed = 1f;

        /// <summary>
        /// The effect ID for this projectile
        /// </summary>
        public string Eid = "";

        /// <summary>
        /// Data for the effect ID handler
        /// </summary>
        public string EidData = "";

        /// <summary>
        /// The skin to apply to this projectile's mesh
        /// </summary>
        public static Texture2D Skin;


        /// <summary>
        /// Creates a projectile from one unit to the other
        /// </summary>
        /// <param name="game">The instance of League</param>
        /// <param name="attacker">The attacking unit</param>
        /// <param name="target">The unit being attacked</param>
        public Projectile(League game, Unit attacker, Unit target) : base(game) {
            Mesh = new GameModel(Engine.Content.Load<Model>("projectiles/" + attacker.Type.AttackGfx));
            Attacker = attacker;
            Target = target;
            Scale = attacker.Type.AttackGfxSize;
            Speed = attacker.Type.AttackSpeed;
            Damage = attacker.Type.AttackDamage;
            Position = attacker.Position;
            Eid = attacker.Type.AttackMagic;
            EidData = attacker.Type.AttackMagicData;
        }

        /// <summary>
        /// Initializes the component
        /// </summary>
        public override void Initialize() {
            // Draw after units before particles
            DrawOrder = 3;
            base.Initialize();

            // Call the effect handler
            Engine.Script.InvokeEvent("ProjectileCreated", Eid, this, EidData);
        }

        /// <summary>
        /// Updates the component - checking for collisions and good things like that
        /// </summary>
        /// <param name="gameTime">A snapshot of game timing values</param>
        public override void Update(GameTime gameTime) {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            Vector2 dest = Target.Position - Position;
            Position.X += Math.Sign(dest.X) * Speed * elapsed;
            Position.Y += Math.Sign(dest.Y) * Speed * elapsed;

            // Gives it a bit of wiggle room
            if (dest.Length() < 5f)
                Collide();

            base.Update(gameTime);
        }

        /// <summary>
        /// Draws the mesh
        /// </summary>
        /// <param name="gameTime">A snapshot of game timing values</param>
        public override void Draw(GameTime gameTime) {
            Mesh.Draw(this);
            base.Draw(gameTime);
        }

        /// <summary>
        /// Gets the world of this object
        /// </summary>
        /// <returns>The world of the Projectile</returns>
        public override Matrix GetTransformation() {
            return Rotation * Matrix.CreateScale(Scale) * GetPositionTransformation();
        }

        /// <summary>
        /// This projectile has hit its target
        /// </summary>
        private void Collide() {
            Target.Hp -= Damage;
            Engine.Script.InvokeEvent("ProjectileDestroyed", Eid, this, EidData);
            Engine.Components.Remove(this);
        }

        /// <summary>
        /// Creates a projectile and adds it to the component list
        /// </summary>
        /// <param name="attacker">The attacking unit</param>
        /// <param name="target">The attacked unit</param>
        /// <param name="gfx">The path to the Projectile's mesh</param>
        /// <param name="size">The scale of the Projectile</param>
        /// <param name="speed">The speed it should travel</param>
        /// <param name="dmg">The damage of the Projectile</param>
        /// <returns>The new projectile</returns>
        public static Projectile MakeProjectile(Unit attacker, Unit target, string gfx, float size, float speed, int dmg) {
            Projectile p = new Projectile(attacker.Engine, attacker, target);
            p.Engine.Components.Add(p);
            return p;
        }
    }
}
