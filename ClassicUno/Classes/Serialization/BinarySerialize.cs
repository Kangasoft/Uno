/* Classic Uno 2026
 * Version: 1.0
 * By: Jason James Newland
 * ©2026 - Kangasoft Software */
using System.IO;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;

namespace ClassicUno.Classes.Serialization
{
    public class BinarySerialize<TType>
    {
        /* Serialization class
           By: Jason James Newland and Adam Oldham
           ©2011 - KangaSoft Software
           All Rights Reserved
         */
        public static bool Load(string fileName, ref TType cObject)
        {
            var fi = new FileInfo(fileName);
            if (!fi.Exists || fi.Length == 0) { return false; }
            TType cRet;
            bool success;
            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                try
                {
                    var bf = new BinaryFormatter
                    {
                        AssemblyFormat = FormatterAssemblyStyle.Simple
                    };
                    cRet = (TType)bf.Deserialize(fs);
                    success = true;
                }
                catch
                {
                    cRet = default(TType);
                    success = false;
                }
                finally { fs.Close(); }
            }
            cObject = cRet;
            return success;
        }

        public static bool Save(string fileName, TType cObject)
        {
            if (cObject.Equals(null)) { return false; }
            bool success;
            using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
            {
                try
                {
                    var bf = new BinaryFormatter
                    {
                        AssemblyFormat = FormatterAssemblyStyle.Simple
                    };
                    bf.Serialize(fs, cObject);
                    success = true;
                }
                catch
                {
                    success = false;
                }
                finally
                {
                    fs.Close();
                }
            }

            return success;
        }
    }
}