using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Algorithms;
using Microsoft.Xna.Framework.Graphics;

namespace LeagueEngine {
    /// <summary>
    /// Describes a Unit in the game, beloning to a UnitType
    /// </summary>
    public class Unit : GameObject {
        /// <summary>
        /// The base type of this Unit
        /// </summary>
        public UnitType Type;

        /// <summary>
        /// The position in 3D of this unit
        /// </summary>
        public Vector3 Position3D {
            get {
                float ypos = Type.Height;
                if (Engine.CurrentMap.IsInMapBounds(Position))
                    ypos += (Type.ConstrainToGround ? Engine.CurrentMap.GetHeight(Position) : 0);
                return new Vector3(Position.X, ypos, Position.Y);
            }
        }

        /// <summary>
        /// The multiplier (0-1 inclusive) of this Unit's HP 
        /// </summary>
        public float HpPercentage {
            get { if (Type.Hp == 0) return 1; return (float)Hp / Type.Hp; }
            set { Hp = (int)(value * Type.Hp); }
        }

        /// <summary>
        /// The multiplier (0-1 inclusive) of this Unit's Energy 
        /// </summary>
        public float EnergyPercentage {
            get { if (Type.Energy == 0) return 1; return (float)Energy / Type.Energy; }
            set { Energy = (int)(value * Type.Energy); }
        }

        /// <summary>
        /// The ability this Unit is casting
        /// </summary>
        public Ability CastingAbility = null;
        
        /// <summary>
        /// This Unit's current movement path
        /// </summary>
        public List<Point> Path = null;

        /// <summary>
        /// The current node of the movement path this unit is on
        /// </summary>
        public int Node;

        /// <summary>
        /// The HP of the Unit
        /// </summary>
        public int Hp;

        /// <summary>
        /// The Energy of the unit
        /// </summary>
        public int Energy;

        /// <summary>
        /// The current State of this unit
        /// </summary>
        public UnitState State = UnitState.None;

        /// <summary>
        /// The Player to whom this unit belongs
        /// </summary>
        public Player Owner;

        /// <summary>
        /// The Unit currently being targeted by this Unit. May be an attackee or
        /// the target of an Ability.
        /// </summary>
        public Unit TargetUnit;

        /// <summary>
        /// For incomplete buildings, the Unit working on this building.
        /// </summary>
        public Unit Builder = null;

        /// <summary>
        /// The time before this Unit can attack again
        /// </summary>
        public float AttackCooldown = 0f;

        /// <summary>
        /// For buildings, the current time this Unit has beein training for
        /// </summary>
        public float TrainTime = 0f;

        /// <summary>
        /// For incomplete buildings, the time this Unit has beein building
        /// </summary>
        public float BuildTime = 0f;

        /// <summary>
        /// For buildings, the point where all produced Units find paths to
        /// </summary>
        public Vector2? RallyPoint = null;

        /// <summary>
        /// The point currently being targed by this Unit. May be a building location
        /// or the target of an Ability
        /// </summary>
        public Vector2 TargetPoint;

        /// <summary>
        /// For buildings, the units currently being trained by this Unit
        /// </summary>
        public UnitQueue Training = new UnitQueue();

        /// <summary>
        /// The Building to be built by this Unit
        /// </summary>
        public string QueuedBuilding = null;

        /// <summary>
        /// For buildings, state whether this Building is incomplete
        /// </summary>
        public bool CurrentlyBuilding = false;

        /// <summary>
        /// A list of Abilities and whether they are cooling down for this Unit
        /// </summary>
        public Dictionary<string, float> AbilityCooldown = new Dictionary<string, float>();

        /// <summary>
        /// The time unit this Unit is ready to cast its Ability
        /// </summary>
        public float? CastTime = null;

        /// <summary>
        /// Creates a new Unit
        /// </summary>
        /// <param name="game">The instance of Leauge</param>
        /// <param name="type">The Type of unit</param>
        /// <param name="pos">The position to create the Unit</param>
        /// <param name="player">The player to whom this Unit belongs</param>
        public Unit(League game, UnitType type, Vector3 pos, Player player) : this(game, type, new Vector2(pos.X, pos.Z), player) {}

