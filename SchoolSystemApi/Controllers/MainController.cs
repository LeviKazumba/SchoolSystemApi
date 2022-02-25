//using CloudinaryDotNet;
//using CloudinaryDotNet.Actions;
//using SchoolSystemApi.Models;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Net.Mail;
//using System.Text;
//using System.Web;
//using System.Web.Http;


//namespace SchoolSystemApi.Controllers
//{
//    public class MainController : ApiController
//    {
//        #region Global
//        string SMTP = System.Configuration.ConfigurationManager.AppSettings["SMTP"].ToString();
//        string SMTPPort = System.Configuration.ConfigurationManager.AppSettings["SMTPPort"].ToString();
//        string SMTPUserName = System.Configuration.ConfigurationManager.AppSettings["SMTPUserName"].ToString();
//        string SMTPPassword = System.Configuration.ConfigurationManager.AppSettings["SMTPPassword"].ToString();

//        string DefaultLogo = System.Configuration.ConfigurationManager.AppSettings["DefaultLogo"].ToString();

//        Data.KazumbaFoundationEntities kz = new Data.KazumbaFoundationEntities();
//        Data.SchoolSystemEntities ss = new Data.SchoolSystemEntities();


//        static readonly Account account = new Account(
//             "kazumbafoundation",
//             "473666744374452",
//             "5P1OtG-jl97yFrfzIquZIRWy3ww");

//        Cloudinary cloudinary = new Cloudinary(account);
//        #endregion


//        #region Admin

//        [HttpPost]
//        [Route("Admin/School/Create")]
//        public object CreateSchool()
//        {
//            var gm = new GenericModel();
//            string UserFunction;
//            string Subject = "";
//            string Content = "";
//            string check;
//            try
//            {

//                var httpRequest = HttpContext.Current.Request;

//                if (string.IsNullOrEmpty(httpRequest.Params["UserID"]))
//                {
//                    throw new Exception("Access denied, unauthorized");
//                }

//                //Validate Admin User
//                string UserID = httpRequest.Params["UserID"];

//                if ((UserFunction = VerifyUserFunction(UserID, "CreateSchool")) == "Denied")
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

//                string SchoolType = httpRequest.Params["SchoolType"];

//                if (string.IsNullOrEmpty(httpRequest.Params["SchoolEmail"]))
//                {
//                    throw new Exception("School email required");
//                }

//                string SchoolEmail = httpRequest.Params["SchoolEmail"];

//                if (string.IsNullOrEmpty(httpRequest.Params["SchoolTelephone"]))
//                {
//                    throw new Exception("School telephone required");
//                }

//                string SchoolTelephone = httpRequest.Params["SchoolTelephone"];

//                if (string.IsNullOrEmpty(httpRequest.Params["CountryCode"]))
//                {
//                    throw new Exception("International dialling code required");
//                }

//                string CountryCode = httpRequest.Params["CountryCode"];

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

//                if (Logo.ContentLength == 0)
//                {
//                    throw new Exception("School logo required");
//                }

//                if (string.IsNullOrEmpty(httpRequest.Params["CompanyRegistrationNumber"]))
//                {
//                    throw new Exception("Company registration number required");
//                }

//                string CompanyRegistrationNumber = httpRequest.Params["CompanyRegistrationNumber"];


//                if (string.IsNullOrEmpty(httpRequest.Params["UserEmail"]))
//                {
//                    throw new Exception("User's email required");
//                }

//                string UserEmail = httpRequest.Params["UserEmail"];

//                if (string.IsNullOrEmpty(httpRequest.Params["Name"]))
//                {
//                    throw new Exception("User's name required");
//                }

//                string Name = httpRequest.Params["Name"];

//                if (string.IsNullOrEmpty(httpRequest.Params["Surname"]))
//                {
//                    throw new Exception("User's surname required");
//                }

//                string Surname = httpRequest.Params["Surname"];

//                if (string.IsNullOrEmpty(httpRequest.Params["UserTelephone"]))
//                {
//                    throw new Exception("User's telephone required");
//                }

//                string UserTelephone = httpRequest.Params["UserTelephone"];

//                if ((check = VerifyDuplicate(SchoolEmail, "Email", "SchoolTable")) == "Duplicate")
//                {
//                    throw new Exception("The school email provided is already in use, please try another email");
//                }

//                if ((check = VerifyDuplicate(UserEmail, "Email", "UserTable")) == "Duplicate")
//                {
//                    throw new Exception("The user's email provided is already in use, please try another email");
//                }

