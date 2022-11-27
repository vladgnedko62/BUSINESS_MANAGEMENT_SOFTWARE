using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;
using Draw= System.Drawing;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using Dapper;
using Dapper.Contrib;
using System.Diagnostics;

namespace program
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=Firma;Integrated Security=SSPI;"; /*ConfigurationManager.ConnectionStrings["MyConnection"].ToString();*/
        DBWork work;
        ClassesDataContext classes;
        AddInDB InDB;
        AddOrder InItemOrder;
        Key a = Key.K, b = Key.K;
        int selectedId = 0;

        public MainWindow()
        {
            InitializeComponent();
            //_grid_Main.Background = new SolidColorBrush(Color.FromRgb(236, 242, 56));
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Task.WaitAll();
            work.Close();
        }
        public void ReCreate(string NameDir)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(NameDir);

            foreach (FileInfo file in dirInfo.GetFiles())
            {
                file.Delete();
            }
            Directory.Delete(NameDir);
            Directory.CreateDirectory(NameDir);
        }

        //Обработка кнопки Odśwież заполняем таблицу
        private void SeeInfo_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                try
                {

                    Dispatcher.Invoke(() => { work.ShowFinished = (bool)_ckboxShowFinished.IsChecked; });
                    work.ShowAllDataInItemOrders();
                }
                catch (Exception ex)
                {

                    MessageBox.Show("SeeInfo: " + ex.Message);
                }
            });


        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                try
                {
                    int countIn;
                    int year = 1987, mont = 1, day = 1;
                    string[] date = null;
                    string txtCountIn = null;
                    Dispatcher.Invoke((Action)(() =>
                    {
                        if (_cmbFromWhom.SelectedItem == null)
                        {
                            MessageBox.Show("Wybierz firmę!");
                            return;
                        }
                        date = _txtDateIn.Text.Split('.');
                        txtCountIn = _txtCountIn.Text;
                    }));

                    //foreach (var item in date)
                    //{
                    //    MessageBox.Show(item);
                    //}
                    
                    if (int.TryParse(txtCountIn, out countIn) && countIn >= 0)
                    {
                        if (date.Length == 3 && int.TryParse(date[0], out day) && int.TryParse(date[1], out mont) && int.TryParse(date[2], out year))
                        {
                            if (year > 1987 && year < 3000 && mont > 0 && mont < 13 && day <= DateTime.DaysInMonth(year, mont) && day > 0)
                            {
                                DateTime dt = new DateTime(year, mont, day);
                                //MessageBox.Show(_cmbSearchNo.SelectedItem.ToString());
                                string SelectedItem = null;
                                string SelectedItemCompany = null;
                                bool Finish = false;
                                Dispatcher.Invoke(() => { SelectedItemCompany = _cmbFromWhom.SelectedItem.ToString(); SelectedItem = _cmbSearchNo.SelectedItem.ToString(); Finish = (bool)_ckbox_Finish.IsChecked; });
                                work.UpdateIfTakeDetail(selectedId, dt, countIn,Finish,SelectedItemCompany);
                                work.ShowAllDataInItemOrders();

                            }
                            else
                            {
                                Dispatcher.Invoke(() => { _txtDateIn.BorderBrush = Brushes.Red; });


                                MessageBox.Show("Nieprawidłowa data");
                            }
                        }
                        else
                        {
                            Dispatcher.Invoke(() => { _txtDateIn.BorderBrush = Brushes.Red; });


                            MessageBox.Show("Nieprawidłowa data");
                        }
                    }
                    else
                    {
                        Dispatcher.Invoke(() => { _txtCountIn.BorderBrush = Brushes.Red; });

                        MessageBox.Show("Zła ilość");

                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("AddOutDate: " + ex.Message);
                    return;
                }
               
                    //Dispatcher.Invoke(() =>
                    //{
                    //    _txtCountIn.Text = "";
                    //    _cmbSearchNo.Items.Clear();
                    //    _txtNoItem.Text = "";
                    //});

                
            });

        }
        //Обработка собития TextChanged для поиска  активных заказов
        private void _txtNoItem_TextChanged(object sender, TextChangedEventArgs e)
        {
            Task.Run(() =>
            {
                string Text = "";
                Dispatcher.Invoke(() => { Text = _txtNoItem.Text; _cmbSearchNo.Items.Clear(); _cmbFromWhom.ItemsSource = null; _cmbFromWhom.Items.Clear(); });
                SqlConnection sql = new SqlConnection(connectionString);

                List<ItemOrder> orders = classes.ItemOrders.Select(x => x).Where(x => x.Finished == false).Where(x => x.Item.ItemCode.Contains(Text)).ToList();
                foreach (var item in orders)
                {
                    Dispatcher.Invoke(() => { _cmbSearchNo.Items.Add(item.Item.ItemCode+"___"+item.DateOut.Date.ToShortDateString());_cmbFromWhom.Items.Add(item.Company.ShortName); });
                    
                }
                Dispatcher.Invoke(() => { _cmbSearchNo.SelectedIndex = 0; _cmbFromWhom.SelectedIndex = 0; });

            });


        }

        //Обновляем базу из таблицы
        private void UpdateSave_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(work.UpdateInfoInGrid);

        }

        private void _txtCountIn_TextChanged(object sender, TextChangedEventArgs e)
        {
            _txtCountIn.BorderBrush = Brushes.Silver;
        }

        private void _txtDateIn_TextChanged(object sender, TextChangedEventArgs e)
        {
            _txtDateIn.BorderBrush = Brushes.Silver;
        }
        //Откриваем окно для добавления елемента в базу
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            InDB = new AddInDB();
            InDB.ShowDialog();
            Restart();
        }
        //Откриваем окно для добавления заказа в базу
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            InItemOrder = new AddOrder(connectionString);
            InItemOrder.work = work;
            InItemOrder.classes = classes;
            InItemOrder.ShowDialog();

        }
        //Обработка собития SelectionChanged у таблицы предназначено для отображения картинки елемента
        private void DataGrid1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Task.Run(() =>
            {
                try
                {

                    string ImageStr = "";
                    string NoDetail = "?";
                    bool ok = false;
                    //MessageBox.Show("OK");
                    int index = 0;
                    object itemCollection = null;
                    int ItemsCount = 0;
                    Dispatcher.Invoke(() =>
                    {
                        index = DataGrid1.SelectedIndex;
                        itemCollection = DataGrid1.Items[index];
                        ItemsCount = DataGrid1.Items.Count - 1;
                    });
                    if (index < ItemsCount && (itemCollection) != DBNull.Value)
                    {
                        string ImagePath = "";
                        Dispatcher.Invoke(() => { ImagePath = "Image\\" + (string)((DataRowView)DataGrid1.Items[index])[2] + ".png"; });
                        if (File.Exists(ImagePath))
                        {
                            ok = true;
                            Dispatcher.Invoke(() => { NoDetail = (string)((DataRowView)DataGrid1.Items[index])[2]; });
                            ImageStr = "1";
                        }
                    }
                    if (!ok)
                    {
                        foreach (Item item in classes.Item)
                        {
                            int SelectedIndex = 0;
                            Dispatcher.Invoke(() => { SelectedIndex = DataGrid1.SelectedIndex; });
                            if (SelectedIndex != -1)
                            {
                                index = SelectedIndex;
                                int ItemsCount1 = 0;
                                object Item = null;
                                string ItemCode = "";
                                Dispatcher.Invoke(() =>
                                {
                                    ItemsCount1 = DataGrid1.Items.Count - 1;
                                    Item = DataGrid1.Items[index];
                                    ItemCode = (string)((DataRowView)DataGrid1.Items[index])[2];
                                });
                                if (index < ItemsCount1 && (Item) != DBNull.Value && ItemCode == item.ItemCode)
                                {
                                    if (item.Image != null)
                                    {
                                        ImageStr = item.Image;
                                        NoDetail = item.ItemCode;
                                        break;
                                    }
                                }
                            }

                        }
                    }

                    BitmapImage bitmap;
                    Dispatcher.Invoke(() => { _imgDetail.Source = null; });
                    string FullPath = "";
                    if (ImageStr != "")
                    {
                       FullPath = System.IO.Path.GetFullPath("Image\\" + NoDetail + ".png"); ;

                        if (ok) { }
                        else
                        {
                            using (var image = Draw.Image.FromStream(new MemoryStream(Convert.FromBase64String(ImageStr))))
                            {
                                image.Save("Image\\" + NoDetail + ".png", Draw.Imaging.ImageFormat.Png);
                            }
                        }
                    }
                    else
                    {
                        FullPath =System.IO.Path.GetFullPath("Default\\Default.png"); 
                    }
                    Dispatcher.Invoke(() => { bitmap = new BitmapImage(new Uri(FullPath)); _imgDetail.Source = bitmap; });

                }
                catch (Exception ex)
                {

                    //MessageBox.Show(ex.Message);
                }
            });


        }
        //Откриваем окно для изменения данных в базе елементов
        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            Edit_Item edit = new Edit_Item();
            edit.classes = classes;
            edit.ShowDialog();
            Restart();
            
        }
        //Откриваем окно для отображения сделаных заказов
        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            Raport raport = new Raport();
            raport.classes = classes;
            raport.ShowDialog();
        }
        //Обработка нажатия f3 для удобного просмотра изображения
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.F3)
                {
                    if (DataGrid1.SelectedItem != null)
                    {
                        Task.Run(() => {
                            int SelectedIndex = 0;
                            Dispatcher.Invoke(() => { SelectedIndex = DataGrid1.SelectedIndex; });
                            string ItemCode = (string)((DataRowView)DataGrid1.Items[SelectedIndex])[2];
                            string path = Path.GetFullPath("Image\\" + ItemCode + ".png");
                            if (File.Exists(path))
                            {
                                using(var proc = new System.Diagnostics.Process())
                                {
                                    proc.StartInfo.FileName = path;
                                    proc.StartInfo.UseShellExecute = true;
                                    proc.Start();
                                }
                               
                            }


                        });
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                throw;
            }
            
            
            
            
        }

        private void _ckboxShowFinished_Checked(object sender, RoutedEventArgs e)
        {

        }

        
        //Обработка генерации колонок и замена формата даты на привычний нам
        private void DataGrid1_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyType == typeof(System.DateTime))
                (e.Column as DataGridTextColumn).Binding.StringFormat = "dd/MM/yyyy";
        }

        private void Button_Click_7(object sender, RoutedEventArgs e)
        {
            AddCompany addCompany = new AddCompany(classes, connectionString);
            addCompany.ShowDialog();
            Restart();

        }

        private void _btnEditCompany_Click(object sender, RoutedEventArgs e)
        {
            EditCompany editCompany=new EditCompany(classes, connectionString);
            editCompany.ShowDialog();
            Restart();
        }





        //Обработки кнопки Wydruk WZ для розпечатки активных заказов
        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                try
                {
                    int startIndex = 9;
                    int countPrint = 0;
                    using (_ExelDoc doc = new _ExelDoc())
                    {
                        doc.Open(@"doc\base.xls");
                        doc.Set("C", 29, DateTime.Now.ToShortDateString());
                        int i = -1;
                        int j = 0;
                        Dispatcher.Invoke(() => { i = DataGrid1.Items.Count - 2; });



                        foreach (DataRowView item in DataGrid1.Items)
                        {
                            if ((bool)item.Row[0])
                            {

                                foreach (ItemOrder item1 in classes.ItemOrders)
                                {
                                    if (item1.Id == (int)item.Row[1] && item1.Finished == false)
                                    {
                                        item1.PrintList = true;
                                        countPrint++;
                                    }
                                }
                            }

                            j++;
                            if (j > i)
                            {
                                break;
                            }
                        }
                        string CompanyName="";
                        string Address = "";
                        string NIP = "";
                        if (countPrint > 0)
                        {

                            foreach (ItemOrder item in classes.ItemOrders)
                            {
                                if (item.PrintList == true && item.Finished == false)
                                {
                                    if (startIndex==9)
                                    {
                                       CompanyName = item.Company.Name;
                                        Address = item.Company.Addres;
                                        NIP = item.Company.NIP.ToString();
                                    }
                                    else
                                    {
                                        if (item.Company.Name!=CompanyName)
                                        {
                                            MessageBox.Show("Aby wydrukować ten dokument, musisz wybrać zamówienie od jednej firmy");
                                            classes.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues,classes.ItemOrders);
                                            return;
                                        }
                                    }
                                    
                                    string NoId = "";
                                    int i1 = 0;
                                    int buffer = item.Id;
                                    int normalCount = 6;
                                    for (; buffer > 0; i1++)
                                    {
                                        buffer /= 10;
                                    }
                                    normalCount -= i1;
                                    for (int j1 = 0; j1 < normalCount; j1++)
                                    {
                                        NoId += "0";
                                    }

                                    NoId += item.Id;
                                    doc.Set("B", startIndex, NoId);
                                    doc.Set("C", startIndex, item.Item.ItemCode);
                                    doc.Set("D", startIndex, item.Item.Size.ToString());
                                    doc.Set("E", startIndex, item.CountOut.ToString());
                                    doc.Set("G", startIndex, item.Description.ToString());
                                    startIndex++;
                                }

                            }
                        }
                        else
                        {
                            MessageBox.Show("Ten dokument jest pusty, wybierz aktywne zamówienia");
                        }
                        doc.Set("D", 2, CompanyName);
                        doc.Set("D", 3, Address);
                        if (NIP!="0")
                        {
                            doc.Set("D", 4,"NIP:"+ NIP);
                        }
                        classes.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues, classes.ItemOrders);
                    }




                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    throw;
                }
            });



        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                work = new DBWork(connectionString);
                work.DataGrid1 = DataGrid1;
                work._txtCountIn = _txtCountIn;
                work._txtDateIn = _txtDateIn;
                _txtDateIn.Text = DateTime.Now.Day.ToString() + "." + DateTime.Now.Month.ToString() + "." + DateTime.Now.Year.ToString();
                classes = new ClassesDataContext(connectionString);
                _btn_AddItem.IsEnabled = true;
                _btn_AddOrder.IsEnabled = true;
                _btn_Raport.IsEnabled = true;
                _btn_SeeAllInfo.IsEnabled = true;
                _btn_EditItem.IsEnabled = true;
                _btn_OrderFinished.IsEnabled = true;
                _btn_Print.IsEnabled = true;
                _btn_UpdateData.IsEnabled = true;
                _txtNoItem.IsEnabled = true;
                _btnAddCompany.IsEnabled = true;
                _btnEditCompany.IsEnabled = true;
                //_btn_DelAllOrders.IsEnabled = true;
                //_cmbFromWhom.ItemsSource = classes.Companies.Select(x=>x.ShortName);
                work.ShowAllDataInItemOrders();
                string dir = @"Image"; // If directory does not exist, create it.
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                else
                {
                    DirectoryInfo dirInfo = new DirectoryInfo(dir);

                    foreach (FileInfo file in dirInfo.GetFiles())
                    {
                        file.Delete();
                    }
                    Directory.Delete(dir);
                    Directory.CreateDirectory(dir);
                }
                //DataGrid1.IsReadOnly = true;
            }
            catch (Exception ex)
            {

                //MessageBox.Show(ex.Message);
            }
        }

        private void _cmbSearchNo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            classes.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues);
            List<string> noItem_Date = _cmbSearchNo.SelectedItem.ToString().Split('_').AsList();
            int i = 0;
            List<int> del=new List<int>();
            noItem_Date.ForEach(item => {
                if (string.IsNullOrEmpty(item))
                {
                    del.Add(i);
                }
                i++;
            }
            );
            noItem_Date.RemoveRange(1, del.Count);
           // noItem_Date.ForEach(item => item= item.Substring(0,item.Length-2));
           //string a= DateTime.Now.ToString();
            foreach(var item in classes.ItemOrders){
                if (item.Item.ItemCode==noItem_Date[0])
                {
                    if (item.DateOut.Date.ToShortDateString() == noItem_Date[1])
                    {
                        selectedId = item.Id;
                        return;
                    }
                }
                
            }
           //selectedId = classes.ItemOrders.Select(x => x).Where(x => x.Item.ItemCode == noItem_Date[0]).Where(x => x.DateOut.ToString() == noItem_Date[1]).First().Id; ????
        }

        private void _btn_DelAllOrders_Click(object sender, RoutedEventArgs e)
        {
            work.DeleteAllOrdersInDB();
            Restart();
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F12)
            {
                _btn_DelAllOrders.IsEnabled = true;
            }
        }

        public void Restart()
        {
            MessageBox.Show("Do poprawnej pracy należy ponownie uruchomić aplikację");
            var proc = new System.Diagnostics.Process();
            proc.StartInfo.FileName = "program.exe";
            proc.StartInfo.UseShellExecute = true;
            proc.Start();
            this.Close();
        }
    }
}
