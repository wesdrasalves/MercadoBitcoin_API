using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Dotend.MBTrade;
using Dotend.MBTrade.DTO;

namespace Trade.Controllers
{
    public class HomeController : Controller
    {
        
        //Valores a serem obtidos na sua conta do Mercado Bitcoin
        private const string _PUBLIC_KEY = "";
        private const string _PRIVATE_KEY = "";
        private const string _PIN = "";
        private MBTAPI _tapi;

        private void loadTradeAPI()
        {
            _tapi = new MBTAPI(_PRIVATE_KEY,_PUBLIC_KEY,_PIN);
        }

        public ActionResult Index()
        {
            loadTradeAPI();

            if (_tapi != null)
            {
                DTOMBMyFunds _funds;

                _funds = _tapi.getMyInfoAccount();
                ViewBag.Funds = _funds;
            }

            return View();
        }

        [HttpPost]
        public JsonResult getMyOpenOrders()
        {
            loadTradeAPI();
            List<DTOMBOrder> _list;
            bool _success = false;
            string _message = string.Empty;
            DTOMBOrder[] _array = null;

            if (_tapi != null)
            {

                _list = _tapi.getMyOpenOrders(MBEnumerables.CoinType.Bit);

                if (_list == null)
                {
                    _message = _tapi.Error;
                }
                else
                {
                    _array = _list.ToArray();
                    _success = true;
                }
            }
            else {
                _message = "Não foi possivel conectar ao servidor do Mercado Bitcoin.";
            }

            var _result = new
            {
                returnExecute = _success,
                returnData = _array ,
                message = _message
            };

            return Json(_result);
        }

        [HttpPost]
        public JsonResult cancelOrder(string orderId)
        {
            loadTradeAPI();
            long _orderId;
            DTOMBOrder _order = null;
            string _message = "";
            bool _success = false;

            if (_tapi != null)
            {
                if (long.TryParse(orderId, out _orderId))
                {
                    _order = _tapi.cancelOrderBit(_orderId);

                    _message = _order == null ? _tapi.Error : "";
                    _success = _order == null ? false : true;
                }
                else
                {
                    _message = "Numero de ordem invalido.";
                }
            }
            else
            {
                _message = "Não foi possivel conectar ao servidor do Mercado Bitcoin.";
            }

            var _result = new
            {
                returnExecute = _success,
                returnData = _order,
                message = _message
            };


            return Json(_result);
        }

        [HttpPost]
        public JsonResult BuyBitCoin(string volume, string price)
        {
            loadTradeAPI();
            double _volume;
            double _price;
            double _valCalc;
            object _result;
            DTOMBOrder _order = null;
            bool _success = false;
            string _message = string.Empty;

            if (_tapi != null)
            {

                if (double.TryParse(volume, out _volume) && double.TryParse(price, out _price))
                {

                    DTOMBMyFunds _funds;
                    _funds = _tapi.getMyInfoAccount();

                    _valCalc = _volume * _price;

                    if (Math.Round(_funds.balanceBRLAvaliable, 5) <= Math.Round(_valCalc, 5))
                    {
                        _order = _tapi.setBitCoinTradeBuy(_volume, _price);
                        if (_order == null)
                            _success = true;
                        else
                            _message = _tapi.Error;
                    }
                    else
                    {
                        _message = "Não possui saldo";
                    }
                }
            }
            else
            {
                _message = "Não foi possivel conectar ao servidor do Mercado Bitcoin.";
            }

            _result = new
            {
                returnExecute = _success,
                returnData = _order,
                message = _message
            };


            return Json(_result);
        }

        [HttpPost]
        public JsonResult SellBitCoin(string volume, string price)
        {
            loadTradeAPI();
            double _volume;
            double _price;
            object _result;
            DTOMBOrder _order = null;
            bool _success = false;
            string _message = string.Empty;

            if (_tapi != null)
            {
                if (double.TryParse(volume, out _volume) && double.TryParse(price, out _price))
                {
                    DTOMBMyFunds _funds;
                    _funds = _tapi.getMyInfoAccount();

                    if (Math.Round(_funds.balanceBTCAvaliable, 5) <= _volume)
                    {
                        _order = _tapi.setBitCoinTradeSell(_volume, _price);

                        if (_order != null)
                            _success = true;
                        else
                            _message = _tapi.Error;
                    }
                }
                else
                    _message = "Quantidade ou Valor incorreto";
            }
            else {
                _message = "Não foi possivel conectar ao servidor do Mercado Bitcoin.";
            }

            _result = new
            {
                returnExecute = _success,
                returnData = _order,
                message = _message
            };


            return Json(_result);
        }

    }
}