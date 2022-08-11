using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;

namespace Race
{
    class Server
    {
        TcpListener listener;
        public TcpClient connectedClient;

        public Server()
        {
            try
            {
                listener = new TcpListener(IPAddress.Any, 4584);
                listener.Start();
                connectedClient = listener.AcceptTcpClient();
            }
            catch (Exception e)
            {
                MessageBox.Show("There was a connection problem. You will return to the menu.");
                MenuManager.MenuAgain(true);
            }
        }
    }
}
