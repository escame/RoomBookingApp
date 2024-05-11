using Microsoft.EntityFrameworkCore;
using RoomBookingApp.Core.Domain;
using RoomBookingApp.Persistence.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomBookingApp.Persistence.Tests
{
    public class RoomBookingServiceTest
    {
        [Fact]
        public void Should_Return_Available_Rooms()
        {
            // Arrange
            var date = new DateTime(2021, 1, 1);

            var dbOptions = new DbContextOptionsBuilder<RoomBookingAppDbContext>()
                .UseInMemoryDatabase("AvailableRoomTest")
                .Options;

            using var context = new RoomBookingAppDbContext(dbOptions);

            context.Add(new Room { Id = 1, Name = "Room 1" });
            context.Add(new Room { Id = 2, Name = "Room 2" });
            context.Add(new Room { Id = 3, Name = "Room 3" });

            context.Add(new RoomBooking { RoomId = 1, Date = date });
            context.Add(new RoomBooking { RoomId = 2, Date = date.AddDays(-1) });

            context.SaveChanges();

            var roomBookingService = new RoomBookingService(context);

            // Act
            var availableRooms = roomBookingService.GetAvailableRooms(date);

            // Assert
            Assert.Equal(2, availableRooms.Count());
            Assert.Contains(availableRooms, r => r.Name == "Room 2");
            Assert.Contains(availableRooms, r => r.Name == "Room 3");
            Assert.DoesNotContain(availableRooms, r => r.Name == "Room 1");
        }

        [Fact]
        public void Should_Save_Room_Booking()
        {
            // Arrange
            var dbOptions = new DbContextOptionsBuilder<RoomBookingAppDbContext>()
                .UseInMemoryDatabase("SaveRoomBookingTest")
                .Options;

            var roomBooking = new RoomBooking { RoomId = 1, Date = new DateTime(2021, 1, 1) };

            using var context = new RoomBookingAppDbContext(dbOptions);
            var roomBookingService = new RoomBookingService(context);
            roomBookingService.Save(roomBooking);

            // Act
            var bookings = context.RoomBookings.ToList();
            var booking = Assert.Single(bookings);

            Assert.Equal(roomBooking.Date, booking.Date);
            Assert.Equal(roomBooking.RoomId, booking.RoomId);
        }
    }
}
