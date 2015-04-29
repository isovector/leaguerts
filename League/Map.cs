using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Algorithms;
using System.Threading;
using LeagueEngine.Effects;
using LeagueEngine.Properties;
using System.IO;
using Microsoft.Xna.Framework.Input;

namespace LeagueEngine {
    /// <summary>
    /// Describes a map of League
    /// </summary>
    public class Map : DrawableGameComponent {
        /// <summary>
        /// The encapsulated shader to draw everything
        /// </summary>
        public static fowEffect FogOfWarEffect;

        /// <summary>
        /// The width of the map
        /// </summary>
        public float Width;

        /// <summary>
        ///  The height of the map
        /// </summary>
        public float Height;

        /// <summary>
        /// The map's world. All other positions are in relation to this.
        /// </summary>
        public Matrix World = Matrix.Identity;

        /// <summary>
        /// The map's rotation
        /// </summary>
        public Rotation Rotation = new Rotation();

        /// <summary>
        /// A texture containing the minimap
        /// </summary>
        public Texture2D Minimap;

        /// <summary>
        /// A dictionary of name = region for use in scripting
        /// </summary>
        public Dictionary<string, Region> Regions = new Dictionary<string, Region>();

        /// <summary>
        /// Has the map been initialized?
        /// </summary>
        public bool Initialized = false;
        
        /// <summary>
        /// The pathing engine to move units around
        /// </summary>
        private PathFinder PathEngine;

        /// <summary>
        /// A queue of units which need paths
        /// </summary>
        private Queue<Unit> UnitsToPath = new Queue<Unit>();

        /// <summary>
        /// A queue of endpoints for the units queued above
        /// </summary>
        private Queue<Point> PathsToPath = new Queue<Point>();

        /// <summary>
        /// A counter of frames - paths are calculated once every 5 frames
        /// </summary>
        private int FrameCount = 0;

        /// <summary>
        /// A list of planes for checking cliff heights against
        /// </summary>
        private List<Plane> Planes = new List<Plane>();

        /// <summary>
        /// The instance of League
        /// </summary>
        private League Engine;

        /// <summary>
        /// The dynamic terrain to draw
        /// </summary>
        public Terrain Terrain;

        /// <summary>
        /// Creates a map
        /// </summary>
        /// <param name="game">The instance of League</param>
        public Map(League game) : base(game) {
            DrawOrder = 0;
            Engine = game;
        }

        /// <summary>
        /// Initializes the component so we may use the ContentMananger
        /// </summary>
        public override void Initialize() {
            base.Initialize();
        }

        /// <summary>
        /// Loads all related content
        /// </summary>
        protected override void LoadContent() {
            // Create the encapsulated shader
            FogOfWarEffect = new fowEffect("shaders/fow");
            FogOfWarEffect.Load(Engine.Content);
            FogOfWarEffect.CliffTexture = Engine.MpqContent.Load<Texture2D>("map/textures/cliff");
            FogOfWarEffect.TerrainTexture = Engine.MpqContent.Load<Texture2D>("map/textures/terrain");
            Terrain = new Terrain(Engine.MpqContent.Load<Texture2D>("map/terrain"));

            // Load the regions from the map
            if (Engine.MpqContent.Archive.FileExists(@"map\regions.lrg")) {
                Stream s = Engine.MpqContent.Archive.OpenFile(@"map\regions.lrg");
                StreamReader sr = new StreamReader(s);
                List<string> regs = new List<string>();
                while (!sr.EndOfStream)
                    regs.Add(sr.ReadLine());
                sr.Close();

                foreach (string reg in regs) {
                    // Regions provide their own serialization/deserialization
                    Region r = new Region(Engine, reg);
                    Regions.Add(r.Name, r);
                    Engine.Components.Add(r);
                }
            }

            Width = Terrain.Width * 16;// HeightData.heightmapWidth;
            Height = Terrain.Height * 16;// HeightData.heightmapHeight;

            // Create the pathing engine and set it up
            byte[,] path = new byte[Terrain.Width, Terrain.Height];

            for (int y = 0; y < Terrain.Height; y++)
                for (int x = 0; x < Terrain.Width; x++) {
                    if (x == Terrain.Width - 1 || y == Terrain.Height - 1)
                        path[x, y] = 0;
                    else
                        if (Terrain.Tiles[x, y].CliffLevel != Terrain.Tiles[x, y + 1].CliffLevel ||
                            Terrain.Tiles[x, y].CliffLevel != Terrain.Tiles[x + 1, y].CliffLevel)
                            path[x, y] = 0;
                        else
                            path[x, y] = 1;
                }


            PathEngine = new PathFinder(path);
            PathEngine.Diagonals = true;
            PathEngine.Formula = HeuristicFormula.DiagonalShortCut;
            PathEngine.SearchLimit = 4000;
            PathEngine.PunishChangeDirection = true;

            for (int i = 0; i < 20; i++)
                Planes.Add(new Plane(Vector3.Up, Terrain.MetaMapTile.GetCliffHeight(i)));

            // This is the model used to view regions - press F1 during the game to see regions
            Region.Model = new GameModel(Engine.Content.Load<Model>("models/region"));

            base.LoadContent();
        }

