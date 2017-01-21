using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Reflection;
using System.Globalization;

namespace Dotend.MBTrade.DTO
{
    public class DTOMBOrder : DTOMB
    {
        public long id { get; set; }//Identificador da Ordem
        public string coinPair { get; set; } //Tipo de moeda BRLBTC ou BRLLTC
        public MBEnumerables.OperationType type { get; set; }
        public int status { get; set; } //2-Open, 3- Canceled, 4-Filled 
        public bool hasFills { get; set; } //Se teve alguma execução 
        public double quantity { get; set; } //Quantidade da moeda digital
        public double price { get; set; } //Preço da moeda digital
        public double executedQuantity { get; set; } //Quantidade da moeda digital executada
        public double executedPriceAvg { get; set; } //Preço unitário medio de execução
        public double fee { get; set; } //Comissão da ordem 
        public DateTime created { get; set; } //Data de Criação
        public DateTime update { get; set; } //Date de Atualização
        public List<DTOMBOperation> operations { get; set; } //Lista de Operações já executadas

        public DTOMBOrder() : base("") { }
        public DTOMBOrder(string pJsonData) : base(pJsonData) {

            foreach (DTOMBOrder _item in this.convertJsonToObject(pJsonData))
            {
                foreach (PropertyInfo _prop in typeof(DTOMBOrder).GetProperties())
                {
                    if(_prop.Name != "operations") _prop.SetValue(this, _prop.GetValue(_item));
                }
                return;
            }
        }

        private DTOMBOrder getCastObject(Dictionary<string,object> pDicOrder)
        {
            DTOMBOrder _order = new DTOMBOrder();
            NumberFormatInfo _provider = new NumberFormatInfo();
            DateTime _dataBase = new DateTime(1970, 1, 1);
            _provider.NumberDecimalSeparator = ".";
            _provider.NumberGroupSeparator = ",";

            try
            {
                _order.id = Convert.ToInt64(pDicOrder["order_id"]);
                _order.coinPair = Convert.ToString(pDicOrder["coin_pair"]);
                _order.type = Convert.ToInt32(pDicOrder["order_type"]) == 2 ? MBEnumerables.OperationType.Buy : MBEnumerables.OperationType.Sell;
                _order.status = Convert.ToInt32(pDicOrder["status"]);
                _order.hasFills = Convert.ToBoolean(pDicOrder["has_fills"]);
                _order.quantity = Convert.ToDouble(pDicOrder["quantity"], _provider);
                _order.price = Convert.ToDouble(pDicOrder["limit_price"], _provider);
                _order.executedQuantity = Convert.ToDouble(pDicOrder["executed_quantity"], _provider);
                _order.executedPriceAvg = Convert.ToDouble(pDicOrder["executed_price_avg"], _provider);
                _order.fee = Convert.ToDouble(pDicOrder["fee"], _provider);
                _order.created = _dataBase.AddSeconds(Convert.ToInt64(pDicOrder["created_timestamp"]));
                _order.update = _dataBase.AddSeconds(Convert.ToInt64(pDicOrder["updated_timestamp"]));

                foreach (object _operation in (object[])pDicOrder["operations"])
                {
                    if (this.operations == null) this.operations = new List<DTOMBOperation>();

                    var _jsonOperation = new JavaScriptSerializer().Serialize(_operation);

                    this.operations.Add(new DTOMBOperation(_jsonOperation));
                }
            }
            catch 
            {
                _order = null;
            }

            return _order;
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

                    if (_response.ContainsKey("order"))
                    {
                        Dictionary<string, object> _dicOrder = ((Dictionary<string, object>)_response["order"]);
                        yield return getCastObject(_dicOrder);
                    }
                    else if (_response.ContainsKey("orders"))
                    {
                        foreach (object _dicObject in (object[])_response["orders"])
                        {
                            Dictionary<string, object> _dicOrder = ((Dictionary<string, object>)_dicObject);
                            yield return getCastObject(_dicOrder);
                        }
                    }
                }
            }

            base.convertJsonToObject(pJsonData);
        }
    }
}
