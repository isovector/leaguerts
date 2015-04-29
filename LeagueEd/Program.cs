using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace LeagueEd {
    static class Program {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args) {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            if (args.Length == 0 || (!args[0].EndsWith(".l") && !args[0].EndsWith(".lss") && !args[0].EndsWith(".la"))) {
                EditorSelection es = new EditorSelection();
                if (es.ShowDialog() == DialogResult.OK) {
                    if (es.comboBox1.SelectedItem.ToString() == "ScriptEd")
                        Application.Run(new ScriptEd(new string[] { }));
                    else if (es.comboBox1.SelectedItem.ToString() == "UnitEd")
                        Application.Run(new UnitEd(new string[] { }));
                    else if (es.comboBox1.SelectedItem.ToString() == "AbilityEd")
                        Application.Run(new AbilityEd(new string[] { }));
                }
            } else {
                if (args[0].EndsWith(".l"))
                    Application.Run(new ScriptEd(args));
                else if (args[0].EndsWith(".lss"))
                    Application.Run(new UnitEd(args));
                else if (args[0].EndsWith(".la"))
                    Application.Run(new AbilityEd(args));
            }
        }
    }
}