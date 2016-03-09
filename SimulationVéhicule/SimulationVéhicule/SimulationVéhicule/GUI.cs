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
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class GUI : Microsoft.Xna.Framework.DrawableGameComponent
    {
        SpriteBatch GestionSprites { get; set; }
        SpriteFont ArialFont { get; set; }
        RessourcesManager<Texture2D> GestionnaireDeTextures { get; set; }

        Texture2D Aiguille { get; set; }
        string NomAiguille { get; set; }
        float RotationAiguille { get; set; }

        Texture2D Accéléromètre { get; set; }
        string NomAccéléromètre { get; set; }

        float IntervalleMAJ { get; set; }
        float TempsÉcouléDepuisMAJ { get; set; }

        string Vitesse { get; set; }
        Vector2 DimensionVitesse { get; set; }
        Vector2 PositionVitesse { get; set; }

        int NbVoiture { get; set; }
        int PositionUtilisateur { get; set; }
        Vector2 PositionDePosition { get; set; }
        Vector2 DimensionPosition { get; set; }

        int Temps { get; set; }
        int[] TableauTemps { get; set; }
        Vector2 PositionTemps { get; set; }
        Vector2 DimensionTemps { get; set; }
        bool AfficherTemps { get; set; }

        bool[] CheckPoint { get; set; }
        int PourcentageCourse { get; set; }
        Vector2 PositionTour { get; set; }
        Vector2 DimensionTour { get; set; }
        int NbTours { get; set; }
        int ToursFait { get; set; }
        public int NbCheckPointFranchis { get; set; }

        int IDVoitureUtilisateur { get; set; }

        public GUI(Game game, float intervalleMAJ, string aiguille, string accéléromètre, int nbVoiture, int nbTours, int idVoitureUtilisateur)
            : base(game)
        {
            IntervalleMAJ = intervalleMAJ;
            NomAiguille = aiguille;
            NomAccéléromètre = accéléromètre;
            NbVoiture = nbVoiture;
            NbTours = nbTours;
            IDVoitureUtilisateur = idVoitureUtilisateur;
        }


        protected override void LoadContent()
        {
            ArialFont = Game.Content.Load<SpriteFont>("Fonts/Arial");
            GestionSprites = Game.Services.GetService(typeof(SpriteBatch)) as SpriteBatch;
            GestionnaireDeTextures = Game.Services.GetService(typeof(RessourcesManager<Texture2D>)) as RessourcesManager<Texture2D>;
            Aiguille = GestionnaireDeTextures.Find(NomAiguille);
            Accéléromètre = GestionnaireDeTextures.Find(NomAccéléromètre);
            base.LoadContent();
        }

        public override void Initialize()
        {
            TempsÉcouléDepuisMAJ = 0;
            RotationAiguille = 0;
            PourcentageCourse = 0;
            PositionUtilisateur = 0;
            TableauTemps = new int[3];
            TableauTemps[0] = 0;
            TableauTemps[1] = 0;
            TableauTemps[2] = 0;
            NbCheckPointFranchis = 0;
            Temps = 0;
            AfficherTemps = true;
            Vitesse = "200";
            PositionVitesse = new Vector2(103, Game.Window.ClientBounds.Height - 22) + DimensionVitesse;//ish
            PositionDePosition = new Vector2(35, 40) + DimensionPosition;
            PositionTemps = new Vector2(40, 70) + DimensionTemps;
            PositionTour = new Vector2(Game.Window.ClientBounds.Width - 35, 40) + DimensionTour;
            base.Initialize();
        }


        public override void Update(GameTime gameTime)
        {
            TempsÉcouléDepuisMAJ += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (TempsÉcouléDepuisMAJ >= IntervalleMAJ)
            {
                if (ToursFait != NbTours)
                {
                    Temps++;
                }
                DimensionVitesse = ArialFont.MeasureString(Vitesse);
                DimensionPosition = ArialFont.MeasureString(PositionUtilisateur.ToString() + "/" + NbVoiture.ToString());
                DimensionTemps = ArialFont.MeasureString(TableauTemps[2].ToString("00") + ":" + TableauTemps[1].ToString("00") + "." + TableauTemps[0].ToString("00").ToString());
                DimensionTour = ArialFont.MeasureString(PourcentageCourse.ToString("000"));
                RotationAiguille = ((Math.Abs(Convert.ToInt32(Vitesse) / 100f)) * (float)MathHelper.Pi * 4 / 3) - 0.6f;//Vitesse max!
                GestionTemps();
                GestionPourcentage();
                TempsÉcouléDepuisMAJ = 0;
            }
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            GestionSprites.Draw(Accéléromètre, new Vector2(100, Game.Window.ClientBounds.Height - 130), null, Color.White, 0, new Vector2(167, 27), 0.40f, SpriteEffects.None, 0);
            GestionSprites.DrawString(ArialFont, Vitesse, PositionVitesse, new Color(17, 83, 133), 0f, new Vector2(DimensionVitesse.X / 2, DimensionVitesse.Y / 2), 0.75f, SpriteEffects.None, 0);
            GestionSprites.Draw(Aiguille, new Vector2(103, Game.Window.ClientBounds.Height - 70), null, Color.White, RotationAiguille, new Vector2(167, 27), 0.35f, SpriteEffects.None, 0);

            GestionSprites.DrawString(ArialFont, ("POSITION"), PositionDePosition - new Vector2(10f, 25), Color.White, 0f, new Vector2(DimensionPosition.X / 2, DimensionPosition.Y / 2), 0.5f, SpriteEffects.None, 0);
            GestionSprites.DrawString(ArialFont, (PositionUtilisateur.ToString() + "/" + NbVoiture.ToString()), PositionDePosition, Color.White, 0f, new Vector2(DimensionPosition.X / 2, DimensionPosition.Y / 2), 1.25f, SpriteEffects.None, 0);

            if (AfficherTemps)
            {
                GestionSprites.DrawString(ArialFont, (TableauTemps[2].ToString("00") + ":" + TableauTemps[1].ToString("00") + "." + TableauTemps[0].ToString("00").ToString()), PositionTemps, Color.White, 0f, new Vector2(DimensionTemps.X / 2, DimensionTemps.Y / 2), 0.75f, SpriteEffects.None, 0);
            }

            GestionSprites.DrawString(ArialFont, ("TOURS"), PositionTour - new Vector2(10f, 25), Color.White, 0f, new Vector2(DimensionTour.X / 2, DimensionTour.Y / 2), 0.5f, SpriteEffects.None, 0);
            GestionSprites.DrawString(ArialFont, (ToursFait + "/" + NbTours), PositionTour, Color.White, 0f, new Vector2(DimensionTour.X / 2, DimensionTour.Y / 2), 1.25f, SpriteEffects.None, 0);


            base.Draw(gameTime);
        }

        public void UpdateGUI(int vitesse, int positionUtilisateur, bool[] checkPoint, int toursFait)
        {
            Vitesse = vitesse.ToString();
            PositionUtilisateur = positionUtilisateur;
            CheckPoint = checkPoint;
            ToursFait = toursFait;
        }

        void GestionTemps()
        {
            TableauTemps[0] = Temps;
            if (TableauTemps[0] > 60)
            {
                Temps = 0;
                TableauTemps[1]++;
            }
            if (TableauTemps[1] > 60)
            {
                Temps = 0;
                TableauTemps[1] = 0;
                TableauTemps[2]++;
            }
        }

        void GestionPourcentage()
        {
            float nbCheckPoint = CheckPoint.Length * NbTours;
            int additionTours = 0;
            NbCheckPointFranchis = CheckPoint.Where(x => x == true).Count();
            if (NbCheckPointFranchis == 0)
            {
                //ToursFait++;
            }
            PourcentageCourse = (int)(((NbCheckPointFranchis / nbCheckPoint) * 100) + additionTours);
            //Game.Window.Title = NbCheckPointFranchis.ToString() + " - " + ToursFait.ToString();
        }

        public void UpdateNbCheckPointFranchis()
        {
            NbCheckPointFranchis++;
        }
    }
}
