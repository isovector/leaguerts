using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using TargetClassification = LeagueEngine.Classification;
using System.ComponentModel;

namespace LeagueEngine {
    [DefaultProperty("Uid")]
    public class UnitType {
        /// <summary>
        /// The mesh associated with this UnitType
        /// </summary>
        public GameModel Mesh;

        /// <summary>
        /// The skin for the mesh
        /// </summary>
        public Texture2D Skin;

        /// <summary>
        /// A list of actions for this UnitType
        /// </summary>
        public List<ContextButton> Actions;
        
        /// <summary>
        /// The SLK data for this UnitType
        /// </summary>
        public Dictionary<string, string> UnitData;

        /// <summary>
        /// The button which produces this UnitType
        /// </summary>
        public ContextButton Button;

        /// <summary>
        /// A list of Uids trained by this UnitType
        /// </summary>
        public List<string> Trains;

        /// <summary>
        /// A list of Uids built by this UnitType
        /// </summary>
        public List<string> Builds;

        /// <summary>
        /// A list of assets required to create this UnitType
        /// </summary>
        public List<string> Requirements;

        /// <summary>
        /// The classification of this UnitType
        /// </summary>
        public Classification Classification = Classification.None;
        
        /// <summary>
        /// The allowed attack targets of this UnitType
        /// </summary>
        public Target AttackTarget;

        /// <summary>
        /// A dictionary of resource names against costs of resource costs for this UnitType
        /// </summary>
        public Dictionary<string, int> Costs;

        private bool _ConstrainToGround, _UseTeamColor, _IsBuilding, _Attacks, _Moves;
        private string _AttackGfx, _Uid, _Name, _Description, _AttackMagic, _AttackMagicData;
        private int _AttackRange, _AttackEngage, _AttackSpeed, _AttackDamage, _BuildTime, _Sight, _Hp, _Energy, _IconX, _IconY;
        private float _Height, _AttackCooldown, _AttackGfxSize, _Scale, _SelectionCircleSize, _Speed, _TurnSpeed;

        #region Getters and Setters
        // I hate doing things like this - it seems like so much more work
        // and it is. Unfortunately to use a PropertyGrid, we need these
        // declarations.

        /// <summary>
        /// Is this Unit's height relative to the ground?
        /// </summary>
        [Category("Movement"), Description("States whether Height is relative to the ground.")]
        public bool ConstrainToGround {
            get { return _ConstrainToGround; }
            set { _ConstrainToGround = value; }
        }

        /// <summary>
        /// Does this Unit show its team color? Most likely unused
        /// </summary>
        // TODO: Use this
        [Category("Art"), Description("Does this unit show team color?")]
        public bool UseTeamColor {
            get { return _UseTeamColor; }
            set { _UseTeamColor = value; }
        }

        /// <summary>
        /// Is it a building?
        /// </summary>
        [Category("Stats"), Description("Is this a building?")]
        public bool IsBuilding {
            get { return _IsBuilding; }
            set { _IsBuilding = value; }
        }

        /// <summary>
        /// Does it attack?
        /// </summary>
        [Category("Combat"), Description("Does this unit attack?")]
        public bool Attacks {
            get { return _Attacks; }
            set { _Attacks = value; }
        }

        /// <summary>
        /// Does it move?
        /// </summary>
        [Category("Movement"), Description("Does this unit move?")]
        public bool Moves {
            get { return _Moves; }
            set { _Moves = value; }
        }

        /// <summary>
        /// The path to it's Projectile model relative to projectiles/
        /// </summary>
        [Category("Combat"), Description("Path to projectile model.")]
        public string AttackGfx {
            get { return _AttackGfx; }
            set { _AttackGfx = value; }
        }

        /// <summary>
        /// The unique ID for this UnitType
        /// </summary>
        [Category("Stats"), Description("The unique ID for this UnitType."), ReadOnly(true)]
        public string Uid {
            get { return _Uid; }
            set { _Uid = value; }
        }

