using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dotend.MBTrade.DTO
{
    abstract public class DTOMB
    {
        private string jsonData;

        public DTOMB(string pJsonData)
        {
            this.jsonData = pJsonData;
        }

        public virtual IEnumerable convertJsonToObject(string pJsonData)
        {
            this.jsonData = pJsonData;

            yield return this;
        }

        public string getJsonData()
        {
            return this.jsonData;
        }

    }
}
