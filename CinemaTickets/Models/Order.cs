﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;

namespace CinemaTickets.Models
{
    public class Order
    {
        // order ID
        public int ID { get; set; }
        // date
        public DateTime Date { get; set; }
        // is paid
        public bool IsPaid { get; set; }
        // name
        public string Name { get; set; }
        // email
        public string Email { get; set; }
        // phone
        public string Phone { get; set; }
        // is processing
        public bool IsProcessing { get; set; }
    }

    public class OrderItem
    {
        // order item ID
        public int ID { get; set; }
        // order ID
        public int OrderID { get; set; }
        // cinema hall movie ID
        public int CinemaHallMovieID { get; set; }
        // cinema hall place ID
        public int CinemaHallPlaceID { get; set; }
        // price
        public int Price { get; set; }
    }

    public class OrderItemInfo
    {
        // order item id
        public int OrderItemId { get; set; }
        // cinema hall place row
        public int Row { get; set; }
        // cinema hall place cell
        public int Cell { get; set; }
        // movie id
        public int MovieID { get; set; }
        // movie name
        public string MovieName { get; set; }
        // movie image
        public string MovieImage { get; set; }
        // movie age limit
        public int? MovieAgeLimit { get; set; }
        // genre name
        public string Genre { get; set; }
        // cinema id
        public int CinemaID { get; set; }
        // cinema name
        public string CinemaName { get; set; }
        // cinema hall name
        public string CinemaHallName { get; set; }
        // cinema hall movie id
        public int CinemaHallMovieID { get; set; }
        // cinema hall place id
        public int CinemaHallPlaceID { get; set; }
        // price
        public int Price { get; set; }
        // duration
        public int Duration { get; set; }
        // date
        public DateTime Date { get; set; }
        // formatted date
        public string FormattedDate { get; set; }
    }

    public class OrderContext : DbContext
    {
        public OrderContext() : base("name=DefaultConnection")
        {
        }

