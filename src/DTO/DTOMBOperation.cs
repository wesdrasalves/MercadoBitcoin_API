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
    public class DTOMBOperation : DTOMB
    {
        public long id { get; set; }
        public double quantity { get; set; }
        public double price { get; set; }
        public double rate { get; set; }
        public DateTime executed { get; set; }


        public DTOMBOperation(string pJsonData) : base(pJsonData)
        {
            foreach (DTOMBOperation _item in this.convertJsonToObject(pJsonData))
            {
                foreach (PropertyInfo _prop in typeof(DTOMBOperation).GetProperties())
                {
                    _prop.SetValue(this, _prop.GetValue(_item));
                }
                return;
            }
        }

        public override IEnumerable convertJsonToObject(string pJsonData)
        {
            var _data = new JavaScriptSerializer().DeserializeObject(pJsonData);
            NumberFormatInfo _provider = new NumberFormatInfo();
            DateTime _dataBase = new DateTime(1970, 1, 1);
            _provider.NumberDecimalSeparator = ".";
            _provider.NumberGroupSeparator = ",";

            if (_data != null)
            {
                this.id = Convert.ToInt64((((Dictionary<string, object>)_data)["operation_id"]));
                this.quantity = Convert.ToDouble((((Dictionary<string, object>)_data)["quantity"]), _provider);
                this.price = Convert.ToDouble((((Dictionary<string, object>)_data)["price"]), _provider);
                this.rate = Convert.ToDouble((((Dictionary<string, object>)_data)["fee_rate"]), _provider);
                this.executed = _dataBase.AddSeconds(Convert.ToInt64((((Dictionary<string, object>)_data)["executed_timestamp"])));
            }

            return base.convertJsonToObject(pJsonData);
        }


    }
}