        /// <summary>
        /// The name of the UnitType
        /// </summary>
        // TODO: Localize this
        [Category("Text"), Description("This unit's name.")]
        public string Name {
            get { return _Name; }
            set { _Name = value; }
        }

        /// <summary>
        /// The tooltip description for this UnitType
        /// </summary>
        [Category("Text"), Description("The tooltip description for this unit.")]
        public string Description {
            get { return _Description; }
            set { _Description = value; }
        }

        /// <summary>
        /// The Projectile event code for the Projectile
        /// </summary>
        [Category("Combat"), Description("The Projectile event code invoked regarding this Projectile.")]
        public string AttackMagic {
            get { return _AttackMagic; }
            set { _AttackMagic = value; }
        }

        /// <summary>
        /// A unit specific piece of data to pass to the Projectile event
        /// </summary>
        [Category("Combat"), Description("A unit specific piece of data to pass to the Projectile event.")]
        public string AttackMagicData {
            get { return _AttackMagicData; }
            set { _AttackMagicData = value; }
        }

        /// <summary>
        /// The range of this unit's attack
        /// </summary>
        [Category("Combat"), Description("The range of this unit's attack.")]
        public int AttackRange {
            get { return _AttackRange; }
            set { _AttackRange = value; }
        }

        /// <summary>
        /// The distance at which this unit can acquire enemies
        /// </summary>
        [Category("Combat"), Description("The distance at which this unit can acquire enemies.")]
        public int AttackEngage {
            get { return _AttackEngage; }
            set { _AttackEngage = value; }
        }

        /// <summary>
        /// The speed of this unit's projectile
        /// </summary>
        [Category("Combat"), Description("The speed of this unit's projectile.")]
        public int AttackSpeed {
            get { return _AttackSpeed; }
            set { _AttackSpeed = value; }
        }

        /// <summary>
        /// The damage of this unit
        /// </summary>
        [Category("Combat"), Description("The damage of this unit.")]
        public int AttackDamage {
            get { return _AttackDamage; }
            set { _AttackDamage = value; }
        }

        /// <summary>
        /// How long this unit takes to build/train
        /// </summary>
        [Category("Stats"), Description("How long this unit takes to build/train.")]
        public int BuildTime {
            get { return _BuildTime; }
            set { _BuildTime = value; }
        }

        /// <summary>
        /// The diameter of the unit's sight circle
        /// </summary>
        [Category("Stats"), Description("The diameter of the unit's sight circle.")]
        public int Sight {
            get { return _Sight; }
            set { _Sight = value; }
        }

        /// <summary>
        /// The amount of life this unit has
        /// </summary>
        [Category("Stats"), Description("The amount of life this unit has.")]
        public int Hp {
            get { return _Hp; }
            set { _Hp = value; }
        }

        /// <summary>
        /// The amount of energy this unit has
        /// </summary>
        [Category("Stats"), Description("The amount of energy this unit has.")]
        public int Energy {
            get { return _Energy; }
            set { _Energy = value; }
        }

        /// <summary>
        /// The X coordinate of this unit's icon in the Action panel
        /// </summary>
        [Category("Art"), Description("The X coordinate of this unit's icon in the Action panel. Must be between 0 - 4 inclusive.")]
        public int IconX {
            get { return _IconX; }
            set { _IconX = value; }
        }

        /// <summary>
        /// The Y coordinate of this unit's icon in the Action panel
        /// </summary>
        [Category("Art"), Description("The Y coordinate of this unit's icon in the Action panel. Must be between 0 - 2 inclusive.")]
        public int IconY {
            get { return _IconY; }
            set { _IconY = value; }
        }

        /// <summary>
        /// The height at which this unit moves
        /// </summary>
        [Category("Movement"), Description("The height at which this unit moves.")]
        public float Height {
            get { return _Height; }
            set { _Height = value; }
        }

