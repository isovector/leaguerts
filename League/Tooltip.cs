using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace LeagueEngine {
    /// <summary>
    /// Describes a tooltip for a context button
    /// </summary>
    public class Tooltip {
        /// <summary>
        /// Describes an icon and a number to be placed on the tooltip
        /// </summary>
        public class IconNumber {
            /// <summary>
            /// The handicap icon
            /// </summary>
            public static Texture2D Handicap;

            /// <summary>
            /// The energy icon
            /// </summary>
            public static Texture2D Energy;

            /// <summary>
            /// The time icon
            /// </summary>
            public static Texture2D Time;

            /// <summary>
            /// The money icon
            /// </summary>
            public static Texture2D Money;

            /// <summary>
            /// The icon of this tooltip
            /// </summary>
            public Texture2D Icon;

            /// <summary>
            /// The number associated with the icon
            /// </summary>
            public int Number;


            /// <summary>
            /// Creates a new IconNumber
            /// </summary>
            /// <param name="texture">The texture to use</param>
            /// <param name="num">The number to display</param>
            public IconNumber(Texture2D texture, int num) {
                Icon = texture;
                Number = num;
            }

            /// <summary>
            /// Draws the IconNumber in the desired spot. Pos is specified
            /// by the Tooltip's IconNumber slots.
            /// </summary>
            /// <param name="pos">The position to draw the NW corner of the icon</param>
            public void Draw(Vector2 pos) {
                Gui.Batch.Draw(Icon, pos, Color.White);
                Gui.DrawString(Gui.BigFont, Number.ToString(), new Vector2(pos.X + Icon.Width + 2, pos.Y));
            }
        }


        /// <summary>
        /// The corner of a tooltip box. This should be the NW corner, it is transformed
        /// for the other corners.
        /// </summary>
        public static Texture2D TooltipCorner;

        /// <summary>
        /// The inside of a tooltip box
        /// </summary>
        public static Texture2D TooltipBox;

        /// <summary>
        /// The edge of a tooltip box. This should be the left edge. it will
        /// be transformed for the other edges.
        /// </summary>
        public static Texture2D TooltipEdge;

        /// <summary>
        /// The title of this tooltip
        /// </summary>
        public string Name;

        /// <summary>
        /// The main text body of the tooltip
        /// </summary>
        public string Description;

        /// <summary>
        /// The first IconNumber slot
        /// </summary>
        public IconNumber Icon1;

        /// <summary>
        /// The second IconNumber slot
        /// </summary>
        public IconNumber Icon2;

        /// <summary>
        /// The third IconNumber slot
        /// </summary>
        public IconNumber Icon3;

        /// <summary>
        /// The fourth IconNumber slot
        /// </summary>
        public IconNumber Icon4;

        /// <summary>
        /// The fifth IconNumber slot
        /// </summary>
        public IconNumber Icon5;

        /// <summary>
        /// The sixth IconNumber slot
        /// </summary>
        public IconNumber Icon6;
        
        /// <summary>
        /// The required assets for this tooltip
        /// </summary>
        public List<string> Requirements = new List<string>();

        /// <summary>
        /// Is the attached ContextButton available?
        /// </summary>
        private bool available = false;

        /// <summary>
        /// Is the attached ContextButton available?
        /// </summary>
        public bool Available {
            get { return available; }
            set { available = value; CompileString(); }
        }

        /// <summary>
        /// The compiled string of the ContextButton
        /// </summary>
        private string Compiled;
        private bool compiled = false;

        /// <summary>
        /// Creates a new tooltip
        /// </summary>
        /// <param name="name">The title of the tooltip</param>
        /// <param name="desc">The text of the tooltip</param>
        /// <param name="reqs">The assets required for the described ContextButton</param>
        /// <param name="icons">The IconNumbers to slot</param>
        public Tooltip(string name, string desc, List<string> reqs, params IconNumber[] icons) {
            Name = name;
            Description = desc;

            if (reqs != null)
                foreach (string req in reqs)
                    Requirements.Add(UnitType.GetUnitType(req).Name);

            if (icons.Length > 0) Icon1 = icons[0]; else Icon1 = null;
            if (icons.Length > 1) Icon2 = icons[1]; else Icon2 = null;
            if (icons.Length > 2) Icon3 = icons[2]; else Icon3 = null;
            if (icons.Length > 3) Icon4 = icons[3]; else Icon4 = null;
            if (icons.Length > 4) Icon5 = icons[4]; else Icon5 = null;
            if (icons.Length > 5) Icon6 = icons[5]; else Icon6 = null;
        }

        /// <summary>
        /// Draws the tooltip
        /// </summary>
        /// <param name="avail">Is the attached ContextButton available?</param>
        public void Draw(bool avail) {
            if (Available != avail || !compiled)
                Available = avail;
            float size = Gui.MeasureString(Gui.BigFont, Compiled).Y;
            float height = Gui.BigFont.MeasureString("A").Y;
            float top = 450 - size;
            Vector2 pos = new Vector2(555, top);

            Gui.Batch.Draw(TooltipBox, new Rectangle((int)pos.X, (int)pos.Y, 226, (int)size), Color.White);
            
            Gui.Batch.Draw(TooltipCorner, pos - new Vector2(8, 8), null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            Gui.Batch.Draw(TooltipCorner, new Vector2(pos.X - 8, pos.Y + size), null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.FlipVertically, 0f);
            Gui.Batch.Draw(TooltipCorner, new Vector2(pos.X + 226, pos.Y - 8), null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.FlipHorizontally, 0f);
            Gui.Batch.Draw(TooltipCorner, new Vector2(pos.X + 226, pos.Y + size), null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.FlipVertically | SpriteEffects.FlipHorizontally, 0f);
            Gui.Batch.Draw(TooltipEdge, new Rectangle((int)pos.X, (int)pos.Y - 8, 226, 8), null, Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0f);
            Gui.Batch.Draw(TooltipEdge, new Rectangle((int)pos.X - 8, (int)pos.Y + (int)size, (int)size, 8), null, Color.White, -MathHelper.PiOver2, Vector2.Zero, SpriteEffects.None, 0f);
            Gui.Batch.Draw(TooltipEdge, new Rectangle((int)pos.X, (int)pos.Y + (int) size, 226, 8), null, Color.White, 0f, Vector2.Zero, SpriteEffects.FlipVertically, 0f);
            Gui.Batch.Draw(TooltipEdge, new Rectangle((int)pos.X + 226 + 8, (int)pos.Y, (int)size, 8), null, Color.White, MathHelper.PiOver2, Vector2.Zero, SpriteEffects.None, 0f);
            Gui.DrawString(Gui.BigFont, Compiled, pos);

            if (Available) {
                if (Icon1 != null)
                    Icon1.Draw(new Vector2(555, top + height));
                if (Icon2 != null)
                    Icon2.Draw(new Vector2(635, top + height));
                if (Icon3 != null)
                    Icon3.Draw(new Vector2(715, top + height));
                if (Icon4 != null)
                    Icon4.Draw(new Vector2(555, top + height * 2));
                if (Icon5 != null)
                    Icon5.Draw(new Vector2(635, top + height * 2));
                if (Icon6 != null)
                    Icon6.Draw(new Vector2(715, top + height * 2));
            }
        }

        /// <summary>
        /// Creates the string to show for this tooltip
        /// </summary>
        private void CompileString() {
            string template = "{0}";
            string desc = (Description != "" && Description != null ? Description : "Tooltip missing!");
            string reqs = "|ccccc00";

            if (Available)
                if (Description != " ") {
                    string lines = "|n|n";
                    if (Icon4 != null || Icon5 != null || Icon6 != null)
                        lines += "|n|n";
                    else if (Icon1 != null || Icon2 != null || Icon3 != null)
                        lines += "|n";
                    template = "{0}" + lines + "{1}";
                } else
                    template = "{0}";
            else {
                if (Requirements != null)
                    foreach (string req in Requirements)
                        reqs += String.Format("- Requires {0}|n", req);
                else
                    reqs = "|n";

                template = "{0}|n{2}|r|n{1}";
            }

            Compiled = Gui.WordWrap(Gui.BigFont, String.Format(template, Name, desc, reqs), 200);
            compiled = true;
        }
    }
}
