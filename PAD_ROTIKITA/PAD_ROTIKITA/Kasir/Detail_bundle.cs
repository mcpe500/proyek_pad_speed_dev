using PAD_ROTIKITA.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PAD_ROTIKITA.Kasir
{
    public partial class Detail_bundle : Form
    {
        hbundle h;
        db_rotiEntities db = DatabaseService.GetDbContext();
        public Detail_bundle(string id)
        {
            InitializeComponent();
            h = db.hbundles.Find(id);
            dgv.DataSource = h.dbundles;
        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void Detail_bundle_Load(object sender, EventArgs e)
        {

        }
    }
}