//                if ((check = VerifyDuplicate(SchoolName, "Name", "SchoolTable")) == "Duplicate")
//                {
//                    throw new Exception("The school name provided is already in use, please try another name");
//                }

//                //create school
//                var sc = new SS_db.School();

//                sc.School_ID = Guid.NewGuid().ToString();
//                sc.SchoolName = SchoolName;
//                sc.SchoolType = SchoolType;
//                sc.Email = SchoolEmail;
//                sc.Telephone = SchoolTelephone;
//                sc.Country = Country;
//                sc.CountryCode = CountryCode;
//                sc.Address = Address;

//                string path = UploadFile(Logo);

//                sc.Logo = path;
//                sc.Active = true;
//                sc.CompanyRegistrationNumber = CompanyRegistrationNumber;
//                sc.DateCreated = DateTime.Now;
//                sc.DateModified = DateTime.Now;

//                ss.Schools.Add(sc);
//                ss.SaveChanges();

//                //Add school principal
//                var u = new KZ_db.User();

//                u.UserID = Guid.NewGuid().ToString();
//                u.Password = Guid.NewGuid().ToString().Substring(1, 6);
//                u.LastPassword = u.Password;
//                u.Email = UserEmail;
//                u.Name = Name;
//                u.Surname = Surname;
//                u.Telephone = UserTelephone;
//                u.Country = Country;
//                u.CountryCode = CountryCode;
//                u.DateCreated = DateTime.Now;
//                u.DateModified = DateTime.Now;
//                u.Active = true;

//                kz.Users.Add(u);
//                kz.SaveChanges();

//                //Add principal functions
//                var a = new SS_db.User();

//                a.UserType = "SchoolManagement";
//                a.UserRole = "Principal";
//                a.LastLogin = DateTime.Now;
//                a.LoginAttempts = 0;
//                a.LockedOut = false;
//                a.Active = true;
//                a.DateCreated = u.DateCreated;
//                a.DateModified = u.DateModified;

//                a.UserID = u.UserID;
//                a.SetUserFunctions = true;
//                a.CreateSchool = false;
//                a.UpdateSchool = true;
//                a.ActivateSchool = false;
//                a.DeactivateSchool = false;
//                a.GetSchool = true;
//                a.GetAllSchools = false;
//                a.CreateUser = false;
//                a.ActivateUser = false;
//                a.DeactivateUser = false;
//                a.LockUser = true;
//                a.UnlockUser = true;

//                ss.Users.Add(a);
//                ss.SaveChanges();


//                //Add principal to School relationship
//                var m = new SS_db.UsersAndSchool();

//                m.School_ID = sc.School_ID;
//                m.UserID = u.UserID;

//                ss.UsersAndSchools.Add(m);
//                ss.SaveChanges();


//                //reuires email


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
//                gm.Status = "Failed";
//                gm.Data = ex.Message;
//            }

//            return gm;
//        }


//        [HttpPost]
//        [Route("Admin/School/Update")]
//        public object UpdateSchool()
//        {
//            var gm = new GenericModel();
//            string UserFunction;
//            string check;
//            try
//            {

//                var httpRequest = HttpContext.Current.Request;

//                if (string.IsNullOrEmpty(httpRequest.Params["UserID"]))
//                {
//                    throw new Exception("Access denied, unauthorized");
//                }

//                //Validate Admin User
//                string UserID = httpRequest.Params["UserID"];

//                if ((UserFunction = VerifyUserFunction(UserID, "CreateSchool")) == "Denied")
//                {
//                    throw new Exception("Access denied, unauthorized");
//                }

//                //school validation
//                if (string.IsNullOrEmpty(httpRequest.Params["School_ID"]))
//                {
//                    throw new Exception("School ID required");
//                }

//                string School_ID = httpRequest.Params["School_ID"];

//                if (string.IsNullOrEmpty(httpRequest.Params["SchoolName"]))
//                {
//                    throw new Exception("School name required");
//                }

//                string SchoolName = httpRequest.Params["SchoolName"];

//                if (string.IsNullOrEmpty(httpRequest.Params["SchoolType"]))
//                {
//                    throw new Exception("Education level required");
//                }

//                string SchoolType = httpRequest.Params["SchoolType"];

//                if (string.IsNullOrEmpty(httpRequest.Params["SchoolEmail"]))
//                {
//                    throw new Exception("School email required");
//                }

//                string SchoolEmail = httpRequest.Params["SchoolEmail"];

