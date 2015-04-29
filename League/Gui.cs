using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using LeagueEngine.Effects;

namespace LeagueEngine {
    /// <summary>
    /// Describes an interface for the user
    /// </summary>
    public class Gui : DrawableGameComponent {
        /// <summary>
        /// The instance of League
        /// </summary>
        League Engine;

        /// <summary>
        /// Used to draw 2d objects
        /// </summary>
        public static SpriteBatch Batch;

        /// <summary>
        /// Fonts used in game
        /// </summary>
        public static SpriteFont BigFont, SmallFont;

        /// <summary>
        /// The button for which a Tooltip is being shown
        /// </summary>
        public ContextButton TooltipButton = ContextButton.Null;

        /// <summary>
        /// The current Tooltip
        /// </summary>
        public Tooltip Tooltip;

        /// <summary>
        /// An object to count garbage collection
        /// </summary>
        WeakReference GCD = new WeakReference(new object());

        /// <summary>
        /// A list of lines to draw - used for selection rectangles.
        /// </summary>
        public List<Line> Lines = new List<Line>();

        /// <summary>
        /// The cursor to be drawn
        /// </summary>
        Texture2D Cursor;

        /// <summary>
        /// A graphical representation of a line
        /// </summary>
        Texture2D Line;

        /// <summary>
        /// The docked part of the UI
        /// </summary>
        Texture2D Interface;

        /// <summary>
        /// A progress bar for showing unit trained progress
        /// </summary>
        Texture2D ProgressBar;

        /// <summary>
        /// A texture to fill the progress bar.
        /// </summary>
        Texture2D ProgressFill;

        /// <summary>
        /// The blip on the minimap for a unit.
        /// </summary>
        Texture2D MapBlit;

        /// <summary>
        /// The blank icon template.
        /// </summary>
        Texture2D Template;

        /// <summary>
        /// The number of garbage collections run
        /// </summary>
        int GCR = 0;

        /// <summary>
        /// The actions shown in the action panel.
        /// </summary>
        public List<ContextButton> Actions;

        /// <summary>
        /// The time since the last GUI action. Used to prevent getting five events
        /// invoked every mouse click.
        /// </summary>
        float Cooldown = 0f;

        /// <summary>
        /// The encapsulated cooldown shader.
        /// </summary>
        public CooldownEffect CooldownEffect;

        /// <summary>
        /// The model to draw cooldowns
        /// </summary>
        public Model CooldownCircle;

        /// <summary>
        /// The render target on which cooldowns are drawn
        /// </summary>
        RenderTarget2D CooldownTarget;

        /// <summary>
        /// A collection of Cooldown textures - one per icon
        /// </summary>
        Texture2D[,] CooldownTextures = new Texture2D[5, 3];

        /// <summary>
        /// A list of Messages to show on screen
        /// </summary>
        public List<GameMessage> Messages = new List<GameMessage>();


        /// <summary>
        /// Creates a new GUI
        /// </summary>
        /// <param name="game">The instace of League</param>
        public Gui(League game) : base(game) {
            Engine = game;
            DrawOrder = 6;
        }

