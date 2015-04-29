using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Reflection;
using LeagueEngine.Scripting;
using System.ComponentModel;

namespace LeagueEngine {
    /// <summary>
    /// Represents a special ability in League.
    /// </summary>
    public class Ability {
        /// <summary>
        /// The context button for this action.
        /// </summary>
        public ContextButton Button;

        private string _Code, _Aid;
        private float _CastTime, _Duration, _Cooldown, _Area, _CastRange;
        private int _Energy, _IconX, _IconY;

        #region Getters and Setters
        /// <summary>
        /// The specific data used for AbilityCast events. Describes how the action works.
        /// </summary>
        [Category("Stats"), Description("Describes the specific AbilityCast event invoked when this Ability is cast")]
        public string Code {
            get { return _Code; }
            set { _Code = value; }
        }

        /// <summary>
        /// A unique identifier for this Ability.
        /// </summary>
        [Category("Stats"), Description("A unique identifier for this Ability"), ReadOnly(true)]
        public string Aid {
            get { return _Aid; }
            set { _Aid = value; }
        }

        /// <summary>
        /// The time it takes this ability to be cast.
        /// </summary>
        [Category("Stats"), Description("The time it takes for this Ability to be cast in seconds")]
        public float CastTime {
            get { return _CastTime; }
            set { _CastTime = value; }
        }

        /// <summary>
        /// The time over which the ability is cast.
        /// </summary>
        [Category("Stats"), Description("The duration of this Ability, in seconds")]
        public float Duration {
            get { return _Duration; }
            set { _Duration = value; }
        }

        /// <summary>
        /// The minimum time before a recast of the ability.
        /// </summary>
        [Category("Stats"), Description("The minimum time before this Ability may be cast again, in seconds")]
        public float Cooldown {
            get { return _Cooldown; }
            set { _Cooldown = value; }
        }

        /// <summary>
        /// The area of which this ability has effect.
        /// </summary>
        [Category("Stats"), Description("The area over which this Ability is effective")]
        public float Area {
            get { return _Area; }
            set { _Area = value; }
        }

        /// <summary>
        /// The distance from which this ability can be cast.
        /// </summary>
        [Category("Stats"), Description("The max distance from which this Ability may be cast")]
        public float CastRange {
            get { return _CastRange; }
            set { _CastRange = value; }
        }

        /// <summary>
        /// The energy required to cast this ability.
        /// </summary>
        [Category("Stats"), Description("The energy cost exerted on a Unit to cast this Ability")]
        public int Energy {
            get { return _Energy; }
            set { _Energy = value; }
        }

        /// <summary>
        /// The X coordinate of this Ability's context button.
        /// </summary>
        [Category("Art"), Description("The X coordinate of this Ability's ContextButton. Set this to -1 if you do not want an Icon shown.")]
        public int IconX {
            get { return _IconX; }
            set { _IconX = value; }
        }

        /// <summary>
        /// The Y coordinate of this Ability's context button.
        /// </summary>
        [Category("Art"), Description("The X coordinate of this Ability's ContextButton. Set this to -1 if you do not want an Icon shown.")]
        public int IconY {
            get { return _IconY; }
            set { _IconY = value; }
        }

        /// <summary>
        /// This Ability's first data slot
        /// </summary>
        [Category("Data"), Description("The first data slot. Data slots are used to provide additional information to the AbilityCast event.")]
        public string Data1 {
            get { return Data[0]; }
            set { Data[0] = value; }
        }

        /// <summary>
        /// This Ability's second data slot
        /// </summary>
        [Category("Data"), Description("The second data slot. Data slots are used to provide additional information to the AbilityCast event.")]
        public string Data2 {
            get { return Data[1]; }
            set { Data[1] = value; }
        }

        /// <summary>
        /// This Ability's third data slot
        /// </summary>
        [Category("Data"), Description("The third data slot. Data slots are used to provide additional information to the AbilityCast event.")]
        public string Data3 {
            get { return Data[2]; }
            set { Data[2] = value; }
        }

        /// <summary>
        /// This Ability's fourth data slot
        /// </summary>
        [Category("Data"), Description("The fourth data slot. Data slots are used to provide additional information to the AbilityCast event.")]
        public string Data4 {
            get { return Data[3]; }
            set { Data[3] = value; }
        }


        private string _IconPath, _Name, _Desc;
        private string[] _AllowedTarget;

