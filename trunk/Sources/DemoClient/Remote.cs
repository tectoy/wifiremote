﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ZeroconfService;
using System.Diagnostics;
using Deusty.Net;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace DemoClient
{
    public partial class Remote : Form
    {
        // Bonjour browsing
        bool useBonjour;
        NetServiceBrowser bonjourBrowser = new NetServiceBrowser();
        BindingList<NetService> servers = new BindingList<NetService>();

        // TCP connection
        AsyncSocket socket;

        /// <summary>
        /// Bonjour service type
        /// </summary>
        private string serviceType = "_mepo-remote._tcp";

        /// <summary>
        /// Bonjour domain (empty = whole network)
        /// </summary>
        private string domain = "";

        /// <summary>
        /// Window to show output in
        /// </summary>
        MessageLog logWindow = new MessageLog();

        // Reusable commands
        MessageVolume volumeMessage = new MessageVolume();
        MessageCommand commandMessage = new MessageCommand();


        public Remote()
        {
            InitializeComponent();

            // Bonjour check
            try
            {
                logWindow.Log = String.Format("Bonjour Version: {0}", NetService.DaemonVersion);
                useBonjour = true;
            }
            catch (Exception ex)
            {
                String message = ex is DNSServiceException ? "Bonjour is not installed!" : ex.Message;
                MessageBox.Show(message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Information);
                useBonjour = false;
            }

            // Start browsing bonjour for WifiRemote service
            if (useBonjour)
            {
                listBoxServers.DataSource = servers;
                listBoxServers.DisplayMember = "Name";

                bonjourBrowser.InvokeableObject = this;
                bonjourBrowser.DidFindService += new NetServiceBrowser.ServiceFound(bonjourBrowser_DidFindService);
                bonjourBrowser.DidRemoveService += new NetServiceBrowser.ServiceRemoved(bonjourBrowser_DidRemoveService);
                
                try
                {
                    bonjourBrowser.SearchForService(serviceType, domain);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Critical Error");
                }
            }
        }

        #region Bonjour
        /// <summary>
        /// Bonjour service vanished
        /// </summary>
        /// <param name="browser"></param>
        /// <param name="service"></param>
        /// <param name="moreComing"></param>
        void bonjourBrowser_DidRemoveService(NetServiceBrowser browser, NetService service, bool moreComing)
        {
            servers.Remove(service);
            if (servers.Count == 0)
            {
                buttonConnectDetected.Enabled = false;
            }
        }

        /// <summary>
        /// Did find bonjour service
        /// </summary>
        /// <param name="browser"></param>
        /// <param name="service"></param>
        /// <param name="moreComing"></param>
        void bonjourBrowser_DidFindService(NetServiceBrowser browser, NetService service, bool moreComing)
        {
            service.DidResolveService += new NetService.ServiceResolved(Remote_DidResolveService);
            service.ResolveWithTimeout(5);
        }

        /// <summary>
        /// Detected bonjour service was resolved
        /// </summary>
        /// <param name="service"></param>
        void Remote_DidResolveService(NetService service)
        {
            servers.Add(service);
            if (!buttonConnectDetected.Enabled)
            {
                buttonConnectDetected.Enabled = true;
            }
        }

        #endregion


        #region Socket connection
        void socket_DidWrite(AsyncSocket sender, long tag)
        {

        }

        void socket_DidRead(AsyncSocket sender, byte[] data, long tag)
        {
            String msg = null;

            try
            {
                msg = Encoding.UTF8.GetString(data);
                logWindow.Log = "Received message: " + msg;
                // Get json object
                JObject message = JObject.Parse(msg);
                string type = (string)message["Type"];

                if (type != null)
                {
                    // {"Type":"welcome","Server_Version":4,"AuthMethod":0}
                    switch (type) 
                    {
                        case "welcome":
                            handleWelcomeMessage((int)message["Server_Version"], (int)message["AuthMethod"]);
                            break;

                        // {"Type":"authenticationresponse","Success":true,"ErrorMessage":null}
                        case "authenticationresponse":
                            handleAuthenticationResponse((bool)message["Success"], (String)message["ErrorMessage"]);
                            break;

                        // {"Type":"status","IsPlaying":false,"IsPaused":false,"Title":"","CurrentModule":"Startbildschirm","SelectedItem":""}
                        case "status":
                            handleStatus((bool)message["IsPlaying"], (bool)message["IsPaused"], (string)message["Title"], (string)message["CurrentModule"], (string)message["SelectedItem"]);
                            break;

                        // {"Type":"volume","Volume":37,"IsMuted":false}
                        case "volume":
                            handleVolumeChange((int)message["Volume"], (bool)message["IsMuted"]);
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Log(null, "Communication Error: " + e.Message, null);
            }

            // Continue listening
            sender.Read(AsyncSocket.CRLFData, -1, 0);
        }

        void socket_DidClose(AsyncSocket sender)
        {
            labelDetail.Text = "";
            Log("", "Disconnected", "Disconnected");
        }

        bool socket_WillConnect(AsyncSocket sender, System.Net.Sockets.Socket socket)
        {
            return true;
        }

        void socket_WillClose(AsyncSocket sender, Exception e)
        {

        }

        /// <summary>
        /// We have a socket connection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="address"></param>
        /// <param name="port"></param>
        void socket_DidConnect(AsyncSocket sender, System.Net.IPAddress address, ushort port)
        {
            Log("Authenticating with server", "Connected, waiting for welcome message", "Authenticating ...");
            socket.Read(AsyncSocket.CRLFData, -1, 0);
        }

        #endregion

        #region form events

        /// <summary>
        /// Connect to server selected in autodetect list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonConnectDetected_Click(object sender, EventArgs e)
        {
            if (servers.Count <= 0 || listBoxServers.SelectedItem == null)
            {
                return;
            }

            if (socket != null)
            {
                socket.Close();
                socket = null;
            }

            Log("Connecting ...", null, "Connecting to server");

            socket = new AsyncSocket();
            socket.WillConnect += new AsyncSocket.SocketWillConnect(socket_WillConnect);
            socket.DidConnect += new AsyncSocket.SocketDidConnect(socket_DidConnect);
            socket.WillClose += new AsyncSocket.SocketWillClose(socket_WillClose);
            socket.DidClose += new AsyncSocket.SocketDidClose(socket_DidClose);
            socket.DidRead += new AsyncSocket.SocketDidRead(socket_DidRead);
            socket.DidWrite += new AsyncSocket.SocketDidWrite(socket_DidWrite);

            if (!socket.Connect(((NetService)listBoxServers.SelectedItem).HostName, (ushort)((NetService)listBoxServers.SelectedItem).Port))
            {
                Log(null, "Could not connect to server: AsyncSocket connect failed", "Could not connect to server");
                MessageBox.Show("Could not connect to server", "Error");
            }
        }

        /// <summary>
        /// Connect with manually entered info
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonConnect_Click(object sender, EventArgs e)
        {
            if (socket != null)
            {
                socket.Close();
                socket = null;
            }

            Log("Connecting ...", null, "Connecting to server");

            socket = new AsyncSocket();
            socket.WillConnect += new AsyncSocket.SocketWillConnect(socket_WillConnect);
            socket.DidConnect += new AsyncSocket.SocketDidConnect(socket_DidConnect);
            socket.WillClose += new AsyncSocket.SocketWillClose(socket_WillClose);
            socket.DidClose += new AsyncSocket.SocketDidClose(socket_DidClose);
            socket.DidRead += new AsyncSocket.SocketDidRead(socket_DidRead);
            socket.DidWrite += new AsyncSocket.SocketDidWrite(socket_DidWrite);


            ushort thePort;
            bool isValidPort = ushort.TryParse(textBoxPort.Text, out thePort);

            if (!isValidPort || !socket.Connect(textBoxAddress.Text, thePort))
            {
                Log(null, "Could not connect to server: AsyncSocket connect failed", "Could not connect to server");
                MessageBox.Show("Could not connect to server", "Error");
            }
        }

        /// <summary>
        /// Show the log window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripStatusLabel1_Click(object sender, EventArgs e)
        {
            if (logWindow.Visible)
            {
                logWindow.Hide();
            }
            else
            {
                logWindow.Show();
            }
        }


        /// <summary>
        /// Volume slider
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void trackBarVolume_Scroll(object sender, EventArgs e)
        {
            volumeMessage.Volume = ((TrackBar)sender).Value;
            SendCommand(volumeMessage, socket);
        }


        /// <summary>
        /// Button clicked, send command
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonCommand_Click(object sender, EventArgs e)
        {
            if (sender == buttonOk)
            {
                commandMessage.Command = "ok";
            }
            else if (sender == buttonRight) 
            {
                commandMessage.Command = "right";
            }
            else if (sender == buttonLeft) 
            {
                commandMessage.Command = "left";
            }
            else if (sender == buttonDown) 
            {
                commandMessage.Command = "down";
            }
            else if (sender == buttonUp) 
            {
                commandMessage.Command = "up";
            }
            else if (sender == buttonHome) 
            {
                commandMessage.Command = "home";
            }
            else if (sender == buttonBack) 
            {
                commandMessage.Command = "back";
            }
            else if (sender == buttonMenu)
            {
                commandMessage.Command = "menu";
            }
            else if (sender == buttonStop)
            {
                commandMessage.Command = "stop";
            }
            else
            {
                // Don't send unknown command
                return;
            }

            SendCommand(commandMessage, socket);
        }

        private void textBoxAddress_Enter(object sender, EventArgs e)
        {
            if (textBoxAddress.Text == "Hostname")
            {
                textBoxAddress.Text = "";
                textBoxAddress.ForeColor = Color.Black;
            }
        }

        private void textBoxAddress_Leave(object sender, EventArgs e)
        {
            if (textBoxAddress.Text == "")
            {
                textBoxAddress.Text = "Hostname";
                textBoxAddress.ForeColor = Color.LightGray;
            }
        }

        private void textBoxPort_Enter(object sender, EventArgs e)
        {
            if (textBoxPort.Text == "Port")
            {
                textBoxPort.Text = "";
                textBoxPort.ForeColor = Color.Black;
            }
        }

        private void textBoxPort_Leave(object sender, EventArgs e)
        {
            if (textBoxPort.Text == "")
            {
                textBoxPort.Text = "Port";
                textBoxPort.ForeColor = Color.LightGray;
            }
        }

        #endregion


        #region Messages and Commands

        /// <summary>
        /// Send a message object
        /// </summary>
        /// <param name="message"></param>
        /// <param name="client"></param>
        public void SendCommand(IMessage message, AsyncSocket client)
        {
            if (message == null)
            {
                logWindow.Log = "SendMessage failed: IMessage object is null";
                return;
            }

            string messageString = JsonConvert.SerializeObject(message);
            SendCommand(messageString, client);
        }

        /// <summary>
        /// Send a message string
        /// </summary>
        /// <param name="message"></param>
        /// <param name="client"></param>
        public void SendCommand(String message, AsyncSocket client)
        {
            if (message == null)
            {
                logWindow.Log = "SendMessage failed: Message string is null";
                return;
            }

            if (client == null)
            {
                Log(null, "SendMessage aborted: Not connected", "Please connect first!");
                return;
            }

            logWindow.Log = "Sending command: " + message;
            byte[] data = Encoding.UTF8.GetBytes(message + "\r\n");
            client.Write(data, -1, 0);
        }

        private void handleWelcomeMessage(int serverVersion, int authMethod)
        {
            switch (authMethod)
            {
                // AuthMethod: User&Password or Both
                case 1:
                case 3:
                    break;

                // AuthMethod: Passcode
                case 2:
                    break;

                // AuthMethod: None
                case 0:
                default:
                    SendCommand(new MessageIdentify(), socket);
                    break;
            }
        }

        private void handleAuthenticationResponse(bool success, String error)
        {
            if (success)
            {
                Log("", "Successfully authenticated with server", "Connected");
            }
            else
            {
                Log("Disconnected", "Failed to authenticate with server: "+ error, "Auth failed");
            }
        }

        private void handleVolumeChange(int newVolume, bool isMuted)
        {
            if (isMuted)
            {
                trackBarVolume.Value = 0;
            }
            else
            {
                if (newVolume < 0) newVolume = 0;
                if (newVolume > 100) newVolume = 100;

                trackBarVolume.Value = newVolume;
            }
        }

         
        private void handleStatus(bool playing, bool paused, string title, string module, string selectedItem)
        {
            if (module != null)
            {
                labelStatus.Text = module;
            }

            if (selectedItem != null && selectedItem != String.Empty)
            {
                labelDetail.Text = selectedItem;
            }
            else if (title != null)
            {
                labelDetail.Text = title;
            }
            else
            {
                labelDetail.Text = "";
            }
        }

        #endregion

        #region Public methods

        public void Log(string statusLabelText, string logWindowText, string toolStripText)
        {
            if (statusLabelText != null)
            {
                labelStatus.Text = statusLabelText;
            }

            if (logWindowText != null)
            {
                logWindow.Log = logWindowText;
            }

            if (toolStripText != null)
            {
                toolStripStatusLabel1.Text = toolStripText;
            }
        }

        #endregion




    }
}
