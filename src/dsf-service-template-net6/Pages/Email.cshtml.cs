using dsf_service_template_net6.Data.Models;
using dsf_service_template_net6.Extensions;
using dsf_service_template_net6.Services;
using dsf_service_template_net6.Services.Model;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;


namespace dsf_service_template_net6.Pages
{
    public class EmailModel : PageModel
    {
        #region "Variables"
        //control variables
        [BindProperty]
        public string crbEmail { get; set; } = "";
        [BindProperty]
        public string DisplaySummary { get; set; } = "display:none";
        [BindProperty]
        public string ErrorsDesc { get; set; } = "";
        
        [BindProperty]
        //Store the email selection Error
        public string EmailSelection { get; set; } = "";
        [BindProperty]
        public string BackLink { get; set; } = "";
        [BindProperty]
        public string NextLink { get; set; } = "";
        //Dependancy injection Variables
        private readonly INavigation _nav;
        private readonly ITasks _service;
        private IValidator<EmailSection> _validator;
        //Object for session data 
        public EmailSection Email_select;
        #endregion
        #region "Custom Methods"
        public EmailModel(IValidator<EmailSection> validator, ITasks service, INavigation nav)
        {
            _service = service;
            _validator = validator;
            _nav = nav;
            Email_select = new EmailSection();
        }
        //Use to show error messages if web form has errors
        bool ShowErrors(bool fromPost)
        {
            if (fromPost)
            {
                var res = HttpContext.Session.GetObjectFromJson<ValidationResult>("valresult");
                // Copy the validation results into ModelState.
                // ASP.NET uses the ModelState collection to populate 
                // error messages in the View.
                res.AddToModelState(this.ModelState, "Email_select");
                //Update Error messages on View
                ClearErrors();
                SetViewErrorMessages(res);
                return true;
            }
            else
            {
                return false;
            }
        }
        void ClearErrors()
        {
            DisplaySummary = "display:none";
            EmailSelection = "";
            ErrorsDesc = "";
        }
        private void SetViewErrorMessages(FluentValidation.Results.ValidationResult result)
        {
            //First Enable Summary Display
            DisplaySummary = "display:block";
            //Then Build Summary Error
            foreach (ValidationFailure Item in result.Errors)
            {
                if (Item.PropertyName == "use_from_civil" || Item.PropertyName == "email")
                {
                    ErrorsDesc += "<a href='#crbEmail'>" + Item.ErrorMessage + "</a>";
                    EmailSelection = Item.ErrorMessage;
                }

            }
        }
        private bool AllowToProceed()
        {
            bool ret = true;
            if (GetCitizenDataFromApi == null)
            {
                ret = false;
            }
            return ret;
        }
        private static string GetLanguage()
        {
            return Thread.CurrentThread.CurrentUICulture.Name == "el-GR" ? "/el" : "/en";
        }
        private string GetAuthTime()
        {
            return User.Claims.First(c => c.Type == "auth_time").Value;
        }
        private TasksGetResponse GetCitizenDataFromApi()
        {
            TasksGetResponse res = HttpContext.Session.GetObjectFromJson<TasksGetResponse>("PersonalDetails", GetAuthTime());
            return res;
        }
        private EmailSection GetSessionData()
        {
            var selectedoptions = HttpContext.Session.GetObjectFromJson<EmailSection>("EmailSection", GetAuthTime());
            return selectedoptions;
        }
       
        private void BindSelectionData()
        {
            TasksGetResponse res = GetCitizenDataFromApi();
            //Set Email info to model class
            if (res?.data?.Count()==0)
            {

                Email_select.email = User.Claims.First(c => c.Type == "email").Value;
            }
            else
            {
                Email_select.email = res?.data?.First()?.name;
            }
        }
        private bool BindData()
        {   //Check if already selected 
            var selectedoptions = GetSessionData();
            if (selectedoptions != null)
            {
                if (selectedoptions.use_from_api)
                {
                    crbEmail = "1";
                }
                else if (selectedoptions.use_other && (selectedoptions.email == User.Claims.First(c => c.Type == "email").Value || selectedoptions.email == GetCitizenDataFromApi()?.data?.First()?.name))
                {
                    //code use when user hit back button on edit page
                    crbEmail = "1";
                    Email_select.use_from_api = true;
                    Email_select.use_other = false;
                    Email_select.email=GetCitizenDataFromApi()?.data?.Count() == 0? User.Claims.First(c => c.Type == "email").Value: GetCitizenDataFromApi()?.data?.First()?.name;
                    HttpContext.Session.SetObjectAsJson("EmailSection", Email_select, GetAuthTime());
                }
                else
                {
                    crbEmail = "2";
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion
        public IActionResult OnGet(bool review, bool fromPost)
        {
            //Chack if user has sequentialy load the page
            bool allow = AllowToProceed();
            if (!allow)
            {
                return RedirectToAction("LogOut", "Account");
            }
            // First set back link
            BackLink = _nav.GetBackLink("/email-selection", review);
            
            if (fromPost)
            {
                ShowErrors(true);
            }
            else
            {
                //check for revisit
                bool revisit = BindData();
                if (!revisit)
                {
                    //Check whether api data were retrieve from login , otherwise call again
                    TasksGetResponse res = GetCitizenDataFromApi();
                    if (res == null)
                    {
                        var lang = GetLanguage();

                        //Call the citizen personal details from civil registry
                        //
                        res = _service.GetAllTasks(HttpContext.GetTokenAsync("access_token")?.Result);
                        if (res.succeeded == false)
                        {
                            return RedirectToPage("/ServerError");
                        }
                        else
                        {
                            //if the user is already login and not passed from login, set in session
                            HttpContext.Session.SetObjectAsJson("PersonalDetails", res, GetAuthTime());

                        }
                    }
                   
                }
            }
            //Show selection Data 
            BindSelectionData();
            return Page();
        }
        public IActionResult OnPost(bool review)
        {
            //Set class Model before validation
            if (crbEmail == "1")
            {
                Email_select.use_from_api = true;
                Email_select.use_other = false;
            }
            else if (crbEmail == "2")
            {
                Email_select.use_from_api = false;
                Email_select.use_other = true;
            }
            else
            {
                Email_select.use_from_api = false;
                Email_select.use_other = false;
            }
            Email_select.validation_mode = ValidationMode.Edit;
            //Validate Model
            FluentValidation.Results.ValidationResult result = _validator.Validate(Email_select);
            if (!result.IsValid)
            {
                HttpContext.Session.SetObjectAsJson("valresult", result);
                return RedirectToPage("Email", null, new { fromPost = true }, "mainContainer");
            }
            //Model is valid so strore 
            HttpContext.Session.Remove("EmailSection");
            HttpContext.Session.SetObjectAsJson("EmailSection", Email_select, GetAuthTime());
            //Remove Error Session 
            HttpContext.Session.Remove("valresult");
           
            //Set back and Next Link

            if (Email_select.use_other)
            {
              NextLink=  _nav.SetLinks("email-selection","Email", review, "No");
            }
            else
            {
              NextLink = _nav.SetLinks("email-selection", "Email", review, "Yes");
            }

            if (review)
            {
                return RedirectToPage(NextLink, null, new { review = review }, "mainContainer");
            }
            else
            {
                return RedirectToPage(NextLink);
            }
        }
    }
}

