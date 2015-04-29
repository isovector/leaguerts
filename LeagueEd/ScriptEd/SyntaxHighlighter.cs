using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;

namespace LeagueEd {
    public partial class SyntaxRTB : RichTextBox {
        public SyntaxRTB()
            : base() {
            syntaxLanguage = "hlsl";
        }

        private RTBSyntaxHighlighter syntaxHighlighter = new RTBSyntaxHighlighter();

        public RTBSyntaxHighlighter SyntaxHighlighter {
            get { return syntaxHighlighter; }
        }

        private string syntaxLanguage;

        public string SyntaxLanguage {
            get { return syntaxLanguage; }
            set { syntaxLanguage = value; }
        }


        public void RefreshHighlight() {
            syntaxHighlighter.ColorizeRTB(this, syntaxLanguage);
        }

        /*protected override void OnTextChanged(EventArgs e) {
            RefreshHighlight();

            base.OnTextChanged(e);
        }*/

    }
}