//                if (string.IsNullOrEmpty(httpRequest.Params["SchoolTelephone"]))
//                {
//                    throw new Exception("School telephone required");
//                }

//                string SchoolTelephone = httpRequest.Params["SchoolTelephone"];

//                if (string.IsNullOrEmpty(httpRequest.Params["CountryCode"]))
//                {
//                    throw new Exception("International dialling code required");
//                }

//                string CountryCode = httpRequest.Params["CountryCode"];

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

//                if (string.IsNullOrEmpty(httpRequest.Params["CompanyRegistrationNumber"]))
//                {
//                    throw new Exception("Company registration number required");
//                }

//                string CompanyRegistrationNumber = httpRequest.Params["CompanyRegistrationNumber"];


//                //create school
//                var sc = (from s in ss.Schools where School_ID == s.School_ID select s).FirstOrDefault();

//                if (sc != null)
//                {

//                    if (SchoolName != sc.SchoolName)
//                    {
//                        if ((check = VerifyDuplicate(SchoolName, "Name", "SchoolTable")) == "Duplicate")
//                        {
//                            throw new Exception("The school name provided is already in use, please try another name");
//                        }

//                        sc.SchoolName = SchoolName;
//                    }

//                    if (SchoolEmail != sc.Email)
//                    {
//                        if ((check = VerifyDuplicate(SchoolEmail, "Email", "SchoolTable")) == "Duplicate")
//                        {
//                            throw new Exception("The school email provided is already in use, please try another email");
//                        }

//                        sc.Email = SchoolEmail;
//                    }

//                    sc.SchoolType = SchoolType;
//                    sc.Telephone = SchoolTelephone;
//                    sc.Country = Country;
//                    sc.CountryCode = CountryCode;
//                    sc.Address = Address;

//                    if (httpRequest.Files[0].ContentLength > 0)
//                    {
//                        HttpPostedFile Logo = httpRequest.Files[0];

//                        string path = UploadFile(Logo);
//                        sc.Logo = path;
//                    }

//                    sc.CompanyRegistrationNumber = CompanyRegistrationNumber;
//                    sc.DateModified = DateTime.Now;

//                    ss.SaveChanges();

//                    gm.Status = "Success";
//                    gm.Data = SchoolName + " has been updated";
//                }
//                else
//                {
//                    gm.Status = "Failed";
//                    gm.Data = "Update failed! invalid school ID provided";
//                }

//            }
//            catch (Exception ex)
//            {
//                gm.Status = "Failed";
//                gm.Data = ex.Message;
//            }

//            return gm;
//        }

//        [HttpPost]
//        [Route("Admin/School/All/{UserID}")]
//        public object GetAllSchools(string UserID)
//        {
//            var gm = new GetSchool();
//            string UserFunction;
//            try
//            {
//                //Validate Admin User

//                if (string.IsNullOrEmpty(UserID))
//                {
//                    throw new Exception("Access denied, unauthorized");
//                }

//                if ((UserFunction = VerifyUserFunction(UserID, "GetSchool")) == "Denied")
//                {
//                    throw new Exception("Access denied, unauthorized");
//                }

//                var s = (from a in ss.Schools
//                         select new GetSchool
//                         {
//                             Status = "Success",
//                             School_ID = a.School_ID,
//                             SchoolName = a.SchoolName,
//                             SchoolType = a.SchoolType,
//                             Email = a.Email,
//                             Telephone = a.Telephone,
//                             Country = a.Country,
//                             CountryCode = a.CountryCode,
//                             Address = a.Address,
//                             Active = a.Active,
//                             CompanyRegistrationNumber = a.CompanyRegistrationNumber,
//                             DateCreated = (DateTime)a.DateCreated,
//                             DateModified = (DateTime)a.DateModified

//                         }).ToList();

//                return s;

//            }
//            catch (Exception ex)
//            {
//                gm.Status = "Failed";
//                gm.Data = ex.Message;

//                return gm;
//            }


//        }

//        [HttpPost]
//        [Route("Admin/School/{UserID}/{School_ID}")]
//        public object GetSchool(string UserID, string School_ID)
//        {
//            var gs = new GetSchool();
//            string UserFunction;
//            try
//            {
//                //Validate Admin User
//                if (string.IsNullOrEmpty(UserID))
//                {
//                    throw new Exception("Access denied, unauthorized");
//                }

//                if ((UserFunction = VerifyUserFunction(UserID, "GetSchool")) == "Denied")
//                {
//                    throw new Exception("Access denied, unauthorized");
//                }

