using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
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
using System.Windows.Shapes;
using Draw= System.Drawing;

namespace program
{
    /// <summary>
    /// Логика взаимодействия для AddInDB.xaml
    /// </summary>
    public partial class AddInDB : Window
    {
        ClassesDataContext classes;
        private static string connectionString = ConfigurationManager.ConnectionStrings["MyConnection1"].ToString();
        private string ImageInString;
        private SqlDataAdapter adapter;

        public AddInDB()
        {
            InitializeComponent();
        }
        //Кнопка добавления
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                double size;
                if (double.TryParse(_txtSize.Text, out size))
                {
                    SqlConnection sql = new SqlConnection(connectionString);
                    sql.Open();
                    string com = "INSERT INTO Item (ItemCode,Size,Image) values('" + _txtNoItem.Text + "','" + size.ToString().Replace(',', '.') + "','" + ImageInString + "')";
                    SqlCommand sqlCommand = new SqlCommand(com, sql);
                    sqlCommand.ExecuteNonQuery();
                    ShowAllInfoInItem();
                    //MessageBox.Show("Zapisano!");
                }
                else
                {
                    _txtSize.BorderBrush = Brushes.Red;
                    MessageBox.Show("Zły rozmiar");
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
           
            
        }
        //Кнопка выбора картинки
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            open.Filter = "Obraz|*.png;*.jpg;*.pdf";
            open.Title = "Wybierz obraz";
            string FullPath;
            if (open.ShowDialog()==true)
            {
               
                if (open.FileName.IndexOf(".pdf")==-1)
                {
                    BitmapImage bitmap = new BitmapImage(new Uri(open.FileName));
                    _imgDetail.Source = bitmap;
                    ImageInString = Convert.ToBase64String(File.ReadAllBytes(open.FileName));
                    _txtboxImagePath.Text = open.FileName;
                }
                else
                {
                   
                    //MessageBox.Show(Directory.GetCurrentDirectory() + "\\");
                   FullPath= PdfToPng.Convert(open.FileName,open.SafeFileName, Directory.GetCurrentDirectory()+"\\");
                    if (FullPath!="")
                    {
                        BitmapImage bitmap = new BitmapImage(new Uri(FullPath));
                        _imgDetail.Source = bitmap;
                        ImageInString = Convert.ToBase64String(File.ReadAllBytes(FullPath));
                        _txtboxImagePath.Text = FullPath;
                    }
                   
                }
                
                //MessageBox.Show(ImageInString);
            } 
        }
        //Заполнения таблицы
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            ShowAllInfoInItem();
        }

        private void _txtSize_TextChanged(object sender, TextChangedEventArgs e)
        {
            _txtSize.BorderBrush = Brushes.Silver;
        }
        //Заполнения таблицы
        private void ShowAllInfoInItem()
        {
            try
            {
                string command = "select Id,ItemCode as Kod," +
                  "Size as dm," +
                   "Image as [Kod obrazu]" +
                   "from Item";
                SqlConnection connection = new SqlConnection(connectionString);
                connection.Open();
                adapter = new SqlDataAdapter(command, connection);
                //DataGrid1.ItemsSource = Source;
                SqlCommandBuilder builder = new SqlCommandBuilder(adapter);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                _gridForItems.Items.Refresh();
                _gridForItems.ItemsSource = dt.DefaultView;
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        
    }
}
