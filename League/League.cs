#region Using Statements
using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Net;
using Algorithms;
using System.IO;
using LeagueEngine.Effects;
using Microsoft.Xna.Framework.GamerServices;
using LeagueEngine.Properties;
using LeagueEngine.Scripting;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.InteropServices;
using System.ComponentModel;
using LeagueEngine.Visual;
#endregion

namespace LeagueEngine {
    /// <summary>
    /// The main class for the game engine
    /// </summary>
    public class League : Microsoft.Xna.Framework.Game {
        /// <summary>
        /// The game instance
        /// </summary>
        public static League Engine;

        /// <summary>
        /// The graphics device
        /// </summary>
        public GraphicsDeviceManager graphics;

        /// <summary>
        /// The MPQ content manager
        /// </summary>
        public ZipContentManager MpqContent;

        /// <summary>
        /// The project matrix for drawing
        /// </summary>
        public static Matrix projection;

        /// <summary>
        /// The view matrix for drawing
        /// </summary>
        public static Matrix view;

        /// <summary>
        /// The currently loaded map
        /// </summary>
        public Map CurrentMap;

        /// <summary>
        /// The current interface
        /// </summary>
        public Gui Interface;

        /// <summary>
        /// Provides random numbers to the entire engine
        /// </summary>
        public static Random Random = new Random();

        /// <summary>
        /// A list of units on the map
        /// </summary>
        public UnitGroup Units = new UnitGroup(128);
        
        /// <summary>
        /// The scripting engine
        /// </summary>
        public ScriptingEngine Script;

        /// <summary>
        /// The starting mouse position of a selection rectangle
        /// </summary>
        MouseState startMouse;

        /// <summary>
        /// The last mouse position
        /// </summary>
        MouseState prevMouse;

        /// <summary>
        /// The OnClick event - clicking on terrain
        /// </summary>
        private OnClickHandler _OnClick = null;
        
        /// <summary>
        /// The OnSelect event - selecting units
        /// </summary>
        private OnSelectHandler _OnSelect = null;
        
        /// <summary>
        /// Information for the OnClick handler
        /// </summary>
        public object OnClickTag;

        /// <summary>
        /// Information for the OnSelect handler
        /// </summary>
        public object OnSelectTag;

        /// <summary>
        /// The OnClick event - clicking on terrain
        /// </summary>
        public OnClickHandler OnClick {
            get { return _OnClick; }
            set { _OnClick = value; _OnSelect = null; }
        }
        
        /// <summary>
        /// The OnSelect event - selecting units
        /// </summary>
        public OnSelectHandler OnSelect {
            get { return _OnSelect; }
            set { _OnSelect = value; _OnClick = null; }
        }


        /// <summary>
        /// Creates a new instance of the game
        /// </summary>
        public League() {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            // Maps are stored as MPQ files - this allows us to read the data
            MpqContent = new ZipContentManager(Services, "maps/" + Settings.Default.map + ".zip");
            Script = new ScriptingEngine(this);

            Engine = this;
        }

        /// <summary>
        /// Sets up game specific things
        /// </summary>
        protected override void Initialize() {
            graphics.PreferredBackBufferWidth = 800;
            graphics.PreferredBackBufferHeight = 600;
            graphics.IsFullScreen = false;
            graphics.ApplyChanges();

            // Current player is the local player
            Player.CurrentPlayer = Player.GetPlayer(0);
            Player.NeutralPlayer = Player.GetPlayer(12);
            startMouse = prevMouse = Mouse.GetState();

            CurrentMap = new Map(this);
            Components.Add(CurrentMap);
            
            Interface = new Gui(this);
            Components.Add(Interface);

            // Initialize 3d matrices
            view = Matrix.CreateTranslation(0f, 0f, -125f) * Matrix.CreateFromYawPitchRoll(0f, 0.866f, 0f);
            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45.0f),
            graphics.GraphicsDevice.Viewport.Width / (float)graphics.GraphicsDevice.Viewport.Height, 1.0f, 10000.0f);

