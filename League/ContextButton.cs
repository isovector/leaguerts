using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace LeagueEngine {
    /// <summary>
    /// A clickable button relating to an action
    /// </summary>
    public class ContextButton {
        /// <summary>
        /// An action to be run when this is clicked
        /// </summary>
        /// <param name="name">The text of the button</param>
        /// <param name="tag">The tag provided to the button</param>
        public delegate void ButtonAction(string name, object tag);

        /// <summary>
        /// The icon shown
        /// </summary>
        public Texture2D Texture;

        /// <summary>
        /// The name of the button. Somewhat deprecate.
        /// </summary>
        public string Name;

        /// <summary>
        /// The tooltip shown on MouseOver
        /// </summary>
        public Tooltip Tooltip;

        /// <summary>
        /// The bounding rectangle for this button.
        /// </summary>
        public Rectangle Rectangle;

        /// <summary>
        /// The action invoked on click.
        /// </summary>
        public ButtonAction Action;

        /// <summary>
        /// The key to use as a hotkey
        /// </summary>
        public Keys Hotkey;
        
        /// <summary>
        /// Specific information given to the ButtonAction
        /// </summary>
        public object Tag;

        /// <summary>
        /// The creating type of this button. Can be UnitType, Ability or null.
        /// </summary>
        public Type TypeOf = null;

        /// <summary>
        /// A grid of hotkeys used to determine hotkeys automatically.
        /// </summary>
        static Keys[,] HotkeyGrid = new Keys[5, 3];

        /// <summary>
        /// Whether the hotkey grid has been initialized.
        /// </summary>
        static bool Initialized = false;

        /// <summary>
        /// Makes a Matrix of hotkeys for every button position
        /// </summary>
        static void SetupHotkeys() {
            Initialized = true;
            HotkeyGrid[0, 0] = Keys.Q;
            HotkeyGrid[1, 0] = Keys.W;
            HotkeyGrid[2, 0] = Keys.E;
            HotkeyGrid[3, 0] = Keys.R;
            HotkeyGrid[4, 0] = Keys.T;
            HotkeyGrid[0, 1] = Keys.A;
            HotkeyGrid[1, 1] = Keys.S;
            HotkeyGrid[2, 1] = Keys.D;
            HotkeyGrid[3, 1] = Keys.F;
            HotkeyGrid[4, 1] = Keys.G;
            HotkeyGrid[0, 2] = Keys.Z;
            HotkeyGrid[1, 2] = Keys.X;
            HotkeyGrid[2, 2] = Keys.C;
            HotkeyGrid[3, 2] = Keys.V;
            HotkeyGrid[4, 2] = Keys.B;
        }

        /// <summary>
        /// Creates a ContextButton
        /// </summary>
        /// <param name="texture">The icon</param>
        /// <param name="name">The name</param>
        /// <param name="tooltip">The description of the button</param>
        /// <param name="reqs">A list of assets pre-requisited.</param>
        /// <param name="x">The x position of the button</param>
        /// <param name="y">The y position of the button</param>
        /// <param name="action">The event of the button</param>
        public ContextButton(Texture2D texture, string name, string tooltip, List<string> reqs, int x, int y, ButtonAction action) {
            if (!Initialized)
                SetupHotkeys();

            Texture = texture;
            Name = name;

            // Create a tooltip from the given information
            Tooltip = new Tooltip(name, tooltip, reqs, null, null, null);

            // Saying x = y = -1 means show no icon
            if (x != -1 && y != -1)
                Rectangle = new Rectangle(612 + x * 36, 491 + y * 36, 36, 36);
            else
                Rectangle = new Rectangle(-1, -1, 0, 0);
            Action = action;

            if (x != -1 && y != -1)
                Hotkey = HotkeyGrid[x, y];
        }

        /// <summary>
        /// The standard move button
        /// </summary>
        public static ContextButton Move = new ContextButton(null, "Move", " ", null, 0, 0, new ButtonAction(Move_Action));

        /// <summary>
        /// The standard stop button
        /// </summary>
        public static ContextButton Stop = new ContextButton(null, "Stop", " ", null, 1, 0, new ButtonAction(Stop_Action));

        /// <summary>
        /// The standard stand guard button
        /// </summary>
        public static ContextButton Guard = new ContextButton(null, "Guard", " ", null, 2, 0, new ButtonAction(Stop_Action));

        /// <summary>
        /// The standard patrol button
        /// </summary>
        public static ContextButton Patrol = new ContextButton(null, "Patrol", " ", null, 3, 0, new ButtonAction(Move_Action));

        /// <summary>
        /// The standard attack button
        /// </summary>
        public static ContextButton Attack = new ContextButton(null, "Attack", " ", null, 4, 0, new ButtonAction(Attack_Action));

        /// <summary>
        /// The standard set rally point button
        /// </summary>
        public static ContextButton Rally = new ContextButton(null, "Set Rally Point", " ", null, 3, 2, new ButtonAction(Rally_Action));

        /// <summary>
        /// The standard cancel button
        /// </summary>
        public static ContextButton Cancel = new ContextButton(null, "Cancel", " ", null, 4, 2, new ButtonAction(Cancel_Action));

        /// <summary>
        /// Used as a template and for keeping track of non-ContextButton related tooltips.
        /// </summary>
        public static ContextButton Null = new ContextButton(null, "", "", null, 0, 0, new ButtonAction(Null_Action));


        static void Move_Action(string name, object tag) {
            League.Engine.OnClickTag = (name == "Patrol" ? UnitState.Patrolling : UnitState.Moving);
            League.Engine.OnClick = new OnClickHandler(Do_Move);
        }

        static void Attack_Action(string name, object tag) {
            League.Engine.OnSelect = new OnSelectHandler(Do_Attack);
            League.Engine.OnSelectTag = new List<Unit>(Player.CurrentPlayer.Selected);
        }

        static void Rally_Action(string name, object tag) {
            League.Engine.OnClick = new OnClickHandler(Do_Rally);
        }

        static void Stop_Action(string name, object tag) {
            foreach (Unit obj in Player.CurrentPlayer.Selected) {
                obj.Cancel();
                if (name == "Guard")
                    obj.State = UnitState.Guarding;
                else
                    obj.State = UnitState.None;
            }
        }

        static void Cancel_Action(string name, object tag) {
            foreach (Unit obj in Player.CurrentPlayer.Selected) {
                if (obj.Training.Count != 0)
                    obj.Training.DequeueAt(obj.Training.Count - 1);
                if (obj.CurrentlyBuilding) {
                    obj.Hp = 0;
                    obj.CurrentlyBuilding = false;
                }
            }
        }

        static void Null_Action(string name, object tag) {
        }

        /// <summary>
        /// Swaps the Action panel with a new one - used for Building menus
        /// </summary>
        public static ButtonAction ChangeActions = new ButtonAction(Change_Press);
        static void Change_Press(string name, object tag) {
            League.Engine.Interface.Actions = tag as List<ContextButton>;
        }

        static bool Do_Move(Point screen, Vector3 world, object tag) {
            foreach (Unit obj in Player.CurrentPlayer.Selected) {
                obj.Node = 0;
                obj.TargetUnit = null;
                League.Engine.CurrentMap.FindPath(obj, League.Engine.CurrentMap.GetNode(world));
                obj.State = (UnitState)tag;
            }
            return true;
        }

        static bool Do_Rally(Point screen, Vector3 world, object tag) {
            foreach (Unit obj in Player.CurrentPlayer.Selected)
                obj.RallyPoint = new Vector2(world.X, world.Z);
            return true;
        }

        static bool Do_Attack(Unit selected, object tag) {
            List<Unit> selection = tag as List<Unit>;

            foreach (Unit unit in selection)
                unit.OrderAttack(selected);

            return false;
        }
    }
}
