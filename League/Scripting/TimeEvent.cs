using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Microsoft.Xna.Framework;

namespace LeagueEngine.Scripting {
    /// <summary>
    /// An event run every x seconds
    /// </summary>
    public class TimeEvent : GameComponent {
        /// <summary>
        /// Whether to run this hooked method asynchronously or not
        /// </summary>
        bool Async;

        /// <summary>
        /// Does this event repeat?
        /// </summary>
        bool Repeat;

        /// <summary>
        /// Themethod hooked.
        /// </summary>
        MethodInfo Event;

        /// <summary>
        /// The current amount of seconds since the last invoke.
        /// </summary>
        float Time;

        /// <summary>
        /// The time between each invoke.
        /// </summary>
        float InvokeTime;

        /// <summary>
        /// Creates a TimeEvent
        /// </summary>
        /// <param name="game">The League instance</param>
        /// <param name="e">The method to be invoked</param>
        /// <param name="time">The time after which to invoke</param>
        /// <param name="repeat">Does this event repeat?</param>
        /// <param name="async">Is this method to be run asynchronously?</param>
        public TimeEvent(League game, MethodInfo e, float time, bool repeat, bool async) : base(game) {
            Event = e;
            InvokeTime = time;
            Repeat = repeat;
            Async = async;
            Time = 0f;
        }

        /// <summary>
        /// Updates the Time and checks whether an invoke is needed
        /// </summary>
        /// <param name="gameTime">Snapshot of timing values</param>
        public override void Update(GameTime gameTime) {
            Time += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (Time > InvokeTime) {
                if (Async)
                    League.Engine.Script.StartMethod(Event);
                else
                    Event.Invoke(League.Engine.Script.ScriptInstance, new object[] { gameTime });

                if (!Repeat)
                    League.Engine.Components.Remove(this);
                else
                    Time = 0f;
            }

            base.Update(gameTime);
        }
    }
}
