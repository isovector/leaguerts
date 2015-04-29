using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LeagueEngine {
    /// <summary>
    /// Describes a Unit, Projectile, or ParticleEmitter attachment
    /// </summary>
    public class GameObject : DrawableGameComponent {
        /// <summary>
        /// The GameModel to draw
        /// </summary>
        public GameModel Mesh;

        /// <summary>
        /// The scale of the GameModel
        /// </summary>
        public float Scale = 1f;

        /// <summary>
        /// The map coordinates of this object
        /// </summary>
        public Vector2 Position;
        
        /// <summary>
        /// The height above the ground
        /// </summary>
        public float Height = 0f;

        /// <summary>
        /// The rotation of the object
        /// </summary>
        public Rotation Rotation = new Rotation();

        /// <summary>
        /// An instance of League
        /// </summary>
        public League Engine;

        /// <summary>
        /// A dictionary provided for scripts to store information
        /// </summary>
        public Dictionary<string, object> Tags = new Dictionary<string, object>();


        /// <summary>
        /// Used as a base for the children.
        /// </summary>
        /// <param name="game">The game instance</param>
        public GameObject(League game) : base(game) {
            Engine = game;
        }

        /// <summary>
        /// Creates a ParticleEmission source
        /// </summary>
        /// <param name="game">The game instance</param>
        /// <param name="pos">The location</param>
        public GameObject(League game, Vector2 pos)
            : base(game) {
            Engine = game;
            Position = pos;
        }


        /// <summary>
        /// Creates an absolutely positioned ParticleEmission source
        /// </summary>
        /// <param name="game">The game instance</param>
        /// <param name="pos">XZ provide map coordinates. Y is height.</param>
        public GameObject(League game, Vector3 pos)
            : base(game) {
            Engine = game;
            Position = new Vector2(pos.X, pos.Z);
            Height = pos.Y;
        }

        /// <summary>
        /// Sets the draw order of this component
        /// </summary>
        public override void Initialize() {
            DrawOrder = 3;
            base.Initialize();
        }

        /// <summary>
        /// Transforms the Position into a 3D position
        /// </summary>
        /// <returns>The Position with the Y filled in by the map</returns>
        public virtual Vector3 GetPosition() {
            float ypos = Height;
            if (Engine.CurrentMap.IsInMapBounds(Position))
                ypos += Engine.CurrentMap.GetHeight(Position);
            return new Vector3(Position.X, ypos, Position.Y);
        }

        /// <summary>
        /// Gets the position aspect of this object's World matrix
        /// </summary>
        /// <returns>The position aspect of the object's World Matrix</returns>
        public virtual Matrix GetPositionTransformation() {
            return Engine.CurrentMap.World * Matrix.CreateTranslation(GetPosition()) * Engine.CurrentMap.Rotation;
        }

        /// <summary>
        /// Gets the World Matrix of the object
        /// </summary>
        /// <returns>The World Matrix</returns>
        public virtual Matrix GetTransformation() {
            return GetPositionTransformation();
        }

        /// <summary>
        /// Gets the World Matrix of the specified mesh in the object
        /// </summary>
        /// <param name="mesh">The mesh requested</param>
        /// <returns>The World Matrix</returns>
        public virtual Matrix GetTransformation(ModelMesh mesh) {
            return GetPositionTransformation();
        }

        /// <summary>
        /// Removes a GameObject from the Update queue.
        /// </summary>
        public virtual void Remove() {
            Engine.Components.Remove(this);
        }

        /// <summary>
        /// Creates an GameObject and binds it to League
        /// </summary>
        /// <param name="pos">The position at which to create this object</param>
        /// <returns>The resulting GameObject</returns>
        public static GameObject CreateGameObject(Vector2 pos) {
            GameObject go = new GameObject(League.Engine, pos);
            League.Engine.Components.Add(go);
            return go;
        }
    }
}
