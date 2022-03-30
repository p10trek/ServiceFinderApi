using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using ServiceFinderApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ServiceFinderApi.Controllers
{
    public class SmsController : MyControllerBase
    {
        
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ServiceFinderDBContext _context;

        string URL = @"https://api.mobitex.pl/sms.php?user=p10trek&pass=24699097bc0b38bbc785f1dc52028df4&type=sms&number=+48{phone}&text={code}&from=ServiceFinder&ext_id=7222";
        //string URL = @"https://api.smsapi.pl/sms.do?from=Test&to=48{phone}&message={code}&format=json";
        public SmsController(ServiceFinderDBContext context, IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            _context = context;
        }
        [HttpGet("/SendSms")]
        public async Task SendSms(string userName, string message)
        {

            var user = await (_context.Users.Where(r => r.Login == userName)).FirstOrDefaultAsync();
            if (user == null)
                return;

            Random random = new Random();

            var httpRequestMessage = new HttpRequestMessage(
            HttpMethod.Get,
            URL.Replace("{code}", message).Replace("{phone}", user.Phone))
            {
                Headers =
                {
                    { HeaderNames.Authorization, "Bearer E3n3UHacCWinZHeBmWzvcFoURHEU7QUPqkt8TG2X" }
                }
            };
            var httpClient = _httpClientFactory.CreateClient();
            var httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);
        }
        [HttpGet("/SendCodeSms")]
        public async Task SendCodeSms(string userName)
        {

            var user = await (_context.Users.Where(r => r.Login == userName)).FirstOrDefaultAsync();
            if (user == null)
                return;
            
            Random random = new Random();
            var code = random.Next(0, 1).ToString();
            code = code.PadLeft(5, '0');
            
            var httpRequestMessage = new HttpRequestMessage(
            HttpMethod.Get,
            URL.Replace("{code}",code).Replace("{phone}",user.Phone))
            {
                Headers =
                {
                    { HeaderNames.Authorization, "Bearer E3n3UHacCWinZHeBmWzvcFoURHEU7QUPqkt8TG2X" }
                }
            };
            var httpClient = _httpClientFactory.CreateClient();
            var httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                user.Code = code;
                _context.Update(user);
                await _context.SaveChangesAsync();
                using var contentStream = await httpResponseMessage.Content.ReadAsStreamAsync();
                using (var reader = new StreamReader(contentStream, Encoding.UTF8))
                {
                    string value = reader.ReadToEnd();
                }
            }
        }
        [HttpGet("/CodeVerify")]
        public async Task<bool> CodeVerify(string code, string userName)
        {
            var user = await (_context.Users.Where(r => r.Login == userName)).FirstOrDefaultAsync();
            if (user == null)
                return false;
            if (user.Code == code)
            {
                user.Code = null;
                _context.Update(user);
                await _context.SaveChangesAsync();
                return true;
            }
            else return false;
        }
    }
}
