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
        Vector3 DIMENSION = new Vector3(20, 25, 40);


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
        bool EnAvant { get; set; }
        Vector3 Dimension { get; set; }
        public float Vitesse { get; set; }
        int CPT { get; set; }

        int Déplacement { get; set; }
        int PositionInitialeSouris { get; set; }
        int PositionFinaleSouris { get; set; }
        float RotationMaximaleDéplacement { get; set; }
        float Temps { get; set; }
        float RotationVoiture { get; set; }

        float FacteurEngine { get; set; }

        SoundEffectInstance SoundAcceleration { get; set; }
        SoundEffectInstance SoundBrake { get; set; }

        bool User { get; set; }

        public bool EnContactVoiture { get; set; }
        bool AvanceCollision { get; set; }
        bool AccélérationPossible { get; set; }
        bool CollisionPrête { get; set; }
        int Collision { get; set; }


        BoundingBox BoxVoiture { get; set; }
        BoundingSphere SphereVoitureAvant { get; set; }
        BoundingSphere SphereVoitureArrière { get; set; }
        BoundingSphere SphereVoitureMillieu { get; set; }

        SpriteBatch GestionSprites { get; set; }//ToDELETE
        SpriteFont ArialFont { get; set; }//ToDELETE


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
            Dimension = new Vector3(20, 25, 40);
            EnAvant = true;
            Vitesse = 0;
            CPT = 0;
            Déplacement = 0;
            Temps = 0;
            EnContactVoiture = false;
            FacteurEngine = -0.5f;
            AvanceCollision = true;
            AccélérationPossible = true;
            CollisionPrête = false;
            Collision = 0;
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
            GestionSprites = Game.Services.GetService(typeof(SpriteBatch)) as SpriteBatch;//ToDELETE
            ArialFont = Game.Content.Load<SpriteFont>("Fonts/Arial");//ToDELETE
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
                    if (GestionInput.EstEnfoncée(Keys.C))
                    {
                        Info();//ToDELETE
                    }
                }
                maille.Draw();
            }
           // DebugShapeRenderer.AddBoundingBox(BoxVoiture, Color.Red);
            DebugShapeRenderer.AddBoundingBox(BoundingBox.CreateFromSphere(SphereVoitureAvant), Color.Green);
            DebugShapeRenderer.AddBoundingBox(BoundingBox.CreateFromSphere(SphereVoitureMillieu), Color.Red);
            DebugShapeRenderer.AddBoundingBox(BoundingBox.CreateFromSphere(SphereVoitureArrière), Color.Yellow);
            //DebugShapeRenderer.AddBoundingSphere((SphereVoitureAvant), Color.Green);
            //DebugShapeRenderer.AddBoundingSphere((SphereVoitureMillieu), Color.Red);
            //DebugShapeRenderer.AddBoundingSphere((SphereVoitureArrière), Color.Yellow);



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
                            if (AccélérationPossible)
                            {
                                Accélération(EnAvant);   
                            }
                        }
                        Avance();
                    }
                    else if (GestionInput.EstEnfoncée(Keys.S) && Vitesse <= 0)
                    {
                        if (GestionInput.EstEnfoncée(Keys.E))
                        {
                            EnAvant = false;
                            if (AccélérationPossible)
                            {
                                Accélération(EnAvant);
                            }
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

                    //PitchAndSound();
                }
                CalculerMonde();
                CreateBoundingBox();
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

        void TranslationCollision(float vitesse)
        {
            Position = new Vector3(Position.X + (vitesse * (float)Math.Cos(Rotation.Y)), Position.Y, Position.Z + (vitesse * (float)Math.Sin(Rotation.Y)));
        }

        public void Décélération(bool négative)
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
                RotationVoiture = ROTATION_MAXIMALE_ROUE;
            }
            else
            {
                RotationVoiture = ROTATION_MAXIMALE_ROUE * (Vitesse / KMHtoPixel(Math.Abs(VITESSE_MAX_RECULONS)));
            }

            if ((Rotation.Y + RotationVoiture) >= (Math.PI * 2))
            {
                rotation -= (float)Math.PI * 2;
            }
            else if ((Rotation.Y - RotationVoiture) <= (Math.PI * -2))
            {
                rotation -= (float)Math.PI * 2;
            }

            if (GestionInput.EstEnfoncée(Keys.Left))
            {
                Rotation = new Vector3(Rotation.X, Rotation.Y + RotationVoiture, Rotation.Z);
            }
            if (GestionInput.EstEnfoncée(Keys.Right))
            {
                Rotation = new Vector3(Rotation.X, Rotation.Y - RotationVoiture, Rotation.Z);
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

        public bool GestionCollisionVoiture2(Voiture voiture)
        {
            //voiture.Vitesse = KMHtoPixel(10.0f);
            //voiture.Avance();
            bool enCollision = false;
            bool avance = true;
            bool translation = false;
            float deltaRotation = NormalizeRotation(Rotation.Y) - NormalizeRotation(voiture.Rotation.Y);
            Vector2 deltaPosition = new Vector2((int)(voiture.Position.X - Position.X), (int)(voiture.Position.Z - Position.Z));
            Collision = GetPositionCollision(deltaPosition, Collision);
            enCollision = voiture.BoxVoiture.Intersects(BoxVoiture);

            if (enCollision)
            {
                enCollision = voiture.BoxVoiture.Intersects(BoxVoiture);
                if (CollisionPrête)
                {
                    if (Math.Abs(Math.Cos(deltaRotation)) >= 0.1f && Math.Abs(Math.Cos(deltaRotation)) <= 0.9f)
                    {
                        if (deltaRotation >= 0)
                        {
                            voiture.Rotation = new Vector3(voiture.Rotation.X, voiture.Rotation.Y - (1 * Vitesse / 10f), voiture.Rotation.Z);
                        }
                        else
                        {
                            voiture.Rotation = new Vector3(voiture.Rotation.X, voiture.Rotation.Y - (-1 * Vitesse / 10f), voiture.Rotation.Z);
                        }
                    }
                    //Transfert d'énergie
                    Vitesse -= KMHtoPixel(2.0f);
                    //if (Collision == 0 && Vitesse >= KMHtoPixel(30.0f))
                    //{
                    //    translation = false;
                    //    voiture.Position = new Vector3(voiture.Position.X, voiture.Position.Y, voiture.Position.Z + (2 * (float)Math.Cos(voiture.Rotation.Y)));
                    //    voiture.Vitesse = Vitesse * 0.6f;
                    //    voiture.Avance();
                    //    Vitesse -= KMHtoPixel(50.0f);
                    //}
                    //else if (Collision == 6 && Vitesse >= KMHtoPixel(30.0f))
                    //{
                    //    translation = false;
                    //    voiture.Position = new Vector3(voiture.Position.X, voiture.Position.Y, voiture.Position.Z - (2 * (float)Math.Cos(voiture.Rotation.Y)));
                    //    voiture.Vitesse = -1 * Math.Abs(Vitesse * 0.6f);
                    //    voiture.Avance();
                    //    Vitesse -= KMHtoPixel(50.0f);
                    //}
                    //else if (Collision == 3 && Vitesse >= KMHtoPixel(15.0f))
                    //{
                    //    if (PixelToKMH(Vitesse) >= 40.0f)
                    //    {
                    //        Vitesse /= 2f;
                    //    }
                    //    Vitesse -= KMHtoPixel(2.0f);
                    //    voiture.Position = new Vector3(Position.X + (deltaPosition.X * 1.1f * (float)Math.Cos(voiture.Rotation.Y)), voiture.Position.Y, voiture.Position.Z + (deltaPosition.Y * 1.1f * (float)Math.Sin(voiture.Rotation.Y)));
                    //}
                    //else if (Collision == 9 && Vitesse >= KMHtoPixel(15.0f))
                    //{
                    //    if (PixelToKMH(Vitesse) >= 40.0f)
                    //    {
                    //        Vitesse /= 2f;
                    //    }
                    //    Vitesse -= KMHtoPixel(2.0f);
                    //    voiture.Position = new Vector3(Position.X + (deltaPosition.X * 1.1f * (float)Math.Cos(voiture.Rotation.Y)), voiture.Position.Y, voiture.Position.Z + (deltaPosition.Y * 1.1f * (float)Math.Sin(voiture.Rotation.Y)));
                    //}
                    //else if(Collision == 2)
                    //{
                    //    Vitesse -= KMHtoPixel(2.0f);
                    //    voiture.Rotation = new Vector3(voiture.Rotation.X, voiture.Rotation.Y - ((float)Math.Cos(PixelToKMH(Vitesse) / VITESSE_MAX)), voiture.Rotation.Z);
                    //}
                    //else
                    //{
                    //    Vitesse = 0;
                    //}
                }
                CollisionPrête = true;
            }
            else
            {
                if (!translation)
                {
                    voiture.Décélération(AvanceCollision);//devrait être la décélération naturelle de l'auto
                    voiture.Avance();//Same ^^
                }
            }

            //Game.Window.Title = Position.ToString() + " - " + collision.ToString() + " - " + PixelToKMH(Vitesse) + " - " + translation.ToString(); 
            Game.Window.Title = deltaPosition.ToString() + " - " + Collision.ToString() + " - " + Math.Cos(deltaRotation).ToString();
            return AvanceCollision;
        }

        public bool GestionCollisionVoiture(Voiture voiture)
        {
            bool enCollisionAvant = false;
            bool enCollisionMilieu = false;
            bool enCollisionArrière = false;
            float deltaRotation = NormalizeRotation(Rotation.Y) - NormalizeRotation(voiture.Rotation.Y);
            enCollisionArrière = SphereVoitureAvant.Intersects(voiture.SphereVoitureArrière);
            if (enCollisionArrière)
            {
                enCollisionArrière = voiture.BoxVoiture.Intersects(BoxVoiture);
                if (CollisionPrête)
                {
                    //if (Math.Abs(deltaRotation) >= 0.1 && Math.Abs(deltaRotation) <= 0.9)
                    //{
                        voiture.Rotation = new Vector3(voiture.Rotation.X, voiture.Rotation.Y - (deltaRotation / 60f) * Vitesse, voiture.Rotation.Z);
                    //}
                    //Transfert d'énergie
                    //voiture.Position = new Vector3(0,0,0);
                    voiture.Position = new Vector3(voiture.Position.X, voiture.Position.Y, voiture.Position.Z + (2 * (float)Math.Cos(voiture.Rotation.Y)));
                    voiture.Vitesse = Vitesse * 0.6f;
                    voiture.Avance();
                    //Vitesse -= KMHtoPixel(50.0f);
                    //Vitesse -= KMHtoPixel(2.0f);
                }
                CollisionPrête = true;
            }
            else
            {
                voiture.Décélération(AvanceCollision);//devrait être la décélération naturelle de l'auto
                voiture.Avance();//Same ^^
            }
            Game.Window.Title = enCollisionArrière.ToString() + " : " + (Math.Cos(deltaRotation)).ToString();
            return true;
        }

        void CreateBoundingBox()
        {
            SphereVoitureAvant = new BoundingSphere(new Vector3(Position.X, Position.Y + 10, Position.Z), 7f);
            SphereVoitureMillieu = new BoundingSphere(new Vector3(Position.X + (15 * (float)Math.Sin(-Rotation.Y)), Position.Y + 12, Position.Z - (15 * (float)Math.Cos(-Rotation.Y))), 7f);
            SphereVoitureArrière = new BoundingSphere(new Vector3(Position.X + (30 * (float)Math.Sin(-Rotation.Y)), Position.Y + 10, Position.Z - (30 * (float)Math.Cos(-Rotation.Y))), 7f);

            BoxVoiture = BoundingBox.CreateMerged(BoundingBox.CreateFromSphere(SphereVoitureAvant), BoundingBox.CreateFromSphere(SphereVoitureMillieu));
            BoxVoiture = BoundingBox.CreateMerged(BoxVoiture, BoundingBox.CreateFromSphere(SphereVoitureArrière));

        }

        float NormalizeRotation(float rotation)
        {
            if (rotation > 0)
            {
                rotation = rotation % (float)(Math.PI * 2);
            }
            else if (rotation < 0)
            {
                rotation = rotation % (float)(Math.PI * 2);
            }
            return rotation;
        }

        int GetPositionCollision(Vector2 deltaPosition, int ancienneCollision)
        {
            int collision = Collision;
            AvanceCollision = false;
            if (Math.Abs(deltaPosition.X) >= 20 && deltaPosition.Y <= 55 && deltaPosition.Y >= -10)
            {
                if (deltaPosition.X > 0)
                {
                    if (deltaPosition.Y > 25)
                    {
                        //Game.Window.Title = "Côté Droit Arrière" + deltaPosition.ToString();
                        collision = 2;
                    }
                    else if (deltaPosition.Y < 7)
                    {
                        //Game.Window.Title = "Côté Droit Avant" + deltaPosition.ToString();
                        collision = 4;
                    }
                    else
                    {
                        //Game.Window.Title = "Côté Droit" + deltaPosition.ToString();
                        collision = 3;
                    }
                }
                else if (deltaPosition.X < 0)
                {
                    if (deltaPosition.Y > 25)
                    {
                        //Game.Window.Title = "Côté Gauche Arrière" + deltaPosition.ToString();
                        collision = 10;
                    }
                    else if (deltaPosition.Y < 7)
                    {
                        //Game.Window.Title = "Côté Gauche Avant" + deltaPosition.ToString();
                        collision = 8;
                    }
                    else
                    {
                        //Game.Window.Title = "Côté Gauche" + deltaPosition.ToString();
                        collision = 9;
                    }
                }
            }
            else
            {
                if (deltaPosition.Y >= 50)
                {
                    if (deltaPosition.X >= 10)
                    {
                        //Game.Window.Title = "Arrière Droite" + deltaPosition.ToString();
                        collision = 1;
                        AvanceCollision = true;
                    }
                    else if (deltaPosition.X <= -10)
                    {
                        //Game.Window.Title = "Arrière Gauche" + deltaPosition.ToString();
                        collision = 11;
                        AvanceCollision = true;
                    }
                    else
                    {
                        //Game.Window.Title = "Arrière " + deltaPosition.ToString();
                        collision = 0;
                        AvanceCollision = true;
                    }
                }
                else if (deltaPosition.Y <= -10)
                {
                    if (deltaPosition.X >= 10)
                    {
                        //Game.Window.Title = "Avant Droite" + deltaPosition.ToString();
                        collision = 5;
                    }
                    else if (deltaPosition.X <= -10)
                    {
                        //Game.Window.Title = "Avant Gauche" + deltaPosition.ToString();
                        collision = 7;
                    }
                    else
                    {
                        //Game.Window.Title = "Avant " + deltaPosition.ToString();
                        collision = 6;
                    }
                }
            }
            return collision;
        }

        void Info()//ToDelete
        {
            if (User)
            {
                string info = "Vitesse: " + ((int)PixelToKMH(Vitesse)) + "  Position: " + Position + "  Rotation:" + Rotation.Y;

                GestionSprites.DrawString(ArialFont, info, new Vector2(0, 0), Color.White,0f,new Vector2(0,0),0.5f,SpriteEffects.None,0);
            }
        }
    }
}