        /// <summary>
        /// Loads all graphics and other misc stuff
        /// </summary>
        protected override void LoadContent() {
            Batch = new SpriteBatch(GraphicsDevice);
            BigFont = Game.Content.Load<SpriteFont>("bigfont");
            SmallFont = Game.Content.Load<SpriteFont>("smallfont");
            Cursor = Game.Content.Load<Texture2D>("cursor");
            Line = Game.Content.Load<Texture2D>("line");
            Interface = Game.Content.Load<Texture2D>("gui");
            ProgressBar = Game.Content.Load<Texture2D>("gui/progressbar");
            ProgressFill = Game.Content.Load<Texture2D>("gui/progressbarfill");
            Template = Game.Content.Load<Texture2D>("icons/template");

            // We can't do these at run time so we set them here
            ContextButton.Attack.Texture = Game.Content.Load<Texture2D>("icons/attack");
            ContextButton.Guard.Texture = Game.Content.Load<Texture2D>("icons/guard");
            ContextButton.Patrol.Texture = Game.Content.Load<Texture2D>("icons/patrol");
            ContextButton.Move.Texture = Game.Content.Load<Texture2D>("icons/move");
            ContextButton.Stop.Texture = Game.Content.Load<Texture2D>("icons/stop");
            ContextButton.Rally.Texture = Game.Content.Load<Texture2D>("icons/rally");
            ContextButton.Cancel.Texture = Game.Content.Load<Texture2D>("icons/cancel");
            ContextButton.Null.Texture = Game.Content.Load<Texture2D>("icons/template");

            CooldownEffect = new CooldownEffect("shaders/Cooldown");
            CooldownEffect.Load(Engine.Content);
            CooldownCircle = Engine.Content.Load<Model>("models/cooldown");

            CooldownTarget = new RenderTarget2D(GraphicsDevice, 36, 36, 1, SurfaceFormat.Color);

            CooldownEffect.World = Matrix.CreateScale(0.5f) * Matrix.CreateTranslation(0, -1f, 0);
            CooldownEffect.View = Matrix.CreateLookAt(Vector3.Up, Vector3.Zero, Vector3.Right);
            CooldownEffect.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, 1f, 1.0f, 10000.0f);

            for (int x = 0; x < 5; x++)
                for (int y = 0; y < 3; y++)
                    CooldownTextures[x, y] = new Texture2D(GraphicsDevice, 36, 36, 1, TextureUsage.None, SurfaceFormat.Color);

            MapBlit = Game.Content.Load<Texture2D>("gui/mapblit");
            base.LoadContent();
        }