        /// <summary>
        /// The time in seconds between projectiles being fired
        /// </summary>
        [Category("Combat"), Description("The time in seconds between projectiles being fired.")]
        public float AttackCooldown {
            get { return _AttackCooldown; }
            set { _AttackCooldown = value; }
        }

        /// <summary>
        /// The scale of the projectile model
        /// </summary>
        [Category("Combat"), Description("The scale of the projectile model.")]
        public float AttackGfxSize {
            get { return _AttackGfxSize; }
            set { _AttackGfxSize = value; }
        }

        /// <summary>
        /// The size of this unit with respect to it's model
        /// </summary>
        [Category("Art"), Description("The size of this unit with respect to it's model.")]
        public float Scale {
            get { return _Scale; }
            set { _Scale = value; }
        }

        /// <summary>
        /// The diameter of the unit's selection circle
        /// </summary>
        [Category("Art"), Description("The diameter of the unit's selection circle.")]
        public float SelectionCircleSize {
            get { return _SelectionCircleSize; }
            set { _SelectionCircleSize = value; }
        }

        /// <summary>
        /// The speed at which the unit moves per second
        /// </summary>
        [Category("Movement"), Description("The speed at which the unit moves per second.")]
        public float Speed {
            get { return _Speed; }
            set { _Speed = value; }
        }

        /// <summary>
        /// The speed at which the unit turns per second
        /// </summary>
        [Category("Movement"), Description("The speed at which the unit turns per second.")]
        public float TurnSpeed {
            get { return _TurnSpeed; }
            set { _TurnSpeed = value; }
        }

        private string _MeshPath, _IconPath, _SkinPath;
        private string[] _UnitsTrained, _UnitsBuilt, _Dependencies, _AttackAllowedTarget, _Abilities, _ClassifiedAs, _ResourceCosts;

        // The following properties are only used by UnitEd
        /// <summary>
        /// The path to this unit's model. This is only used by UnitEd
        /// </summary>
        [Category("Art"), Description("The path to this unit's model.")]
        public string MeshPath {
            get { return _MeshPath; }
            set { _MeshPath = value; }
        }

        /// <summary>
        /// The path to this unit's icon. This is only used by UnitEd
        /// </summary>
        [Category("Art"), Description("The path to this unit's icon.")]
        public string IconPath {
            get { return _IconPath; }
            set { _IconPath = value; }
        }
        
        /// <summary>
        /// The skin applied to this unit's model. This is only used by UnitEd
        /// </summary>
        [Category("Art"), Description("The skin applied to this unit's model.")]
        public string SkinPath {
            get { return _SkinPath; }
            set { _SkinPath = value; }
        }

        /// <summary>
        /// The cost to produce this unit. This is only used by UnitEd
        /// </summary>
        [Category("Stats"), Description("The cost to produce this unit. Each entry should be formatted as `resource=###` without the quotes.")]
        public string[] ResourceCosts {
            get { return _ResourceCosts; }
            set { _ResourceCosts = value; }
        }

        /// <summary>
        /// The units which this unit trains. This is only used by UnitEd
        /// </summary>
        [Category("Stats"), Description("The units which this unit trains.")]
        public string[] UnitsTrained {
            get { return _UnitsTrained; }
            set { _UnitsTrained = value; }
        }

        /// <summary>
        /// The buildings built by this unit. This is only used by UnitEd
        /// </summary>
        [Category("Stats"), Description("The buildings built by this unit.")]
        public string[] UnitsBuilt {
            get { return _UnitsBuilt; }
            set { _UnitsBuilt = value; }
        }

        /// <summary>
        /// What is required before this unit may be created. This is only used by UnitEd
        /// </summary>
        [Category("Stats"), Description("What is required before this unit may be created.")]
        public string[] Dependencies {
            get { return _Dependencies; }
            set { _Dependencies = value; }
        }

        /// <summary>
        /// What this unit is allowed to attack. This is only used by UnitEd
        /// </summary>
        [Category("Combat"), Description("What this unit is allowed to attack.")]
        public string[] AttackAllowedTarget {
            get { return _AttackAllowedTarget; }
            set { _AttackAllowedTarget = value; }
        }

