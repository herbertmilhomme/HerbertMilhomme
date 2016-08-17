using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Diagnostics;
using System.Net;
using System.Xml;
using System.IO;
using System.Text;
    
    public class CVAPI
    {

        String service = "http://<server>:81/SearchSvc/CVWebService.svc/";
        public void CVRESTAPISample()
        {
            //1. Login
            string user = "username";
            string pwd = "plainpassword";
            string token = GetSessionToken(user, pwd);
            if (string.IsNullOrEmpty(token))
            {
                Debug.WriteLine("Login Failed");
            }
            else
            {
                Debug.WriteLine("Login Successful");

                //Login successful.	
                //2. Get client props. Client Id is hard coded to 2. 
                int iClient = 2;
                string clientPropService = service + "client/" + iClient;
                HttpWebResponse ClientResp = SendRequest(clientPropService, "GET", token, null);
                if (ClientResp.StatusCode == HttpStatusCode.OK)
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.Load(ClientResp.GetResponseStream());
                    //Parse response to get client name, host name and client description
                    Debug.WriteLine(string.Format("Client properties response: ", xmlDoc.InnerXml));
                    string clientName = xmlDoc.SelectSingleNode("/App_GetClientPropertiesResponse/clientProperties/client/clientEntity/@clientName").Value;
                    string clientHostName = xmlDoc.SelectSingleNode("/App_GetClientPropertiesResponse/clientProperties/client/clientEntity/@hostName").Value;
                    string clientDescription = xmlDoc.SelectSingleNode("/App_GetClientPropertiesResponse/clientProperties/client/@clientDescription").Value;
                }
                else
                {
                    Debug.WriteLine(string.Format("Get Client properties request Failed. Status Code: {0}, Status Description: {1}", ClientResp.StatusCode, ClientResp.StatusDescription));
                }

                //3. Set client props
                //The following request XML is hard coded here but can be read from a file and appropriate properties set.
                string newJobPriority = "7";
                string updateClientProps = "<App_SetClientPropertiesRequest><clientProperties><clientProps JobPriority=\"<<jobPriority>>\"></clientProps></clientProperties></App_SetClientPropertiesRequest>";
                updateClientProps = updateClientProps.Replace("<<jobPriority>>", newJobPriority);
                HttpWebResponse clientUpdateResp = SendRequest(clientPropService, "POST", token, updateClientProps);
                if (ClientResp.StatusCode == HttpStatusCode.OK)
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.Load(clientUpdateResp.GetResponseStream());
                    Debug.WriteLine(string.Format("Client properties response: ", xmlDoc.InnerXml));
                    string errorCode = xmlDoc.SelectSingleNode("/App_SetClientPropertiesResponse/response/@errorCode").Value;
                    if (errorCode == "0")
                    {
                        Debug.WriteLine("Client properties set successfully");
                    }
                    else
                    {
                        Debug.WriteLine("Client properties could not be set. Error Code: " + errorCode);
                    }
                }
                else
                {
                    Debug.WriteLine(string.Format("Set client properties request Failed. Status Code: {0}, Status Description: {1}", ClientResp.StatusCode, ClientResp.StatusDescription));
                }

            }
        }

        private string GetSessionToken(string userName, string password)
        {
            string token = string.Empty;
            string loginService = service + "Login";
            byte[] pwd = System.Text.Encoding.UTF8.GetBytes(password);
            String encodedPassword = Convert.ToBase64String(pwd, 0, pwd.Length, Base64FormattingOptions.None);
            string loginReq = string.Format("<DM2ContentIndexing_CheckCredentialReq mode=\"Webconsole\" username=\"{0}\" password=\"{1}\" />", userName, encodedPassword);
            HttpWebResponse resp = SendRequest(loginService, "POST", null, loginReq);
            //Check response code and check if the response has an attribute "token" set
            if (resp.StatusCode == HttpStatusCode.OK)
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(resp.GetResponseStream());
                token = xmlDoc.SelectSingleNode("/DM2ContentIndexing_CheckCredentialResp/@token").Value;
            }
            else
            {
                Debug.WriteLine(string.Format("Login Failed. Status Code: {0}, Status Description: {1}", resp.StatusCode, resp.StatusDescription));
            }
            return token;
        }

        private HttpWebResponse SendRequest(string serviceURL, string httpMethod, string token, string requestBody)
        {
            WebRequest req = WebRequest.Create(serviceURL);
            req.Method = httpMethod;
            req.ContentType = @"application/xml; charset=utf-8";
            //build headers with the received token
            if (!string.IsNullOrEmpty(token))
                req.Headers.Add("Authtoken", token);
            if (!string.IsNullOrEmpty(requestBody))
                WriteRequest(req, requestBody);
            return req.GetResponse() as HttpWebResponse;
        }

        private void WriteRequest(WebRequest req, string input)
        {
            req.ContentLength = Encoding.UTF8.GetByteCount(input);
            using (Stream stream = req.GetRequestStream())
            {
                stream.Write(Encoding.UTF8.GetBytes(input), 0, Encoding.UTF8.GetByteCount(input));
            }
        }
    }
