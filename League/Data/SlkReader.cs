using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace LeagueEngine {
    /// <summary>
    /// Loads SLK (and LSS as it's now called) data. Functionality for reading this is in
    /// the GameData class.
    /// </summary>
    public class SlkReader {
        /// <summary>
        /// A dictionary of row:col = data
        /// </summary>
        public Dictionary<string, Dictionary<string, string>> Data = new Dictionary<string, Dictionary<string, string>>();

        /// <summary>
        /// The spreadsheet representation of the file
        /// </summary>
        public string[,] Table;

        /// <summary>
        /// Creates a SlkReader from a Stream
        /// </summary>
        /// <param name="data">The stream to read</param>
        public SlkReader(Stream data) {
            StreamReader reader = new StreamReader(data);
            List<string> lines = new List<string>();
            while (!reader.EndOfStream)
                lines.Add(reader.ReadLine());
            SetupTable(lines.ToArray());
        }

        /// <summary>
        /// Creates a SlkReader from a file
        /// </summary>
        /// <param name="file">The file to be read</param>
        public SlkReader(string file) {
            string[] lines = File.ReadAllLines(file);
            SetupTable(lines);
        }

        /// <summary>
        /// Constructs the data from the lines in the file
        /// </summary>
        /// <param name="lines">The lines of SLK data</param>
        private void SetupTable(string[] lines) {
            int xmax = 0;
            int ymax = 0;

            Regex reg = new Regex("^C;X(?<x>[0-9]+);Y(?<y>[0-9]+);K(?<value>.*)$");
            List<Match> matches = new List<Match>();
            foreach (string line in lines) {
                Match m = reg.Match(line);
                if (m.Success) {
                    matches.Add(m);
                    int localx = int.Parse(m.Groups["x"].Value);
                    int localy = int.Parse(m.Groups["y"].Value);

                    
                    if (localx > xmax) xmax = localx;
                    if (localy > ymax) ymax = localy;
                }
            }
            Table = new string[xmax, ymax];

            // Slk stores the first cell as X1;Y1
            foreach (Match match in matches)
                if (match.Groups["value"].Value.StartsWith("\""))
                    Table[int.Parse(match.Groups["x"].Value) - 1, int.Parse(match.Groups["y"].Value) - 1] = match.Groups["value"].Value.Substring(1, match.Groups["value"].Value.Length - 2);
                else
                    Table[int.Parse(match.Groups["x"].Value) - 1, int.Parse(match.Groups["y"].Value) - 1] = match.Groups["value"].Value;

            for (int y = 1; y < ymax; y++) {
                Dictionary<string, string> row = new Dictionary<string,string>();
                for (int x = 0; x < xmax; x++)
                    if (Table[x, 0] != null)
                        row.Add(Table[x, 0], Table[x, y] == null ? "" : Table[x, y]);
                if (!Data.ContainsKey(Table[0, y]))
                    Data.Add(Table[0, y], row);
            }
        }

        /// <summary>
        /// Returns an entire row of SLK data
        /// </summary>
        /// <param name="row">The row to get (value in the first column)</param>
        /// <returns>A col = data dictionary of data</returns>
        public Dictionary<string, string> GetRow(string row) {
            return Data[row];
        }

        public int GetIntCell(string row, string col) {
            return int.Parse(Data[row][col]);
        }

        public float GetRealCell(string row, string col) {
            return float.Parse(Data[row][col]);
        }

        public bool GetBoolCell(string row, string col) {
            return int.Parse(Data[row][col]) != 0;
        }

        public string GetStringCell(string row, string col) {
            return Data[row][col];
        }
    }
}
