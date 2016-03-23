using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace SimulationVéhicule
{
    public class Terrain : PrimitiveDeBaseAnimée
    {
        const int NB_TRIANGLES_PAR_TUILE = 2;
        const int NB_SOMMETS_PAR_TRIANGLE = 3;
        const int NB_SOMMETS_PAR_TUILE = 4;
        const float MAX_COULEUR = 255f;

        private Texture2D CarteTerrain { get; set; }
        private Color[] DataTexture { get; set; }
        private Vector2 DeltaCarte { get; set; }
        private Vector2 DeltaTexture { get; set; }
        private float ÉcartTexture { get; set; }
        private Vector3 Étendue { get; set; }

        private Matrix monde { get; set; }
        private BasicEffect EffetDeBase { get; set; }

        private RessourcesManager<Texture2D> GestionnaireDeTextures { get; set; }

        private int NbColonnes { get; set; }
        private int NbNiveauTexture { get; set; }
        private int NbRangées { get; set; }

        private string NomCarteTerrain { get; set; }
        private string NomTextureTerrain { get; set; }

        private Vector3 Origine { get; set; }
        private Vector3[,] PtsSommets { get; set; }
        private VertexPositionTexture[] TabSommets { get; set; }
        private Texture2D TextureTerrain { get; set; }

        private List<Vector3> ListePoints { get; set; }

        float PositionRelativeX { get; set; }
        float PositionRelativeZ { get; set; }
        float PositionRelativeY { get; set; }
        Model Ciel { get; set; }
        Matrix[] TransformationsModèle { get; set; }
        Texture2D TextureCiel { get; set; }
        RessourcesManager<Model> GestionnaireDeModèles { get; set; }
        GraphicsDevice Device { get; set; }


        public Terrain(Game jeu, float homothétieInitiale, Vector3 rotationInitiale, Vector3 positionInitiale,
                       Vector3 étendue, string nomCarteTerrain, string nomTextureTerrain, int nbNiveauxTexture, float intervalleMAJ, Caméra caméraJeu)
            : base(jeu, homothétieInitiale, rotationInitiale, positionInitiale, intervalleMAJ)
        {
            Étendue = étendue;
            NomCarteTerrain = nomCarteTerrain;
            NomTextureTerrain = nomTextureTerrain;
            NbNiveauTexture = nbNiveauxTexture;
            //CaméraJeu = caméraJeu;
        }

        public override void Initialize()
        {
            GestionnaireDeTextures = Game.Services.GetService(typeof(RessourcesManager<Texture2D>)) as RessourcesManager<Texture2D>;
            Device = Game.Services.GetService(typeof(GraphicsDevice)) as GraphicsDevice;
            InitialiserDonnéesCarte();
            InitialiserDonnéesTexture();
            Origine = new Vector3(-Étendue.X / 2, 0, Étendue.Z / 2); //pour centrer la primitive au point (0,0,0)
            AllouerTableaux();
            CréerTableauPoints();
            base.Initialize();
        }


        void InitialiserDonnéesCarte()
        {
            CarteTerrain = GestionnaireDeTextures.Find(NomCarteTerrain); //Trouver le terrain
            NbColonnes = CarteTerrain.Width - 1;
            NbRangées = CarteTerrain.Height - 1;
            DataTexture = new Color[CarteTerrain.Width * CarteTerrain.Height];
            CarteTerrain.GetData<Color>(DataTexture);
            DeltaCarte = new Vector2(Étendue.X / (float)NbColonnes, Étendue.Z / (float)NbRangées);
            NbTriangles = NbColonnes * NbRangées * 2;
            NbSommets = NbColonnes * NbRangées * 6; //*2 pour le nb de triangles et *3 pour le nombre de sommets
        }

        void InitialiserDonnéesTexture()
        {
            TextureTerrain = this.GestionnaireDeTextures.Find(this.NomTextureTerrain);
            DeltaTexture = new Vector2((float)TextureTerrain.Width, (float)TextureTerrain.Height / Étendue.Y / (float)NbNiveauTexture);
            ÉcartTexture = 1f / (float)NbNiveauTexture;
        }

        //
        // Allocation des deux tableaux
        //    1) celui contenant les points de sommet (les points uniques), 
        //    2) celui contenant les sommets servant à dessiner les triangles
        //
        void AllouerTableaux()
        {
            PtsSommets = new Vector3[NbColonnes + 1, NbRangées + 1];
            TabSommets = new VertexPositionTexture[NbSommets];
        }

        protected override void LoadContent()
        {
            base.LoadContent();
            EffetDeBase = new BasicEffect(GraphicsDevice);
            InitialiserParamètresEffetDeBase();
            monde = GetMonde();
            GestionnaireDeModèles = Game.Services.GetService(typeof(RessourcesManager<Model>)) as RessourcesManager<Model>;
            Ciel = GestionnaireDeModèles.Find("Ciel2");
            TransformationsModèle = new Matrix[Ciel.Bones.Count];
            Ciel.CopyAbsoluteBoneTransformsTo(TransformationsModèle);
            //TextureCiel = GestionnaireDeModèles.Find("cloudMap");
            //Ciel.Meshes[0].MeshParts[0].Effect = EffetDeBase.Clone()
        }

        void InitialiserParamètresEffetDeBase()
        {
            EffetDeBase.TextureEnabled = true;
            EffetDeBase.Texture = TextureTerrain;
        }

        private void CréerTableauPoints()
        {
            for (int i = 0; i <= NbColonnes; i++)
            {
                for (int j = 0; j <= NbRangées; j++)
                {
                    PtsSommets[i, j] = new Vector3(Origine.X + (float)i * DeltaCarte.X, TrouverHauteur(i, NbRangées - j) * 5f, Origine.Z - (float)j * DeltaCarte.Y);

                }
            }
        }

        public float GetHauteur(Vector3 positionVoiture)
        {
            for (int i = 0; i < NbColonnes; i++)
            {
                if ((int)positionVoiture.X == (int)PtsSommets[i, 0].X)
                {
                    PositionRelativeX = PtsSommets[i, 0].X;
                }
            }
            for (int j = 0; j < NbRangées; j++)
            {
                if ((int)positionVoiture.Z == (int)PtsSommets[0, j].Z)
                {
                    PositionRelativeZ = PtsSommets[0, j].Z;
                }
            }

            for (int i = 0; i < NbColonnes; i++)
            {
                for (int j = 0; j < NbRangées; j++)
                {
                    if (PositionRelativeX == (int)PtsSommets[i, j].X && PositionRelativeZ == (int)PtsSommets[i, j].Z)
                    {
                        PositionRelativeY = PtsSommets[i, j].Y;
                    }
                }
            }
            return PositionRelativeY;
        }


        private float TrouverHauteur(int compteurColonnes, int nbRangéesRestantes)
        {
            int i = compteurColonnes + (NbRangées + 1) * nbRangéesRestantes;
            return MathHelper.Max((float)DataTexture[i].R / 255f * Étendue.Y, Étendue.Y / (float)NbNiveauTexture);
        }

        //
        // Création des sommets.
        // N'oubliez pas qu'il s'agit d'un TriangleList...
        //
        protected override void InitialiserSommets()
        {
            float margeDétailsTexture = (Étendue.Y / (float)NbNiveauTexture) + 1;
            int cpt = 0;
            for (int i = 0; i < NbRangées; i++)
            {
                for (int j = 0; j < NbColonnes; j++)
                {
                    float positionY = (PtsSommets[j, i].Y + PtsSommets[j + 1, i].Y + PtsSommets[j, i + 1].Y) / 3f;
                    float écartTexture = (float)((int)(positionY / margeDétailsTexture)) * ÉcartTexture;

                    int cpt1 = cpt;
                    cpt = cpt1 + 1;
                    TabSommets[cpt1] = new VertexPositionTexture(PtsSommets[j, i], new Vector2(0f, écartTexture + ÉcartTexture));

                    int cpt2 = cpt;
                    cpt = cpt2 + 1;
                    TabSommets[cpt2] = new VertexPositionTexture(PtsSommets[j, i + 1], new Vector2(0f, écartTexture));

                    int cpt3 = cpt;
                    cpt = cpt3 + 1;
                    TabSommets[cpt3] = new VertexPositionTexture(PtsSommets[j + 1, i], new Vector2(1f, écartTexture + ÉcartTexture));

                    positionY = (PtsSommets[j + 1, i].Y + PtsSommets[j, i + 1].Y + PtsSommets[j + 1, i + 1].Y) / 3f;
                    écartTexture = (float)((int)(positionY / margeDétailsTexture)) * ÉcartTexture;

                    int cpt4 = cpt;
                    cpt = cpt4 + 1;
                    TabSommets[cpt4] = new VertexPositionTexture(PtsSommets[j, i + 1], new Vector2(0f, écartTexture));

                    int cpt5 = cpt;
                    cpt = cpt5 + 1;
                    TabSommets[cpt5] = new VertexPositionTexture(PtsSommets[j + 1, i + 1], new Vector2(1f, écartTexture));

                    int cpt6 = cpt;
                    cpt = cpt6 + 1;
                    TabSommets[cpt6] = new VertexPositionTexture(PtsSommets[j + 1, i], new Vector2(1f, écartTexture + ÉcartTexture));
                }
            }
        }

        //
        // Deviner ce que fait cette méthode...
        //
        public override void Draw(GameTime gameTime)
        {
            EffetDeBase.World = monde;
            EffetDeBase.View = CaméraJeu.Vue;
            EffetDeBase.Projection = CaméraJeu.Projection;

            foreach (ModelMesh Mesh in Ciel.Meshes)
            {
                foreach (BasicEffect Effect in Mesh.Effects)
                {
                    Effect.Projection = CaméraJeu.Projection;
                    Effect.View = CaméraJeu.Vue;
                    Effect.World = TransformationsModèle[Mesh.ParentBone.Index] * Matrix.CreateScale(10000/2) * Matrix.CreateTranslation(CaméraJeu.Position);
                }
                Mesh.Draw();
            }

            foreach (EffectPass pass in EffetDeBase.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawUserPrimitives<VertexPositionTexture>(PrimitiveType.TriangleList, TabSommets, 0, NbTriangles);
            }

            //Device.Ren   
        }

    }
}