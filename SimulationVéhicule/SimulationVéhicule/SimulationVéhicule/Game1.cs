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
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        const float INTERVALLE_CALCUL_FPS = 1f;
        const float INTERVALLE_MAJ_STANDARD = 1f / 60f;

        const int KILOMÈTRE = 10000;
        const int MÈTRE = 10;

        GraphicsDeviceManager PériphériqueGraphique { get; set; }
        SpriteBatch GestionSprites { get; set; }

        RessourcesManager<SpriteFont> GestionnaireDeFonts { get; set; }
        RessourcesManager<Texture2D> GestionnaireDeTextures { get; set; }
        RessourcesManager<Model> GestionnaireDeModèles { get; set; }
        RessourcesManager<Effect> GestionnaireDeShaders { get; set; }
        RessourcesManager<SoundEffect> GestionnaireDeSon { get; set; }
        RessourcesManager<Song> GestionnaireDeMusique { get; set; }

        CaméraSubjective CaméraJeu { get; set; }

        Voiture Mustang { get; set; }
        Voiture AI { get; set; }
        Terrain Carte { get; set; }
        List<Sol> LaPiste { get; set; }
        List<Voiture> ListeVoiture { get; set; }
        bool[] ÉtatCollisionPiste { get; set; }
        bool EstEnCollisionAvecUnePiste { get; set; }

        Vector3 PositionCaméra { get; set; }
        float CibleYCaméra { get; set; }
        int VueArrière { get; set; }
        //Vector3 ArrièreRapproché { get; set; }
        //Vector3 ArrièreMoyen { get; set; }
        //Vector3 ArrièreLoin { get; set; }
        //Vector3 IntérieurConducteur { get; set; }
        //Vector3 Capo { get; set; }
        //Vector3 VueArrière { get; set; }
        Vector3[] TableauPositionCaméra { get; set; }
        int IndexPositionCaméra { get; set; }

        float TempsÉcouléDepuisMAJ { get; set; }

        public InputManager GestionInput { get; private set; }

        GUI Interface { get; set; }

        //Course
        int PositionUtilisateur { get; set; }
        int NbVoiture { get; set; }
        bool[] CheckPoint { get; set; }
        int NbTours { get; set; }
        int ToursFait { get; set; }

        public Game1()
        {
            PériphériqueGraphique = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            PériphériqueGraphique.SynchronizeWithVerticalRetrace = false;
            IsFixedTimeStep = false;
            IsMouseVisible = true;
        }
        protected override void Initialize()
        {
            DebugShapeRenderer.Initialize(GraphicsDevice);
            PositionUtilisateur = 1;
            NbTours = 2;
            ToursFait = 0;
            Carte = new Terrain(this, 1f, Vector3.Zero, new Vector3(0,-1275,0), new Vector3(25600, 1000, 25600), "test", "DétailsTerrain", 5, INTERVALLE_MAJ_STANDARD);
            LaPiste = new List<Sol>();
            ListeVoiture = new List<Voiture>();

            LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, MathHelper.Pi, 0), new Vector3(0, 0, 0), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, 1));
            LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, MathHelper.Pi, 0), new Vector3(0, 0, 200), new Vector2(100, 200), new Vector2(2, 20), "route", 15, 2 * MathHelper.Pi, false, 0, 1));
            LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, MathHelper.Pi, 0), new Vector3(0, 0, 400), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, 1));
            LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, MathHelper.PiOver2, 0), new Vector3(-125, -0.5f, 750), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, 1));
            LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, MathHelper.Pi, 0), new Vector3(0, 0, 600), new Vector2(100, 200), new Vector2(2, 20), "route", 0, 0, true, MathHelper.Pi / 4, -1));
            LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, MathHelper.PiOver2, 0), new Vector3(-275, 0, 750), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, 1));
            LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, MathHelper.PiOver2, 0), new Vector3(-475, 0, 750), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, 1));
            LaPiste.Add(new Sol(this, 1.0f, new Vector3(MathHelper.Pi / 8f, MathHelper.PiOver2, 0), new Vector3(-675, 0, 750), new Vector2(100, 800), new Vector2(2, 8), "route", 0, 0, false, 0, 1));
            LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, MathHelper.PiOver2, 0), new Vector3(-1875, 50, 750), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, 1));
            LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, MathHelper.PiOver2, 0), new Vector3(-2075, 0, 750), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, 1));
            LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, MathHelper.PiOver2, 0), new Vector3(-2275, 0, 750), new Vector2(100, 200), new Vector2(2, 20), "route", 0, 0, true, MathHelper.PiOver4, -1));
            LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, 0, 0), new Vector3(-2450, 0, 575), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, 1));
            LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, 0, 0), new Vector3(-2450, 0, 375), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, 1));
            LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, 0, 0), new Vector3(-2450, 0, 175), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, 1));
            LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, 0, 0), new Vector3(-2450, 0, -25), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, 1));
            LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, 0, 0), new Vector3(-2450, 0, -225), new Vector2(100, 200), new Vector2(2, 20), "route", 0, 0, true, MathHelper.PiOver4, -1));
            LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, -MathHelper.PiOver2, 0), new Vector3(-2300, 0, -400), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, 1));
            LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, -MathHelper.PiOver2, 0), new Vector3(-2100, 0, -400), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, 1));
            LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, -MathHelper.PiOver2, 0), new Vector3(-1900, 0, -400), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, 1));
            LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, -MathHelper.PiOver2, 0), new Vector3(-1700, 0, -400), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, 1));
            LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, -MathHelper.PiOver2, 0), new Vector3(-1500, 0, -400), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, 1));
            LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, -MathHelper.PiOver2, 0), new Vector3(-1300, 0, -400), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, 1));
            LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, -MathHelper.PiOver2, 0), new Vector3(-1100, 0, -400), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, 1));
            LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, -MathHelper.PiOver2, 0), new Vector3(-900, 0, -400), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, 1));
            LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, -MathHelper.PiOver2, 0), new Vector3(-700, 0, -400), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, 1));
            LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, -MathHelper.PiOver2, 0), new Vector3(-500, 0, -400), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, 1));
            LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, -MathHelper.PiOver2, 0), new Vector3(-300, 0, -400), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, 1));
            LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, MathHelper.Pi, 0), new Vector3(0, 0, -200), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, 1));
            LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, -MathHelper.PiOver2, 0), new Vector3(-150, 0, -400), new Vector2(100, 200), new Vector2(2, 20), "route", 0, 0, true, MathHelper.PiOver4, -1));

            Mustang = new Voiture(this, "MustangGT500", 0.0088f, new Vector3(0, 0, 0), new Vector3(-70, 0, -100), INTERVALLE_MAJ_STANDARD, true);
            AI = new Voiture(this, "MustangGT500", 0.0088f, new Vector3(0, 0, 0), new Vector3(-30, 0, -100), INTERVALLE_MAJ_STANDARD, false);

            ListeVoiture.Add(Mustang);
            ListeVoiture.Add(AI);

            CheckPoint = new bool[LaPiste.Count()];

            ÉtatCollisionPiste = new bool[LaPiste.Count()];
            EstEnCollisionAvecUnePiste = false;
            TempsÉcouléDepuisMAJ = 0;

            CibleYCaméra = 0;
            VueArrière = 1;
            TableauPositionCaméra = new Vector3[6];
            IndexPositionCaméra = 0;
            //Vector3 positionCaméra = new Vector3(0, 50, -120);
            Vector3 positionCaméra = new Vector3(0, 20, -5070);
            PositionCaméra = new Vector3(-80, 20, -80);

            Vector3 cibleCaméra = new Vector3(0, 0, 0);

            GestionnaireDeFonts = new RessourcesManager<SpriteFont>(this, "Fonts");
            GestionnaireDeTextures = new RessourcesManager<Texture2D>(this, "Textures");
            GestionnaireDeModèles = new RessourcesManager<Model>(this, "Models");
            GestionnaireDeShaders = new RessourcesManager<Effect>(this, "Effects");
            GestionnaireDeSon = new RessourcesManager<SoundEffect>(this, "Sounds");
            GestionnaireDeMusique = new RessourcesManager<Song>(this, "Songs");
            GestionInput = new InputManager(this);
            CaméraJeu = new CaméraSubjective(this, positionCaméra, cibleCaméra, new Vector3(0, 1, 0), INTERVALLE_MAJ_STANDARD);

            Components.Add(GestionInput);
            Components.Add(CaméraJeu);
            Components.Add(new Afficheur3D(this));
            Components.Add(new AfficheurFPS(this, INTERVALLE_CALCUL_FPS));
            Components.Add(Carte);

            foreach (Sol x in LaPiste)
            {
                Components.Add(x);
            }

            foreach (Voiture x in ListeVoiture)
            {
               Components.Add(x);
            }

            NbVoiture = ListeVoiture.Count();


            Interface = new GUI(this, INTERVALLE_MAJ_STANDARD, "aiguille", "speedometer", NbVoiture, NbTours);
            Components.Add(Interface);

            Services.AddService(typeof(RessourcesManager<SpriteFont>), GestionnaireDeFonts);
            Services.AddService(typeof(RessourcesManager<Texture2D>), GestionnaireDeTextures);
            Services.AddService(typeof(RessourcesManager<Model>), GestionnaireDeModèles);
            Services.AddService(typeof(RessourcesManager<Effect>), GestionnaireDeShaders);
            Services.AddService(typeof(RessourcesManager<SoundEffect>), GestionnaireDeSon);
            Services.AddService(typeof(RessourcesManager<Song>), GestionnaireDeMusique);
            Services.AddService(typeof(InputManager), GestionInput);
            Services.AddService(typeof(Caméra), CaméraJeu);
            GestionSprites = new SpriteBatch(GraphicsDevice);
            Services.AddService(typeof(SpriteBatch), GestionSprites);
            base.Initialize();
        }

        protected override void Update(GameTime gameTime)
        {
            GérerClavier();
            TempsÉcouléDepuisMAJ += (float)gameTime.ElapsedGameTime.TotalSeconds;
            //Mustang.Position = new Vector3(Mustang.Position.X, Carte.GetHauteur(Mustang.Position), Mustang.Position.Z);
            if (TempsÉcouléDepuisMAJ >= INTERVALLE_MAJ_STANDARD)
            {
                bool collisionTouteFausse = true;
                GestionOrientationCaméra();
                CaméraJeu.CréerPointDeVue();
                //Mustang.Position = new Vector3(Mustang.Position.X, Carte.GetHauteur(Mustang.Position), Mustang.Position.Z);
                AI.Vitesse = 1f;
                Mustang.GestionCollisionVoiture(AI);
                for (int i = 0; i < LaPiste.Count(); i++)
                {
                    ÉtatCollisionPiste[i] = Mustang.GestionCollisionPiste(LaPiste[i]);
                }
                for (int i = 0; i < ÉtatCollisionPiste.Length && collisionTouteFausse; i++)
                {
                    if (ÉtatCollisionPiste[i])
                    {
                        collisionTouteFausse = false;
                    }
                }
                if (collisionTouteFausse)
                {
                    Mustang.GetHauteur();
                    Mustang.VariationInclinaison();
                }

                GestionCourse();
                Interface.UpdateGUI((int)Mustang.PixelToKMH(Mustang.Vitesse), PositionUtilisateur, CheckPoint, ToursFait);
                
                TempsÉcouléDepuisMAJ = 0;
            }
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.SkyBlue);
            GestionSprites.Begin();
            DebugShapeRenderer.Draw(gameTime, CaméraJeu.Vue, CaméraJeu.Projection);
            //DebugShapeRenderer.AddBoundingBox(Piste.Box, Color.Wheat);
            //DebugShapeRenderer.AddBoundingBox(Piste.BoxDroite, Color.YellowGreen);
            //DebugShapeRenderer.AddBoundingBox(Piste.BoxGauche, Color.YellowGreen);
            base.Draw(gameTime);
            GestionSprites.End();
        }

        private void GérerClavier()
        {
            if (GestionInput.EstEnfoncée(Keys.Escape))
            {
                Exit();
            }
            Vector3 arrièreRapproché = new Vector3(-80, 20, -80);
            Vector3 arrièreMoyen = new Vector3(-90, 25, -90);
            Vector3 arrièreLoin = new Vector3(-100, 30, -100);
            Vector3 intérieurConducteur = new Vector3(-20, 11, -20);
            Vector3 capo = new Vector3(-10, 12, -10);
            Vector3 vueArrière = new Vector3(70, 20, 70);
            TableauPositionCaméra[0] = arrièreRapproché;
            TableauPositionCaméra[1] = arrièreMoyen;
            TableauPositionCaméra[2] = arrièreLoin;
            TableauPositionCaméra[3] = intérieurConducteur;
            TableauPositionCaméra[4] = capo;
            TableauPositionCaméra[5] = vueArrière;

            if (GestionInput.EstNouvelleTouche(Keys.NumPad1))
            {
                PositionCaméra = arrièreRapproché;
                CibleYCaméra = 0;
                VueArrière = 1;
                IndexPositionCaméra = 0;
            }
            else if (GestionInput.EstNouvelleTouche(Keys.NumPad2))
            {
                PositionCaméra = arrièreMoyen;
                CibleYCaméra = -0.04f;
                VueArrière = 1;
                IndexPositionCaméra = 1;
            }
            else if (GestionInput.EstNouvelleTouche(Keys.NumPad3))
            {
                PositionCaméra = arrièreLoin;
                CibleYCaméra = -0.06f;
                VueArrière = 1;
                IndexPositionCaméra = 2;
            }
            else if (GestionInput.EstNouvelleTouche(Keys.NumPad4))
            {
                PositionCaméra = intérieurConducteur;
                CibleYCaméra = 0;
                VueArrière = 1;
                IndexPositionCaméra = 3;
            }
            else if(GestionInput.EstNouvelleTouche(Keys.NumPad5))
            {
                PositionCaméra = capo;
                CibleYCaméra = 0;
                VueArrière = 1;
                IndexPositionCaméra = 4;
            }
            else if (GestionInput.EstNouvelleTouche(Keys.NumPad6))
            {
                PositionCaméra = vueArrière;
                CibleYCaméra = 0;
                VueArrière = -1;
                IndexPositionCaméra = 5;
            }
        }

        private void GestionOrientationCaméra()
        {
            float orientationX = (float)Math.Sin(Mustang.Rotation.Y);
            float orientationZ = (float)Math.Cos(Mustang.Rotation.Y);
            Vector3 cible = new Vector3(VueArrière * orientationX, VueArrière * CaméraJeu.Direction.Y, VueArrière * orientationZ);
            Vector3 ciblePosition = Mustang.AvanceCaméra() +
                PositionCaméra * new Vector3((float)Math.Sin(Mustang.Rotation.Y), 1, (float)Math.Cos(Mustang.Rotation.Y));

            CaméraJeu.Direction = Vector3.Lerp(CaméraJeu.Direction, cible, 0.1f);
            //CaméraJeu.Position = Vector3.Lerp(CaméraJeu.Position, ciblePosition, 0.1f);
            CaméraJeu.Position = new Vector3(Vector3.Lerp(CaméraJeu.Position, ciblePosition, 0.1f).X, Vector3.Lerp(CaméraJeu.Position, ciblePosition, 1.0f).Y,
                Vector3.Lerp(CaméraJeu.Position, ciblePosition, 0.1f).Z);

            PositionCaméra = new Vector3(TableauPositionCaméra[IndexPositionCaméra].X - -45 * (Mustang.PixelToKMH(Mustang.Vitesse) / 100.0f), TableauPositionCaméra[IndexPositionCaméra].Y, TableauPositionCaméra[IndexPositionCaméra].Z - -45 * (Mustang.PixelToKMH(Mustang.Vitesse) / 100.0f));
        }

        void GestionCourse()
        {
            string debug = "";

            if (ToursFait < NbTours)
            {
                if (LaPiste.TrueForAll(x => x.Franchi))
                {
                    for (int j = 0; j < LaPiste.Count(); j++)
                    {
                        LaPiste[j].Franchi = false;
                    }
                    Interface.NbCheckPointFranchis = 0;
                    ToursFait++;
                }
                for (int j = 0; j < LaPiste.Count(); j++)
                {
                    CheckPoint[j] = LaPiste[j].Franchi;
                    debug += LaPiste[j].Franchi + " ";
                }
                LaPiste[LaPiste.Count() - 1].Franchi = false;   
            }

            for (int i = 0; i < LaPiste.Count(); i++)
            {
                if (Mustang.BoxVoiture.Intersects(LaPiste[i].BoxÉtape))
                {
                    LaPiste[i].Franchi = true;
                }
            }

            //Window.Title = debug + " : " + ToursFait.ToString();
        }
    }
}
