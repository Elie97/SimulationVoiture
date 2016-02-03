﻿using System;
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
        Caméra CaméraJeu { get; set; }

        Voiture Mustang { get; set; }

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

            Vector3 positionCaméra = new Vector3(-25, 50, -120);
            Vector3 cibleCaméra = new Vector3(0, 0, 0);

            GestionnaireDeFonts = new RessourcesManager<SpriteFont>(this, "Fonts");
            GestionnaireDeTextures = new RessourcesManager<Texture2D>(this, "Textures");
            GestionnaireDeModèles = new RessourcesManager<Model>(this, "Models");
            GestionnaireDeShaders = new RessourcesManager<Effect>(this, "Effects");
            GestionInput = new InputManager(this);
            CaméraJeu = new CaméraSubjective(this, positionCaméra, cibleCaméra, Vector3.Up, INTERVALLE_MAJ_STANDARD);

            Components.Add(GestionInput);
            Components.Add(CaméraJeu);
            Components.Add(new Afficheur3D(this));
            Components.Add(new Sol(this, 1f, Vector3.Zero, new Vector3(0, 0, 0), new Vector3(1, 0, 1), new Vector3(100, 0, 10000), 1.0f, Color.White));
            //Components.Add(new Sol(this, 1f, Vector3.Zero, new Vector3(0, 0, 10 * MÈTRE), new Vector3(1, 0, 1), new Vector3(KILOMÈTRE, 0, 20 * MÈTRE), 1.0f, Color.Blue));
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
            Services.AddService(typeof(InputManager), GestionInput);
            Services.AddService(typeof(Caméra), CaméraJeu);
            GestionSprites = new SpriteBatch(GraphicsDevice);
            Services.AddService(typeof(SpriteBatch), GestionSprites);
            base.Initialize();
        }

        protected override void Update(GameTime gameTime)
        {
            GérerClavier();
            CaméraJeu.Position = new Vector3(-25 + Mustang.Position.X, 50 + +Mustang.Position.Y, -120 + Mustang.Position.Z);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.SkyBlue);
            //Window.Title = CaméraJeu.Position.ToString();
            base.Draw(gameTime);
        }

        private void GérerClavier()
        {
            if (GestionInput.EstEnfoncée(Keys.Escape))
            {
                Exit();
            }
        }
    }
}
