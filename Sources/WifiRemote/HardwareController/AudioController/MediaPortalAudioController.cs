﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MediaPortal.Player;

namespace WifiRemote.HardwareController.AudioController
{
    class MediaPortalAudioController : IAudioController
    {
        public void VolumeUp()
        {
            VolumeHandler.Instance.Volume = VolumeHandler.Instance.Next;
        }

        public void VolumeDown()
        {
            VolumeHandler.Instance.Volume = VolumeHandler.Instance.Previous;
        }

        public void ToggleMute()
        {
            VolumeHandler.Instance.IsMuted = !VolumeHandler.Instance.IsMuted;
        }

        /// <summary>
        /// Sets the volume of the client
        /// </summary>
        /// <param name="volume">The new volume, ranging from 0 to 100</param>
        /// <param name="relative">True if the volume should be changed relative to the current volume</param>
        public void SetVolume(int volume, bool relative)
        {
            if (relative)
            {
                int currentVolume = 0;

                try
                {
                    currentVolume = VolumeHandler.Instance.Volume / (VolumeHandler.Instance.Maximum / 100);
                }
                catch (Exception) { }

                volume += currentVolume;
            }

            if (volume >= 0 && volume <= 100)
            {
                VolumeHandler.Instance.Volume = (int)Math.Floor(volume * VolumeHandler.Instance.Maximum / 100.0);
            }
        }
    }
}
