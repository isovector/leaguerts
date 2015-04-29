using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;

namespace LeagueEd {
    public partial class ScriptEd : Form {
        public string CurrentFile = "";
        public Stack<UndoData> Undos = new Stack<UndoData>();
        public Stack<UndoData> Redos = new Stack<UndoData>();
        public bool Saved = true;

        public ScriptEd(string[] args) {
            InitializeComponent();

            if (args.Length != 0)
                OpenLFile(args[0]);
            else
                newToolStripMenuItem_Click(null, null);
        }

        private void openScriptToolStripMenuItem_Click(object sender, EventArgs e) {
            if (OpenFile.ShowDialog() == DialogResult.OK) {
                OpenLFile(OpenFile.FileName);
                Status.Text = "Opened file `" + CurrentFile + "` successfully.";
            }
        }

        public void OpenLFile(string file) {
            CurrentFile = file;
            TextBox.Text = File.ReadAllText(file);
            RefreshHighlight();
            Saved = true;
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e) {
            MessageBox.Show("v 1.0.0.0\r\nWritten by Alex Maguire.", "About LeagueEd - Script Editor");
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e) {
            TextBox.Text = "";
            CurrentFile = "";
            Undos.Push(new UndoData("", 0));
            Status.Text = "New file created";
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e) {
            if (CurrentFile != "") {
                File.WriteAllText(CurrentFile, TextBox.Text);
                Saved = true;
                Status.Text = "Saved file sucessfully.";
            } else
                saveAsToolStripMenuItem_Click(sender, e);
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e) {
            if (SaveFile.ShowDialog() == DialogResult.OK) {
                CurrentFile = SaveFile.FileName;
                File.WriteAllText(CurrentFile, TextBox.Text);
                Saved = true;
                Status.Text = "Saved file `" + CurrentFile + "` successfully.";
            }
        }

        private void TextBox_KeyUp(object sender, KeyEventArgs e) {
            int pos = TextBox.SelectionStart;
            if (e.KeyCode == Keys.Tab) {
                /*int start = TextBox.GetFirstCharIndexOfCurrentLine();
                if (TextBox.Text.Length > 5 && TextBox.Text.Substring(start, 5) == "cast:") {
                    string aid = TextBox.Text.Substring(start + 5, 4);
                    TextBox.Text = TextBox.Text.Remove(start, 9);
                    TextBox.Text = TextBox.Text.Substring(0, pos - 9) +
                        "on AbilityCast(\"" + aid + "\"): bool Common_AbilityCast_" + aid + "(Ability ability, Unit caster, object target) {" +
                        "\r\n    \r\n    return true;\r\n}" +
                        TextBox.Text.Substring(pos - 9);
                    TextBox.SelectionStart = pos - 9 + 104;
                    RefreshHighlight();
                    Status.Text = "Generated AbilityCast stub for `" + aid + "`.";
                } else if (TextBox.Text.Length > 6 && TextBox.Text.Substring(start, 6) == "smart:") {
                    string aid = TextBox.Text.Substring(start + 6, 4);
                    TextBox.Text = TextBox.Text.Remove(start, 10);
                    TextBox.Text = TextBox.Text.Substring(0, pos - 10) +
                        "on AbilitySmart(\"" + aid + "\"): bool Common_AbilitySmart_" + aid + "(Ability ability, Unit caster, object target) {" +
                        "\r\n    \r\n    return true;\r\n}" +
                        TextBox.Text.Substring(pos - 10);
                    TextBox.SelectionStart = pos - 10 + 106;
                    RefreshHighlight();
                    Status.Text = "Generated AbilitySmart stub for `" + aid + "`.";
                } else if (TextBox.Text.Length > 9 && TextBox.Text.Substring(start, 8) == "partsys:") {
                    int end = TextBox.Text.IndexOf("\n", start + 8);
                    if (end == -1) end = TextBox.Text.Length;
                    string aid = TextBox.Text.Substring(start + 8, end - (start + 8));
                    TextBox.Text = TextBox.Text.Remove(start, 8 + aid.Length);
                    TextBox.Text = TextBox.Text.Substring(0, pos - (8 + aid.Length)) +
                        "on ParticleSystem(\"" + aid + "\"): void Common_ParticleSystem_" + aid.Replace(" ", "_") + "(ParticleSettings settings) {" +
                        "\r\n    \r\n}" +
                        TextBox.Text.Substring(pos - (8 + aid.Length));
                    TextBox.SelectionStart = pos - 8 + 84 + aid.Length;
                    RefreshHighlight();
                    Status.Text = "Generated ParticleSystem stub for `" + aid + "`.";
                } else {*/
                    TextBox.Text = TextBox.Text.Insert(pos, "    ");
                    TextBox.SelectionStart = pos + 4;
                    e.Handled = true;
                    RefreshHighlight();
                //}
            } else if (e.KeyCode == Keys.Space) {
                RefreshHighlight();
            } else if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Return) {
                string line = TextBox.Lines[TextBox.GetLineFromCharIndex(pos) - 1];

                string tab = "";
                char last = '\x00';
                foreach (char c in line)
                    if (Char.IsWhiteSpace(c))
                        tab += " ";
                    else {
                        //last = c;
                        break;
                    }


                if (last == '}') {
                    if (pos < 6)
                        return;

                    if (TextBox.Text[TextBox.GetFirstCharIndexFromLine(TextBox.GetLineFromCharIndex(pos) - 1)] != '}') {
                        TextBox.Text = TextBox.Text.Remove(pos - 6, 4);
                        TextBox.SelectionStart = pos - 4;

                        if (tab.Length >= 4)
                            tab = tab.Remove(0, 4);
                    }
                }// else if (pos - 2 > 0 && TextBox.Text[pos - 2] == '{')
                    //tab += "    ";

                TextBox.Text = TextBox.Text.Substring(0, pos) + tab + TextBox.Text.Substring(pos);
                TextBox.SelectionStart = pos + tab.Length;

                //Status.Text = "Autotab: " + tab.Length;

                e.Handled = true;
                RefreshHighlight();
            } else if (e.KeyCode == Keys.Z && e.Control) {
                if (e.Shift)
                    redoToolStripMenuItem_Click(null, null);
                else
                    undoToolStripMenuItem_Click(null, null);
                e.Handled = true;
            } else if (e.KeyCode == Keys.S && e.Control)
                saveToolStripMenuItem_Click(null, null);
            else if (e.KeyCode == Keys.Delete || e.KeyCode == Keys.Back)
                RefreshHighlight();
        }

