using System;
using System.Collections.Generic;
using System.Text;
using TargetTeam = LeagueEngine.Team;

namespace LeagueEngine {
    /// <summary>
    /// Describes the valid target of an action
    /// </summary>
    public class Target {
        /// <summary>
        /// The type of what this action may target - None, Self, Unit, Doodad, Point
        /// </summary>
        public TargetType Type = TargetType.None;

        /// <summary>
        /// The team of which this action may target - see the Team enum for values
        /// </summary>
        public TargetTeam Team = TargetTeam.Any;

        /// <summary>
        /// The classification of the unit which may be targeted
        /// </summary>
        public TargetClassification ClassificData = new TargetClassification();

        /// <summary>
        /// The specific unit type which may be targeted
        /// </summary>
        public string Specific = null;

        /// <summary>
        /// Parses a target string into a Target
        /// </summary>
        /// <param name="data">The target string. Should be a comma deliminated string of valid targets.</param>
        public Target(string data) {
            string[] parsed = data.Split(',');
            List<string> specifics = new List<string>();

            foreach (string target in parsed) {
                switch (target) {
                    case "terrain":
                        Type = TargetType.Point;
                        break;
                    case "point":
                        Type = TargetType.Point;
                        break;
                    /*case "doodad":
                        Type = TargetType.Doodad;
                        break;*/
                    case "self":
                        Type = TargetType.None;
                        Team = Team.Self;
                        break;
                    case "owner":
                        Type = TargetType.Unit;
                        Team = Team.Owner;
                        break;
                    case "allies":
                        Type = TargetType.Unit;
                        Team = Team.Allies;
                        break;
                    case "friendly":
                        Type = TargetType.Unit;
                        Team = Team.Friendly;
                        break;
                    case "enemy":
                        Type = TargetType.Unit;
                        Team = Team.Enemy;
                        break;
                    case "neutral":
                        Type = TargetType.Unit;
                        Team = Team.Neutral;
                        break;
                    case "structure":
                        ClassificData.Add(Classification.Structure, ClassificationState.Yes);
                        break;
                    case "unit":
                        ClassificData.Add(Classification.Structure, ClassificationState.No);
                        break;
                    case "ground":
                        ClassificData.Add(Classification.Air, ClassificationState.No);
                        break;
                    case "air":
                        ClassificData.Add(Classification.Air, ClassificationState.Yes);
                        break;
                    case "organic":
                        ClassificData.Add(Classification.Biological, ClassificationState.Yes);
                        break;
                    case "biological":
                        ClassificData.Add(Classification.Biological, ClassificationState.Yes);
                        break;
                    case "mechanical":
                        ClassificData.Add(Classification.Biological, ClassificationState.No);
                        break;
                    case "corporeal":
                        ClassificData.Add(Classification.Corporeal, ClassificationState.Yes);
                        break;
                    case "ethereal":
                        ClassificData.Add(Classification.Corporeal, ClassificationState.No);
                        break;
                    case "worker":
                        ClassificData.Add(Classification.Worker, ClassificationState.Yes);
                        break;
                    case "!worker":
                        ClassificData.Add(Classification.Worker, ClassificationState.No);
                        break;
                    case "townhall":
                        ClassificData.Add(Classification.TownHall, ClassificationState.Yes);
                        break;
                    case "!townhall":
                        ClassificData.Add(Classification.TownHall, ClassificationState.No);
                        break;
                    case "melee":
                        ClassificData.Add(Classification.Melee, ClassificationState.Yes);
                        break;
                    case "ranged":
                        ClassificData.Add(Classification.Melee, ClassificationState.No);
                        break;
                    case "attacks":
                        ClassificData.Add(Classification.Attacks, ClassificationState.Yes);
                        break;
                    case "!attacks":
                        ClassificData.Add(Classification.Attacks, ClassificationState.No);
                        break;
                    case "attacksground":
                        ClassificData.Add(Classification.AttacksGround, ClassificationState.Yes);
                        break;
                    case "!attacksground":
                        ClassificData.Add(Classification.AttacksGround, ClassificationState.No);
                        break;
                    case "attacksair":
                        ClassificData.Add(Classification.AttacksAir, ClassificationState.Yes);
                        break;
                    case "!attacksair":
                        ClassificData.Add(Classification.AttacksAir, ClassificationState.No);
                        break;
                    case "summoned":
                        ClassificData.Add(Classification.Summoned, ClassificationState.Yes);
                        break;
                    case "!summoned":
                        ClassificData.Add(Classification.Summoned, ClassificationState.No);
                        break;
                    case "resource":
                        ClassificData.Add(Classification.Resource, ClassificationState.Yes);
                        break;
                    default:
                        // Get a specific uid
                        if (target != "") {
                            try {
                                UnitType.GetUnitType(target);
                                specifics.Add(target);
                                Type = TargetType.Specific;
                            } catch {
                                //throw new Exception("Unknown target `" + target + "`");
                            }
                        }
                        break;
                }
            }
            if (specifics.Count > 0)
                Specific = String.Join(",", specifics.ToArray());
        }

        /// <summary>
        /// Compares Unit b against the Target relative to Unit a
        /// </summary>
        /// <param name="a">The caster of the action</param>
        /// <param name="b">The target of the action</param>
        /// <returns>Whether the target matches Unit b</returns>
        public bool CompareUnit(Unit a, Unit b) {
            foreach (KeyValuePair<Classification, ClassificationState> pair in ClassificData.Data)
                // Make sure b matches every thing he should and nothing he shouldn't
                if (pair.Value == ClassificationState.Yes && (b.Type.Classification & pair.Key) != pair.Key)
                    return false;
                else if (pair.Value == ClassificationState.No && (b.Type.Classification & pair.Key) == pair.Key)
                    return false;

            // Check if B's Uid is the specific
            if (!String.IsNullOrEmpty(Specific))
                if (!new List<string>(Specific.Split(',')).Contains(b.Type.Uid))
                    return false;

            // Check B's team relative to A
            if (Team == Team.Self && a != b)
                return false;
            else if ((Team & a.Owner.GetUnitTeam(b)) == Team.None)
                return false;

            return true;
        }

        /// <summary>
        /// Gets the Classification from a target string
        /// </summary>
        /// <param name="data">A target string</param>
        /// <returns>The Classification of the target string</returns>
        public static Classification GetClassification(string data) {
            Target t = new Target(data);
            Classification clas = new Classification();
            foreach (KeyValuePair<Classification, ClassificationState> pair in t.ClassificData.Data) {
                if (pair.Value == ClassificationState.Yes)
                    clas |= pair.Key;
            }

            return clas;
        }
    }


    /// <summary>
    /// Possible classifications for a unit
    /// </summary>
    public enum Classification {
        None = 0,
        Structure = 1,
        Air = 2,
        Biological = 4,
        Corporeal = 8,
        Worker = 16,
        TownHall = 32,
        Melee = 64,
        Attacks = 128,
        AttacksGround = 256,
        AttacksAir = 512,
        Summoned = 1024,
        Resource = 2048,
        Any = 4095,
    }

    /// <summary>
    /// The possible targets of an action
    /// </summary>
    public enum TargetType {
        None = 0,
        Point = 1,
        Unit = 2,
        Doodad = 4,
        Specific = 8
    }

    /// <summary>
    /// The possible owners of a Unit
    /// </summary>
    public enum Team {
        None = 0,
        Self = 1,
        Owner = 2,
        Allies = 4,
        Friendly = 6,
        Enemy = 8,
        Neutral = 16,
        Any = 31
    }
}
