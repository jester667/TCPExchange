using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClientTcpWinForm
{
    public partial class FormPicture : Form
    {
        public FormPicture()
        {
            InitializeComponent();
        }

        public FormPicture(string fullFileName)
        {
            InitializeComponent();
            pictureBox1.ImageLocation = fullFileName;
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox1.ClientSize = new Size(pictureBox1.Width, pictureBox1.Height);
            pictureBox1.Load();
        }

        private void FormPicture_Load(object sender, EventArgs e)
        {
            
        }
    }
}