        /// <summary>
        /// Abilities possessed by this unit. This is only used by UnitEd
        /// </summary>
        [Category("Stats"), Description("Abilities possessed by this unit.")]
        public string[] Abilities {
            get { return _Abilities; }
            set { _Abilities = value; }
        }

        /// <summary>
        /// This unit's classification. This is only used by UnitEd
        /// </summary>
        [Category("Stats"), Description("This unit's classification.")]
        public string[] ClassifiedAs {
            get { return _ClassifiedAs; }
            set { _ClassifiedAs = value; }
        }
        #endregion

        /// <summary>
        /// Creates a blank UnitType
        /// </summary>
        public UnitType() {
        }

        /// <summary>
        /// Loads a UnitType from a uid
        /// </summary>
        /// <param name="uid">The uid to load</param>
        public UnitType(string uid) {
            // Load lots of goodies from the SLK
            MeshPath = GameData.GetUnitData<string>(uid, "model");
            if (League.Engine != null)
                Mesh = new GameModel(League.Engine.Content.Load<Model>("models/" + MeshPath));
            Height = GameData.GetUnitData<float>(uid, "moveHeight");
            Speed = GameData.GetUnitData<float>(uid, "moveSpeed");
            TurnSpeed = GameData.GetUnitData<float>(uid, "turnSpeed");
            ConstrainToGround = GameData.GetUnitData<bool>(uid, "moveOnGround");
            Scale = GameData.GetUnitData<float>(uid, "scale");
            BuildTime = GameData.GetUnitData<int>(uid, "buildTime");
            Name = GameData.GetUnitData<string>(uid, "comment");
            SelectionCircleSize = GameData.GetUnitData<float>(uid, "selectionCircle");
            IsBuilding = GameData.GetUnitData<bool>(uid, "isBuilding");
            UseTeamColor = GameData.GetUnitData<bool>(uid, "useTeamColor");
            Moves = GameData.GetUnitData<bool>(uid, "moveSpeed");
            Attacks = GameData.GetUnitData<bool>(uid, "attack1");
            AttackGfx = GameData.GetUnitData<string>(uid, "attackGfx1");
            AttackGfxSize = GameData.GetUnitData<float>(uid, "attackScale1");
            AttackMagic = GameData.GetUnitData<string>(uid, "attackMagic1");
            AttackMagicData = GameData.GetUnitData<string>(uid, "attackMagicData1");
            AttackRange = GameData.GetUnitData<int>(uid, "attackRng1");
            AttackSpeed = GameData.GetUnitData<int>(uid, "attackSpeed1");
            AttackEngage = GameData.GetUnitData<int>(uid, "attackEngage1");
            AttackDamage = GameData.GetUnitData<int>(uid, "attackDmg1");
            AttackAllowedTarget = GameData.GetUnitData<string>(uid, "attackTarget1") != null
                ? GameData.GetUnitData<string>(uid, "attackTarget1").Split(',')
                : new string[0];
            AttackTarget = new Target(GameData.GetUnitData<string>(uid, "attackTarget1") != null
                ? GameData.GetUnitData<string>(uid, "attackTarget1")
                : "");
            Sight = GameData.GetUnitData<int>(uid, "sight");
            AttackCooldown = GameData.GetUnitData<float>(uid, "attackCooldown1");
            Hp = GameData.GetUnitData<int>(uid, "hp");
            Energy = GameData.GetUnitData<int>(uid, "energy");
            Description = GameData.GetUnitData<string>(uid, "desc");

            SkinPath = GameData.GetUnitData<string>(uid, "skin");
            if (String.IsNullOrEmpty(SkinPath))
                SkinPath = "jamie";
            if (League.Engine != null)
                Skin = League.Engine.Content.Load<Texture2D>("textures/" + SkinPath);

            if (GameData.GetUnitData<string>(uid, "classification") != null) {
                ClassifiedAs = GameData.GetUnitData<string>(uid, "classification").Split(',');
                Classification = Target.GetClassification(GameData.GetUnitData<string>(uid, "classification"));
            }

            List<ContextButton> actions = new List<ContextButton>();
            if (Moves) {
                // Give him appropriate actions
                actions.Add(ContextButton.Move);
                actions.Add(ContextButton.Stop);
                actions.Add(ContextButton.Patrol);
            }

            if (Attacks) {
                // Give him appropriate actions
                actions.Add(ContextButton.Guard);
                actions.Add(ContextButton.Attack);
                
                Classification |= Classification.Attacks;
                
                // TODO: Actually see if they can attack air and or ground
                Classification |= Classification.AttacksAir;
                Classification |= Classification.AttacksGround;
            }

            Costs = new Dictionary<string, int>();
            if (!String.IsNullOrEmpty(GameData.GetUnitData<string>(uid, "cost"))) {
                ResourceCosts = GameData.GetUnitData<string>(uid, "cost").Split(',');

                foreach (string dat in ResourceCosts) {
                    string[] bits = dat.Split('=');
                    Costs.Add(bits[0].ToLower(), int.Parse(bits[1]));
                }
            }

            if (!String.IsNullOrEmpty(GameData.GetUnitData<string>(uid, "requires"))) {
                Dependencies = GameData.GetUnitData<string>(uid, "requires").Split(',');
                Requirements = new List<string>(Dependencies);
            } else {
                Requirements = new List<string>();
            }

            // Construct the tooltip
            IconPath = GameData.GetUnitData<string>(uid, "icon");
            IconX = GameData.GetUnitData<int>(uid, "x");
            IconY = GameData.GetUnitData<int>(uid, "y");
            if (League.Engine != null) {
                Button = new ContextButton(League.Engine.Content.Load<Texture2D>("icons/" + IconPath),
                        "Train " + Name, Description, Requirements,
                        IconX, IconY, TrainPress);

                foreach (KeyValuePair<string, int> pair in Costs) {
                    Resource r = Resource.Resources[pair.Key.ToLower()];
                    switch (r.PreferredIconNumber) {
                        case 1:
                            Button.Tooltip.Icon1 = new Tooltip.IconNumber(r.Icon, pair.Value);
                            break;
                        case 2:
                            Button.Tooltip.Icon2 = new Tooltip.IconNumber(r.Icon, pair.Value);
                            break;
                        case 3:
                            Button.Tooltip.Icon3 = new Tooltip.IconNumber(r.Icon, pair.Value);
                            break;
                        case 4:
                            Button.Tooltip.Icon4 = new Tooltip.IconNumber(r.Icon, pair.Value);
                            break;
                        case 5:
                            Button.Tooltip.Icon5 = new Tooltip.IconNumber(r.Icon, pair.Value);
                            break;
                        case 6:
                            Button.Tooltip.Icon6 = new Tooltip.IconNumber(r.Icon, pair.Value);
                            break;
                    }
                }
            }

            if (League.Engine != null) {
                if (IsBuilding) {
                    Button.Tooltip.Name = "Build " + Name;
                    Button.Action = BuildPress;
                    Classification |= Classification.Structure;
                }

                Button.Tag = uid;
                Button.TypeOf = typeof(UnitType);
                Actions = new List<ContextButton>(actions.ToArray());
            }

            UnitData = GameData.UnitData.GetRow(uid);
            Uid = uid;
        }

