using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace LeagueEngine {
    /// <summary>
    /// Describes a resource type
    /// </summary>
    public class Resource {
        /// <summary>
        /// A dictionary of resource names against resources
        /// </summary>
        public static Dictionary<string, Resource> Resources = new Dictionary<string, Resource>();

        /// <summary>
        /// The default amount of this Resource given
        /// </summary>
        public int Amount;

        /// <summary>
        /// The preferred icon number slot to put this resource
        /// in for tooltips. Valid numbers are 1-6 inclusive.
        /// </summary>
        public int PreferredIconNumber;

        /// <summary>
        /// The name of this Resource.
        /// </summary>
        public string Name;

        /// <summary>
        /// The path to this Resource's tooltip icon
        /// </summary>
        public string IconPath;

        /// <summary>
        /// The point on the screen to draw the Resource watcher
        /// </summary>
        public Vector2 DrawPoint;

        /// <summary>
        /// The icon of the Resource's tooltip
        /// </summary>
        public Texture2D Icon;


        /// <summary>
        /// Makes a new resource. This should not be called, instead call Resource.CreateResource();
        /// </summary>
        /// <param name="name">The name of the resource</param>
        /// <param name="icon">The path relative to icons/tooltips/ of the icon</param>
        /// <param name="draw">The screen point to draw the Resource amount watcher</param>
        /// <param name="amount">The default amount of the Resource to be given</param>
        /// <param name="number">The preferred icon number slot of this resource</param>
        public Resource(string name, string icon, Vector2 draw, int amount, int number) {
            Name = name;
            IconPath = icon;
            Icon = League.Engine.Content.Load<Texture2D>("icons/tooltips/" + IconPath);
            DrawPoint = draw;
            Amount = amount;
            PreferredIconNumber = number;
        }

        /// <summary>
        /// Creates a resource and adds it to the Resource list.
        /// </summary>
        /// <param name="name">The name of the resource</param>
        /// <param name="icon">The path relative to icons/tooltips/ of the icon</param>
        /// <param name="draw">The screen point to draw the Resource amount watcher</param>
        /// <param name="amount">The default amount of the Resource to be given</param>
        /// <param name="number">The preferred icon number slot of this resource</param>
        /// <returns>The new Resource</returns>
        public static Resource CreateResource(string name, string icon, Vector2 draw, int amount, int number) {
            Resource res = new Resource(name, icon, draw, amount, number);
            Resources.Add(name.ToLower(), res);
            return res;
        }
    }
}
