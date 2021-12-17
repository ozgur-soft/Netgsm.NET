using System.Text;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Netgsm {
    public interface INetgsm {
        Netgsm.XML Sms(string header, string phone, string message, string startdate = "", string stopdate = "");
        Netgsm.XML Otp(string header, string phone, string message);
    }
    public class Netgsm : INetgsm {
        private string Usercode { get; set; }
        private string Password { get; set; }
        public Netgsm() { }
        [Serializable, XmlRoot("mainbody")]
        public class MainBody {
            [XmlElement("header", IsNullable = false)]
            public Header Header { init; get; }
            [XmlElement("body", IsNullable = false)]
            public Body Body { init; get; }
        }
        public class Header {
            [XmlElement("company", IsNullable = false)]
            public string Company { init; get; }
            [XmlElement("type", IsNullable = false)]
            public string Type { init; get; }
            [XmlElement("usercode", IsNullable = false)]
            public string Usercode { init; get; }
            [XmlElement("password", IsNullable = false)]
            public string Password { init; get; }
            [XmlElement("msgheader", IsNullable = false)]
            public string MsgHeader { init; get; }
            [XmlElement("startdate", IsNullable = false)]
            public string StartDate { init; get; }
            [XmlElement("stopdate", IsNullable = false)]
            public string StopDate { init; get; }
        }
        public class Body {
            [XmlElement("msg", IsNullable = false)]
            public string Msg { init; get; }
            [XmlElement("no", IsNullable = false)]
            public string No { init; get; }
        }
        [Serializable, XmlRoot("xml")]
        public class XML {
            [XmlElement("main", IsNullable = false)]
            public Main Main { init; get; }
        }
        public class Main {
            [XmlElement("code", IsNullable = false)]
            public int Code { init; get; }
            [XmlElement("jobID", IsNullable = false)]
            public long JobID { init; get; }
        }
        public class Writer : StringWriter {
            public override Encoding Encoding => Encoding.UTF8;
        }
        public void SetUsercode(string usercode) {
            Usercode = usercode;
        }
        public void SetPassword(string password) {
            Password = password;
        }
        public XML Sms(string header, string phone, string message, string startdate = "", string stopdate = "") {
            var data = new MainBody {
                Header = new Header {
                    Company = "Netgsm",
                    Type = "1:n",
                    Usercode = Usercode,
                    Password = Password,
                    MsgHeader = header,
                    StartDate = startdate,
                    StopDate = stopdate
                },
                Body = new Body {
                    No = phone.ToString(),
                    Msg = new XCData(message).ToString()
                }
            };
            var mainbody = new XmlSerializer(typeof(MainBody));
            var writer = new Writer();
            var ns = new XmlSerializerNamespaces();
            ns.Add(string.Empty, string.Empty);
            mainbody.Serialize(writer, data, ns);
            try {
                var http = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Post, "https://api.netgsm.com.tr/sms/send/xml") {
                    Content = new StringContent(HttpUtility.HtmlDecode(writer.ToString()), Encoding.UTF8, "text/xml")
                };
                var response = http.Send(request);
                var reader = new StreamReader(response.Content.ReadAsStream());
                var result = reader.ReadToEnd();
                if (string.IsNullOrEmpty(result)) {
                    return null;
                } else if (result.Split(" ").Length == 1) {
                    return new XML { Main = new Main { Code = int.Parse(result.Split(" ").GetValue(0).ToString()), JobID = 0 } };
                } else {
                    return new XML { Main = new Main { Code = int.Parse(result.Split(" ").GetValue(0).ToString()), JobID = long.Parse(result.Split(" ").GetValue(1).ToString()) } };
                }
            } catch (Exception err) {
                if (err.InnerException != null) {
                    Console.WriteLine(err.InnerException.Message);
                } else {
                    Console.WriteLine(err.Message);
                }
            }
            return null;
        }
        public XML Otp(string header, string phone, string message) {
            var data = new MainBody {
                Header = new Header {
                    Usercode = Usercode,
                    Password = Password,
                    MsgHeader = header
                },
                Body = new Body {
                    No = phone.ToString(),
                    Msg = new XCData(message).ToString()
                }
            };
            var mainbody = new XmlSerializer(typeof(MainBody));
            var xml = new XmlSerializer(typeof(XML));
            var writer = new Writer();
            var ns = new XmlSerializerNamespaces();
            ns.Add(string.Empty, string.Empty);
            mainbody.Serialize(writer, data, ns);
            try {
                var http = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Post, "https://api.netgsm.com.tr/sms/send/otp") {
                    Content = new StringContent(HttpUtility.HtmlDecode(writer.ToString()), Encoding.UTF8, "text/xml")
                };
                var response = http.Send(request);
                var result = (XML)xml.Deserialize(response.Content.ReadAsStream());
                return result;
            } catch (Exception err) {
                if (err.InnerException != null) {
                    Console.WriteLine(err.InnerException.Message);
                } else {
                    Console.WriteLine(err.Message);
                }
            }
            return null;
        }
    }
}