        /// <summary>
        /// Finishes creating the UnitType. This must be done separately so we don't get stack overflows
        /// when we add references to other UnitTypes.
        /// </summary>
        public void FinishConstruction() {
            if (!String.IsNullOrEmpty(GameData.GetUnitData<string>(Uid, "trains"))) {
                if (League.Engine != null)
                    Actions.Add(ContextButton.Rally);
                Trains = new List<string>();
                UnitsTrained = GameData.GetUnitData<string>(Uid, "trains").Split(',');
                if (League.Engine != null)
                    foreach (string tuid in UnitsTrained) {
                        Actions.Add(UnitType.GetUnitType(tuid));
                        Trains.Add(tuid);
                    }
            }

            if (!String.IsNullOrEmpty(GameData.GetUnitData<string>(Uid, "builds"))) {
                List<ContextButton> builds = new List<ContextButton>();
                UnitsBuilt = GameData.GetUnitData<string>(Uid, "builds").Split(',');
                if (League.Engine != null) {
                    foreach (string tuid in UnitsBuilt)
                        builds.Add(UnitType.GetUnitType(tuid));

                    // This is a button which looks like a cancel but actually just resets our buttons
                    builds.Add(new ContextButton(League.Engine.Content.Load<Texture2D>("icons/cancel"),
                       "Cancel", "", null, 4, 2, ContextButton.ChangeActions));
                    ContextButton button = new ContextButton(League.Engine.Content.Load<Texture2D>("icons/cancel"),
                       "Build Structure", "", null, 0, 2, ContextButton.ChangeActions);
                    button.Tag = new List<ContextButton>(builds.ToArray());
                    Actions.Add(button);
                }
            }

            if (!String.IsNullOrEmpty(GameData.GetUnitData<string>(Uid, "abilities"))) {
                Abilities = GameData.GetUnitData<string>(Uid, "abilities").Split(',');
                if (League.Engine != null)
                    foreach (string aid in Abilities)
                        Actions.Add(Ability.GetAbility(aid));
            }
        }

