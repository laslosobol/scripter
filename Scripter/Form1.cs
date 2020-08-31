using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.OCR;
using Emgu.Util;
using Emgu.CV.Structure;
using OpenTK.Graphics.OpenGL;

namespace Scripter
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var request = WebRequest.Create("http://185.80.129.249:4222/getImage");
            var response = request.GetResponse();
            var stream = response.GetResponseStream();
            pictureBox1.Image = Image.FromStream(stream);
            pictureBox2.Image = Image.FromFile(@"F:\Working Pack\c#\Scripter\Scripter\Templates\Customise.png");
            pictureBox3.Image = Image.FromFile(@"F:\Working Pack\c#\Scripter\Scripter\Templates\NewItem.png");
            var img = new Bitmap(pictureBox1.Image)
                .ToImage<Gray, byte>();
            var customiseTemplate = new Bitmap(pictureBox2.Image)
                .ToImage<Gray, byte>();
            var newItemTemplate = new Bitmap(pictureBox3.Image)
                .ToImage<Gray, byte>();

            Mat imgOut = new Mat();
            if (DetectObject(img, customiseTemplate))
            {
                richTextBox1.AppendText("Customise\n");
            }

            else if (DetectObject(img, newItemTemplate))
            {
                richTextBox1.AppendText("NewItem\n");
                if (DetectObject(img, new Bitmap(@"F:\Working Pack\c#\Scripter\Scripter\Templates\SendTo.png")
                    .ToImage<Gray, byte>()))
                {
                    richTextBox1.AppendText("Send to transfer list: 1 move down!\n\n");
                }
                else if (DetectObject(img, new Bitmap(@"F:\Working Pack\c#\Scripter\Scripter\Templates\QuickSell.png")
                    .ToImage<Gray, byte>()))
                {
                    richTextBox1.AppendText("Quick sell now: 1 move up!\n\n");
                }
                else
                {
                    //TextRecogniser(pictureBox1.Image);
                    richTextBox1.AppendText("Keep Items:");
                }
            }
            else
            {
                richTextBox1.AppendText("Not Appropriate for analysis!\n");
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private bool DetectObject(Image<Gray, Byte> inputImage, Image<Gray, Byte> templateImage)
        {
            bool success = false;
            Point dftSize = new Point(inputImage.Width + (templateImage.Width * 2),
                inputImage.Height + (templateImage.Height * 2));
            using (Image<Gray, Byte> pad_array = new Image<Gray, Byte>(dftSize.X, dftSize.Y))
            {
                pad_array.ROI = new Rectangle(templateImage.Width, templateImage.Height, inputImage.Width,
                    inputImage.Height);
                CvInvoke.cvCopy(inputImage.Convert<Gray, Byte>(), pad_array, IntPtr.Zero);

                pad_array.ROI = (new Rectangle(0, 0, dftSize.X, dftSize.Y));
                using (Image<Gray, float> result_Matrix =
                    pad_array.MatchTemplate(templateImage, TemplateMatchingType.CcoeffNormed))
                {
                    result_Matrix.ROI = new Rectangle(templateImage.Width, templateImage.Height, inputImage.Width,
                        inputImage.Height);

                    Point[] MAX_Loc, Min_Loc;
                    double[] min, max;
                    result_Matrix.MinMax(out min, out max, out Min_Loc, out MAX_Loc);

                    using (Image<Gray, double> RG_Image = result_Matrix.Convert<Gray, double>().Copy())
                    {
                        //#TAG WILL NEED TO INCREASE SO THRESHOLD AT LEAST 0.8...used to have 0.7

                        if (max[0] > 0.85)
                        {
                            success = true;
                        }
                    }
                }
            }

            return success;
        }

        private void TextRecogniser(Image image)
        {
            Emgu.CV.OCR.Tesseract tesseract = new Emgu.CV.OCR.Tesseract
                (@"F:\Working Pack\c#\Scripter\Scripter\Templates\eng.traineddata", "eng",
                OcrEngineMode.TesseractLstmCombined);
            var tempImg = new Bitmap(image).ToImage<Gray, byte>();
            tesseract.SetImage(tempImg);
            tesseract.Recognize();
            string response = tesseract.GetUTF8Text();
            tesseract.Dispose();
        }
    }
}