using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _3s_atc
{
    public partial class Form_Sizes : Form
    {
        public List<double> sizes;

        public Form_Sizes(List<double> sizes_list = null)
        {
            InitializeComponent();

            if (sizes_list == null)
                sizes = new List<double>();
            else
            {
                sizes = new List<double>(sizes_list);
                foreach(double size in sizes)
                {
                    int index = comboBox1.FindString(String.Format("US {0}", size.ToString("0.#").Replace(',', '.')));
                    listBox1.Items.Add(comboBox1.Items[index].ToString());
                }
            }

            button2.Text = char.ConvertFromUtf32(0x2191);
            button3.Text = char.ConvertFromUtf32(0x2193);
            button4.Text = char.ConvertFromUtf32(0x02DF);
        }

        public double getUSSize(string size)
        {
            string[] tokens = size.Split(null);
            double US_Size = double.Parse(tokens[1], System.Globalization.CultureInfo.InvariantCulture);

            //double Size = ((US_Size - 6.5) * 20) + 580;
            //int rawSize = Convert.ToInt32(Size);

            return US_Size;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (listBox1.FindString(comboBox1.SelectedItem.ToString()) == -1)
            {
                sizes.Add(getUSSize(comboBox1.SelectedItem.ToString()));
                listBox1.Items.Add(comboBox1.SelectedItem.ToString());
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            int selectedIndex = listBox1.SelectedIndex;
            string selectedItemText = listBox1.SelectedItem.ToString();

            if (selectedIndex > 0)
            {
                listBox1.Items.Insert(selectedIndex - 1, listBox1.Items[selectedIndex]);
                listBox1.Items.RemoveAt(selectedIndex + 1);
                listBox1.SelectedIndex = selectedIndex - 1;

                int index = sizes.FindIndex(x => x == getUSSize(selectedItemText));

                if ((index - 1) >= 0)
                {
                    double currentValue = sizes[index];
                    sizes[index] = sizes[index - 1];
                    sizes[index - 1] = currentValue;
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            int selectedIndex = listBox1.SelectedIndex;
            string selectedItemText = listBox1.SelectedItem.ToString();

            if (selectedIndex < listBox1.Items.Count - 1 & selectedIndex != -1)
            {
                listBox1.Items.Insert(selectedIndex + 2, listBox1.Items[selectedIndex]);
                listBox1.Items.RemoveAt(selectedIndex);
                listBox1.SelectedIndex = selectedIndex + 1;

                int index = sizes.FindIndex(x => x == getUSSize(selectedItemText));

                if ((index + 1) < sizes.Count)
                {
                    double currentValue = sizes[index];
                    sizes[index] = sizes[index + 1];
                    sizes[index + 1] = currentValue;
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex != -1)
            {
                for (int i = listBox1.SelectedItems.Count - 1; i >= 0; i--)
                {
                    listBox1.Items.Remove(listBox1.SelectedItems[i]);
                    sizes.RemoveAt(i);
                }
            }
        }
    }
}
