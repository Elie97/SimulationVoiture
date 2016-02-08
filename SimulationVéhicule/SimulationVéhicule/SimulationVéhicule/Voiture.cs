using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;


namespace SimulationVéhicule
{
    public class Voiture : Microsoft.Xna.Framework.DrawableGameComponent
    {
        const float DÉCÉLÉRATION = 0.02f;// en décimètre
        const float VITESSE_MAX = 100.0f;// Km/H
        const float VITESSE_MAX_RECULONS = -30.0f;// Km/H
        const float ACCÉLÉRATION = 4.0f;// Temps en seconde pour atteindre 100 km/h
        const float ACCÉLÉRATION_RECULONS = -2.0f;// Temps en seconde pour atteindre 100 km/h
        const float MASSE_VOITURE = 1000.0f;// En Kilogramme
        const float ACCÉLÉRATION_FREIN = 3.0f;// Km/h Valeur pas correcte
        const float ROTATION_MAXIMALE_ROUE = (float)Math.PI / 80f;


        string NomModèle { get; set; }
        RessourcesManager<Model> GestionnaireDeModèles { get; set; }
        RessourcesManager<SoundEffect> GestionnaireDeSon { get; set; }
        Caméra CaméraJeu { get; set; }
        protected float Échelle { get; set; }
        protected float ÉchelleInitiale { get; set; }
        public Vector3 Rotation { get; set; }
        protected Vector3 RotationInitiale { get; set; }
        public Vector3 Position { get; set; }
        public Model Modèle { get; private set; }
        protected Matrix[] TransformationsModèle { get; private set; }
        protected Matrix Monde { get; set; }
        float IntervalleMAJ { get; set; }
        float TempsÉcouléDepuisMAJ { get; set; }
        InputManager GestionInput { get; set; }
        bool D1Actif { get; set; }
        bool D2Actif { get; set; }
        bool D3Actif { get; set; }
        bool EnAvant { get; set; }
        public float Vitesse { get; set; }
        int CPT { get; set; }

        int Déplacement { get; set; }
        int PositionInitialeSouris { get; set; }
        int PositionFinaleSouris { get; set; }
        float RotationMaximaleDéplacement { get; set; }
        float Temps { get; set; }

        float FacteurEngine { get; set; }

        SoundEffectInstance SoundAcceleration { get; set; }
        SoundEffectInstance SoundBrake { get; set; }

        bool User { get; set; }

        public bool EnContactVoiture { get; set; }


        BoundingBox BoxVoiture { get; set; }




        public Voiture(Game jeu, String nomModèle, float échelleInitiale, Vector3 rotationInitiale, Vector3 positionInitiale, float intervalleMAJ, bool user)
            : base(jeu)
        {
            NomModèle = nomModèle;
            Position = positionInitiale;
            ÉchelleInitiale = échelleInitiale;
            Échelle = échelleInitiale;
            Rotation = rotationInitiale;
            RotationInitiale = rotationInitiale;
            IntervalleMAJ = intervalleMAJ;
            User = user;
        }

        public override void Initialize()
        {
            CalculerMonde();
            D1Actif = false;
            D2Actif = false;
            D3Actif = false;
            EnAvant = true;
            Vitesse = 0;
            CPT = 0;
            Déplacement = 0;
            Temps = 0;
            EnContactVoiture = false;
            FacteurEngine = -0.5f;

            //http://timjones.tw/blog/archive/2010/12/10/drawing-an-xna-model-bounding-box


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
            GestionnaireDeSon = Game.Services.GetService(typeof(RessourcesManager<SoundEffect>)) as RessourcesManager<SoundEffect>;
            Modèle = GestionnaireDeModèles.Find(NomModèle);
            TransformationsModèle = new Matrix[Modèle.Bones.Count];
            Modèle.CopyAbsoluteBoneTransformsTo(TransformationsModèle);
            GestionInput = Game.Services.GetService(typeof(InputManager)) as InputManager;
            SoundAcceleration = GestionnaireDeSon.Find("acceleration").CreateInstance();
            SoundBrake = GestionnaireDeSon.Find("brakeEffect").CreateInstance();
        }

