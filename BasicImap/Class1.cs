using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicIMAP
{
    public class IMAPClient
    {
        // Enumeration for state of IMAP connection
        public enum IMAPState { NoConnection, NotAuthenticated, Authenticated, Selected };

        // Private variables for details of connection
        private System.Net.Sockets.TcpClient tcpc = null;
        private System.Net.Security.SslStream ssl = null;
        private byte[] writeBuffer;
        private int commandNumber;
        private string Host;
        private int Port;


        // Publicly readable state of the connection
        private IMAPState currentState;
        public IMAPState CurrentState
        {
            get
            {
                try
                {
                    if (tcpc.Connected)
                        receiveResponse("");
                    else
                        currentState = IMAPState.NoConnection;
                }
                catch (Exception ex)
                {
                    currentState = IMAPState.NoConnection;
                    if (ssl != null)
                    {
                        ssl.Close();
                        ssl.Dispose();
                    }
                    if (tcpc != null)
                    {
                        tcpc.Close();
                    }
                }
                return currentState;
            }
        }

        // Default constructor. Sets up all private variables for the connection
        public IMAPClient(string host, int port)
        {
            Host = host;
            Port = port;
            Connect();
        }

        public int Connect()
        {
            try
            {
                tcpc = new System.Net.Sockets.TcpClient(Host, Port);
                ssl = new System.Net.Security.SslStream(tcpc.GetStream());
                ssl.ReadTimeout = 2000;
                ssl.AuthenticateAsClient(Host);
                receiveResponse("");
                commandNumber = 0;
                currentState = IMAPState.NotAuthenticated;
                return 0;
            }
            catch (Exception ex)
            {
                currentState = IMAPState.NoConnection;
                return -1;
            }
        }

        private void receiveResponse(string command)
        {
            try
            {
                if (command != "")
                {
                    if (tcpc.Connected)
                    {
                        writeBuffer = Encoding.ASCII.GetBytes(command);
                        ssl.Write(writeBuffer, 0, writeBuffer.Length);
                    }
                    else
                    {
                        throw new ApplicationException("TCP CONNECTION DISCONNECTED");
                    }
                }
                ssl.Flush();

                byte[] buffer = new byte[2048];
                StringBuilder messageData = new StringBuilder();
                int bytes = -1;
                do
                {
                    bytes = ssl.Read(buffer, 0, buffer.Length);

                    Console.WriteLine($"bytes: {bytes}");

                    Decoder decoder = Encoding.UTF8.GetDecoder();
                    char[] chars = new char[decoder.GetCharCount(buffer, 0, bytes, true)];
                    Console.WriteLine($"array length: {chars.Length}");
                    decoder.GetChars(buffer, 0, bytes, chars, 0, true);
                    messageData.Append(chars);

                } while (bytes == 2048);
                
            }
            catch (Exception ex)
            {
                throw new ApplicationException(ex.Message);
            }
        }
    }
}
