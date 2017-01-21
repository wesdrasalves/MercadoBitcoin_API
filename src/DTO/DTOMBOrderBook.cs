using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Dotend.MBTrade.DTO
{
    public class DTOMBOrderBook : DTOMB
    {
        public List<double[]> asks{ get; set;}
        public List<double[]> bids { get; set; }

        public DTOMBOrderBook(string pJsonData) : base(pJsonData)
        {
            this.convertJsonToObject(pJsonData);
        }

        public override IEnumerable convertJsonToObject(string pJsonData)
        {
            var _data = new JavaScriptSerializer().DeserializeObject(pJsonData);
            NumberFormatInfo _provider = new NumberFormatInfo();
            _provider.NumberDecimalSeparator = ".";
            _provider.NumberGroupSeparator = ",";

            if (_data != null)
            {
                if (((Dictionary<string, object>)_data).ContainsKey("asks") &&
                    ((Dictionary<string, object>)_data).ContainsKey("bids"))
                {
                    object[] _asks = (object[])(((Dictionary<string, object>)_data)["asks"]);
                    object[] _bids = (object[])(((Dictionary<string, object>)_data)["bids"]);

                    if (this.asks == null) this.asks = new List<double[]>();
                    if (this.bids == null) this.bids = new List<double[]>();

                    foreach (object[] _item in _asks)
                    {

                        this.asks.Add(new double[] { Convert.ToDouble(_item[0], _provider), Convert.ToDouble(_item[1], _provider) });
                    }

                    foreach (object[] _item in _bids)
                    {
                        this.bids.Add(new double[] { Convert.ToDouble(_item[0], _provider), Convert.ToDouble(_item[1], _provider) });
                    }

                }
            }

            return base.convertJsonToObject(pJsonData);
        }
    }
}
