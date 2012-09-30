using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Antem.Parts
{
    public class Server
    {
        /// <summary>
        /// Address of the server
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// User to authenticate
        /// </summary>
        public string User { get; set; }

        /// <summary>
        /// Password to authenticate
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Port used to interact
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Name of the database
        /// </summary>
        public string Database { get; set; }

        public string Protocol { get; set; }
    }
}