            base.Initialize();
        }

        /// <summary>
        /// Load content which can't be done anywhere else
        /// </summary>
        protected override void LoadContent() {
            Player.Selection = new GameModel(Content.Load<Model>("models/selection"));
            Player.SightCircle = Content.Load<Texture2D>("shaders/vision");
            Tooltip.TooltipBox = Content.Load<Texture2D>("gui/tooltipbox");
            Tooltip.TooltipCorner = Content.Load<Texture2D>("gui/tooltipcorner");
            Tooltip.TooltipEdge = Content.Load<Texture2D>("gui/tooltipedge");
            Projectile.Skin = Content.Load<Texture2D>("textures/projectile");
        }

        /// <summary>
        /// The main game loop
        /// </summary>
        /// <param name="gameTime">A snapshot of timing values</param>
        protected override void Update(GameTime gameTime) {
            // If we haven't initialized the map, do so
            if (!CurrentMap.Initialized) {
                CurrentMap.MapInit();
            }

            IsMouseVisible = false;

            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            MouseState ms = Mouse.GetState();
            KeyboardState ks = Keyboard.GetState();

            if (ks.IsKeyDown(Keys.Escape))
                Exit();

            // Change the view
            Vector3 worldtransform = Vector3.Zero;
            if (ks.IsKeyDown(Keys.Left) || ms.X <= 3)
                worldtransform.X = 1f;
            else if (ks.IsKeyDown(Keys.Right) || ms.X >= graphics.PreferredBackBufferWidth - 4)
                worldtransform.X = -1f;
            if (ks.IsKeyDown(Keys.Up) || ms.Y <= 3)
                worldtransform.Z = 1f;
            else if (ks.IsKeyDown(Keys.Down) || ms.Y >= graphics.PreferredBackBufferHeight - 4)
                worldtransform.Z = -1f;
            if (ks.IsKeyDown(Keys.PageUp))
                worldtransform.Y = -1f;
            else if (ks.IsKeyDown(Keys.PageDown))
                worldtransform.Y = 1f;

            worldtransform = Vector3.Transform(worldtransform * elapsed, Matrix.CreateScale(75f) * Matrix.Invert(CurrentMap.Rotation));
            CurrentMap.World *= Matrix.CreateTranslation(worldtransform.X, worldtransform.Y, worldtransform.Z);

            // Enforce some kind of map bounds - should be done with respect to the world height

            CurrentMap.World.M41 = MathHelper.Clamp(CurrentMap.World.M41, -(CurrentMap.Width / 2), (CurrentMap.Width / 2));
            CurrentMap.World.M43 = MathHelper.Clamp(CurrentMap.World.M43, -(CurrentMap.Height / 2), (CurrentMap.Height / 2));

            if (ks.IsKeyDown(Keys.Divide))
                CurrentMap.Rotation.Yaw -= (float)(MathHelper.PiOver2 * elapsed);
            else if (ks.IsKeyDown(Keys.Multiply))
                CurrentMap.Rotation.Yaw += (float)(MathHelper.PiOver2 * elapsed);

            // Gets/Sets control groups
            int cgid = GetD(ks);
            if (cgid != -1)
                if (ks.IsKeyDown(Keys.LeftControl))
                    Player.CurrentPlayer.SetControlGroup(cgid);
                else
                    Player.CurrentPlayer.RecallControlGroup(cgid);

            // The last selected units - used for failed OnSelects
            UnitGroup prevselected = new UnitGroup(Player.CurrentPlayer.Selected.ToArray());
            bool changeselected = false;


            // A smart event is occuring
            if (prevMouse.RightButton == ButtonState.Pressed && ms.RightButton == ButtonState.Released && ((ms.Y >= 467 && ms.X <= 133) || ms.Y < 457) && Player.CurrentPlayer.Selected.Count > 0 && Player.CurrentPlayer.Selected[0].Owner == Player.CurrentPlayer) {
                Unit obj = GetUnitAtScreenPoint(new Vector2(ms.X, ms.Y));

                Vector3 worldspace = Vector3.Zero;
                if (ms.Y >= 467) {
                    Vector2 mapspace = new Vector2((ms.X - 69) / 128f * CurrentMap.Width, (ms.Y - 531) / 128f * CurrentMap.Height);
                    worldspace = new Vector3(mapspace.X, CurrentMap.GetHeight(mapspace), mapspace.Y);
                } else
                    worldspace = CurrentMap.GetWorldPos(new Point(ms.X, ms.Y)) ?? Vector3.Zero;

                object target = obj;
                if (target == null)
                    target = worldspace;

                foreach (Unit caster in Player.CurrentPlayer.Selected) {
                    bool success = false;

                    if (caster != null) {
                        // Check each button for a smart - if it has one and returns true,
                        // the smart is handled.
                        foreach (ContextButton button in caster.Type.Actions) {
                            if (button.TypeOf == typeof(Ability)) {
                                string aid = (string)button.Tag;
                                Ability ability = Ability.GetAbility(aid);
                                success = Ability.InvokeSmart(ability.Code, aid, caster, target);
                                if (success)
                                    break;
                            }
                        }
                    }

                    // If the smart is not handled, we'll do it ourselves
                    if (!success)
                        if (obj != null)
                            foreach (Unit selected in Player.CurrentPlayer.Selected)
                                if (selected.Type.Attacks && obj.Owner != Player.CurrentPlayer)
                                    selected.OrderAttack(obj);
                                else
                                    selected.OrderMove(worldspace);
                        else
                            foreach (Unit selected in Player.CurrentPlayer.Selected)
                                if (selected.Type.IsBuilding)
                                    selected.RallyPoint = new Vector2(worldspace.X, worldspace.Z);
                                else if (selected.Type.Moves)
                                    selected.OrderMove(worldspace);
                }
            }


            if (prevMouse.LeftButton == ButtonState.Pressed && ms.LeftButton == ButtonState.Released && OnClick == null && ms.Y < 457) {
                // A single left mouse click has occured - selected the unit at the cursor
                if (startMouse.X == ms.X && startMouse.Y == ms.Y) {
                    if (!ks.IsKeyDown(Keys.LeftShift))
                        Player.CurrentPlayer.Selected.Clear();

                    Unit obj = GetUnitAtScreenPoint(new Vector2(ms.X, ms.Y));
                    if (obj != null) {
                        Player.CurrentPlayer.Select(obj);
                        Player.CurrentPlayer.RefreshActions();
                        changeselected = true;
                    } else
                        OnSelect = null;
                } else {
                    // A selection rectangle has occured - select all units in it
                    Rectangle r = new Rectangle(Math.Min(startMouse.X, ms.X), Math.Min(startMouse.Y, ms.Y), Math.Max(startMouse.X, ms.X) - Math.Min(startMouse.X, ms.X), Math.Max(startMouse.Y, ms.Y) - Math.Min(startMouse.Y, ms.Y));
                    if (!ks.IsKeyDown(Keys.LeftShift))
                        Player.CurrentPlayer.Selected.Clear();

                    foreach (Unit obj in Units) {
                        Vector3 pos = obj.Project();
                        if (r.Contains(new Point((int)pos.X, (int)pos.Y)) || (Player.CurrentPlayer.Selected.Contains(obj) && ks.IsKeyDown(Keys.LeftShift))) {
                            if (!Player.CurrentPlayer.Selected.Contains(obj)) {
                                Player.CurrentPlayer.Select(obj);
                                Player.CurrentPlayer.RefreshActions();
                                changeselected = true;
                            }
                        }
                    }

                    if (Player.CurrentPlayer.Selected.Count == 0)
                        OnSelect = null;
                }
            }

            // Reverts selection changes if OnSelect was enabled and requested
            if (OnSelect != null && changeselected) {
                if (!OnSelect(Player.CurrentPlayer.Selected[0], OnSelectTag))
                    Player.CurrentPlayer.Selected = prevselected;
                else
                    Player.CurrentPlayer.CoerceSelectionSize(1);
                OnSelect = null;
            }


            if (prevMouse.LeftButton == ButtonState.Pressed && ms.LeftButton == ButtonState.Released && OnClick != null && ((ms.Y >= 467 && ms.X <= 133) || ms.Y < 457)) {
                // An action targeting the ground has occured
                Vector3 worldspace = Vector3.Zero;
                if (ms.Y >= 467) {
                    Vector2 mapspace = new Vector2((ms.X - 69) / 128f * CurrentMap.Width, (ms.Y - 531) / 128f * CurrentMap.Height);
                    worldspace = new Vector3(mapspace.X, CurrentMap.GetHeight(mapspace), mapspace.Y);
                } else
                    worldspace = CurrentMap.GetWorldPos(new Point(ms.X, ms.Y)) ?? Vector3.Zero;
                if (OnClick(new Point(ms.X, ms.Y), worldspace, OnClickTag))
                    OnClick = null;
            } else if (ms.LeftButton == ButtonState.Released) {
                startMouse = ms;
            } else if (ms.LeftButton == ButtonState.Pressed) {
               if (ms.Y >= 467 && ms.X <= 133 && ms.Y <= 595 && ms.X >= 5 && OnClick == null) {
                   // someone is moving via minimap
                   Vector2 mapspace = new Vector2((ms.X - 69) / 128f * CurrentMap.Width, (ms.Y - 531) / 128f * CurrentMap.Height);

                   CurrentMap.World.M41 = -mapspace.X;
                   CurrentMap.World.M43 = -mapspace.Y;
                } else {
                   // There is a selection rectangle in the works - add the lines for it
                   Vector2 topleft = new Vector2(startMouse.X, startMouse.Y);
                   Vector2 topright = new Vector2(ms.X, startMouse.Y);
                   Vector2 botleft = new Vector2(startMouse.X, ms.Y);
                   Vector2 botright = new Vector2(ms.X, ms.Y);
                   Interface.Lines.Add(new Line(topleft, topright, Color.LimeGreen));
                   Interface.Lines.Add(new Line(topleft, botleft, Color.LimeGreen));
                   Interface.Lines.Add(new Line(topright, botright, Color.LimeGreen));
                   Interface.Lines.Add(new Line(botleft, botright, Color.LimeGreen));
                }
            }

            prevMouse = ms;

            base.Update(gameTime);
        }

        /// <summary>
        /// The main drawing loop
        /// </summary>
        /// <param name="gameTime">A snapshot of timing values</param>
        protected override void Draw(GameTime gameTime) {
            graphics.GraphicsDevice.Clear(new Color(25, 25, 25));
            base.Draw(gameTime);
        }

        /// <summary>
        /// Transforms a screen point to a unit
        /// </summary>
        /// <param name="ms">The mouse point</param>
        /// <returns>The unit directly under the mouse</returns>
        public Unit GetUnitAtScreenPoint(Vector2 ms) {

            Vector3 rayStart = graphics.GraphicsDevice.Viewport.Unproject(
                new Vector3(ms.X, ms.Y, 1.0f),
                projection, view, Matrix.Identity);

            Vector3 rayEnd = graphics.GraphicsDevice.Viewport.Unproject(
                    new Vector3(ms.X, ms.Y, 0.0f),
                    projection, view, Matrix.Identity);

            Ray r = new Ray(rayStart, Vector3.Normalize(rayEnd - rayStart));
            Vector3 raypos = r.Position;
            Vector3 raydir = r.Direction;

            foreach (Unit obj in Units) {
                Model m = obj.Type.Mesh.Model;
                Matrix[] bones = new Matrix[m.Bones.Count];
                m.CopyAbsoluteBoneTransformsTo(bones);

                // Check every modelmesh's bounding sphere against the ray
                foreach (ModelMesh mesh in m.Meshes) {
                    Matrix mat = Matrix.Invert(obj.GetTransformation());
                    Ray clickRay = new Ray(Vector3.Transform(raypos, mat), Vector3.TransformNormal(raydir, mat));

                    if (mesh.BoundingSphere.Intersects(clickRay).HasValue == true)
                        return obj;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the first D button which is down
        /// </summary>
        /// <param name="ks"></param>
        /// <returns></returns>
        public int GetD(KeyboardState ks) {
            for (int i = 0; i < 10; i++)
                if (ks.IsKeyDown((Keys)((int)Keys.D0 + i)))
                    return i;
            return -1;
        }
    }

    /// <summary>
    /// The OnClick delegate
    /// </summary>
    /// <param name="screen">Screen space</param>
    /// <param name="world">Worldspace</param>
    /// <param name="tag">An attached tag</param>
    /// <returns>Succesful - unhook</returns>
    public delegate bool OnClickHandler(Point screen, Vector3 world, object tag);

    /// <summary>
    /// The OnSelect delegate
    /// </summary>
    /// <param name="obj">The unit selected</param>
    /// <param name="tag">An attached tag</param>
    /// <returns>Revert selection or not</returns>
    public delegate bool OnSelectHandler(Unit obj, object tag);
}