//                if (string.IsNullOrEmpty(School_ID))
//                {
//                    throw new Exception("School ID required");
//                }

//                var s = (from a in ss.Schools where a.School_ID == School_ID select a).FirstOrDefault();

//                if (s != null)
//                {
//                    gs.Status = "Success";

//                    gs.School_ID = s.School_ID;
//                    gs.SchoolName = s.SchoolName;
//                    gs.SchoolType = s.SchoolType;
//                    gs.Email = s.Email;
//                    gs.Telephone = s.Telephone;
//                    gs.Country = s.Country;
//                    gs.CountryCode = s.CountryCode;
//                    gs.Address = s.Address;
//                    gs.Logo = s.Logo;
//                    gs.Active = s.Active;
//                    gs.CompanyRegistrationNumber = s.CompanyRegistrationNumber;

//                }
//                else
//                {
//                    throw new Exception("Invalid school ID");
//                }
//            }

//            catch (Exception ex)
//            {
//                gs.Status = "Failed";
//                gs.Data = ex.Message;
//            }
//            return gs;
//        }


//       

//        [HttpPost]
//        [Route("Admin/User/Create")]
//        public object CreateUser(NewUser n)
//        {

//            string Subject = "";
//            string Content = "";
//            bool CC = false;
//            List<string> CC_Emails = null;
//            var school = new SS_db.School();
//            var gm = new GenericModel();
//            string UserFunction;
//            try
//            {

//                //Validate UserID
//                if (n.CreatedBy == "Admin")
//                {
//                    if (string.IsNullOrEmpty(n.UserID))
//                    {
//                        throw new Exception("Access denied, unauthorized");
//                    }


//                    if ((UserFunction = VerifyUserFunction(n.UserID, "CreateUser")) == "Denied")
//                    {
//                        throw new Exception("Access denied, unauthorized");
//                    }

//                }

//                //validate input

//                if (string.IsNullOrEmpty(n.Name))
//                {
//                    throw new Exception("Name required");
//                }

//                if (string.IsNullOrEmpty(n.Surname))
//                {
//                    throw new Exception("Surname required");
//                }

//                if (string.IsNullOrEmpty(n.Email))
//                {
//                    throw new Exception("Email required");
//                }

//                if (string.IsNullOrEmpty(n.Telephone))
//                {
//                    throw new Exception("Telephone required");
//                }

//                if (string.IsNullOrEmpty(n.CountryCode))
//                {
//                    throw new Exception("International dialling code required");
//                }

//                if (string.IsNullOrEmpty(n.Country))
//                {
//                    throw new Exception("Country name required");
//                }

//                if (string.IsNullOrEmpty(n.UserType))
//                {
//                    throw new Exception("User type required");
//                }

//                if (string.IsNullOrEmpty(n.UserRole))
//                {
//                    throw new Exception("User role required");

//                }

//                if (n.UserType != "SystemAdmin")
//                {
//                    if (string.IsNullOrEmpty(n.School_ID))
//                    {
//                        throw new Exception("School ID required");
//                    }
//                }

//                string check = VerifyDuplicate(n.Email, "Email", "UserTable");

//                if (check == "Duplicate")
//                {
//                    throw new Exception("The email provided is already in use, please try another email");
//                }

//                //Create User in KazumbaFoundation db
//                var u = new KZ_db.User();

//                u.UserID = Guid.NewGuid().ToString("N")

//                if (n.CreatedBy == "Admin")
//                {
//                    //Generate random password
//                    u.Password = Guid.NewGuid().ToString().Substring(1, 6);
//                    u.LastPassword = u.Password;
//                }
//                else
//                {
//                    if (string.IsNullOrEmpty(n.Password))
//                    {
//                        throw new Exception("Password required");
//                    }

//                    u.Password = n.Password;
//                    u.LastPassword = u.Password;
//                }

//                u.Name = n.Name;
//                u.Surname = n.Surname;
//                u.Email = n.Email;
//                u.Telephone = n.Telephone;
//                u.Country = n.Country;
//                u.CountryCode = n.CountryCode;
//                u.DateCreated = DateTime.Now;
//                u.DateModified = DateTime.Now;
//                u.Active = true;

//                kz.Users.Add(u);
//                kz.SaveChanges();

//                //Create User in SchoolSystem db
//                var b = new SS_db.User();

