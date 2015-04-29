using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace LeagueEngine.Visual {
    public class ParticleEmitter : GameComponent {
        #region Fields

        ParticleSystem particleSystem;
        float timeBetweenParticles;
        Vector3 previousPosition;
        float timeLeftOver;
        public GameObject AttachedTo;
        public float RemainingTime = 1337f;
        public bool OwnerOf = false;

        #endregion


        /// <summary>
        /// Constructs a new particle emitter object.
        /// </summary>
        public ParticleEmitter(ParticleSystem particleSystem,
                               float particlesPerSecond, GameObject attach) : base(League.Engine) {
            this.particleSystem = particleSystem;

            timeBetweenParticles = 1.0f / particlesPerSecond;

            AttachedTo = attach;

            previousPosition = attach.GetPosition();

            //AttachedTo = attachment;
        }


        /// <summary>
        /// Updates the emitter, creating the appropriate number of particles
        /// in the appropriate positions.
        /// </summary>
        public override void Update(GameTime gameTime) {
            if (RemainingTime != 1337)
                RemainingTime -= (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (!League.Engine.Components.Contains(AttachedTo) || RemainingTime < 0) {
                League.Engine.Components.Remove(this);

                if (OwnerOf)
                    League.Engine.Components.Remove(AttachedTo);

                return;
            }

            Vector3 newPosition = AttachedTo.GetPosition();

            if (gameTime == null)
                throw new ArgumentNullException("gameTime");

            // Work out how much time has passed since the previous update.
            float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (elapsedTime > 0) {
                // Work out how fast we are moving.
                Vector3 velocity = (newPosition - previousPosition) / elapsedTime;

                // If we had any time left over that we didn't use during the
                // previous update, add that to the current elapsed time.
                float timeToSpend = timeLeftOver + elapsedTime;

                // Counter for looping over the time interval.
                float currentTime = -timeLeftOver;

                // Create particles as long as we have a big enough time interval.
                while (timeToSpend > timeBetweenParticles) {
                    currentTime += timeBetweenParticles;
                    timeToSpend -= timeBetweenParticles;

                    // Work out the optimal position for this particle. This will produce
                    // evenly spaced particles regardless of the object speed, particle
                    // creation frequency, or game update rate.
                    float mu = currentTime / elapsedTime;

                    Vector3 position = Vector3.Lerp(previousPosition, newPosition, mu);

                    // Create the particle.
                    particleSystem.AddParticle(position, velocity);
                }

                // Store any time we didn't use, so it can be part of the next update.
                timeLeftOver = timeToSpend;
            }

            previousPosition = newPosition;

            base.Update(gameTime);
        }

        public static ParticleEmitter CreateParticleEmitter(string vid, float partsec, GameObject obj) {
            ParticleEmitter e = new ParticleEmitter(ParticleSystem.GetParticleSystem(vid), partsec, obj);
            League.Engine.Components.Add(e);
            return e;
        }

        public static ParticleEmitter CreateParticleEmitter(string vid, float partsec, Vector2 pos) {
            return CreateParticleEmitter(vid, partsec, GameObject.CreateGameObject(pos));
        }

        public static ParticleEmitter CreateTimedParticleEmitter(string vid, float partsec, float time, GameObject obj) {
            ParticleEmitter e = new ParticleEmitter(ParticleSystem.GetParticleSystem(vid), partsec, obj);
            e.RemainingTime = time;
            League.Engine.Components.Add(e);
            return e;
        }

        public static ParticleEmitter CreateTimedParticleEmitter(string vid, float partsec, float time, Vector2 pos) {
            ParticleEmitter e = CreateTimedParticleEmitter(vid, partsec, time, GameObject.CreateGameObject(pos));
            e.OwnerOf = true;
            return e;
        }
    }
}
