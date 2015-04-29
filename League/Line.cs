using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LeagueEngine {
    /// <summary>
    /// Describes a line
    /// </summary>
    public class Line {
        /// <summary>
        /// The start of the line in screen space
        /// </summary>
        public Vector2 Start = new Vector2(-1f, -1f);

        /// <summary>
        /// The end of the line in screen space
        /// </summary>
        public Vector2 End = new Vector2(-1f, -1f);

        /// <summary>
        /// The color of the line
        /// </summary>
        public Color Color = Color.White;

        /// <summary>
        /// Gets the length of the line
        /// </summary>
        public float Length {
            get {
                return Vector2.Distance(Start, End);
            }
        }

        /// <summary>
        /// Gets the slope of the line
        /// </summary>
        public float Slope {
            get {
                return (float)Math.Atan2(End.Y - Start.Y, End.X - Start.X);
            }
        }

        /// <summary>
        /// Creates a line
        /// </summary>
        /// <param name="s">The start of the line</param>
        /// <param name="e">The end of the line</param>
        /// <param name="c">The color of the line</param>
        public Line(Vector2 s, Vector2 e, Color c) {
            Start = s;
            End = e;
            Color = c;
        }
    }

}
