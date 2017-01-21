#region Credits 
/*
 * Author:          Wesdras Alves 
 * Version:         1.0 
 * Email:           wesdras.alves@gmail.com
 * Description:     Class MBTAPI, é a class a ser usada pelo aplicação do usuário final, 
 *                  a class possui vários metodos que facilitam no uso da API de Trade 
 *                  e acesso a API publica do Mercado BitCoin
 */
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Diagnostics;
using System.Web;
using System.Web.Script.Serialization;
using Dotend.MBTrade.DTO;


namespace Dotend.MBTrade
{
    /// <summary>
    /// Class de acesso do usuário, contendo métodos simples para integrar com o a API do Mercado Bitcoin.
    /// 
    /// </summary>
    public class MBTAPI
    {
        private MBAccess _mbAcess;
        private string _error;
        private int _codeError;

        /// <summary>
        /// Construtor responsavel por receber e armazenas os valores referente as informações do usuário para acesso 
        /// a API do Mercado Bitcoin, as informações solicitadas são obrigatórioas, as mesmas estão presente em sua conta
        /// do Mercado Bitcoin
        /// </summary>
        /// <param name="pPrivateKey"></param>
        /// <param name="pPublicKey"></param>
        /// <param name="pPin"></param>
        public MBTAPI(string pPrivateKey, string pPublicKey, string pPin)
        {
            _mbAcess = new MBAccess();

            this.PrivateKey = pPrivateKey;
            this.PublicKey = pPublicKey;
            this.Pin = pPin;
        }


        /// <summary>
        ///  Propriedade que armazenada o erro ocorrido em determinada chamada de um metodo
        /// </summary>
        public string Error
        {
            get { return _error; }
        }

        public int CodeError
        {
            get { return _codeError; }
        }

        private string PrivateKey
        {
            set { _mbAcess.PrivateKey = value; }
        }

        private string PublicKey
        {
            set { _mbAcess.PublicKey = value; }
        }

        private string Pin
        {
            set { _mbAcess.Pin = value; }
        }

        #region "MB Public function non authentic"

        /// <summary>
        /// Função responsavel por buscar a lista das ultimas ordens que estão aguardando serem executadas no MercadoBitcoin
        /// Essa função retorna tanto as ordens de compra quanto a de venda abertas. 
        /// </summary>
        /// <returns></returns>
        public DTOMBOrderBook getLastOrders()
        {
            DTOMBOrderBook _orderBook = null;
            try
            {
                string jsonString = _mbAcess.getPublicDataMBbyMethod(MBEnumerables.SearchType.Ordens, MBEnumerables.CoinType.Bit);
                _orderBook  = new DTOMBOrderBook(jsonString);
                _orderBook.convertJsonToObject(jsonString);
            }
            catch (Exception ex)
            {
                this._error = ex.Message;
            }
            return _orderBook;
        }

        #endregion


        #region "MB Public function with authentic"

