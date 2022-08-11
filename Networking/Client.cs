using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net.Sockets;
using System.Windows.Forms;

namespace Race
{
    public delegate void Data_Handler(byte[] data);

    class Client
    {
        TcpClient client;
        NetworkStream sReader;
        NetworkStream sWriter;
        byte[] readBuffer;
        public Data_Handler KeyboardProcessing;
        public Data_Handler PositionProcessing;
        bool connected = true;

        public Client()
        {
            try
            {
                client = new TcpClient("127.0.0.1", 4584);
            }
            catch (Exception e)
            {
                MessageBox.Show("There was a connection problem. You will return to the menu.");
                MenuManager.MenuAgain(true);
            }
            sReader = client.GetStream();
            sWriter = client.GetStream();
            readBuffer = new byte[16];
        }
        public Client(TcpClient client)
        {
            this.client = client;
            sReader = client.GetStream();
            sWriter = client.GetStream();
            readBuffer = new byte[16];
        }

        public void GiveHandshake()
        {
            lock (client.GetStream())
            {
                try
                {
                    sWriter.BeginWrite(new byte[] { Convert.ToByte('s') }, 0, 1, null, null);
                }
                catch (Exception e)
                {
                    MenuManager.MenuAgain(true);
                }
            }
        }
        public void RecieveHandShake()
        {
            byte[] shake = new byte[1];
            lock (client.GetStream())
            {
                try
                {
                    sReader.Read(shake, 0, 1);
                }
                catch (Exception e)
                {
                    MenuManager.MenuAgain(true);
                }
            }
        }

        #region non-blocking methods
        public void SendData(byte[] data)
        {
            if (connected)
            {
                try
                {
                    sWriter.BeginWrite(data, 0, data.Length, DataSent, null);
                }
                catch (Exception e)
                {
                    MenuManager.MenuAgain(true);
                    connected = false;
                }
            }
        }
        public void DataSent(IAsyncResult ar)
        {
        }
        public void RecieveData()
        {
            if (connected)
            {
                try
                {
                    sReader.BeginRead(readBuffer, 0, readBuffer.Length, DataReceived, null);
                }
                catch (Exception e)
                {
                    MenuManager.MenuAgain(true);
                    connected = false;
                }
            }
        }
        public void DataReceived(IAsyncResult ar)
        {
            if (connected)
            {
                try
                {
                    int byteNum = sReader.EndRead(ar);
                }
                catch (Exception e)
                {
                    MenuManager.MenuAgain(true);
                    connected = false;
                }
            }
            PositionProcessing(readBuffer);
            //if (Convert.ToBoolean(readBuffer[0]))
            //    KeyboardProcessing(readBuffer);
            //else
            //    PositionProcessing(readBuffer);
        } 
        #endregion

        #region blocking methods
        public void BlockSendData(byte[] data)
        {
            sWriter.Write(data, 0, data.Length);
        }
        public void BlockReceiveData()
        {
            byte[] result = new byte[readBuffer.Length];
            sReader.Read(result, 0, result.Length);
            KeyboardProcessing(result);
        } 
        #endregion
    }
}