using System;

using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using MetroFramework.Forms;
using MySql.Data.MySqlClient.Memcached;
using ProjectoXappia.Properties;

namespace ProjectoXappia
{
    public partial class UIPanel : MetroForm
    {

        private Timer timer;
        private bool unt = false;
        public UIPanel()
        {
            InitializeComponent();
            this.Icon = Resources.openpark;


            this.pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            this.pictureBox1.Image = Resources.Bienvenida;
            var pos = this.PointToScreen(label1.Location);
            pos = pictureBox1.PointToClient(pos);
            label1.Parent = pictureBox1;
            label1.Location = pos;
            label1.BackColor = Color.Transparent;
            label1.AutoSize = true;
            timer = new Timer { Interval = 1000 }; // todo: verificar cuanto es medio minuto
            timer.Tick += timer_Tick;
        }

        protected override sealed void OnResize(EventArgs e)

        {
            if (this.WindowState == FormWindowState.Maximized)
            {
                var prev = pictureBox1.Size.Height;
                pictureBox1.Size = new Size(pictureBox1.Size.Width + 150, prev);
                this.pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                unt = true;

            }
            else if (unt)
            {
                var prev = pictureBox1.Size.Height;
                pictureBox1.Size = new Size(pictureBox1.Size.Width - 150, prev);
                this.pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                unt = false;
            }
            base.OnResize(e);
        }


        public void setInfo(bool isFromDB, DataRow cliente = null)
        {
            if (!isFromDB && cliente == null)
            {
                this.pictureBox1.Image = Resources.ErrorRecepcion;
                timer.Start();
            }

            else if (!isFromDB)
            {

                loadController("Registrando a :",
                    cliente[Settings.Default.ColumnaNombre].ToString(),
                    cliente[Settings.Default.ColumnaDNI].ToString());


            }
            else
            {
                loadController("Bienvenido :",
                    cliente[Settings.Default.ColumnaNombre].ToString(),
                    cliente[Settings.Default.ColumnaDNI].ToString());
                timer.Start();

            }

        }

        public Image byteArrayToImage(byte[] byteArrayIn)
        {
            MemoryStream ms = new MemoryStream(byteArrayIn);
            Image returnImage = Image.FromStream(ms);
            return returnImage;
        }

        public void clearInfo()
        {
            loadController(string.Empty, string.Empty, string.Empty);
            this.pictureBox1.Image = Resources.Bienvenida;

        }

        public void loadController(string Label1, string Label2, string Label3)
        {
            label1.Text = Label1 + Label2;

        }



        private void timer_Tick(object sender, EventArgs e)
        {
            timer.Stop();
            clearInfo();
        }

        public void notEnoughData()
        {
            this.pictureBox1.Image = Resources.ErrorHuella;
            timer.Start();

        }

        public void clearLabel3()
        {
        }
        bool flag = false;
        private void UIPanel_MouseDown(object sender, MouseEventArgs e)
        { 
            flag = true;
        } 
        private void UIPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (flag == true)
            {
                this.Location = Cursor.Position; 
            }
        } 
        private void UIPanel_MouseUp(object sender, MouseEventArgs e)
        {
            flag = false;
        }
    

    }
}
