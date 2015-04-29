using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace LeagueEngine {
    /// <summary>
    /// Provides a common infrastructure for reading SLK data.
    /// </summary>
    public static class GameData {
        public static SlkReader UnitData, AbilityData;

        static GameData() {
            if (File.Exists(System.Windows.Forms.Application.StartupPath + "/Content/units.lss"))
                UnitData = new SlkReader(System.Windows.Forms.Application.StartupPath + "/Content/units.lss");
            if (File.Exists(System.Windows.Forms.Application.StartupPath + "/Content/abilities.la"))
                AbilityData = new SlkReader(System.Windows.Forms.Application.StartupPath + "/Content/abilities.la");
        }

        /// <summary>
        /// Gets a cell from UnitData
        /// </summary>
        /// <typeparam name="T">The cell's data type</typeparam>
        /// <param name="row">The row</param>
        /// <param name="col">The column</param>
        /// <returns>The requested cell</returns>
        public static T GetUnitData<T>(string row, string col) {
            return GetData<T>(UnitData, row, col);
        }

        /// <summary>
        /// Gets a cell from AbilityData
        /// </summary>
        /// <typeparam name="T">The cell's data type</typeparam>
        /// <param name="row">The row</param>
        /// <param name="col">The column</param>
        /// <returns>The requested cell</returns>
        public static T GetAbilityData<T>(string row, string col) {
            return GetData<T>(AbilityData, row, col);
        }

        /// <summary>
        /// Gets a cell from either data
        /// </summary>
        /// <typeparam name="T">The cell's data type</typeparam>
        /// <param name="type">The name of the data source. Must be either unit or ability.</param>
        /// <param name="row">The row</param>
        /// <param name="col">The column</param>
        /// <returns>The requested cell</returns>
        public static T GetData<T>(string type, string row, string col) {
            if (type == "unit")
                return GetData<T>(UnitData, row, col);
            else if (type == "ability")
                return GetData<T>(AbilityData, row, col);
            throw new Exception("Invalid data type for GetData");
        }

        /// <summary>
        /// Gets a cell from either data
        /// </summary>
        /// <typeparam name="T">The cell's data type</typeparam>
        /// <param name="type">The data source. Must be a valid SlkReader</param>
        /// <param name="row">The row</param>
        /// <param name="col">The column</param>
        /// <returns>The requested cell</returns>
        public static T GetData<T>(SlkReader dat, string row, string col) {
            if (typeof(T) == typeof(bool))
                return (T)Convert.ChangeType(dat.GetBoolCell(row, col), typeof(T));
            return (T)Convert.ChangeType(dat.GetStringCell(row, col), typeof(T));
        }
    }
}