        /// <summary>
        /// Adds a unit to the pathing queue
        /// </summary>
        /// <param name="unit">The unit to queue</param>
        /// <param name="dest">The desired endpoint</param>
        public void FindPath(Unit unit, Point dest) {
            // This is required or one unit can completely clog the engine
            if (!UnitsToPath.Contains(unit)) {
                UnitsToPath.Enqueue(unit);
                PathsToPath.Enqueue(dest);
            }
        }

        /// <summary>
        /// Get the height of the map at a location
        /// </summary>
        /// <param name="pos">The position to get</param>
        /// <returns>The height of the map</returns>
        public float GetHeight(Vector2 pos) {
            int x = (int)((pos.X + Width / 2f) / 16f);
            int y = (int)((pos.Y + Height / 2f) / 16f);
            return Terrain.Tiles[x, y].CliffHeight - 180;
        }

        /// <summary>
        /// Get the height of the map at a location
        /// </summary>
        /// <param name="pos">The position to get</param>
        /// <returns>The height of the map</returns>
        public float GetHeight(Vector3 pos) {
            return Terrain.Tiles[(int)((pos.X + Width / 2f) / 16f), (int)((pos.Z + Height / 2f) / 16f)].CliffHeight;
        }

        /// <summary>
        /// Gets the pathfinding node of a position
        /// </summary>
        /// <param name="pos">The position to get</param>
        /// <returns>The pathfinding node</returns>
        public Point GetNode(Vector2 pos) {
            return new Point((int)((pos.X + Width / 2f) / 16f), (int)((pos.Y + Height / 2f) / 16f));
        }

        /// <summary>
        /// Gets the pathfinding node of a position
        /// </summary>
        /// <param name="pos">The position to get</param>
        /// <returns>The pathfinding node</returns>
        public Point GetNode(Vector3 pos) {
            return new Point((int)((pos.X + Width / 2f) / 16f), (int)((pos.Z + Height / 2f) / 16f));
        }

        /// <summary>
        /// Transforms a pathfinding node to a map position
        /// </summary>
        /// <param name="x">Node x</param>
        /// <param name="y">Node y</param>
        /// <returns>The map position</returns>
        public Vector3 GetNodePosition(int x, int y) {
            return new Vector3(x * 16f - Width / 2f, Terrain.Tiles[x, y].CliffHeight, y * 16 - Height / 2f);
        }

        /// <summary>
        /// Determines whether a point is in the map bounds
        /// </summary>
        /// <param name="pos">The position to check</param>
        /// <returns>Is it?</returns>
        public bool IsInMapBounds(Vector2 pos) {
            int x = (int)((pos.X + Width / 2f) / 16f);
            int y = (int)((pos.Y + Height / 2f) / 16f);

            return (x >= 0 && y >= 0 && x < Terrain.Width && y < Terrain.Height);
        }

