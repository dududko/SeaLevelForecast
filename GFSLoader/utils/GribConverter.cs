using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GFSConverter;

namespace FloodForecasting.utils
{
    class GribConverter
    {
        /***
         * @inputDir - путиь к директории с входными данными
         * @outputDir - путь к директории с выходными данными 
         */
        static public void ConvertToGrib1(String inputDir, String outputDir, String[] files)
        {
            // Загрузка списка входных файлов.
            

            // Блок обработки исключений, возникающих при создании экземпляров классов библиотеки и запуске процедур конвертации.
            try
            {
                // Создание экземпляра конвертера с заданием максимально допустимого числа одновременных процессов.
                GFSConverter.Converter converter = new GFSConverter.Converter(8);

                // Подписка на событие об окончании конвертирования файла.
                converter.OnFinish += new EventHandler<GFSConverter.ConverterEvent>(FileConvertedHandler);

                foreach (string f in files)
                {
                    string file = inputDir + f;
                    // Запуск конвертирования всех файлов без ожидания окончания конвертации очередного файла.
                    converter.Convert(file, Path.Combine(outputDir, Path.GetFileName(file)), false);
                }

                // Ожидание окончания конвертирования всех файлов.
                converter.WaitForAll();
            }
            catch (ConverterException e)
            {
                Console.WriteLine(string.Format("[ERROR] {0}", e.Message));
            }
        }

        // Обработка сообщений об окончании конвертации файла.
        private static void FileConvertedHandler(object sender, GFSConverter.ConverterEvent e)
        {
            if (e.Status == GFSConverter.ConverterEvent.ResultStatus.OK)
            {
                Console.WriteLine(string.Format("[INFO] File {0} converted and saved as {1}", e.InputFile, e.OutputFile));
            }
            else if (e.Status == GFSConverter.ConverterEvent.ResultStatus.ERROR)
            {
                Console.WriteLine(string.Format("[ERROR] {0}", e.Exception.Message));
            }
        }
    }
}
