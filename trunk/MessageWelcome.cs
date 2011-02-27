﻿using System;
using System.Collections.Generic;
using MediaPortal.GUI.Library;
using MediaPortal.Configuration;
using System.Collections;
using System.Linq;

namespace WifiRemote
{
    class MessageWelcome
    {
        string type = "welcome";
        int server_version = 1;
        ArrayList plugins;
        MessageStatus status;
        MessageVolume volume;

        public string Type
        {
            get { return type; }
        }

        public int Server_Version 
        { 
            get { return server_version; }
        }

        public ArrayList Plugins
        {
            get { return plugins; }
        }

        public MessageStatus Status
        {
            get { return status; }
            set { status = value; }
        }

        public MessageVolume Volume
        {
            get { return volume; }
            set { volume = value; }
        }


        /// <summary>
        /// Contructor.
        /// </summary>
        public MessageWelcome()
        {
            plugins = WifiRemote.GetActiveWindowPluginsAndIDs();
        }
    }
}