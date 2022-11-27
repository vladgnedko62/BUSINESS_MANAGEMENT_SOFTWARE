using System;
using System.Collections.Generic;
using System.Data;
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
    /// Логика взаимодействия для Raport.xaml
    /// </summary>
    public partial class Raport : Window
    {
       public ClassesDataContext classes;
        List<_Raport> raports;
        public Raport()
        {
            InitializeComponent();
          
            _txtDateStart.Text = "1." + DateTime.Now.Month.ToString() + "." + DateTime.Now.Year.ToString(); 
          
            _txtDateEnd.Text = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month).ToString() + "." + DateTime.Now.Month.ToString() + "." + DateTime.Now.Year.ToString();

        }
        //Заполняем таблицу сумой елеметов
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (_cmbWhatCompany.SelectedItem==null)
            {
                return;
            }
            Task.Run(() => {
                string[] date=null;
                string[] date1 = null; ;
                Dispatcher.Invoke(() => {
                    date = _txtDateStart.Text.Split('.');
                    date1 = _txtDateEnd.Text.Split('.');
                });
                int year = 1987, mont = 1, day = 1;
                DateTime dtStart = DateTime.Now, dtEnd = DateTime.Now;
                if (date.Length == 3 && int.TryParse(date[0], out day) && int.TryParse(date[1], out mont) && int.TryParse(date[2], out year))
                {
                    if (year > 1987 && year < 3000 && mont > 0 && mont < 13 && day <= DateTime.DaysInMonth(year, mont) && day > 0)
                    {
                        dtStart = new DateTime(year, mont, day);
                    }
                    else
                    {
                        Dispatcher.Invoke(() => { _txtDateStart.BorderBrush = Brushes.Red; });
                       

                        MessageBox.Show("Nieprawidłowa data");
                        return;
                    }
                }
                else
                {
                    Dispatcher.Invoke(() => { _txtDateStart.BorderBrush = Brushes.Red; });
                  

                    MessageBox.Show("Nieprawidłowa data");
                    return;
                }

                if (date1.Length == 3 && int.TryParse(date1[0], out day) && int.TryParse(date1[1], out mont) && int.TryParse(date1[2], out year))
                {
                    if (year > 1987 && year < 3000 && mont > 0 && mont < 13 && day <= DateTime.DaysInMonth(year, mont) && day > 0)
                    {
                        dtEnd = new DateTime(year, mont, day);
                    }
                    else
                    {
                        Dispatcher.Invoke(() => { _txtDateEnd.BorderBrush = Brushes.Red; });
                       

                        MessageBox.Show("Nieprawidłowa data");
                        return;
                    }
                }
                else
                {
                    Dispatcher.Invoke(() => { _txtDateEnd.BorderBrush = Brushes.Red; });
                   

                    MessageBox.Show("Nieprawidłowa data");
                    return;
                }

                if (dtStart > dtEnd)
                {
                    Dispatcher.Invoke(() => {
                        _txtDateStart.BorderBrush = Brushes.Red;
                        _txtDateEnd.BorderBrush = Brushes.Red;
                    });
                 

                    MessageBox.Show("Data rozpoczęcia nie\n może być późniejsza\n niż data zakończenia!");
                    return;
                }
                else
                {
                    SetGrid_Raport(dtStart, dtEnd);
                }
            });
            
        }
        //Заполняем таблицу сумой елеметов
        private void SetGrid_Raport(DateTime dtStart,DateTime dtEnd)
        {
            raports = new List<_Raport>();
            foreach (ItemOrder item in classes.ItemOrders)
            {
                foreach (var InnerOrder in item.InnerOrders)
                {
                    if (InnerOrder.DateIn >= dtStart && InnerOrder.DateIn<= dtEnd && (item.Finished == true || InnerOrder.CountIn != 0))
                    {
                        if (raports.Count != 0)
                        {
                            bool ok = false;
                            foreach (_Raport raport in raports)
                            {
                                if (InnerOrder.Item.ItemCode == raport._NoItem)
                                {
                                    raport._TotalCountIn += (int)InnerOrder.CountIn;
                                    raport._TotalSurfface += (double)item.TotalDm; ok = true; break;
                                }
                            }
                            if (!ok) { raports.Add(new _Raport(InnerOrder.Item.ItemCode, (int)InnerOrder.CountIn, (double)item.TotalDm)); }
                        }
                        else
                        {
                            string SelectedItem = "";
                            Dispatcher.Invoke(() => { SelectedItem = _cmbWhatCompany.SelectedItem.ToString(); });
                            if (InnerOrder.ItemOrder.Company.ShortName == SelectedItem)
                            {
                                raports.Add(new _Raport(InnerOrder.Item.ItemCode, (int)InnerOrder.CountIn, (double)InnerOrder.CountIn*InnerOrder.Item.Size));
                            }

                        }
                    }
                }
                
            }
            int line = 0;
            DataTable table = new DataTable();
            for (int i = 0; i < raports.Count; i++)
            {
                if (line == 0)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        if (j == 0)
                        {
                            table.Columns.Add("Kod");
                        }
                        else
                        {
                            if (j == 1)
                            {
                                table.Columns.Add("Razem sztuk");
                            }
                            else
                            {
                                if (j == 2)
                                {
                                    table.Columns.Add("Powierzchnia całkowita");
                                }
                            }
                        }
                       
                    }
                    line++;
                }
                DataRow row;
                row = raports[i].SetRow(table);
                table.Rows.Add(row);
            }
            Dispatcher.Invoke(() => {
                _grdRaports.Items.Refresh();
                _grdRaports.ItemsSource = table.DefaultView;
            });
           

        }
        //Заполняем документ для печати
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Task.Run(() => {
                using (_ExelDoc doc = new _ExelDoc())
                {
                    doc.Open(@"doc\baseforraport.xlsx");
                    int startIndex = 3;
                    string UpStr=null;
                    Dispatcher.Invoke(() => { UpStr = "Raport: " + _txtDateStart.Text + " - " + _txtDateEnd.Text; });
                    double TotalSurface = 0;
                    doc.Set("A", 1, UpStr);
                    for (int i = 0; i < raports.Count; i++)
                    {
                        doc.Set("B", startIndex, raports[i]._NoItem);
                        doc.Set("C", startIndex, raports[i]._TotalCountIn.ToString());
                        doc.Set("D", startIndex, raports[i]._TotalSurfface.ToString());
                        TotalSurface += raports[i]._TotalSurfface;
                        startIndex++;
                    }
                    doc.Set("B", 22, TotalSurface.ToString());
                    //MessageBox.Show("Ładowanie");

                }
            });
            
            
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _cmbWhatCompany.ItemsSource=classes.Companies.Select(x=>x.ShortName);
        }
    }
}