        private CinemaHallMoviePlaceContext cinemaHallMoviePlaceDb = new CinemaHallMoviePlaceContext();
        private CinemaHallPlaceContext cinemaHallPlaceDb = new CinemaHallPlaceContext();

        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<CinemaHallMovie> CinemaHallMovies { get; set; }
        public DbSet<CinemaHallPlace> CinemaHallPlaces { get; set; }
        public DbSet<CinemaHall> CinemaHalls { get; set; }
        public DbSet<Cinema> Cinemas { get; set; }
        public DbSet<Movie> Movies { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<CinemaHallMoviePlace> CinemaHallMoviePlaces { get; set; }

        public List<OrderItemInfo> GetOrderItems(int id)
        {
            var orderItems = from o in Orders
                             join oi in OrderItems on o.ID equals oi.OrderID
                             join chm in CinemaHallMovies on oi.CinemaHallMovieID equals chm.ID
                             join ch in CinemaHalls on chm.CinemaHallID equals ch.ID
                             join c in Cinemas on ch.CinemaID equals c.ID
                             join m in Movies on chm.MovieID equals m.ID
                             join g in Genres on m.GenreID equals g.ID
                             join chp in CinemaHallPlaces on oi.CinemaHallPlaceID equals chp.ID
                             where o.ID == id
                             select new OrderItemInfo
                             {
                                 OrderItemId = oi.ID,
                                 Row = chp.Row,
                                 Cell = chp.Cell,
                                 MovieID = m.ID,
                                 MovieName = m.Name,
                                 MovieImage = m.Image,
                                 MovieAgeLimit = m.AgeLimit,
                                 Genre = g.Name,
                                 CinemaID = c.ID,
                                 CinemaName = c.Name,
                                 CinemaHallName = ch.Name,
                                 CinemaHallMovieID = chm.ID,
                                 CinemaHallPlaceID = chp.ID,
                                 Price = oi.Price,
                                 Duration = m.Duration,
                                 Date = chm.Date,
                             };

            List<OrderItemInfo> items = orderItems.ToList();
            for (var i = 0; i < items.Count; i++)
            {
                items.ElementAt(i).FormattedDate = Core.GetFormatedDate(items.ElementAt(i).Date);
            }

            return items;
        }

        public void SetSuccessfullOrder(int id)
        {
            Order order = Orders.Find(id);
            order.IsPaid = true;
            SaveChanges();

            List<OrderItemInfo> orderItems = GetOrderItems(id);
            foreach (OrderItemInfo orderItem in orderItems)
            {
                CinemaHallMoviePlace cinemaHallMoviePlace = cinemaHallMoviePlaceDb.GetCinemaHallMoviePlace(
                    orderItem.CinemaHallMovieID, orderItem.CinemaHallPlaceID);

                cinemaHallMoviePlace.Status = CinemaHallMoviePlace.STATUS_SUCCESSFULL;
                cinemaHallMoviePlaceDb.SaveChanges();
            }
        }

        public void SetFailedOrder(int id)
        {
            List<OrderItemInfo> orderItems = GetOrderItems(id);
            foreach (OrderItemInfo orderItem in orderItems)
            {
                CinemaHallMoviePlace cinemaHallMoviePlace = cinemaHallMoviePlaceDb.GetCinemaHallMoviePlace(
                    orderItem.CinemaHallMovieID, orderItem.CinemaHallPlaceID);

                cinemaHallMoviePlaceDb.CinemaHallMoviePlaces.Remove(cinemaHallMoviePlace);
            }
            cinemaHallMoviePlaceDb.SaveChanges();
        }

        public string GetOrderItemDetails(int id)
        {
            string details = "";

            Order order = Orders.Find(id);

            List<OrderItemInfo> orderItems = GetOrderItems(id);
            foreach (OrderItemInfo orderItem in orderItems)
            {
                CinemaHallMoviePlace cinemaHallMoviePlace = cinemaHallMoviePlaceDb.GetCinemaHallMoviePlace(
                    orderItem.CinemaHallMovieID, orderItem.CinemaHallPlaceID);

                CinemaHallPlace cinemaHallPlace = cinemaHallPlaceDb.CinemaHallPlaces.Find(cinemaHallMoviePlace.CinemaHallPlaceID);

                details += "Кінотеатр: " + orderItem.CinemaName + ", Зал: " + orderItem.CinemaHallName + "\n";
                details += "Фільм: " + orderItem.MovieName + ", Тривалість: " + orderItem.Duration + " хв." + "\n";
                details += "Ряд: " + cinemaHallPlace.Row + ", Місце: " + cinemaHallPlace.Cell + "\n";
                details += "Дата: " + orderItem.FormattedDate + ", Ціна: " + orderItem.Price + " грн." + "\n";
                details += "Оплачено: " + (order.IsPaid ? "Так" : "Ні") + "\n\n";
            }

            return details;
        }

        public string GetOrderItemDetailsGrouped(List<OrderItemInfo> orderItems)
        {
            string details = "";
            int placesCount = 0;
            Dictionary<string, string> groupedDetails = new Dictionary<string, string>();

            foreach (OrderItemInfo orderItem in orderItems)
            {
                groupedDetails["cinema"] = orderItem.CinemaName;
                groupedDetails["hall"] = orderItem.CinemaHallName;
                groupedDetails["movie"] = orderItem.MovieName;
                groupedDetails["date"] = orderItem.FormattedDate;
                placesCount++;
            }

            details += "Кінотеатр: " + groupedDetails["cinema"] + ", Зал: " + groupedDetails["hall"] + "\n";
            details += "Фільм: " + groupedDetails["movie"] + ", Дата: " + groupedDetails["date"] + "\n";
            details += "К-ть місць: " + placesCount.ToString() + ".";

            return details;
        }

        public List<OrderItemInfo> GetUnpaidPlaces()
        {
            DateTime date = DateTime.Now;
            date = date.AddMinutes(-Config.CONFIRM_PAYMENT_MINUTES_TIMEOUT);

            var orderItems = from o in Orders
                             join oi in OrderItems on o.ID equals oi.OrderID
                             join chmp in CinemaHallMoviePlaces on oi.CinemaHallMovieID equals chmp.CinemaHallMovieID
                             where o.Date <= date && chmp.CinemaHallPlaceID == oi.CinemaHallPlaceID && chmp.Status == CinemaHallMoviePlace.STATUS_PROCESSING
                             select new OrderItemInfo
                             {
                                 CinemaHallMovieID = chmp.CinemaHallMovieID,
                                 CinemaHallPlaceID = chmp.CinemaHallPlaceID,
                             };

            return orderItems.ToList();
        }

        public List<OrderItem> GetOrderItemsByIds(int[] orderItemIds)
        {
            var orderItems = from oi in OrderItems
                             select oi;

            orderItems = orderItems.Where(s => orderItemIds.Contains(s.ID));

            return orderItems.ToList();
        }
    }
}