        /// <summary>
        /// Creates a new Unit
        /// </summary>
        /// <param name="game">The instance of Leauge</param>
        /// <param name="type">The Type of unit</param>
        /// <param name="pos">The position to create the Unit</param>
        /// <param name="player">The player to whom this Unit belongs</param>
        public Unit(League game, UnitType type, Vector2 pos, Player player) : base(game) {
            Type = type;
            Position = pos;
            Owner = player;
            Hp = Type.Hp;
            Energy = Type.Energy;
        }

        /// <summary>
        /// Updates the component - calculates pathing maps, moves, attacks, casts abilities,
        /// builds, trains
        /// </summary>
        /// <param name="gameTime">A snapshot of game timing values</param>
        public override void Update(GameTime gameTime) {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Update times for everything
            AttackCooldown -= elapsed;

            if (CastTime != null)
                CastTime -= elapsed;

            string[] keys = new string[AbilityCooldown.Count];
            AbilityCooldown.Keys.CopyTo(keys, 0);
            for (int i = 0; i < AbilityCooldown.Count; i++) {
                AbilityCooldown[keys[i]] -= elapsed;
                if (AbilityCooldown[keys[i]] < 0)
                    AbilityCooldown.Remove(keys[i]);
            }

            if (TargetUnit != null && !Engine.Units.Contains(TargetUnit)) {
                // The target unit is dead
                TargetUnit = null;
                CastingAbility = null;
                Path = null;
                State &= ~(UnitState.Attacking | UnitState.Moving | UnitState.Casting);
            }

            if (CurrentlyBuilding) {
                // This is an incomplete building and should continue to build
                BuildTime += elapsed;
                Hp = (int)(BuildTime / Type.BuildTime * Type.Hp);

                if (BuildTime >= Type.BuildTime) {
                    // The building is complete!
                    CurrentlyBuilding = false;
                    Builder.State &= ~UnitState.Building;
                    Builder = null;

                    if (Player.CurrentPlayer.Selected.Count != 0 && Player.CurrentPlayer.Selected[0] == this)
                        // If this is the selected object, refresh actions
                        Player.CurrentPlayer.RefreshActions();
                }
            }
            
            if (Hp <= 0 && !(CurrentlyBuilding && BuildTime < 0.5f)) {
                // Time to DIE
                Die();

                return;
            }


            if (QueuedBuilding != null) {
                // There is a building to build
                if ((Position - TargetPoint).Length() <= 8 * UnitType.GetUnitType(QueuedBuilding).SelectionCircleSize) {
                    // We are in range to build it
                    if (Owner.MatchesResources(UnitType.GetUnitType(QueuedBuilding).Costs)) {
                        // We are able to build it
                        Path = null;
                        Unit u = UnitType.CreateUnit(QueuedBuilding, TargetPoint, Owner);
                        u.CurrentlyBuilding = true;
                        u.Builder = this;
                        Owner.ChargeResources(u.Type.Costs);
                        QueuedBuilding = null;
                        State = UnitState.Building;
                    } else {
                        // We can't build it :(
                        QueuedBuilding = null;
                        Path = null;
                    }
                } else if (Path == null) {
                    // We are not moving but need to get to the build site
                    State = UnitState.Moving;
                    Engine.CurrentMap.FindPath(this, Engine.CurrentMap.GetNode(TargetPoint));
                }
            }

            if (CastingAbility == null) {
                if (TargetUnit != null && TargetUnit != this) {
                    // We have a target and it's not us and it's not an Ability target
                     if ((TargetUnit.Position - Position).Length() < Type.AttackRange) {
                         // We're in range!
                         if (AttackCooldown <= 0)
                             DoAttack();
                    } else if (Path == null) {
                        // We're not in range so let's get there
                        Engine.CurrentMap.FindPath(this, Engine.CurrentMap.GetNode(TargetUnit.Position));
                        State = UnitState.Moving;
                    }
                }
            } else {
                // There's magic to be done
                if (!(TargetUnit == null && TargetPoint == Vector2.Zero) && Path == null && CastTime == null) {
                    // And we need to get there
                    Engine.CurrentMap.FindPath(this, Engine.CurrentMap.GetNode((CastingAbility.Target.Type == TargetType.Point ? TargetPoint : TargetUnit.Position)));
                    State = UnitState.Moving;
                }

                if ((TargetUnit == null && TargetPoint == Vector2.Zero) || (Position - (CastingAbility.Target.Type == TargetType.Point ? TargetPoint : TargetUnit.Position)).Length() <= CastingAbility.CastRange) {
                    // We're in range or there's no Target
                    if (CastTime == null) {
                        // And we haven't started casting
                        CastTime = CastingAbility.CastTime;
                        // Start casting ;)
                        Path = null;
                    } else if (CastTime <= 0) {
                        if (TargetUnit == null && TargetPoint == Vector2.Zero)
                            // Cast against nothing
                            CastingAbility.Invoke(this, null);
                        else
                            // Cast against our target
                            CastingAbility.Invoke(this, (CastingAbility.Target.Type == TargetType.Point ? (object)TargetPoint : (object)TargetUnit));
                    }
                }
            }

            if (Path != null) {
                lock (Path) {
                    // Move along our path
                    Vector3 pos = Engine.CurrentMap.GetNodePosition(Path[Node].X, Path[Node].Y);
                    Vector2 intended = new Vector2(pos.X, pos.Z);
                    Vector2 dif = intended - Position;
                    float angle = (float)(Math.Atan2(-dif.Y, dif.X) + MathHelper.PiOver2);
                    if (Rotation.Yaw < angle)
                        Rotation.Yaw += elapsed * Type.TurnSpeed;
                    else if (Rotation.Yaw > angle)
                        Rotation.Yaw -= elapsed * Type.TurnSpeed;
                    float xdir = Math.Sign(dif.X);
                    float ydir = Math.Sign(dif.Y);
                    Position.X += Type.Speed * elapsed * xdir;
                    Position.Y += Type.Speed * elapsed * ydir;

                    if (TargetUnit != null) {
                        if ((TargetUnit.Position - Position).Length() < Type.AttackRange) {
                            // Our target is in range!
                            Path = null;
                            State = UnitState.Attacking;
                            return;
                        }
                    }

                    if (Math.Abs(dif.X) < Type.Speed * elapsed * 2 && Math.Abs(dif.Y) < Type.Speed * elapsed * 2) {
                        // We're through this node - on to the next!
                        Node++;
                        if (Node == Path.Count && State != UnitState.Patrolling)
                            Path = null;
                        else if (State == UnitState.Patrolling) {
                            // If we're patrolling, repeat the path over and over
                            List<Point> newpath = new List<Point>();
                            for (int i = 1; i <= Path.Count; i++)
                                newpath.Add(Path[Path.Count - i]);
                            Path = newpath;
                            Node = 0;
                        }
                    }
                }
            } else if (Type.Attacks && TargetUnit == null && !CurrentlyBuilding)
                foreach (Unit u in Engine.Units)
                    if (u.Owner != Owner && (Position - u.Position).Length() <= Type.AttackEngage && u.Owner != Player.NeutralPlayer)
                        // We can attack and there are enemy units nearby
                        TargetUnit = u;

            if (Training.Count != 0) {
                // We're training units
                TrainTime -= elapsed;

                if (TrainTime <= 0f) {
                    // Training is complete, let's pop one out
                    TrainUnit(Training.Dequeue());
                    if (Training.Count != 0)
                        TrainTime = UnitType.GetUnitType(Training.Peek()).BuildTime;
                }
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// Trains a unit
        /// </summary>
        /// <param name="uid">The UID to build</param>
        /// <returns>The built unit</returns>
        public Unit TrainUnit(string uid) {
            Unit trained = UnitType.CreateUnit(uid, Position + Vector2.Transform(new Vector2(0f, 4f * Type.SelectionCircleSize), Matrix.CreateRotationZ(MathHelper.ToRadians(League.Random.Next(0, 360)))), Owner);
            if (RallyPoint != null && trained.Type.Moves) {
                // We have a rally point, so we might as well use it
                trained.State = UnitState.Moving;
                Engine.CurrentMap.FindPath(trained, Engine.CurrentMap.GetNode(RallyPoint ?? Vector2.Zero));
            }
            return trained;
        }

        /// <summary>
        /// Issue an order to build a unit
        /// </summary>
        /// <param name="uid">The uid to build</param>
        /// <param name="pos">The position to build it</param>
        public void OrderBuild(string uid, Vector2 pos) {
            Cancel();
            QueuedBuilding = uid;
            TargetPoint = pos;
        }

        /// <summary>
        /// Issues an order to move
        /// </summary>
        /// <param name="pos">The position to move to</param>
        public void OrderMove(Vector3 pos) {
            OrderMove(new Vector2(pos.X, pos.Z), true);
        }

        /// <summary>
        /// Issues an order to move
        /// </summary>
        /// <param name="pos">The position to move to</param>
        public void OrderMove(Vector2 pos) {
            OrderMove(pos, true);
        }

        /// <summary>
        /// Issues an order to move
        /// </summary>
        /// <param name="pos">The position to move to</param>
        /// <param name="cancel">Should this action trump others?</param>
        public void OrderMove(Vector2 pos, bool cancel) {
            if (cancel)
                Cancel();
            State |= UnitState.Moving;
            Engine.CurrentMap.FindPath(this, Engine.CurrentMap.GetNode(pos));
        }

        /// <summary>
        /// Issues an order to attack a Unit
        /// </summary>
        /// <param name="u">The unit to attack</param>
        public void OrderAttack(Unit u) {
            if (Type.AttackTarget.CompareUnit(this, u)) {
                Cancel();
                TargetUnit = u;
                Path = null;
            } else
                System.Windows.Forms.MessageBox.Show("Invalid Target");
        }

        /// <summary>
        /// Issues an order to cast a spell targetting nothing
        /// </summary>
        /// <param name="aid">The aid to cast</param>
        public void OrderCast(string aid) { OrderCast(Ability.GetAbility(aid)); }

        /// <summary>
        /// Issues an order to cast a spell targetting a Unit
        /// </summary>
        /// <param name="aid">The aid to cast</param>
        /// <param name="target">The target of the Ability</param>
        public void OrderCast(string aid, Unit target) { OrderCast(Ability.GetAbility(aid), target); }

        /// <summary>
        /// Issues an order to cast a spell targetting a Point
        /// </summary>
        /// <param name="aid">The aid to cast</param>
        /// <param name="target">The target of the Ability</param>
        public void OrderCast(string aid, Vector2 target) { OrderCast(Ability.GetAbility(aid), target); }

        /// <summary>
        /// Issues an order to cast a spell targetting anything
        /// </summary>
        /// <param name="aid">The aid to cast</param>
        /// <param name="target">The target of the Ability</param>
        public void OrderCast(string aid, object target) { OrderCast(Ability.GetAbility(aid), target); }

        /// <summary>
        /// Issues an order to cast a spell targetting nothing
        /// </summary>
        /// <param name="aid">The Ability to cast</param>
        public void OrderCast(Ability ability) {
            Cancel();
            CastingAbility = ability;
            TargetUnit = null;
            TargetPoint = Vector2.Zero;
            State |= UnitState.Casting;
        }

        /// <summary>
        /// Issues an order to cast a spell targetting a Unit
        /// </summary>
        /// <param name="ability">The Ability to cast</param>
        /// <param name="target">The target of the Ability</param>
        public void OrderCast(Ability ability, Unit target) {
            Cancel();
            CastingAbility = ability;
            TargetUnit = target;
            TargetPoint = Vector2.Zero;
            State |= UnitState.Casting;
        }

        /// <summary>
        /// Issues an order to cast a spell targetting a Point
        /// </summary>
        /// <param name="ability">The Ability to cast</param>
        /// <param name="target">The target of the Ability</param>
        public void OrderCast(Ability ability, Vector2 target) {
            Cancel();
            CastingAbility = ability;
            TargetPoint = target;
            TargetUnit = null;
            State |= UnitState.Casting;
        }

        /// <summary>
        /// Issues an order to cast a spell targetting anything
        /// </summary>
        /// <param name="ability">The Ability to cast</param>
        /// <param name="target">The target of the Ability</param>
        public void OrderCast(Ability ability, object target) {
            Cancel();
            if (target is Vector2)
                OrderCast(ability, (Vector2)target);
            else if (target is Unit)
                OrderCast(ability, (Unit)target);
        }

        /// <summary>
        /// Cancels all orders
        /// </summary>
        public void Cancel() {
            TargetUnit = null;
            Path = null;
            State = UnitState.None;
            QueuedBuilding = null;
            CastingAbility = null;
            CastTime = null;
            Training.Clear();
            TrainTime = 0;
        }

        /// <summary>
        /// Performs an attack
        /// </summary>
        private void DoAttack() {
            if (AttackCooldown <= 0) {
                AttackCooldown = Type.AttackCooldown;
                Projectile.MakeProjectile(this, TargetUnit, Type.AttackGfx, Type.AttackGfxSize, Type.AttackSpeed, Type.AttackDamage);
                Engine.Script.InvokeEvent("UnitAttacked", null, this, TargetUnit);
            }
        }

        /// <summary>
        /// Kills this Unit
        /// </summary>
        public void Die() {
            // TODO: provide dying animations
            Remove(false);
            Engine.Script.InvokeEvent("UnitDied", null, this);
        }

        /// <summary>
        /// Removes this unit and invokes the UnitRemoved event
        /// </summary>
        public override void Remove() {
            Remove(true);
        }

        /// <summary>
        /// Removes this unit
        /// </summary>
        /// <param name="fire">Should the UnitRemoved event be invoked?</param>
        public void Remove(bool fire) {
            if (fire)
                Engine.Script.InvokeEvent("UnitRemoved", null, this);
            Engine.Units.Remove(this);
            Owner.RemoveAsset(Type.Uid);

            base.Remove();
        }

        /// <summary>
        /// Draws this unit
        /// </summary>
        /// <param name="gameTime">A snapshot of game timing values</param>
        public override void Draw(GameTime gameTime) {
            Type.Mesh.Draw(this);
        }

        /// <summary>
        /// Gets the Uid of this unit
        /// </summary>
        /// <returns>The uid</returns>
        public override string ToString() {
            return Type.Uid;
        }

        /// <summary>
        /// Gets the actions of this Unit
        /// </summary>
        /// <returns>The actions of the unit</returns>
        public List<ContextButton> GetActions() {
            if (Type.IsBuilding && CurrentlyBuilding)
                // We are building, so we only get a cancel button
                return new List<ContextButton>(new ContextButton[] { ContextButton.Cancel });
            else if (Training.Count != 0) {
                // We are training, so we get all of our actions and a cancel button
                List<ContextButton> list = new List<ContextButton>(new ContextButton[] { ContextButton.Cancel });
                foreach (ContextButton button in Type.Actions)
                    list.Add(button);
                return list;
            }
            // We have all of our actions
            return Type.Actions;
        }

        /// <summary>
        /// Gets the 3D position of this unit. Overridden from GameObject
        /// </summary>
        /// <returns>The 3D position</returns>
        public override Vector3 GetPosition() {
            return Position3D;
        }

        /// <summary>
        /// Gets the world matrix of the unit. Overridden from GameObject;
        /// </summary>
        /// <returns>The world matrix</returns>
        public override Matrix GetTransformation() {
            return Rotation * Matrix.CreateScale(Type.Scale) * GetPositionTransformation();
        }

        /// <summary>
        /// Gets the world matrix of the specified ModelMesh. Overridden from GameObject
        /// </summary>
        /// <param name="mesh">The mesh to get</param>
        /// <returns>The world matrix of the mesh</returns>
        public override Matrix GetTransformation(ModelMesh mesh) {
            Matrix[] bones = new Matrix[Type.Mesh.Model.Bones.Count];
            Type.Mesh.Model.CopyAbsoluteBoneTransformsTo(bones);
            return bones[mesh.ParentBone.Index] * GetTransformation();
        }

        /// <summary>
        /// Gets the screen space of this Unit
        /// </summary>
        /// <returns>The screen space</returns>
        public Vector3 Project() {
            return Engine.GraphicsDevice.Viewport.Project(Position3D, League.projection, League.view, Engine.CurrentMap.World * Engine.CurrentMap.Rotation);
        }

        public static explicit operator Vector2(Unit u) {
            return u.Position;
        }
    }
}