//                b.UserID = u.UserID;
//                b.UserType = n.UserType;
//                b.UserRole = n.UserRole;
//                b.LastLogin = DateTime.Now;
//                b.LoginAttempts = 0;
//                b.LockedOut = false;
//                b.Active = true;
//                b.DateCreated = u.DateCreated;
//                b.DateModified = u.DateModified;


//                //Add User to school if required
//                if (n.UserType != "SystemAdmin")
//                {
//                    var m = new SS_db.UsersAndSchool();

//                    m.School_ID = n.School_ID;
//                    m.UserID = u.UserID;

//                    ss.UsersAndSchools.Add(m);
//                    ss.SaveChanges();

//                    school = (from a in ss.Schools where a.School_ID == n.School_ID select a).FirstOrDefault();
//                }

//                //Add User to functions table

//                b.UserID = u.UserID;
//                b.UserType = n.UserType;
//                b.UserRole = n.UserRole;

//                if (n.UserType == "SystemAdmin")
//                {
//                    if (n.UserRole == "Manager")
//                    {
//                        b.SetUserFunctions = true;
//                        b.CreateSchool = true;
//                        b.UpdateSchool = true;
//                        b.ActivateSchool = true;
//                        b.DeactivateSchool = true;
//                        b.GetSchool = true;
//                        b.GetAllSchools = true;
//                        b.CreateUser = true;
//                        b.ActivateUser = true;
//                        b.DeactivateUser = true;
//                        b.LockUser = true;
//                        b.UnlockUser = true;
//                    }

//                    if (n.UserRole == "Standard")
//                    {
//                        b.SetUserFunctions = false;
//                        b.CreateSchool = false;
//                        b.UpdateSchool = false;
//                        b.ActivateSchool = true;
//                        b.DeactivateSchool = true;
//                        b.GetSchool = true;
//                        b.GetAllSchools = true;
//                        b.CreateUser = false;
//                        b.ActivateUser = false;
//                        b.DeactivateUser = false;
//                        b.LockUser = false;
//                        b.UnlockUser = false;
//                    }

//                }

//                if (n.UserType == "SchoolManagement")
//                {
//                    if (n.UserRole == "Principal")
//                    {
//                        b.SetUserFunctions = true;
//                        b.CreateSchool = false;
//                        b.UpdateSchool = true;
//                        b.ActivateSchool = false;
//                        b.DeactivateSchool = false;
//                        b.GetSchool = true;
//                        b.GetAllSchools = false;
//                        b.CreateUser = false;
//                        b.ActivateUser = true;
//                        b.DeactivateUser = true;
//                        b.LockUser = false;
//                        b.UnlockUser = false;
//                    }

//                    if (n.UserRole == "SGB")
//                    {
//                        b.SetUserFunctions = false;
//                        b.CreateSchool = false;
//                        b.UpdateSchool = false;
//                        b.ActivateSchool = false;
//                        b.DeactivateSchool = false;
//                        b.GetSchool = true;
//                        b.GetAllSchools = false;
//                        b.CreateUser = false;
//                        b.ActivateUser = false;
//                        b.DeactivateUser = false;
//                        b.LockUser = false;
//                        b.UnlockUser = false;
//                    }

//                    if (n.UserRole == "Secretary")
//                    {
//                        b.SetUserFunctions = false;
//                        b.CreateSchool = false;
//                        b.UpdateSchool = true;
//                        b.ActivateSchool = false;
//                        b.DeactivateSchool = false;
//                        b.GetSchool = true;
//                        b.GetAllSchools = false;
//                        b.CreateUser = false;
//                        b.ActivateUser = false;
//                        b.DeactivateUser = false;
//                        b.LockUser = false;
//                        b.UnlockUser = false;
//                    }

//                }

//                if (n.UserType == "SchoolTeacher")
//                {
//                    if (n.UserRole == "HOD")
//                    {
//                        b.SetUserFunctions = false;
//                        b.CreateSchool = false;
//                        b.UpdateSchool = false;
//                        b.ActivateSchool = false;
//                        b.DeactivateSchool = false;
//                        b.GetSchool = true;
//                        b.GetAllSchools = false;
//                        b.CreateUser = false;
//                        b.ActivateUser = false;
//                        b.DeactivateUser = false;
//                        b.LockUser = false;
//                        b.UnlockUser = false;
//                    }

//                    if (n.UserRole == "Standard")
//                    {
//                        b.SetUserFunctions = false;
//                        b.CreateSchool = false;
//                        b.UpdateSchool = false;
//                        b.ActivateSchool = false;
//                        b.DeactivateSchool = false;
//                        b.GetSchool = true;
//                        b.GetAllSchools = false;
//                        b.CreateUser = false;
//                        b.ActivateUser = false;
//                        b.DeactivateUser = false;
//                        b.LockUser = false;
//                        b.UnlockUser = false;
//                    }
//                }

