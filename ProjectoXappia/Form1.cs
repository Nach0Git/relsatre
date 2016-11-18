using System;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using MetroFramework.Forms;
using ProjectoXappia.Properties;
using RestSharp;
using RestSharp.Deserializers;
using ZKFPEngXControl;
using ProjectoXappia.Class;

namespace ProjectoXappia
{
    public partial class Form1 : MetroForm
    {

        private UIPanel UIPanel;
        private string errorMsg = "";
        private ZKFPEngX ZKEngine;
        private DataBase DB;
        private DataSet dbClients;
        private DataSet EnrollClient;
        private List<byte[]> huellas = new List<byte[]>();
        private bool lastSucces = true;
        private Timer timer;
        private SerialPort portCom1;
        private OAuthTokenResponse oAuthToken;
        public Form1()
        {
            InitializeComponent();
            ZKEngine = new ZKFPEngX();
            DB = new DataBase();
            dbClients = new DataSet();
            EnrollClient = new DataSet();
            //salesforceCall("AYIHDBJKLQWD","38573102",0);
            label2.AutoSize = true;

            ZKEngine.OnCapture += ZKEngine_OnCapture;
            ZKEngine.OnImageReceived += ZKEngine_OnImageReceived;
            ZKEngine.OnEnroll += ZKEngine_OnEnroll;
            ZKEngine.OnFeatureInfo += ZKEngine_OnFeatureInfo;
            label2.TextChanged += label2_TextChanged;
            initEngine();


        }

        void label2_TextChanged(object sender, EventArgs e)
        {
            base.OnTextChanged(e);
            Size sz = new Size(label2.Width, Int32.MaxValue);
            sz = TextRenderer.MeasureText(label2.Text, label2.Font, sz, TextFormatFlags.WordBreak);
            label2.Width = sz.Width;
            label2.Height = sz.Height;
        }


        void ZKEngine_OnFeatureInfo(int AQuality)
        {


            setqualitybar(ZKEngine.LastQuality);
            switch (AQuality)
            {
                case 0:
                    lastSucces = true;
                    if (ZKEngine.IsRegister)
                    {
                        label1.Text = "Quedan " + (ZKEngine.EnrollIndex - 1) + " verificaciones dactilares más";

                    }
                    break;
                case 1:
                    lastSucces = false;
                    break;
            }



        }

        void ZKEngine_OnEnroll(bool ActionResult, object ATemplate)
        {
            if (ActionResult)
            {
                var encodeFingerPrint = ZKEngine.GetTemplateAsString();
                if (salesforceCall(encodeFingerPrint, EnrollClient.Tables[0].Rows[0][Settings.Default.ColumnaDNI].ToString(), 0))
                {
                    if (!DB.saveTemplate(encodeFingerPrint, EnrollClient.Tables[0].Rows[0]))
                    {
                        label2.Text = Settings.Default.localDataBaseError;
                    }

                    dbClients.Clear();
                    UIPanel.clearInfo();
                    DB.getAllFingerPrint(dbClients);
                    EnrollClient.Clear();
                    label1.Text = "";
                    label2.ForeColor=Color.Green;
                    label2.Text=Settings.Default.enrollSucces;
                }
                else
                {
                    label2.ForeColor = Color.DarkRed;

                    label2.Text="Hubo un error:" + Environment.NewLine + errorMsg;
                    errorMsg = "";

                }
                ZKEngine.BeginCapture();


            }
        }

        void ZKEngine_OnImageReceived(ref bool AImageValid)
        {
            if (AImageValid)
            {
                ZKEngine.PrintImageAt((int)pictureBox1.CreateGraphics().GetHdc(), 0, 0, pictureBox1.Height, pictureBox1.Width);

            }
        }

        void ZKEngine_OnCapture(bool ActionResult, object ATemplate)
        {
            if (ActionResult)
            {
                if (!ZKEngine.IsRegister && dbClients.Tables[0].Rows.Count != 0)
                {
                    var found = false;
                    foreach (DataRow userRow in dbClients.Tables[0].Rows)
                    {
                        string Oldtemplate = userRow["Huella"].ToString();

                        if (DBNull.Value.Equals(Oldtemplate)) continue;
                        bool compareresult = ZKEngine.VerFingerFromStr(ref Oldtemplate, ZKEngine.GetTemplateAsString(), true, true);
                        if (!compareresult) continue;

                        UIPanel.setInfo(true, userRow);
                        openTurnstile(Settings.Default.HexaAbrirMolinete);
                        found = true;
                        break;
                    }
                    if (!found)
                    {
                        openTurnstile(Settings.Default.hexaAvvesoRechazado);
                        
                        UIPanel.setInfo(false);
                    }

                }



            }


        }

