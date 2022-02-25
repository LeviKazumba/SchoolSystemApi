//using Newtonsoft.Json;
//using SchoolSystemApi.Models;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net;
//using System.Net.Http;
//using System.Web;
//using System.Web.Http;

//namespace SchoolSystemApi.Controllers
//{
//    public class AdminController : ApiController
//    {
//        Data.SchoolSystemEntities db = new Data.SchoolSystemEntities();
//        Methods api = new Methods();
//        string ZeroGuid = new Guid().ToString();

//        #region School

//        [HttpPost]
//        [Route("School/Create")]
//        public object CreateSchool()
//        {
//            var gm = new GenericModel();
//            string UserAccess;
//            string Check;
//            int LogID = 0;
//            string Activity = "You created a new school on " + DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss");
//            string ActivityError = "You failed to create a new school on " + DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss");

//            string Subject = "";
//            string Content = "";

//            try
//            {
//                if (!Request.Headers.Contains("ApiKey"))
//                {
//                    throw new Exception("Access denied, unauthorized");
//                }

//                var httpRequest = HttpContext.Current.Request;

//                string ApiKey = Request.Headers.GetValues("ApiKey").FirstOrDefault();

//                Dictionary<string, string> Req = httpRequest.Params.AllKeys.ToDictionary(x => x, x => httpRequest.Params[x]);

//                LogID = api.Log(LogID, ApiKey, "CreateSchool", Activity, JsonConvert.SerializeObject(Req), null);

//                if (string.IsNullOrEmpty(ApiKey))
//                {
//                    throw new Exception("Access denied, unauthorized");
//                }

//                if ((UserAccess = api.IsSystemManager(ApiKey)) == "NO")
//                {
//                    throw new Exception("Access denied, unauthorized");
//                }

//                if ((UserAccess = api.CheckUserAccess(ApiKey, "SchoolWrite")) == "NO")
//                {
//                    throw new Exception("Access denied, unauthorized");
//                }

//                //school validation
//                if (string.IsNullOrEmpty(httpRequest.Params["SchoolName"]))
//                {
//                    throw new Exception("School name required");
//                }

//                string SchoolName = httpRequest.Params["SchoolName"];

//                if (string.IsNullOrEmpty(httpRequest.Params["SchoolType"]))
//                {
//                    throw new Exception("Education level required");
//                }

//                int SchoolType = Convert.ToInt32(httpRequest.Params["SchoolType"]);

//                if (string.IsNullOrEmpty(httpRequest.Params["Email"]))
//                {
//                    throw new Exception("School email required");
//                }

//                string Email = httpRequest.Params["Email"];

//                if (string.IsNullOrEmpty(httpRequest.Params["Telephone"]))
//                {
//                    throw new Exception("School telephone required");
//                }

//                string Telephone = httpRequest.Params["Telephone"];

//                if (string.IsNullOrEmpty(httpRequest.Params["Country"]))
//                {
//                    throw new Exception("Country name required");
//                }

//                string Country = httpRequest.Params["Country"];


//                if (string.IsNullOrEmpty(httpRequest.Params["Address"]))
//                {
//                    throw new Exception("School address required");
//                }

//                string Address = httpRequest.Params["Address"];

//                HttpPostedFile Logo = httpRequest.Files[0];

//                if (Logo.ContentLength <= 0)
//                {
//                    throw new Exception("School logo required");
//                }

//                if (string.IsNullOrEmpty(httpRequest.Params["CompanyRegistrationNumber"]))
//                {
//                    throw new Exception("Company registration number required");
//                }

//                string CompanyRegistrationNumber = httpRequest.Params["CompanyRegistrationNumber"];

//                if ((Check = api.SchoolDuplicate(Email, "Email")) == "YES")
//                {
//                    throw new Exception("The email provided is already taken, please try another email");
//                }

//                if ((Check = api.SchoolDuplicate(Telephone, "Telephone")) == "YES")
//                {
//                    throw new Exception("The telephone number provided is already taken, please try another number");
//                }

//                //create school
//                var sc = new Data.School();

//                sc.SchoolID = Guid.NewGuid().ToString();
//                sc.SchoolName = SchoolName;
//                sc.SchoolType = SchoolType;
//                sc.Email = Email;
//                sc.Telephone = Telephone;
//                sc.Country = Country;
//                sc.Address = Address;

//                string path = api.UploadFile(Logo);

//                sc.Logo = path;
//                sc.Active = true;
//                sc.CompanyRegistrationNumber = CompanyRegistrationNumber;
//                sc.DateCreated = DateTime.Now;
//                sc.CreatedBy = ApiKey;

//                db.Schools.Add(sc);
//                db.SaveChanges();


//                //send email
//                //Content = System.IO.File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + @"MailTemplates\ForgotPassword.html");
//                //    Content = Content.Replace("{Name}", User.Name);
//                //    Content = Content.Replace("{Username}", User.Username);
//                //    Content = Content.Replace("{Password}", User.Password);

//                //    bool CC = false;

//                //    SendEmail(User.Email, Content, Subject, CC, "Support");


//                gm.Status = "Success";
//                gm.Data = SchoolName + " has been added";

//            }
//            catch (Exception ex)
//            {
//                string ErrorData = ex.Message + "\n" + ex.InnerException + "\n" + ex.StackTrace;
//                api.Log(LogID, "", "", ActivityError, "", ErrorData);

