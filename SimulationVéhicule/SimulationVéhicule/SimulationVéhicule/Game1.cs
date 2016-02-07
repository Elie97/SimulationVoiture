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

        Vector3 PositionCaméra { get; set; }
        float CibleYCaméra { get; set; }
        int VueArrière { get; set; }

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
            Mustang = new Voiture(this, "MustangGT500", 0.0088f, new Vector3(0, 0, 0), new Vector3(0, 0, -5000), INTERVALLE_MAJ_STANDARD);

            CibleYCaméra = 0;
            VueArrière = 1;
            //Vector3 positionCaméra = new Vector3(0, 50, -120);
            Vector3 positionCaméra = new Vector3(0, 20, -5070);
            PositionCaméra = new Vector3(0, 20, -70);

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
            Components.Add(new Sol(this, 1f, Vector3.Zero, new Vector3(0, 500, 0), new Vector3(1, 0, 1), new Vector3(100, 0, 10000), 1.0f, Color.White));
            Components.Add(new Sol(this, 1f, Vector3.Zero, new Vector3(0, 0, -5000), new Vector3(1, 0, 1), new Vector3(10000, 0, 100), 1.0f, Color.Blue));
            //Components.Add(new Sol(this, 1f, Vector3.Zero, new Vector3(0, 0, 20 * MÈTRE), new Vector3(1, 0, 1), new Vector3(KILOMÈTRE, 0, 20 * MÈTRE), 1.0f, Color.Green));
            //Components.Add(new Sol(this, 1f, Vector3.Zero, new Vector3(0, 0, 30 * MÈTRE), new Vector3(1, 0, 1), new Vector3(KILOMÈTRE, 0, 20 * MÈTRE), 1.0f, Color.Yellow));
            //Components.Add(new Sol(this, 1f, Vector3.Zero, new Vector3(0, 0, 40 * MÈTRE), new Vector3(1, 0, 1), new Vector3(KILOMÈTRE, 0, 20 * MÈTRE), 1.0f, Color.Black));
            //Components.Add(new Sol(this, 1f, Vector3.Zero, new Vector3(0, 0, 50 * MÈTRE), new Vector3(1, 0, 1), new Vector3(KILOMÈTRE, 0, 20 * MÈTRE), 1.0f, Color.Orange));
            //Components.Add(new Sol(this, 1f, Vector3.Zero, new Vector3(0, 0, 60 * MÈTRE), new Vector3(1, 0, 1), new Vector3(KILOMÈTRE, 0, 20 * MÈTRE), 1.0f, Color.Pink));
            //Components.Add(new Sol(this, 1f, Vector3.Zero, new Vector3(0, 0, 70 * MÈTRE), new Vector3(1, 0, 1), new Vector3(KILOMÈTRE, 0, 20 * MÈTRE), 1.0f, Color.Brown));
            //Components.Add(new Sol(this, 1f, Vector3.Zero, new Vector3(0, 0, 80 * MÈTRE), new Vector3(1, 0, 1), new Vector3(KILOMÈTRE, 0, 20 * MÈTRE), 1.0f, Color.Red));
            Components.Add(Mustang);

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
            CaméraJeu.Direction = new Vector3(VueArrière * (float)Math.Sin(Mustang.Rotation.Y), CibleYCaméra, VueArrière * (float)Math.Cos(Mustang.Rotation.Y));
            CaméraJeu.Position = new Vector3((PositionCaméra.Z * (float)Math.Sin(Mustang.Rotation.Y)) + Mustang.Position.X, (PositionCaméra.Y) + Mustang.Position.Y, (PositionCaméra.Z * (float)Math.Cos(Mustang.Rotation.Y)) + Mustang.Position.Z);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.SkyBlue);
            //Window.Title = Mustang.Position.ToString();
            base.Draw(gameTime);
        }

        private void GérerClavier()
        {
            if (GestionInput.EstEnfoncée(Keys.Escape))
            {
                Exit();
            }

            Vector3 arrièreRapproché = new Vector3(0, 20, -70);
            Vector3 arrièreMoyen = new Vector3(0, 25, -80);
            Vector3 arrièreLoin = new Vector3(0, 30, -90);
            Vector3 intérieurConducteur = new Vector3(0, 11, -20);
            Vector3 capo = new Vector3(0, 12, -8);
            Vector3 vueArrière = new Vector3(0, 20, 55);

            if (GestionInput.EstNouvelleTouche(Keys.NumPad1))
            {
                PositionCaméra = arrièreRapproché;
                CibleYCaméra = 0;
                VueArrière = 1;
            }
            else if (GestionInput.EstNouvelleTouche(Keys.NumPad2))
            {
                PositionCaméra = arrièreMoyen;
                CibleYCaméra = -0.04f;
                VueArrière = 1;
            }
            else if (GestionInput.EstNouvelleTouche(Keys.NumPad3))
            {
                PositionCaméra = arrièreLoin;
                CibleYCaméra = -0.06f;
                VueArrière = 1;
            }
            else if (GestionInput.EstNouvelleTouche(Keys.NumPad4))
            {
                PositionCaméra = intérieurConducteur;
                CibleYCaméra = 0;
                VueArrière = 1;
            }
            else if(GestionInput.EstNouvelleTouche(Keys.NumPad5))
            {
                PositionCaméra = capo;
                CibleYCaméra = 0;
                VueArrière = 1;
            }
            else if (GestionInput.EstNouvelleTouche(Keys.NumPad6))
            {
                PositionCaméra = vueArrière;
                CibleYCaméra = 0;
                VueArrière = -1;
            }
        }

        float GetAngle(float angle)
        {
            angle = angle / (float)(2*Math.PI);
            if (angle >= 2 * Math.PI)
            {
                angle -= 2 * (float)Math.PI;
            }
            if (angle <= -2 * Math.PI)
	        {
                angle += 2 * (float)Math.PI;
	        }
            return angle;
        }
    }
}