        private void button4_Click(object sender, EventArgs e)
        {
            int parsedDni;
            if (textBox1.Text != "" && !textBox1.Text.Contains(".") && int.TryParse(textBox1.Text, out parsedDni))
            {
                DB.getClientByDNI(textBox1.Text, EnrollClient);
                if (EnrollClient.Tables[0].Rows.Count != 0)
                {
                    button5.Enabled = true;
                    UIPanel.setInfo(false, EnrollClient.Tables[0].Rows[0]);

                }
                else
                {
                    MessageBox.Show("No existe el dni en la Base");

                }

            }
            else
            {
                MessageBox.Show("Ingresar un DNI valido");

            }
        }
        private void initEngine()
        {
            // init engine properties and quality
            //ZKEngine.FPEngineVersion = "10";
            label2.ForeColor = Color.DarkRed; ;

            var result = ZKEngine.InitEngine();
            ZKEngine.LowestQuality = 75;
            checkForDependencies();
            switch (result)
            {
                case 0:
                    ZKEngine.BeginCapture();
                    //ZKEngine.CreateFPCacheDB();
                    if (errorMsg != "")
                    {
                        label2.Text += errorMsg;

                    }
                    else
                    {
                        label2.Text = "Todo okey.";
                        label2.ForeColor = Color.DarkGreen; ;

                    }
                    UIPanel = new UIPanel();
                    UIPanel.Show();
                    break;
                case 1:
                    label2.Text += Settings.Default.driverNotFound + Environment.NewLine;
                    break;
                case 2:
                    label2.Text += Settings.Default.fingerPrintNotFound + Environment.NewLine;
                    break;
                case 3:
                    label2.Text += "Si esta linea se lee, algo salio muy mal con el SDK fingerprint." + Environment.NewLine;
                    MessageBox.Show(errorMsg); //nunca debería pasar
                    break;
            }

        }

        private void checkForDependencies()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11;

            var ports = SerialPort.GetPortNames();
            if (!ports.Contains("COM1"))
            {
                label2.Text += Settings.Default.portNotFound + Environment.NewLine;
            }
            else
            {
                portCom1 = new SerialPort("COM1", Settings.Default.numberPort, Parity.None, 8, StopBits.One);
            }
            if (!DB.ConnectWithReturnvalue())
            {
                label2.Text += Settings.Default.DBError + Environment.NewLine;
            }
            else
            {
                DB.getAllFingerPrint(dbClients);
            }
            label1.Text = "Autenticando con Salesforce...";
            oAuthToken = getToken();
            label1.Text = "";

        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (EnrollClient.Tables[0].Rows.Count != 0)
            {
                button5.Enabled = false;
                ZKEngine.BeginEnroll();
                ZKEngine.BeginCapture();
            }

        }

        private void setqualitybar(int quality)
        {
            progressBar1.Value = ZKEngine.LastQuality;
            label1.Text = "";
            if (quality < 75 && lastSucces)
            {
                ModifyProgressBarColor.SetState(progressBar1, 3);

                label2.ForeColor = Color.DarkRed;
                label2.Text = "No se pudo tomar informacion suficiente, reintente la acción";

            }
            else if (quality > 75 && !lastSucces)
            {
                ModifyProgressBarColor.SetState(progressBar1, 1);
                label2.Text = "";
                label2.ForeColor = Color.DarkRed;


            }



        }

        private void openTurnstile(string hexa)
        {
            if (portCom1 != null)
            {
                try
                {
                    byte[] bytes =
                        hexa.Split(' ').Select(s => Convert.ToByte(s, 16)).ToArray();
                    portCom1.Open();
                    portCom1.Handshake = Handshake.None;
                    portCom1.RtsEnable = true;
                    portCom1.Write(bytes, 0, bytes.Length);
                    portCom1.Close();
                }
                catch (Exception)
                {
                    label2.ForeColor = Color.DarkRed;
                    label2.Text = "hubo un error al intentar abrir el molinete, asegurese de que el puerto serie este bien conectado.";

                }

            }
            else
            {
                label2.ForeColor = Color.DarkRed;
                label2.Text = Settings.Default.portNotFound;
            }
        }

        private OAuthTokenResponse getToken()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11;

