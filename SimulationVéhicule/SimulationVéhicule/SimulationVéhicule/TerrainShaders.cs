using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;


namespace SimulationVéhicule
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class TerrainShaders : PrimitiveDeBaseAnimée
    {
        //GraphicsDeviceManager graphics;
        //SpriteBatch spriteBatch;
        //GraphicsDevice device;
        VertexBuffer myVertexBuffer;
        IndexBuffer myIndexBuffer;

        Effect effect;
        VertexPositionNormalTexture[] terrainVertices;
        Matrix viewMatrix;
        Matrix projectionMatrix;
        int[] indices;

        private float angle = 0f;
        private int terrainWidth = 400;
        private int terrainLength = 300;
        private float[,] heightData;

        Texture2D grassTexture;

        public TerrainShaders(Game jeu, float homothétieInitiale, Vector3 rotationInitiale, Vector3 positionInitiale, float intervalleMAJ)
            : base(jeu, homothétieInitiale, rotationInitiale, positionInitiale, intervalleMAJ)
        {
            //graphics = new GraphicsDeviceManager(this);
            //Content.RootDirectory = "Content";
        }

        public override void Initialize()
        {
            //Window.Title = "Riemer's XNA Tutorials -- 3D Series 1";

            base.Initialize();
        }

        protected override void LoadContent()
        {
            base.LoadContent();
            //spriteBatch = new SpriteBatch(GraphicsDevice);

            effect = Game.Content.Load<Effect>("effects");
            grassTexture = Game.Content.Load<Texture2D>("Textures/DétailsTerrain");

            Texture2D heightMap = Game.Content.Load<Texture2D>("Textures/CarteTest"); LoadHeightData(heightMap);
            //SetUpVertices();
            terrainVertices = SetUpTerrainVertices();
            SetUpIndices();
            CalculateNormals();

            CopyToBuffers();
        }

        protected override void UnloadContent()
        {
        }

        protected override void InitialiserSommets()
        {
        }

        private void SetUpVertices()
        {
            float minHeight = float.MaxValue;
            float maxHeight = float.MinValue;
            for (int x = 0; x < terrainWidth; x++)
            {
                for (int y = 0; y < terrainLength; y++)
                {
                    if (heightData[x, y] < minHeight)
                        minHeight = heightData[x, y];
                    if (heightData[x, y] > maxHeight)
                        maxHeight = heightData[x, y];
                }
            }

            terrainVertices = new VertexPositionNormalTexture[terrainWidth * terrainLength];
            for (int x = 0; x < terrainWidth; x++)
            {
                for (int y = 0; y < terrainLength; y++)
                {
                    terrainVertices[x + y * terrainWidth].Position = new Vector3(x, heightData[x, y], -y);

                    //if (heightData[x, y] < minHeight + (maxHeight - minHeight) / 4)
                    //    vertices[x + y * terrainWidth].Color = Color.Blue;
                    //else if (heightData[x, y] < minHeight + (maxHeight - minHeight) * 2 / 4)
                    //    vertices[x + y * terrainWidth].Color = Color.Green;
                    //else if (heightData[x, y] < minHeight + (maxHeight - minHeight) * 3 / 4)
                    //    vertices[x + y * terrainWidth].Color = Color.Brown;
                    //else
                    //    vertices[x + y * terrainWidth].Color = Color.White;
                }
            }
        }

        private VertexPositionNormalTexture[] SetUpTerrainVertices()
        {
            VertexPositionNormalTexture[] terrainVertices = new VertexPositionNormalTexture[terrainWidth * terrainLength];

            for (int x = 0; x < terrainWidth; x++)
            {
                for (int y = 0; y < terrainLength; y++)
                {
                    terrainVertices[x + y * terrainWidth].Position = new Vector3(x, heightData[x, y], -y);
                    terrainVertices[x + y * terrainWidth].TextureCoordinate.X = (float)x / 30.0f;
                    terrainVertices[x + y * terrainWidth].TextureCoordinate.Y = (float)y / 30.0f;
                }
            }

            return terrainVertices;
        }

        private void SetUpIndices()
        {
            indices = new int[(terrainWidth - 1) * (terrainLength - 1) * 6];
            int counter = 0;
            for (int y = 0; y < terrainLength - 1; y++)
            {
                for (int x = 0; x < terrainWidth - 1; x++)
                {
                    int lowerLeft = x + y * terrainWidth;
                    int lowerRight = (x + 1) + y * terrainWidth;
                    int topLeft = x + (y + 1) * terrainWidth;
                    int topRight = (x + 1) + (y + 1) * terrainWidth;

                    indices[counter++] = topLeft;
                    indices[counter++] = lowerRight;
                    indices[counter++] = lowerLeft;

                    indices[counter++] = topLeft;
                    indices[counter++] = topRight;
                    indices[counter++] = lowerRight;
                }
            }
        }

        private void CalculateNormals()
        {
            for (int i = 0; i < terrainVertices.Length; i++)
                terrainVertices[i].Normal = new Vector3(0, 0, 0);

            for (int i = 0; i < indices.Length / 3; i++)
            {
                int index1 = indices[i * 3];
                int index2 = indices[i * 3 + 1];
                int index3 = indices[i * 3 + 2];

                Vector3 side1 = terrainVertices[index1].Position - terrainVertices[index3].Position;
                Vector3 side2 = terrainVertices[index1].Position - terrainVertices[index2].Position;
                Vector3 normal = Vector3.Cross(side1, side2);

                terrainVertices[index1].Normal += normal;
                terrainVertices[index2].Normal += normal;
                terrainVertices[index3].Normal += normal;
            }

            for (int i = 0; i < terrainVertices.Length; i++)
                terrainVertices[i].Normal.Normalize();
        }


        private void LoadHeightData(Texture2D heightMap)
        {
            terrainWidth = heightMap.Width;
            terrainLength = heightMap.Height;

            Color[] heightMapColors = new Color[terrainWidth * terrainLength];
            heightMap.GetData(heightMapColors);

            heightData = new float[terrainWidth, terrainLength];
            for (int x = 0; x < terrainWidth; x++)
                for (int y = 0; y < terrainLength; y++)
                    heightData[x, y] = heightMapColors[x + y * terrainWidth].R / 5.0f;
        }

        private void SetUpCamera()
        {
            viewMatrix = Matrix.CreateLookAt(new Vector3(60, 80, -80), new Vector3(0, 0, 0), new Vector3(0, 1, 0));
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 1.0f, 300.0f);
        }

        private void CopyToBuffers()
        {
            myVertexBuffer = new VertexBuffer(GraphicsDevice, VertexPositionNormalTexture.VertexDeclaration, terrainVertices.Length, BufferUsage.WriteOnly);
            myVertexBuffer.SetData(terrainVertices);

            myIndexBuffer = new IndexBuffer(GraphicsDevice, typeof(int), indices.Length, BufferUsage.WriteOnly);
            myIndexBuffer.SetData(indices);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            //GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);

            //RasterizerState rs = new RasterizerState();
            //rs.CullMode = CullMode.None;
            //GraphicsDevice.RasterizerState = rs;

            effect.CurrentTechnique = effect.Techniques["Textured"];
            effect.Parameters["xTexture"].SetValue(grassTexture);

            Matrix worldMatrix = Matrix.CreateTranslation(-terrainWidth / 1.0f, 50, terrainLength / 1.0f) * Matrix.CreateRotationY(angle);
            effect.Parameters["Monde"].SetValue(Matrix.Identity);
            effect.Parameters["MatriceVue"].SetValue(viewMatrix);
            effect.Parameters["MatriceProjection"].SetValue(projectionMatrix);

            Vector3 lightDirection = new Vector3(1.0f, -1.0f, -1.0f);
            lightDirection.Normalize();

            effect.Parameters["DirectionLumiere"].SetValue(lightDirection);
            effect.Parameters["LumiereAmbiante"].SetValue(0.1f);
            effect.Parameters["LumiereActive"].SetValue(true);

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, terrainVertices, 0, terrainVertices.Length, indices, 0, indices.Length / 3, VertexPositionNormalTexture.VertexDeclaration);
            }

            base.Draw(gameTime);
        }
    }
}
