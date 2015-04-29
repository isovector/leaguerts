using System;
using System.Collections.Generic;
using System.Text;

namespace LeagueEngine {
    /// <summary>
    /// Describes what state a unit is in.
    /// Probably isn't really necessary anymore.
    /// </summary>
    public enum UnitState {
        None = 0, Moving, Guarding, Patrolling, Attacking, Building = 8, Casting = 16
    }
}
