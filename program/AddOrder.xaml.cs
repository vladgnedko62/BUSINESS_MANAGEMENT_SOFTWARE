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
namespace program
{
    /// <summary>
    /// Логика взаимодействия для AddOrder.xaml
    /// </summary>
    public partial class AddOrder : Window
    {
        public ClassesDataContext classes;
         public DBWork work;
        string connectionString;
        public AddOrder(string connstr)
        {
            InitializeComponent();
            _txtDateIn.Text = DateTime.Now.Day.ToString() + "." + DateTime.Now.Month.ToString() + "." + DateTime.Now.Year.ToString();
            connectionString=connstr;
        }
        //Добавления заказа
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                int countIn;
                int year = 1987, mont = 1, day = 1;
                string[] date = _txtDateIn.Text.Split('.');
                //foreach (var item in date)
                //{
                //    MessageBox.Show(item);
                //}
                if (_cmbWhereToSend.SelectedItem==null)
                {
                    MessageBox.Show("Wybierz firmę!");
                    return;
                }
                if (int.TryParse(_txtCountIn.Text, out countIn) && countIn >= 0)
                {
                    if (date.Length == 3 && int.TryParse(date[0], out day) && int.TryParse(date[1], out mont) && int.TryParse(date[2], out year))
                    {
                        if (year > 1987 && year < 3000 && mont > 0 && mont < 13 && day <= DateTime.DaysInMonth(year, mont) && day > 0)
                        {
                            DateTime dt = new DateTime(year, mont, day);
                            //MessageBox.Show(_cmbSearchNo.SelectedItem.ToString());
                            work.AddInOrders(_cmbSearchNo.SelectedItem.ToString(),dt,Convert.ToInt32(_txtCountIn.Text),_cmbWhereToSend.SelectedItem.ToString(),_txtDescription.Text);
                        }
                        else
                        {
                            _txtDateIn.BorderBrush = Brushes.Red;

                            MessageBox.Show("Nieprawidłowa data");
                        }
                    }
                    else
                    {
                        _txtDateIn.BorderBrush = Brushes.Red;

                        MessageBox.Show("Nieprawidłowa data");
                    }
                }
                else
                {
                    _txtCountIn.BorderBrush = Brushes.Red;
                    MessageBox.Show("Zła ilość");

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("AddOutDate: " + ex.Message);
            }
        }

        private void _txtDateIn_TextChanged(object sender, TextChangedEventArgs e)
        {
            _txtDateIn.BorderBrush = Brushes.Silver;
        }

        private void _txtCountIn_TextChanged(object sender, TextChangedEventArgs e)
        {
            _txtCountIn.BorderBrush = Brushes.Silver;
        }
        //Для заполнения combobox
        private void _txtNoItem_TextChanged(object sender, TextChangedEventArgs e)
        {
            Task.Run(() => {
                SqlConnection sql = new SqlConnection(connectionString);
                string noItem = "";
                Dispatcher.Invoke(() => { noItem = _txtNoItem.Text;  });
               
                Dispatcher.Invoke(() => { _cmbSearchNo.ItemsSource = classes.Item.Select(x => x.ItemCode).Where(x => x.Contains(noItem)); _cmbSearchNo.SelectedIndex = 0; });
               
            });
            
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _cmbWhereToSend.ItemsSource = classes.Companies.Select(x => x.ShortName);
        }
    }
}
