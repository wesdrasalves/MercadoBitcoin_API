using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Dotend.MBTrade.DTO
{
    public class DTOMBMyFunds : DTOMB
    {
        public double balanceBRLAvaliable { get; set; }
        public double balanceBRLTotal { get; set; }
        public double balanceBTCAvaliable { get; set; }
        public double balanceBTCTotal { get; set; }
        public double balanceLTCAvaliable { get; set; }
        public double balanceLTCTotal { get; set; }

        public DTOMBMyFunds(string pJsonData) : base(pJsonData)
        {
            this.convertJsonToObject(pJsonData);
        }

        private void castObject(Dictionary<string, object> pDicResponse)
        {
            DTOMBOrder _order = new DTOMBOrder();
            NumberFormatInfo _provider = new NumberFormatInfo();
            DateTime _dataBase = new DateTime(1970, 1, 1);
            _provider.NumberDecimalSeparator = ".";
            _provider.NumberGroupSeparator = ",";

            try
            {
                var _balance = (Dictionary<string, object>)pDicResponse["balance"];

                this.balanceBRLAvaliable = Convert.ToDouble(((Dictionary<string, object>)_balance["brl"])["available"], _provider);
                this.balanceBRLTotal = Convert.ToDouble(((Dictionary<string, object>)_balance["brl"])["total"], _provider);

                this.balanceBTCAvaliable = Convert.ToDouble(((Dictionary<string, object>)_balance["btc"])["available"], _provider);
                this.balanceBTCTotal = Convert.ToDouble(((Dictionary<string, object>)_balance["btc"])["total"], _provider);

                this.balanceLTCAvaliable = Convert.ToDouble(((Dictionary<string, object>)_balance["ltc"])["available"], _provider);
                this.balanceLTCTotal = Convert.ToDouble(((Dictionary<string, object>)_balance["ltc"])["total"], _provider);
            }
            catch
            {
                _order = null;
            }
        }

        public override IEnumerable convertJsonToObject(string pJsonData)
        {
            var _data = new JavaScriptSerializer().DeserializeObject(pJsonData);

            if (_data != null)
            {
                if (((Dictionary<string, object>)_data).ContainsKey("status_code") &&
                    Convert.ToString(((Dictionary<string, object>)_data)["status_code"]) == "100")
                {
                    Dictionary<string, object> _response = (Dictionary<string, object>)((Dictionary<string, object>)_data)["response_data"];
                    this.castObject(_response);
                }
            }

            return base.convertJsonToObject(pJsonData);
        }
    }
}
