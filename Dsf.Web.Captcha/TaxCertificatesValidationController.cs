using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using TaxPortal.Helpers;
using TaxPortal.Models;
using TaxPortal.Services;

namespace TaxPortalCore3.Controllers
{
    [AutoValidateAntiforgeryToken]
    [AllowAnonymous]
    public class TaxCertificatesValidationController : Controller
    {
        private IConfiguration _configuration;
        private readonly IStringLocalizer<TaxCertificatesValidationController> _localizer;
        IDataProtector _protector;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private ITokenService _tokenService;

        public TaxCertificatesValidationController(IDataProtectionProvider provider,
            IConfiguration config,
            IStringLocalizer<TaxCertificatesValidationController> localizer,
            IHttpContextAccessor httpContextAccessor,
            ITokenService tokenService)
        {
            _configuration = config;
            _localizer = localizer;
            _protector = provider.CreateProtector(GetType().FullName);
            _httpContextAccessor = httpContextAccessor;
            _tokenService = tokenService;
        }

        public IActionResult TaxCertificatesValidationForm()
        {
            //Validate the certificate and load new Result View
            return View();
        }

        public IActionResult TaxCertificatesValidation(string ticNumber, string referenceCode, string captchaCode)
        {

            IActionResult response = BadRequest();

            if (string.IsNullOrEmpty(ticNumber) || string.IsNullOrEmpty(referenceCode))
            {
                return response;
            }

            TaxCertificateValidationViewModel model = new TaxCertificateValidationViewModel();
            if (!Captcha.ValidateCaptchaCode(captchaCode, HttpContext))
            {
                ModelState.AddModelError("CaptchaCode", "Invalid captcha");
                model.ReferenceCode = "";
                model.CaptchaCode = "";
               
                return View("TaxCertificatesValidationForm", model);
            }

            TaxCertificateValidationRequestModel tcv = new TaxCertificateValidationRequestModel();
            tcv.ReferenceCode = referenceCode;
            tcv.Tic = ticNumber;

            string res = GetTaxCertificateByReferenceAsync(tcv);

            TaxCertificate tc = new TaxCertificate();            

            try
            {
                tc = JsonConvert.DeserializeObject<RootTaxCertificate>(res).TaxCertificate;

                if (tc != null)
                {
                    tc.ReferenceCode = referenceCode;
                }

            }
            catch (Exception ex)
            {
                //return RedirectToAction("Main", "Home");
            }

            return View("TaxCertificateValidationResult", tc);

        }

        private string GetTaxCertificateByReferenceAsync(TaxCertificateValidationRequestModel tcv)
        {
            string res;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            string baseUrl = _configuration["AuthWebApiBaseUri"];

            HttpClientHandler httpClientHandler = new HttpClientHandler();
            httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
            HttpClient httpClient = new HttpClient(httpClientHandler);

            httpClient.BaseAddress = new Uri(baseUrl);
            //This is the ApiKey for the TaxPortal
            //It should be included in the header of every request to the AuthWebApi
            httpClient.DefaultRequestHeaders.Add("user-key", _configuration["UserKey"]);
            //httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            //HttpContent inputContent = new StringContent(string.Empty, Encoding.UTF8, "application/json");
            Task<HttpResponseMessage> response = httpClient.PostAsJsonAsync($"api/TaxCertificatesValidation/Validate", tcv);
            response.Wait(TimeSpan.FromSeconds(10));

            if (response.IsCompleted)
            {
                if (response.Result.StatusCode == HttpStatusCode.OK)
                {
                    res = response.Result.Content.ReadAsStringAsync().Result;
                }
                else
                {
                    res = response.Result.StatusCode.ToString();
                }
            }
            else
            {
                res = null;
            }

            return res;
        }

    }
}
