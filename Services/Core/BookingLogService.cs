using AutoMapper;
using Data.DbContext;
using Data.Entities;
using Data.ViewModels;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Data.ConstData;
using Hubs;
using Hangfire;
using MoreLinq;

namespace Services.Core
{
    public interface IBookingLogService
    {
        ResultModel Get(Guid? id);
        ResultModel GetImagesByBookingId(Guid bookingId, string role);
        ResultModel GetByBookingId(Guid bookingId);

        ResultModel Create(BookingLogCreateModel model);
        ResultModel Update(Guid id, BookingLogUpdateModel model);
        ResultModel Delete(Guid id);

        Task<ResultModel> AddImages(List<IFormFile> files, Guid bookingId, string statusId);
        FileModel GetImageById(Guid fileId, string fileLocation);

    }
    public class BookingLogService : IBookingLogService
    {
        private readonly IMapper _mapper;
        private readonly AppDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly IBookingHub _bookingHub;
        private readonly INotificationService _notificationService;
        private readonly ITransactionService _transactionService;

        private readonly int MaxFileSize;

        public BookingLogService(IMapper mapper, AppDbContext dbContext, IConfiguration configuration, IBookingHub bookingHub,
            INotificationService notificationService, ITransactionService transactionService)
        {
            _mapper = mapper;
            _dbContext = dbContext;
            _bookingHub = bookingHub;
            _notificationService = notificationService;
            _transactionService = transactionService;

            _configuration = configuration;
            MaxFileSize = int.Parse(_configuration["MaxFileSizeAllowed"]);
        }

