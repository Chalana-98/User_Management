﻿using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace User.Management.Service.Models
{
    public class Message
    {
        public List <MailboxAddress>? To { get; set; }
        public string? Subject { get; set; }
        public string? Content { get; set; }
        public Message (IEnumerable<string> to, string subject, string content)
        {
            To = new List<MailboxAddress>();
            Subject = subject;
            Content = content;
            To.AddRange(to.Select(x => new MailboxAddress("email",x)));
        }
    }
}
