using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using qcloudsms_csharp.httpclient;
using qcloudsms_csharp.json;
using qcloudsms_csharp;
namespace SportStore.SMS
{
    public static class SmsSender
    {
        private static SmsSingleSender smsSingleSender;
        public static SmsSingleSender SmsSingleSender { get
            {
                if (smsSingleSender == null)
                {
                    smsSingleSender = new SmsSingleSender(1400087730, "a3ba3488d6330aa1038fa371df101d1f");
                    return smsSingleSender;
                }
                else
                    return smsSingleSender;
            }
        }
    }
}
