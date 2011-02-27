﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MediaPortal.Player;

namespace WifiRemote
{
    class MessageVolume
    {
        string type = "volume";

        public string Type
        {
            get { return type; }
        }

        /// <summary>
        /// Current volume in percent
        /// </summary>
        public int Volume
        {
            get
            {
                try
                {
                    return VolumeHandler.Instance.Volume / (VolumeHandler.Instance.Maximum / 100);
                }
                catch (Exception)
                {
                    return 101;
                }
            }
        }

        public bool IsMuted
        {
            get { return VolumeHandler.Instance.IsMuted; }
        }
    }
}