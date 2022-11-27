using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace program
{
    public class DBWork:MainWindow
    {
       private SqlConnection conn = null;
       private ClassesDataContext classes;
       private SqlDataAdapter adapter;
        public bool ShowFinished = false;
        private DataTable table;

        public DBWork(string connectionString) { conn = new SqlConnection(connectionString); classes = new ClassesDataContext(connectionString); }
        //Подключаемся к базе
        public void Connect()
        {
            try
            {
                conn.Open();
            }
            catch (Exception a)
            {
                //WriteLine("" + a.Message);
                if (conn != null) conn.Close();

                return;
            }
            //WriteLine("Connection OK\nBG------------------------------------------------------------------------------------------------------------");
        }
        //Отключаемся от базы
        public void Dissconect()
        {
            if (conn != null) conn.Close();
           /* WriteLine("----------------------------------------------------------------------------------------------------------------EN\nDissconect OK\n");*/
        }
        //Заполняем главную таблицу
        public void ShowAllDataInItemOrders()
        {

            try
            {
                //if (reader != null) reader.Close();
                //string command = "select ItemId as Id,ItemCode as Kod," +
                //    "Size as dm," +
                //    "Image as [Kod obrazu]," +
                //    "DateOut as [Data wyjazdu]," +
                //    "CountOut as [Ile wyślij]," +
                //    "DateIn as [Data powrotu]" +
                //    ",CountIn as [Ilu przyszło]," +
                //    "CountFails as [Wadliwe części]," +
                //    "TotalDm as [Powierzchnia całkowita] " +
                //    "from ItemOrders,Item";
                //conn.Open();
                //cmd = new SqlCommand(command, conn);
                //table = new DataTable();
                //reader = cmd.ExecuteReader();
                //int line = 0;
                //do
                //{
                //    while (reader.Read())
                //    {
                //        if (line == 0)
                //        {
                //            for (int i = 0; i < reader.FieldCount; i++)
                //            {
                //                table.Columns.Add(reader.GetName(i));
                //            }
                //            line++;
                //        }
                //        DataRow row = table.NewRow();
                //        for (int i = 0; i <
                //        reader.FieldCount; i++)
                //        {
                //            row[i] = reader[i];
                //        }
                //        table.Rows.Add(row);
                //    }
                //} while (reader.NextResult());
                //DataGrid1.Items.Refresh();
                string command = null;
                if (ShowFinished)
                {
                    command = "select PrintList as [Print]," +
                     "IO.Id," +
                     "ItemCode as [Nazwa detalu]," +
                     "Size as dm2," +
                     "DateOut as [Data wysyłki]," +
                     "CountOut as [Wysłane]," +
                     "DateIn as [Data powrotu]" +
                     ",CountIn as [Odebrane]," +
                     "CountFails as [Brakuje]," +
                     "TotalDm as [PowCałk]," +
                     "Finished Ukończone, " +
                     "C.ShortName as [Krótkie imię], " +
                     "IO.Description as [Uwagi] "+
                     "from ItemOrders as IO,Item as I, Companies as C where I.Id=IO.ItemId AND CompanyId=C.Id  AND IO.Finished=";
                    command += "'True'";
                    command += " OR CountIn!=0 AND IO.ItemId=I.Id AND CompanyId=C.Id";
                }
                else
                {
                    command = "select PrintList as [Print]," +
                     "IO.Id," +
                     "ItemCode as [Nazwa detalu]," +
                     "Size as dm2," +
                     "DateOut as [Data wysyłki]," +
                     "CountOut-CountIn as [Wysłane]," +
                     "DateIn as [Data powrotu.]" +
                     ",CountIn as [Odebrane.]," +
                     "CountFails as [Brakuje.]," +
                     "TotalDm as [PowCałk.]," +
                     "Finished Ukończone, " +
                     "C.ShortName as [Krótkie imię], " +
                      "IO.Description as [Uwagi] " +
                     "from ItemOrders as IO,Item as I, Companies as C where I.Id=IO.ItemId AND CompanyId=C.Id  AND IO.Finished=";
                    command += "'False'";
                }
                adapter = new SqlDataAdapter(command, conn);
                //DataGrid1.ItemsSource = Source;
                SqlCommandBuilder builder = new SqlCommandBuilder(adapter);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                Dispatcher.Invoke(() =>
                {
                    DataGrid1.Items.Refresh();
                    DataGrid1.ItemsSource = dt.DefaultView;
                });


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                
            }
            finally
            {
                conn.Close();
            }
        }
        //Обновляем информацию из таблицы
        public void UpdateInfoInGrid()
        {
            try
            {
                int Id=0,CountOut,CountIn=-1;
                string NoItem = "";
                double dm = 0;
                string CodeImage = "";
                DateTime DateOut = DateTime.Now,DateIn=new DateTime(1987,1,1);
                bool Finished = false;
                string Desc = "";

                int i = -1;
                Dispatcher.Invoke(() => { i = DataGrid1.Items.Count - 2; });
                int j = 0;
                foreach (DataRowView item in DataGrid1.Items)
                {
                   
                    Id = (int)item.Row[1];
                    NoItem = (string)item.Row[2];
                    dm = (double)item.Row[3];
                    //if (item.Row[3] != DBNull.Value)
                    //{
                    //    CodeImage=(string)item.Row[3];
                    //}
                    DateOut = (DateTime)item.Row[4];
                    CountOut = (int)item.Row[5];
                    if (item.Row[6] != DBNull.Value)
                    {
                        DateIn = (DateTime)item.Row[6];
                    }
                    if (item.Row[7]!= DBNull.Value)
                    {
                        CountIn = (int)item.Row[7];
                    }
                    if (item.Row[12] != DBNull.Value)
                    {
                        Desc = (string)item.Row[12];
                    }
                    Finished = (bool)item.Row[10];
                    SearchAndUpdate(Id,NoItem,dm,CodeImage,DateOut,CountOut,DateIn,CountIn,Finished,Desc);
                    j++;
                    if (j > i)
                    {
                        break;
                    }
                  
                }
                MessageBox.Show("Wszystkie elementy zapisane");
            }
            catch (Exception ex)
            {
                MessageBox.Show("UpdateBD: " + ex.Message);
            }
           
        }
        //Обновляем один елемент
        public void SearchAndUpdate(string NoItem,DateTime DateIn,int CountIn)
        {
            foreach (ItemOrder item in classes.ItemOrders)
            {
                if (item.Item.ItemCode==NoItem)
                {
                    item.DateIn = DateIn;
                    if (CountIn!=-1)
                    {
                        item.CountIn = CountIn;
                    }
                }
            }
        }
        private void SearchAndUpdate(int ItemOrdersId,string NoItem,double dm,string ImageCode,DateTime DateOut,int CountOut,DateTime DateIn, int CountIn, bool Finished,string Desc)
        {
            foreach (ItemOrder item in classes.ItemOrders)
            {
                if (item.Id==ItemOrdersId)
                {
                    if (CountIn != -1)
                    {
                        if (!(CountIn <= item.CountOut))
                        {
                            MessageBox.Show("Liczba uzyskana w tabeli nie powinna być większa niż ta, która została wysłana");
                            return;
                        }
                        item.CountIn = CountIn; item.CountFails = CountOut - CountIn;
                    }
                    if (DateIn.Year != 1987)
                    {
                        if (!(DateIn >= item.DateOut))
                        {
                            MessageBox.Show("Data przyjazdu w tabeli nie powinna być wcześniejsza niż data wyjazdu");
                            return;

                        }
                        item.DateIn = DateIn;
                    }
                    //item.Item.Size = dm;
                    item.DateOut = DateOut;
                    item.CountOut = CountOut;
                    item.Finished = Finished;
                    if (Desc!="")
                    {
                        item.Description = Desc;
                    }
                    item.TotalDm = item.CountIn * item.Item.Size;
                    classes.SubmitChanges();
                    return;
                }
            }
        }
        //Функция для добавления заказов
        public void AddInOrders(string NoItem,DateTime DateOut,int CountOut,string CompanyShortName,string Desc)
        {
            int Id = -1;
            foreach (Item item in classes.Item)
            {
                if (item.ItemCode==NoItem) { Id = item.Id; break; }
            }
            //foreach (var item in classes.ItemOrders)
            //{
            //    if (item.Item.ItemCode==NoItem&&item.CountOut!=0&&item.Finished==false)
            //    {
            //        item.CountOut += CountOut;
            //        item.DateOut = DateOut;
            //        classes.SubmitChanges();
            //        MessageBox.Show("Zapisano!");
            //        return;
            //    }
            //}
            ItemOrder orders = new ItemOrder();
            orders.ItemId = Id;
            orders.CountIn = 0;
            orders.DateOut = DateOut;
            orders.CountOut = CountOut;
            orders.CompanyId = classes.Companies.First(x =>x.ShortName==CompanyShortName).Id;
            orders.Company = classes.Companies.First(x => x.ShortName == CompanyShortName);
            orders.Description = Desc;
            classes.ItemOrders.InsertOnSubmit(orders);
            classes.SubmitChanges();
            MessageBox.Show("Zapisano!");

        }
        //Функция при приёме заказа
        public void UpdateIfTakeDetail(int NoItem,DateTime DateIn,int CountIn,bool Finished,string CompanyShortName)
        {
            foreach (ItemOrder item in classes.ItemOrders)
            {
                if (item.Id==NoItem&&item.Finished==false&&item.Company.ShortName==CompanyShortName)
                {
                    if (!(CountIn<=item.CountOut-item.CountIn))
                    {
                        Dispatcher.Invoke(() => { _txtCountIn.BorderBrush = Brushes.Red; });
                        
                        MessageBox.Show("Liczba, która nadeszła,\n nie powinna być większa niż ta,\n która została wysłana");
                        return;
                    }
                    if (!(DateIn >= item.DateOut))
                    {
                        Dispatcher.Invoke(() => { _txtDateIn.BorderBrush = Brushes.Red; });
                       
                        MessageBox.Show("Data przyjazdu nie może być wcześniejsza niż data wyjazdu");
                        return;

                    }
                    //MessageBox.Show(DateIn.ToString());
                    if (CountIn!=0)
                    {
                        InnerOrder inner = new InnerOrder();
                        inner.ItemId = item.ItemId;
                        inner.Item = item.Item;
                        inner.CountIn = CountIn;
                        inner.DateIn = DateIn;
                        inner.OrderId = item.Id;
                        classes.InnerOrders.InsertOnSubmit(inner);
                        classes.SubmitChanges();
                    }
                    
                    item.DateIn = DateIn;
                    item.CountIn += CountIn;
                    item.CountFails = item.CountOut - item.CountIn;
                    item.Finished = Finished;
                    item.TotalDm = item.Item.Size * item.CountIn;
                    if (item.CountIn==item.CountOut)
                    {
                        item.Finished = true;
                    }
                    classes.SubmitChanges();
                    MessageBox.Show("Zapisano!");
                    return;
                }
            }
            MessageBox.Show("Wybrałeś złą firmę");
        }
        public void AddCompanyInDB(string CompanyName,string CompanyShortName,string CompanyAddres,int NIP)
        {
            Company company = new Company();
            company.Name = CompanyName;
            company.ShortName = CompanyShortName;
            company.Addres = CompanyAddres;
            company.NIP = NIP;
            classes.Companies.InsertOnSubmit(company);
            classes.SubmitChanges();
                
        }
        //public DataTable CreateTableOnDataGrid(DataGrid grid)
        //{
        //    DataTable table = new DataTable();
        //    Data
        //    for (int i = 0; i < grid.Columns.Count; i++)
        //    {

        //    }
        //}
        public void DeleteAllOrdersInDB()
        {
            classes.InnerOrders.DeleteAllOnSubmit(classes.InnerOrders);
            classes.ItemOrders.DeleteAllOnSubmit(classes.ItemOrders);
            classes.SubmitChanges();
        }

    }
}
