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

namespace PAD_ROTIKITA
{
    public partial class FormHistory : Form
    {
        db_rotiEntities db = DatabaseService.GetDbContext();
        public FormHistory()
        {
            InitializeComponent();
            dari.Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1); ;
        }

        private void FormHistory_Load(object sender, EventArgs e)
        {
            loadDGV();
        }

        private void loadDGV()
        {
            DateTime start = dari.Value.Date;
            DateTime end = sampai.Value.Date.AddDays(1);

            dgv.DataSource = (from h in db.htrans
                              join d in db.dtrans on h.htrans_id equals d.htrans_id
                              join r in db.rotis on d.kode_roti equals r.kode_roti
                              join j in db.jenis_roti on r.kode_jenis equals j.kode_jenis
                              where h.tanggal >= start && h.tanggal < end
                              group d by new { j.kode_jenis, j.nama } into groupedData
                              orderby groupedData.Sum(item => item.qty) descending
                              select new
                              {
                                  Kode = groupedData.Key.kode_jenis,
                                  Nama = groupedData.Key.nama,
                                  TotalQuantity = groupedData.Sum(item => item.qty)
                              }).ToList();
        }

        private void btn_cek_Click(object sender, EventArgs e)
        {
            loadDGV();
        }
    }
}
