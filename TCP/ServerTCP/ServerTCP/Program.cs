using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
 
namespace ServerTCP
{
    class Program
    {
        static void Main(string[] args)
        {
            const string ip = "127.0.0.1";
            const int port = 8080;

            var tcpEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);

            var tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            tcpSocket.Bind(tcpEndPoint);
            tcpSocket.Listen(5);
            Console.WriteLine("Сервер запущен. Ожидание подключений...");

            while (true)
            {
                var listner = tcpSocket.Accept();
                var buffer = new byte[256];
                var size = 0;
                var data = new StringBuilder();

                do
                {
                    size = listner.Receive(buffer);
                    data.Append(Encoding.UTF8.GetString(buffer, 0, size));
                }
                while (listner.Available > 0);

                Console.WriteLine(data);

                SendCallBack(data.ToString(), listner);

                //listner.Send(Encoding.UTF8.GetBytes("Succes"));

                listner.Shutdown(SocketShutdown.Both);
                listner.Close();
            }


        }

        private static void SendCallBack(string request, Socket listner)
        {
            if (request.Equals("Check"))
            {
                listner.Send(GetBytesMessage("Succes"));
            }
            else if (request.Contains("CheckDirectory"))
            {
                CheckDirectory(request, listner);
            }
            else if (request.Contains("GetDirectory"))
            {
                GetDirectory(request, listner);
            }
            else if (request.Contains("GetFile"))
            {
                GetFile(request, listner);
            }

        }

        private static void GetFile(string request, Socket listner)
        {
            var fileFullName = request.Replace("GetFile", "").Trim();
            var messageError = "Error: file not exist";
            
            if (File.Exists(fileFullName))
            {
                byte[] arrFile = File.ReadAllBytes(fileFullName);
                var tempArr = new byte[UInt16.MaxValue];
                var lenght  = 0;
                for (int i = 0; i<arrFile.Length; i += UInt16.MaxValue)
                {
                    lenght = (arrFile.Length-i>= UInt16.MaxValue) ?  UInt16.MaxValue : arrFile.Length - i;
                    Array.Copy(arrFile,i, tempArr,0, lenght);
                    listner.Send(tempArr, SocketFlags.Partial);
                }
            }
            else
                listner.Send(GetBytesMessage(messageError));          
        }
        private static void GetDirectory(string request, Socket listner)
        {
            var path      = request.Replace("GetDirectory", "").Trim();
            var arrFilesName = Directory.GetFiles(path);
            for (int i = 0; i < arrFilesName.Length; i++)
            {
                arrFilesName[i] += '.'+(new FileInfo(arrFilesName[i])).Length.ToString();
            }
   
            var strFilesName = String.Concat(arrFilesName);
            var subString    = Path.GetFullPath(path)+"\\";
            
            strFilesName = strFilesName.Replace(subString, "#row#");
            listner.Send(GetBytesMessage("GetDirectory"+strFilesName));
        }

        private static void CheckDirectory(string request, Socket listner)
        {
            var path = request.Replace("CheckDirectory", "").Trim();

            if (Directory.Exists(path))
            {
                listner.Send(GetBytesMessage("Directory exist"));
            }
            else
            {
                listner.Send(GetBytesMessage("Error: directory not exist"));
            }
        }
        private static byte[] GetBytesMessage<T>( T str)
        {
            return Encoding.UTF8.GetBytes(str.ToString());
        }
    }
}
