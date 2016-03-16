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
                for (int i = 0; i < LaPiste.Count(); i++)
                {
                    for (int j = 0; j < LaPiste[i].BoxÉtape.Count(); j++)
                    {
                        if (ListeVoiture[v].BoxVoiture.Intersects(LaPiste[i].BoxÉtape[j]))
                        {
                            LaPiste[i].ListeFranchiParVoiture[v][j] = true;
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


            Game.Window.Title = ÉgaleUnePosition(0).ToString();
            //Game.Window.Title = NbFranchis[0].ToString() + " + " + NbFranchis[1].ToString();



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
    }
}
