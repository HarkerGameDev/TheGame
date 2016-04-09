using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Source.Graphics
{
    /// <summary>
    /// Make an emitter if you want a continous stream of particles from some location (which can move)
    /// If you just want one spew of particles, use the method World.MakeParticles. This will probably be overwritten eventually
    /// </summary>
    public class ParticleEmitter
    {
        private Random random;
        public Vector2 EmitterLocation;
        private List<Particle> particles;
        private List<Texture2D> textures;

        private float spawnTime { get; }
        private float currentTime;
        public bool Enabled;

        public float VelX, VelY;
        public float VelVarX, VelVarY;
        public float AngVel;
        public float AngVelVar;
        public float Red, Blue, Green;
        public float RedVar, BlueVar, GreenVar;
        public float Size;
        public float SizeVar;
        public float LiveTime;
        public float LiveTimeVar;

        public ParticleEmitter(List<Texture2D> textures, Vector2 location, float particlesPerSec)
        {
            EmitterLocation = location;
            this.textures = textures;
            this.particles = new List<Particle>();
            random = new Random();

            spawnTime = 1f / particlesPerSec;
            currentTime = 0;
            Enabled = true;

            VelX = 0f;
            VelY = 0f;
            VelVarX = 1f;
            VelVarY = 1f;
            AngVel = 0f;
            AngVelVar = 1f;
            Red = 0.5f;
            Blue = 0.5f;
            Green = 0.5f;
            RedVar = BlueVar = GreenVar = 0.5f;
            Size = 1f;
            SizeVar = 0.5f;
            LiveTime = 1.5f;
            LiveTimeVar = 1f;
        }

        private Particle MakeParticle()
        {
            Texture2D texture = textures[random.Next(textures.Count)];
            Vector2 position = EmitterLocation;
            Vector2 velocity = new Vector2(VelX + VelVarX * ((float)random.NextDouble() - 0.5f),
                VelY + VelVarY * ((float)random.NextDouble() - 0.5f));
            float angle = 0;
            float angularVelocity = AngVel + AngVelVar * ((float)random.NextDouble() - 0.5f);
            Color color = new Color(Red + RedVar * ((float)random.NextDouble() - 0.5f),
                Blue + BlueVar * ((float)random.NextDouble() - 0.5f),
                Green + GreenVar * ((float)random.NextDouble() - 0.5f));
            float size = Size + SizeVar * ((float)random.NextDouble() - 0.5f);
            float liveTime = LiveTime + LiveTimeVar * ((float)random.NextDouble() - 0.5f);

            return new Particle(texture, position, velocity, angle, angularVelocity, color, size, liveTime);
        }

        public void Update(float deltaTime)
        {
            for (int i = particles.Count - 1; i >= 0; i--)
            {
                particles[i].Update(deltaTime);
                if (particles[i].LiveTime < 0)
                {
                    particles.RemoveAt(i);
                }
            }

            if (Enabled)
            {
                currentTime -= deltaTime;
                while (currentTime < 0)
                {
                    currentTime += spawnTime;
                    particles.Add(MakeParticle());
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            for (int index = 0; index < particles.Count; index++)
            {
                particles[index].Draw(spriteBatch);
            }
        }
    }
}