        /// <summary>
        /// Determines whether a point is in the map bounds
        /// </summary>
        /// <param name="pos">The position to check</param>
        /// <returns>Is it?</returns>
        public bool IsInMapBounds(Vector3 pos) {
            int x = (int)((pos.X + Width / 2f) / 16f);
            int y = (int)((pos.Z + Height / 2f) / 16f);

            return (x >= 0 && y >= 0 && x < Terrain.Width && y < Terrain.Height);
        }

        /// <summary>
        /// Initializes the map by calling the MapInit event, and creates
        /// all of the resources.
        /// </summary>
        public void MapInit() {
            Engine.Script.InvokeAllSyncEvent("resource", null);

            foreach (KeyValuePair<string, Resource> res in Resource.Resources)
                foreach (KeyValuePair<int, Player> player in Player.Players)
                    player.Value.Resources.Add(res.Key, res.Value.Amount);

            Engine.Script.InvokeEvent("mapinit", null);
            Initialized = true;
        }

        /// <summary>
        /// Transforms screen space to world space
        /// </summary>
        /// <param name="mousecoords">The screen coords</param>
        /// <returns>The point in the world or null if not on the world</returns>
        public Vector3? GetWorldPos(Point mousecoords) {
            Vector3 nearSource = GraphicsDevice.Viewport.Unproject(new Vector3(mousecoords.X, mousecoords.Y, GraphicsDevice.Viewport.MinDepth), League.projection, League.view, World * Rotation);
            Vector3 farSource = GraphicsDevice.Viewport.Unproject(new Vector3(mousecoords.X, mousecoords.Y, GraphicsDevice.Viewport.MaxDepth), League.projection, League.view, World * Rotation);
            Vector3 direction = farSource - nearSource;

            Ray ray = new Ray(nearSource, direction);

            for (int h = 0; h < 20; h++)
                if (ray.Intersects(Planes[h]).HasValue) {
                    Vector3 pos = nearSource + (direction * ray.Intersects(Planes[h]).Value);
                    if (IsInMapBounds(pos)) {
                        float height = GetHeight(pos) - 180;
                        if (Math.Abs(GetHeight(pos) - 180 - pos.Y) <= 8)
                            return pos;
                    }
                }

            return null;
        }

        /// <summary>
        /// Performs map specific updates - pathing
        /// </summary>
        /// <param name="gameTime">A snapshot of timing values</param>
        public override void Update(GameTime gameTime) {
            FrameCount++;

            // Every three frames we'll determine a path from the queue
            if (FrameCount % 3 == 0) {
                if (UnitsToPath.Count > 0) {
                    Unit u = UnitsToPath.Dequeue();
                    Point start = GetNode(u.Position);
                    Point end = PathsToPath.Dequeue();
                    PathEngine.HeuristicEstimate = Math.Abs(start.X - end.X) + Math.Abs(start.Y - end.Y);
                    u.Path = PathEngine.FindPath(start, end);
                    u.Node = 0;
                }
            }
            base.Update(gameTime);
        }

        /// <summary>
        /// Draws the map
        /// </summary>
        /// <param name="gameTime">A snapshot of timing values</param>
        public override void Draw(GameTime gameTime) {
            FogOfWarEffect.SetCurrentTechnique(fowEffectTechniques.Terrain);
            FogOfWarEffect.FOWTexture = Player.CurrentPlayer.FogMap.GetTexture();
            FogOfWarEffect.World = World * Rotation;

            GraphicsDevice.RenderState.PointSpriteEnable = false;
            GraphicsDevice.RenderState.DepthBufferEnable = true;
            GraphicsDevice.RenderState.DepthBufferWriteEnable = true;

            FogOfWarEffect.BaseEffect.Begin(SaveStateMode.SaveState);
            foreach (EffectPass pass in FogOfWarEffect.BaseEffect.CurrentTechnique.Passes) {
                pass.Begin();

                GraphicsDevice.Vertices[0].SetSource(Terrain.VBuffer, 0, Terrain.VertexMultitextured.SizeInBytes);
                GraphicsDevice.Indices = Terrain.IBuffer;
                GraphicsDevice.VertexDeclaration = new VertexDeclaration(GraphicsDevice, Terrain.VertexMultitextured.VertexElements);
                GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, Terrain.Width * Terrain.Height * 4, 0, Terrain.Indices.Length / 3);

                pass.End();
            }

