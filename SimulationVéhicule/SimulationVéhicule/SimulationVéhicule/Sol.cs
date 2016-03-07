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
    public class Sol : PrimitiveDeBase
    {
        Vector2 Étendue { get; set; }
        Vector2 Charpente { get; set; }
        int NbRangées { get; set; }
        int NbColonnes { get; set; }
        String NomTexture { get; set; }
        float Largeur { get; set; }
        float Hauteur { get; set; }
        Vector2 Delta { get; set; }
        Vector2[,] PtsTexture { get; set; }
        Vector3[,] PtsSommets { get; set; }
        VertexPositionTexture[] Sommets { get; set; }
        Texture2D LaTexture { get; set; }
        BasicEffect EffetDeBase { get; set; }
        Vector3 Origine { get; set; }
        RessourcesManager<Texture2D> GestionnaireDeTextures { get; set; }
        Vector3[] PointsBox { get; set; }
        Vector3[] PointsÉtape { get; set; }
        Vector3[] PointBoxLatéralGauche { get; set; }
        Vector3[] PointBoxLatéralDroit { get; set; }

        float Rayon { get; set; }
        float Angle { get; set; }
        bool Courbe { get; set; }
        float AngleCourbe { get; set; }
        float SensCourbe { get; set; }

        public bool Franchi { get; set; }

        public BoundingBox Box 
        { 
            get 
            {
                return BoundingBox.CreateFromPoints(PointsBox); 
            } 
        }

        public BoundingBox BoxÉtape
        {
            get
            {
                return BoundingBox.CreateFromPoints(PointsÉtape);
            }
        }

        //public BoundingBox BoxDroite
        //{
        //    get
        //    {
        //        return BoundingBox.CreateFromPoints(PointBoxLatéralDroit);
        //    }
        //}

        //public BoundingBox BoxGauche
        //{
        //    get
        //    {
        //        return BoundingBox.CreateFromPoints(PointBoxLatéralGauche);
        //    }
        //}

        float PositionRelativeX { get; set; }
        float PositionRelativeZ { get; set; }
        float PositionRelativeY { get; set; }

        public Sol(Game jeu, float échelleInitiale, Vector3 rotationInitiale, Vector3 positionInitiale, Vector2 étendue, 
            Vector2 charpente, string nomTexture, float rayon, float angle, bool courbe, float angleCourbe, float sensCourbe)
            : base(jeu, échelleInitiale, rotationInitiale, positionInitiale)
        {
            Étendue = étendue;
            Charpente = charpente;
            NomTexture = nomTexture;
            Rayon = rayon;
            Angle = angle;
            Courbe = courbe;
            AngleCourbe = angleCourbe;
            SensCourbe = sensCourbe;
        }


        public override void Initialize()
        {
            Franchi = false;
            GestionnaireDeTextures = Game.Services.GetService(typeof(RessourcesManager<Texture2D>)) as RessourcesManager<Texture2D>;
            LaTexture = GestionnaireDeTextures.Find(NomTexture);
            Largeur = Étendue.X;
            Hauteur = Étendue.Y;
            NbColonnes = (int)Charpente.X;
            NbRangées = (int)Charpente.Y;
            PointsBox = new Vector3[NbColonnes * NbRangées];
            PointsÉtape = new Vector3[2];
            Delta = new Vector2(Largeur / ((float)NbColonnes - 1), Hauteur / ((float)NbRangées - 1));
            //Origine = new Vector3(Position.X - (Étendue.X / 2) + ((Étendue.X / Charpente.X) / 2), Position.Y, Position.Z);
            Origine = new Vector3(0, 0, 0);
            PtsSommets = new Vector3[NbColonnes, NbRangées];
            PtsTexture = new Vector2[NbColonnes, NbRangées];
            Sommets = new VertexPositionTexture[(NbColonnes) * (NbRangées) * 6];
            NbTriangles = NbColonnes * NbRangées * 2;
            CréerTableauPoints();
            InitialiserDonnéesTexture();
            SetPointBox();
            base.Initialize();
        }

        private void CréerTableauPoints()
        {
            for (int j = 0; j < NbRangées; j++)
            {
                for (int i = 0; i < NbColonnes; i++)
                {
                    PtsSommets[i, j] = new Vector3(Origine.X + Delta.X * i, Origine.Y, Origine.Z - Delta.Y * j);

                }
            }

            if (Courbe)
            {
                //Game.Window.Title = AngleCourbe.ToString();
                for (int j = 0; j < NbRangées; j++)
                {
                    for (int i = 0; i < NbColonnes; i++)
                    {
                        PtsSommets[i, j] = Vector3.Transform(new Vector3(PtsSommets[i, j].X, PtsSommets[i, j].Y, PtsSommets[i, j].Z),
                            Matrix.CreateFromYawPitchRoll((((j) * SensCourbe) / (float)(NbRangées)) * AngleCourbe, 0, 0));
                    }
                }
            }

            for (int j = 0; j < NbRangées; j++)
            {
                for (int i = 0; i < NbColonnes; i++)
                {
                    PtsSommets[i, j] = new Vector3(PtsSommets[i, j].X - (Rayon * (float)Math.Sin(Angle * (j / (float)NbRangées)))
                    , PtsSommets[i, j].Y, PtsSommets[i, j].Z);
                }
            }
        }

        protected override void InitialiserSommets()
        {
            int NoSommet = -1;
            for (int j = 0; j < NbRangées - 1; j++)
            {
                for (int i = 0; i < NbColonnes - 1; i++)
                {
                    Sommets[++NoSommet] = new VertexPositionTexture(PtsSommets[i, j], PtsTexture[i, j]);
                    Sommets[++NoSommet] = new VertexPositionTexture(PtsSommets[i, j + 1], PtsTexture[i, j + 1]);
                    Sommets[++NoSommet] = new VertexPositionTexture(PtsSommets[i + 1, j], PtsTexture[i + 1, j]);
                    Sommets[++NoSommet] = new VertexPositionTexture(PtsSommets[i + 1, j], PtsTexture[i + 1, j]);
                    Sommets[++NoSommet] = new VertexPositionTexture(PtsSommets[i, j + 1], PtsTexture[i, j + 1]);
                    Sommets[++NoSommet] = new VertexPositionTexture(PtsSommets[i + 1, j + 1], PtsTexture[i + 1, j + 1]);
                    //Sommets[++NoSommet] = new VertexPositionColor(PtsSommets[i, j], Color.Red);
                    //Sommets[++NoSommet] = new VertexPositionColor(PtsSommets[i, j + 1], Color.Red);
                    //Sommets[++NoSommet] = new VertexPositionColor(PtsSommets[i + 1, j], Color.Red);
                    //Sommets[++NoSommet] = new VertexPositionColor(PtsSommets[i + 1, j], Color.Red);
                    //Sommets[++NoSommet] = new VertexPositionColor(PtsSommets[i, j + 1], Color.Red);
                    //Sommets[++NoSommet] = new VertexPositionColor(PtsSommets[i + 1, j + 1], Color.Red);
                }
            }
        }

        void InitialiserDonnéesTexture()
        {
            Vector2 ptsTexture;
            ptsTexture = new Vector2(1f / (NbColonnes - 1), 1f / (NbRangées - 1));
            for (int j = 0; j < NbRangées; j++)
            {
                if (Hauteur / (float)NbRangées == 100)
                {
                    if (j % 2 == 0)
                    {
                        ptsTexture = new Vector2(1f / (NbColonnes - 1), 0);
                    }
                    else
                    {
                        ptsTexture = new Vector2(1f / (NbColonnes - 1), 1);
                    }
                }
                for (int i = 0; i < NbColonnes; i++)
                {
                    PtsTexture[i, j] = new Vector2(i * ptsTexture.X, ptsTexture.Y * j);
                }
            }
        }

        protected override void LoadContent()
        {
            base.LoadContent();
            EffetDeBase = new BasicEffect(GraphicsDevice);
            //EffetDeBase.VertexColorEnabled = true;
            EffetDeBase.TextureEnabled = true;
            EffetDeBase.Texture = LaTexture;
        }

        public override void Draw(GameTime gameTime)
        {
            EffetDeBase.World = GetMonde();
            EffetDeBase.View = CaméraJeu.Vue;
            EffetDeBase.Projection = CaméraJeu.Projection;
            foreach (EffectPass passeEffet in EffetDeBase.CurrentTechnique.Passes)
            {
                passeEffet.Apply();
                GraphicsDevice.DrawUserPrimitives<VertexPositionTexture>(PrimitiveType.TriangleList, Sommets, 0, NbTriangles);

            }
            DebugShapeRenderer.AddBoundingBox(Box, Color.Wheat);
            DebugShapeRenderer.AddBoundingBox(BoxÉtape, Color.Green);
            //DebugShapeRenderer.AddBoundingBox(BoxDroite, Color.YellowGreen);
            //DebugShapeRenderer.AddBoundingBox(BoxGauche, Color.YellowGreen);
            //Game.Window.Title = PtsSommets[0, 50].Y.ToString();
            //DebugShapeRenderer.AddBoundingBox(Box, Color.Wheat);
            //DebugShapeRenderer.AddBoundingBox(Box, Color.Blue);
            //DebugShapeRenderer.AddBoundingSphere(Sphere, Color.Yellow);
            //DebugShapeRenderer.AddBoundingSphere(new BoundingSphere(new Vector3(Position.X, Position.Y, Position.Z), (Étendue.X / 2)), Color.Green);
            base.Draw(gameTime);
        }

        public float GetHauteur(Vector3 positionVoiture)
        {
            float hauteur = PositionInitiale.Y;
            if (RotationInitiale.X != 0)
            {
                float hauteurInitiale = Vector3.Transform(PtsSommets[0, 0], Matrix.CreateFromYawPitchRoll(RotationInitiale.Y, RotationInitiale.X, RotationInitiale.Z)).Y;
                float hauteurFinale = Vector3.Transform(PtsSommets[0, NbRangées - 1], Matrix.CreateFromYawPitchRoll(RotationInitiale.Y, RotationInitiale.X, RotationInitiale.Z)).Y;
                float deltaHauteur = (hauteurFinale - hauteurInitiale) / Hauteur;
                float profondeur;

                if (RotationInitiale.Y == 0 || RotationInitiale.Y == MathHelper.Pi)
                {
                    profondeur = Math.Abs(positionVoiture.Z) - Math.Abs(PositionInitiale.Z);
                }
                else
                {
                    profondeur = Math.Abs(positionVoiture.X) - Math.Abs(PositionInitiale.X);
                }

                if (profondeur >= Hauteur)
                {
                    profondeur = Hauteur;
                }
                if (profondeur <= 0)
                {
                    profondeur = 0;
                }

                hauteur = (profondeur * deltaHauteur);
                if (RotationInitiale.X != 0)
                {
                    hauteur += ((profondeur / Hauteur) * (Hauteur * 0.03f));
                }   
            }

            //Game.Window.Title = "I : " + Hauteur + " - F : " + hauteurFinale + " - D : " + deltaHauteur + " - D : " + profondeur + " - H : " + hauteur;

            return hauteur;
        }

        void SetPointBox()
        {
            for (int i = 0; i < NbColonnes; i++)
            {
                for (int j = 0; j < NbRangées; j++)
                {
                    PointsBox[i + (i * j)] = PtsSommets[i, j];
                }
            }
            PointsBox[0] = new Vector3(PointsBox[0].X, PointsBox[0].Y + 3, PointsBox[0].Z);
            Matrix transformation = Matrix.CreateFromYawPitchRoll(RotationInitiale.Y, RotationInitiale.X, 0) * Matrix.CreateTranslation(PositionInitiale);
            for (int i = 0; i < PointsBox.Length; i++)
			{
                PointsBox[i] = Vector3.Transform(PointsBox[i], transformation);
			}


            PointsÉtape[0] = PtsSommets[0, NbRangées - 1];
            PointsÉtape[1] = new Vector3(PtsSommets[NbColonnes - 1, NbRangées - 1].X, PtsSommets[NbColonnes - 1, NbRangées - 1].Y + 200, PtsSommets[NbColonnes - 1, NbRangées - 1].Z);

            for (int i = 0; i < PointsÉtape.Length; i++)
            {
                PointsÉtape[i] = Vector3.Transform(PointsÉtape[i], transformation);
            }

            ////PointBoxLatéralDroit = new Vector3[] { PtsSommets[NbColonnes - 1, 0], 
            //  //  new Vector3(PtsSommets[NbColonnes - 1, NbRangées - 1].X, PtsSommets[NbColonnes - 1, NbRangées - 1].Y + 50, PtsSommets[NbColonnes - 1, NbRangées - 1].Z) };
            //PointBoxLatéralDroit = new Vector3[NbRangées];
            //for (int i = 0; i < NbRangées - 1; i++)
            //{
            //    PointBoxLatéralDroit[i] = Vector3.Transform(PtsSommets[NbColonnes - 1, i], Matrix.CreateFromYawPitchRoll(RotationInitiale.Y, RotationInitiale.X, 0));
            //}
            //for (int i = 0; i < PointBoxLatéralDroit.Length; i++)
            //{
            //    //PointBoxLatéralDroit[i] = Vector3.Transform(PointBoxLatéralDroit[i], Matrix.CreateFromYawPitchRoll(RotationInitiale.Y, 0, 0));
            //    //PointBoxLatéralDroit[i] = Vector3.Transform(PointBoxLatéralDroit[i], Matrix.CreateFromYawPitchRoll(0, 0, RotationInitiale.Y));        
            //}

            //PointBoxLatéralGauche = new Vector3[] { PtsSommets[0, 0], 
            //    new Vector3(PtsSommets[0, NbRangées - 1].X, PtsSommets[0, NbRangées - 1].Y + 50, PtsSommets[0, NbRangées - 1].Z) };

            //for (int i = 0; i < PointBoxLatéralGauche.Length; i++)
            //{
            //   PointBoxLatéralGauche[i] = Vector3.Transform(PointBoxLatéralGauche[i], Matrix.CreateFromYawPitchRoll(RotationInitiale.Y, 0, 0));
            //}
        }

    }
}
