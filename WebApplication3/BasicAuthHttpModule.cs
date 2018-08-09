using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Web;
using System.Data.SQLite;
using System.Security.Cryptography;
using System.IO;

namespace WebHostBasicAuth.Modules
{
    public class BasicAuthHttpModule : IHttpModule
    {
        private const string Realm = "My Realm";

        public void Init(HttpApplication context)
        {
            // Register event handlers
            context.AuthenticateRequest += OnApplicationAuthenticateRequest;
            context.EndRequest += OnApplicationEndRequest;
        }

        private static void SetPrincipal(IPrincipal principal)
        {
            Thread.CurrentPrincipal = principal;
            if (HttpContext.Current != null)
            {
                HttpContext.Current.User = principal;
            }
        }

        // TODO: Here is where you would validate the username and password.
        private static bool CheckPassword(string username, string password)
        {
            using (SQLiteConnection cn = new SQLiteConnection("Data Source=|DataDirectory|\\AUTHORIZATION.sqlite3; Version=3"))
            {
                using (Aes alg = Aes.Create())
                {
                    string query = "SELECT KEYBYTES, IVBYTES, PWBYTES, USERNAME FROM AES";
                    SQLiteCommand cmd = new SQLiteCommand(query, cn);
                    int i = 0;
                    int j = 0;
                    int k = 0;
                    byte[] key = new byte[32];
                    byte[] iv = new byte[16];
                    byte[] pwb = new byte[16];
                    string un = "";
                    cn.Open();
                    SQLiteDataReader dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        key[i] = (byte)int.Parse(dr[0].ToString());
                        i++;
                        if(!string.IsNullOrEmpty(dr[1].ToString()))
                        {
                            iv[j] = (byte)int.Parse(dr[1].ToString());
                            j++;
                        }
                        if (!string.IsNullOrEmpty(dr[2].ToString()))
                        {
                            pwb[k] = (byte)int.Parse(dr[2].ToString());
                            k++;
                        }
                        if (!string.IsNullOrEmpty(dr[3].ToString()))
                        {
                            un = dr[3].ToString();
                        }
                    }
                    cn.Close();

                    alg.Key = key;
                    alg.IV = iv;

                    if(key[0] != 124)
                    {
                        key = key.Reverse().ToArray();
                    }
                    if(iv[0] != 34)
                    {
                        iv = iv.Reverse().ToArray();
                    }
                    if (pwb[0] != 248)
                    {
                        pwb = pwb.Reverse().ToArray();
                    }

                    ICryptoTransform encryptor = alg.CreateEncryptor(alg.Key, alg.IV);

                    byte[] encrypted;
                    using (MemoryStream msEncrypt = new MemoryStream())
                    {
                        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                            {
                                swEncrypt.Write(password);
                            }
                            encrypted = msEncrypt.ToArray();
                        }
                    }
                    for(int l = 0; l < pwb.Length; l++)
                    {
                        if(pwb[l] != encrypted[l])
                        {
                            return false;
                        }
                    }
                    return username == un;
                }
            }
        }

        private static void AuthenticateUser(string credentials)
        {
            try
            {
                var encoding = Encoding.GetEncoding("iso-8859-1");
                credentials = encoding.GetString(Convert.FromBase64String(credentials));

                int separator = credentials.IndexOf(':');
                string name = credentials.Substring(0, separator);
                string password = credentials.Substring(separator + 1);

                if (CheckPassword(name, password))
                {
                    var identity = new GenericIdentity(name);
                    SetPrincipal(new GenericPrincipal(identity, null));
                }
                else
                {
                    // Invalid username or password.
                    HttpContext.Current.Response.StatusCode = 401;
                }
            }
            catch (FormatException)
            {
                // Credentials were not formatted correctly.
                HttpContext.Current.Response.StatusCode = 401;
            }
        }

        private static void OnApplicationAuthenticateRequest(object sender, EventArgs e)
        {
            var request = HttpContext.Current.Request;
            var authHeader = request.Headers["Authorization"];
            if (authHeader != null)
            {
                var authHeaderVal = AuthenticationHeaderValue.Parse(authHeader);

                // RFC 2617 sec 1.2, "scheme" name is case-insensitive
                if (authHeaderVal.Scheme.Equals("basic",
                        StringComparison.OrdinalIgnoreCase) &&
                    authHeaderVal.Parameter != null)
                {
                    AuthenticateUser(authHeaderVal.Parameter);
                }
            }
        }

        // If the request was unauthorized, add the WWW-Authenticate header 
        // to the response.
        private static void OnApplicationEndRequest(object sender, EventArgs e)
        {
            var response = HttpContext.Current.Response;
            if (response.StatusCode == 401)
            {
                response.Headers.Add("WWW-Authenticate",
                    string.Format("Basic realm=\"{0}\"", Realm));
            }
        }

        public void Dispose()
        {
        }
    }
}