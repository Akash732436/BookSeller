using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity.UI.Services;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookSeller.Utility
{
	public class EmailManager : IEmailSender
	{
		public Task SendEmailAsync(string email, string subject, string htmlMessage)
		{
			var emailToSend = new MimeMessage();
			emailToSend.From.Add(MailboxAddress.Parse("akashhoo65@gmail.com"));
			emailToSend.To.Add(MailboxAddress.Parse(email));
			emailToSend.Subject = subject;
			emailToSend.Body=new TextPart(MimeKit.Text.TextFormat.Html) { Text= htmlMessage };


			//send email
			using(var emailClient = new SmtpClient())
			{
				emailClient.Connect("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
				emailClient.Authenticate("akashhoo65@gmail.com", "mvuwzhcrgeprysgo");
				emailClient.Send(emailToSend);
				emailClient.Disconnect(true);
			}

			return Task.CompletedTask;
		}
	}
}