        /// <summary>
        /// Updates the Gui
        /// </summary>
        /// <param name="gameTime">A snapshot of timing values</param>
        public override void Update(GameTime gameTime) {
            if (Player.CurrentPlayer.Selected.Count > 0)
                // Create a cooldown texture for all abilities which need it
                foreach (KeyValuePair<string, float> pair in Player.CurrentPlayer.Selected[0].AbilityCooldown) {
                    Ability a = Ability.GetAbility(pair.Key);
                    if (a.IconX != -1 && a.IconY != -1) {
                        float complete = (a.Cooldown - pair.Value) / a.Cooldown;
                        GetCooldownCircle(CooldownTextures[a.IconX, a.IconY], complete);
                    }
                }

            // Increase GC count
            if (!GCD.IsAlive) {
                GCR++;
                GCD = new WeakReference(new object());
            }

            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            MouseState ms = Mouse.GetState();
            Rectangle mouse = new Rectangle(ms.X, ms.Y, 1, 1);
            Cooldown -= elapsed;

            KeyboardState ks = Keyboard.GetState();

            // There are multiple units selected - show the selection UI
            if (Player.CurrentPlayer.Selected.Count > 1) {
                TooltipButton = ContextButton.Null;
                int x = 0;
                int y = 0;
                for (int i = 0; i < Player.CurrentPlayer.Selected.Count; i++) {
                    Unit selected = Player.CurrentPlayer.Selected[i];

                    // We don't have rectangles for these, so we need to do the math ourselves
                    if (ms.X >= 173 + x * 36 && ms.Y >= 481 + y * 36 && ms.X < 209 + x * 36 && ms.Y < 520 + y * 36) {
                        if (ms.LeftButton == ButtonState.Pressed && Cooldown <= 0f) {
                            if (ks.IsKeyDown(Keys.LeftControl)) {
                                Player.CurrentPlayer.Selected.Remove(selected);
                            } else {
                                if ((Engine.OnSelect != null && Engine.OnSelect.Invoke(selected, Engine.OnSelectTag)) ||
                                    Engine.OnSelect == null) {
                                    Player.CurrentPlayer.Selected.Clear();
                                    Player.CurrentPlayer.Selected.Add(selected);
                                }
                            }
                            Cooldown = 0.2f;
                            return;
                        }

                        // Create a non-ContextButton tooltip
                        TooltipButton = null;
                        Tooltip = new Tooltip(selected.Type.Name, " ", null);
                        return;
                    }

                    x++;
                    if (x == 9) {
                        x = 0;
                        y++;
                    }
                }
            }
            // There is one unit selected and it is training
            else if (Player.CurrentPlayer.Selected.Count == 1 && Player.CurrentPlayer.Selected[0].Training.Count != 0) {
                TooltipButton = ContextButton.Null;
                int x = 0;
                int y = 0;
                for (int i = 0; i < Player.CurrentPlayer.Selected[0].Training.Count; i++) {
                    if (ms.X >= 173 + x * 36 && ms.Y >= 504 + y * 36 && ms.X < 209 + x * 36 && ms.Y < 540 + y * 36) {
                        if (ms.LeftButton == ButtonState.Pressed && Cooldown <= 0f) {
                            // Cancel training units
                            Player.CurrentPlayer.Selected[0].Training.DequeueAt(i);
                            if (i == 0 && Player.CurrentPlayer.Selected[0].Training.Count > 0)
                                Player.CurrentPlayer.Selected[0].TrainTime = GameData.GetUnitData<int>(Player.CurrentPlayer.Selected[0].Training.Peek(), "buildTime");
                            Cooldown = 0.2f;
                            return;
                        }

                        TooltipButton = null;
                        Tooltip = new Tooltip(UnitType.GetUnitType(Player.CurrentPlayer.Selected[0].Training.Peek(i)).Name, " ", null);
                        return;
                    }

                    if (y == 0)
                        y++;
                    else
                        x++;
                    if (x == 9) {
                        x = 0;
                        y++;
                    }
                }
            }

            // There is a tooltip to be shown
            if (TooltipButton != null) {
                Tooltip = null;
                TooltipButton = ContextButton.Null;
                if (Actions != null)
                    foreach (ContextButton button in Actions) {
                        if ((ks.IsKeyDown(button.Hotkey) || (button.Rectangle.Intersects(mouse) && ms.LeftButton == ButtonState.Pressed)) && Cooldown <= 0f) {
                            button.Action.Invoke(button.Name, button.Tag);
                            //Player.CurrentPlayer.RefreshActions();
                            Cooldown = 0.2f;
                        }
                        if (button.Rectangle.Intersects(mouse)) {
                            TooltipButton = button;
                            Tooltip = button.Tooltip;
                        }
                    }
            }

            // Coerce messages count
            if (Messages.Count > 6)
                for (int i = 0; i < Messages.Count - 6; i++)
                    Messages.RemoveAt(0);

            base.Update(gameTime);
        }