            FogOfWarEffect.BaseEffect.End();

            base.Draw(gameTime);
        }

        /// <summary>
        /// Refreshes the minimap
        /// </summary>
        public void DrawMinimap() {
            RenderTarget2D target = new RenderTarget2D(GraphicsDevice, 128, 128, 1, SurfaceFormat.Color);

            GraphicsDevice.SetRenderTarget(0, target);
            GraphicsDevice.Clear(Color.Black);

            FogOfWarEffect.SetCurrentTechnique(fowEffectTechniques.Terrain);
            FogOfWarEffect.FOWTexture = Player.CurrentPlayer.FogMap.GetTexture();
            FogOfWarEffect.View = Matrix.CreateFromYawPitchRoll(0f, MathHelper.PiOver2, 0f);
            FogOfWarEffect.Proj = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45.0f), 1f, 1.0f, 10000.0f);
            FogOfWarEffect.World = Matrix.CreateTranslation(0, -500f, 0) * Matrix.CreateScale(0.01f);

            GraphicsDevice.RenderState.PointSpriteEnable = false;
            GraphicsDevice.RenderState.DepthBufferEnable = true;
            GraphicsDevice.RenderState.DepthBufferWriteEnable = true;

            FogOfWarEffect.BaseEffect.Begin(SaveStateMode.SaveState);
            foreach (EffectPass pass in FogOfWarEffect.BaseEffect.CurrentTechnique.Passes) {
                pass.Begin();

                GraphicsDevice.Vertices[0].SetSource(Terrain.VBuffer, 0, Terrain.VertexMultitextured.SizeInBytes);
                GraphicsDevice.Indices = Terrain.IBuffer;
                GraphicsDevice.VertexDeclaration = new VertexDeclaration(GraphicsDevice, Terrain.VertexMultitextured.VertexElements);
                GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, Terrain.Width * Terrain.Height * 4, 0, Terrain.Indices.Length / 3);

                pass.End();
            }

            FogOfWarEffect.BaseEffect.End();

            GraphicsDevice.SetRenderTarget(0, null);
            Minimap = target.GetTexture();

            FogOfWarEffect.View = League.view;
            FogOfWarEffect.Proj = League.projection;
        }


        /// <summary>
        /// A region of the map, used for scripting.
        /// </summary>
        public class Region : DrawableGameComponent {
            /// <summary>
            /// The name of the region, as defined in map/regions.lrg
            /// </summary>
            public string Name;

            /// <summary>
            /// The instance of League
            /// </summary>
            public League Engine;

            /// <summary>
            /// The bounds of the region
            /// </summary>
            public Rectangle Rect;
            
            /// <summary>
            /// A list of units in the region. Used to fire RegionEntered and RegionLeft events
            /// </summary>
            public UnitGroup Contents = new UnitGroup();

            /// <summary>
            /// A mesh to show the Region. Used for debugging. Press F1 in game to see the regions.
            /// </summary>
            public static GameModel Model;


            /// <summary>
            /// Creates a Region from a rectanngle
            /// </summary>
            /// <param name="game">The instance of League</param>
            /// <param name="name">The name of the region</param>
            /// <param name="rect">The defining rectangle</param>
            public Region(League game, string name, Rectangle rect) : base(game) {
                Engine = game;
                Name = name;
                Rect = rect;
            }

            /// <summary>
            /// Creates a Region from raw data
            /// </summary>
            /// <param name="game">The instance of league</param>
            /// <param name="name">The name of the region</param>
            /// <param name="x">The x coord of the region</param>
            /// <param name="y">The y coord of the region</param>
            /// <param name="width">The width of the region</param>
            /// <param name="height">The height of the region</param>
            public Region(League game, string name, int x, int y, int width, int height)
                : this(game, name, new Rectangle(x, y, width, height)) {}

            /// <summary>
            /// Creates a region from serialized data
            /// </summary>
            /// <param name="game">The instance of League</param>
            /// <param name="data">The serialized region</param>
            public Region(League game, string data) : base(game) {
                Engine = game;
                int pos = data.IndexOf('"', 2);
                Name = data.Substring(2, pos - 2);
                data = data.Substring(pos + 2, data.Length - pos - 3);
                data = data.Replace("(", "").Replace(")", "");
                string[] bits = data.Split(',');
                Rect = new Rectangle(int.Parse(bits[0]), int.Parse(bits[1]), int.Parse(bits[2]), int.Parse(bits[3]));
            }

            /// <summary>
            /// Sets the draw order of the region
            /// </summary>
            public override void Initialize() {
                DrawOrder = 2;
                base.Initialize();
            }

            /// <summary>
            /// Draws the region. Used for debugging. Press F1 in game for this to be enabled
            /// </summary>
            public override void Draw(GameTime gameTime) {
                if (Keyboard.GetState().IsKeyDown(Keys.F1)) {
                    Vector2 pos = this;

                    float ypos = 1f;
                    if (Engine.CurrentMap.IsInMapBounds(pos))
                        ypos += Engine.CurrentMap.GetHeight(pos);
                    Vector3 d3pos = new Vector3(pos.X, ypos, pos.Y);

                    Matrix m = Matrix.CreateScale(Rect.Width / 2f, 0f, Rect.Height / 2f) * Engine.CurrentMap.World * Matrix.CreateTranslation(d3pos) * Engine.CurrentMap.Rotation;
                    Model.Draw(m, Contents.Count > 0 ? Color.Red : Color.Blue);
                }
                base.Draw(gameTime);
            }

            /// <summary>
            /// Checks whether units have entered or left the region
            /// </summary>
            /// <param name="gameTime">A snapshot of timing values.</param>
            public override void Update(GameTime gameTime) {
                for (int i = 0; i < Contents.Count; i++) {
                    Unit u = Contents[i];
                    if (!Rect.Contains((int)u.Position.X, (int)u.Position.Y)) {
                        // The region no longer contains a unit
                        Contents.RemoveAt(i);
                        Engine.Script.InvokeEvent("regionleft", Name, u);
                    }
                }

                Vector2 pos = this;

                lock (Engine.Units) {
                    foreach (Unit u in Engine.Units)
                        if (!Contents.Contains(u) && Rect.Contains((int)u.Position.X, (int)u.Position.Y)) {
                            // The region contains a new unit
                            Contents.Add(u);
                            Engine.Script.InvokeEvent("regionentered", Name, u);
                        }
                }

                base.Update(gameTime);
            }

            /// <summary>
            /// Implicitly converts a Region to a Vector2 by using it's center
            /// </summary>
            /// <param name="r">The region to convert</param>
            /// <returns>The center of the Region</returns>
            public static implicit operator Vector2(Region r) {
                Vector2 start = new Vector2(r.Rect.X, r.Rect.Y);
                Vector2 mid = new Vector2(r.Rect.Width / 2, r.Rect.Height / 2);
                return start + mid;
            }

            /// <summary>
            /// Serailizes a region
            /// </summary>
            /// <returns>The serialized stream in the format {"name",(x,y),(w,h)}</returns>
            public override string ToString() {
                return "{" + String.Format("\"{0}\",({1},{2}),({3},{4})", Name, Rect.X, Rect.Y, Rect.Width, Rect.Height) + "}";
            }
        }
    }
}

