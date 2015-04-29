using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace LeagueEd {
    public unsafe class RTBSyntaxHighlighter {
        #region Properties

        public class SyntaxItem {
            private Color color;
            public Color Color {
                get { return color; }
                set { color = value; }
            }

            private string expression;
            public string Expression {
                get { return expression; }
                set { expression = value; }
            }

            private RegexOptions regexOptions;
            public RegexOptions RegexOptions {
                get { return regexOptions; }
                set { regexOptions = value; }
            }

            public SyntaxItem(Color color, string expression, RegexOptions regexOptions) {
                this.Color = color;
                this.expression = expression;
                this.regexOptions = regexOptions;
            }

            public SyntaxItem(Color color, string expression)
                : this(color, expression, RegexOptions.Singleline) { }

        }

        public class LanguageSyntax {
            private string language;

            public string Language {
                get { return language; }
                set { language = value; }
            }

            private List<SyntaxItem> syntaxList = new List<SyntaxItem>();

            public List<SyntaxItem> SyntaxList {
                get { return syntaxList; }
                set { syntaxList = value; }
            }
        }

        private Dictionary<string, LanguageSyntax> languageSet = new Dictionary<string, LanguageSyntax>();

        public Dictionary<string, LanguageSyntax> LanguageSet {
            get { return languageSet; }
            set { languageSet = value; }
        }
        #endregion


        public void CreateLSCScript() {
            LanguageSyntax syntax = new LanguageSyntax();
            syntax.Language = "lsc";
            syntax.SyntaxList.Add(new SyntaxItem(Color.FromArgb(127, 85, 0), @"\b(on|TimeElapsed|Periodic|MapInit|UnitCreated|UnitDied|RegionEntered|RegionLeft|AbilityCast|AbilitySmart|ParticleSystem|Update|ProjectileCreated|ProjectileDestroyed|UnitAttacked|UnitRemoved)\b"));
            syntax.SyntaxList.Add(new SyntaxItem(Color.Blue, @"\b(new|as|null|switch|object|bool|false|throw|break|finally|out|true|byte|try|case|float|params|typeof|catch|for|uint|char)\b"));
            syntax.SyntaxList.Add(new SyntaxItem(Color.Blue, @"\b(foreach|ulong|goto|if|readonly|const|ref|ushort|continue|in|return|using|decimal|int|sbyte|default|volatile|delegate|internal|short|void|do|is|sizeof|while|double|lock|else|long|static|enum|string)\b"));
            syntax.SyntaxList.Add(new SyntaxItem(Color.Teal, @"\b(Dictionary|List|Queue|Stack|String|System|Vector3|Vector2|Matrix|Texture2D|Model|GameTime|Color|MathHelper|Unit|Player|UnitType|Map|Region|Projectile|Ability|ContextButton|Tooltip|UnitGroup|Visibility|UnitState|UnitQueue|Rotation|League|Blend|ParticleSystem|ParticleEmitter|ParticleSettings|GameObject|GameMessage)\b"));
            syntax.SyntaxList.Add(new SyntaxItem(Color.Red/*FromArgb(100, 100, 100)*/, @"^#.*$", RegexOptions.Multiline));
            syntax.SyntaxList.Add(new SyntaxItem(Color.FromArgb(0xA31515), "\"[^\"]*\""));
            syntax.SyntaxList.Add(new SyntaxItem(Color.Green, @"(\/\/.*)", RegexOptions.Multiline));
            syntax.SyntaxList.Add(new SyntaxItem(Color.Green, @"\/\*.*\*\/"));

            if (this.languageSet.ContainsKey(syntax.Language))
                this.languageSet.Remove(syntax.Language);
            this.languageSet.Add(syntax.Language, syntax);
        }

        public RTBSyntaxHighlighter() {
            //CreateCSharpScript();
            //CreateHLSLScript();
            CreateLSCScript();
        }


        #region ColorizeRTB

        public unsafe void ColorizeRTB(RichTextBox rtb, string language) {
            if (this.languageSet.ContainsKey(language) == false ||
                rtb.Text.Length == 0) {
                return;
            }

            if (rtb.Parent != null)
                if (rtb.Parent.Parent != null)
                    Win32.LockWindowUpdate(rtb.Parent.Parent.Handle);
                else
                    Win32.LockWindowUpdate(rtb.Parent.Handle);
            else
                Win32.LockWindowUpdate(rtb.Handle);



            try {

                Point pScrollPos = Win32.GetScrollPos(rtb.Handle);

                int iSelStart = rtb.SelectionStart;

                rtb.SelectAll();
                rtb.SelectionColor = Color.Black;


                LanguageSyntax syntax = languageSet[language];

                foreach (SyntaxItem si in syntax.SyntaxList) {
                    Regex regx = new Regex(si.Expression, si.RegexOptions);

                    foreach (Match m in regx.Matches(rtb.Text)) {
                        rtb.SelectionStart = m.Index;
                        rtb.SelectionLength = m.Length;

                        rtb.SelectionColor = si.Color;
                    }
                }

                rtb.DeselectAll();

                rtb.SelectionStart = iSelStart;
                rtb.SelectionLength = 0;

                Win32.SetScrollPos(rtb.Handle, pScrollPos);
            } catch (Exception e) {
                System.Windows.Forms.MessageBox.Show("Error evaluating regex expression during rtb colorizing: " + e.Message);
            } finally {
                Win32.LockWindowUpdate(IntPtr.Zero);
            }
        }
        #endregion
    }
}
