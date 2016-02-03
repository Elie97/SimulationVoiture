using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


namespace SimulationVéhicule
{
    public class Voiture : Microsoft.Xna.Framework.DrawableGameComponent
    {
        const float DÉCÉLÉRATION = 0.002f;// en décimètre
        const float VITESSE_MAX = 100.0f;// Km/H
        const float ACCÉLÉRATION = 4.0f;// Temps en seconde pour atteindre 100 km/h
        const float MASSE_VOITURE = 1000.0f;// En Kilogramme


        string NomModèle { get; set; }
        RessourcesManager<Model> GestionnaireDeModèles { get; set; }
        Caméra CaméraJeu { get; set; }
        protected float Échelle { get; set; }
        protected float ÉchelleInitiale { get; set; }
        protected Vector3 Rotation { get; set; }
        protected Vector3 RotationInitiale { get; set; }
        public Vector3 Position { get; set; }
        protected Model Modèle { get; private set; }
        protected Matrix[] TransformationsModèle { get; private set; }
        protected Matrix Monde { get; set; }
        float IntervalleMAJ { get; set; }
        float TempsÉcouléDepuisMAJ { get; set; }
        InputManager GestionInput { get; set; }
        bool D1Actif { get; set; }
        bool D2Actif { get; set; }
        bool D3Actif { get; set; }
        public float Vitesse { get; set; }
        int CPT { get; set; }

        public Voiture(Game jeu, String nomModèle, float échelleInitiale, Vector3 rotationInitiale, Vector3 positionInitiale, float intervalleMAJ)
            : base(jeu)
        {
            NomModèle = nomModèle;
            Position = positionInitiale;
            ÉchelleInitiale = échelleInitiale;
            Échelle = échelleInitiale;
            Rotation = rotationInitiale;
            RotationInitiale = rotationInitiale;
            IntervalleMAJ = intervalleMAJ;
        }

        public override void Initialize()
        {
            CalculerMonde();
            D1Actif = false;
            D2Actif = false;
            D3Actif = false;
            Vitesse = 0;
            CPT = 0;
            base.Initialize();
        }

        private void CalculerMonde()
        {
            Monde = Matrix.Identity;
            Monde *= Matrix.CreateScale(Échelle);
            Monde *= Matrix.CreateFromYawPitchRoll(Rotation.Y, Rotation.X, Rotation.Z);
            Monde *= Matrix.CreateTranslation(Position);
        }

        protected override void LoadContent()
        {
            CaméraJeu = Game.Services.GetService(typeof(Caméra)) as Caméra;
            GestionnaireDeModèles = Game.Services.GetService(typeof(RessourcesManager<Model>)) as RessourcesManager<Model>;
            Modèle = GestionnaireDeModèles.Find(NomModèle);
            TransformationsModèle = new Matrix[Modèle.Bones.Count];
            Modèle.CopyAbsoluteBoneTransformsTo(TransformationsModèle);
            GestionInput = Game.Services.GetService(typeof(InputManager)) as InputManager;
        }

        public override void Draw(GameTime gameTime)
        {
            foreach (ModelMesh maille in Modèle.Meshes)
            {
                Game.Window.Title = Convert.ToInt32(PixelToKMH(Vitesse)).ToString();
                Matrix mondeLocal = TransformationsModèle[maille.ParentBone.Index] * GetMonde();
                foreach (ModelMeshPart portionDeMaillage in maille.MeshParts)
                {
                    BasicEffect effet = (BasicEffect)portionDeMaillage.Effect;
                    effet.EnableDefaultLighting();
                    effet.Projection = CaméraJeu.Projection;
                    effet.View = CaméraJeu.Vue;
                    effet.World = mondeLocal;
                }
                maille.Draw();
            }
        }

        public override void Update(GameTime gameTime)
        {
            D1Actif = GestionToucheActive(Keys.D1, D1Actif);
            D2Actif = GestionToucheActive(Keys.D2, D2Actif);
            D3Actif = GestionToucheActive(Keys.D3, D3Actif);

            TempsÉcouléDepuisMAJ += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (TempsÉcouléDepuisMAJ >= IntervalleMAJ)
            {
                //if (D2Actif)
                //{
                //    Rotation = new Vector3(Rotation.X + 0.05f, Rotation.Y, Rotation.Z);//Pas cette valeur, check msg colnet ;)
                //}
                //if (D1Actif)
                //{
                //    Rotation = new Vector3(Rotation.X, Rotation.Y + 0.05f, Rotation.Z);
                //}
                //if (D3Actif)
                //{
                //    Rotation = new Vector3(Rotation.X, Rotation.Y, Rotation.Z + 0.05f);
                //}

                //if (GestionInput.EstEnfoncée(Keys.Space))
                //{
                //    Rotation = RotationInitiale;
                //}
                //if (GestionInput.EstEnfoncée(Keys.Add))
                //{
                //    Échelle += 0.01f;
                //}
                //if (GestionInput.EstEnfoncée(Keys.Subtract))
                //{
                //    Échelle -= 0.01f;
                //    if (Échelle <= 0)
                //    {
                //        Échelle = ÉchelleInitiale;
                //    }
                //}
                // GestionVitesse();
                //if (GestionInput.EstEnfoncée(Keys.W))
                //{
                //    Avance();
                //}
                //if (GestionInput.EstEnfoncée(Keys.S))
                //{
                //    Recule();
                //}
                if (GestionInput.EstEnfoncée(Keys.W))
                {
                    Accélération();
                }
                else
                {
                    Décélération();
                }
                Avance();
                CalculerMonde();
                TempsÉcouléDepuisMAJ = 0;
            }
            base.Update(gameTime);
        }

        public virtual Matrix GetMonde()
        {
            return Monde;
        }

        public bool GestionToucheActive(Keys key, bool estActif)
        {
            if (GestionInput.EstNouvelleTouche(key))
            {
                estActif = !estActif;
            }
            return estActif;
        }

        public void Avance()
        {
            //Vitesse += 1;
            Position = new Vector3(Position.X, Position.Y, Position.Z + Vitesse);
        }
        public void Recule()
        {
            Position = new Vector3(Position.X, Position.Y, Position.Z - Vitesse);
        }

        void Décélération()
        {
            if (Vitesse > 0)
            {
                Vitesse -= DÉCÉLÉRATION;//mettons
            }
            else
            {
                Vitesse = 0;
            }
        }

        void Accélération()
        {
            if (Vitesse < KMHtoPixel(VITESSE_MAX))
            {
                Vitesse += ((KMHtoPixel(VITESSE_MAX) / ACCÉLÉRATION) / 60f);
            }
            else
            {
                Vitesse = KMHtoPixel(VITESSE_MAX);
            }
        }

        float KMHtoPixel(float kmh)
        {
            return (kmh / 3.6f) / 6;
        }

        float PixelToKMH(float pixel)
        {
            return (pixel * 6) * 3.6f;
        }
    }
}
