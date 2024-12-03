using Microsoft.EntityFrameworkCore;
using Movie_System.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
namespace Movie_System
{
    internal class Program
    {
        static void Main()
        {
            using (var context = new MovieContext())
            {
                context.Database.EnsureCreated();

                while (true)
                {
                    Console.WriteLine("\n--- Movie Reservation System ---");
                    Console.WriteLine("1. Register");
                    Console.WriteLine("2. Login");
                    Console.WriteLine("3. Exit");
                    Console.Write("Choose an option: ");

                    string choice = Console.ReadLine();

                    switch (choice)
                    {
                        case "1":
                            RegisterUser(context);
                            break;
                        case "2":
                            User loggedInUser = LoginUser(context);
                            if (loggedInUser != null)
                            {
                                if (loggedInUser.IsAdmin)
                                {
                                    AdminMenu(context);
                                }
                                else
                                {
                                    UserMenu(context, loggedInUser);
                                }
                            }
                            break;
                        case "3":
                            return;
                    }
                }
            }
        }

        static void RegisterUser(MovieContext context)
        {
            Console.Write("Enter username: ");
            string username = Console.ReadLine();

            if (context.Users.Any(u => u.Username == username))
            {
                Console.WriteLine("Username already exists.");
                return;
            }

            Console.Write("Enter password: ");
            string password = Console.ReadLine();

            Console.Write("Are you an admin? (yes/no): ");
            bool isAdmin = Console.ReadLine().ToLower() == "yes";

            var newUser = new User
            {
                Username = username,
                Password = password,
                IsAdmin = isAdmin
            };

            context.Users.Add(newUser);
            context.SaveChanges();

            Console.WriteLine("Registration successful!");
        }

        static User LoginUser(MovieContext context)
        {
            Console.Write("Enter username: ");
            string username = Console.ReadLine();
            Console.Write("Enter password: ");
            string password = Console.ReadLine();

            var user = context.Users.FirstOrDefault(u =>
                u.Username == username && u.Password == password);

            if (user == null)
            {
                Console.WriteLine("Invalid credentials.");
                return null;
            }

            Console.WriteLine("Login successful!");
            return user;
        }

        static void AdminMenu(MovieContext context)
        {
            while (true)
            {
                Console.WriteLine("\n--- Admin Menu ---");
                Console.WriteLine("1. Add Movie");
                Console.WriteLine("2. View All Reservations");
                Console.WriteLine("3. View All Movies");
                Console.WriteLine("4. Logout");
                Console.Write("Choose an option: ");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        AddMovie(context);
                        break;
                    case "2":
                        ViewAllReservations(context);
                        break;
                    case "3":
                        ViewMovies(context);
                        break;
                    case "4":
                        return;
                }
            }
        }

        static void AddMovie(MovieContext context)
        {
            Console.Write("Enter movie title: ");
            string title = Console.ReadLine();

            Console.Write("Enter movie description: ");
            string description = Console.ReadLine();

            Console.Write("Enter show time (yyyy-MM-dd HH:mm): ");
            DateTime showTime = DateTime.Parse(Console.ReadLine());

            Console.Write("Enter total seats: ");
            int totalSeats = int.Parse(Console.ReadLine());

            var movie = new Movie
            {
                Title = title,
                Description = description,
                ShowTime = showTime,
                TotalSeats = totalSeats,
                AvailableSeats = totalSeats
            };

            context.Movies.Add(movie);
            context.SaveChanges();

            Console.WriteLine("Movie added successfully!");
        }

        static void UserMenu(MovieContext context, User user)
        {
            while (true)
            {
                Console.WriteLine("\n--- User Menu ---");
                Console.WriteLine("1. View Movies");
                Console.WriteLine("2. Reserve Seats");
                Console.WriteLine("3. View My Reservations");
                Console.WriteLine("4. Cancel Reservation");
                Console.WriteLine("5. Logout");
                Console.Write("Choose an option: ");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        ViewMovies(context);
                        break;
                    case "2":
                        ReserveSeat(context, user);
                        break;
                    case "3":
                        ViewUserReservations(context, user);
                        break;
                    case "4":
                        CancelReservation(context, user);
                        break;
                    case "5":
                        return;
                }
            }
        }

        static void ReserveSeat(MovieContext context, User user)
        {
            ViewMovies(context);

            Console.Write("Enter Movie ID to reserve: ");
            int movieId = int.Parse(Console.ReadLine());

            var movie = context.Movies.FirstOrDefault(m => m.Id == movieId);

            if (movie == null)
            {
                Console.WriteLine("Movie not found.");
                return;
            }

            Console.Write("Enter number of seats to reserve: ");
            int numberOfSeats = int.Parse(Console.ReadLine());

            if (numberOfSeats > movie.AvailableSeats)
            {
                Console.WriteLine("Not enough seats available.");
                return;
            }

            var reservation = new Reservation
            {
                UserId = user.Id,
                MovieId = movie.Id,
                NumberOfSeats = numberOfSeats
            };

            context.Reservations.Add(reservation);
            movie.AvailableSeats -= numberOfSeats;
            context.SaveChanges();

            Console.WriteLine("Seats reserved successfully!");
        }

        static void ViewMovies(MovieContext context)
        {
            var movies = context.Movies.ToList();

            foreach (var movie in movies)
            {
                Console.WriteLine($"ID: {movie.Id}, " +
                                  $"Title: {movie.Title}, " +
                                  $"Showtime: {movie.ShowTime}, " +
                                  $"Available Seats: {movie.AvailableSeats}");
            }
        }

        static void ViewUserReservations(MovieContext context, User user)
        {
            var reservations = context.Reservations
                .Where(r => r.UserId == user.Id)
                .ToList();

            foreach (var reservation in reservations)
            {
                var movie = context.Movies.First(m => m.Id == reservation.MovieId);
                Console.WriteLine($"Reservation ID: {reservation.Id}, " +
                                  $"Movie: {movie.Title}, " +
                                  $"Seats: {reservation.NumberOfSeats}");
            }
        }

        static void ViewAllReservations(MovieContext context)
        {
            var reservations = context.Reservations
                .Select(r => new
                {
                    ReservationId = r.Id,
                    Username = r.User.Username,
                    MovieTitle = r.Movie.Title,
                    Seats = r.NumberOfSeats
                })
                .ToList();

            foreach (var reservation in reservations)
            {
                Console.WriteLine($"Reservation ID: {reservation.ReservationId}, " +
                                  $"User: {reservation.Username}, " +
                                  $"Movie: {reservation.MovieTitle}, " +
                                  $"Seats: {reservation.Seats}");
            }
        }

        static void CancelReservation(MovieContext context, User user)
        {
            ViewUserReservations(context, user);

            Console.Write("Enter Reservation ID to cancel: ");
            int reservationId = int.Parse(Console.ReadLine());

            var reservation = context.Reservations
                .FirstOrDefault(r => r.Id == reservationId && r.UserId == user.Id);

            if (reservation == null)
            {
                Console.WriteLine("Reservation not found.");
                return;
            }

            var movie = context.Movies.First(m => m.Id == reservation.MovieId);
            movie.AvailableSeats += reservation.NumberOfSeats;

            context.Reservations.Remove(reservation);
            context.SaveChanges();

            Console.WriteLine("Reservation cancelled successfully!");
        }
    }
}