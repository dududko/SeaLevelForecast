using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FloodForecasting
{
    public static class Utils
    {
        public static DateTime GetValueAsDateTime(
      IniConfig config,
      string sectionName,
      string paramName,
      bool mandatoryParam,
      DateTime defaultValue)
        {
            string resultStr;
            if (!config.TryGetValue(sectionName, paramName, out resultStr))
            {
                if (mandatoryParam)
                {
                    /*throw new ConveyorException(string.Format(CultureInfo.InvariantCulture,
                      "Parameter \"[{0}] {1}\" not specified.", sectionName, paramName));*/
                }
                return defaultValue;
            }

            DateTime result;
            if (
              !DateTime.TryParseExact(resultStr, "yyyyMMddHH", CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
            {
                /*throw new ConveyorException(string.Format(CultureInfo.InvariantCulture,
                  "Unexpected value \"[{0}] {1}\"={2}. DateTime value (yyyyMMddHH) is expected.", sectionName, paramName,
                  resultStr));*/
            }

            return result;
        }

        public static string GetValueAsPath(
      IniConfig config,
      string sectionName,
      string paramName,
      bool mandatoryParam,
      string defaultValue)
        {
            string result;
            if (!config.TryGetValue(sectionName, paramName, out result))
            {
                if (mandatoryParam)
                {
                    /*throw new ConveyorException(string.Format(CultureInfo.InvariantCulture,
                      "Parameter \"[{0}] {1}\" not specified.", sectionName, paramName));*/
                }
                return defaultValue;
            }

            result = result.Trim('"');

            try
            {
                // ReSharper disable once ObjectCreationAsStatement
                new FileInfo(result);
            }
            catch
            {
                if (mandatoryParam)
                {
                    /*throw new ConveyorException(string.Format(CultureInfo.InvariantCulture,
                      "Unexpected value \"[{0}] {1}\"={2}. Correct path is expected.", sectionName, paramName, result));*/
                }
                return defaultValue;
            }

            return Path.GetFullPath(result);
        }

        public static int GetValueAsInt(
      IniConfig config,
      string sectionName,
      string paramName,
      bool mandatoryParam,
      int defaultValue)
        {
            string resultStr;
            if (!config.TryGetValue(sectionName, paramName, out resultStr))
            {
                if (mandatoryParam)
                {
                    /*throw new ConveyorException(string.Format(CultureInfo.InvariantCulture,
                      "Parameter \"[{0}] {1}\" not specified.", sectionName, paramName));*/
                }
                return defaultValue;
            }

            int result;
            if (!int.TryParse(resultStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out result))
            {
                /*throw new ConveyorException(string.Format(CultureInfo.InvariantCulture,
                  "Unexpected value \"[{0}] {1}\"={2}. Integer value is expected.", sectionName, paramName, resultStr));*/
            }

            return result;
        }

        public static void Empty(string path)
        {
            System.IO.DirectoryInfo directory = new System.IO.DirectoryInfo(path);

            foreach (System.IO.FileInfo file in directory.GetFiles()) file.Delete();
            foreach (System.IO.DirectoryInfo subDirectory in directory.GetDirectories()) subDirectory.Delete(true);
        }
    }
}
