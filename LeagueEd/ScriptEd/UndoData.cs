using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace LeagueEd {
    public class UndoData {
        public int Position;
        public string Text;

        public UndoData(string dat, int pos) {
            Text = dat;
            Position = pos;
        }

        public void Undo(RichTextBox box) {
            box.Text = Text;
            box.SelectionStart = Position;
        }
    }
}
