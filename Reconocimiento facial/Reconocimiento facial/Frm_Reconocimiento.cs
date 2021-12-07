using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using System.Data.OleDb;
using System.Speech.Synthesis;
using System.Media;
using System.Runtime.InteropServices;

namespace Reconocimiento_facial
{
    public partial class Frm_Reconocimiento : Form
    {
        public int heigth, width;
        public string[] Labels;
        DBCon dbc = new DBCon();
        int con = 0;
        SoundPlayer media = new SoundPlayer();
        SpeechSynthesizer vos = new SpeechSynthesizer();
        Image<Bgr, Byte> currentFrame;
        Capture grabber;
        HaarCascade face;
        HaarCascade eye;
        MCvFont font = new MCvFont(FONT.CV_FONT_HERSHEY_TRIPLEX, 0.4d, 0.4d);
        Image<Gray, byte> result, TrainedFace = null;
        Image<Gray, byte> gray = null;
        List<Image<Gray, byte>> trainingImages = new List<Image<Gray, byte>>();
        List<string> labels = new List<string>();
        List<string> NamePersons = new List<string>();
        int ContTrain, NumLabels, t;
        string name, Labelsinfo, names = null;



        public Frm_Reconocimiento()
        {
            InitializeComponent();
            heigth = this.Height; width = this.Width;
             
            face = new HaarCascade("haarcascade_frontalface_default.xml");
            try
            {
                dbc.ObtenerBytesImagen();
                               
                string[] Labels = dbc.Name;
                NumLabels = dbc.TotalUser;
                ContTrain = NumLabels;
                string LoadFaces;

                for (int tf = 0; tf < NumLabels; tf++)
                {
                    con = tf;
                    Bitmap bmp = new Bitmap(dbc.ConvertByteToImg(con));
                    trainingImages.Add(new Image<Gray, byte>(bmp));
                    labels.Add(Labels[tf]);
                }

            }
            catch (Exception e)
            {
                MessageBox.Show(e + "No hay ningun rosto registrado).", "Cargar rostros", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void Reconocer()
        {
            try
            {
               
                grabber = new Capture();
                grabber.QueryFrame();
                Application.Idle += new EventHandler(FrameGrabber);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void FrameGrabber(object sender, EventArgs e)
        {
            lblNumeroDetect.Text = "0";
            NamePersons.Add("");
            try
            {
                currentFrame = grabber.QueryFrame().Resize(320, 240, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
                gray = currentFrame.Convert<Gray, Byte>();
                MCvAvgComp[][] facesDetected = gray.DetectHaarCascade(face, 1.2, 10, Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING, new Size(20, 20));
                foreach (MCvAvgComp f in facesDetected[0])
                {
                    t = t + 1;
                    result = currentFrame.Copy(f.rect).Convert<Gray, byte>().Resize(100, 100, INTER.CV_INTER_CUBIC);
                    currentFrame.Draw(f.rect, new Bgr(Color.Red), 3);

                    if (trainingImages.ToArray().Length != 0)
                    {
                        MCvTermCriteria termCrit = new MCvTermCriteria(ContTrain, 0.001);

                        EigenObjectRecognizer recognizer = new EigenObjectRecognizer(trainingImages.ToArray(), labels.ToArray(), ref termCrit);
                        var fa = new Image<Gray, byte>[trainingImages.Count]; 
                        name = recognizer.Recognize(result);
                        currentFrame.Draw(name, ref font, new Point(f.rect.X - 2, f.rect.Y - 2), new Bgr(Color.Red));
                    }

                    NamePersons[t - 1] = name;
                    NamePersons.Add("");
                    lblNumeroDetect.Text = facesDetected[0].Length.ToString();
                    lblNadie.Text = name;

                }
                t = 0;

                for (int nnn = 0; nnn < facesDetected[0].Length; nnn++)
                {
                    names = names + NamePersons[nnn] + ", ";
                }


                imageBoxFrameGrabber.Image = currentFrame;
                name = "";
        
                NamePersons.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void lblNadie_Click(object sender, EventArgs e)
        {

        }

        private void lblNumeroDetect_Click(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void imageBoxFrameGrabber_Click(object sender, EventArgs e)
        {

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void Frm_Reconocimiento_Load(object sender, EventArgs e)
        {
            Reconocer();
        }

        private void btn_Salir_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btn_mini_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }
        private void btn_Registrar_Click(object sender, EventArgs e)
        {
           try
            {
                Desconectar();
                Frm_Registrar re = new Frm_Registrar();
                Frm_Filtro fil = new Frm_Filtro();
                fil.Show();
                re.ShowDialog();
                fil.Hide();
            }
            catch
            {

            }
        
        }

        private void button4_Click(object sender, EventArgs e)
        {
            switch (button4.Text)
            {
                case "Conectar":
                    Reconocer();
                    button4.Text = "Desconectar";
                    button4.BackgroundStyle.SolidColor = Color.GreenYellow;
                    button4.BorderStyle.SolidColor = Color.GreenYellow;
                    button4.StateStyles.HoverStyle.BackgroundSolidColor = Color.GreenYellow;
                    button4.StateStyles.HoverStyle.BorderSolidColor = Color.GreenYellow;

                    break;
                case "Desconectar":
                    Desconectar();
                    break;
            }
        }
        private void Desconectar()
        {
            Application.Idle -= new EventHandler(FrameGrabber);
            grabber.Dispose();
            imageBoxFrameGrabber.ImageLocation = "img/1.jpg";
            lblNadie.Text = string.Empty;
            lblNumeroDetect.Text = string.Empty;
            button4.Text = "Conectar";
            button4.BackgroundStyle.SolidColor = Color.FromArgb(192, 0, 0);
            button4.BorderStyle.SolidColor= Color.FromArgb(192, 0, 0);
            button4.StateStyles.HoverStyle.BackgroundSolidColor = Color.FromArgb(192, 0, 0);
            button4.StateStyles.HoverStyle.BorderSolidColor = Color.FromArgb(192, 0, 0);
        }
    }
}
