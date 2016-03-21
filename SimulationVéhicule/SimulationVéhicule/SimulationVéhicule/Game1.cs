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
        bool[,] ÉtatCollisionPiste { get; set; }
        bool EstEnCollisionAvecUnePiste { get; set; }

        Vector3 PositionCaméra { get; set; }
        float CibleYCaméra { get; set; }
        int VueArrière { get; set; }
        Vector3[] TableauPositionCaméra { get; set; }
        int IndexPositionCaméra { get; set; }

        float TempsÉcouléDepuisMAJ { get; set; }

        public InputManager GestionInput { get; private set; }

        GUI Interface { get; set; }
        Course LaCourse { get; set; }

        //Course
        int PositionUtilisateur { get; set; }
        int NbVoiture { get; set; }
        bool[] CheckPoint { get; set; }
        int NbTours { get; set; }
        int ToursFait { get; set; }

        int IDVoitureUtilisateur { get; set; }

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
            PériphériqueGraphique.IsFullScreen = false;
            PériphériqueGraphique.PreferredBackBufferWidth = 900;
            PériphériqueGraphique.PreferredBackBufferHeight = 600;
            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += new EventHandler<EventArgs>(Window_ClientSizeChanged);
            PériphériqueGraphique.ApplyChanges();

            PositionUtilisateur = 1;
            IDVoitureUtilisateur = 0;
            NbTours = 2;
            ToursFait = 0;
            Carte = new Terrain(this, 1f, Vector3.Zero, new Vector3(0, -1258, 0), new Vector3(25600, 1000, 25600), "CarteTest", "DétailsTerrain2", 5, INTERVALLE_MAJ_STANDARD, CaméraJeu);
            LaPiste = new List<Sol>();
            ListeVoiture = new List<Voiture>();
            Mustang = new Voiture(this, "MustangGT500SansRoue", 0.0088f, new Vector3(0, 0, 0), new Vector3(-3861, 0, 1877), INTERVALLE_MAJ_STANDARD, true);
            AI = new Voiture(this, "MustangGT500SansRoue", 0.0088f, new Vector3(0, 0, 0), new Vector3(-30, 0, -0), INTERVALLE_MAJ_STANDARD, false);
            ListeVoiture.Add(Mustang);
            ListeVoiture.Add(AI);
            NbVoiture = ListeVoiture.Count();


            LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, MathHelper.Pi, 0), new Vector3(-3800, 0, 1800), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, NbVoiture));
            LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, MathHelper.Pi, 0), new Vector3(-3800, 0, 2000), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, NbVoiture));
            LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, MathHelper.Pi, 0), new Vector3(-3800, 0, 2200), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, NbVoiture));
            LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, MathHelper.Pi, 0), new Vector3(-3800, 0, 2400), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, NbVoiture));
            LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, MathHelper.Pi, 0), new Vector3(-3800, 0, 2600), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, NbVoiture));
            LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, MathHelper.Pi, 0), new Vector3(-3800, 0, 2800), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, NbVoiture));
            LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, MathHelper.Pi, 0), new Vector3(-3800, 0, 3000), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, NbVoiture));
            LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, MathHelper.Pi, 0), new Vector3(-3800, 0, 3200), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, NbVoiture));
            LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, MathHelper.Pi, 0), new Vector3(-3800, 0, 3400), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, NbVoiture));
            LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, MathHelper.Pi, 0), new Vector3(-3800, 0, 3600), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, NbVoiture));
            LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, MathHelper.Pi, 0), new Vector3(-3800, 0, 3800), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, NbVoiture));
            LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, MathHelper.Pi, 0), new Vector3(-3800, 0, 4000), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, NbVoiture));
            LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, MathHelper.Pi, 0), new Vector3(-3800, 0, 4200), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, NbVoiture));
            LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, MathHelper.Pi, 0), new Vector3(-3800, 0, 4400), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, NbVoiture));
            LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, MathHelper.Pi, 0), new Vector3(-3800, 0, 4600), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, NbVoiture));
            LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, MathHelper.Pi, 0), new Vector3(-3800, 0, 4800), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, NbVoiture));
            LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, MathHelper.Pi, 0), new Vector3(-3800, 0, 5000), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, NbVoiture));
            LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, MathHelper.Pi, 0), new Vector3(-3800, 0, 5200), new Vector2(100, 200), new Vector2(2, 20), "route", 0, 0, true, 0, NbVoiture));
            LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, -MathHelper.PiOver2, 0), new Vector3(-3800, 0, 5200), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, NbVoiture));
            LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, -MathHelper.PiOver2, 0), new Vector3(-3600, 0, 5200), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, NbVoiture));
            LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, -MathHelper.PiOver2, 0), new Vector3(-3400, 0, 5200), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, NbVoiture));
            LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, -MathHelper.PiOver2, 0), new Vector3(-3200, 0, 5200), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, NbVoiture));
            LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, -MathHelper.PiOver2, 0), new Vector3(-3000, 0, 5200), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, NbVoiture));
            LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, -MathHelper.PiOver2, 0), new Vector3(-2800, 0, 5200), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, NbVoiture));
            LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, -MathHelper.PiOver2, 0), new Vector3(-2600, 0, 5200), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, NbVoiture));
            LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, -MathHelper.PiOver2, 0), new Vector3(-2400, 0, 5200), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, NbVoiture));
            LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, -MathHelper.PiOver2, 0), new Vector3(-2200, 0, 5200), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, NbVoiture));
            LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, -MathHelper.PiOver2, 0), new Vector3(-2000, 0, 5200), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, NbVoiture));
            LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, -MathHelper.PiOver2, 0), new Vector3(-1800, 0, 5200), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, NbVoiture));
            LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, -MathHelper.PiOver2, 0), new Vector3(-1600, 0, 5200), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, NbVoiture));
            LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, -MathHelper.PiOver2, 0), new Vector3(-1400, 0, 5200), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, NbVoiture));
            LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, -MathHelper.PiOver2, 0), new Vector3(-1200, 0, 5200), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, NbVoiture));
            LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, -MathHelper.PiOver2, 0), new Vector3(-1000, 0, 5300), new Vector2(100, 200), new Vector2(2, 20), "route", 0, 0, true, MathHelper.PiOver2, NbVoiture));
            LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, MathHelper.Pi, 0), new Vector3(-900, 0, 5300), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, NbVoiture));
            LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, MathHelper.Pi, 0), new Vector3(-900, 0, 5500), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, NbVoiture));
            LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, MathHelper.Pi, 0), new Vector3(-900, 0, 5700), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, NbVoiture));
            LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, MathHelper.Pi, 0), new Vector3(-900, 0, 5900), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, NbVoiture));
            LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, MathHelper.Pi, 0), new Vector3(-900, 0, 6100), new Vector2(100, 200), new Vector2(2, 20), "route", 0, 0, true,0, NbVoiture));
            LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, -MathHelper.PiOver2, 0), new Vector3(-900, 0, 6100), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, NbVoiture));
            LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, -MathHelper.PiOver2, 0), new Vector3(-700, 0, 6100), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, NbVoiture));
            LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, -MathHelper.PiOver2, 0), new Vector3(-500, 0, 6100), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, NbVoiture));
            LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, -MathHelper.PiOver2, 0), new Vector3(-300, 0, 6100), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, NbVoiture));
            LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, -MathHelper.PiOver2, 0), new Vector3(-200, 0, 6100), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, NbVoiture));
            LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, -MathHelper.PiOver2, 0), new Vector3(0, 0, 6100), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, NbVoiture));
            LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, -MathHelper.PiOver2, 0), new Vector3(200, 0, 6100), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, NbVoiture));
            LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, -MathHelper.PiOver2, 0), new Vector3(400, 0, 6100), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, NbVoiture));
            LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, -MathHelper.PiOver2, 0), new Vector3(600, 0, 6100), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, NbVoiture));
            LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, -MathHelper.PiOver2, 0), new Vector3(800, 0, 6100), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, NbVoiture));
            LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, -MathHelper.PiOver2, 0), new Vector3(1000, 0, 6100), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, NbVoiture));
            LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, -MathHelper.PiOver2, 0), new Vector3(1200, 0, 6100), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, NbVoiture));
            LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, -MathHelper.PiOver2, 0), new Vector3(1400, 0, 6100), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, NbVoiture));
            LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, -MathHelper.PiOver2, 0), new Vector3(1600, 0, 6100), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, NbVoiture));
            LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, -MathHelper.PiOver2, 0), new Vector3(1800, 0, 6100), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, NbVoiture));
            LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, -MathHelper.PiOver2, 0), new Vector3(2000, 0, 6100), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, NbVoiture));
            LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, -MathHelper.PiOver2, 0), new Vector3(2200, 0, 6100), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, NbVoiture));
            LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, -MathHelper.PiOver2, 0), new Vector3(2400, 0, 6100), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, NbVoiture));
            LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, -MathHelper.PiOver2, 0), new Vector3(2600, 0, 6100), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, NbVoiture));
            LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, -MathHelper.PiOver2, 0), new Vector3(2800, 0, 6100), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, NbVoiture));
            LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, -MathHelper.PiOver2, 0), new Vector3(3000, 0, 6100), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, NbVoiture));
            LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, -MathHelper.PiOver2, 0), new Vector3(3200, 0, 6100), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, NbVoiture));
            LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, -MathHelper.PiOver2, 0), new Vector3(3400, 0, 6100), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, NbVoiture));
            LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, -MathHelper.PiOver2, 0), new Vector3(3600, 0, 6100), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, NbVoiture));
            LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, -MathHelper.PiOver2, 0), new Vector3(3800, 0, 6100), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, NbVoiture));
            LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, -MathHelper.PiOver2, 0), new Vector3(4000, 0, 6100), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, NbVoiture));

            //LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, MathHelper.Pi, 0), new Vector3(0, 0, 0), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, NbVoiture));
            //LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, MathHelper.Pi, 0), new Vector3(0, 0, 200), new Vector2(100, 200), new Vector2(2, 20), "route", 15, 2 * MathHelper.Pi, false, 0, NbVoiture));
            //LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, MathHelper.Pi, 0), new Vector3(0, 0, 400), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, NbVoiture));
            //LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, MathHelper.Pi, 0), new Vector3(-100, 0, 600), new Vector2(100, 200), new Vector2(2, 20), "route", 0, 0, true, MathHelper.PiOver2, NbVoiture));
            //LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, MathHelper.PiOver2, 0), new Vector3(-100, -0, 700), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, NbVoiture));
            //LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, MathHelper.PiOver2, 0), new Vector3(-275, 0, 700), new Vector2(100, 200), new Vector2(2, 4), "route", 0, 0, false, 0, NbVoiture));
            //LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, MathHelper.PiOver2, 0), new Vector3(-475, 0, 700), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, NbVoiture));
            
            //LaPiste.Add(new Sol(this, 1.0f, new Vector3(MathHelper.Pi / 8f, MathHelper.PiOver2, 0), new Vector3(-675, 0, 700), new Vector2(100, 800), new Vector2(2, 8), "route", 0, 0, false, 0, NbVoiture));
            //LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, MathHelper.PiOver2, 0), new Vector3(-675, 0, 700), new Vector2(100, 800), new Vector2(2, 8), "route", 0, 0, false, 0, NbVoiture));

            //LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, MathHelper.PiOver2, 0), new Vector3(-1875, 50, 700), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, NbVoiture));
            
            //LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, MathHelper.PiOver2, 0), new Vector3(-1875, 0, 700), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, NbVoiture));

            //LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, MathHelper.PiOver2, 0), new Vector3(-2075, 0, 700), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, NbVoiture));
            //LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, MathHelper.PiOver2, 0), new Vector3(-2275, 0, 600), new Vector2(100, 200), new Vector2(2, 20), "route", 0, 0, true, MathHelper.PiOver2, NbVoiture));
            //LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, 0, 0), new Vector3(-2375, 0, 600), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, NbVoiture));
            //LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, 0, 0), new Vector3(-2375, 0, 400), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, NbVoiture));
            //LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, 0, 0), new Vector3(-2375, 0, 200), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, NbVoiture));
            //LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, 0, 0), new Vector3(-2375, 0, 0), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, NbVoiture));
            //LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, 0, 0), new Vector3(-2275, 0, -200), new Vector2(100, 200), new Vector2(2, 20), "route", 0, 0, true, MathHelper.PiOver2, NbVoiture));
            //LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, -MathHelper.PiOver2, 0), new Vector3(-2275, 0, -300), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, NbVoiture));
            //LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, -MathHelper.PiOver2, 0), new Vector3(-2100, 0, -300), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, NbVoiture));
            //LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, -MathHelper.PiOver2, 0), new Vector3(-1900, 0, -300), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, NbVoiture));
            //LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, -MathHelper.PiOver2, 0), new Vector3(-1700, 0, -300), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, NbVoiture));
            //LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, -MathHelper.PiOver2, 0), new Vector3(-1500, 0, -300), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, NbVoiture));
            //LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, -MathHelper.PiOver2, 0), new Vector3(-1300, 0, -300), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, NbVoiture));
            //LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, -MathHelper.PiOver2, 0), new Vector3(-1100, 0, -300), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, NbVoiture));
            //LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, -MathHelper.PiOver2, 0), new Vector3(-900, 0, -300), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, NbVoiture));
            //LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, -MathHelper.PiOver2, 0), new Vector3(-700, 0, -300), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, NbVoiture));
            //LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, -MathHelper.PiOver2, 0), new Vector3(-500, 0, -300), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, NbVoiture));
            //LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, -MathHelper.PiOver2, 0), new Vector3(-300, 0, -300), new Vector2(100, 200), new Vector2(2, 2), "route", 0, 0, false, 0, NbVoiture));
            //LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, -MathHelper.PiOver2, 0), new Vector3(-100, 0, -200), new Vector2(100, 200), new Vector2(2, 20), "route", 0, 0, true, MathHelper.PiOver2, NbVoiture));
            //LaPiste.Add(new Sol(this, 1.0f, new Vector3(0, MathHelper.Pi, 0), new Vector3(0, 0, -200), new Vector2(100, 200), new Vector2(2, 20), "route", 15, 2 * MathHelper.Pi, false, 0, NbVoiture));
            //bug hauteur??

            CheckPoint = new bool[LaPiste.Count()];

            ÉtatCollisionPiste = new bool[LaPiste.Count(), NbVoiture];
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

            LaCourse = new Course(this, NbTours, NbVoiture, LaPiste, ListeVoiture);
            Components.Add(LaCourse);

            Interface = new GUI(this, INTERVALLE_MAJ_STANDARD, "aiguille2", "speedometer3", LaCourse.NbVoiture, LaCourse.NbTours, IDVoitureUtilisateur, new Vector2(Window.ClientBounds.Width, Window.ClientBounds.Height));
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
            Window.ClientSizeChanged += new EventHandler<EventArgs>(Window_ClientSizeChanged);
            GérerClavier();
            Window.Title = ListeVoiture[0].Position.ToString();
            TempsÉcouléDepuisMAJ += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (TempsÉcouléDepuisMAJ >= INTERVALLE_MAJ_STANDARD)
            {
                GestionOrientationCaméra();
                CaméraJeu.CréerPointDeVue();
                ListeVoiture[1].Vitesse = 1f;
                for (int i = 1; i < ListeVoiture.Count(); i++)
                {
                    ListeVoiture[IDVoitureUtilisateur].GestionCollisionVoiture(ListeVoiture[i]);

                }
                for (int v = 0; v < NbVoiture; v++)
                {
                    bool collisionTouteFausse = true;
                    for (int i = 0; i < LaPiste.Count(); i++)
                    {
                        ÉtatCollisionPiste[i,v] = ListeVoiture[v].GestionCollisionPiste(LaPiste[i]);
                    }
                    for (int i = 0; i < ÉtatCollisionPiste.GetLength(0) && collisionTouteFausse; i++)
                    {
                        if (ÉtatCollisionPiste[i,v])
                        {
                            collisionTouteFausse = false;
                        }
                    }
                    if (collisionTouteFausse)
                    {
                        ListeVoiture[v].GetHauteur();
                        //ListeVoiture[IDVoitureUtilisateur].VariationInclinaison();
                    }
                }

                Interface.UpdateGUI((int)ListeVoiture[IDVoitureUtilisateur].PixelToKMH(ListeVoiture[IDVoitureUtilisateur].Vitesse),
                    LaCourse.PositionUtilisateur, LaCourse.CheckPointParVoiture[IDVoitureUtilisateur], 
                    LaCourse.ToursFait[IDVoitureUtilisateur]);

                //Window.Title = LaCourse.FranchiParVoiture[0][0].ToString() + " - " + LaCourse.FranchiParVoiture[1][0].ToString();

                TempsÉcouléDepuisMAJ = 0;
            }

            //Interface.UpdateScreenSize(new Vector2(Window.ClientBounds.Width, Window.ClientBounds.Height));
            //Window.Title = Window.ClientBounds.ToString();
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.SkyBlue);
            GestionSprites.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);
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
            else if (GestionInput.EstNouvelleTouche(Keys.NumPad5))
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
            float orientationX = (float)Math.Sin(ListeVoiture[IDVoitureUtilisateur].Rotation.Y);
            float orientationZ = (float)Math.Cos(ListeVoiture[IDVoitureUtilisateur].Rotation.Y);
            Vector3 cible = new Vector3(VueArrière * orientationX, VueArrière * CaméraJeu.Direction.Y, VueArrière * orientationZ);
            Vector3 ciblePosition = ListeVoiture[IDVoitureUtilisateur].Position +
                PositionCaméra * new Vector3((float)Math.Sin(ListeVoiture[IDVoitureUtilisateur].Rotation.Y), 1, (float)Math.Cos(ListeVoiture[IDVoitureUtilisateur].Rotation.Y));

            CaméraJeu.Direction = Vector3.Lerp(CaméraJeu.Direction, cible, 0.1f);
            CaméraJeu.Position = new Vector3(Vector3.Lerp(CaméraJeu.Position, ciblePosition, 0.1f).X, Vector3.Lerp(CaméraJeu.Position, ciblePosition, 1.0f).Y,
                Vector3.Lerp(CaméraJeu.Position, ciblePosition, 0.1f).Z);

            PositionCaméra = new Vector3(TableauPositionCaméra[IndexPositionCaméra].X - -45 * (ListeVoiture[IDVoitureUtilisateur].PixelToKMH(ListeVoiture[IDVoitureUtilisateur].Vitesse) / 100.0f), TableauPositionCaméra[IndexPositionCaméra].Y, TableauPositionCaméra[IndexPositionCaméra].Z - -45 * (ListeVoiture[IDVoitureUtilisateur].PixelToKMH(ListeVoiture[IDVoitureUtilisateur].Vitesse) / 100.0f));
        }

        void Window_ClientSizeChanged(object sender, EventArgs e)
        {
            Interface.UpdateScreenSize(new Vector2(Window.ClientBounds.Width, Window.ClientBounds.Height));          
        }
    }
}