        /// <summary>
        /// Draws the GUI
        /// </summary>
        /// <param name="gameTime">A snapshot of timing values</param>
        public override void Draw(GameTime gameTime) {
            Batch.Begin(SpriteBlendMode.AlphaBlend);
            MouseState ms = Mouse.GetState();

            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            DrawString(SmallFont, String.Format("|cffcc00FPS:|r {0}", 1 / elapsed), new Vector2(33));
            DrawString(SmallFont, String.Format("|cffcc00GC:|r  {0}", GCR), new Vector2(33, 45));

            // Draw all the lines in our collection.
            // Why doesn't XNA do this?
            for (int l = 0; l < Lines.Count; l++) {
                int i = 0;
                for (; i < (int)Math.Floor(Lines[l].Length / 100f); i++)
                    Batch.Draw(Line, Lines[l].Start + Vector2.Transform(new Vector2(i * 100f, 0f), Matrix.CreateRotationZ(Lines[l].Slope)), null, Lines[l].Color, Lines[l].Slope, new Vector2(1, 1), new Vector2(1f, 1f), SpriteEffects.None, 1f);
                Batch.Draw(Line, Lines[l].Start + Vector2.Transform(new Vector2(i * 100f, 0f), Matrix.CreateRotationZ(Lines[l].Slope)), null, Lines[l].Color, Lines[l].Slope, new Vector2(1, 1), new Vector2((Lines[l].Length % 100f) / 100f, 1f), SpriteEffects.None, 1f);
            }
            // Lines are only valid for one draw
            Lines.Clear();
            
            //There is a tooltip to be drawn
            if (TooltipButton != null && TooltipButton.TypeOf == typeof(UnitType) && Tooltip != null)
                // Is this a unit and are it's requisists met?
                Tooltip.Draw(Player.CurrentPlayer.MatchesAssets(UnitType.GetUnitType((string)TooltipButton.Tag).Requirements));
            else if (Tooltip != null)
                Tooltip.Draw(true);

            Batch.Draw(Interface, new Rectangle(0, 450, 800, 150), Color.White);

            if (Player.CurrentPlayer.Selected.Count == 1) {
                if (Player.CurrentPlayer.Selected[0].Training.Count == 0) {
                    // There is only one unit selected so we can draw its stats
                    DrawCenteredString(SmallFont, Player.CurrentPlayer.Selected[0].Type.Name, new Vector2(355, 516));
                    DrawCenteredString(SmallFont, String.Format("|c66ff66{0}/{1}", Player.CurrentPlayer.Selected[0].Hp, Player.CurrentPlayer.Selected[0].Type.Hp), new Vector2(221, 567));
                    DrawCenteredString(SmallFont, String.Format("|c66ccff{0}/{1}", Player.CurrentPlayer.Selected[0].Energy, Player.CurrentPlayer.Selected[0].Type.Energy), new Vector2(221, 583));
                } else {
                    // The unit is training
                    UnitType type = UnitType.GetUnitType(Player.CurrentPlayer.Selected[0].Training.Peek());
                    DrawString(SmallFont, String.Format("Training |cffcc00{0}|r...", type.Name), new Vector2(245, 504));
                    float progress = (type.BuildTime - Player.CurrentPlayer.Selected[0].TrainTime) / type.BuildTime * 148f;
                    Batch.Draw(ProgressBar, new Vector2(245, 520), Color.LightBlue);
                    Batch.Draw(ProgressFill, new Rectangle(246, 521, (int)progress, 10), Color.Green);
                    int x = 0;
                    int y = 0;
                    foreach (string training in Player.CurrentPlayer.Selected[0].Training) {
                        UnitType nowtype = UnitType.GetUnitType(training);
                        Batch.Draw(nowtype.Button.Texture, new Rectangle(173 + x * 36, 504 + y * 36, 36, 36), Color.White);
                        if (y == 0)
                            y++;
                        else
                            x++;
                        if (x == 9) {
                            x = 0;
                            y++;
                        }
                    }
                }
            } else {
                // There are multiple units selected
                int x = 0;
                int y = 0;
                foreach (Unit selected in Player.CurrentPlayer.Selected) {
                    Batch.Draw(selected.Type.Button.Texture, new Rectangle(173 + x * 36, 481 + y * 36, 36, 36), Color.White);
                    x++;
                    if (x == 9) {
                        x = 0;
                        y++;
                    }
                }
            }

            // Draw the action panel
            if (Actions != null)
                foreach (ContextButton button in Actions) {
                    if ((button.TypeOf == typeof(UnitType) && Player.CurrentPlayer.MatchesAssets(UnitType.GetUnitType((string)button.Tag).Requirements)) ||
                        button.TypeOf != typeof(UnitType)) {
                        // The requisits are met
                        Batch.Draw(Template, button.Rectangle, Color.White);
                        Batch.Draw(button.Texture, button.Rectangle, Color.White);

                        if (button.TypeOf == typeof(Ability)) {
                            Ability a = Ability.GetAbility(button.Tag.ToString());
                            if (Player.CurrentPlayer.Selected[0].AbilityCooldown.ContainsKey(a.Aid) && a.IconX != -1 && a.IconY != -1)
                                // There is a cooldown to be drawn
                                Batch.Draw(CooldownTextures[a.IconX, a.IconY], button.Rectangle, Color.White);
                        }
                    } else {
                        // Dependencies are not met
                        Batch.Draw(Template, button.Rectangle, Color.Gray);
                        Batch.Draw(button.Texture, button.Rectangle, Color.Gray);
                    }
                }

            Batch.Draw(Engine.CurrentMap.Minimap, new Rectangle(5, 467, 128, 128), Color.White);

            // Draw minimap blits for units
            lock (Engine.Units) {
                foreach (Unit u in Engine.Units) {
                    Visibility v = Player.CurrentPlayer.GetPointVisibility(u.Position);

                    // Buildings are seen through fog - units are not
                    if (((v & Visibility.Fogged) == Visibility.Fogged && u.Type.IsBuilding) || (!u.Type.IsBuilding && v == Visibility.Visible)) {
                        int x = 69 + (int)(u.Position.X / Engine.CurrentMap.Width * 128);
                        int y = 531 + (int)(u.Position.Y / Engine.CurrentMap.Height * 128);

                        Batch.Draw(MapBlit, new Vector2(x, y), u.Owner.TeamColor);
                    }
                }
            }

            // Draw all of the resources
            foreach (KeyValuePair<string, Resource> pair in Resource.Resources) {
                Resource res = pair.Value;
                DrawString(SmallFont, Player.CurrentPlayer.Resources[pair.Key].ToString(), res.DrawPoint + new Vector2(20, 0));
                Batch.Draw(res.Icon, res.DrawPoint, Color.White);
            }


            // Draw game messages
            float start = 400 - (Messages.Count * 14);
            foreach (GameMessage message in Messages) {
                // This needs to be changed when we get multiple players
                // going.

                Vector2 pos = new Vector2(50, start);
                start += MeasureString(BigFont, message.Message).Y;
                DrawString(BigFont, message.Message, pos);
            }

            // Put on the cursor
            Batch.Draw(Cursor, new Vector2(ms.X, ms.Y), Color.White);
            Batch.End();
        }

