using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using LeagueEngine.Effects;

namespace LeagueEngine {
    /// <summary>
    /// Describes a model to be drawn
    /// </summary>
    public class GameModel {
        /// <summary>
        /// The internal XNA model
        /// </summary>
        public Model Model;
        
        /// <summary>
        /// Creates a GameModel
        /// </summary>
        /// <param name="model">The internal XNA model</param>
        public GameModel(Model model) {
            Model = model;
        }

        /// <summary>
        /// Draws selection circles and regions
        /// </summary>
        /// <param name="world">The world of the model</param>
        /// <param name="c">The color of the model</param>
        public void Draw(Matrix world, Color c) {
            Map.FogOfWarEffect.SetCurrentTechnique(fowEffectTechniques.Selection);
            League.Engine.GraphicsDevice.RenderState.AlphaBlendEnable = true;
            League.Engine.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
            League.Engine.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
            League.Engine.GraphicsDevice.RenderState.CullMode = CullMode.CullCounterClockwiseFace;

            Map.FogOfWarEffect.Color = c.ToVector3();
            Map.FogOfWarEffect.View = League.view;
            Map.FogOfWarEffect.Proj = League.projection;
            Map.FogOfWarEffect.World = world;

            Map.FogOfWarEffect.BaseEffect.Begin(SaveStateMode.SaveState);
            foreach (EffectPass pass in Map.FogOfWarEffect.BaseEffect.CurrentTechnique.Passes) {
                pass.Begin();
                foreach (ModelMesh mesh in Model.Meshes) {
                    if (!mesh.Name.StartsWith("transform")) {
                        foreach (ModelMeshPart meshpart in mesh.MeshParts) {
                            League.Engine.GraphicsDevice.VertexDeclaration = meshpart.VertexDeclaration;
                            League.Engine.GraphicsDevice.Vertices[0].SetSource(mesh.VertexBuffer, meshpart.StreamOffset, meshpart.VertexStride);
                            League.Engine.GraphicsDevice.Indices = mesh.IndexBuffer;
                            League.Engine.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, meshpart.BaseVertex, 0, meshpart.NumVertices, meshpart.StartIndex, meshpart.PrimitiveCount);
                        }
                    }
                }

                pass.End();
            }
            Map.FogOfWarEffect.BaseEffect.End();
        }

        /// <summary>
        /// Draws a GameObject
        /// </summary>
        /// <param name="u">The GameObject to be drawn.</param>
        public void Draw(GameObject u) {
            Map.FogOfWarEffect.SetCurrentTechnique(fowEffectTechniques.Unit);
            League.Engine.GraphicsDevice.RenderState.AlphaBlendEnable = true;
            League.Engine.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
            League.Engine.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
            League.Engine.GraphicsDevice.RenderState.CullMode = CullMode.CullCounterClockwiseFace;

            // Get the unit's position on the FOW map
            int x = (int)(u.Position.X + (u.Engine.CurrentMap.Width / 2.0f) * (512f / u.Engine.CurrentMap.Width));
            int y = (int)(u.Position.Y + (u.Engine.CurrentMap.Height / 2.0f) * (512f / u.Engine.CurrentMap.Height));

            Map.FogOfWarEffect.Color = ((u is Unit) ? (Unit)u : ((Projectile)u).Attacker).Owner.TeamColor.ToVector3();

            Map.FogOfWarEffect.View = League.view;
            Map.FogOfWarEffect.Proj = League.projection;
            float visib = Player.CurrentPlayer.FogMapPixels[y * 512 + x].ToVector3().X;
            Map.FogOfWarEffect.Visibility = Player.CurrentPlayer.FogMapPixels[y * 512 + x].ToVector3().X;
            Map.FogOfWarEffect.FOWTexture = Player.CurrentPlayer.FogMap.GetTexture();
            Map.FogOfWarEffect.World = u.GetTransformation();
            Map.FogOfWarEffect.UnitTexture = (u is Unit) ? ((Unit)u).Type.Skin : Projectile.Skin;

            Map.FogOfWarEffect.BaseEffect.Begin(SaveStateMode.SaveState);
            foreach (EffectPass pass in Map.FogOfWarEffect.BaseEffect.CurrentTechnique.Passes) {
                pass.Begin();
                foreach (ModelMesh mesh in Model.Meshes) {
                    if (!mesh.Name.StartsWith("transform")) {
                        foreach (ModelMeshPart meshpart in mesh.MeshParts) {
                            League.Engine.GraphicsDevice.VertexDeclaration = meshpart.VertexDeclaration;
                            League.Engine.GraphicsDevice.Vertices[0].SetSource(mesh.VertexBuffer, meshpart.StreamOffset, meshpart.VertexStride);
                            League.Engine.GraphicsDevice.Indices = mesh.IndexBuffer;
                            League.Engine.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, meshpart.BaseVertex, 0, meshpart.NumVertices, meshpart.StartIndex, meshpart.PrimitiveCount);
                        }
                    }
                }

                pass.End();
            }
            Map.FogOfWarEffect.BaseEffect.End();
        }
    }
}
