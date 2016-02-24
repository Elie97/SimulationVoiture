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

        CaméraSubjective CaméraJeu { get; set; }

        Voiture Mustang { get; set; }
        Voiture AI { get; set; }
        Terrain Carte { get; set; }

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

            Mustang = new Voiture(this, "MustangGT500", 0.0088f, new Vector3(0, 0, 0), new Vector3(0, 0, 0), INTERVALLE_MAJ_STANDARD, true);
            AI = new Voiture(this, "MustangGT500", 0.0088f, new Vector3(0, 0, 0), new Vector3(0, 0, -4500), INTERVALLE_MAJ_STANDARD, false);
            Carte = new Terrain(this, 1f, Vector3.Zero, Vector3.Zero, new Vector3(25600, 250, 25600), "Terrain", "DétailsTerrain", 5, INTERVALLE_MAJ_STANDARD);
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
            GestionInput = new InputManager(this);
            CaméraJeu = new CaméraSubjective(this, positionCaméra, cibleCaméra, new Vector3(0, 1, 0), INTERVALLE_MAJ_STANDARD);

            Components.Add(GestionInput);
            Components.Add(CaméraJeu);
            Components.Add(new Afficheur3D(this));
            Components.Add(new AfficheurFPS(this, INTERVALLE_CALCUL_FPS));
           // Components.Add(Carte);
            Components.Add(new Sol(this, 1f, Vector3.Zero, new Vector3(0, 0, 0), new Vector3(1, 0, 1), new Vector3(100, 0, 100), 1.0f, Color.Blue));
            Components.Add(Mustang);
            Components.Add(AI);
            Services.AddService(typeof(RessourcesManager<SpriteFont>), GestionnaireDeFonts);
            Services.AddService(typeof(RessourcesManager<Texture2D>), GestionnaireDeTextures);
            Services.AddService(typeof(RessourcesManager<Model>), GestionnaireDeModèles);
            Services.AddService(typeof(RessourcesManager<Effect>), GestionnaireDeShaders);
            Services.AddService(typeof(RessourcesManager<SoundEffect>), GestionnaireDeSon);
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
                GestionOrientationCaméra();
                CaméraJeu.CréerPointDeVue();
                //Mustang.Position = new Vector3(Mustang.Position.X, Carte.GetHauteur(Mustang.Position), Mustang.Position.Z);
                //AI.Vitesse = 2f;
                Mustang.GestionCollisionVoiture(AI);
                TempsÉcouléDepuisMAJ = 0;
            }
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.SkyBlue);
            GestionSprites.Begin();
            Window.Title = Carte.GetHauteur(Mustang.Position).ToString() + " - " + Mustang.Position.ToString(); 
            DebugShapeRenderer.Draw(gameTime, CaméraJeu.Vue, CaméraJeu.Projection);
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
            CaméraJeu.Position = Vector3.Lerp(CaméraJeu.Position, ciblePosition, 0.1f);

            PositionCaméra = new Vector3(TableauPositionCaméra[IndexPositionCaméra].X - -45 * (Mustang.PixelToKMH(Mustang.Vitesse) / 100.0f), TableauPositionCaméra[IndexPositionCaméra].Y, TableauPositionCaméra[IndexPositionCaméra].Z - -45 * (Mustang.PixelToKMH(Mustang.Vitesse) / 100.0f));
        }
    }
}
