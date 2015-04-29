using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using LeagueEngine.Effects;

namespace LeagueEngine {
    /// <summary>
    /// Creates dynamic terrain
    /// </summary>
    public class Terrain {
        /// <summary>
        /// The vertex structure used by our fow shader
        /// </summary>
        public struct VertexMultitextured {
            /// <summary>
            /// POSITION
            /// </summary>
            public Vector3 Position;

            /// <summary>
            /// TEXCOORD0
            /// </summary>
            public Vector2 Texture;

            /// <summary>
            /// TEXCOORD1
            /// </summary>
            public Vector2 Cliff;

            /// <summary>
            /// NORMAL0
            /// </summary>
            public Vector3 Normal;
            
            /// <summary>
            /// The size of this structure
            /// </summary>
            public static int SizeInBytes = (3 + 3 + 2 + 2) * 4;

            /// <summary>
            /// The semantic make-up of our structure
            /// </summary>
            public static VertexElement[] VertexElements = new VertexElement[] {
                new VertexElement( 0, 0, VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.Position, 0 ),
                new VertexElement( 0, sizeof(float) * 3, VertexElementFormat.Vector2, VertexElementMethod.Default, VertexElementUsage.TextureCoordinate, 0 ),
                new VertexElement( 0, sizeof(float) * 5, VertexElementFormat.Vector2, VertexElementMethod.Default, VertexElementUsage.TextureCoordinate, 1 ),
                new VertexElement( 0, sizeof(float) * 7, VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.Normal, 0 ),
             };
        }

        /// <summary>
        /// The tile make-up of our map. Every pixel on the terrain map translates
        /// into one of these.
        /// </summary>
        public struct MetaMapTile {
            /// <summary>
            /// The cliff level of this tile
            /// </summary>
            public int CliffLevel;

            /// <summary>
            /// The heightmap level of this tile
            /// </summary>
            public float Height;
            
            /// <summary>
            /// Is this tile a ramp?
            /// </summary>
            public bool IsRamp;

            /// <summary>
            /// The type of terrain for this tile. Unused as of Alpha 1
            /// </summary>
            public int Type;

            /// <summary>
            /// The height of the cliff in game units
            /// </summary>
            public float CliffHeight { get { return CliffLevel * 16; } }

            /// <summary>
            /// The total height of this tile in game units
            /// </summary>
            public float RealHeight { get { return CliffHeight + Height; } }

            /// <summary>
            /// Creates a new tile
            /// </summary>
            /// <param name="dat">A Vector4 from a terrain map</param>
            public MetaMapTile(Vector4 dat) {
                CliffLevel = (int)(dat.Y  * 255 / 13);
                Height = dat.X / 180f;
                IsRamp = ((int)(dat.Z * 255) & 128) == 128;
                Type = ((int)(dat.Z * 255) & ~128) / 8;
            }

            public static float GetCliffHeight(int level) {
                MetaMapTile tile = new MetaMapTile();
                tile.CliffLevel = level;
                tile.Height = 0;
                return tile.CliffHeight;
            }
        }

        /// <summary>
        /// The index buffer of our terrain
        /// </summary>
        public IndexBuffer IBuffer;

        /// <summary>
        /// The vertex buffer of our terrain
        /// </summary>
        public VertexBuffer VBuffer;

        /// <summary>
        /// The vertices which make up the terrain
        /// </summary>
        public VertexMultitextured[] Vertices;

        /// <summary>
        /// The indices which describe the terrain
        /// </summary>
        public short[] Indices;

        /// <summary>
        /// The tiles of the terrain
        /// </summary>
        public MetaMapTile[,] Tiles;

        /// <summary>
        /// The width in tiles of this terrain
        /// </summary>
        public int Width;

        /// <summary>
        /// The height in tiles of this terrain
        /// </summary>
        public int Height;

        /// <summary>
        /// Creates a new terrain from a terrain map using the default GraphicsDevice
        /// </summary>
        /// <param name="source">The terrain map to use</param>
        public Terrain(Texture2D source) : this(source, League.Engine.GraphicsDevice) {}

        /// <summary>
        /// Creates a new terrain from a terrain map using a specific GraphicsDevice. MapEd
        /// uses this to create Terrain without an instance of League.
        /// </summary>
        /// <param name="source">The terrain map to use</param>
        /// <param name="device">The GraphicsDevice on which to draw</param>
        public Terrain(Texture2D source, GraphicsDevice device) {
            Width = source.Width;
            Height = source.Height;

            Tiles = new MetaMapTile[Width, Height];
            Color[] copy = new Color[Width * Height];
            source.GetData<Color>(copy);

            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                    Tiles[x, y] = new MetaMapTile(copy[y * Width + x].ToVector4());

            Build(device);
        }


        /// <summary>
        /// Builds the terrain mesh
        /// </summary>
        public void Build() {
            Build(League.Engine.GraphicsDevice);
        }


