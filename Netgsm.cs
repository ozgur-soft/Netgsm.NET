using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Netgsm {
    public interface INetgsm {
        void SetUsercode(string usercode);
        void SetPassword(string password);
        Netgsm.XML Sms(string header, string phone, string message, string startdate = null, string stopdate = null, string filter = null);
        Netgsm.XML Otp(string header, string phone, string message);
    }
    public class Netgsm : INetgsm {
        private string Endpoint { get; set; }
        private string Usercode { get; set; }
        private string Password { get; set; }
        private string Appkey { get; set; }
        public Netgsm() {
            Endpoint = "https://api.netgsm.com.tr";
        }
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
            [JsonPropertyName("usercode")]
            [XmlElement("usercode", IsNullable = false)]
            public string Usercode { init; get; }
            [JsonPropertyName("password")]
            [XmlElement("password", IsNullable = false)]
            public string Password { init; get; }
            [JsonPropertyName("appkey")]
            [XmlElement("appkey", IsNullable = false)]
            public string Appkey { init; get; }
            [XmlElement("msgheader", IsNullable = false)]
            public string MsgHeader { init; get; }
            [XmlElement("startdate", IsNullable = false)]
            public string StartDate { init; get; }
            [XmlElement("stopdate", IsNullable = false)]
            public string StopDate { init; get; }
            [JsonPropertyName("stip")]
            [XmlElement("stip", IsNullable = false)]
            public string Stip { init; get; }
            [JsonPropertyName("view")]
            [XmlElement("view", IsNullable = false)]
            public string View { init; get; }
            [XmlElement("filter", IsNullable = false)]
            public string Filter { init; get; }
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
        public class Results {
            [JsonPropertyName("balance")]
            public List<Result> Balance { init; get; }
        }
        public class Result {
            [JsonPropertyName("balance_name")]
            public string Name { init; get; }
            [JsonPropertyName("amount")]
            public long? Amount { init; get; }
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
        public void SetAppkey(string appkey) {
            Appkey = appkey;
        }
        public XML Sms(string header, string phone, string message, string startdate = "", string stopdate = "", string filter = "") {
            var data = new MainBody {
                Header = new Header {
                    Company = "Netgsm",
                    Type = "1:n",
                    Usercode = Usercode,
                    Password = Password,
                    MsgHeader = header,
                    StartDate = startdate,
                    StopDate = stopdate,
                    Filter = filter
                },
                Body = new Body {
                    No = phone.ToString(),
                    Msg = new XCData(message).ToString()
                }
            };
            using var stream = new MemoryStream();
            var xml = new XmlSerializer(typeof(XML));
            var mainbody = new XmlSerializer(typeof(MainBody));
            using var writer = new XmlTextWriter(stream, new UTF8Encoding(false));
            mainbody.Serialize(writer, data);
            try {
                using var http = new HttpClient();
                using var request = new HttpRequestMessage(HttpMethod.Post, Endpoint + "/sms/send/xml") {
                    Content = new StringContent(HttpUtility.HtmlDecode(Encoding.UTF8.GetString(stream.ToArray())), Encoding.UTF8, "text/xml")
                };
                using var response = http.Send(request);
                using var content = response.Content.ReadAsStream();
                using var reader = new StreamReader(content, Encoding.UTF8);
                var result = reader.ReadToEnd();
                var parse = result.Split(' ');
                if (parse.Length == 2) {
                    if (int.TryParse(parse[0], out var code)) {
                        return new XML { Main = new() { Code = code, JobID = long.Parse(parse[1]) } };
                    }
                } else if (int.TryParse(result, out var code)) {
                    return new XML { Main = new() { Code = code } };
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
            using var stream = new MemoryStream();
            var xml = new XmlSerializer(typeof(XML));
            var mainbody = new XmlSerializer(typeof(MainBody));
            using var writer = new XmlTextWriter(stream, new UTF8Encoding(false));
            mainbody.Serialize(writer, data);
            try {
                using var http = new HttpClient();
                using var request = new HttpRequestMessage(HttpMethod.Post, Endpoint + "/sms/send/otp") {
                    Content = new StringContent(HttpUtility.HtmlDecode(Encoding.UTF8.GetString(stream.ToArray())), Encoding.UTF8, "text/xml")
                };
                using var response = http.Send(request);
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
        public Results Balance() {
            try {
                var header = new Header { Usercode = Usercode, Password = Password, Appkey = Appkey, Stip = "1" };
                using var http = new HttpClient();
                using var request = new HttpRequestMessage(HttpMethod.Post, Endpoint + "/balance") {
                    Content = new StringContent(JsonSerializer.Serialize(header, new JsonSerializerOptions { WriteIndented = false, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }), Encoding.UTF8, "application/json")
                };
                using var response = http.Send(request);
                using var stream = response.Content.ReadAsStream();
                using var reader = new StreamReader(stream, Encoding.UTF8);
                var results = JsonSerializer.Deserialize<Results>(reader.ReadToEnd());
                return results;
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