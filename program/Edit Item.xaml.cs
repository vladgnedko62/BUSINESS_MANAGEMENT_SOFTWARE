using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Draw = System.Drawing;
using Dapper.Contrib;
using Dapper;
using WF= System.Windows.Forms;

namespace program
{
    /// <summary>
    /// Логика взаимодействия для Edit_Item.xaml
    /// </summary>
    public partial class Edit_Item : Window
    {
        public ClassesDataContext classes;
       
        static string connectionString = ConfigurationManager.ConnectionStrings["MyConnection1"].ToString();
        public string ImageInString { get; private set; }

        public Edit_Item()
        {
            InitializeComponent();
        }
        //Обработка собития TextChanged для поиска  елементов в базе 
        private void _txtNoItem_TextChanged(object sender, TextChangedEventArgs e)
        {

            //Task.Run(() =>
            //{
            //    string Text = "";
            //    Dispatcher.Invoke(() => { Text = _txtNoItem.Text; _cmbSearchNo.Items.Clear(); /*_cmbFromWhom.ItemsSource = null; _cmbFromWhom.Items.Clear();*/ });
            //    SqlConnection sqlConnection = new SqlConnection(connectionString);
            //    lock (sqlConnection)
            //    {
            //        List<Item> items = sqlConnection.QueryAsync<Item>("select * from Item").Result.Select(x => x).Where(x => x.ItemCode.Contains(Text)).ToList();
            //        double Size = 0;
            //        Size = items[0].Size;
            //        foreach (var item in items)
            //        {
            //            Dispatcher.Invoke(() => { _cmbSearchNo.Items.Add(item.ItemCode); });
            //        }


            //        Dispatcher.Invoke(() => { _txtSize.Text = Size.ToString(); _txtNewNoDetal.Text = items[0].ItemCode; _cmbSearchNo.SelectedIndex = 0; });




            //    }
            //});
            classes.Refresh(System.Data.Linq.RefreshMode.KeepChanges);
            _cmbSearchNo.ItemsSource = null;
            List<Item> item = classes.Item.Select(x => x).Where(x => x.ItemCode.Contains(_txtNoItem.Text)).AsList();
            _cmbSearchNo.ItemsSource=item.Select(x=>x.ItemCode);
            _cmbSearchNo.SelectedIndex = 0;




        }
        //функция для загрузки фото текущего выбраного елемента
        public void ImageConverter()
        {
            try
            {

                string ImageStr = "";
                string NoDetail = "?";
                bool ok = false;
                //MessageBox.Show("OK");
                int index = -1;
                string SelectedItem = "";
                lock (classes)
                {
                    foreach (Item item in classes.Item)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            if (_cmbSearchNo.SelectedItem!=null)
                            {
                                SelectedItem = _cmbSearchNo.SelectedItem.ToString();
                            }
                            else
                            {
                                SelectedItem="";
                            }
                        });

                        if (item.ItemCode == SelectedItem)
                        {
                            NoDetail = item.ItemCode;
                            ImageInString = ImageStr = item.Image;
                            ok = true; break;
                        }
                    }
                    if (ok)
                    {
                        BitmapImage bitmap;
                        Dispatcher.Invoke(() => { _imgDetail.Source = null; });

                        string FullPath = "";
                        if (ImageStr != "")
                        {
                            FullPath = System.IO.Path.GetFullPath("Image\\" + NoDetail + ".png"); ;
                            var image = Draw.Image.FromStream(new MemoryStream(Convert.FromBase64String(ImageStr)));
                            if (File.Exists("Image\\" + NoDetail + ".png"))
                            {
                                //File.Delete(FullPath);
                                //image.Save(FullPath, Draw.Imaging.ImageFormat.Png);
                            }
                            else
                            {
                                image.Save("Image\\" + NoDetail + ".png", Draw.Imaging.ImageFormat.Png);
                            }
                        }
                        else
                        {
                            FullPath = System.IO.Path.GetFullPath("Default\\Default.png");
                        }
                        Dispatcher.Invoke(() => { bitmap = new BitmapImage(new Uri(FullPath)); _imgDetail.Source = bitmap; });
                    }
                }
                
                
              

            }
            catch (Exception ex)
            {

                //MessageBox.Show(ex.Message);
            }

        }
        //Вибераем фото или pdf документ для елемента
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            open.Filter = "Obraz|*.png;*.jpg;*.pdf;";
            open.Title = "Wybierz obraz";
            string FullPath = null;
            if (open.ShowDialog() == true)
            {
                if (open.FileName.IndexOf(".pdf") == -1)
                {
                    BitmapImage bitmap = new BitmapImage(new Uri(open.FileName));
                    _imgDetail.Source = bitmap;
                    ImageInString = Convert.ToBase64String(File.ReadAllBytes(open.FileName));
                    _txtboxImagePath.Text = open.FileName;
                }
                else
                {

                    //MessageBox.Show(Directory.GetCurrentDirectory() + "\\");
                    FullPath = PdfToPng.Convert(open.FileName, open.SafeFileName, Directory.GetCurrentDirectory() + "\\");
                    if (FullPath != "")
                    {
                        BitmapImage bitmap = new BitmapImage(new Uri(FullPath));
                        _imgDetail.Source = bitmap;
                        ImageInString = Convert.ToBase64String(File.ReadAllBytes(FullPath));
                        _txtboxImagePath.Text = FullPath;
                    }

                }
            }
        }
        //Обработка кнопки Zapisać для сохранения обновлённой информации в бд
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                double size=0;
                SqlConnection sql = new SqlConnection(ConfigurationManager.ConnectionStrings["MyConnection"].ToString());

                string query="";
                bool ok = false;
                SqlCommand command=null;
                foreach (Item item in classes.Item)
                {
                    if (item.ItemCode == _cmbSearchNo.SelectedItem.ToString())
                    {
                       
                        if (!double.TryParse(_txtSize.Text,out size))
                        {
                            _txtSize.BorderBrush =System.Windows.Media.Brushes.Red;
                            MessageBox.Show("Zły rozmiar");
                            return;
                        }
                        query = "UPDATE Item SET ItemCode='" + _txtNewNoDetal.Text + "', Image='" + ImageInString + "',Size='" + size.ToString().Replace(',','.') + "'" +
                    "where ItemCode='" + _cmbSearchNo.SelectedItem.ToString() + "'";
                        command = new SqlCommand(query, sql);
                        sql.Open();
                        ok = true;
                        break;
                        
                    }
                }
                if (ok)
                {
                    command.ExecuteNonQuery();
                    MessageBox.Show("Zapisane!");
                }
               
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                
            }


        }

        private void _cmbSearchNo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Task.Run(()=> {

                double Size = 0;
                string NeNoItem = "Error";
                string SelectedItem = "&&&";
                Dispatcher.Invoke(() => { if (_cmbSearchNo.SelectedItem != null) { SelectedItem = _cmbSearchNo.SelectedItem.ToString(); } });
                Item item = classes.Item.Select(x => x).Where(x => x.ItemCode == SelectedItem).FirstOrDefault();
                if (item != null) { Size = item.Size;NeNoItem = item.ItemCode; }
                Dispatcher.Invoke(() => {
                    _txtNewNoDetal.Text = NeNoItem;
                    _txtSize.Text = Size.ToString();
                });
                
                
                
                
                
                ImageConverter();
            });
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.F3)
                {
                    if (_cmbSearchNo.SelectedItem != null)
                    {
                        Task.Run(() => {
                            string SelectedIndex = "";
                            Dispatcher.Invoke(() => { SelectedIndex = _cmbSearchNo.SelectedItem.ToString(); });
                            string ItemCode = SelectedIndex;
                            string path = Path.GetFullPath("Image\\" + ItemCode + ".png");
                            if (File.Exists(path))
                            {
                                var proc = new System.Diagnostics.Process();
                                proc.StartInfo.FileName = path;
                                proc.StartInfo.UseShellExecute = true;
                                proc.Start();
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

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_cmbSearchNo.SelectedItem!=null)
                {
                    WF.DialogResult dialogResult = WF.MessageBox.Show("Po usunięciu części wszystkie zamówienia, które dotyczyły tej części, zostaną usunięte. Pewnie?", "!!!", WF.MessageBoxButtons.YesNo);
                    if (dialogResult == WF.DialogResult.Yes)
                    {
                        SqlConnection sql = new SqlConnection(connectionString);
                        Item item = classes.Item.Select(x => x).Where(x => x.ItemCode == _cmbSearchNo.SelectedItem.ToString()).FirstOrDefault();
                        sql.Execute("delete from InnerOrders where ItemId=@ii", new { ii = item.Id });
                        sql.Execute("delete from ItemOrders where ItemId=@ii", new { ii = item.Id });
                        sql.Execute("delete from Item where ItemCode=@ic", new { ic = _cmbSearchNo.SelectedItem.ToString() });
                        MessageBox.Show("Usunięty!");
                    }
                   
                }
                
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }
    }
}