        public ResultModel Get(Guid? id)
        {
            var result = new ResultModel();
            try
            {
                var bookingLogs = _dbContext.BookingLogs.Where(s => id == null || (s.IsDisable == false && s.Id == id)).ToList();

                result.Data = _mapper.Map<List<BookingLog>, List<BookingLogViewModel>>(bookingLogs);
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        public ResultModel GetByBookingId(Guid bookingId)
        {
            var result = new ResultModel();
            try
            {
                var bookingLogs = _dbContext.BookingLogs.Include(s => s.BookingImages)
                                                        .Where(s => s.BookingId == bookingId)
                                                        .OrderBy(s => s.DateCreated).ToList();

                result.Data = _mapper.Map<List<BookingLog>, List<BookingLogWithImagesViewModel>>(bookingLogs);
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        public ResultModel Create(BookingLogCreateModel model)
        {
            var result = new ResultModel();
            try
            {
                var bookingLog = _mapper.Map<BookingLogCreateModel, BookingLog>(model);

                _dbContext.Add(bookingLog);
                _dbContext.SaveChanges();

                result.Data = bookingLog.Id;
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        public ResultModel Update(Guid id, BookingLogUpdateModel model)
        {
            var result = new ResultModel();
            try
            {
                var bookingLog = _dbContext.BookingLogs.FirstOrDefault(s => s.Id == id);

                if (bookingLog == null)
                {
                    throw new Exception("Invalid Id");
                }

                bookingLog = _mapper.Map<BookingLogUpdateModel, BookingLog>(model);
                bookingLog.DateUpdated = DateTime.UtcNow.AddHours(7);

                _dbContext.Update(bookingLog);
                _dbContext.SaveChanges();

                result.Data = bookingLog.Id;
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        public ResultModel Delete(Guid id)
        {
            var result = new ResultModel();
            try
            {
                var bookingLog = _dbContext.BookingLogs.FirstOrDefault(s => s.Id == id);

                if (bookingLog == null)
                {
                    throw new Exception("Invalid Id");
                }

                bookingLog.IsDisable = true;
                bookingLog.DateUpdated = DateTime.UtcNow.AddHours(7);

                _dbContext.Update(bookingLog);
                _dbContext.SaveChanges();

                result.Data = bookingLog.Id;
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        public async Task<ResultModel> FinishBooking(Guid bookingId)
        {
            var result = new ResultModel();
            try
            {
                var bLog = _dbContext.BookingLogs.Include(bl => bl.Booking)
                                                 .Include(bl => bl.Booking.Customer)
                                                 .Include(bl => bl.Booking.Province)
                                                 .Include(bl => bl.Booking.District)
                                                 .Include(bl => bl.Booking.Ward)
                                                 .Where(bl => bl.BookingId == bookingId)
                                                 .Where(bl => bl.IsDisable == false)
                                                 .OrderByDescending(bl => bl.DateCreated)
                                                 .DistinctBy(bl => bl.BookingId).FirstOrDefault();

                if (bLog.BookingStatusId == ConstBookingStatus.CONFIRM_WAITING)
                {
                    // _bookingService.UpdateBookingStatus(bookingId, ConstBookingStatus.DONE);
                    BookingLogCreateModel blcModel = new BookingLogCreateModel()
                    {
                        BookingId = bookingId,
                        BookingStatusId = ConstBookingStatus.DONE,
                        Description = "Đặt dịch vụ dọn dẹp đã tự động thành \"Hoàn thành\""
                    };
                    Guid bLogId = (Guid)Create(blcModel).Data;

                    // Update date end of booking
                    var booking = bLog.Booking;
                    booking.DateEnd = DateTime.UtcNow.AddHours(7);

                    _dbContext.Update(booking);
                    _dbContext.SaveChanges();

                    // Transaction Customer
                    TransactionCreateModel cusTrans = new TransactionCreateModel()
                    {
                        UserId = booking.CustomerId,
                        Deposit = -booking.TotalPrice,
                        BookingId = booking.Id,
                        Description = "Thanh toán đặt lịch thành công",
                        Type = ConstTransactionType.USER
                    };
                    _transactionService.Create(cusTrans);

                    // Transaction Company
                    TransactionCreateModel companyTrans = new TransactionCreateModel()
                    {
                        UserId = _dbContext.Users.FirstOrDefault(u => u.UserName == ConstGeneral.COMPANY_USERNAME).Id,
                        Deposit = booking.TotalPrice,
                        BookingId = booking.Id,
                        Description = "Đặt lịch đã hoàn thành",
                        Type = ConstTransactionType.COMPANY
                    };
                    _transactionService.Create(companyTrans);

                    //await _bookingHub.ChangeBooking(booking.CustomerId, booking.EmployeeId, statusId);
                    await _bookingHub.ChangeBooking(booking.CustomerId, booking.EmployeeId);

                    // thong bao toi emp dat lich da ket thuc
                    NotificationCreateModel notiModel = new NotificationCreateModel()
                    {
                        BookingId = booking.Id,
                        UserId = booking.EmployeeId,
                        Type = ConstNotificationType.BOOKING,
                        Description = "Đặt lịch dọn dẹp đã hoàn thành"
                    };
                    _notificationService.Create(notiModel);

                    // thong bao toi emp dat lich da ket thuc
                    notiModel.UserId = booking.CustomerId;
                    _notificationService.Create(notiModel);
                }

                result.Data = "OK";
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        #region Process Image
        public ResultModel GetImagesByBookingId(Guid bookingId, string role)
        {
            var result = new ResultModel();
            try
            {
                var bookingLog1 = _dbContext.BookingLogs.Include(b => b.BookingImages)
                                                        .Where(bl => bl.IsDisable == false)
                                                        .Where(b => b.BookingId == bookingId && b.IsDisable == false)
                                                        .Where(bl => bl.BookingImages != null && bl.BookingImages.Count() > 0)
                                                        .Where(b => b.BookingStatusId == ConstBookingStatus.PROCESSING)
                                                        .OrderByDescending(b => b.DateCreated).FirstOrDefault();

                List<Guid> referenceImages = new List<Guid>();
                List<Guid> evidenceImages  = new List<Guid>();
                if (bookingLog1 != null)
                {
                    foreach (var image in bookingLog1.BookingImages)
                    {
                        referenceImages.Add(image.Id);
                    }

                    var bookingLog2 = _dbContext.BookingLogs.Include(b => b.BookingImages)
                                                            .Where(bl => bl.IsDisable == false)
                                                            .Where(b => b.BookingId == bookingId && b.IsDisable == false)
                                                            .Where(bl => bl.BookingImages != null && bl.BookingImages.Count() > 0)
                                                            .Where(b => b.BookingStatusId == ConstBookingStatus.CONFIRM_WAITING)
                                                            .OrderByDescending(b => b.DateCreated).FirstOrDefault();

                    if (bookingLog2 != null)
                    {
                        foreach (var image in bookingLog2.BookingImages)
                        {
                            evidenceImages.Add(image.Id);
                        }
                    }
                }

                if(role == ConstRole.CUSTOMER)
                {
                    var countReclean = _dbContext.BookingLogs.Include(bl => bl.Booking)
                                                  .Where(bl => bl.IsDisable == false)
                                                  .Where(bl => bl.Booking.IsDisable == false)
                                                  .Where(bl => bl.BookingId == bookingId)
                                                  .Where(bl => bl.BookingStatusId == ConstBookingStatus.RE_CLEANING)
                                                  .ToList().Count;

                    var numberRecleanStr = _dbContext.Settings.FirstOrDefault(s => s.Key == ConstSetting.NUMBER_RE_CLEAN).Data;
                    int numerReclean = int.Parse(numberRecleanStr);
                    result.Data = new ImagesCustomerViewModel()
                    {
                        ShowRecleanBtn = countReclean < numerReclean,
                        ReferenceImages = referenceImages,
                        EvidenceImages = evidenceImages
                    };
                }
                else
                {
                    result.Data = new ImagesUserViewModel()
                    {
                        ReferenceImages = referenceImages,
                        EvidenceImages = evidenceImages
                    };
                }
                
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        public async Task<ResultModel> AddImages(List<IFormFile> files, Guid bookingId, string statusId)
        {
            var result = new ResultModel();
            try
            {
                string fileLocation = "";
                string notiDescription = "";
                string bLogDescription = "";
                if (statusId == ConstBookingStatus.PROCESSING)
                {
                    fileLocation = Path.Combine($"FileStored/BookingImageReferences");

                    notiDescription = "Nhân viên đã bắt đầu dọn dẹp";

                    bLogDescription = "Nhân viên bắt đầu dọn dẹp";
                }
                else if (statusId == ConstBookingStatus.CONFIRM_WAITING)
                {
                    fileLocation = Path.Combine($"FileStored/BookingImageEvidences");

                    notiDescription = "Nhân viên đã hoàn thành công việc";

                    bLogDescription = "Nhân viên đã hoàn thành công và đợi xác nhận từ khách hàng";

                    var minutesStr = _dbContext.Settings.FirstOrDefault(s => s.Key == ConstSetting.PENDING_MINUTES_CONFIRM_BOOKING && s.IsDisable == false).Data;
                    int minutes = int.Parse(minutesStr);
                    BackgroundJob.Schedule(() => FinishBooking(bookingId), new TimeSpan(0, minutes, 0));

                }

                BookingLogCreateModel blcModel = new BookingLogCreateModel()
                {
                    BookingId = bookingId,
                    BookingStatusId = statusId,
                    Description = bLogDescription
                };
                Guid bLogId = (Guid)Create(blcModel).Data;

                foreach (var item in files)
                {
                    await HandleFile(item, bLogId, fileLocation);
                }

                await _dbContext.SaveChangesAsync();

                result.Data = bLogId;
                result.Succeed = true;

                var booking = _dbContext.Bookings.FirstOrDefault(b => b.Id == bookingId);

                await _bookingHub.ChangeBooking(booking.CustomerId, booking.EmployeeId);
                // await _bookingHub.ChangeBooking(booking.CustomerId, booking.EmployeeId, statusId);

                // thông báo công việc của nhân viên cho khách hàng
                NotificationCreateModel notiCus = new NotificationCreateModel()
                {
                    BookingId = booking.Id,
                    UserId = booking.CustomerId,
                    Type = ConstNotificationType.BOOKING,
                    Description = notiDescription
                };
                _notificationService.Create(notiCus);

            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        private async Task<Guid> HandleFile(IFormFile file, Guid bookingLogId, string fileLocation)
        {
            if (file.Length > MaxFileSize)
            {
                throw new Exception("File size too large. File name: " + file.FileName);
            }

            var image = new BookingImage()
            {
                BookingLogId = bookingLogId,
                FileExtension = Path.GetExtension(file.FileName)
            };

            _dbContext.BookingImages.Add(image);

            var pictureName = $"{image.Id}{Path.GetExtension(file.FileName)}";

            using (var fileStream = new FileStream(Path.Combine(fileLocation, pictureName), FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
                fileStream.Flush();
                fileStream.Close();
            }
            return image.Id;
        }

        public FileModel GetImageById(Guid fileId, string fileLocation)
        {
            var result = new FileModel();
            var bookingImage = _dbContext.BookingImages.FirstOrDefault(e => e.Id == fileId);

            if (bookingImage == null)
            {
                throw new Exception("Invalid Id");
            }

            var data = File.ReadAllBytes(fileLocation + "/" + bookingImage.Id + bookingImage.FileExtension);
            result.Id = bookingImage.Id;
            result.Data = data;
            result.FileType = bookingImage.FileType;

            return result;
        }
        #endregion

    }

}
