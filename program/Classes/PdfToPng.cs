using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using SautinSoft;
namespace program
{
    internal static class PdfToPng
    {
        //Функция для конвертирования png to jpg
        public static string Convert(string sourcePath,string FileName,string targetFolder)
        {
            try
            {
                if (!File.Exists(sourcePath))
                {
                    MessageBox.Show("File error");
                    return "";
                }
                PdfFocus f = new PdfFocus();
                f.OpenPdf(sourcePath);
                if (f.PageCount>0)
                {
                    f.ImageOptions.Dpi = 300;
                    System.Drawing.Image image= f.ToDrawingImage(1);
                    Rectangle cropRect = new Rectangle(0,100,image.Width,image.Height+100);
                    Bitmap src = (Bitmap)image;
                    Bitmap target = new Bitmap(cropRect.Width, cropRect.Height);

                    using (Graphics g = Graphics.FromImage(target))
                    {
                        g.DrawImage(src, new Rectangle(0, 0, target.Width, target.Height),
                                         cropRect,
                                         GraphicsUnit.Pixel);
                    }

                    Image image1 = target;
                    string SavedPath = targetFolder + FileName.Split('.')[0] + ".png";
                    if (File.Exists(SavedPath))
                    {
                        File.Delete(SavedPath);
                    }
                    image1.Save(SavedPath);
                    return SavedPath;
                }
                
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return "";
        }
    }
}
