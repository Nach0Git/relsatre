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
            timer = new Timer { Interval = 1000 }; // todo: verificar cuanto es medio minuto
            timer.Tick += timer_Tick;
        }

        public void setInfo(bool isFromDB, DataRow cliente = null)
        {
            if (!isFromDB && cliente == null)
            {

                loadController("Bienvenido :", Properties.Resources.TextoInvalido,
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

                        loadController("Bienvenido :", cliente["Apellido"].ToString(), cliente["DNI"].ToString());
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
            label1.TextChanged += label1_TextChanged;
            //label2.TextChanged += label1_TextChanged;
        }

        void label1_TextChanged(object sender, EventArgs e)
        {
            base.OnTextChanged(e);
            resizeLabel();

        }
        private void resizeLabel()
        {

            Size sz = new Size(label2.Width, Int32.MaxValue);
            sz = TextRenderer.MeasureText(label2.Text, label2.Font, sz, TextFormatFlags.WordBreak);
            label2.Width = sz.Width;
            //this.Font = sz.Height;

        }
        private void timer_Tick(object sender, EventArgs e)
        {
            timer.Stop();
            clearInfo();
        }


    }
}
