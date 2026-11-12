using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using System;
using System.Collections.Generic;

namespace Trianz.Enterprise.Operations
{
    public class SesEmailSender
    {
        private readonly IAmazonSimpleEmailService _sesClient;

        public SesEmailSender()
        {
            _sesClient = new AmazonSimpleEmailServiceClient();
        }

        public SesEmailSender(IAmazonSimpleEmailService client)
        {
            _sesClient = client;
        }

        public SendEmailResponse SendEmail(string from, string to, string subject, string body, bool isHtml = false)
        {
            var request = new SendEmailRequest
            {
                Source = from,
                Destination = new Destination { ToAddresses = new List<string> { to } },
                Message = new Message
                {
                    Subject = new Content(subject),
                    Body = isHtml ? new Body { Html = new Content(body) } : new Body { Text = new Content(body) }
                }
            };
            return _sesClient.SendEmail(request);
        }

        public SendEmailResponse SendEmail(string from, List<string> to, string subject, string body, bool isHtml = false)
        {
            var request = new SendEmailRequest
            {
                Source = from,
                Destination = new Destination { ToAddresses = to },
                Message = new Message
                {
                    Subject = new Content(subject),
                    Body = isHtml ? new Body { Html = new Content(body) } : new Body { Text = new Content(body) }
                }
            };
            return _sesClient.SendEmail(request);
        }
    }
}
