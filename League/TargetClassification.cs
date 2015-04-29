using System;
using System.Collections.Generic;
using System.Text;

namespace LeagueEngine {
    /// <summary>
    /// Provides a classification system with support for uninteresting values
    /// </summary>
    public class TargetClassification {
        /// <summary>
        /// The data held by this TC
        /// </summary>
        public Dictionary<Classification, ClassificationState> Data = new Dictionary<Classification, ClassificationState>();

        /// <summary>
        /// Adds a classification and the required value for it
        /// </summary>
        /// <param name="clas">The Classification term</param>
        /// <param name="state">The required value of the Classification</param>
        public void Add(Classification clas, ClassificationState state) {
            if (!Data.ContainsKey(clas))
                Data.Add(clas, state);
            else
                if (Data[clas] != state)
                    Data[clas] = ClassificationState.Either;
        }
    }

    public enum ClassificationState {
        No, Yes, Either
    }
}
