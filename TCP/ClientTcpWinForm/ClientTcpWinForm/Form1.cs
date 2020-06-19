using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.IO;

namespace ClientTcpWinForm
{
    public partial class Form1 : Form
    {
        string ip;
        int port;
        IPEndPoint tcpEndPoint;
        Socket tcpSocket;
        string[] allFormatImage;
        public Form1()
        {
            InitializeComponent();
            InitializePerem();
        }
        private void InitializePerem()
        {
            allFormatImage = new string[] { ".jpeg", ".jpg", ".jpe", ".jfif",".png", "ico"};
        }
        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            var message = "Check";

            var data = Encoding.UTF8.GetBytes(message);

            if (!CreateSocket())
                return;

            if (!SendMessage(data))
                return;

            var answer = ReceiveCallBack();

            CheckAnswer(answer.ToString());
        }



        private void mainTable_DoubleClick(object sender, EventArgs e)
        {
            var answerQuestion = MessageBox.Show("Open file?", "", MessageBoxButtons.YesNo);
            if (answerQuestion != DialogResult.Yes)
                return;
            //var check = true;
            var selectedRowIndex = mainTable.CurrentCell.RowIndex;
            var selectedRow = mainTable.Rows[selectedRowIndex].Cells;
            var filename    = selectedRow[0].FormattedValue.ToString();
            var formatFile  = selectedRow[1].FormattedValue.ToString();
            int dataSize    = Convert.ToInt32(selectedRow[2].FormattedValue);

            if (allFormatImage.Contains(formatFile.ToLower()))
            {
                var message = "GetFile" + textBoxDirectory.Text+"\\"+filename+formatFile;
                var data = Encoding.UTF8.GetBytes(message);

                if (!CreateSocket())
                    return;
                if (!SendMessage(data))
                    return;

                ReceiveFileCallBack(filename + formatFile, dataSize);
                OpenImage(filename + formatFile);
                //File.WriteAllBytes(filename + formatFile, answer);
                //CheckAnswer(strAnswer, false);
            }
            else
            {
                MessageBox.Show("Данное приложение предназначено для открытия графических файлов");
            }
        }

        private void Update_Click(object sender, EventArgs e)
        {
            var message = "GetDirectory" + textBoxDirectory.Text;
            var data = Encoding.UTF8.GetBytes(message);
            int dataSize = 32768;

            if (!CreateSocket())
                return;
            if (!SendMessage(data))
                return;

            var answer = ReceiveCallBack(dataSize);
            var strAnswer = answer.ToString();
            CheckAnswer(strAnswer, false);
        }

        #region function
        private bool CreateSocket()
        {
            ip = textBoxIP.Text;
            try
            {

                port = Convert.ToInt32(textBoxPort.Text);
            }
            catch
            {
                MessageBox.Show("Incorrect format port");
                return false;
            }
            try
            {
                tcpEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
            try
            {
                tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
            return true;
        }
        private void CheckAnswer(string answer, bool showMessage = true)
        {
            if(answer.Contains("Directory exist"))
            {
                BtnUpdate.Enabled = true;
            }
            else if(answer.Contains("Succes"))
            {
                ChangeAvailability(true);
            }
            else if(answer.Contains("GetDirectory"))
            {
                UpdateMainTable(answer.Replace("GetDirectory",""));
            }

            if(showMessage || answer.Contains("Error"))
                MessageBox.Show(answer);
        }
       
        private void UpdateMainTable(string answer)
        {
            var arrAnswer = Regex.Split(answer, "#row#");
           
            int indexFormat;
            string strTemp;
            string[] row = new string[3];

            foreach (var str in arrAnswer)
            {
                if (String.IsNullOrWhiteSpace(str))
                    continue;
                indexFormat = str.LastIndexOf('.');
                row[2] = str.Substring(indexFormat+1);
                strTemp = str.Replace('.'+row[2], "");
                indexFormat = strTemp.LastIndexOf('.');
                row[1] = strTemp.Substring(indexFormat);
                row[0] = strTemp.Substring(0, indexFormat);
                mainTable.Rows.Add(row);
            }

        }
        private void ChangeAvailability(bool isConnection)
        {
            textBoxIP.Enabled           = !isConnection;
            textBoxPort.Enabled         = !isConnection;
            BtnSetDirectory.Enabled     = isConnection;
            textBoxDirectory.Enabled    = isConnection;
            mainTable.Enabled           = isConnection;
        }

        private void ReceiveFileCallBack(string package_name, int buferSize = 256)
        {

            MemoryStream memstr = new MemoryStream();
            FileStream fs = File.Create(package_name);
            var buffer = new byte[UInt16.MaxValue];
            var count = 0;
            do
            {
                count+= buffer.Length;
                tcpSocket.Receive(buffer);
                memstr.Write(buffer, 0, buffer.Length);
            }
            while (buferSize > count);
            memstr.WriteTo(fs);
            fs.Close();
            memstr.Close();

        }
        private StringBuilder ReceiveCallBack(int buferSize = 256)
        {
            var buffer = new byte[buferSize];
            var size = 0;
            var answer = new StringBuilder();

            do
            {
                size = tcpSocket.Receive(buffer);
                answer.Append(Encoding.UTF8.GetString(buffer, 0, size));
            }
            while (tcpSocket.Available > 0);

            tcpSocket.Shutdown(SocketShutdown.Both);
            tcpSocket.Close();

            return answer;
        }

        private static void OpenImage(string fileFullName)
        {
            var pictureForm = new FormPicture(fileFullName);
            pictureForm.Show();
        }
        #endregion

        private bool SendMessage(byte[] message)
        {
            if (!tcpSocket.Connected)
            {
                try
                {
                    tcpSocket.Connect(tcpEndPoint);
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return false;
                }

            } 
            tcpSocket.Send(message);
            return true;
        }

        private void BtnDisconnect_Click(object sender, EventArgs e)
        {
            //tcpSocket.Shutdown(SocketShutdown.Both);
            //tcpSocket.Close();
            ChangeAvailability(false);
            BtnUpdate.Enabled = false;
        }

        private void BtnSetDirectory_Click(object sender, EventArgs e)
        {
            var message = "CheckDirectory" + textBoxDirectory.Text;
            var data = Encoding.UTF8.GetBytes(message);

            if (!CreateSocket())
                return;
            if (!SendMessage(data))
                return;

            var answer    = ReceiveCallBack();
            var strAnswer = answer.ToString();

            CheckAnswer(strAnswer);
        }

        private void textBoxDirectory_TextChanged(object sender, EventArgs e)
        {
            if (BtnUpdate.Enabled)
            {
                BtnUpdate.Enabled = false;
            }
        }
    }
}
