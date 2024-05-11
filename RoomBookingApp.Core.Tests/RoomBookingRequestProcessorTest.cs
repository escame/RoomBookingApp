using Moq;
using RoomBookingApp.Core.DataServices;
using RoomBookingApp.Core.Domain;
using RoomBookingApp.Core.Enums;
using RoomBookingApp.Core.Models;
using Shouldly;

namespace RoomBookingApp.Core
{
    public class RoomBookingRequestProcessorTest
    {
        private readonly RoomBookingRequestProcessor _processor;
        private RoomBookingRequest _request;
        private Mock<IRoomBookingService> _roomBookingServiceMock;
        private List<Room> _availableRooms;

        public RoomBookingRequestProcessorTest()
        {
            _request = new RoomBookingRequest
            {
                FullName = "Test Name",
                Email = "test@request.com",
                Date = new DateTime(2021, 1, 1),
            };
            _availableRooms = new List<Room>() { new Room() { Id = 1 } };

            _roomBookingServiceMock = new Mock<IRoomBookingService>();
            _roomBookingServiceMock.Setup(x => x.GetAvailableRooms(It.IsAny<DateTime>())).Returns(_availableRooms);
            _processor = new RoomBookingRequestProcessor(_roomBookingServiceMock.Object);

        }

        [Fact]
        public void Should_Return_Room_Booking_Response_With_Request_Values()
        {            
            // Act
            RoomBookingResult result = _processor.BookRoom(_request);

            // Assert
            result.ShouldNotBeNull();
            result.FullName.ShouldBe(_request.FullName);
            result.Email.ShouldBe(_request.Email);
            result.Date.ShouldBe(_request.Date);
        }

        [Fact]
        public void Should_Throw_Exception_For_Null_Request()
        {
            // Act & Assert
            var exception = Should.Throw<ArgumentNullException>(() => _processor.BookRoom(null));
            exception.Message.ShouldBe("Value cannot be null. (Parameter 'bookingRequest')");
        }

        [Fact]
        public void Should_Save_Room_Booking_Request()
        {
            RoomBooking saveBooking = null;
            _roomBookingServiceMock.Setup(x => x.Save(It.IsAny<RoomBooking>()))
                .Callback<RoomBooking>(x => saveBooking = x);
            _processor.BookRoom(_request);

            _roomBookingServiceMock.Verify(x => x.Save(It.IsAny<RoomBooking>()), Times.Once);
  
            saveBooking.ShouldNotBeNull();
            saveBooking.FullName.ShouldBe(_request.FullName);
            saveBooking.Email.ShouldBe(_request.Email);
            saveBooking.Date.ShouldBe(_request.Date);
            saveBooking.RoomId.ShouldBe(_availableRooms.First().Id);
        }

        [Fact]
        public void Should_Not_Save_Room_Booking_Request_If_None_Available()
        {
            _availableRooms.Clear();
            _processor.BookRoom(_request);

            _roomBookingServiceMock.Verify(x => x.Save(It.IsAny<RoomBooking>()), Times.Never);
        }

        [Theory]
        [InlineData(BookingResultFlag.Failure, false)]
        [InlineData(BookingResultFlag.Success, true)]
        public void Should_Return_SuccessOrFailure_Flag_In_Result(BookingResultFlag bookingSuccessFlags, bool isAvailable)
        {
            if(!isAvailable)
            {
                _availableRooms.Clear();
            }

            RoomBookingResult result = _processor.BookRoom(_request);

            bookingSuccessFlags.ShouldBe(result.Flag);
        }

        [Theory]
        [InlineData(1, true)]
        [InlineData(null, false)]
        public void Should_Return_RoomId_In_Result(int? roomBookingId, bool isAvailable)
        {
            if(!isAvailable)
            {
                _availableRooms.Clear();
            }
            else
            {
                _roomBookingServiceMock.Setup(x => x.Save(It.IsAny<RoomBooking>()))
                    .Callback<RoomBooking>(x => x.Id = roomBookingId.Value);
            }

            var result = _processor.BookRoom(_request);
            result.RoomBookingId.ShouldBe(roomBookingId);
        }
    }
}