        /// <summary>
        /// Determines whether a UnitType can train another looking at one player's assets
        /// </summary>
        /// <param name="uid">The unit being trained</param>
        /// <param name="p">The player at whose assets to look</param>
        /// <returns>Can it train it?</returns>
        public bool CanTrainUnit(string uid, Player p) {
            UnitType u = GetUnitType(uid);
            return Trains.Contains(uid)
                && p.MatchesAssets(u.Requirements)
                && p.MatchesResources(u.Costs);
        }

        /// <summary>
        /// Gets a UnitType by looking it up or by loading it from SLK
        /// </summary>
        /// <param name="uid">The uid to get</param>
        /// <returns>The UnitType</returns>
        public static UnitType GetUnitType(string uid) {
            UnitType type = (Types.ContainsKey(uid) ? Types[uid] : new UnitType(uid));
            if (!Types.ContainsKey(uid)) {
                Types.Add(uid, type);
                type.FinishConstruction();
            }
            return type;
        }

        /// <summary>
        /// Creates a new UnitType based on another. Used by UnitEd to derrive
        /// UnitTypes.
        /// </summary>
        /// <param name="uid">The new Uid</param>
        /// <param name="baseuid">The Uid to base the copy on</param>
        /// <returns>The new UnitType</returns>
        public static UnitType NewUnitType(string uid, string baseuid) {
            UnitType type = (Types.ContainsKey(uid) ? Types[baseuid] : new UnitType(baseuid));
            if (!Types.ContainsKey(baseuid)) {
                Types.Add(baseuid, type);
                type.FinishConstruction();
            }
            UnitType newtype = (UnitType)type.MemberwiseClone();
            newtype.Uid = uid;
            Types.Add(uid, newtype);
            return type;
        }

        /// <summary>
        /// Implicitly turns a UnitType into it's Button
        /// </summary>
        /// <param name="type">The UnitType to convert</param>
        /// <returns>The Button of the UnitType</returns>
        public static implicit operator ContextButton(UnitType type) {
            return type.Button;
        }

