using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace program
{
    class _Raport
    {
        public string _NoItem { get; set; }
        public int _TotalCountIn { get; set; }
        public double _TotalSurfface { get; set; }
        public _Raport(string noitem,int count,double surfface) { _NoItem = noitem;_TotalCountIn = count;_TotalSurfface = surfface; }
        //Функция для заполнения таблици рапорта
        public DataRow SetRow(DataTable dt)
        {
            DataRow row = dt.NewRow();
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                if (i==0)
                {
                    row[i] = _NoItem;
                }
                else
                {
                    if (i==1)
                    {
                        row[i] = _TotalCountIn;
                    }
                    else
                    {
                        if (i==2)
                        {
                            row[i] = _TotalSurfface;
                        }
                    }
                }
                
            }
            return row;
        }


    }
}
