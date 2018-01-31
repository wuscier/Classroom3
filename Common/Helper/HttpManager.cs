using System;
using System.IO;
using System.Net;
using System.Text;

namespace Common.Helper
{
    public class HttpManager
    {
        public static string HttpPostData(string aUrl, string aData)
        {
            if (string.IsNullOrEmpty(aUrl))
                throw new ArgumentNullException("url");
            try
            {
                var encoding = Encoding.GetEncoding("UTF-8");
                byte[] postBytes = Encoding.UTF8.GetBytes(aData);
                var request = WebRequest.Create(aUrl) as HttpWebRequest;
                if (request != null)
                {
                    request.Method = "POST";
                    request.ContentType = "application/x-www-form-urlencoded;charset=utf-8";
                    request.ContentLength = postBytes.Length;
                    request.Timeout = 3000;
                    using (var stream = request.GetRequestStream())
                    {
                        stream.Write(postBytes, 0, postBytes.Length);
                        stream.Close();
                    }
                    var respone = (HttpWebResponse)request.GetResponse();
                    string responseData;
                    using (var reader = new StreamReader(respone.GetResponseStream(), encoding))
                    {
                        responseData = reader.ReadToEnd();
                    }
                    respone.Close();
                    return responseData;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return "";
        }
    }
}
