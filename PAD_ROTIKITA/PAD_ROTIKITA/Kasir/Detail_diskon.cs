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
    public partial class Detail_diskon : Form
    {
        diskon d;
        db_rotiEntities db = DatabaseService.GetDbContext();
        public Detail_diskon(string id)
        {
            InitializeComponent();
            d = db.diskons.Find(id);

            kode_diskon.Text = d.kode_diskon;
            keterangan.Text = d.keterangan;
            tgl_mulai.Text = ((DateTime)d.tanggal_mulai).Date + "";
            tgl_selesai.Text = ((DateTime)d.tanggal_selesai).Date + "";
            kode_roti.Text = d.kode_roti;
            string kodejenis = db.rotis.Where(p => p.kode_roti == d.kode_roti).FirstOrDefault()?.kode_jenis;
            nama_roti.Text = db.jenis_roti.Where(p=>p.kode_jenis == kodejenis).FirstOrDefault()?.nama;
            nama.Text = d.nama;



        }

        private void Detail_diskon_Load(object sender, EventArgs e)
        {

        }

        private void keterangan_Click(object sender, EventArgs e)
        {

        }
    }
}
