using Barcoded;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestBarcoded
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            InstalledFontCollection installedFonts = new InstalledFontCollection();
            List<String> fonts = installedFonts.Families
                .Cast<FontFamily>()
                .Select(f => f.Name.ToString())
                .ToList();

            comboBox1.Items.AddRange(fonts.ToArray());
            comboBox1.SelectedIndex = 4;
        }

        private void PictureBox1_Click(object sender, EventArgs e)
        {

            LinearBarcode lb = new LinearBarcode(textBox1.Text, symbology.SelectedItem.ToString());

            lb.Encoder.ShowEncoding = false;
            lb.Encoder.TargetWidth = (int)Target.Value;
            lb.Encoder.BarcodeHeight = (int)Height.Value;
            lb.Encoder.DPI = (int)DPI.Value;
            lb.Encoder.Xdimension = (int)Xdim.Value;
            lb.Encoder.HumanReadableValue = HumanReadable.Text;
            lb.Encoder.SetEncodingFontFamily(comboBox1.SelectedItem.ToString());
            lb.Encoder.SetHumanReadableFont(comboBox1.SelectedItem.ToString(), (int)numericUpDown1.Value);
            lb.Encoder.SetHumanReadablePosition("Below");
            lb.Encoder.ShowEncoding = showencoding.Checked;
            pictureBox1.Image = lb.Image;
            labFontSize.Text = lb.Encoder.HumanReadableFont.Size.ToString();
            labFontChanged.Text = lb.Encoder.HumanReadabaleFontSizeChanged.ToString();
            labHeight.Text = lb.Encoder.BarcodeHeight.ToString();
            labHeightChanged.Text = lb.Encoder.BarcodeHeightChanged.ToString();
            labWidth.Text = lb.Encoder.BarcodeWidth.ToString();
            labDPI.Text = lb.Encoder.DPI.ToString();
            labDPIChanged.Text = lb.Encoder.DPIChanged.ToString();
            labXdim.Text = lb.Encoder.Xdimension.ToString();
            LabXdimChanged.Text = lb.Encoder.XdimensionChanged.ToString();

        }

        private void Button1_Click(object sender, EventArgs e)
        {


        }
    }
}