        /// <summary>
        /// Customized string drawing routine to create a centered string.
        /// </summary>
        /// <param name="font">The font of the text</param>
        /// <param name="text">The text to be written. See DrawString notes for usage.</param>
        /// <param name="pos">The position at which to draw</param>
        public static void DrawCenteredString(SpriteFont font, string text, Vector2 pos) {
            string[] lines = text.Split(new string[] { "|n" }, StringSplitOptions.None);
            Vector2 size = MeasureString(font, text);
            pos.Y -= (int)(size.Y / 2);
            float height = font.MeasureString("A").Y;

            foreach (string line in lines) {
                size = MeasureString(font, line);
                DrawString(font, line, new Vector2(pos.X - (int)(size.X / 2), pos.Y));
                pos.Y += height;
            }
        }

        /// <summary>
        /// A customized string drawing routine. Has support for colors and newlines.
        /// </summary>
        /// <param name="font">The font of the text.</param>
        /// <param name="text">The text to be written. The following commands exist:
        /// |cRRGGBB sets the current color to RRGGBB
        /// |r resets the formatting
        /// |n creates a newline</param>
        /// <param name="spos"></param>
        public static void DrawString(SpriteFont font, string text, Vector2 spos) {
            Color color = Color.White;
            Vector2 pos = spos;
            string[] bits = text.Split('|');
            float height = font.MeasureString("A").Y;

            foreach (string bit in bits) {
                string write = bit;
                if (!text.StartsWith(bit)) {
                    if (bit[0] == 'c') {
                        byte r = Convert.ToByte(bit.Substring(1, 2), 16);
                        byte g = Convert.ToByte(bit.Substring(3, 2), 16);
                        byte b = Convert.ToByte(bit.Substring(5, 2), 16);
                        color = new Color(r, g, b);
                        write = bit.Substring(7);
                    } else if (bit[0] == 'r') {
                        color = Color.White;
                        write = bit.Substring(1);
                    } else if (bit[0] == 'n') {
                        pos.X = spos.X;
                        pos.Y += height;
                        write = bit.Substring(1);
                    }
                }
                Batch.DrawString(font, write, pos, color);
                pos.X += font.MeasureString(write).X;
            }
        }

