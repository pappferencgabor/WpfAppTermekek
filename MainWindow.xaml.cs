using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using MySql.Data.MySqlClient;

namespace SQL
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            AdatbazisMegnyitas();

            GyartoBetoltese();
            KategoriaBetoltese();

            TermekBetolteseListaba();

        }

        string connectionString = "datasource=127.0.0.1;port=3306;username=root;password=;database=hardver";
        List<Termek> termekek = new List<Termek>();
        MySqlConnection connection;

        private void AdatbazisMegnyitas()
        {
            try
            {
                connection = new MySqlConnection(connectionString);
                connection.Open();
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(ex.Message);
                this.Close();
            }
        }

        private void AdatbazisLezaras()
        {
            connection.Close();
            connection.Dispose();
        }

        private void TermekBetolteseListaba()
        {
            MySqlCommand SQLParancs = new MySqlCommand("select * from termékek", connection);
            MySqlDataReader reader = SQLParancs.ExecuteReader();

            while (reader.Read())
            {
                Termek ujTermek = new Termek(
                    reader.GetString(1),
                    reader.GetString(2),
                    reader.GetString(3),
                    reader.GetInt32(4),
                    reader.GetInt32(5)
                );
                termekek.Add(ujTermek);
            }
            reader.Close();

            dgTermekek.ItemsSource = termekek;
        }

        private string SzukitoLerendezesEloallitasa()
        {
            bool vanFeltetel = false;
            string SQLSzukites = "select * from termékek ";

            if (cbGyarto.SelectedIndex > 0 || cbKategoria.SelectedIndex > 0 || txtTermek.Text != "")
            {
                SQLSzukites += "where ";
            }

            if (cbGyarto.SelectedIndex > 0)
            {
                SQLSzukites += $"gyártó = '{cbGyarto.SelectedItem}'";
                vanFeltetel = true;
            }

            if (cbKategoria.SelectedIndex > 0)
            {
                if (vanFeltetel)
                {
                    SQLSzukites += " and ";
                }
                SQLSzukites += $"kategória = '{cbKategoria.SelectedItem}'";
                vanFeltetel = true;
            }

            if (txtTermek.Text != "")
            {
                if (vanFeltetel)
                {
                    SQLSzukites += " and ";
                }
                SQLSzukites += $"név like '%{txtTermek.Text}%'";
            }
            return SQLSzukites;
        }

        private void KategoriaBetoltese()
        {
            MySqlCommand SQLParancs = new MySqlCommand("select distinct kategória from termékek order by kategória;", connection);
            MySqlDataReader reader = SQLParancs.ExecuteReader();

            cbKategoria.Items.Add(" ~ Nincs megadva! ~ ");
            cbKategoria.SelectedIndex = 0;
            while (reader.Read())
            {
                cbKategoria.Items.Add(reader.GetString(0));
            }
            reader.Close();
        }

        private void GyartoBetoltese()
        {
            MySqlCommand SQLParancs = new MySqlCommand("select distinct gyártó from termékek order by gyártó;", connection);
            MySqlDataReader reader = SQLParancs.ExecuteReader();

            cbGyarto.Items.Add(" ~ Nincs megadva! ~ ");
            cbGyarto.SelectedIndex = 0;
            while (reader.Read())
            {
                cbGyarto.Items.Add(reader.GetString(0));
            }
            reader.Close();
        }

        private void btnSzukit_Click(object sender, RoutedEventArgs e)
        {
            termekek.Clear();
            string SQLSzukitettLista = SzukitoLerendezesEloallitasa();

            MySqlCommand SQLParancs = new MySqlCommand(SQLSzukitettLista, connection);
            MySqlDataReader reader = SQLParancs.ExecuteReader();

            while (reader.Read())
            {
                Termek ujTermek = new Termek(
                    reader.GetString(1),
                    reader.GetString(2),
                    reader.GetString(3),
                    reader.GetInt32(4),
                    reader.GetInt32(5)
                );
                termekek.Add(ujTermek);
            }
            reader.Close();

            dgTermekek.Items.Refresh();
        }

        private void btnMentes_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sd = new SaveFileDialog();
            sd.DefaultExt = ".csv";
            sd.Filter = "CSV file (*.csv)|*.csv| All Files (*.*)|*.*";

            if (sd.ShowDialog() == true)
            {
                StreamWriter sw = new StreamWriter(sd.FileName);
                foreach (var item in termekek)
                {
                    sw.WriteLine(item.ToCSVstr());
                }
                sw.Close();
                MessageBox.Show("Sikeresen elmentetted a táblázat tartalmát!");
            }
            else
            {
                MessageBox.Show("A táblázat tartalmának mentése sikertelen!");
            }
        }
    }
}
