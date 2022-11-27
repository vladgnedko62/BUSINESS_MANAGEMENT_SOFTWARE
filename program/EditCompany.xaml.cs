using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
using System.Windows.Shapes;
using Dapper;
using Dapper.Contrib;
using WF = System.Windows.Forms;
namespace program
{
    /// <summary>
    /// Interaction logic for EditCompany.xaml
    /// </summary>
    public partial class EditCompany : Window
    {

        string connectionString = "";
        ClassesDataContext classes = null;

        public EditCompany(ClassesDataContext classes,string connectionString)
        {
            InitializeComponent();
            this.classes = classes;
            this.connectionString = connectionString;
        }

        private void _txtNoItem_TextChanged(object sender, TextChangedEventArgs e)
        {
            Task.Run(() =>
            {
                string Text = "";
                Dispatcher.Invoke(() => { Text = _txtNoItem.Text; _cmbSearchNo.Items.Clear(); /*_cmbFromWhom.ItemsSource = null; _cmbFromWhom.Items.Clear();*/ });
                SqlConnection sqlConnection = new SqlConnection(connectionString);
                lock (sqlConnection)
                {
                    List<Company> items = sqlConnection.QueryAsync<Company>("select * from Companies").Result.Select(x => x).Where(x => x.ShortName.Contains(Text)).ToList();
                    foreach (var item in items)
                    {
                        Dispatcher.Invoke(() => { _cmbSearchNo.Items.Add(item.ShortName); });
                    }


                    Dispatcher.Invoke(() => {  _cmbSearchNo.SelectedIndex = 0; });




                }
            });
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_cmbSearchNo.SelectedItem!=null)
                {
                    int NIP = 0;
                    if (!string.IsNullOrEmpty(_txtCompanyNIP.Text))
                    {
                        if (!int.TryParse(_txtCompanyNIP.Text, out NIP))
                        {
                            _txtCompanyNIP.BorderBrush = Brushes.Red;
                            return;
                        }
                    }
                    SqlConnection sql = new SqlConnection(connectionString);
                    Company company = sql.QueryFirstOrDefault<Company>("select * from Companies where ShortName=@sh", new { sh = _cmbSearchNo.SelectedItem.ToString() });
                    sql.Execute("update Companies set Name=@n,ShortName=@sn,Addres=@a,NIP=@nip where ShortName=@shn", new { n = _txtNewCompanyName.Text, sn = _txtCompanyShortName.Text, a = _txtCompanyAddres.Text, nip = NIP,shn=_cmbSearchNo.SelectedItem.ToString() });
                    //MessageBox.Show("Zapisano!");
                }
                
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void _txtNewCompanyName_TextChanged(object sender, TextChangedEventArgs e)
        {
            (sender as TextBox).BorderBrush = Brushes.Silver;
        }

        private void _txtCompanyShortName_TextChanged(object sender, TextChangedEventArgs e)
        {
            (sender as TextBox).BorderBrush = Brushes.Silver;
        }

        private void _txtCompanyAddres_TextChanged(object sender, TextChangedEventArgs e)
        {
            (sender as TextBox).BorderBrush = Brushes.Silver;
        }

        private void _txtCompanyNIP_TextChanged(object sender, TextChangedEventArgs e)
        {
            (sender as TextBox).BorderBrush = Brushes.Silver;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_cmbSearchNo.SelectedItem != null)
                {
                    WF.DialogResult dialogResult = WF.MessageBox.Show("Z usunięciemfirma wszystkie zamówienia, które miały zostać usunięte dla tej firmy.Pewnie ? ", "!!!", WF.MessageBoxButtons.YesNo);
                    if (dialogResult == WF.DialogResult.Yes)
                    {
                        SqlConnection sql = new SqlConnection(connectionString);
                        Company companyForDelete = classes.Companies.Select(x => x).Where(x => x.ShortName == _cmbSearchNo.SelectedItem.ToString()).FirstOrDefault();
                        List<ItemOrder> ordersForDelete = classes.ItemOrders.Select(x => x).Where(x => x.CompanyId == companyForDelete.Id).AsList();
                        ordersForDelete.ForEach(x => sql.Execute("delete from InnerOrders where OrderId=@oi", new { oi = x.Id }) );
                        ordersForDelete.ForEach(x => sql.Execute("delete from ItemOrders where Id=@id", new { id = x.Id }));
                        sql.Execute("delete from Companies where Id=@id", new { id = companyForDelete.Id });
                        MessageBox.Show("Usunięty!");
                    }

                }

            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void _cmbSearchNo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Task.Run(() => {

                int NIP = 0;
                string CompanyName = "Error";
                string CompanyShortName = "Error";
                string CompanyAddres = "Error";
                string SelectedItem = "&&&";
                Dispatcher.Invoke(() => { if (_cmbSearchNo.SelectedItem != null) { SelectedItem = _cmbSearchNo.SelectedItem.ToString(); } });
                Company item = classes.Companies.Select(x => x).Where(x => x.ShortName == SelectedItem).FirstOrDefault();
                if (item != null) { NIP =(int)item.NIP; CompanyName = item.Name;CompanyShortName = item.ShortName;CompanyAddres = item.Addres; }
                Dispatcher.Invoke(() => {
                    _txtNewCompanyName.Text = CompanyName;
                    _txtCompanyShortName.Text = CompanyShortName;
                    _txtCompanyAddres.Text = CompanyAddres;
                    _txtCompanyNIP.Text = NIP.ToString();
                });
            });
        }
    }
}
