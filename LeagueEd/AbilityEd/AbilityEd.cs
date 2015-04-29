using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using LeagueEngine;
using System.IO;

namespace LeagueEd {
    public partial class AbilityEd : Form {
        public bool Saved = true;
        public string CurrentFile = "";
        StringBuilder sb;
        List<string> cols;

        public AbilityEd(string[] args) {
            InitializeComponent();

            if (args.Length != 0) {
                Open(args[0]);
            } else {
                Open(null);
            }
        }

        public void Open(string file) {
            AidList.Items.Clear();
            Ability.Abilities.Clear();

            if (file != null) {
                GameData.AbilityData = new SlkReader(file);
                CurrentFile = file;
            }

            foreach (KeyValuePair<string, Dictionary<string, string>> pair in GameData.AbilityData.Data) {
                AidList.Items.Add(pair.Key);
                Ability.GetAbility(pair.Key);
            }
            AidList.SelectedIndex = 0;
            Sort();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e) {
            if (AidList.SelectedIndices.Count == 1) {
                Properties.SelectedObject = Ability.GetAbility(AidList.SelectedItem.ToString());
                Status.Text = "Editing `" + Ability.GetAbility(AidList.SelectedItem.ToString()).Name + " (" + AidList.SelectedItem.ToString() + ")`.";
            } else {
                Properties.SelectedObject = null;
            }
        }

        private void newUnitToolStripMenuItem_Click(object sender, EventArgs e) {
            MessagePrompt mp = new MessagePrompt("Enter new Aid:");

            if (mp.ShowDialog() == DialogResult.OK) {
                string aid = mp.textBox1.Text;
                Ability.NewAbility(aid, AidList.SelectedItem.ToString());
                AidList.Items.Add(aid);
                Saved = false;

                Sort();

                Status.Text = "Derrived new Ability `" + aid + "`.";
            }
        }

        private void createNewUnitToolStripMenuItem_Click(object sender, EventArgs e) {
            newUnitToolStripMenuItem_Click(null, null);
        }

        private void newUnitToolStripMenuItem1_Click(object sender, EventArgs e) {
            MessagePrompt mp = new MessagePrompt("Enter new Aid:");

            if (mp.ShowDialog() == DialogResult.OK) {
                string aid = mp.textBox1.Text;
                Ability a = new Ability();
                a.Aid = aid;
                Ability.Abilities.Add(aid, a);
                AidList.Items.Add(aid);
                Saved = false;

                Sort();

                Status.Text = "Created new Ability `" + aid + "`.";
            }
        }

        private void createNewUnitToolStripMenuItem1_Click(object sender, EventArgs e) {
            newUnitToolStripMenuItem1_Click(null, null);
        }

        private void Properties_PropertyValueChanged(object s, PropertyValueChangedEventArgs e) {
            Saved = false;

            if (e.ChangedItem.Label == "Name") {
                Sort();
            }
        }

        private void UnitEd_FormClosing(object sender, FormClosingEventArgs e) {
            if (e.CloseReason != CloseReason.WindowsShutDown && e.CloseReason != CloseReason.TaskManagerClosing) {
                if (!Saved) {
                    DialogResult dr = (new ConfirmClose()).ShowDialog();

                    if (dr == DialogResult.Yes)
                        Save();
                    else if (dr == DialogResult.Cancel)
                        e.Cancel = true;
                }
            }
        }

        public void SaveAs() {
            if (SaveFile.ShowDialog() == DialogResult.OK) {
                CurrentFile = SaveFile.FileName;
                Save();
            }
        }

        public void Save() {
            if (CurrentFile == "") {
                SaveAs();
                return;
            }

            sb = new StringBuilder();
            cols = new List<string>();
            for (int i = 0; i < GameData.AbilityData.Table.GetLength(0); i++)
                cols.Add(GameData.AbilityData.Table[i, 0]);

            sb.AppendLine("ID;AbilityEd100");

            foreach (string col in cols)
                WriteData(1, col, col);

            int row = 2;
            foreach (KeyValuePair<string, Ability> pair in Ability.Abilities) {
                Ability a = pair.Value;

                WriteData(row, "aid", a.Aid);
                WriteData(row, "code", a.Code);
                WriteData(row, "comment", a.Name);
                WriteData(row, "desc", a.Desc);
                WriteData(row, "cast", a.CastTime);
                WriteData(row, "duration", a.Duration);
                WriteData(row, "cooldown", a.Cooldown);
                WriteData(row, "energy", a.Energy);
                WriteData(row, "aoe", a.Area);
                WriteData(row, "range", a.CastRange);
                WriteData(row, "target", a.AllowedTarget);
                WriteData(row, "data1", a.Data1);
                WriteData(row, "data2", a.Data2);
                WriteData(row, "data3", a.Data3);
                WriteData(row, "data4", a.Data4);
                WriteData(row, "icon", a.IconPath);
                WriteData(row, "x", a.IconX); 
                WriteData(row, "y", a.IconY);

                row++;
            }

            sb.AppendLine("E");

            File.WriteAllText(CurrentFile, sb.ToString());
            Saved = true;

            Status.Text = "File `" + CurrentFile + "` was saved successfully.";
        }

        void WriteData(int row, string column, object data) {
            WriteData(row, cols.IndexOf(column) + 1, data);
        }

        void WriteData(int row, int column, object data) {
            string serial = "";

            if (data is int || data is float)
                serial = data.ToString();
            else if (data is bool)
                serial = (bool)data == true ? "1" : "0";
            else if (data is string)
                serial = "\"" + data.ToString() + "\"";
            else if (data is string[])
                serial = "\"" + String.Join(",", (string[])data) + "\"";

            sb.AppendLine(String.Format("C;X{1};Y{2};K{0}", serial, column, row));
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e) {
            Save();
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e) {
            SaveAs();
        }

        private void deleteUnitToolStripMenuItem_Click(object sender, EventArgs e) {
            string aid = AidList.SelectedItem.ToString();
            if (MessageBox.Show("Are you sure you want to delete `" + aid + "`?", "", MessageBoxButtons.YesNo) == DialogResult.Yes) {
                Ability.Abilities.Remove(aid);
                AidList.Items.Remove(AidList.SelectedItem);
                AidList.SelectedIndex = 0;
                Saved = false;
                Status.Text = "Ability `" + aid + "` was deleted.";
            }
        }

        private void deleteUnitToolStripMenuItem1_Click(object sender, EventArgs e) {
            deleteUnitToolStripMenuItem_Click(null, null);
        }

        private void UidList_Format(object sender, ListControlConvertEventArgs e) {
            e.Value = String.Format("{0} - {1}", e.ListItem.ToString(), Ability.GetAbility(e.ListItem.ToString()).Name);
        }

        private void UidList_FormattingEnabledChanged(object sender, EventArgs e) {
            List<string> strs = new List<string>();
            foreach (object o in AidList.Items)
                strs.Add(o.ToString());
            AidList.Items.Clear();
            AidList.Items.AddRange(strs.ToArray());
        }

        public void Sort() {
            object[] data = new object[AidList.Items.Count];
            AidList.Items.CopyTo(data, 0);
            AidList.Items.Clear();
            Array.Sort<object>(data, new Comparison<object>(delegate (object a, object b) {
                return String.Compare(Ability.GetAbility(a.ToString()).Name, Ability.GetAbility(b.ToString()).Name);
            }));
            AidList.Items.AddRange(data);

            AidList.FormattingEnabled = false;
            AidList.FormattingEnabled = true;
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e) {
            if (OpenFile.ShowDialog() == DialogResult.OK)
                Open(OpenFile.FileName);
        }
    }
}