using System.Collections.Specialized;
using System.Web;
using Microsoft.AspNetCore.Mvc.Rendering;
using PacketDotNet;

namespace CloudInteractive.HomNetBridge.Util
{
    public abstract class Rule : Attribute
    {
        public abstract bool Check(IPPacket x);
        public abstract bool Check(string x);
    }

    public abstract class SerialRuleAttribute : Rule
    {
        public override bool Check(IPPacket x) => throw new NotImplementedException();
    }

    public abstract class EthernetRuleAttribute : Rule
    {
        public override bool Check(string x) => throw new NotImplementedException();
    }


    [AttributeUsage(AttributeTargets.Method)]
    public class ProtocolAttribute : EthernetRuleAttribute
    {
        public ProtocolType Value { get; }
        public ProtocolAttribute(ProtocolType type) => Value = type;
        

        public override bool Check(IPPacket x)
        {
            return x.Protocol == Value;
        }

    }

    [AttributeUsage(AttributeTargets.Method)]
    public class UrlParameterAttribute : EthernetRuleAttribute
    {
        public string Value { get; }
        public UrlParameterAttribute(string value) => Value = value;
        
        public override bool Check(IPPacket x)
        {
            if (x.Protocol is ProtocolType.Udp or ProtocolType.Tcp)
            {
                NameValueCollection parameters = HttpUtility.ParseQueryString(x.PayloadToString());
                return !String.IsNullOrWhiteSpace(parameters[Value]);
            }
            return false;
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class StartsWithAttribute : Rule
    {
        public string Value { get; }
        public StartsWithAttribute(string value) => Value = value;
        

        public override bool Check(IPPacket x) => x.PayloadToString().StartsWith(Value);
        public override bool Check(string x) => x.StartsWith(Value);

    }

    [AttributeUsage(AttributeTargets.Method)]
    public class ContainsAttribute : Rule
    {
        public string Value { get; }
        public ContainsAttribute(string value) => Value = value;
        
        public override bool Check(IPPacket packet)
        {
            if (packet.Protocol is ProtocolType.Udp or ProtocolType.Tcp)
            {
                return packet.PayloadToString().Contains(Value);
            }
            return false;
        }

        public override bool Check(string x) => x.Contains(Value);
    }


}
