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
        public UIPanel()
        {
            InitializeComponent();
            this.Icon = Resources.openpark;


            this.pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            this.pictureBox1.Image = Resources.banner;
            label1.AutoSize = true;
            label2.AutoSize = true;
            label3.AutoSize=true;
            timer = new Timer { Interval = 1000 }; // todo: verificar cuanto es medio minuto
            timer.Tick += timer_Tick;

        }


        public void setInfo(bool isFromDB, DataRow cliente = null)
        {
            if (!isFromDB && cliente == null)
            {
                label1.ForeColor = Color.DarkRed;

                loadController(string.Empty, Settings.Default.usuarioInexistente,
                    Properties.Resources.TextoInvalido);
                timer.Start();
            }

            else if (!isFromDB)
            {

                label1.ForeColor = Color.DarkGreen;
                loadController("Registrando a :",
                    cliente[Settings.Default.ColumnaNombre].ToString(),
                    cliente[Settings.Default.ColumnaDNI].ToString());


            }
            else
            {
                label1.ForeColor = Color.DarkGreen;
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
        }

        public void loadController(string Label1, string Label2, string Label3)
        {
            label1.Text = Label1;

            label2.Text = Label2;
        }



        private void timer_Tick(object sender, EventArgs e)
        {
            timer.Stop();
            clearInfo();
        }

        public void notEnoughData()
        {
            label3.ForeColor = Color.DarkRed;

            label3.Text = "No se pudo leer la huella, por favor intentá nuevamente.";
        }

        public void clearLabel3()
        {
            label3.Text = string.Empty;
        }

    }
}
