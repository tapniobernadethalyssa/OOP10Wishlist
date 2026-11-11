using System;
using System.Globalization;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace Wishlist
{
    public partial class Form1 : Form
    {

        private const string ConnectionString =
            "Server=localhost;Port=3306;Database=wishlist_db;Uid=root;Pwd=;";

        public Form1()
        {
            InitializeComponent();


            dataGridView1.Columns.Clear();

            dataGridView1.Columns.Add("Id", "Id");
            dataGridView1.Columns["Id"].Visible = false;  

            dataGridView1.Columns.Add("WishlistName", "Wishlist Name");
            dataGridView1.Columns.Add("Item1", "Item 1");
            dataGridView1.Columns.Add("Item2", "Item 2");
            dataGridView1.Columns.Add("Amount", "Amount Range");


            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.ReadOnly = true;
            dataGridView1.MultiSelect = false;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.RowHeadersVisible = false;


            button1.Click += txtSubmit_Click;


            dataGridView1.CellMouseDown += dataGridView1_CellMouseDown;

            LoadWishlist();
        }
        private static string FormatAmount(decimal amount)
        {
            return amount.ToString("C2", CultureInfo.GetCultureInfo("en-US"));
        }

        private void LoadWishlist()
        {
            try
            {
                dataGridView1.Rows.Clear();

                using (var conn = new MySqlConnection(ConnectionString))
                using (var cmd = new MySqlCommand(
                    "SELECT id, wishlist_name, item1, item2, amount FROM wishlist ORDER BY id",
                    conn))
                {
                    conn.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            dataGridView1.Rows.Add(
                                reader.GetInt32("id"),
                                reader.GetString("wishlist_name"),
                                reader.GetString("item1"),
                                reader.GetString("item2"),
                                FormatAmount(reader.GetDecimal("amount")));
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                ShowDbError("load your wishlist", ex);
            }
        }

        private void txtSubmit_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text) ||
                string.IsNullOrWhiteSpace(textBox2.Text) ||
                string.IsNullOrWhiteSpace(textBox3.Text) ||
                string.IsNullOrWhiteSpace(textBox4.Text))
            {
                MessageBox.Show(
                    "Please enter all required information.",
                    "Validation Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            decimal amount;

            if (!decimal.TryParse(
                textBox4.Text,
                NumberStyles.Currency,
                CultureInfo.CurrentCulture,
                out amount) || amount < 0)
            {
                MessageBox.Show(
                    "Please enter a valid, non-negative amount.",
                    "Invalid Amount",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                textBox4.Focus();
                return;
            }

            string name = textBox1.Text.Trim();
            string item1 = textBox2.Text.Trim();
            string item2 = textBox3.Text.Trim();

            long newId;

            try
            {
                using (var conn = new MySqlConnection(ConnectionString))
                using (var cmd = new MySqlCommand(
                    "INSERT INTO wishlist (wishlist_name, item1, item2, amount) " +
                    "VALUES (@name, @item1, @item2, @amount)",
                    conn))
                {
                    cmd.Parameters.AddWithValue("@name", name);
                    cmd.Parameters.AddWithValue("@item1", item1);
                    cmd.Parameters.AddWithValue("@item2", item2);
                    cmd.Parameters.AddWithValue("@amount", amount);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                    newId = cmd.LastInsertedId;
                }
            }
            catch (MySqlException ex)
            {
                ShowDbError("save this item", ex);
                return;
            }

            dataGridView1.Rows.Add(newId, name, item1, item2, FormatAmount(amount));

            textBox1.Clear();
            textBox2.Clear();
            textBox3.Clear();
            textBox4.Clear();

            textBox1.Focus();
        }

        private void dataGridView1_CellMouseDown(
            object sender,
            DataGridViewCellMouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right || e.RowIndex < 0)
            {
                return;
            }

            dataGridView1.ClearSelection();
            dataGridView1.Rows[e.RowIndex].Selected = true;

            ContextMenuStrip menu = new ContextMenuStrip();
            ToolStripMenuItem deleteItem = new ToolStripMenuItem("Delete Row");

            deleteItem.Click += (s, args) =>
            {
                DialogResult result = MessageBox.Show(
                    "Are you sure you want to delete this item from your wishlist?",
                    "Confirm Deletion",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result != DialogResult.Yes || e.RowIndex >= dataGridView1.Rows.Count)
                {
                    return;
                }

                int id = Convert.ToInt32(dataGridView1.Rows[e.RowIndex].Cells["Id"].Value);

                try
                {
                    using (var conn = new MySqlConnection(ConnectionString))
                    using (var cmd = new MySqlCommand(
                        "DELETE FROM wishlist WHERE id = @id", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);

                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }

                    dataGridView1.Rows.RemoveAt(e.RowIndex);
                }
                catch (MySqlException ex)
                {
                    ShowDbError("delete this item", ex);
                }
            };

            menu.Items.Add(deleteItem);
            menu.Show(Cursor.Position);
        }

        private void ShowDbError(string action, MySqlException ex)
        {
            MessageBox.Show(
                "Could not " + action + ".\n\n" +
                "Make sure MySQL is running in the XAMPP Control Panel and that the " +
                "wishlist_db database exists.\n\nDetails: " + ex.Message,
                "Database Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }

        private void label1_Click(object sender, EventArgs e) { }
        private void label2_Click(object sender, EventArgs e) { }
        private void label3_Click(object sender, EventArgs e) { }
        private void label4_Click(object sender, EventArgs e) { }
        private void Form1_Load(object sender, EventArgs e) { }
        private void panel1_Paint(object sender, PaintEventArgs e) { }
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e) { }
    }
}