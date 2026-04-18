using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommUnityApp.ApplicationCore.Models
{
    internal class Email
    {
    }
    public class BulkEmailRequest
    {
       
        public List<string> Emails { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }
}