        private void RefreshHighlight() {
            Undos.Push(new UndoData(TextBox.Text, TextBox.SelectionStart));
            Redos.Clear();
            TextBox.RefreshHighlight();
            Saved = false;
        }

        private void TextBox_KeyPress(object sender, KeyPressEventArgs e) {
            int pos = TextBox.SelectionStart;
            if (TextBox.Text.Length > pos + 1) {
                if (Char.IsLetterOrDigit(e.KeyChar) && Char.IsLetter(TextBox.Text[pos + 1])) {
                    if (TextBox.SelectionLength != 0) {
                        TextBox.Text = TextBox.Text.Remove(TextBox.SelectionStart, TextBox.SelectionLength);
                        TextBox.SelectionStart = pos;
                    }
                    RefreshHighlight();
                }
            }

            if (Char.IsLetterOrDigit(e.KeyChar) || Char.IsSymbol(e.KeyChar) || Char.IsWhiteSpace(e.KeyChar) || Char.IsPunctuation(e.KeyChar))
                Saved = false;
        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e) {
            if (Undos.Count > 0) {
                UndoData undo = Undos.Pop();
                undo.Undo(TextBox);
                Redos.Push(undo);
                TextBox.RefreshHighlight();
                Status.Text = "Undo";
            }
        }

        private void redoToolStripMenuItem_Click(object sender, EventArgs e) {
            if (Redos.Count > 0) {
                UndoData redo = Redos.Pop();
                redo.Undo(TextBox);
                Undos.Push(redo);
                TextBox.RefreshHighlight();
                Status.Text = "Redo";
            }
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e) {
            if (TextBox.SelectionLength != 0) {
                int pos = TextBox.SelectionStart;
                TextBox.Text = TextBox.Text.Remove(TextBox.SelectionStart, TextBox.SelectionLength);
                TextBox.SelectionStart = pos;
                RefreshHighlight();
            }
        }

        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e) {
            TextBox.SelectionStart = 0;
            TextBox.SelectionLength = TextBox.Text.Length;
        }

        private void ScriptEd_FormClosing(object sender, FormClosingEventArgs e) {
            if (e.CloseReason != CloseReason.WindowsShutDown && e.CloseReason != CloseReason.TaskManagerClosing) {
                if (!Saved) {
                    DialogResult dr = (new ConfirmClose()).ShowDialog();

                    if (dr == DialogResult.Yes)
                        saveToolStripMenuItem_Click(null, null);
                    else if (dr == DialogResult.Cancel)
                        e.Cancel = true;
                }
            }
        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e) {
            Close();
        }
    }
}