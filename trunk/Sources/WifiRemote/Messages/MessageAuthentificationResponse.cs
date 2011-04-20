﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WifiRemote.Messages
{
    public class MessageAuthentificationResponse
    {
        string type = "authentificationresponse";
  
        public MessageAuthentificationResponse(bool success)
        {
            this.Success = success;
        }
        
        public String Type
        {
            get { return type; }
        }

        /// <summary>
        /// Indicator if authentification was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Error messsage in case authentification failed
        /// </summary>
        public String ErrorMessage { get; set; }
    }
}
