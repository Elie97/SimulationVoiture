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
        VertexPositionColor[] Sommets { get; set; }
        Vector3 Étendue { get; set; }
        Vector3 Charpente { get; set; }
        Vector3 Origine { get; set; }
        Vector3 Delta { get; set; }
        Vector3[] PtsSommets { get; set; }
        int Hauteur { get; set; }
        int Largeur { get; set; }
        Color Couleur { get; set; }

        BasicEffect EffetDeBase { get; set; }
        RessourcesManager<Texture2D> GestionnaireDeTextures { get; set; }

        public Sol(Game jeu, float homothétieInitiale, Vector3 rotationInitiale, Vector3 positionInitiale,
                       Vector3 étendue, Vector3 charpente, float intervalleMAJ, Color couleur)
            : base(jeu, homothétieInitiale, rotationInitiale, positionInitiale)
        {
            Étendue = étendue;
            Charpente = charpente;
            Couleur = couleur;
        }

        public override void Initialize()
        {
            GestionnaireDeTextures = Game.Services.GetService(typeof(RessourcesManager<Texture2D>)) as RessourcesManager<Texture2D>;
            Origine = new Vector3(-Charpente.X / 2, 0, Charpente.Z / 2);
            Hauteur = (int)Charpente.X;
            Largeur = (int)Charpente.Z;
            Delta = new Vector3(Hauteur / Étendue.X, 0, Largeur / Étendue.Z);
            PtsSommets = new Vector3[Hauteur * Largeur];
            Sommets = new VertexPositionColor[6];
            NbTriangles = Hauteur * Largeur * 2;
            CréerTableauPoints();
            InitialiserSommets();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            base.LoadContent();
            EffetDeBase = new BasicEffect(GraphicsDevice);
            EffetDeBase.VertexColorEnabled = true;
        }

        private void CréerTableauPoints()
        {
            //for (int j = 0; j < Largeur; j++)
            //{
            //    for (int i = 0; i < Hauteur; i++)
            //    {
            //        PtsSommets[i + j * Hauteur] = new Vector3(Origine.X + Delta.X * i, 0, (Origine.Z - Delta.Z * j));
            //    }
            //}
            PtsSommets[0] = new Vector3(Origine.X + Delta.X * 0, 0, (Origine.Z - Delta.Z * 0));
            PtsSommets[1] = new Vector3(Origine.X + Delta.X * 1, 0, (Origine.Z - Delta.Z * 0));
            PtsSommets[2] = new Vector3(Origine.X + Delta.X * 0, 0, (Origine.Z - Delta.Z * 1));
            PtsSommets[3] = new Vector3(Origine.X + Delta.X * 1, 0, (Origine.Z - Delta.Z * 1));


        }

        protected override void InitialiserSommets()
        {
            //int NoSommet = -1;
            //for (int j = 0; j < Largeur - 1; j++)
            //{
            //    for (int i = 0; i < Hauteur - 1; i++)
            //    {
            //        Sommets[++NoSommet] = new VertexPositionColor(PtsSommets[(i) + j * Hauteur], Color.White);
            //        Sommets[++NoSommet] = new VertexPositionColor(PtsSommets[(i) + (j + 1) * Hauteur], Color.White);
            //        Sommets[++NoSommet] = new VertexPositionColor(PtsSommets[(i + 1) + j * Hauteur], Color.White);
            //        Sommets[++NoSommet] = new VertexPositionColor(PtsSommets[(i + 1) + j * Hauteur], Color.White);
            //        Sommets[++NoSommet] = new VertexPositionColor(PtsSommets[(i) + (j + 1) * Hauteur], Color.White);
            //        Sommets[++NoSommet] = new VertexPositionColor(PtsSommets[(i + 1) + (j + 1) * Hauteur], Color.White);

            //    }
            //}
            Sommets[0] = new VertexPositionColor(PtsSommets[0], Couleur);
            Sommets[1] = new VertexPositionColor(PtsSommets[2], Couleur);
            Sommets[2] = new VertexPositionColor(PtsSommets[1], Couleur);
            Sommets[3] = new VertexPositionColor(PtsSommets[1], Couleur);
            Sommets[4] = new VertexPositionColor(PtsSommets[2], Couleur);
            Sommets[5] = new VertexPositionColor(PtsSommets[3], Couleur);

        }

        public override void Draw(GameTime gameTime)
        {
            EffetDeBase.World = GetMonde();
            EffetDeBase.View = CaméraJeu.Vue;
            EffetDeBase.Projection = CaméraJeu.Projection;
            foreach (EffectPass passeEffet in EffetDeBase.CurrentTechnique.Passes)
            {
                passeEffet.Apply();
                GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.TriangleList, Sommets, 0, 2);
            }
            base.Draw(gameTime);
        }
    }
}
