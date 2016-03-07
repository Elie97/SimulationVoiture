using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;


namespace SimulationVéhicule
{
    public class Voiture : Microsoft.Xna.Framework.DrawableGameComponent
    {
        const float DÉCÉLÉRATION = 0.02f;// en décimètre
        const float VITESSE_MAX = 100.0f;// Km/H
        const float VITESSE_MAX_RECULONS = -30.0f;// Km/H
        const float ACCÉLÉRATION = 4.0f;// Temps en seconde pour atteindre 100 km/h 4.0f!!!
        const float ACCÉLÉRATION_RECULONS = -2.0f;// Temps en seconde pour atteindre 100 km/h
        const float MASSE_VOITURE = 1000.0f;// En Kilogramme
        const float ACCÉLÉRATION_FREIN = 3.0f;// Km/h Valeur pas correcte
        const float ROTATION_MAXIMALE_ROUE = (float)Math.PI / 80f;
        Vector3 DIMENSION = new Vector3(20, 25, 40);


        string NomModèle { get; set; }
        RessourcesManager<Model> GestionnaireDeModèles { get; set; }
        RessourcesManager<SoundEffect> GestionnaireDeSon { get; set; }
        RessourcesManager<Song> GestionnaireDeMusique { get; set; }
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
        Song Queen { get; set; }

        bool User { get; set; }

        public bool EnContactVoiture { get; set; }
        bool AvanceCollision { get; set; }
        bool AvancePossible { get; set; }
        bool AccélérationPossible { get; set; }
        bool CollisionPrête { get; set; }
        int Collision { get; set; }
        float RotationEnCollision { get; set; }
        float VitesseRotation { get; set; }
        bool DernièreCollisionEstAvant { get; set; }
        bool Translation { get; set; }
        public bool ChuteLibrePossible { get; set; }


        public BoundingBox BoxVoiture { get; set; }
        public BoundingSphere SphereVoitureAvant { get; set; }
        BoundingSphere SphereVoitureArrière { get; set; }
        BoundingSphere SphereVoitureMillieu { get; set; }
        BoundingSphere SphereRouteAvant { get; set; }

        SpriteBatch GestionSprites { get; set; }//ToDELETE
        SpriteFont ArialFont { get; set; }//ToDELETE

        Quaternion RotationQuaternion = Quaternion.Identity;

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
            DernièreCollisionEstAvant = false;
            Translation = false;
            ChuteLibrePossible = true;
            AvancePossible = true;
            Collision = 0;
            RotationEnCollision = 0;
            VitesseRotation = 0;
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
            GestionnaireDeMusique = Game.Services.GetService(typeof(RessourcesManager<Song>)) as RessourcesManager<Song>;
            Modèle = GestionnaireDeModèles.Find(NomModèle);
            TransformationsModèle = new Matrix[Modèle.Bones.Count];
            Modèle.CopyAbsoluteBoneTransformsTo(TransformationsModèle);
            GestionInput = Game.Services.GetService(typeof(InputManager)) as InputManager;
            SoundAcceleration = GestionnaireDeSon.Find("acceleration").CreateInstance();
            SoundBrake = GestionnaireDeSon.Find("brakeEffect").CreateInstance();
        }