        public override void Draw(GameTime gameTime)
        {
            foreach (ModelMesh maille in Modèle.Meshes)
            {
                //Game.Window.Title = ((int)PixelToKMH(Vitesse)).ToString();
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
            TempsÉcouléDepuisMAJ += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (TempsÉcouléDepuisMAJ >= IntervalleMAJ)
            {
                if (User)
                {
                    if (GestionInput.EstEnfoncée(Keys.W) && Vitesse >= 0)
                    {
                        if (GestionInput.EstEnfoncée(Keys.E))
                        {
                            EnAvant = true;
                            Accélération(EnAvant);
                        }
                        Avance();
                    }
                    else if (GestionInput.EstEnfoncée(Keys.S) && Vitesse <= 0)
                    {
                        if (GestionInput.EstEnfoncée(Keys.E))
                        {
                            EnAvant = false;
                            Accélération(EnAvant);
                        }
                        Avance();
                    }
                    else
                    {
                        Décélération(EnAvant);
                        Avance();
                    }
                    if (Vitesse != 0)
                    {
                        GestionRotationVoiture();
                    }
                    if (GestionInput.EstEnfoncée(Keys.Tab))
                    {
                        Freinage();
                    }

                    if (Position.Y <= 0)
                    {
                        Temps = 0;
                        Position = new Vector3(Position.X, 0, Position.Z);
                    }
                    else
                    {
                        GetHauteur();
                    }

                    PitchAndSound();
                }
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
            Position = new Vector3(Position.X + (Vitesse * (float)Math.Sin(Rotation.Y)), Position.Y, Position.Z + (Vitesse * (float)Math.Cos(Rotation.Y)));
        }

        void Décélération(bool négative)
        {
            if (négative)
            {
                if (Vitesse > 0)
                {
                    Vitesse -= DÉCÉLÉRATION;
                }
                else
                {
                    Vitesse = 0;
                }   
            }
            else
            {
                if (Vitesse < 0)
                {
                    Vitesse += DÉCÉLÉRATION;
                }
                else
                {
                    Vitesse = 0;
                }
            }
        }

        void Accélération(bool positive)
        {
            if (positive)
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
            else
            {
                if (Vitesse > KMHtoPixel(VITESSE_MAX_RECULONS))
                {
                    Vitesse -= ((KMHtoPixel(VITESSE_MAX_RECULONS) / ACCÉLÉRATION_RECULONS) / 60f);
                }
            }
        }

        void Freinage()
        {
            if (Vitesse > 0)
            {
                Vitesse -= (ACCÉLÉRATION_FREIN) / 60f;
            }
            else if (Vitesse < 0)
            {
                Vitesse += (ACCÉLÉRATION_FREIN) / 60f;
            }
            else
            {
                Vitesse = 0;
            }
        }

        float KMHtoPixel(float kmh)
        {
            return (kmh / 3.6f) / 6;
        }

        public float PixelToKMH(float pixel)
        {
            return (pixel * 6) * 3.6f;
        }

        void GestionRotationVoiture()
        {
            float rotation = 0;
            if (Vitesse >= KMHtoPixel(Math.Abs(VITESSE_MAX_RECULONS)))
            {
                rotation = ROTATION_MAXIMALE_ROUE;
            }
            else
            {
                rotation = ROTATION_MAXIMALE_ROUE * (Vitesse / KMHtoPixel(Math.Abs(VITESSE_MAX_RECULONS)));
            }

            if ((Rotation.Y + rotation) >= (Math.PI * 2))
            {
                rotation -= (float)Math.PI * 2;
            }
            else if((Rotation.Y - rotation) <= (Math.PI * -2))
            {
                rotation -= (float)Math.PI * 2;
            }

            if (GestionInput.EstEnfoncée(Keys.Left))
            {
                Rotation = new Vector3(Rotation.X, Rotation.Y + rotation, Rotation.Z);
            }
            if (GestionInput.EstEnfoncée(Keys.Right))
            {
                Rotation = new Vector3(Rotation.X, Rotation.Y - rotation, Rotation.Z);
            }
        }

        void GetHauteur()
        {
            Temps++; // 1/60s
            float g = 98.1f / (60f * 60f);
            float vitesse = g * Temps;
            Position = new Vector3(Position.X, Position.Y - vitesse, Position.Z);
        }

        void PitchAndSound()
        {
            SoundAcceleration.Play();
            SoundAcceleration.Volume = 0.5f;
            if (!GestionInput.EstEnfoncée(Keys.Tab))
            {
                if (SoundBrake.Volume - 0.05f >= 0)
                {
                    SoundBrake.Volume = SoundBrake.Volume - 0.05f;   
                }
                if (Vitesse >= KMHtoPixel(0) && Vitesse < KMHtoPixel(30))
                {
                    SoundAcceleration.Pitch = (Math.Abs(PixelToKMH(Vitesse)) / 100f) - 0.1f;
                }
                else if (Vitesse >= KMHtoPixel(30) && Vitesse < KMHtoPixel(60))
                {
                    SoundAcceleration.Pitch = (Math.Abs(PixelToKMH(Vitesse)) / 100f) - 0.3f;
                }
                else if (Vitesse >= KMHtoPixel(60))
                {
                    SoundAcceleration.Pitch = (Math.Abs(PixelToKMH(Vitesse)) / 100f) - 0.5f;
                }
            }
            else
            {
                SoundAcceleration.Pitch = (Math.Abs(PixelToKMH(Vitesse)) / 100f) - 0.1f;
                if (Vitesse >= KMHtoPixel(60))
                {
                    SoundBrake.Play();
                    SoundBrake.Volume = 1.0f;
                }
            }

            if (Vitesse == 0)
            {
                SoundAcceleration.Volume = 0.5f;
            }
        }

        public void GestionCollisionVoiture(Voiture voiture)
        {
            for (int i = 0; i < Modèle.Meshes.Count; i++)
            {
                BoundingSphere bordureUtilisateur = Modèle.Meshes[i].BoundingSphere;
                bordureUtilisateur.Center += Position;

                for (int j = 0; j < voiture.Modèle.Meshes.Count; j++)
                {
                    BoundingSphere bordureAutre = voiture.Modèle.Meshes[j].BoundingSphere;
                    bordureAutre.Center += voiture.Position;

                    if (bordureUtilisateur.Intersects(bordureAutre))
                    {
                        EnContactVoiture = true;
                    }
                }
            }
        }
    }
}