//                if (n.UserType == "SchoolParent")
//                {
//                    b.SetUserFunctions = false;
//                    b.CreateSchool = false;
//                    b.UpdateSchool = false;
//                    b.ActivateSchool = false;
//                    b.DeactivateSchool = false;
//                    b.GetSchool = true;
//                    b.GetAllSchools = false;
//                    b.CreateUser = false;
//                    b.ActivateUser = false;
//                    b.DeactivateUser = false;
//                    b.LockUser = false;
//                    b.UnlockUser = false;
//                }

//                if (n.UserType == "SchoolStudent")
//                {
//                    b.SetUserFunctions = false;
//                    b.CreateSchool = false;
//                    b.UpdateSchool = false;
//                    b.ActivateSchool = false;
//                    b.DeactivateSchool = false;
//                    b.GetSchool = true;
//                    b.GetAllSchools = false;
//                    b.CreateUser = false;
//                    b.ActivateUser = false;
//                    b.DeactivateUser = false;
//                    b.LockUser = false;
//                    b.UnlockUser = false;
//                }

//                ss.Users.Add(b);
//                ss.SaveChanges();


//                //Send emails

//                if (n.CreatedBy == "Admin")
//                {
//                    if (n.UserType != "SystemAdmin")
//                    {
//                        //Send user email that their account was created by respective school admin


//                        Content = System.IO.File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + @"MailTemplates\ForgotPassword.html");
//                        Content = Content.Replace("{Name}", u.Name);
//                        Content = Content.Replace("{Username}", u.Email);

//                        Subject = "Welcome to " + school.SchoolName;
//                        SendEmail(school.SchoolName, u.Email, Content, Subject, CC, CC_Emails);
//                    }
//                    else
//                    {
//                        //Send schoolsystem admin user email that his account was created

//                        Content = System.IO.File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + @"MailTemplates\ForgotPassword.html");
//                        Content = Content.Replace("{Name}", u.Name);
//                        Content = Content.Replace("{Username}", u.Email);

//                        Subject = "Welcome to The School System!";
//                        SendEmail("The School System", u.Email, Content, Subject, CC, CC_Emails);
//                    }
//                }
//                if (n.CreatedBy == "Self")
//                {

//                    Content = System.IO.File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + @"MailTemplates\ForgotPassword.html");
//                    Content = Content.Replace("{Name}", u.Name);
//                    Content = Content.Replace("{Username}", u.Email);

//                    Subject = "Welcome to " + school.SchoolName;

//                    SendEmail(school.SchoolName, u.Email, Content, Subject, CC, CC_Emails);
//                }



//                if (n.CreatedBy == "Admin")
//                {
//                    gm.Status = "Success";
//                    gm.Data = u.Name + " has been added";
//                }

//                if (n.CreatedBy == "Self")
//                {
//                    gm.Status = "Success";
//                    gm.Data = "All done " + n.Name + "! your account has been created, you will be redirected to your portal shortly";
//                }

//            }

//            catch (Exception ex)
//            {
//                gm.Status = "Failed";
//                gm.Data = ex.Message;
//            }
//            return gm;
//        }


//        [HttpPost]
//        [Route("Admin/User/Update")]
//        public object UpdateUser(UpdateUser n)
//        {
//            var gm = new GenericModel();
//            string UserFunction;
//            try
//            {

//                //Validate UserID

//                if (string.IsNullOrEmpty(n.AdminID))
//                {
//                    throw new Exception("Access denied, unauthorized");
//                }

//                if ((UserFunction = VerifyUserFunction(n.AdminID, "UpdateUser")) == "Denied")
//                {
//                    throw new Exception("Access denied, unauthorized");
//                }

//                //validate input

//                if (string.IsNullOrEmpty(n.Name))
//                {
//                    throw new Exception("Name required");
//                }

//                if (string.IsNullOrEmpty(n.Surname))
//                {
//                    throw new Exception("Surname required");
//                }

//                if (string.IsNullOrEmpty(n.Telephone))
//                {
//                    throw new Exception("Telephone required");
//                }

//                if (string.IsNullOrEmpty(n.CountryCode))
//                {
//                    throw new Exception("International dialling code required");
//                }

//                if (string.IsNullOrEmpty(n.Country))
//                {
//                    throw new Exception("Country name required");
//                }


