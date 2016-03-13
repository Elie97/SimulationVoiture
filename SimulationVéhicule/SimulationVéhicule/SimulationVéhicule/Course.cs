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
        public List<int> PositionVoiture { get; set; }
        public List<int[]> FranchiParVoiture { get; set; }
        public List<int[]> NbFranchis { get; set; }

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
            ToursFait = new int[NbVoiture];
            for (int i = 0; i < NbVoiture; i++)
            {
                ToursFait[i] = 0;
            }

            CheckPointParVoiture = new List<bool[]>(NbVoiture);
            for (int i = 0; i < NbVoiture; i++)
            {
                CheckPointParVoiture.Add(new bool[LaPiste.Count()]);
            }
            FranchiParVoiture = new List<int[]>(NbVoiture);
            for (int i = 0; i < NbVoiture; i++)
            {
                FranchiParVoiture.Add(new int[2]);
            }
            base.Initialize();
        }

        public override void Update(GameTime gameTime)
        {
            for (int v = 0; v < NbVoiture; v++)
            {
                if (ToursFait[v] < NbTours)
                {
                    if (LaPiste.TrueForAll(x => x.Franchi[v]))
                    {
                        for (int j = 0; j < LaPiste.Count(); j++)
                        {
                            LaPiste[j].Franchi[v] = false;
                        }
                        //Interface.NbCheckPointFranchis = 0;
                        ToursFait[v]++;
                    }
                    for (int j = 0; j < LaPiste.Count(); j++)
                    {
                        CheckPointParVoiture[v][j] = LaPiste[j].Franchi[v];
                    }
                    LaPiste[LaPiste.Count() - 1].Franchi[v] = false;
                }

                for (int i = 0; i < LaPiste.Count(); i++)
                {
                    for (int j = 0; j < LaPiste[i].BoxÉtape.Count(); j++)
                    {
                        if (ListeVoiture[v].BoxVoiture.Intersects(LaPiste[i].BoxÉtape[j]))
                        {
                            LaPiste[i].Franchi[v] = true;
                        }
                    }
                }
            }

            for (int v = 0; v < NbVoiture; v++)
            {
                FranchiParVoiture[v][0] = CheckPointParVoiture[v].Where(x => x).Count() + (ToursFait[v] * LaPiste.Count());//En ordre plz
                FranchiParVoiture[v][1] = v;
            }
            Game.Window.Title = LaPiste[1].BoxÉtape[1].Min.ToString();
            NbFranchis = FranchiParVoiture.OrderBy(x => x[0]).ToList();




            base.Update(gameTime);
        }
    }
}
