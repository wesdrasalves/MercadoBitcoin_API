#region Credits 
/*
 * Author:          Wesdras Alves 
 * Version:         1.0 
 * Email:           wesdras.alves@gmail.com
 * Description:     Class MBAccess, é a class que encapsula as chamadas publicas e 
 *                  privadas da API do Mercado Bitcoin aplicando os devidos filtros. 
 */
#endregion
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Dotend.MBTrade
{
    internal class MBAccess
    {
        const string _REQUEST_HOST = "https://www.mercadobitcoin.net";
        const string _REQUEST_PATH = "/tapi/v3/";

        #region "Properties"

        private string _privateKey = "";
        private string _publicKey = "";
        private string _pin = "";
        private string _error;

        internal string Error
        {
            get { return _error; }
        }

        internal string PrivateKey
        {
            set { _privateKey = value; }
        }

        internal string PublicKey
        {
            set { _publicKey = value; }
        }

        internal string Pin
        {
            set { _pin = value; }
        }

        #endregion

        /// <summary>
        /// Metodo responsavel por montar os parâmetros que seram enviados ao servidor
        /// via POST
        /// </summary>
        /// <param name="pMethod">Função que deseja acessar na API</param>
        /// <param name="pParameters">Parametros a serem incorporado nos paramentro 
        /// montado pela função bind</param>
        /// <returns>Paramtros no formato a serem evniados para a API do MercadoBitcoin</returns>
        private string bindParmeters(MBEnumerables.MethodAPI pMethod, string pParameters)
        {
            string _parameters = string.Empty;
            string _method = string.Empty;
            string _time;
            string _bodyText = string.Empty;

            try
            {

                switch (pMethod)
                {
                    case MBEnumerables.MethodAPI.getInfo:
                        _method = "get_account_info";
                        break;
                    case MBEnumerables.MethodAPI.OrderList:
                        _method = "list_orders";
                        break;
                    case MBEnumerables.MethodAPI.Buy:
                        _method = "place_buy_order";
                        break;
                    case MBEnumerables.MethodAPI.Sell:
                        _method = "place_sell_order";
                        break;
                    case MBEnumerables.MethodAPI.CancelOrder:
                        _method = "cancel_order";
                        break;
                }

                //Calcula a hora que está sendo feito o post para ser validado
                //no servidor da API do Mercado Bitcoin para não ter 
                TimeSpan _t = (DateTime.UtcNow - new DateTime(1970, 1, 1));
                _time = Convert.ToString((int)_t.TotalSeconds);

                _bodyText = "tapi_method={0}&tapi_nonce={1}{2}";
                _bodyText = string.Format(_bodyText, _method, _time, pParameters);
                _parameters = _bodyText;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }

            return _parameters;
        }

        private string cripParametersSign(string pParamentersCallback)
        {
            string _sign = string.Empty;
            byte[] _signByte;

            try
            {
                HMACSHA512 _crip;
                System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();

                _crip = new HMACSHA512(encoding.GetBytes(this._privateKey));
                _signByte = _crip.ComputeHash(encoding.GetBytes(string.Format("{0}?{1}", _REQUEST_PATH, pParamentersCallback)));

                StringBuilder _sBuilder = new StringBuilder();

                // Loop through each byte of the hashed data 
                // and format each one as a hexadecimal string.
                for (int _i = 0; _i < _signByte.Length; _i++)
                {
                    _sBuilder.Append(_signByte[_i].ToString("x2"));
                }

                // Return the hexadecimal string.
                _sign = _sBuilder.ToString();

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }

            return _sign;
        }

        /// <summary>
        /// Metodo responsavel por realizar acessar os metodos privados na API do 
        /// Mercado Bitcoin
        /// </summary>
        /// <param name="pMethod">Funcionalidade que deseja acessar da API</param>
        /// <param name="pParameters">paramentros a serem incorporados ao parametros default</param>
        /// <returns></returns>
        internal string getRequestPrivate(MBEnumerables.MethodAPI pMethod, string pParameters)
        {
            string _sign = string.Empty;
            byte[] _body = null;
            string _return = string.Empty;
            string _MB_TAPI_ID = this._publicKey;

            try
            {
                this._error = "";

                //Criando o bind dos parâmentros que serão passados para o API
                //e convertendo em ByteArray para populado no corpo do Request 
                //para o Server
                string _paramenterText = bindParmeters(pMethod, pParameters);
                _body = Encoding.UTF8.GetBytes(_paramenterText);

                //Chamando função que criptografará os parâmentros a serem enviados
                _sign = cripParametersSign(_paramenterText);

                //Criando Metodo de Request para o Servidor do Mercado Bitcoin
                WebRequest request = null;
                request = WebRequest.Create(_REQUEST_HOST + _REQUEST_PATH);

                request.Method = "POST";
                request.Headers.Add("tapi-id", _MB_TAPI_ID);
                request.Headers.Add("tapi-mac", _sign);
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = _body.Length;
                request.Timeout = 360000;

                //Escrevendo parâmentros no corpo do Request para serem enviados a API
                Stream _req = request.GetRequestStream();
                _req.Write(_body, 0, _body.Length);
                _req.Close();

                //Pegando retorno do servidor
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream dataStream = response.GetResponseStream();

                //Convertendo Stream de retorno em texto para 
                //Texto de retorno será um JSON 
                using (StreamReader reader = new StreamReader(dataStream))
                    _return = reader.ReadToEnd();

                //Liberando objetos para o Coletor de Lixo
                dataStream.Close();
                dataStream.Dispose();
                response.Close();
                response.Dispose();

            }
            catch (Exception ex)
            {
                this._error = ex.Message;
                _return = "";
            }

            return _return;
        }

        /// <summary>
        /// Overload do Metodo getRequestPrivate sem a necessidade dos parametros customizados
        /// </summary>
        /// <param name="pMethod"></param>
        /// <returns></returns>
        internal string getRequestPrivate(MBEnumerables.MethodAPI pMethod)
        {
            return getRequestPrivate(pMethod, "");
        }

        /// <summary>
        /// Acessa a API publica do Mercado Bitcoin, neste método não possui a necessidade
        /// de ser passado as chaves privadas e publicas, e o acesso é via GET 
        /// </summary>
        /// <param name="pURLType">Metodo que acessará as funcionalidade da API publica 
        /// do MercadoBitcoin</param>
        /// <param name="pType">Se a busca será por Bitcoin ou Litecoin</param>
        /// <returns></returns>
        internal string getPublicDataMBbyMethod(MBEnumerables.SearchType pTypeSearch, MBEnumerables.CoinType pType)
        {
            string _url = string.Empty;
            string _responseFromServer =string.Empty;

            try
            {

                switch (pTypeSearch)
                {
                    case MBEnumerables.SearchType.Ordens:
                        _url = _REQUEST_HOST + "/api/orderbook{0}";
                        break;
                    case MBEnumerables.SearchType.Trades:
                        _url = _REQUEST_HOST + "/api/trades{0}";
                        break;
                    case MBEnumerables.SearchType.Infos30m:
                        _url = _REQUEST_HOST + "/api/v1/ticker{0}";
                        break;
                    case MBEnumerables.SearchType.Infos24h:
                        _url = _REQUEST_HOST + "/api/v2/ticker{0}";
                        break;
                }

                if (pType == MBEnumerables.CoinType.Bit)
                {
                    _url = string.Format(_url, "");
                }
                else
                {
                    _url = string.Format(_url, "_litecoin");
                }

                //Criando o request para o servidor pegando o JSON de retorno
                WebRequest _request = WebRequest.Create(_url);
                HttpWebResponse _response = (HttpWebResponse)_request.GetResponse();

                Stream _dataStream = _response.GetResponseStream();

                StreamReader _reader = new StreamReader(_dataStream);
                _responseFromServer = _reader.ReadToEnd();

                _reader.Close();
                _dataStream.Close();
                _response.Close();

            }
            catch (Exception ex)
            {
                _error = ex.Message;
                _responseFromServer = string.Empty;
            }

            return _responseFromServer;
        }

    }
}
