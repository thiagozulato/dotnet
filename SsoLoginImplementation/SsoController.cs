using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Specialized;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SsoLogin
{
    [Route("api/[controller]")]
    [ApiController]
    public class SsoController: ControllerBase
    {
        readonly ISsoLoginFactory _loginFactory;

        public SsoController(ISsoLoginFactory loginFactory)
        {
            _loginFactory = loginFactory;    
        }

        [HttpPost]
        public async Task<IActionResult> SsoExternalClients([FromBody]object param, 
                                                            [FromQuery]string type)
        {
            try 
            {   
                await _loginFactory.CreateLoginAsync(type, JObject.FromObject(param).ToDictionary());
            
                return Ok(new {
                    token = "28743592874352743957203498572094837502983"
                });
            }
            catch
            {
                return BadRequest(new {
                    message = "Não foi possível encontrar um canal para login SSO"
                });
            }            
        }
    }

    public interface ISsoLoginFactory
    {
        Task CreateLoginAsync(string type, Dictionary<string, string> values);
    }

    public class SsoLoginFactory: ISsoLoginFactory
    {
        readonly IEnumerable<ISsoLogin> _logins;
        public SsoLoginFactory(IEnumerable<ISsoLogin> logins)
        {
            _logins = logins;
        }

        public async Task CreateLoginAsync(string type, Dictionary<string, string> values)
        {
            var selectedLogin = _logins.Where(x => x.Type == type)
                                       .FirstOrDefault();
            
            if (selectedLogin == null) 
            {
                throw new Exception($"Login do tipo {type} não encontrado");
            }

            var requestContext = values.ToNameValueCollection();

            await selectedLogin.ValidateLoginAsync(new RequestContext()
            {
                Values = requestContext
            });
        }
    }

    public class SsoCpfCnpjLogin : ISsoLogin
    {
        public string Type => "documento";
        public async Task ValidateLoginAsync(RequestContext context)
        {
            string cpf = context.Values.Get("cpf");
            string senha = context.Values.Get("senha");

            if (cpf != "12345678909" && senha != "123456")
            {
                throw new Exception("Login inválido");
            }

            await Task.CompletedTask;
        }
    }

    public class SsoClientIdLogin : ISsoLogin
    {
        public string Type => "clientid";
        public async Task ValidateLoginAsync(RequestContext context)
        {
            string grupos = context.Values.Get("clientId");
            string accessCode = context.Values.Get("accessCode");

            await Task.CompletedTask;
        }
    }

    public class SsoTicketLogin : ISsoLogin
    {
        public string Type => "ticket";
        public async Task ValidateLoginAsync(RequestContext context)
        {
            string ticket = context.Values.Get("ticket");

            await Task.CompletedTask;
        }
    }

    public class RequestContext
    {
        public NameValueCollection Values { get; set; }
    }

    public interface ISsoLogin
    {
        string Type { get; }
        Task ValidateLoginAsync(RequestContext context);
    }

    public static class DictionaryExtensions
    {
        public static NameValueCollection ToNameValueCollection(this Dictionary<string, string> data)
        {
            var result = new NameValueCollection();

            if (data == null || data.Count == 0)
            {
                return result;
            }

            foreach (var name in data.Keys)
            {
                var value = data[name];
                if (value != null)
                {
                    result.Add(name, value);
                }
            }

            return result;
        }
    }

    public static class JObjectExtensons
    {
        public static Dictionary<string, string> ToDictionary(this JObject jsonObject)
        {
            string json = jsonObject.ToString();
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        }
    }
}