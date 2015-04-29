using System;
using System.Collections.Generic;
using System.Text;

namespace LeagueEngine {
    /// <summary>
    /// Describes a group of Units
    /// </summary>
    public class UnitGroup : List<Unit> {
        /// <summary>
        /// Creates an empty group of units
        /// </summary>
        public UnitGroup() : base() {}

        /// <summary>
        /// Creats a group of units from the provided units
        /// </summary>
        /// <param name="units">The units to put in the group</param>
        public UnitGroup(IEnumerable<Unit> units) : base(units) {}

        /// <summary>
        /// Creates a group of units with an initial size
        /// </summary>
        /// <param name="size">The initial size of the group to make</param>
        public UnitGroup(int size) : base(size) {}

        /// <summary>
        /// Determines whether one unit group contains the same units as another
        /// </summary>
        /// <param name="obj">The object to test</param>
        /// <returns>Whether they contain the same units</returns>
        public override bool Equals(object obj) {
            UnitGroup b = obj as UnitGroup;
            if (b != null)
                // We compare the string representation of them to tell
                return ToString() == b.ToString();
            return false;
        }

        public override int GetHashCode() {
            return ToString().GetHashCode();
        }

        /// <summary>
        /// Gets a string representation of this group
        /// </summary>
        /// <returns>A comma deliminated string of uids and their owner from this group</returns>
        public override string ToString() {
            if (Count != 0) {
                string data = this[0].Type.Uid;
                for (int i = 1; i < Count; i++)
                    data += "," + this[i].Type.Uid + this[i].Owner.ToString();
                return data;
            }
            return "";
        }

        public static bool operator ==(UnitGroup a, UnitGroup b) {return a.ToString() == b.ToString();}
        public static bool operator !=(UnitGroup a, UnitGroup b) { return a.ToString() != b.ToString(); }
    }
}
