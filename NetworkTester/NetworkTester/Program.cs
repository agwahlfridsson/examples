using System.Net;
using System.Net.Sockets;
using System.Text;

namespace NetworkTester
{
    internal class Program
    {
        private enum appmode { client, server, unset };
        private enum tcpudp { tcp, udp, unset};

        private static int port;
        private static appmode mode = appmode.unset;
        private static tcpudp tech = tcpudp.unset;
        static void Main(string[] args)
        {
            /* 
             * --mode client
             * --port 8080
             * --ip 127.0.0.1
             * --type udp
             */


            Console.WriteLine(args.Length);
            if (args.Length == 7 &&
                args[0] == "--mode" &&
                args[1] == "client" &&
                args[2] == "--port" &&
                int.TryParse(args[3], out port) &&
                args[4] == "--ip") 
            {
                mode = appmode.client;
                if (args[4] == "tcp")
                    tech = tcpudp.tcp;
                else if (args[4] == "udp")
                    tech = tcpudp.udp;
            }
            else if (args.Length == 7 &&
                     args[0] == "--mode" &&
                     args[1] == "server" &&
                     args[2] == "--port" &&
                     int.TryParse(args[3], out port))
            { 
                mode = appmode.server;
            }

            if (mode == appmode.unset || tech == tcpudp.unset)
            {
                Console.WriteLine("error in mode or technique");
                return;
            }

            if (mode == appmode.client && tech == tcpudp.tcp)
            {
                SetupTcpClient();
            }
            else if (mode == appmode.client && tech == tcpudp.udp)
            {
                SetupUdpClient();
            }
            else if(mode == appmode.server && tech == tcpudp.tcp)
            {
                SetupTcpServer();
            }
            else if (mode == appmode.server && tech == tcpudp.udp)
            {
                SetupUdpServer();
            }
        }
        private static void SetupTcpClient()
        {
            string serverAddress = "127.0.0.1"; // Localhost for this example
            int port = 8080;

            try
            {
                // Create a TcpClient
                using (TcpClient client = new TcpClient(serverAddress, port))
                {
                    // Get the network stream for sending and receiving data
                    NetworkStream stream = client.GetStream();

                    // Convert a message to bytes and send it
                    string messageToSend = "Hello, Server!";
                    byte[] dataToSend = Encoding.ASCII.GetBytes(messageToSend);
                    stream.Write(dataToSend, 0, dataToSend.Length);
                    Console.WriteLine("Message sent to the server: " + messageToSend);

                    // Receive a response from the server
                    byte[] dataReceived = new byte[256];
                    int bytesRead = stream.Read(dataReceived, 0, dataReceived.Length);
                    string response = Encoding.ASCII.GetString(dataReceived, 0, bytesRead);
                    Console.WriteLine("Response from the server: " + response);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }
        private static void SetupUdpClient() 
        {
            using (UdpClient udpClient = new UdpClient())
            {
                // Set the server address and port
                string serverAddress = "127.0.0.1"; // Replace with the server IP
                int port = 8080; // Replace with the server port

                // Convert the message to bytes
                string message = "Hello, UDP Server!";
                byte[] sendBytes = Encoding.ASCII.GetBytes(message);

                // Send the message to the server
                udpClient.Send(sendBytes, sendBytes.Length, serverAddress, port);
                Console.WriteLine("Message sent: " + message);

                // Receive a response (blocking call, waits until data is received)
                IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, port);
                byte[] receivedBytes = udpClient.Receive(ref remoteEndPoint);
                string receivedMessage = Encoding.ASCII.GetString(receivedBytes);
                Console.WriteLine("Received response: " + receivedMessage);
            }
        }
        private static void SetupTcpServer()
        {  // Set up the server
            int port = 8080; // Define the port to listen on
            TcpListener tcpListener = new TcpListener(IPAddress.Any, port);
            tcpListener.Start();
            Console.WriteLine("Server started on port " + port);

            while (true)
            {
                // Wait for incoming client connection
                TcpClient tcpClient = tcpListener.AcceptTcpClient();
                Console.WriteLine("Client connected.");

                // Get the network stream for reading and writing
                NetworkStream networkStream = tcpClient.GetStream();

                // Read the data sent by the client
                byte[] receivedBytes = new byte[1024];
                int bytesRead = networkStream.Read(receivedBytes, 0, receivedBytes.Length);
                string receivedMessage = Encoding.ASCII.GetString(receivedBytes, 0, bytesRead);
                Console.WriteLine("Received message: " + receivedMessage);

                // Send a response back to the client
                string responseMessage = "Hello, TCP Client!";
                byte[] sendBytes = Encoding.ASCII.GetBytes(responseMessage);
                networkStream.Write(sendBytes, 0, sendBytes.Length);
                Console.WriteLine("Sent response: " + responseMessage);

                // Close the connection
                tcpClient.Close();
                Console.WriteLine("Client disconnected.");
            }

        }
        private static void SetupUdpServer()
        {
            // Set up the UDP listener on a specific port
            int port = 8080; // Define the port to listen on
            UdpClient udpServer = new UdpClient(port);
            Console.WriteLine("UDP Server started on port " + port);

            // Set up the end point for receiving data
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, port);

            while (true)
            {
                // Receive data from the client (blocking call)
                byte[] receivedBytes = udpServer.Receive(ref endPoint);
                string receivedMessage = Encoding.ASCII.GetString(receivedBytes);
                Console.WriteLine("Received message: " + receivedMessage);

                // Prepare a response
                string responseMessage = "Hello, UDP Client!";
                byte[] sendBytes = Encoding.ASCII.GetBytes(responseMessage);

                // Send a response back to the client
                udpServer.Send(sendBytes, sendBytes.Length, endPoint);
                Console.WriteLine("Sent response: " + responseMessage);
            }
        }
    }
}
