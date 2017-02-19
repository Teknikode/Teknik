using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Teknik.Utilities
{
    public static class CurrencyHelper
    {
        private static decimal m_CurrentBTCPrice = 0;
        private static DateTime m_LastBTCQuery = DateTime.MinValue;
        private static TimeSpan m_MaxBTCQueryTime = new TimeSpan(1, 0, 0); // Max query of every hour

        /// <summary>
        /// Gets the exchange rate for a given currency relative to USD
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static decimal GetExchangeRate(CurrencyType target)
        {
            decimal exchangeRate = 1;
            switch (target)
            {
                case CurrencyType.USD:
                    exchangeRate = 1;
                    break;
                case CurrencyType.BTC:
                    // get BTC usd price
                    exchangeRate = GetBTCPrice();
                    break;
            }

            return exchangeRate;
        }

        public static decimal GetBTCPrice()
        {
            DateTime curTime = DateTime.Now;
            if (curTime - m_LastBTCQuery > m_MaxBTCQueryTime)
            {
                m_LastBTCQuery = curTime;
                string url = "http://api.bitcoincharts.com/v1/weighted_prices.json";
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                try
                {
                    WebResponse response = request.GetResponse();
                    using (Stream responseStream = response.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                        string jsonResult = reader.ReadToEnd();
                        JObject result = JsonConvert.DeserializeObject<JObject>(jsonResult);
                        if (result["USD"] != null)
                        {
                            string priceStr = result["USD"]["24h"].ToString();
                            decimal.TryParse(priceStr, out m_CurrentBTCPrice);
                        }
                    }
                }
                catch (WebException ex)
                {
                    WebResponse errorResponse = ex.Response;
                    using (Stream responseStream = errorResponse.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(responseStream, Encoding.GetEncoding("utf-8"));
                        String errorText = reader.ReadToEnd();
                    }
                }
            }

            return m_CurrentBTCPrice;
        }
    }
}
