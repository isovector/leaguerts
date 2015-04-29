using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace LeagueEngine {
    /// <summary>
    /// Describes a message shown in game.
    /// </summary>
    public class GameMessage : GameComponent {
        /// <summary>
        /// The time used if none is specified
        /// </summary>
        public const float DefaultTime = 8;

        /// <summary>
        /// To whom this message is visible - not available in Alpha 1
        /// </summary>
        public Team VisibleTo;

        /// <summary>
        /// From whom this message was sent - not available in Alpha 1
        /// </summary>
        public Player Origin;

        /// <summary>
        /// The contents of the message. May contain League string formatting
        /// </summary>
        public string Message;

        /// <summary>
        /// The time over which this message will be shown, if not interrupted
        /// </summary>
        public float IntendedTime;

        /// <summary>
        /// The current time this message has been shown
        /// </summary>
        public float Time = 0;


        /// <summary>
        /// Creates a message to be shown to everyone for a specified time
        /// </summary>
        /// <param name="message">The message to show</param>
        /// <param name="time">The time to show the message</param>
        public GameMessage(string message, float time) 
            : base(League.Engine) {
            Message = message;
            IntendedTime = time;
        }

        /// <summary>
        /// Creates a message shown to only one Player for a specified time
        /// </summary>
        /// <param name="to">To whom to show this message</param>
        /// <param name="message">The message</param>
        /// <param name="time">The time to show this message</param>
        public GameMessage(Player to, string message, float time)
            : base(League.Engine) {
            Origin = to;
            VisibleTo = Team.Any;
            Message = message;
            IntendedTime = time;
        }

        /// <summary>
        /// Creates a message sent by a Player's chat
        /// </summary>
        /// <param name="from">The origin of the message</param>
        /// <param name="view">Who can see the message</param>
        /// <param name="message">The message</param>
        public GameMessage(Player from, Team view, string message)
            : base(League.Engine) {
            Origin = from;
            VisibleTo = view;
            Message = message;
            IntendedTime = DefaultTime;
        }

        /// <summary>
        /// Enforces time rules
        /// </summary>
        /// <param name="gameTime">A snapshot of timing values</param>
        public override void Update(GameTime gameTime) {
            Time += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (Time > IntendedTime) {
                League.Engine.Components.Remove(this);
                League.Engine.Interface.Messages.Remove(this);
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// Shows a game message for the default time
        /// </summary>
        /// <param name="message">The message</param>
        public static void ShowMessage(string message) {
            GameMessage m = new GameMessage(message, DefaultTime);
            League.Engine.Interface.Messages.Add(m);
            League.Engine.Components.Add(m);
        }

        /// <summary>
        /// Shows a game message for the specified time
        /// </summary>
        /// <param name="message">The message</param>
        /// <param name="time">The time to show the message</param>
        public static void ShowMessage(string message, float time) {
            GameMessage m = new GameMessage(message, time);
            League.Engine.Interface.Messages.Add(m);
            League.Engine.Components.Add(m);
        }
    }
}
