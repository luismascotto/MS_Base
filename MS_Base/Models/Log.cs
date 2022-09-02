using System;

namespace MS_Base.Models {
    public class Log
    {
        public string _id { get; set; }
        public string ParentLog_ID { get; set; }
        public int Client_ID { get; set; }
        public string Application_ID { get; set; }
        public string Version { get; set; }
        public string vchSerialTerminal { get; set; }
        public string vchMessage { get; set; }
        public string vchFunctionName { get; set; }
        public int intLevel { get; set; }               //1.Info    //2.Warning    //3.Critical
        public int intLogType { get; set; }             //1.Info    //2.Biz        //3.Database    //4.Settings     //5.Audit 
        public DateTime dtmReceived { get; set; }
        public DateTime dtmRecordTime { get; set; }
        public bool isException { get; set; }

        public Log()
        {
            _id = Guid.NewGuid().ToString();
        }

    }
}
