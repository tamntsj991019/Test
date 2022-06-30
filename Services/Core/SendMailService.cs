using Data.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace Services.Core
{
    public interface ISendMailService
    {
        void SendMail(string email, string subject, string body);
        void SendHTMLMail(string email, string subject, string code);
        FileModel GetImageByName(string name);
    }
    public class SendMailService : ISendMailService
    {
        public async void SendMail(string email, string subject, string body)
        {
            var smtpClient = new SmtpClient
            {
                Host = "smtp.gmail.com", // set your SMTP server name here
                Port = 587, // Port 
                EnableSsl = true,
                DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("becleanservice6789@gmail.com", "Bcs_6789")
            };

            using (var message = new MailMessage("becleanservice6789@gmail.com", email)
            {
                Subject = subject,
                Body = body,
            })
            {
                message.From = new MailAddress("becleanservice6789@gmail.com", "BeCleanService");
                await smtpClient.SendMailAsync(message);
            }
        }

        public async void SendHTMLMail(string email, string subject, string code)
        {
            var smtpClient = new SmtpClient
            {
                Host = "smtp.gmail.com", // set your SMTP server name here
                Port = 587, // Port 
                EnableSsl = true,
                DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("becleanservice6789@gmail.com", "Bcs_6789")
            };

            string readText = File.ReadAllText(Path.Combine($"FileStored/HTML/ActiveCode.html"));
            using (var message = new MailMessage("becleanservice6789@gmail.com", email)
            {
                Subject = subject,
                Body = readText.Replace("{CODE}", code)

            })
            {
                message.IsBodyHtml = true;
                message.From = new MailAddress("becleanservice6789@gmail.com", "BeCleanService");
                await smtpClient.SendMailAsync(message);
            }
        }

        public FileModel GetImageByName(string name)
        {
            var result = new FileModel();
            string fileLocation = Path.Combine($"FileStored/MailImages");

            var data = File.ReadAllBytes(fileLocation + "/" + name + ".jpg");
            result.Data = data;
            result.FileType = "image/jpeg";

            return result;
        }
    }
}
