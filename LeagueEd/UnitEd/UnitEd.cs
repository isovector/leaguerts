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
    public partial class UnitEd : Form {
        public bool Saved = true;
        public string CurrentFile = "";
        StringBuilder sb;
        List<string> cols;

        public UnitEd(string[] args) {
            InitializeComponent();

            if (args.Length != 0) {
                Open(args[0]);
            } else {
                Open(null);
            }
        }

        public void Open(string file) {
            UidList.Items.Clear();
            UnitType.Types.Clear();

            if (file != null) {
                GameData.UnitData = new SlkReader(file);
                CurrentFile = file;
            }

            foreach (KeyValuePair<string, Dictionary<string, string>> pair in GameData.UnitData.Data) {
                UidList.Items.Add(pair.Key);
                UnitType.GetUnitType(pair.Key);
            }
            UidList.SelectedIndex = 0;
            Sort();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e) {
            if (UidList.SelectedIndices.Count == 1) {
                Properties.SelectedObject = UnitType.GetUnitType(UidList.SelectedItem.ToString());
                Status.Text = "Editing `" + UnitType.GetUnitType(UidList.SelectedItem.ToString()).Name + " (" + UidList.SelectedItem.ToString() + ")`.";
            } else {
                Properties.SelectedObject = null;
            }
        }

        private void newUnitToolStripMenuItem_Click(object sender, EventArgs e) {
            MessagePrompt mp = new MessagePrompt("Enter new Uid:");

            if (mp.ShowDialog() == DialogResult.OK) {
                string uid = mp.textBox1.Text;
                UnitType.NewUnitType(uid, UidList.SelectedItem.ToString());
                UidList.Items.Add(uid);
                Saved = false;

                Sort();

                Status.Text = "Derrived new UnitType `" + uid + "`.";
            }
        }

        private void createNewUnitToolStripMenuItem_Click(object sender, EventArgs e) {
            newUnitToolStripMenuItem_Click(null, null);
        }

        private void newUnitToolStripMenuItem1_Click(object sender, EventArgs e) {
            MessagePrompt mp = new MessagePrompt("Enter new Uid:");

            if (mp.ShowDialog() == DialogResult.OK) {
                string uid = mp.textBox1.Text;
                UnitType type = new UnitType();
                type.Uid = uid;
                UnitType.Types.Add(uid, type);
                UidList.Items.Add(uid);
                Saved = false;

                Sort();

                Status.Text = "Created new UnitType `" + uid + "`.";
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
            for (int i = 0; i < GameData.UnitData.Table.GetLength(0); i++)
                cols.Add(GameData.UnitData.Table[i, 0]);

            sb.AppendLine("ID;UnitEd100");

            foreach (string col in cols)
                WriteData(1, col, col);

            int row = 2;
            foreach (KeyValuePair<string, UnitType> pair in UnitType.Types) {
                UnitType type = pair.Value;

                WriteData(row, "uid", type.Uid);
                WriteData(row, "moveHeight", type.Height);
                WriteData(row, "moveSpeed", type.Speed);
                WriteData(row, "turnSpeed", type.TurnSpeed);
                WriteData(row, "moveOnGround", type.ConstrainToGround);
                WriteData(row, "scale", type.Scale);
                WriteData(row, "buildTime", type.BuildTime);
                WriteData(row, "cost", type.ResourceCosts);
                WriteData(row, "comment", type.Name);
                WriteData(row, "selectionCircle", type.SelectionCircleSize);
                WriteData(row, "isBuilding", type.IsBuilding);
                WriteData(row, "useTeamColor", type.UseTeamColor);
                WriteData(row, "attack1", type.Attacks);
                WriteData(row, "attackGfx1", type.AttackGfx);
                WriteData(row, "attackScale1", type.AttackGfxSize);
                WriteData(row, "attackRng1", type.AttackRange);
                WriteData(row, "attackMagic1", type.AttackMagic);
                WriteData(row, "attackMagicData1", type.AttackMagicData);
                WriteData(row, "attackSpeed1", type.AttackSpeed);
                WriteData(row, "attackEngage1", type.AttackEngage);
                WriteData(row, "attackDmg1", type.AttackDamage);
                WriteData(row, "attackTarget1", type.AttackAllowedTarget);
                WriteData(row, "sight", type.Sight);
                WriteData(row, "attackCooldown1", type.AttackCooldown);
                WriteData(row, "hp", type.Hp);
                WriteData(row, "energy", type.Energy);
                WriteData(row, "desc", type.Description);
                WriteData(row, "trains", type.UnitsTrained);
                WriteData(row, "builds", type.UnitsBuilt);
                WriteData(row, "abilities", type.Abilities);
                WriteData(row, "skin", type.SkinPath);
                WriteData(row, "model", type.MeshPath);
                WriteData(row, "classification", type.ClassifiedAs);
                WriteData(row, "icon", type.IconPath);
                WriteData(row, "x", type.IconX); 
                WriteData(row, "y", type.IconY);
                WriteData(row, "requires", type.Dependencies);
                //WriteData(row, cols.Count + 1, "");

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
            string uid = UidList.SelectedItem.ToString();
            if (MessageBox.Show("Are you sure you want to delete `" + uid + "`?", "", MessageBoxButtons.YesNo) == DialogResult.Yes) {
                UnitType.Types.Remove(uid);
                UidList.Items.Remove(UidList.SelectedItem);
                UidList.SelectedIndex = 0;
                Saved = false;
                Status.Text = "Unit `" + uid + "` was deleted.";
            }
        }

        private void deleteUnitToolStripMenuItem1_Click(object sender, EventArgs e) {
            deleteUnitToolStripMenuItem_Click(null, null);
        }

        private void UidList_Format(object sender, ListControlConvertEventArgs e) {
            e.Value = String.Format("{0} - {1}", e.ListItem.ToString(), UnitType.GetUnitType(e.ListItem.ToString()).Name);
        }

        private void UidList_FormattingEnabledChanged(object sender, EventArgs e) {
            List<string> strs = new List<string>();
            foreach (object o in UidList.Items)
                strs.Add(o.ToString());
            UidList.Items.Clear();
            UidList.Items.AddRange(strs.ToArray());
        }

        public void Sort() {
            object[] data = new object[UidList.Items.Count];
            UidList.Items.CopyTo(data, 0);
            UidList.Items.Clear();
            Array.Sort<object>(data, new Comparison<object>(delegate (object a, object b) {
                return String.Compare(UnitType.GetUnitType(a.ToString()).Name, UnitType.GetUnitType(b.ToString()).Name);
            }));
            UidList.Items.AddRange(data);

            UidList.FormattingEnabled = false;
            UidList.FormattingEnabled = true;
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e) {
            if (OpenFile.ShowDialog() == DialogResult.OK)
                Open(OpenFile.FileName);
        }
    }
}