using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sample.Models
{
    public class Response
    {
        public int ErrNum { get; set; }
        public string ErrMsg { get; set; }
        public Dictionary<string, object> Data { get; set; }
        public Response()
        {
            ErrNum = 0; 
            ErrMsg = string.Empty;
            Data = new Dictionary<string, object>();
        }
        public Response(int errNum, string errMsg)
        {
            ErrNum = errNum;
            ErrMsg = errMsg;
            Data = new Dictionary<string, object>();
        }
    }
}