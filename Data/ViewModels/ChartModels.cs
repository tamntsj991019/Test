using System;
using System.Collections.Generic;
using System.Text;

namespace Data.ViewModels
{
    #region Chart Booking
    public class ChartBookingViewModel
    {
        public BookingElement BookingWaiting { get; set; }
        public BookingElement BookingDone { get; set; }
        public BookingElement BookingCancelled { get; set; }
        public BookingElement BookingOther { get; set; }
    }

    public class BookingElement
    {
        public string Name { get; set; }
        public int Quantity { get; set; }
    }
    #endregion

    public class ChartTransactionViewModel
    {
        public DateTime Date { get; set; }
        public double Total { get; set; }

    }
}
