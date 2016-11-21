using System;

using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using MetroFramework.Forms;
using ProjectoXappia.Properties;

namespace ProjectoXappia
{
    public partial class UIPanel : MetroForm
    {

        private Timer timer;
        public UIPanel()
        {
            InitializeComponent();
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

                loadController(string.Empty,Settings.Default.usuarioInexistente,
                    Properties.Resources.TextoInvalido);
                timer.Start();
            }
            else if (!isFromDB)
            {
                loadController("Bienvenido :",
                            cliente[Settings.Default.ColumnaApellido].ToString(),
                            cliente[Settings.Default.ColumnaDNI].ToString());

            }

            else
            {
                try
                {
                    if ((bool)cliente[Settings.Default.ColumnaPuedepasar])
                    {
                        label1.ForeColor = Color.DarkGreen;
                        loadController("Bienvenido :",
                            cliente[Settings.Default.ColumnaApellido].ToString(),
                            cliente[Settings.Default.ColumnaDNI].ToString());
                    }
                    else
                    {

                        label1.ForeColor = Color.DarkRed;

                        loadController("Bienvenido :", cliente[Settings.Default.ColumnaApellido].ToString(), cliente[Settings.Default.ColumnaDNI].ToString());
                    }

                    timer.Start();
                }
                catch (System.ArgumentException)
                {
                    MessageBox.Show("Estamos queriendo acceder a una columna que no existe");
                }
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
            //label2.TextChanged += label1_TextChanged;
        }



        private void timer_Tick(object sender, EventArgs e)
        {
            timer.Stop();
            clearInfo();
        }

        public void notEnoughData()
        {
            label3.ForeColor = Color.DarkRed;

            label3.Text="No se pudo tomar informacion suficiente, reintente la acción";
        }

        public void clearLabel3()
        {
            label3.Text = string.Empty;
        }

    }
}
