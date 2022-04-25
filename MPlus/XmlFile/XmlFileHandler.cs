using System;
using System.Diagnostics;
using System.IO;
using System.Xml.Serialization;

namespace FSMPlus
{
    public class XmlFileHandler
    {

        public static bool ConfigSave<T>(string FilePath, ref T src)
        {
            // xml 형태로 파일 기록
            try
            {
                FileStream fs3 = new FileStream(FilePath, FileMode.Create, FileAccess.Write);
                XmlSerializer xs = new XmlSerializer(src.GetType());
                xs.Serialize(fs3, src);
                fs3.Close();
            }
            catch (Exception e)
            {
                Debug.Assert(false, "ConfigFile Save Error.\r\n" + e.ToString());
            }
            return true;
        }

        public static bool ConfigLoad<T>(string FilePath, ref T dst)
        {
            if (!File.Exists(FilePath))
            {
                return false;
            }

            // xml 형태로 파일 읽기
            try
            {
                FileStream fs4 = new FileStream(FilePath, FileMode.Open, FileAccess.Read);
                XmlSerializer xs2 = new XmlSerializer(dst.GetType());
                //var temp = (T)xs2.Deserialize(fs4);
                dst = (T)xs2.Deserialize(fs4);
                fs4.Close();

                //GlobalData.Inst.DataDeepCopy(temp);
            }
            catch (Exception e)
            {
                Debug.Assert(false, "ConfigFile Load Error.\r\n" + e.ToString());
            }
            return true;
        }



    }
}