        /// <summary>
        /// Builds the terrain mesh on the specified GraphicsDevice
        /// </summary>
        /// <param name="device">The GraphicsDevice to use</param>
        public void Build(GraphicsDevice device) {
            // TODO: These should not be offsetted...
            List<VertexMultitextured> vertices = new List<VertexMultitextured>();

            for (int y = 0; y < Height; y++) {
                for (int x = 0; x < Width; x++) {
                    Vector3 pos = new Vector3(16 * (x - ((Width - 1) / 2.0f)),
                        Tiles[x, y].RealHeight - 180,
                        16 * (y - ((Height - 1) / 2.0f)));

                    // Create the square of the tile
                    vertices.Add(MakeVertex(pos + new Vector3(-8f, 0, -8f), x, y, 0));
                    vertices.Add(MakeVertex(pos + new Vector3(-8f, 0, 8f), x, y, 1));
                    vertices.Add(MakeVertex(pos + new Vector3(8f, 0, -8f), x, y, 2));
                    vertices.Add(MakeVertex(pos + new Vector3(8f, 0, 8f), x, y, 3));
                }
            }

            List<short> indices = new List<short>();

            for (int y = 0; y < Height - 1; y++) {
                for (int x = 0; x < Width - 1; x++) {
                    // Create the triangles to make the square
                    indices.Add((short)((x + y * Width) * 4 + 0));
                    indices.Add((short)((x + y * Width) * 4 + 2));
                    indices.Add((short)((x + y * Width) * 4 + 3));
                    indices.Add((short)((x + y * Width) * 4 + 0));
                    indices.Add((short)((x + y * Width) * 4 + 3));
                    indices.Add((short)((x + y * Width) * 4 + 1));

                    // There is a cliff on the NS axis
                    if (Tiles[x, y].CliffLevel != Tiles[x, y + 1].CliffLevel && !Tiles[x, y + 1].IsRamp) {
                        indices.Add((short)((x + y * Width) * 4 + 1));
                        indices.Add((short)((x + y * Width) * 4 + 3));
                        indices.Add((short)((x + (y + 1) * Width) * 4 + 2));
                        indices.Add((short)((x + y * Width) * 4 + 1));
                        indices.Add((short)((x + (y + 1) * Width) * 4 + 2));
                        indices.Add((short)((x + (y + 1) * Width) * 4 + 0));
                    }

                    // There is a cliff on the WE axis
                    if (Tiles[x, y].CliffLevel != Tiles[x + 1, y].CliffLevel && !Tiles[x + 1, y].IsRamp) {
                        indices.Add((short)((x + y * Width) * 4 + 2));
                        indices.Add((short)(((x + 1) + y * Width) * 4 + 0));
                        indices.Add((short)(((x + 1) + y * Width) * 4 + 1));
                        indices.Add((short)((x + y * Width) * 4 + 2));
                        indices.Add((short)(((x + 1) + y * Width) * 4 + 1));
                        indices.Add((short)((x + y * Width) * 4 + 3));
                    }
                }
            }

            VBuffer = new VertexBuffer(device, VertexMultitextured.SizeInBytes * vertices.Count, BufferUsage.WriteOnly);
            Vertices = vertices.ToArray();
            VBuffer.SetData<VertexMultitextured>(Vertices);

            IBuffer = new IndexBuffer(device, typeof(short), indices.Count, BufferUsage.WriteOnly);
            Indices = indices.ToArray();
            IBuffer.SetData<short>(Indices);
        }

        /// <summary>
        /// Creates a tile vertex
        /// </summary>
        /// <param name="pos">The position of the tile</param>
        /// <param name="x">The x position of this tile</param>
        /// <param name="y">The y position of this tile.</param>
        /// <param name="v">Which corner of the tile is this vertex?</param>
        /// <returns>The created vertex</returns>
        public VertexMultitextured MakeVertex(Vector3 pos, int x, int y, int v) {
            return MakeVertex(pos, x, y, v, -1);
        }

        /// <summary>
        /// Creates a tile vertex
        /// </summary>
        /// <param name="pos">The position of the tile</param>
        /// <param name="x">The x position of this tile</param>
        /// <param name="y">The y position of this tile.</param>
        /// <param name="v">Which corner of the tile is this vertex?</param>
        /// <param name="w">The height level of this vertex</param>
        /// <returns>The created vertex</returns>
        public VertexMultitextured MakeVertex(Vector3 pos, int x, int y, int v, int w) {
            // Transform the v into an xy coordinate
            // 0  2 = v
            // 1  3
            int xcorner = (v < 2 ? -1 : 1);
            int ycorner = (v < 2 ? v * 2 - 1 : (v - 2) * 2 - 1);

            VertexMultitextured vertex = new VertexMultitextured();
            vertex.Position = pos;
            vertex.Texture = new Vector2(x + xcorner / 2f, y + ycorner / 2f) * 0.03f;

            int here = Tiles[x, y].CliffLevel;
            if (w != -1)
                vertex.Cliff = new Vector2(x, y + w) * 0.03f;
            else
                vertex.Cliff = new Vector2(-1f);
            
            // TODO: Get a real normal for it
            vertex.Normal = Vector3.One;
            return vertex;
        }
    }
}