        /// <summary>
        /// Função responsavel por trazer informações gerais da sua conta, como
        /// a quantidade de valores em Reais, Bitcoin e Litecoin tem disponivel e total 
        /// contando com as ordens criadas e aguardando serem executadas
        /// </summary>
        /// <returns></returns>
        public DTOMBMyFunds getMyInfoAccount()
        {

            DTOMBMyFunds _return;
            string _json;

            _json = _mbAcess.getRequestPrivate(MBEnumerables.MethodAPI.getInfo);


            if (validateJsonReturn(_json))
            {
                _return = new DTOMBMyFunds(_json);                
                return _return;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Função que retorna todas as suas ordens em aberto, fazendo filtro pelos parâmentros
        /// passados pela função
        /// </summary>
        /// <param name="pCoinType"></param>
        /// <param name="pNumberDays"></param>
        /// <returns></returns>
        public List<DTOMBOrder> getMyOpenOrders(MBEnumerables.CoinType pCoinType,int pNumberDays = 1 )
        {
            string _parameters = string.Empty;
            string _json;
            List<DTOMBOrder> _listOrders = new List<DTOMBOrder>();

            _parameters += "&" ;

            _parameters += "status_list=[2]&";

            switch (pCoinType)
            {
                case MBEnumerables.CoinType.Bit:
                    _parameters += "coin_pair=BRLBTC&";
                    break;
                case MBEnumerables.CoinType.Lit:
                    _parameters += "coin_pair=BRLLTC&";
                    break;
            }

            TimeSpan _t = (DateTime.UtcNow.AddDays(pNumberDays * -1) - new DateTime(1970, 1, 1));
            string _time;
            _time = Convert.ToString((int)_t.TotalSeconds);

            _json = _mbAcess.getRequestPrivate(MBEnumerables.MethodAPI.OrderList,_parameters);

            if (validateJsonReturn(_json))
            {
                DTOMBOrder _orderBase = new DTOMBOrder();

                foreach (DTOMBOrder _order in _orderBase.convertJsonToObject(_json))
                {
                    _listOrders.Add(_order);
                }
            }
            else
            {
                _listOrders = null;
            }

            return _listOrders;
        }

        public DTOMBOrder  setBitCoinTradeBuy(double pVolume, double pPrice)
        {
            return setTrade(MBEnumerables.CoinType.Bit, MBEnumerables.OperationType.Buy, pVolume, pPrice);
        }

        public DTOMBOrder setBitCoinTradeSell(double pVolume, double pPrice)
        {
            return setTrade(MBEnumerables.CoinType.Bit, MBEnumerables.OperationType.Sell, pVolume, pPrice);
        }

        public DTOMBOrder setLitCoinTradeBuy(double pVolume, double pPrice)
        {
            return setTrade(MBEnumerables.CoinType.Lit, MBEnumerables.OperationType.Buy, pVolume, pPrice);
        }

        public DTOMBOrder setLitTradeSell(double pVolume, double pPrice)
        {
            return setTrade(MBEnumerables.CoinType.Lit, MBEnumerables.OperationType.Sell, pVolume, pPrice);
        }

        private DTOMBOrder setTrade(MBEnumerables.CoinType pCoinType, MBEnumerables.OperationType pOperationType, double pVolume, double pPrice)
        {
            string _parameters = string.Empty;
            string _json;
            DTOMBOrder _myorders;

            _parameters += "&" ;

            switch (pCoinType)
            {
                case MBEnumerables.CoinType.Bit:
                    _parameters += "coin_pair=BRLBTC&";
                    break;
                case MBEnumerables.CoinType.Lit:
                    _parameters += "coin_pair=BRLLTC&";
                    break;
            }

            _parameters += string.Format("quantity={0}", Convert.ToString(pVolume).Replace(",",".")) + "&";
            _parameters += string.Format("limit_price={0}", Convert.ToString(pPrice).Replace(",", "."));

            if (pOperationType == MBEnumerables.OperationType.Buy)
            {
                _json = _mbAcess.getRequestPrivate(MBEnumerables.MethodAPI.Buy, _parameters);
            }
            else {
                _json = _mbAcess.getRequestPrivate(MBEnumerables.MethodAPI.Sell, _parameters);
            }

            if (validateJsonReturn(_json))
            {
                _myorders = new DTOMBOrder(_json);

            }
            else
            {
                _myorders = null;
            }
            
            return _myorders;
        }

        public DTOMBOrder cancelOrderBit(double pNumberOrder)
        {
            return setCancelTrade(MBEnumerables.CoinType.Bit, pNumberOrder);
        }

        public DTOMBOrder cancelOrderLit(double pNumberOrder)
        {
            return setCancelTrade(MBEnumerables.CoinType.Lit, pNumberOrder);
        }

        private DTOMBOrder setCancelTrade(MBEnumerables.CoinType pTipoMoeda, double pNumberOrder)
        {
            string _parameters = string.Empty;
            string _json;
            DTOMBOrder _order = new DTOMBOrder();

            _parameters += "&" ;

            switch (pTipoMoeda)
            {
                case MBEnumerables.CoinType.Bit:
                    _parameters += "coin_pair=BRLBTC&";
                    break;
                case MBEnumerables.CoinType.Lit:
                    _parameters += "coin_pair=BRLLTC&";
                    break;
            }

            _parameters += string.Format("order_id={0}", Convert.ToString(pNumberOrder));

            _json = _mbAcess.getRequestPrivate(MBEnumerables.MethodAPI.CancelOrder, _parameters);

            if (validateJsonReturn(_json))
            {
                _order = new DTOMBOrder(_json);
            }
            else
            {
                _order = null;
            }

            return _order;
        }

        #endregion

        #region "Helper functions"

        private bool validateJsonReturn(string pJson)
        {
            bool _valid = false;

            if (pJson != string.Empty)
            {
                var _data = new JavaScriptSerializer().DeserializeObject(pJson);

                if (_data != null)
                {
                    if (((Dictionary<string, object>)_data).ContainsKey("status_code"))
                    {
                        if (Convert.ToString(((Dictionary<string, object>)_data)["status_code"]) == "100")
                            _valid = true;
                        else
                        {
                            if (((Dictionary<string, object>)_data).ContainsKey("error_message"))
                                _error = Convert.ToString(((Dictionary<string, object>)_data)["error_message"]);
                            else
                            {
                                _codeError = 3;
                                _error = "Json em formato incorreto do pardão do MercadoBitcoin.";
                            }
                        }
                        int.TryParse(Convert.ToString(((Dictionary<string, object>)_data)["status_code"]), out _codeError);
                    }
                    else
                    {
                        _codeError = 3;
                        _error = "Json em formato incorreto do pardão do MercadoBitcoin.";
                    }

                }
                else {
                    _codeError = 2;
                    _error = "Json em formato incorreto, não possivel converter.";
                }

            }
            else
            {
                _codeError = 1;
                _error = "Dados não retornados corretamente.";
            }

            return _valid;
        }        
       
        #endregion
    }

}
