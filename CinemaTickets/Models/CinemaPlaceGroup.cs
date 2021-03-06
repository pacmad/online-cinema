﻿using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace CinemaTickets.Models
{
    public class CinemaPlaceGroup
    {
        // cinema place group ID
        public int ID { get; set; }
        // cinema id
        public int CinemaID { get; set; }
        // cinema place group name
        public string Name { get; set; }
    }

    public class CinemaHallPlaceGroup
    {
        // cinema hall place group ID
        public int ID { get; set; }
        // cinema hall place id
        public int CinemaHallPlaceID { get; set; }
        // cinema place group id
        public int CinemaPlaceGroupID { get; set; }
    }

    public class CinemaPlaceGroupContext : DbContext
    {
        public CinemaPlaceGroupContext() : base("name=DefaultConnection")
        {
        }

        public DbSet<CinemaPlaceGroup> CinemaPlaceGroups { get; set; }
        public DbSet<CinemaHallPlaceGroup> CinemaHallPlaceGroups { get; set; }

        public CinemaHallPlaceGroup Get(int cinemaHallPlaceID)
        {
            var cinemaHallPlaceGroups = from c in CinemaHallPlaceGroups
                                        select c;

            cinemaHallPlaceGroups = cinemaHallPlaceGroups.Where(s => s.CinemaHallPlaceID == cinemaHallPlaceID);

            return cinemaHallPlaceGroups.FirstOrDefault();
        }

        public string GetName(int cinemaHallPlaceID)
        {
            var cinemaHallPlaceGroups = from c in CinemaHallPlaceGroups
                                        join cpg in CinemaPlaceGroups on c.CinemaPlaceGroupID equals cpg.ID
                                        select new
                                        {
                                            CinemaHallPlaceID = c.CinemaHallPlaceID,
                                            Name = cpg.Name,
                                        };

            cinemaHallPlaceGroups = cinemaHallPlaceGroups.Where(s => s.CinemaHallPlaceID == cinemaHallPlaceID);

            return cinemaHallPlaceGroups.FirstOrDefault() != null ? cinemaHallPlaceGroups.FirstOrDefault().Name : "";
        }

        public List<CinemaPlaceGroup> GetList(int ID = 0)
        {
            var cinemaPlaceGroups = from d in CinemaPlaceGroups
                                    select d;

            cinemaPlaceGroups = cinemaPlaceGroups.Where(s => s.CinemaID == ID);

            return cinemaPlaceGroups.ToList();
        }

        public IEnumerable<SelectListItem> GetSelectList(int ID = 0)
        {
            int cinemaId = Core.GetCinemaId();
            return from d in CinemaPlaceGroups
                             where d.CinemaID == cinemaId
                   select new SelectListItem
                   {
                       Text = d.Name,
                       Value = d.ID.ToString(),
                       Selected = d.ID == ID
                   };
        }

        public int GetCinemaPlaceGroupId(int cinemaHallPlaceID)
        {
            var cinemaHallPlaceGroupQuery = from c in CinemaHallPlaceGroups
                          where c.CinemaHallPlaceID == cinemaHallPlaceID
                          select c;

            CinemaHallPlaceGroup cinemaHallPlaceGroup = cinemaHallPlaceGroupQuery.FirstOrDefault();

            return cinemaHallPlaceGroup != null ? cinemaHallPlaceGroup.CinemaPlaceGroupID : 0;
        }
    }
}