        // The following properties are only used by AbilityEd

        /// <summary>
        /// The name of the Ability
        /// </summary>
        [Category("Text"), Description("This Ability's name.")]
        public string Name {
            get { return _Name; }
            set { _Name = value; }
        }

        /// <summary>
        /// The description on the Ability's tooltip. This is only used in AbilityEd.
        /// </summary>
        [Category("Text"), Description("The description on this Ability's tooltip.")]
        public string Desc {
            get { return _Desc; }
            set { _Desc = value; }
        }

        /// <summary>
        /// The path to this Ability's icon. This is used only by AbilityEd.
        /// </summary>
        [Category("Art"), Description("The path to this Ability's icon.")]
        public string IconPath {
            get { return _IconPath; }
            set { _IconPath = value; }
        }

        /// <summary>
        /// The allowed targets of this Ability. This is used only by AbilityEd.
        /// </summary>
        [Category("Stats"), Description("The allowed targets of this Ability.")]
        public string[] AllowedTarget {
            get { return _AllowedTarget; }
            set { _AllowedTarget = value; }
        }
        #endregion

        /// <summary>
        /// Ability specific data to give extra functionality to the AbilityCast.
        /// </summary>
        public string[] Data = new string[4];

        /// <summary>
        /// The targets on whom this Ability is effective.
        /// </summary>
        public Target Target;


        /// <summary>
        /// Creates a blank Ability
        /// </summary>
        public Ability() {
        }

        /// <summary>
        /// Loads an Ability from abilities.slk
        /// </summary>
        /// <param name="aid">The unique ID of this Ability</param>
        public Ability(string aid) {
            Aid = aid;
            Code = GameData.GetAbilityData<string>(aid, "code");

            CastTime = GameData.GetAbilityData<float>(aid, "cast");
            Duration = GameData.GetAbilityData<float>(aid, "duration");
            Cooldown = GameData.GetAbilityData<float>(aid, "cooldown");
            Energy = GameData.GetAbilityData<int>(aid, "energy");
            Area = GameData.GetAbilityData<float>(aid, "aoe");
            CastRange = GameData.GetAbilityData<float>(aid, "range");

            AllowedTarget = (GameData.GetAbilityData<string>(aid, "target") != null
                ? GameData.GetAbilityData<string>(aid, "target").Split(',')
                : new string[0]);
            Target = new Target(GameData.GetAbilityData<string>(aid, "target") != null
                ? GameData.GetAbilityData<string>(aid, "target")
                : "");

            for (int i = 0; i < 4; i++)
                Data[i] = GameData.GetAbilityData<string>(aid, "data" + (i + 1).ToString());

            Texture2D icon = null;
            IconPath = GameData.GetAbilityData<string>(aid, "icon");

            // If it does not have an icon, we'll provide one
            if (String.IsNullOrEmpty(IconPath))
                IconPath = "jamie";

            if (League.Engine != null)
                icon = League.Engine.Content.Load<Texture2D>("icons/" + IconPath);

            IconX = GameData.GetAbilityData<int>(aid, "x");
            IconY = GameData.GetAbilityData<int>(aid, "y");

            Name = GameData.GetAbilityData<string>(aid, "comment");
            Desc = GameData.GetAbilityData<string>(aid, "desc");

            if (League.Engine != null) {
                Button = new ContextButton(icon,
                    Name, Desc, null,
                    IconX, IconY, AbilityButton);
                Button.Tag = aid;
            }
        }

        /// <summary>
        /// Performs the logic necessary to cast the Ability.
        /// </summary>
        /// <param name="caster">The unit casting.</param>
        /// <param name="target">The target of the Ability - either null, Unit or Vector2</param>
        public void Invoke(Unit caster, object target) {
            if (caster.Energy >= Energy && !caster.AbilityCooldown.ContainsKey(Aid)) {
                if (Target.Type != TargetType.Point && Target.Type != TargetType.None) {
                    if (!Target.CompareUnit(caster, (Unit)target)) {
                        return;
                    }
                }

                caster.CastingAbility = null;
                caster.Path = null;
                caster.State &= ~(UnitState.Casting | UnitState.Moving);

                // If InvokeAbility() returns true the ability was successful
                if (InvokeAbility(Code, Aid, caster, target)) {
                    caster.Energy -= Energy;
                    caster.CastTime = null;
                    caster.AbilityCooldown.Add(Aid, Cooldown);
                }
            }
        }

