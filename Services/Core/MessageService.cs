using AutoMapper;
using Data;
using Data.DbContext;
using Data.Entities;
using Data.ViewModels;
using Hubs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Core
{
    public interface IMessageService
    {
        Task<ResultModel> SendMessage(MessageCreateModel model, string userId);
        ResultModel GetMessage(Guid bookingId, int pageIndex, int pageSize, string userId);
        FileModel GetMessageFile(Guid messageId);
        ResultModel SeenMessage(Guid conversationId, string userId);
    }
    public class MessageService : IMessageService
    {
        private readonly AppDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly int MaxFileSize;
        private readonly IConfiguration _configuration;
        private readonly IChatHub _chatHub;
        //private readonly UserManager<User> _userManager;

        public MessageService(AppDbContext dbContext, IMapper mapper, IConfiguration configuration, IChatHub chatHub)//, UserManager<User> userManager)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _configuration = configuration;
            _chatHub = chatHub;
            //_userManager = userManager;
            MaxFileSize = int.Parse(_configuration["MaxFileSizeAllowed"]);
        }

        public async Task<ResultModel> SendMessage(MessageCreateModel model, string userId)
        {
            var result = new ResultModel();
            try
            {
                var conversation = _dbContext.Conversations.Any(c => c.Id == model.ConversationId && (c.Booking.CustomerId == userId || c.Booking.EmployeeId == userId));
                if (!conversation)
                {
                    throw new Exception("Invalid conversation");
                }
                Message message = new Message()
                {
                    ConversationId = model.ConversationId,
                    Description = model.Description,
                    SenderId = userId
                };
                if (model.File != null)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await model.File.CopyToAsync(memoryStream);
                        // Upload the file if less than x MB
                        if (memoryStream.Length < MaxFileSize)
                        {
                            string fileType = "image/jpeg";
                            if (model.File.ContentType != null && !string.IsNullOrEmpty(model.File.ContentType))
                            {
                                fileType = model.File.ContentType;
                            }

                            message.Data = memoryStream.ToArray();
                            message.FileType = fileType;
                            message.FileName = model.File.FileName;
                        }
                        else
                        {
                            throw new Exception("File size too large. File name: " + model.File.FileName);
                        }
                    }
                }
                else
                {
                    message.FileName = null;
                    message.Data = null;
                }

                _dbContext.Add(message);
                _dbContext.SaveChanges();

                // Send Message
                var messageViewModel = new MessageViewModel()
                {
                    Id = message.Id,
                    DateCreated = message.DateCreated.Value,
                    YourMessage = false,
                    Description = message.Description,
                    FileName = message.FileName,
                    IsFile = message.FileName != null,
                    ConversationId = model.ConversationId,
                    BookingId = _dbContext.Conversations.FirstOrDefault(c => c.Id == model.ConversationId).BookingId
                };
                var receiver = GetReceiver(model.ConversationId, userId).Id;
                await _chatHub.SendMessage(receiver, userId, messageViewModel);

                result.Data = message.Id;
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        public User GetReceiver(Guid conversationId, string userId)
        {
            User result = null;
            try
            {
                var conv = _dbContext.Conversations.FirstOrDefault(_c => _c.Id == conversationId && _c.IsDisable == false);
                if (userId == conv.Booking.CustomerId)
                {
                    result = conv.Booking.Employee;
                }
                else
                {
                    result = conv.Booking.Customer;
                }
            }
            catch (Exception) { }
            return result;
        }

        public ResultModel GetMessage(Guid bookingId, int pageIndex, int pageSize, string userId)
        {
            var result = new ResultModel();
            try
            {
                var conversation = _dbContext.Conversations.Where(c => c.BookingId == bookingId && c.IsDisable == false)
                                                           .OrderByDescending(c => c.DateCreated).FirstOrDefault();

                var listMessage = new List<MessageViewModel>();
                //var conversation = _dbContext.Conversations.Any(c => c.Booking.CustomerId == userId || c.Booking.EmployeeId == userId);
                if (conversation == null)
                {
                    throw new Exception("Invalid Conversation");
                }

                var messages = _dbContext.Messages.Where(m => m.ConversationId == conversation.Id).ToList();
                if (messages == null || messages.Count() == 0)
                {
                    result.Data = new PagingModel
                    {
                        Total = listMessage.Count,
                        //Data = listMessage.Skip((pageIndex - 1) * pageSize).Take(pageSize).OrderBy(_d => _d.DateCreated).ToList()
                        Data = new ChatViewModel()
                        {
                            ConversationId = conversation.Id,
                            Receiver = _mapper.Map<User, UserFullnameAvatarViewModel>(GetReceiver(conversation.Id, userId)),
                            ListMessage = listMessage
                        }
                    };
                    result.Succeed = true;
                    return result;
                }

                messages = messages.OrderByDescending(_m => _m.DateCreated).ToList();
                foreach (var message in messages)
                {
                    var messageViewModel = new MessageViewModel()
                    {
                        Id = message.Id,
                        DateCreated = message.DateCreated.Value,
                        YourMessage = message.SenderId == userId,
                        Description = message.Description,
                        FileName = message.FileName,
                        IsFile = message.FileName != null,
                        ConversationId = conversation.Id
                    };
                    listMessage.Add(messageViewModel);

                    result.Data = new PagingModel
                    {
                        Total = listMessage.Count,
                        //Data = listMessage.Skip((pageIndex - 1) * pageSize).Take(pageSize).OrderBy(_d => _d.DateCreated).ToList()
                        Data = new ChatViewModel()
                        {
                            ConversationId = conversation.Id,
                            Receiver = _mapper.Map<User, UserFullnameAvatarViewModel>(GetReceiver(conversation.Id, userId)),
                            ListMessage = listMessage.Skip((pageIndex - 1) * pageSize).Take(pageSize).OrderBy(_d => _d.DateCreated).ToList()
                        }
                    };
                    result.Succeed = true;
                }
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        public FileModel GetMessageFile(Guid messageId)
        {
            var mess = _dbContext.Messages.FirstOrDefault(_m => _m.Id == messageId);
            var conversation = _dbContext.Conversations.Any(_c => _c.Id == mess.ConversationId);
            if (!conversation)
            {
                throw new Exception("Invalid Message");
            }
            if (string.IsNullOrEmpty(mess.FileName) || mess.Data == null || mess.Data.Length == 0)
            {
                throw new Exception("Invalid Message");
            }
            var result = new FileModel()
            {
                Id = mess.Id,
                Data = mess.Data,
                FileName = mess.FileName,
                FileType = mess.FileType
            };
            return result;
        }

        public ResultModel SeenMessage(Guid conversationId, string userId)
        {
            var result = new ResultModel();
            try
            {
                //var conversation = _dbContext.Conversations.FirstOrDefault(_c => _c.Id == conversationId);
                var message = _dbContext.Messages.Where(_m => _m.ConversationId == conversationId && _m.SenderId != userId).OrderByDescending(_m => _m.DateCreated).First();
                message.Seen = true;

                _dbContext.Update(message);
                _dbContext.SaveChanges();

                result.Data = message.Id;
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }
    }
}
