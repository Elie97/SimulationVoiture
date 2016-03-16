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
    public class Course : Microsoft.Xna.Framework.GameComponent
    {
        public int NbTours { get; set; }
        public int NbVoiture { get; set; }
        public int[] ToursFait { get; set; }
        public List<Sol> LaPiste { get; set; }
        public List<Voiture> ListeVoiture { get; set; }
        public List<bool[]> CheckPointParVoiture { get; set; }
        public List<int[]> FranchiParVoiture { get; set; }
        public int[] NbFranchis { get; set; }
        List<bool>[] ListeCheckPoint { get; set; }
        List<int[]> PositionVoiture { get; set; }
        public int PositionUtilisateur { get; set; }
        List<bool[]> CheckPointFranchiParVoiture { get; set; }

        public List<int> Position { get; set; }

        public Course(Game game, int nbTours, int nbVoiture, List<Sol> laPiste, List<Voiture> listeVoiture)
            : base(game)
        {
            NbTours = nbTours;
            NbVoiture = nbVoiture;
            LaPiste = laPiste;
            ListeVoiture = listeVoiture;
        }

        public override void Initialize()
        {
            PositionUtilisateur = NbVoiture;
            NbFranchis = new int[NbVoiture];
            ListeCheckPoint = new List<bool>[2];
            ToursFait = new int[NbVoiture];
            CheckPointParVoiture = new List<bool[]>(NbVoiture);
            CheckPointFranchiParVoiture = new List<bool[]>(NbVoiture);
            FranchiParVoiture = new List<int[]>(NbVoiture);
            PositionVoiture = new List<int[]>();

            for (int i = 0; i < NbVoiture; i++)
            {
                NbFranchis[i] = 0;
                ToursFait[i] = 0;
                CheckPointParVoiture.Add(new bool[LaPiste.Count()]);
                FranchiParVoiture.Add(new int[2]);
            }


            base.Initialize();
        }

        public override void Update(GameTime gameTime)
        {
            int nbCheckPointParTour = LaPiste.Count() * Sol.NB_CHECK_POINT;
            PositionVoiture = new List<int[]>();

            for (int v = 0; v < NbVoiture; v++)
            {
                //ini liste de tous les checks points
                ListeCheckPoint[v] = new List<bool>();
                for (int i = 0; i < LaPiste.Count(); i++)
                {
                    foreach (bool x in LaPiste[i].ListeFranchiParVoiture[v])
                    {
                        ListeCheckPoint[v].Add(x);
                    }
                }

                if (ToursFait[v] < NbTours)
                {
                    //retour checkpont à false si tour complet
                    if (ListeCheckPoint[v].TrueForAll(x => x))
                    {
                        for (int i = 0; i < LaPiste.Count(); i++)
                        {
                            for (int j = 0; j < Sol.NB_CHECK_POINT; j++)
                            {
                                LaPiste[i].ListeFranchiParVoiture[v][j] = false;
                            }
                        }
                        ToursFait[v]++;
                    }
                }
                if (!ListeVoiture[v].BoxVoiture.Intersects(LaPiste[LaPiste.Count() - 1].BoxÉtape[Sol.NB_CHECK_POINT - 1]))
                {
                    LaPiste[LaPiste.Count() - 1].ListeFranchiParVoiture[v][1] = false;
                }

                //détection collision check point
                float t = 0;
                for (int i = 0; i < LaPiste.Count(); i++)
                {
                    for (int j = 0; j < LaPiste[i].BoxÉtape.Count(); j++)
                    {
                        if (ListeVoiture[v].BoxVoiture.Intersects(LaPiste[i].BoxComplet))
                        {
                            //Game.Window.Title = GetSensDeltaRotationSol(ListeVoiture[0].Rotation.Y, LaPiste[i].RotationInitiale.Y).ToString();
                            //LaPiste[i].ListeFranchiParVoiture[v][j] = false;
                            if (ListeVoiture[v].BoxVoiture.Intersects(LaPiste[i].BoxÉtape[j]))
                            {
                                //LaPiste[i].ListeFranchiParVoiture[v][j] = true;
                                if (GetSensDeltaRotationSol(ListeVoiture[v].Rotation.Y, LaPiste[i].RotationInitiale.Y) >= 0)
                                {
                                    LaPiste[i].ListeFranchiParVoiture[v][j] = true;
                                    //Game.Window.Title = GetSensDeltaRotationSol(ListeVoiture[0].Rotation.Y, LaPiste[i].RotationInitiale.Y).ToString();
                                }
                                if (GetSensDeltaRotationSol(ListeVoiture[v].Rotation.Y, LaPiste[i].RotationInitiale.Y) <= 0)
                                {
                                    LaPiste[i].ListeFranchiParVoiture[v][j] = false;
                                }
                            }
                        }
                    }
                }

                for (int i = 0; i < LaPiste.Count(); i++)
                {
                    for (int j = 0; j < LaPiste[i].BoxÉtape.Count(); j++)
                    {
                        ListeCheckPoint[v][i + i + j] = LaPiste[i].ListeFranchiParVoiture[v][j];
                    }
                }

                NbFranchis[v] = ListeCheckPoint[v].Where(x => x).Count() + (ToursFait[v] * LaPiste.Count() * Sol.NB_CHECK_POINT);

                PositionVoiture.Add(new int[] { v, NbFranchis[v] });

            }

            PositionVoiture = PositionVoiture.OrderByDescending(x => x[1]).ToList();

            for (int i = 0; i < PositionVoiture.Count(); i++)
            {
                if (!ÉgaleUnePosition(0))
                {
                    if (0 == PositionVoiture[i][0])//id voiture utilisateur
                    {
                        PositionUtilisateur = i + 1;
                    }
                }
            }

            string d = "";
            foreach (bool x in ListeCheckPoint[0])
            {
                d += " - " + x;
            }

            for (int i = 0; i < LaPiste.Count(); i++)
            {
                for (int j = 0; j < LaPiste[i].BoxÉtape.Count(); j++)
                {
                    if(ListeCheckPoint[0][i + i + j] == true)
                    {
                        LaPiste[i].CouleurCheckPoint = Color.Green;
                    }
                }
            }

            //Game.Window.Title = GetSensDeltaRotationSol(ListeVoiture[0].Rotation.Y, LaPiste[ListeCheckPoint[0].Where(x => x).Count() / 2].RotationInitiale.Y).ToString() + " - " + (ListeCheckPoint[0].Where(x => x).Count() / 2).ToString() + "/" + LaPiste.Count().ToString();
            //Game.Window.Title = (ListeVoiture[0].Rotation.Y % MathHelper.Pi).ToString();
            Game.Window.Title = NbFranchis[0].ToString() + " + " + NbFranchis[1].ToString();



            base.Update(gameTime);
        }

        bool ÉgaleUnePosition(int IDUtilisateur)
        {
            bool égaleUnePosition = false;
            for (int i = 1; i < NbVoiture; i++)
            {
                égaleUnePosition = false;
                if (NbFranchis[IDUtilisateur] == NbFranchis[i])
                {
                    égaleUnePosition = true;
                } 
            }
            return égaleUnePosition;
        }

        float NormalizeRotation(float rotation)
        {
            //if (rotation > 0)
            //{
            //    rotation = rotation % (float)(Math.PI);
            //}
            //else if (rotation < 0)
            //{
            //    rotation = rotation % (float)(Math.PI);
            //}
            //if (rotation > (Math.PI))
            //{
            //    rotation = rotation - ((float)Math.PI - rotation);
            //}
            return (float)Math.Cos(rotation);
        }

        float GetSensDeltaRotationSol(float rotationVoiture, float rotationSol)
        {
            float deltaRotation = 0;
            float facteurVitesse = 1;
            if (rotationSol == MathHelper.Pi)
            {
                deltaRotation = (float)Math.Cos(rotationVoiture);
            }
            if (rotationSol == (MathHelper.PiOver2))
            {
                deltaRotation = (float)-Math.Sin(rotationVoiture);
            }
            if (rotationSol == 0)
            {
                deltaRotation = (float)-Math.Cos(rotationVoiture);
            }
            if (rotationSol == -MathHelper.Pi)
            {
                deltaRotation = (float)Math.Sin(rotationVoiture);
            }
            if (ListeVoiture[0].Vitesse != 0)
            {
                facteurVitesse = (ListeVoiture[0].Vitesse / Math.Abs(ListeVoiture[0].Vitesse));
            }
            return deltaRotation * facteurVitesse;
        }
    }
}