//                //Update User in KazumbaFoundation db
//                var a = (from b in kz.Users where b.UserID == n.UserID & b.Active select b).FirstOrDefault();

//                if (a != null)
//                {

//                    //check Admin to edit user roles and set functions
//                    if ((UserFunction = VerifyUserFunction(n.AdminID, "SetUserFunctions")) == "Allowed")
//                    {

//                        var e = (from f in ss.Users where f.UserID == n.UserID & f.Active == true select f).FirstOrDefault();

//                        if (string.IsNullOrEmpty(n.UserType))
//                        {
//                            throw new Exception("User type required");
//                        }

//                        if (string.IsNullOrEmpty(n.UserRole))
//                        {
//                            throw new Exception("User role required");
//                        }

//                        e.UserType = n.UserType;
//                        e.UserRole = e.UserRole;

//                        e.SetUserFunctions = n.SetUserFunctions;
//                        e.CreateSchool = n.CreateSchool;
//                        e.UpdateSchool = n.UpdateSchool;
//                        e.ActivateSchool = n.ActivateSchool;
//                        e.DeactivateSchool = n.DeactivateSchool;
//                        e.GetSchool = n.GetSchool;
//                        e.GetAllSchools = n.GetAllSchools;
//                        e.CreateUser = n.CreateUser;
//                        e.ActivateUser = n.ActivateUser;
//                        e.DeactivateUser = n.DeactivateUser;
//                        e.LockUser = n.LockUser;
//                        e.UnlockUser = n.UnlockUser;

//                        e.DateModified = DateTime.Now;
//                        ss.SaveChanges();

//                    }

//                    a.Name = n.Name;
//                    a.Surname = n.Surname;
//                    a.Telephone = n.Telephone;
//                    a.Country = n.Country;
//                    a.CountryCode = n.CountryCode;

//                    a.DateModified = DateTime.Now;
//                    kz.SaveChanges();

//                    gm.Status = "Success";
//                    gm.Data = n.Name + " has been updated";
//                }
//                else
//                {
//                    throw new Exception("Invalid user ID");
//                }

//            }

//            catch (Exception ex)
//            {
//                gm.Status = "Failed";
//                gm.Data = ex.Message;
//            }
//            return gm;
//        }

//       

//       
//        #endregion

//        #region Portal
//        [HttpPost]
//        [Route("Portal/User/{UserID}")]
//        public object GetUser(string UserID)
//        {
//            GetUser gu = new GetUser();

//            try
//            {
//                if (string.IsNullOrEmpty(UserID))
//                {
//                    throw new Exception("User ID required");
//                }
//                var UT = (from u in ss.Users where u.UserID == UserID & u.Active == true select u).FirstOrDefault();


//                if (UT != null)
//                {

//                    if (UT.UserType == "SystemAdmin")
//                    {
//                        gu = (GetUser)GetAdmin(UserID);
//                    }

//                    if (UT.UserType == "SchoolManagement")
//                    {

//                    }

//                    if (UT.UserType == "SchoolTeacher")
//                    {
//                        gu = (GetUser)GetTeacher(UserID);

//                    }

//                    if (UT.UserType == "SchoolStudent")
//                    {

//                    }

//                    if (UT.UserType == "SchoolParent")
//                    {

//                    }
//                }
//                else
//                {
//                    throw new Exception("Invalid user ID");
//                }

//            }
//            catch (Exception ex)
//            {
//                gu.Status = "Failed";
//                gu.Data = ex.Message;

//            }

//            return gu;
//        }

//        [HttpPost]
//        [Route("Portal/Dashboard/{UserID}")]
//        public object GetDashboard(string UserID)
//        {
//            GetDashboard gd = new GetDashboard();


//            try
//            {
//                if (string.IsNullOrEmpty(UserID))
//                {
//                    throw new Exception("User ID required");
//                }
//                var UT = (from u in ss.Users where u.UserID == UserID select u).FirstOrDefault();

//                if (UT != null)
//                {

//                    if (UT.UserType == "SystemAdmin")
//                    {
//                        gd.NumberOfSchools = (from s in ss.Schools where s.Active == true select s).Count();
//                        gd.NumberOfUsers = (from u in ss.Users where u.Active == true select u).Count();
//                    }

//                    gd.Status = "Success";
//                }
//                else
//                {
//                    throw new Exception("Invalid User ID");
//                }

//            }
//            catch (Exception ex)
//            {
//                gd.Status = "Failed";
//                gd.Data = ex.Message;

