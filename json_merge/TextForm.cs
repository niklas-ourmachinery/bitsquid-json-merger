using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace json_merge
{
    /// <summary>
    /// Form used to display a simple text message.
    /// </summary>
    public partial class TextForm : Form
    {
        public TextForm()
        {
            InitializeComponent();
        }

        public string DisplayText
        {
            set { textBox1.Text = value; }
        }
    }
}
