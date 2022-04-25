using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tech_library.Tools
{
    public static class UtilMgr
    {
        /// <summary>
        /// unixtimestamp ==> datetime 으로 변환
        /// </summary>
        /// <param name="unixTimeStamp">unixTimeStamp</param>
        /// <returns>datetime</returns>
        public static DateTime UnixtimeStamptoDateTime(double unixTimeStamp)
        {
            System.DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dt = dt.AddSeconds(unixTimeStamp).ToLocalTime();
            return dt;
        }

        public static Int32 DateTimetoUnixtimeStamp()
        {
            Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            return unixTimestamp;
        }
        /// <summary>
        /// datetime string ==> formatted datetime 으로 변환
        /// </summary>
        /// <param name="strtime">string typed datetime</param>
        /// <param name="formatted">format = "yyyy-MM-dd HH:mm:ss.fff"</param>
        /// <returns>datetime</returns>
        public static DateTime StringToDatetimeInFormated(string strtime, string formatted)
        {
            return DateTime.ParseExact(strtime, formatted, null);
        }

        /// <summary>
        /// 이미 split 된 string [] 에서 key 를 포함하는 string 을 서치
        /// </summary>
        /// <param name="words">RV 에서 수신한 문자열을 공백으로 구분한 string 배열</param>
        /// <param name="key">words 에서 찾을 키 문자열</param>
        /// <returns>key 를 포함하는 List형 문자열 or null</returns>
        public static List<string> FindString(string[] words, string key)
        {
            var items = from a in words
                        where a.ToUpperInvariant().Contains(key)
                        select a;
            if (items.Count() > 0)
                return items.ToList();
            return null;
        }
    }
}