//            }

//            return gd;
//        }

//        [HttpPost]
//        [Route("Portal/Teachers/{School_ID}")]
//        public object GetSchoolTeachers(string School_ID)
//        {
//            //Get teachers for a particular school for the user portal UI

//            GenericModel gm = new GenericModel();
//            try
//            {
//                //var s = (from b in dbP.Teachers
//                //         join c in db.Users on b.UserID equals c.UserID
//                //         join d in db.Schools on b.School_ID equals d.School_ID
//                //         join e in dbP.Departments on b.Department_ID equals e.ID
//                //         where d.School_ID == School_ID
//                //         select new GetUser
//                //         {
//                //             Status = "Success",
//                //             UserID = b.UserID,
//                //             School_ID = b.School_ID,
//                //             SchoolName = d.SchoolName,
//                //             Name = c.Name,
//                //             Surname = c.Surname,
//                //             Email = c.Email,
//                //             UserType = c.UserType,
//                //             Department_ID = e.ID,
//                //             DepartmentName = e.DepartmentName,
//                //             HOD = b.IsHOD
//                //         }).ToList();

//                //if (s != null)
//                //{
//                //    return s;
//                //}


//            }
//            catch (Exception ex)
//            {
//                gm.Status = "Failed";
//                gm.Data = ex.Message;
//            }

//            return gm;
//        }


//        #endregion

//        #region Methods

//        public object GetTeacher(string UserID)
//        {
//            GetUser gu = new GetUser();
//            try
//            {
//                var user = (from a in kz.Users where a.UserID == UserID & a.Active select a).FirstOrDefault();

//                if (user != null)
//                {
//                    var details = (from b in ss.Users
//                                   join c in ss.UsersAndSchools on b.UserID equals c.UserID
//                                   join d in ss.Schools on c.School_ID equals d.School_ID
//                                   where b.UserID == UserID & b.Active
//                                   select new GetUser
//                                   {
//                                       Status = "Success",
//                                       UserID = b.UserID,
//                                       School_ID = d.School_ID,
//                                       SchoolName = d.SchoolName,
//                                       Logo = d.Logo,
//                                       Name = user.Name,
//                                       Surname = user.Surname,
//                                       Email = user.Email,
//                                       Telephone = user.Telephone,
//                                       Country = user.Country,
//                                       CountryCode = user.CountryCode,
//                                       DateCreated = b.DateCreated,
//                                       DateModified = b.DateModified,

//                                       UserType = b.UserType,
//                                       UserRole = b.UserRole,

//                                       SetUserFunctions = b.SetUserFunctions,
//                                       CreateSchool = b.CreateSchool,
//                                       UpdateSchool = b.UpdateSchool,
//                                       ActivateSchool = b.ActivateSchool,
//                                       DeactivateSchool = b.DeactivateSchool,
//                                       GetSchool = b.GetSchool,
//                                       GetAllSchools = b.GetAllSchools,
//                                       CreateUser = b.CreateUser,
//                                       ActivateUser = b.ActivateUser,
//                                       DeactivateUser = b.DeactivateUser,
//                                       UnlockUser = b.UnlockUser,
//                                       LockUser = b.LockUser

//                                   }).FirstOrDefault();

//                    return user;
//                }
//                else
//                {
//                    throw new Exception("Failed to get teacher's information");
//                }

//            }

//            catch (Exception ex)
//            {
//                gu.Status = "Failed";
//                gu.Data = ex.Message;
//            }
//            return gu;

//        }

//        public object GetAdmin(string UserID)
//        {
//            GetUser gu = new GetUser();
//            try
//            {
//                var user = (from b in kz.Users
//                            join c in ss.Users on b.UserID equals c.UserID
//                            where b.UserID == UserID
//                            select new GetUser
//                            {
//                                Status = "Success",
//                                UserID = b.UserID,
//                                SchoolName = "The School System",
//                                Logo = DefaultLogo,
//                                Name = b.Name,
//                                Surname = b.Surname,
//                                Email = b.Email,
//                                UserType = c.UserType,
//                                UserRole = c.UserRole
//                            }).FirstOrDefault();

//                if (user != null)
//                {
//                    return user;
//                }
//                else
//                {
//                    throw new Exception("Failed to get admin's information");
//                }

//            }

//            catch (Exception ex)
//            {
//                gu.Status = "Failed";
//                gu.Data = ex.Message;
//                return gu;
//            }

//        }


// 

//      

//        
//        #endregion
//    }
//}