        /// <summary>
        /// Hassleless way of getting the ContextButton
        /// </summary>
        /// <param name="ability">The Ability being cast</param>
        /// <returns>The Ability's ContextButton</returns>
        public static implicit operator ContextButton(Ability ability) {
            ContextButton button = ability.Button;
            button.TypeOf = typeof(Ability);
            return button;
        }

        /// <summary>
        /// Loads, caches and returns an Ability from abilities.slk
        /// </summary>
        /// <param name="aid">The unique identifier of the desired Ability</param>
        /// <returns>The Ability</returns>
        public static Ability GetAbility(string aid) {
            Ability ability = (Abilities.ContainsKey(aid) ? Abilities[aid] : new Ability(aid));
            if (!Abilities.ContainsKey(aid))
                Abilities.Add(aid, ability);
            return ability;
        }

        /// <summary>
        /// Used by AbilityEd to create a new Ability
        /// </summary>
        /// <param name="aid">The new Aid</param>
        /// <param name="baseaid">The Aid of the derrived Ability</param>
        /// <returns>The new Ability</returns>
        public static Ability NewAbility(string aid, string baseaid) {
            Ability ability = (Abilities.ContainsKey(baseaid) ? Abilities[baseaid] : new Ability(baseaid));
            if (!Abilities.ContainsKey(baseaid))
                Abilities.Add(baseaid, ability);
            Ability newability = (Ability)ability.MemberwiseClone();
            newability.Aid = aid;
            if (!Abilities.ContainsKey(aid))
                Abilities.Add(aid, newability);
            return newability;
        }

        /// <summary>
        /// A cache of all loaded Abilities
        /// </summary>
        public static Dictionary<string, Ability> Abilities = new Dictionary<string, Ability>(128);

        /// <summary>
        /// A delegate for handling Ability related button presses
        /// </summary>
        private static ContextButton.ButtonAction AbilityButton = new ContextButton.ButtonAction(Action_Button);

        /// <summary>
        /// Called when an Ability's ContextButton is clicked
        /// </summary>
        /// <param name="name">The text on the button</param>
        /// <param name="tag">The tag of the button - the Aid in our case</param>
        private static void Action_Button(string name, object tag) {
            Unit u = Player.CurrentPlayer.Selected[0];
            Ability ability = Ability.GetAbility((string)tag);

            if (u.Energy < ability.Energy) return;

            if (ability.Target.Type == TargetType.Point) {
                League.Engine.OnClick = new OnClickHandler(delegate(Point screen, Vector3 world, object ttag) {
                    u.OrderCast(ability, new Vector2(world.X, world.Z));
                    return true;
                });
            } else if (ability.Target.Type != TargetType.None) {
                // As of now, if it's not a point and it's not none, it's a unit

                League.Engine.OnSelect = new OnSelectHandler(delegate(Unit target, object ttag) {
                    u.OrderCast(ability, target);

                    // Don't accept the selection change
                    return false;
                });
            } else {
                u.OrderCast(ability);
            }
        }

        /// <summary>
        /// Delegates Ability casts to the scripting engine
        /// </summary>
        /// <param name="code">The specific ability to invoke</param>
        /// <param name="aid">The unique identifier of the Ability</param>
        /// <param name="caster">The caster</param>
        /// <param name="target">The target of the Ability</param>
        /// <returns>Success</returns>
        public static bool InvokeAbility(string code, string aid, Unit caster, object target) {
            object o = League.Engine.Script.InvokeSyncEvent("AbilityCast", code, Ability.GetAbility(aid), caster, target);
            if (o == null)
                return false;
            return (bool)o;
        }

        /// <summary>
        /// Delegates smart events to the scripting engine
        /// </summary>
        /// <param name="code">The specific ability to invoke</param>
        /// <param name="aid">The unique identifier of the Ability</param>
        /// <param name="caster">The caster</param>
        /// <param name="target">The target of the Ability</param>
        /// <returns>Whether the smart handled this</returns>
        public static bool InvokeSmart(string code, string aid, Unit caster, object target) {
            object o = League.Engine.Script.InvokeSyncEvent("AbilitySmart", code, Ability.GetAbility(aid), caster, target);
            if (o == null)
                return false;
            return (bool)o;
        }
    }
}