//                gm.Status = "Failed";
//                gm.Data = ex.Message;
//            }

//            return gm;
//        }

//        [HttpPost]
//        [Route("Admin/School/Deactivate/{SchoolID}")]
//        public object DeactivateSchool(string SchoolID)
//        {
//            var gm = new GenericModel();
//            string UserAccess;
//            int LogID = 0;
//            string Activity = "You deactivated a user account on " + DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss");
//            string ActivityError = "You failed to deactivate a user account on " + DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss");

//            try
//            {
//                if (!Request.Headers.Contains("ApiKey"))
//                {
//                    throw new Exception("Access denied, unauthorized");
//                }

//                string ApiKey = Request.Headers.GetValues("ApiKey").FirstOrDefault();

//                LogID = api.Log(LogID, ApiKey, "DeactivateSchool", Activity, JsonConvert.SerializeObject(SchoolID), null);

//                if (string.IsNullOrEmpty(ApiKey))
//                {
//                    throw new Exception("Access denied, unauthorized");
//                }

//                if ((UserAccess = api.IsSystemManager(ApiKey)) == "NO")
//                {
//                    throw new Exception("Access denied, unauthorized");
//                }

//                if ((UserAccess = api.CheckUserAccess(ApiKey, "SchoolWrite")) == "NO")
//                {
//                    throw new Exception("Access denied, unauthorized");
//                }

//                if (string.IsNullOrEmpty(SchoolID))
//                {
//                    throw new Exception("School ID required");
//                }

//                var s = (from a in db.Schools where a.SchoolID == SchoolID select a).FirstOrDefault();

//                if (s != null)
//                {
//                    s.Email = "%" + s.Email;
//                    s.Telephone = "%" + s.Telephone;
//                    s.Active = false;
//                    s.ModifiedBy = ApiKey;
//                    s.DateModified = DateTime.Now;
//                    db.SaveChanges();

//                    gm.Status = "Success";
//                    gm.Data = s.SchoolName + " has been deactivated";
//                }
//                else
//                {
//                    throw new Exception("School not found");
//                }
//            }

//            catch (Exception ex)
//            {
//                string ErrorData = ex.Message + "\n" + ex.InnerException + "\n" + ex.StackTrace;
//                api.Log(LogID, "", "", ActivityError, "", ErrorData);

//                gm.Status = "Failed";
//                gm.Data = ex.Message;
//            }
//            return gm;
//        }

//        [HttpPost]
//        [Route("Admin/School/Activate/{SchoolID}")]
//        public object ActivateSchool(string SchoolID)
//        {

//            var gm = new GenericModel();
//            string UserAccess;
//            string Check;
//            int LogID = 0;
//            string Activity = "You activated a user account on " + DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss");
//            string ActivityError = "You failed to activate a user account on " + DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss");

//            string Email = "";
//            string Telephone = "";
//            try
//            {
//                if (!Request.Headers.Contains("ApiKey"))
//                {
//                    throw new Exception("Access denied, unauthorized");
//                }

//                string ApiKey = Request.Headers.GetValues("ApiKey").FirstOrDefault();

//                LogID = api.Log(LogID, ApiKey, "ActivateSchool", Activity, JsonConvert.SerializeObject(SchoolID), null);

//                if (string.IsNullOrEmpty(ApiKey))
//                {
//                    throw new Exception("Access denied, unauthorized");
//                }

//                if ((UserAccess = api.IsSystemManager(ApiKey)) == "NO")
//                {
//                    throw new Exception("Access denied, unauthorized");
//                }

//                if ((UserAccess = api.CheckUserAccess(ApiKey, "SchoolWrite")) == "NO")
//                {
//                    throw new Exception("Access denied, unauthorized");
//                }

//                if (string.IsNullOrEmpty(SchoolID))
//                {
//                    throw new Exception("School ID required");
//                }

//                var s = (from a in db.Schools where a.SchoolID == SchoolID select a).FirstOrDefault();

//                if (s != null)
//                {
//                    Email = s.Email.Replace("%", "");
//                    Telephone = s.Telephone.Replace("%", "");

//                    if ((Check = api.UserDuplicate(Email, "Email")) == "YES")
//                    {
//                        throw new Exception("Email: " + s.Email + " is already in use. Please contact tech support for assistance in activating this account");
//                    }

//                    if ((Check = api.UserDuplicate(Telephone, "Telephone")) == "YES")
//                    {
//                        throw new Exception("Telephone: " + s.Telephone + ", is already in use. Please contact tech support for assistance in activating this account");
//                    }

//                    s.Email = Email;
//                    s.Telephone = Telephone;
//                    s.Active = true;
//                    s.DateModified = DateTime.Now;
//                    db.SaveChanges();

//                    gm.Status = "Success";
//                    gm.Data = s.SchoolName + " has been activated";
//                }
//                else
//                {
//                    throw new Exception("School not found");
//                }
//            }

//            catch (Exception ex)
//            {
//                string ErrorData = ex.Message + "\n" + ex.InnerException + "\n" + ex.StackTrace;
//                api.Log(LogID, "", "", ActivityError, "", ErrorData);

//                gm.Status = "Failed";
//                gm.Data = ex.Message;
//            }
//            return gm;
//        }

//        #endregion
//    }
//}