            var restClient = new RestClient(Settings.Default.salesforceUrlToken);
            OAuthTokenResponse token = new OAuthTokenResponse();
            var request = new RestRequest(Settings.Default.dataSalesforceWsAuthentication, Method.POST);
            request.AddHeader("content-type", "application/x-www-form-urlencoded");
            var response = restClient.Execute(request);
            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                    label2.ForeColor = Color.DarkRed;
                    label2.Text ="Bad Request on token authentication : " + response.Content.Trim('/');

            }
            JsonDeserializer deserial = new JsonDeserializer();
            try
            {
                deserial.Deserialize<OAuthTokenResponse>(response);
            }
            catch (Exception)
            {

            }
            return deserial.Deserialize<OAuthTokenResponse>(response);

        }

        public IRestResponse sendData(string fingerprint, string dni)
        {
            if (oAuthToken == null)
            {
                oAuthToken = getToken();

            }
            var Client = new RestClient(oAuthToken.instance_url);
            var request = new RestRequest("/services/apexrest/FingerprintService", Method.POST);
            request.AddHeader("Authorization", oAuthToken.token_type + " " + oAuthToken.access_token);
            request.AddHeader("content-type", "application/json");
            request.AddHeader("accept", "application/json");
            var jsonData = new wsObject()
            {
                docNum = dni,
                fingerprint = fingerprint
            };
            request.AddJsonBody(jsonData);

            return Client.Execute(request);
        }

        private bool salesforceCall(string fingerprint, string dni, int intent)
        {
            if (intent == 3)
                errorMsg += Settings.Default.salesforceError + Environment.NewLine;


            var result = sendData(fingerprint, dni);
            switch (result.StatusCode)
            {
                case HttpStatusCode.OK:
                    if (result.Content.Trim('"').Equals("OK")) return true;
                    errorMsg += result.Content.Trim('/') + Environment.NewLine;
                    break;
                case HttpStatusCode.Unauthorized:
                    oAuthToken = getToken();
                    return salesforceCall(fingerprint, dni, intent++);
                case HttpStatusCode.BadRequest:
                    errorMsg += "Bad Request : " + result.Content.Trim('/') + Environment.NewLine;
                    break;
            }
            return false;

            //Todo: Legacy way of doing the service call, restsharFTW
            //System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11;
            //var c = new wsObject()
            //{
            //    docNum = "38555777",
            //    fingerprint = "ABUHIJNKJDBYUHJNAKOJISUHIBJNMKJ"
            //};
            //using (var client = new WebClient())
            //{
            //client.Headers[HttpRequestHeader.ContentType] = "application/json";
            //client.Headers.Add("Authorization", "Bearer" + " " + "fruta" + "sdfsgag");
            //try
            //{
            //    var responseDa = client.UploadString("Settings.Default.salesforceUrlToken", "asfsd");

            //}
            //catch (Exception p)
            //{

            //    var r = p;
            //}
            //byte[] postData = Encoding.UTF8.GetBytes(string.Empty);
            ////client.
            //var responseData = client.UploadData(Settings.Default.salesforceUrlToken+Settings.Default.dataSalesforceWsAuthentication, "POST", postData);
            //if (responseData != null)
            //{
            //    var asd = new OAuthTokenResponse();
            //    asd = JsonConvert.DeserializeObject<OAuthTokenResponse>(Encoding.UTF8.GetString(responseData));
            //    var j = JsonConvert.SerializeObject(c);
            //    client.Headers[HttpRequestHeader.ContentType] = "application/json";
            //    client.Headers.Add("Authorization", asd.token_type + " " + "fruta"+asd.access_token);
            //    try
            //    {
            //        var responseDa = client.UploadString(asd.instance_url + "/services/apexrest/FingerprintService", j);

            //    }
            //    catch (Exception p)
            //    {

            //        var r = p;
            //    }

            //}

        }
        [Serializable]
        public class OAuthTokenResponse
        {
            public string access_token { get; set; }
            public string instance_url { get; set; }
            public string id { get; set; }
            public string token_type { get; set; }
            public string issued_at { get; set; }
            public string signature { get; set; }
        }

        [Serializable]
        public class wsObject
        {
            public string docNum { get; set; }
            public string fingerprint { get; set; }
        }



    }


    public static class ModifyProgressBarColor
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr w, IntPtr l);
        public static void SetState(this ProgressBar pBar, int state)
        {
            SendMessage(pBar.Handle, 1040, (IntPtr)state, IntPtr.Zero);
        }
    }
}
