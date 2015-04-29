using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace LeagueEngine {
    public class Player : DrawableGameComponent {
        /// <summary>
        /// The shape of a Unit's vision circle
        /// </summary>
        public static Texture2D SightCircle;

        /// <summary>
        /// The model used to indicate selection of a unit
        /// </summary>
        public static GameModel Selection;

        /// <summary>
        /// The instance of League
        /// </summary>
        public League Engine;

        /// <summary>
        /// A SpriteBatch to draw fog vision
        /// </summary>
        SpriteBatch Batch;
        
        /// <summary>
        /// The color of this Player
        /// </summary>
        public Color TeamColor;

        /// <summary>
        /// This Player's unique id
        /// </summary>
        public int Pid;

        /// <summary>
        /// The units currently selected by this Player
        /// </summary>
        public UnitGroup Selected = new UnitGroup();

        /// <summary>
        /// The previously selected serialized unit group of the Player. Used
        /// to determine when to recalculate the action panel.
        /// </summary>
        public string PrevSelected = "";

        /// <summary>
        /// A list of assets (what the Player owns) to match requirements
        /// against.
        /// </summary>
        public List<string> Assets = new List<string>();

        /// <summary>
        /// A list of control groups for the player.
        /// </summary>
        public List<UnitGroup> ControlGroups;

        /// <summary>
        /// The render target on which fog of war is calculated.
        /// </summary>
        public RenderTarget2D FogMap;

        /// <summary>
        /// A list of Allies of the player. Doesn't do much right now.
        /// TODO: Should be replaced by a team id.
        /// </summary>
        public List<Player> Allies = new List<Player>();

        /// <summary>
        /// The pixels of the fog map to turn vision into fog.
        /// </summary>
        public Color[] FogMapPixels;

        /// <summary>
        /// The time (out of 2.0f) until the fog is recalculated
        /// </summary>
        public float Time = 1.9f;

        /// <summary>
        /// Has the Player's fog map been rendered yet?
        /// </summary>
        public bool HasFogMap = false;

        /// <summary>
        /// States whether the player's vision does not become clouded
        /// by fog of war.
        /// </summary>
        public bool IsOmniscient = false;

        /// <summary>
        /// The player's resources, a dictionary of strings of resources against
        /// the amount of it they have.
        /// </summary>
        public Dictionary<string, int> Resources = new Dictionary<string, int>();


        /// <summary>
        /// Creates a new Player from a Pid. This should not be called, instead call Player.GetPlayer()
        /// </summary>
        /// <param name="game">The instance of League</param>
        /// <param name="pid">The id of the player</param>
        public Player(League game, int pid) : base(game) {
            Pid = pid;

            // The player's color is based on their pid
            TeamColor = Colors[pid];
            Engine = game;

            // Set control groups if this is the Current Player
            if (pid == 0) {
                ControlGroups = new List<UnitGroup>();
                for (int i = 0; i < 10; i++)
                    ControlGroups.Add(new UnitGroup());
            }

            // TODO: Find a better place to put this.
            Tooltip.IconNumber.Energy = Game.Content.Load<Texture2D>("icons/tooltips/energy");
            Tooltip.IconNumber.Handicap = Game.Content.Load<Texture2D>("icons/tooltips/handicap");
            Tooltip.IconNumber.Time = Game.Content.Load<Texture2D>("icons/tooltips/time");
            Tooltip.IconNumber.Money = Game.Content.Load<Texture2D>("icons/tooltips/money");

            // Give the player the default amount of all resources.
            foreach (KeyValuePair<string, Resource> pair in Resource.Resources)
                Resources.Add(pair.Key, pair.Value.Amount);

            FogMapPixels = new Color[512 * 512];
        }

        /// <summary>
        /// Initializes the component
        /// </summary>
        public override void Initialize() {
            // Draw selection circles after the map but before units

            DrawOrder = 1;
            base.Initialize();
        }

        /// <summary>
        /// Loads all content relevant to this player.
        /// </summary>
        protected override void LoadContent() {
            Batch = new SpriteBatch(GraphicsDevice);
            FogMap = new RenderTarget2D(GraphicsDevice, 512, 512, 1, SurfaceFormat.Color, RenderTargetUsage.PreserveContents);
            GraphicsDevice.SetRenderTarget(0, FogMap);
            GraphicsDevice.Clear(Color.Black);
            GraphicsDevice.SetRenderTarget(0, null);

            base.LoadContent();
        }

        /// <summary>
        /// Draws selection circles
        /// </summary>
        /// <param name="gameTime">A snapshot of game timing values</param>
        public override void Draw(GameTime gameTime) {
            // We will only draw selections for the current player
            if (this == Player.CurrentPlayer) {
                foreach (Unit u in Selected)
                    Selection.Draw(Matrix.CreateScale(u.Type.SelectionCircleSize) * u.GetPositionTransformation(), u.Owner.TeamColor);
            }

            base.Draw(gameTime);
        }

        /// <summary>
        /// Add (or remove) a resource from this player's resources.
        /// </summary>
        /// <param name="type">The resource to modify</param>
        /// <param name="amount">The amount to add. Can be negative to remove a resource.</param>
        public void AddResource(string type, int amount) {
            Resources[type.ToLower()] += amount;
        }

        /// <summary>
        /// Sets a resource amount for this player.
        /// </summary>
        /// <param name="type">The resource to modify</param>
        /// <param name="amount">The new value of the resource</param>
        public void SetResource(string type, int amount) {
            Resources[type.ToLower()] = amount;
        }

        /// <summary>
        /// Charges a batch of resources at once
        /// </summary>
        /// <param name="costs">A dictionary of resource names against costs</param>
        public void ChargeResources(Dictionary<string, int> costs) {
            foreach (KeyValuePair<string, int> pair in costs)
                Resources[pair.Key] -= pair.Value;
        }

        /// <summary>
        /// Equivalent to a map vision cheat
        /// </summary>
        public void DisableBlackMask() {
            GraphicsDevice.SetRenderTarget(0, FogMap);
            GraphicsDevice.Clear(Color.White);
            GraphicsDevice.SetRenderTarget(0, null);
            IsOmniscient = true;
        }

        /// <summary>
        /// Adds an asset to the player
        /// </summary>
        /// <param name="id">The to add</param>
        public void AddAsset(string id) {
            if (!Assets.Contains(id))
                Assets.Add(id);
        }

        /// <summary>
        /// Attempts to remove an asset if the player has no more
        /// </summary>
        /// <param name="id">The asset to remove</param>
        public void RemoveAsset(string id) {
            if (Assets.Contains(id)) {
                int count = 0;
                
                // Count the number of this asset the Player has
                foreach (Unit u in Engine.Units)
                    if (u.Type.Uid == id && u.Owner == this)
                        count++;

                // If she has none, we can remove the asset
                if (count == 0)
                    Assets.Remove(id);
            }
        }

        /// <summary>
        /// Determines whether the Player has a batch of assets
        /// </summary>
        /// <param name="assets">The assets to check</param>
        /// <returns>Whether all assets are owned</returns>
        public bool MatchesAssets(List<string> assets) {
            if (assets != null)
                foreach (string asset in assets)
                    if (!Assets.Contains(asset))
                        return false;
            return true;
        }

        /// <summary>
        /// Determines whether the Player's resources are sufficient
        /// for a cost.
        /// </summary>
        /// <param name="cost">A dictionary of resource names against cost to check</param>
        /// <returns>Enough resources to do the requested action.</returns>
        public bool MatchesResources(Dictionary<string, int> cost) {
            foreach (KeyValuePair<string, int> pair in cost)
                if (Resources[pair.Key] < pair.Value)
                    return false;
            return true;
        }

        /// <summary>
        /// Adds units to the Player's selection group
        /// </summary>
        /// <param name="us">The units to select</param>
        public void Select(params Unit[] us) {
            foreach (Unit u in us)
                Selected.Add(u);
            ValidateSelection();
        }

        /// <summary>
        /// Sets a control group to the currently selected group
        /// </summary>
        /// <param name="cgid">The control group to set.</param>
        public void SetControlGroup(int cgid) {
            if (this == CurrentPlayer) {
                ControlGroups[cgid].Clear();
                foreach (Unit u in Selected)
                    if (u.Owner == CurrentPlayer)
                        ControlGroups[cgid].Add(u);
            }
        }

        /// <summary>
        /// Recalls a control group to the currently selected group
        /// </summary>
        /// <param name="cgid">The control group to get</param>
        public void RecallControlGroup(int cgid) {
            if (this == CurrentPlayer) {
                Selected.Clear();
                foreach (Unit u in ControlGroups[cgid])
                    if (League.Engine.Units.Contains(u))
                        Selected.Add(u);
            }
        }

        /// <summary>
        /// Calculates the fog map for the player based on all of her
        /// units.
        /// </summary>
        public void CalculateFogMap() {
            if (HasFogMap && !IsOmniscient) {
                // Turn vision into fog

                for (int i = 0; i < FogMapPixels.Length; i++)
                    if (FogMapPixels[i] != Color.Black)
                        FogMapPixels[i] = Color.Gray;

                FogMap.GetTexture().SetData<Color>(FogMapPixels);
            }

            RenderTarget oldRT = GraphicsDevice.GetRenderTarget(0);
            GraphicsDevice.SetRenderTarget(0, FogMap);

            
            Batch.Begin(SpriteBlendMode.Additive, SpriteSortMode.Immediate, SaveStateMode.SaveState);

            lock (League.Engine.Units) {
                try {
                    foreach (Unit unit in League.Engine.Units)
                        if (unit.Owner == this) {
                            // Draw the circle
                            Rectangle destRect = new Rectangle();
                            destRect.X = (int)(unit.Position.X + (League.Engine.CurrentMap.Width / 2.0f) * (512f / League.Engine.CurrentMap.Width)) - (unit.Type.Sight / 2);
                            destRect.Y = (int)(unit.Position.Y + (League.Engine.CurrentMap.Height / 2.0f) * (512f / League.Engine.CurrentMap.Height)) - (unit.Type.Sight / 2);
                            destRect.Width = unit.Type.Sight;
                            destRect.Height = unit.Type.Sight;
                            Batch.Draw(SightCircle, destRect, Color.White);
                        }
                } catch { }
            }

            Batch.End();

            GraphicsDevice.SetRenderTarget(0, oldRT as RenderTarget2D);
            FogMap.GetTexture().GetData(FogMapPixels);

            HasFogMap = true;

            League.Engine.CurrentMap.DrawMinimap();
        }

        /// <summary>
        /// Updates the component
        /// </summary>
        /// <param name="gameTime">A snapshot of game timing values.</param>
        public override void Update(GameTime gameTime) {
            if (this == CurrentPlayer) {
                float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
                for (int i = 0; i < Selected.Count; i++)
                    // Remove dead and non-visible units from selection
                    if (!Engine.Units.Contains(Selected[i]) || GetPointVisibility(Selected[i].Position) != Visibility.Visible)
                         Selected.Remove(Selected[i]);

                // If our selection has changed, we'll remake our actions list
                if (PrevSelected != Selected.ToString() || Engine.Interface.Actions == null) {
                    RefreshActions();
                    PrevSelected = Selected.ToString();
                }

                Time += elapsed;
                if (Time > 0.2f) {
                    // Calculate the fog map every .2 seconds
                    CalculateFogMap();
                    Time = 0;
                }
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// Updates our list of actions from the selected units.
        /// </summary>
        public void RefreshActions() {
            if (Selected.Count != 0 && Selected[0].Owner == this)
                Engine.Interface.Actions = Selected[0].GetActions();
            else
                Engine.Interface.Actions = null;
        }

        /// <summary>
        /// Asserts that a selection is valid in that it conforms to the following
        /// criteria, and they have the following priority:
        /// - Multiple units may be selected if they are all owned by the player
        /// - Multiple buildings if they are the same type and owned by the player
        /// - Single enemy unit
        /// - Single enemy building
        /// </summary>
        private void ValidateSelection() {
            UnitGroup playerunits = new UnitGroup(), nonplayerunits = new UnitGroup(),
                nonplayerbuildings = new UnitGroup();
            Dictionary<string, UnitGroup> playerbuildings = new Dictionary<string, UnitGroup>();
            Dictionary<string, int> counts = new Dictionary<string,int>();

            foreach (Unit unit in Selected) {
                if (unit.Owner == this) {
                    if (!unit.Type.IsBuilding)
                        playerunits.Add(unit);
                    else {
                        string uid = unit.Type.Uid;
                        if (playerbuildings.ContainsKey(uid)) {
                            playerbuildings[uid].Add(unit);
                            counts[uid]++;
                        }
                        else {
                            playerbuildings.Add(uid, new UnitGroup());
                            playerbuildings[uid].Add(unit);
                            counts.Add(uid, 1);
                        }
                    }
                } else {
                    if (!unit.Type.IsBuilding)
                        nonplayerunits.Add(unit);
                    else
                        nonplayerbuildings.Add(unit);
                }
            }

            if (nonplayerunits.Count != 0) {
                Unit badunit = nonplayerunits[0];
                nonplayerunits.Clear();
                nonplayerunits.Add(badunit);
            }
            if (nonplayerbuildings.Count != 0) {
                Unit badbuild = nonplayerbuildings[0];
                nonplayerbuildings.Clear();
                nonplayerbuildings.Add(badbuild);
            }

            int max = 0;
            string bkey = "";
            foreach (KeyValuePair<string, int> pair in counts) {
                if (pair.Value > max) {
                    max = pair.Value;
                    bkey = pair.Key;
                }
            }

            Selected = (playerunits.Count != 0 ? playerunits : 
                (playerbuildings.Count != 0 ? playerbuildings[bkey] :
                (nonplayerunits.Count != 0 ? nonplayerunits : 
                (nonplayerbuildings.Count != 0 ? nonplayerbuildings : new UnitGroup()))));
            
            CoerceSelectionSize(27);
        }

        /// <summary>
        /// Resizes the player's selection to be at most a certain size
        /// </summary>
        /// <param name="size">The new max size of the selection</param>
        public void CoerceSelectionSize(int size) {
            if (Selected.Count > size) {
                Unit[] copy = new Unit[size];
                Selected.CopyTo(0, copy, 0, size);
                Selected = new UnitGroup(copy);
            }
        }

        /// <summary>
        /// Gets the Team of a unit in relation to the player
        /// </summary>
        /// <param name="u">The unit to check</param>
        /// <returns>Team.Owner if belonging to the player, Team.Allies if to her allies, Team.Enemy otherwise.</returns>
        public Team GetUnitTeam(Unit u) {
            if (u.Owner == this)
                return Team.Owner;
            else if (Allies.Contains(u.Owner))
                return Team.Allies;
            return Team.Enemy;
        }

        /// <summary>
        /// Gets the visibility of a point on the map
        /// </summary>
        /// <param name="pos">The point to check</param>
        /// <returns>A Visibility enum describing the point</returns>
        public Visibility GetPointVisibility(Vector2 pos) {
            int x = (int)((pos.X + League.Engine.CurrentMap.Height / 2) * 512 / League.Engine.CurrentMap.Width);
            int y = (int)((pos.Y + League.Engine.CurrentMap.Height / 2) * 512 / League.Engine.CurrentMap.Height);
            Color c = FogMapPixels[y * 512 + x];
            if (c == Color.White) return Visibility.Visible;
            else if (c == Color.Black) return Visibility.Masked;
            return Visibility.Fogged;
        }


        /// <summary>
        /// Gets a player by its uid
        /// </summary>
        /// <param name="pid">The pid to get</param>
        /// <returns>The player indicated by the pid.</returns>
        public static Player GetPlayer(int pid) {
            Player p = (Players.ContainsKey(pid) ? Players[pid] : new Player(League.Engine, pid));
            if (!Players.ContainsKey(pid)) {
                Players.Add(pid, p);
                League.Engine.Components.Add(p);
            }
            return p;
        }

        public override bool Equals(object obj) { return Pid.Equals(obj); }
        public override int GetHashCode() { return Pid.GetHashCode(); }
        public override string ToString() { return Pid.ToString(); }
        public static bool operator ==(Player a, Player b) { return a.Pid == b.Pid; }
        public static bool operator !=(Player a, Player b) { return a.Pid != b.Pid; }

        /// <summary>
        /// The number of players currently loaded
        /// </summary>
        public static int PlayerCount = 0;

        /// <summary>
        /// The local player
        /// </summary>
        public static Player CurrentPlayer;

        /// <summary>
        /// The neutral player
        /// </summary>
        public static Player NeutralPlayer;

        /// <summary>
        /// A dictionary of Pids against players
        /// </summary>
        public static Dictionary<int, Player> Players = new Dictionary<int, Player>();

        /// <summary>
        /// The team colors for Pids
        /// </summary>
        private static Color[] Colors = new Color[] { Color.Red, Color.Blue, Color.Teal, Color.Purple, Color.Yellow, Color.Orange, Color.Green, Color.Pink, Color.Gray, Color.LightBlue, Color.DarkGreen, Color.Brown, Color.White };
    }
}
