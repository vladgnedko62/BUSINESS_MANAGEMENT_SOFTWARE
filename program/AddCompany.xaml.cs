using System;
using System.Collections.Generic;
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

namespace program
{
    /// <summary>
    /// Interaction logic for AddCompany.xaml
    /// </summary>
    public partial class AddCompany : Window
    {
        ClassesDataContext classes = null;
        DBWork work=null;
        public AddCompany(ClassesDataContext classes,string ConnectionString)
        {
            InitializeComponent();
            this.classes = classes;
            this.work = new DBWork(ConnectionString);
        }

        private void _btnAddCompany_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(CompanyName.Text))
            {
                if (!string.IsNullOrEmpty(CompanyShortName.Text))
                {
                    if (!string.IsNullOrEmpty(CompanyAddres.Text))
                    {
                        int NIP=0;
                        if (!string.IsNullOrEmpty(CompanyNIP.Text))
                        {
                            if (!int.TryParse(CompanyNIP.Text, out NIP))
                            {
                                CompanyNIP.BorderBrush = Brushes.Red;
                            }
                           
                        }
                        work.AddCompanyInDB(CompanyName.Text, CompanyShortName.Text, CompanyAddres.Text, NIP);
                        //MessageBox.Show("Zapisane!");


                }
                    else
                    {
                        CompanyAddres.BorderBrush = Brushes.Red;
                    }
                }
                else
                {
                    CompanyShortName.BorderBrush = Brushes.Red;
                }
            }
            else
            {
                CompanyName.BorderBrush = Brushes.Red;
            }
        }

        private void CompanyName_TextChanged(object sender, TextChangedEventArgs e)
        {
            (sender as TextBox).BorderBrush = Brushes.Silver;
        }

        private void CompanyShortName_TextChanged(object sender, TextChangedEventArgs e)
        {
            (sender as TextBox).BorderBrush = Brushes.Silver;
        }

        private void CompanyAddres_TextChanged(object sender, TextChangedEventArgs e)
        {
            (sender as TextBox).BorderBrush = Brushes.Silver;
        }

        private void CompanyNIP_TextChanged(object sender, TextChangedEventArgs e)
        {
            (sender as TextBox).BorderBrush = Brushes.Silver;
        }
    }
}