        /// <summary>
        /// Measures the size of a string.
        /// </summary>
        /// <param name="font">The font to be measured</param>
        /// <param name="text">The text to be written.</param>
        /// <returns>The size of the text.</returns>
        public static Vector2 MeasureString(SpriteFont font, string text) {
            Vector2 pos = Vector2.Zero;
            float max = 0f;
            string[] bits = text.Split('|');
            float height = font.MeasureString("A").Y;

            foreach (string bit in bits) {
                string write = bit;
                if (!text.StartsWith(bit)) {
                    if (bit[0] == 'c')
                        write = bit.Substring(7);
                    else if (bit[0] == 'r')
                        write = bit.Substring(1);
                    else if (bit[0] == 'n') {
                        if (pos.X > max)
                            max = pos.X;
                        pos.X = 0;
                        pos.Y += height;
                        write = bit.Substring(1);
                    }
                }
                pos.X += font.MeasureString(write).X;
            }

            if (pos.X > max)
                max = pos.X;

            return new Vector2(max, pos.Y + height);
        }

        /// <summary>
        /// Returns a word wrapped version of the string.
        /// </summary>
        /// <param name="font">The font to measure with</param>
        /// <param name="text">The text to be wrapped</param>
        /// <param name="intended">The intended width of the wrap.</param>
        /// <returns>The word wrapped string</returns>
        public static string WordWrap(SpriteFont font, string text, int intended) {
            text = text.Replace("|n", " |n ");
            string[] words = text.Split(' ');
            string final = "";
            float width = 0f;
            float space = MeasureString(font, " ").X;

            foreach (string word in words) {
                if (word == "|n") {
                    width = 0f;
                    final += "|n";
                } else {
                    Vector2 size = MeasureString(font, word);
                    if ((int)(width + size.X) > intended) {
                        width = 0f;
                        final += "|n" + word + " ";
                    } else {
                        final += word + " ";
                        width += size.X + space;
                    }
                }
            }

            return final;
        }

        /// <summary>
        /// Draws a cooldown circle
        /// </summary>
        /// <param name="texture">The texture to be drawn upon</param>
        /// <param name="complete">The percent complete of the cooldown (0 - 1)</param>
        public void GetCooldownCircle(Texture2D texture, float complete) {
            CooldownEffect.SetCurrentTechnique(CooldownEffectTechniques.Cooldown);
            CooldownEffect.Complete = complete;

            RenderTarget target = GraphicsDevice.GetRenderTarget(0);
            GraphicsDevice.SetRenderTarget(0, CooldownTarget);
            GraphicsDevice.Clear(Color.TransparentBlack);

            CooldownEffect.BaseEffect.Begin(SaveStateMode.SaveState);
            foreach (EffectPass pass in CooldownEffect.BaseEffect.CurrentTechnique.Passes) {
                pass.Begin();
                foreach (ModelMesh mesh in CooldownCircle.Meshes) {
                    if (!mesh.Name.StartsWith("transform")) {
                        foreach (ModelMeshPart meshpart in mesh.MeshParts) {
                            GraphicsDevice.VertexDeclaration = meshpart.VertexDeclaration;
                            GraphicsDevice.Vertices[0].SetSource(mesh.VertexBuffer, meshpart.StreamOffset, meshpart.VertexStride);
                            GraphicsDevice.Indices = mesh.IndexBuffer;
                            GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, meshpart.BaseVertex, 0, meshpart.NumVertices, meshpart.StartIndex, meshpart.PrimitiveCount);
                        }
                    }
                }

                pass.End();
            }
            CooldownEffect.BaseEffect.End();

            GraphicsDevice.SetRenderTarget(0, target as RenderTarget2D);

            // Blit the render target onto the texture
            int[] data = new int[36 * 36];
            CooldownTarget.GetTexture().GetData<int>(data);
            texture.SetData<int>(data);
        }
    }
}
