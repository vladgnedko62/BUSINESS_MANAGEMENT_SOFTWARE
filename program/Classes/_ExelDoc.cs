using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Exel = Microsoft.Office.Interop.Excel;

namespace program
{
    public class _ExelDoc:IDisposable
    {
        private Workbook book;
        private Exel.Application _exel;
        private string Path;
        public _ExelDoc() { _exel = new Exel.Application(); }
        //Откриваем файл для записи
        public bool Open(string Path)
        {
            try
            {
               string fullpathbase= System.IO.Path.GetFullPath(Path);
                string fullpathbase1 = System.IO.Path.GetFullPath(Path.Insert(Path.IndexOf('.'),"1"));
                if (File.Exists(fullpathbase1))
                    {
                        File.Delete(fullpathbase1);
                    File.Copy(fullpathbase, fullpathbase1);
                   this.Path = fullpathbase1;
                }
                else
                {
                    File.Copy(fullpathbase, fullpathbase1);
                   this.Path = fullpathbase1;
                }


                book = _exel.Workbooks.Open(this.Path);
                  

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return false;
        }
        public void Save()
        {
            book.Save();
        }
        //Завершение роботи и откритие файла
        public void Dispose()
        {
            book.Save();
            book.Close();
            string commandText = Path;
            var proc = new System.Diagnostics.Process();
            proc.StartInfo.FileName = commandText;
            proc.StartInfo.UseShellExecute = true;
            proc.Start();
        }
        //Заполнение таблици
        public void Set(string col,int row,string Data)
        {
            ((Exel.Worksheet)_exel.ActiveSheet).Cells[row, col] = Data;
        }
    }
}
