#region Credits 
/*
 * Author:          Wesdras Alves 
 * Version:         1.0 
 * Email:           wesdras.alves@gmail.com
 * Description:     Class Enumerable, contendo todos os enums usados nos metodos 
 *                  facilitando o uso dos mesmo.
 */
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dotend.MBTrade
{
    public static class MBEnumerables
    {
        #region "Enums"

        internal enum SearchType
        {
            Ordens,
            Trades,
            Infos30m,
            Infos24h

        }
        public enum MethodAPI
        {
            getInfo,
            OrderList,
            Buy,
            Sell,
            CancelOrder
        }

        public enum CoinType
        {
            Bit,
            Lit
        }

        public enum OperationType
        {
            Buy,
            Sell
        }

        public enum StatusOrder
        {
            None,
            canceled,
            completed,
            active
        }


        #endregion
    }
}
