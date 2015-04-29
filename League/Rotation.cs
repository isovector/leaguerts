using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace LeagueEngine {
    /// <summary>
    /// Describes the Rotation of something. Probably could be done with a Quaternion,
    /// but math is yucky.
    /// </summary>
    public class Rotation {
        /// <summary>
        /// A list of Axes for this Rotation.
        /// </summary>
        private float[] Axes = new float[3] { 0f, 0f, 0f };

        /// <summary>
        /// The pitch (rotation around the X axis)
        /// </summary>
        public float Pitch {
            get { return Axes[0]; }
            set { Axes[0] = value; Update(); }
        }
        
        /// <summary>
        /// The yaw (rotation around the Y axis)
        /// </summary>
        public float Yaw {
            get { return Axes[1]; }
            set { Axes[1] = value; Update(); }
        }

        /// <summary>
        /// The roll (rotation around the Z axis)
        /// </summary>
        public float Roll {
            get { return Axes[2]; }
            set { Axes[2] = value; Update(); }
        }

        /// <summary>
        /// The internal matrix which represents this Rotation
        /// </summary>
        private Matrix matrix = Matrix.Identity;

        /// <summary>
        /// Indexes the axes of the Rotation
        /// </summary>
        /// <param name="axis">The axis to get</param>
        /// <returns>The value of the axis</returns>
        public float this[int axis] {
            get {
                if (axis == 0) return Pitch;
                else if (axis == 1) return Yaw;
                else if (axis == 2) return Roll;
                else throw new Exception("Undefined axis");
            }
            set {
                if (axis == 0) Pitch = value;
                else if (axis == 1) Yaw = value;
                else if (axis == 2) Roll = value;
                else throw new Exception("Undefined axis");
                matrix = Matrix.CreateRotationX(Pitch) * Matrix.CreateRotationY(Yaw) * Matrix.CreateRotationZ(Roll);
                Update();
            }
        }

        /// <summary>
        /// Recreates the Matrix
        /// </summary>
        private void Update() {
            matrix = Matrix.CreateRotationX(Pitch) * Matrix.CreateRotationY(Yaw) * Matrix.CreateRotationZ(Roll);
        }

        /// <summary>
        /// Turns the Rotation into an equivilent Matrix
        /// </summary>
        /// <param name="r">The rotation</param>
        /// <returns>A matrix of rotations</returns>
        public static implicit operator Matrix(Rotation r) {
            return r.matrix;
        }
    }
}
