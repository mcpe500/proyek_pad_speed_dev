using PAD_ROTIKITA.Controller;
using PAD_ROTIKITA.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PAD_ROTIKITA.Kasir
{
    public partial class Kasir_history : Form
    {
        db_rotiEntities db = DatabaseService.GetDbContext();
        DateTime dateStart, dateEnd;

        public Kasir_history()
        {
            InitializeComponent();
            dari.MaxDate = DateTime.Now.Date;
            sampai.MaxDate = DateTime.Now.Date;

            dari.Value = DateTime.Now.Date;
            sampai.Value = DateTime.Now.Date;
        }

        private void Kasir_history_Load(object sender, EventArgs e)
        {
            loadHistory();
        }
        private void loadHistory()
        {
            DateTime dateStart = dari.Value;
            DateTime dateEnd = sampai.Value.AddDays(1);

            var trans = from h in db.htrans
                        where h.user_id == UserHandler.user_ID
                        where h.tanggal >= dateStart && h.tanggal < dateEnd
                        select new
                        {
                            Tanggal = h.tanggal,
                            ID = h.htrans_id,
                            Item = h.total_item,
                            Diskon = h.total_diskon,
                            Total = h.subtotal
                        };
            dgv_roti.DataSource = trans.ToList();
        }

        private void dari_ValueChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            loadHistory();
        }
    }
}
