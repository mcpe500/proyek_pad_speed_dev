
using PAD_ROTIKITA.Contracts;
using PAD_ROTIKITA.Controller;
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
    public partial class Kasir_transaksi : Form
    {
        List<dbundle> bundles;
        List<string> diskonList;
        List<string> dbundleList;
        db_rotiEntities db = new db_rotiEntities();
        DateTime now = DateTime.Now.Date;
        public Kasir_transaksi()
        {
            InitializeComponent();
        }

        private void LoadListRoti()
        {
            List<RotiCartVisible> data = new List<RotiCartVisible>();

            foreach (RotiCartVisible r in RotiHandler.GetAllRotiInCart())
            {
                if (r.Name.IndexOf(textBox1.Text, StringComparison.OrdinalIgnoreCase) != -1)
                {
                    data.Add(r);
                }
            }

            listRotiGridView.DataSource = data;
            listRotiGridView.Columns["Expiration"].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;

            diskonList = db.diskons.Where(p=> p.tanggal_mulai <= now && p.tanggal_selesai > now).Select(p => p.kode_roti).ToList();
            dbundleList = db.dbundles.Where(p => p.hbundle.tanggal_mulai <= now && p.hbundle.tanggal_selesai > now).Select(p => p.kode_roti.ToString()).ToList();

            foreach (DataGridViewRow row in listRotiGridView.Rows)
            {
                if (diskonList.Contains(row.Cells[0].Value.ToString()))
                {
                    row.DefaultCellStyle.BackColor = Color.LightGreen;
                }
                else if (dbundleList.Contains(row.Cells[0].Value.ToString()))
                {
                    row.DefaultCellStyle.BackColor = Color.Yellow;
                }
            }
        }
        private void loadCart()
        {
            keranjangDataGridView.Columns["KodeRoti"].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            keranjangDataGridView.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            keranjangDataGridView.Columns["Expiration"].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
        }
        private void reloadCart()
        {

        }

        private void Kasir_transaksi_Load(object sender, EventArgs e)
        {
            LoadListRoti();
            loadCart();
        }

        private void listRotiGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = this.listRotiGridView.Rows[e.RowIndex];
                string kodeRoti = row.Cells["Kode"].Value.ToString();
                string namaRoti = row.Cells["Name"].Value.ToString();
                string expired = row.Cells["Expiration"].Value.ToString();
                string stock = row.Cells["Quantity"].Value.ToString();
                string hargaRoti = row.Cells["Price"].Value.ToString();

                kodeRotiLabel.Text = kodeRoti;
                namaJenisRoti.Text = namaRoti;
                tanggalExpiredLabel.Text = Convert.ToDateTime(expired).Date.ToString();
                qtyNumericUpDown.Value = 0;
                qtyNumericUpDown.Maximum = int.Parse(stock);
                hargaLabel.Text = hargaRoti;
            }
        }
        private void addToCartButton_Click(object sender, EventArgs e)
        {
            string kode_roti = kodeRotiLabel.Text;
            string jenis_roti = namaJenisRoti.Text;
            string tanggal_expired = tanggalExpiredLabel.Text;
            int quantity_label = int.Parse(qtyNumericUpDown.Value.ToString());
            string harga_label = hargaLabel.Text;

            if (!ValidateInput(kode_roti, jenis_roti, tanggal_expired, harga_label, quantity_label))
            {
                return;
            }

            if (!UpdateCartIfItemExists(kode_roti, quantity_label))
            {
                AddNewItemToCart(kode_roti, jenis_roti, harga_label, quantity_label, tanggal_expired);
            }
            diskonDataGridView.Rows.Clear();
            cekDiskon();
            cekBundle();
            UpdateCartSummary();
        }

        private void cekDiskon()
        {
            diskonDataGridView.Rows.Clear();
            foreach (DataGridViewRow row in keranjangDataGridView.Rows)
            {
                string kode = row.Cells[0].Value.ToString();

                diskon d = db.diskons.Where(p => p.kode_roti == kode && p.tanggal_mulai <= now && p.tanggal_selesai > now).FirstOrDefault();
                if (d != null)
                {
                    diskonDataGridView.Rows.Add(d.kode_diskon, d.keterangan, row.Cells[1].Value, d.potongan, row.Cells[3].Value, d.potongan * (int)row.Cells[3].Value);
                }
            }
        }

        private void cekBundle()
        {
            List<string> listKodeRoti = new List<string>();
            List<int> quantities = new List<int>();

            foreach (DataGridViewRow row in keranjangDataGridView.Rows)
            {
                listKodeRoti.Add(row.Cells[0].Value.ToString());
                quantities.Add((int)row.Cells[3].Value);
            }



            int counter = 0;
            string nama_roti = string.Empty;
            var hbundle = db.hbundles.Where(p => p.tanggal_mulai <= now && p.tanggal_selesai > now).ToList();
            for (int a = 0; a < hbundle.Count; a++)
            {
                string kode_bundle = hbundle[a].kode_bundle.ToString();
                List<dbundle> alldbundle = db.dbundles.Where(p => p.kode_bundle.Equals(kode_bundle)).ToList();
                int match = 0;
                foreach (dbundle d in alldbundle)
                {
                    for (int i = 0; i < quantities.Count; i++)
                    {
                        if (listKodeRoti[i].Equals(d.kode_roti) && quantities[i] >= d.qty)
                        {
                            quantities[i] -= (int)d.qty;

                            string kodejenis = db.rotis.Where(p => p.kode_roti == d.kode_roti).FirstOrDefault()?.kode_jenis;
                            if (counter == 0)
                            {
                                string namaRoti = db.jenis_roti.Where(p => p.kode_jenis == kodejenis).FirstOrDefault()?.nama;
                                nama_roti += namaRoti + ", ";

                            }
                            match++;
                        }
                    }
                }
                if (match == alldbundle.Count)
                {
                    counter++;
                    a--;

                }
                else if (counter > 0)
                {
                    nama_roti = nama_roti.TrimEnd(',', ' ');
                    diskonDataGridView.Rows.Add(hbundle[a].kode_bundle, hbundle[a].keterangan, nama_roti, hbundle[a].potongan, counter, (int)hbundle[a].potongan * counter);
                    counter = 0;
                    nama_roti = string.Empty;
                }
            }
        }

        private void handleCekBundles()
        {
            List<string> listKodeRoti = new List<string>();
            List<int> quantities = new List<int>();
            for (int i = 0; i < keranjangDataGridView.Rows.Count; i++)
            {
                var cell1Value = keranjangDataGridView.Rows[i].Cells[1].Value;
                var cell3Value = keranjangDataGridView.Rows[i].Cells[3].Value;
                if (cell1Value != null && cell3Value != null)
                {
                    listKodeRoti.Add(cell1Value.ToString());
                    quantities.Add(int.Parse(cell3Value.ToString()));
                }
            }
            bundles = RotiHandler.GetDbundles(listKodeRoti, quantities);
            if (bundles != null)
            {
                int index = 0;
                List<string> ht_ids = new List<string>();
                List<List<dbundle>> dbundle_on_ht_id = new List<List<dbundle>>();
                foreach (dbundle item in bundles)
                {
                    if (item != null && !ht_ids.Contains(item.kode_bundle))
                    {
                        ht_ids.Add(item.kode_bundle);
                        dbundle_on_ht_id.Add(new List<dbundle>());
                        index++;
                    }
                    if (item != null)
                    {
                        dbundle_on_ht_id[index].Add(item);
                    }
                }
                for (int i = 0; i < dbundle_on_ht_id.Count; i++)
                {
                    List<dbundle> item = dbundle_on_ht_id[i];
                    string kode_roti_ht = "";
                    string jenis_roti_ht = "";
                    string diskonValue_ht = "";
                    string quantity_label_ht = "";
                    string diskonValue_quantity_label = "";
                    for (int j = 0; j < item.Count; j++)
                    {
                        dbundle item1 = item[j];
                        if (item1 != null)
                        {
                            kode_roti_ht += item1.kode_roti + ",";
                            jenis_roti_ht += item1.kode_roti + ",";
                            diskonValue_ht += item1.harga_roti + ",";
                            quantity_label_ht += item1.qty + ",";
                            diskonValue_quantity_label += int.Parse(diskonValue_ht) * int.Parse(quantity_label_ht) + ",";
                        }
                    }
                }
            }
        }

        private bool ValidateInput(string kode_roti, string jenis_roti, string tanggal_expired, string harga_label, int quantity_label)
        {
            if (kode_roti == "" || jenis_roti == "" || tanggal_expired == "" || harga_label == "")
            {
                MessageBox.Show("Belum Memilih Roti");
                return false;
            }
            else if (quantity_label == 0)
            {
                MessageBox.Show("Quantity tidak boleh 0");
                return false;
            }
            else if (quantity_label > qtyNumericUpDown.Maximum)
            {
                MessageBox.Show("Quantity melebihi stock");
                return false;
            }
            return true;
        }

        private bool UpdateCartIfItemExists(string kode_roti, int quantity_label)
        {
            foreach (DataGridViewRow item in keranjangDataGridView.Rows)
            {
                if (item.Cells[0].Value.ToString().Equals(kode_roti))
                {
                    UpdateItemQuantityIfValid(item, kode_roti, quantity_label);
                    return true;
                }
            }
            return false;
        }

        private void UpdateItemQuantityIfValid(DataGridViewRow item, string kode_roti, int quantity_label)
        {
            string currentQty = item.Cells[3].Value.ToString();
            int currentQtyInt = int.Parse(currentQty) + quantity_label;
            if (IsQuantityValid(kode_roti, currentQtyInt))
            {
                item.Cells[3].Value = currentQtyInt;
                float harga = float.Parse(item.Cells[2].Value.ToString());
                item.Cells[5].Value = harga * currentQtyInt;
            }
            else
            {
                MessageBox.Show("Permintaan Melebihi Stok");
            }
        }

        private bool IsQuantityValid(string kode_roti, int currentQtyInt)
        {
            foreach (DataGridViewRow item in listRotiGridView.Rows)
            {
                if (item.Cells[0].Value.ToString().Equals(kode_roti))
                {
                    return int.Parse(item.Cells[3].Value.ToString()) >= currentQtyInt;
                }
            }
            return false;
        }

        private void AddNewItemToCart(string kode_roti, string jenis_roti, string harga_label, int quantity_label, string tanggal_expired)
        {
            float harga = float.Parse(harga_label);
            keranjangDataGridView.Rows.Add(kode_roti, jenis_roti, harga_label, quantity_label, tanggal_expired, quantity_label * harga);
        }

        private void UpdateCartSummary()
        {
            float totalHarga = 0;
            int jumlahItem = 0;
            foreach (DataGridViewRow item in keranjangDataGridView.Rows)
            {
                jumlahItem += (int)item.Cells[3].Value;
                totalHarga += float.Parse(item.Cells[5].Value.ToString());
            }
            int totalDiskon = 0;
            foreach (DataGridViewRow item in diskonDataGridView.Rows)
            {
                totalDiskon += int.Parse(item.Cells[5].Value.ToString());
            }
            jumlahItemLabel.Text = jumlahItem.ToString();
            totalHargaLabel.Text = (totalHarga - totalDiskon).ToString();
        }

        private void buyButton_Click(object sender, EventArgs e)
        {
            List<BuyRotiItemVo> cartList = new List<BuyRotiItemVo>();
            List<DiskonItemVo> diskonList = new List<DiskonItemVo>();
            foreach (DataGridViewRow item in keranjangDataGridView.Rows)
            {
                string kode = item.Cells[0].Value.ToString();
                string nama = item.Cells[1].Value.ToString();
                int harga = int.Parse(item.Cells[2].Value.ToString());
                int quantity = int.Parse(item.Cells[3].Value.ToString());
                int subtotal = int.Parse(item.Cells[5].Value.ToString());
                cartList.Add(new BuyRotiItemVo(kode, nama, harga, quantity, subtotal));
            }
            foreach (DataGridViewRow item in diskonDataGridView.Rows)
            {
                string kode = item.Cells[0].Value.ToString();
                string keterangan = item.Cells[1].Value.ToString();
                string nama = item.Cells[2].Value.ToString();
                int harga = int.Parse(item.Cells[3].Value.ToString());
                int quantity = int.Parse(item.Cells[4].Value.ToString());
                int subtotal = int.Parse(item.Cells[5].Value.ToString());
                diskonList.Add(new DiskonItemVo(kode, nama, harga, quantity, subtotal));
            }
            int totalHarga = int.Parse(totalHargaLabel.Text);
            RotiHandler.BuyRoti(cartList, totalHarga, diskonList);

            keranjangDataGridView.Rows.Clear();
            diskonDataGridView.Rows.Clear();

            totalHargaLabel.Text = String.Empty;
            jumlahItemLabel.Text = String.Empty;
            LoadListRoti();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (RotiHandler.HEADER_HTRANS.Length == 10)
            {
                Hide();
                FormStruk frS = new FormStruk(RotiHandler.HEADER_HTRANS);
                frS.ShowDialog();
                Show();
            }
            else
            {
                MessageBox.Show("BELUM MELAKUKAN TRANSAKSI");
            }
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void listRotiGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox1_KeyUp(object sender, KeyEventArgs e)
        {
            LoadListRoti();
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void label17_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged_1(object sender, EventArgs e)
        {
        }

        private void butButton_Click_1(object sender, EventArgs e)
        {

        }

        private void listRotiGridView_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            string kode_roti = listRotiGridView.Rows[e.RowIndex].Cells[0].Value.ToString();
            diskon d = db.diskons.Where(p=> p.kode_roti == kode_roti && p.tanggal_mulai <= now && p.tanggal_selesai > now).FirstOrDefault();

            if (d != null)
            {
                Detail_diskon form_detail = new Detail_diskon(d.kode_diskon);
                form_detail.ShowDialog();
            } else
            {
                dbundle dbundle = db.dbundles.Where(p=>p.kode_roti == kode_roti).FirstOrDefault();
                if (dbundle != null)
                {
                    if (dbundle.hbundle.tanggal_mulai <= now && dbundle.hbundle.tanggal_selesai > now)
                    {
                        Detail_bundle detail_Bundle = new Detail_bundle(dbundle.hbundle.kode_bundle);
                        detail_Bundle.ShowDialog();
                    }
                }
                    
                
            }
        }
    }
}