        /// <summary>
        /// Creates a unit at the desired location for the given player
        /// </summary>
        /// <param name="uid">The uid to create</param>
        /// <param name="pos">The position to create the unit</param>
        /// <param name="player">The player for whom to create the unit</param>
        /// <returns>The created unit</returns>
        public static Unit CreateUnit(string uid, Vector3 pos, Player player) {
            GetUnitType(uid);
            Unit u = new Unit(League.Engine, GetUnitType(uid), pos, player);
            League.Engine.Components.Add(u);
            League.Engine.Units.Add(u);
            player.AddAsset(uid);
            League.Engine.Script.InvokeEvent("UnitCreated", null, u);
            return u;
        }
        
        /// <summary>
        /// Creates a unit at the desired location for the given player
        /// </summary>
        /// <param name="uid">The uid to create</param>
        /// <param name="pos">The position to create the unit</param>
        /// <param name="player">The player for whom to create the unit</param>
        /// <returns>The created unit</returns>
        public static Unit CreateUnit(string uid, Vector2 pos, Player player) {
            GetUnitType(uid);
            Unit u = new Unit(League.Engine, GetUnitType(uid), pos, player);
            League.Engine.Components.Add(u);
            League.Engine.Units.Add(u);
            player.AddAsset(uid);
            League.Engine.Script.InvokeEvent("UnitCreated", null, u);
            return u;
        }

        /// <summary>
        /// Determines whether a UnitType can train another looking at one player's assets
        /// </summary>
        /// <param name="trainer">The training unit</param>
        /// <param name="uid">The unit being trained</param>
        /// <param name="p">The player at whose assets to look</param>
        /// <returns>Can it train it?</returns>
        public static bool CanTrainUnit(string trainer, string uid, Player p) {
            UnitType u = UnitType.GetUnitType(uid);
            return (UnitType.GetUnitType(trainer).Trains.Contains(uid)
                && p.MatchesAssets(u.Requirements))
                && p.MatchesResources(u.Costs);
        }

        /// <summary>
        /// A Dictionary of uids against UnitTypes to store all loaded UnitTypes
        /// </summary>
        public static Dictionary<string, UnitType> Types = new Dictionary<string, UnitType>(128);

        /// <summary>
        /// The Action to perform when a unit is to be trained
        /// </summary>
        private static ContextButton.ButtonAction TrainPress = new ContextButton.ButtonAction(Train_Press);

        /// <summary>
        /// How to handle unit train clicks
        /// </summary>
        /// <param name="name">The button name</param>
        /// <param name="tag">The button tag - in our case the uid to train</param>
        private static void Train_Press(string name, object tag) {
            string uid = (string)tag;
            foreach (Unit selected in Player.CurrentPlayer.Selected) {
                if (UnitType.CanTrainUnit(selected.Type.Uid, uid, Player.CurrentPlayer) && selected.Training.Count < 10 && Player.CurrentPlayer.MatchesResources(UnitType.GetUnitType(uid).Costs)) {
                    Player.CurrentPlayer.ChargeResources(UnitType.GetUnitType(uid).Costs);
                    selected.Training.Enqueue(uid);
                    if (selected.Training.Count == 1)
                        selected.TrainTime = GameData.GetUnitData<int>(uid, "buildTime");
                }
            }
        }

        /// <summary>
        /// The Action to perform when a building is to be created
        /// </summary>
        private static ContextButton.ButtonAction BuildPress = new ContextButton.ButtonAction(Build_Press);

        /// <summary>
        /// How to handle building creations
        /// </summary>
        /// <param name="name">The button name</param>
        /// <param name="tag">The button tag - in our case the uid to build</param>
        private static void Build_Press(string name, object tag) {
            string uid = (string)tag;
            if (Player.CurrentPlayer.MatchesResources(UnitType.GetUnitType(uid).Costs)) {
                Unit selected = Player.CurrentPlayer.Selected[0];
                League.Engine.OnClick = new OnClickHandler(delegate(Point screen, Vector3 world, object ctag) {
                    if (screen.Y > 487) return false;
                    selected.OrderBuild(uid, new Vector2(world.X, world.Z));
                    return true;
                });
            }
            League.Engine.Interface.Actions = null;
        }
    }
}