        public override void Draw(GameTime gameTime)
        {
            //foreach (ModelMesh maille in Modèle.Meshes)
            //{
            //    //Game.Window.Title = ((int)PixelToKMH(Vitesse)).ToString();
            //    Matrix mondeLocal = TransformationsModèle[maille.ParentBone.Index] * GetMonde();
            //    foreach (ModelMeshPart portionDeMaillage in maille.MeshParts)
            //    {
            //        BasicEffect effet = (BasicEffect)portionDeMaillage.Effect;
            //        effet.EnableDefaultLighting();
            //        effet.Projection = CaméraJeu.Projection;
            //        effet.View = CaméraJeu.Vue;
            //        effet.World = mondeLocal;
            //    }
            //    maille.Draw();
            //}

            foreach (ModelMesh Mesh in Modèle.Meshes)
            {
                foreach (BasicEffect Effect in Mesh.Effects)    //Every Mesh has a different world matrix that has to be passed to the shader and the shader here is stored as part of the mesh. The model object basically draws itself using the information stored in the model object.
                {
                    Effect.Projection = CaméraJeu.Projection;   //Set the camera's projection.
                    Effect.View = CaméraJeu.Vue;   //Set the camera in position.
                    Effect.World = TransformationsModèle[Mesh.ParentBone.Index] * GetMonde(); //Each part's world matrix is relative to the parent part. And all the parts must move with the tank itself. 
                    Effect.EnableDefaultLighting(); //Little more detailed scene lighting.
                }
                Mesh.Draw();    //Draw this mesh and then go to the next mesh.
            }

            if (GestionInput.EstEnfoncée(Keys.C))
            {
                Info();//ToDELETE
            }

           // DebugShapeRenderer.AddBoundingBox(BoxVoiture, Color.Red);
            //DebugShapeRenderer.AddBoundingBox(BoundingBox.CreateFromSphere(SphereVoitureAvant), Color.Green);
            //DebugShapeRenderer.AddBoundingBox(BoundingBox.CreateFromSphere(SphereVoitureMillieu), Color.Red);
            //DebugShapeRenderer.AddBoundingBox(BoundingBox.CreateFromSphere(SphereVoitureArrière), Color.Yellow);
            DebugShapeRenderer.AddBoundingSphere(SphereRouteAvant, Color.Yellow);
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
                    if (GestionInput.EstEnfoncée(Keys.W) && Vitesse >= 0 && AvancePossible)
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
                    //else
                    //{
                    //    GetHauteur();
                    //}

                    PitchAndSound();
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

        public Vector3 AvanceCaméra()
        {
            return new Vector3(Position.X + (0 * (float)Math.Sin(Rotation.Y)), Position.Y, Position.Z + (0 * (float)Math.Cos(Rotation.Y)));
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

        public void GetHauteur()
        {
            Temps++; // 1/60s
            float g = 98.1f / (60f * 60f);
            float vitesse = g * Temps;
            Position = new Vector3(Position.X, Position.Y - vitesse, Position.Z);
        }

        void PitchAndSound()
        {
            SoundAcceleration.Play();
            SoundAcceleration.Volume = 0.25f;
            if (!GestionInput.EstEnfoncée(Keys.Tab))
            {
                if (SoundBrake.Volume - 0.05f >= 0)
                {
                    SoundBrake.Volume = SoundBrake.Volume - 0.05f;   
                }
                if (Vitesse >= KMHtoPixel(0) && Vitesse < KMHtoPixel(30))
                {
                    SoundAcceleration.Pitch = (Math.Abs(PixelToKMH(Vitesse)) / VITESSE_MAX) - 0.1f;
                }
                else if (Vitesse >= KMHtoPixel(30) && Vitesse < KMHtoPixel(60))
                {
                    SoundAcceleration.Pitch = (Math.Abs(PixelToKMH(Vitesse)) / VITESSE_MAX) - 0.3f;
                }
                else if (Vitesse >= KMHtoPixel(60))
                {
                    SoundAcceleration.Pitch = (Math.Abs(PixelToKMH(Vitesse)) / VITESSE_MAX) - 0.5f;
                }
            }
            else
            {
                SoundAcceleration.Pitch = (Math.Abs(PixelToKMH(Vitesse)) / VITESSE_MAX) - 0.1f;
                if (Vitesse >= KMHtoPixel(60))
                {
                    SoundBrake.Play();
                    SoundBrake.Volume = 1.0f;
                }
            }

            if ((GestionInput.EstEnfoncée(Keys.Left) || GestionInput.EstEnfoncée(Keys.Right)) && Vitesse >= KMHtoPixel(70))
            {
                SoundBrake.Play();
                SoundBrake.Volume = 1.0f;
            }
            else
            {
                SoundBrake.Stop();
            }

            if (Vitesse == 0)
            {
                SoundAcceleration.Volume = 0.5f;
            }
        }


        public bool GestionCollisionVoiture(Voiture voiture)
        {
            bool enCollisionAvant = false;
            bool enCollisionMilieu = false;
            bool enCollisionArrière = false;
            bool collisionAvantVitesseNégative = false;
            float deltaRotation = NormalizeRotation(Rotation.Y) - NormalizeRotation(voiture.Rotation.Y);
            float deltaVitesse = voiture.Vitesse - Vitesse;
            Vector2 deltaPosition = new Vector2((int)(voiture.Position.X - Position.X), (int)(voiture.Position.Z - Position.Z));
            if (Math.Cos(deltaRotation) <= 0)
            {
                collisionAvantVitesseNégative = true;
            }
            else
            {
                collisionAvantVitesseNégative = false;
            }
            //if (Math.Sqrt(deltaPosition.X * deltaPosition.X + deltaPosition.Y * deltaPosition.Y) >= 20)
            //{
                enCollisionArrière = SphereVoitureAvant.Intersects(voiture.SphereVoitureArrière);
                enCollisionAvant = SphereVoitureAvant.Intersects(voiture.SphereVoitureAvant);
                enCollisionMilieu = SphereVoitureAvant.Intersects(voiture.SphereVoitureMillieu);
                //enCollision = BoxVoiture.Intersects(voiture.BoxVoiture);
                if (enCollisionArrière || enCollisionAvant || enCollisionMilieu)
                {
                    if (Vitesse >= KMHtoPixel(20.0f) || Translation)
                    {
                        if (enCollisionArrière)
                        {
                            DernièreCollisionEstAvant = false;
                            Translation = false;
                            if (Math.Abs(deltaRotation) >= 0.1f)
                            {
                                RotationEnCollision = (float)Math.Sin(deltaRotation) * ((float)(Math.PI / 4f) * (PixelToKMH(Vitesse) / VITESSE_MAX));//deltaVitesse?
                                RotationCollision(voiture, true, RotationEnCollision, DernièreCollisionEstAvant);
                            }
                            AvanceCollision = true;
                            voiture.Vitesse = Vitesse * 0.6f;
                            voiture.Position = new Vector3(voiture.Position.X, voiture.Position.Y, voiture.Position.Z + 0);
                            voiture.Avance();
                            Vitesse -= KMHtoPixel(50.0f);
                        }
                        else if (enCollisionAvant)
                        {
                            DernièreCollisionEstAvant = true;
                            Translation = false;
                            AvanceCollision = false;

                            if (collisionAvantVitesseNégative)
                            {
                                if (Math.Abs(deltaRotation) >= 0.1f)
                                {
                                    RotationEnCollision = (float)Math.Sin(deltaRotation) * ((float)(Math.PI / 8f) * (PixelToKMH(Vitesse) / VITESSE_MAX));//deltaVitesse?
                                    RotationCollision(voiture, true, RotationEnCollision, DernièreCollisionEstAvant);
                                }
                                voiture.Vitesse = -1 * Math.Abs(Vitesse * 0.7f);
                                voiture.Position = new Vector3(voiture.Position.X, voiture.Position.Y, voiture.Position.Z - 2);
                            }
                            else
                            {
                                //voiture.Vitesse = Math.Abs(Vitesse * 0.7f);
                                //voiture.Position = new Vector3(voiture.Position.X, voiture.Position.Y, voiture.Position.Z - 10);
                                if (Math.Abs(deltaRotation) >= 0.1f)
                                {
                                    RotationEnCollision = (float)Math.Sin(deltaRotation) * ((float)(Math.PI / 8f) * (PixelToKMH(Vitesse) / VITESSE_MAX));//deltaVitesse?
                                    RotationCollision(voiture, true, RotationEnCollision, DernièreCollisionEstAvant);
                                }
                                if (deltaRotation <= MathHelper.Pi)
                                {
                                    voiture.Position = new Vector3(voiture.Position.X + (10 * (float)Math.Sin(deltaRotation)), voiture.Position.Y, voiture.Position.Z - (10 * (float)Math.Cos(deltaRotation)));
                                }
                                else
                                {
                                    voiture.Position = new Vector3(voiture.Position.X - (10 * (float)Math.Sin(deltaRotation)), voiture.Position.Y, voiture.Position.Z + (10 * (float)Math.Cos(deltaRotation)));
                                }
                            }
                            voiture.Avance();
                            Vitesse -= KMHtoPixel(40.0f);
                        }
                        else if (enCollisionMilieu && !enCollisionAvant && !enCollisionArrière)
                        {
                            Translation = true;
                            if (PixelToKMH(Vitesse) >= 40.0f)
                            {
                                Vitesse /= 2f;
                            }
                            if (Vitesse > 0)
                            {
                                Vitesse -= KMHtoPixel(1.0f);                                
                            }
                            if (deltaRotation <= MathHelper.Pi)
                            {
                                voiture.Position = new Vector3(voiture.Position.X + (1.01f * Vitesse * (float)Math.Sin(deltaRotation)), voiture.Position.Y, voiture.Position.Z - (1.01f * Vitesse * (float)Math.Cos(deltaRotation)));
                            }
                            else
                            {
                                voiture.Position = new Vector3(voiture.Position.X - (1.01f * Vitesse * (float)Math.Sin(deltaRotation)), voiture.Position.Y, voiture.Position.Z + (1.01f * Vitesse * (float)Math.Cos(deltaRotation)));
                            }
                        }
                    }
                    else if (Vitesse < KMHtoPixel(20.0f) && !Translation)
                    {
                        Vitesse = 0;
                        Position = new Vector3(Position.X + (20 * (float)Math.Cos(deltaRotation)), Position.Y, Position.Z + (20 * (float)Math.Sin(deltaRotation)));
                    }
                }
                else
                {
                    if (!Translation)
                    {
                        voiture.Décélération(AvanceCollision);//devrait être la décélération naturelle de l'auto
                        voiture.Avance();//Same ^^   
                    }
                }
            RotationCollision(voiture, false, RotationEnCollision, DernièreCollisionEstAvant);
            //Game.Window.Title = collisionAvantVitesseNégative.ToString() + " : " + Math.Cos(deltaRotation).ToString();
            return true;
        }

        public bool GestionCollisionPiste(Sol sol)
        {
            bool enCollision = SphereRouteAvant.Intersects(sol.Box);
            if (enCollision)
            {
                //sol.GetHauteur(Position);
                Position = new Vector3(Position.X, sol.GetHauteur(Position), Position.Z);
                Rotation = new Vector3(-sol.RotationInitiale.X, Rotation.Y, Rotation.Z);

                //ChuteLibrePossible = false;
            }

            //if (BoxVoiture.Intersects(sol.BoxDroite) || BoxVoiture.Intersects(sol.BoxGauche))
            //{
            //    Vitesse = 0;//Pour l'instant
            //    AvancePossible = false;
            //}
            if (GestionInput.EstEnfoncée(Keys.S))
            {
                AvancePossible = true;
            }

            return enCollision;
        }

        void CreateBoundingBox()
        {
            SphereVoitureAvant = new BoundingSphere(new Vector3(Position.X, Position.Y + 10, Position.Z), 7f);
            SphereVoitureMillieu = new BoundingSphere(new Vector3(Position.X + (15 * (float)Math.Sin(-Rotation.Y)), Position.Y + 12, Position.Z - (15 * (float)Math.Cos(-Rotation.Y))), 7f);
            SphereVoitureArrière = new BoundingSphere(new Vector3(Position.X + (30 * (float)Math.Sin(-Rotation.Y)), Position.Y + 10, Position.Z - (30 * (float)Math.Cos(-Rotation.Y))), 7f);
            SphereRouteAvant = new BoundingSphere(Position, 1);

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

        void Info()//ToDelete
        {
            if (User)
            {
                string info = "Vitesse: " + ((int)PixelToKMH(Vitesse)) + "  Position: " + Position + "  Rotation:" + Rotation.Y;

                GestionSprites.DrawString(ArialFont, info, new Vector2(0, 0), Color.White,0f,new Vector2(0,0),0.5f,SpriteEffects.None,0);
            }
        }

        void RotationCollision(Voiture voiture, bool enContact, float angleMax, bool avant)
        {
            if (enContact)
            {
                VitesseRotation = (angleMax) / 4f;
            }
            VitesseRotation -= VitesseRotation * 0.1f;
            if (!avant)
            {
                voiture.Rotation = new Vector3(voiture.Rotation.X, voiture.Rotation.Y - (VitesseRotation), voiture.Rotation.Z);
            }
            else
            {
                voiture.Rotation = new Vector3(voiture.Rotation.X, voiture.Rotation.Y + (VitesseRotation*2), voiture.Rotation.Z);
            }
        }

        public void VariationInclinaison()
        {
            Rotation = new Vector3(Rotation.X + 0.005f, Rotation.Y, Rotation.Z);
        }
    }
}