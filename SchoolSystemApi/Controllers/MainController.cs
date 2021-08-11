using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Newtonsoft.Json;
using SchoolSystemApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Http;

namespace SchoolSystemApi.Controllers
{
    public class MainController : ApiController
    {
        #region Global
        string SMTP = System.Configuration.ConfigurationManager.AppSettings["SMTP"].ToString();
        string SMTPPort = System.Configuration.ConfigurationManager.AppSettings["SMTPPort"].ToString();
        string SMTPUserName = System.Configuration.ConfigurationManager.AppSettings["SMTPUserName"].ToString();
        string SMTPPassword = System.Configuration.ConfigurationManager.AppSettings["SMTPPassword"].ToString();

        string DefaultLogo = System.Configuration.ConfigurationManager.AppSettings["DefaultLogo"].ToString();

        SchoolSystemEntities db = new SchoolSystemEntities();
        //PreSchoolsEntities dbP = new PreSchoolsEntities();


        static readonly Account account = new Account(
             "kazumbafoundation",
             "473666744374452",
             "5P1OtG-jl97yFrfzIquZIRWy3ww");

        Cloudinary cloudinary = new Cloudinary(account);
        #endregion

        #region Accounts
        [HttpPost]
        [Route("Accounts/Login")]
        public object Login(UserLogin l)
        {
            UserLoginResponse r = new UserLoginResponse();
            GenericModel gm = new GenericModel();
            try
            {
                //Validate input
                if (string.IsNullOrEmpty(l.Email))
                {
                    throw new Exception("Email required");
                }

                if (string.IsNullOrEmpty(l.Password))
                {
                    throw new Exception("Password required");
                }

                var User = (from a in db.Users where a.Email == l.Email & a.Active == true select a).FirstOrDefault();

                string school_ID = "";

                if (User != null)
                {

                    if (User.LockedOut)
                    {
                        throw new Exception("You are locked out of your account, please contact customer support for assistance.");
                    }

                    if (l.Password == User.Password)
                    {

                        r.Status = "Success";
                        r.UserID = User.UserID;
                        r.Email = User.Email;

                        if (User.UserType != "SystemAdmin")
                        {
                            school_ID = (from s in db.UsersAndSchools where User.UserID == s.UserID select s.School_ID).FirstOrDefault();
                        }

                        r.School_ID = school_ID;

                        User.LastLogin = DateTime.Now;
                        User.LoginAttempts = 0;
                        db.SaveChanges();
                    }
                    else
                    {
                        if (User.LoginAttempts > 4)
                        {
                            User.LockedOut = true;
                            db.SaveChanges();
                            throw new Exception("Your account has been locked due to multiple failed login attempts, please contact customer support for assistance.");
                        }
                        else
                        {
                            User.LoginAttempts = User.LoginAttempts + 1;
                            db.SaveChanges();
                            throw new Exception("Incorrect password");
                        }
                    }


                }
                else
                {
                    throw new Exception("Invalid username");
                }

                return r;

            }
            catch (Exception ex)
            {
                gm.Status = "Failed";
                gm.Data = ex.Message;
                return gm;
            }
        }

        [HttpPost]
        [Route("Accounts/Password/Forgot")]
        public object ForgotPassword(ForgotPassword f)
        {
            GenericModel gm = new GenericModel();

            string Subject = "Password Request";
            string Content = "";

            try
            {

                //Validate input
                if (string.IsNullOrEmpty(f.Email))
                {
                    throw new Exception("Email required");
                }

                var User = (from a in db.Users where a.Email == f.Email & a.Active == true select a).FirstOrDefault();

                if (User != null)
                {

                    Content = System.IO.File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + @"MailTemplates\ForgotPassword.html");
                    Content = Content.Replace("{Name}", User.Name);
                    Content = Content.Replace("{Username}", User.Email);
                    Content = Content.Replace("{Password}", User.Password);

                    bool CC = false;
                    List<string> CC_Emails = null;

                    SendEmail("Support", User.Email, Content, Subject, CC, CC_Emails);


                    gm.Status = "Success";
                    gm.Data = "We have sent your account credentials to this email address: " + User.Email;
                }
                else
                {
                    throw new Exception("We do not recognize this email, please enter your account email.");
                }
            }
            catch (Exception ex)
            {
                gm.Status = "Failed";
                gm.Data = ex.Message;
            }

            return gm;
        }

        [HttpPost]
        [Route("Accounts/Password/Update")]
        public object UpdatePassword(UpdatePassword u)
        {
            GenericModel gm = new GenericModel();

            string Subject = "Password Update";
            string Content = "";

            try
            {

                //Validate input
                if (string.IsNullOrEmpty(u.UserID))
                {
                    throw new Exception("User ID required");
                }

                if (string.IsNullOrEmpty(u.CurrentPassword))
                {
                    throw new Exception("Your current password is required");
                }

                if (string.IsNullOrEmpty(u.NewPassword))
                {
                    throw new Exception("Your new password is required");
                }

                var user = (from a in db.Users where a.UserID == a.UserID & a.Active == true select a).FirstOrDefault();

                if (user != null)
                {
                    if (user.Password != u.CurrentPassword)
                    {
                        throw new Exception("The current password entered is incorrect");
                    }

                    else
                    {
                        user.Password = u.NewPassword;
                        user.LastPassword = u.CurrentPassword;
                        db.SaveChanges();

                        gm.Status = "Success";
                        gm.Data = "Your password has been updated";

                        Content = System.IO.File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + @"MailTemplates\PasswordUpdate.html");
                        Content = Content.Replace("{Name}", user.Name);
                        Content = Content.Replace("{Link}", "/RecoverAccount?=" + user.UserID);

                        bool CC = false;
                        List<string> CC_Emails = null;

                        SendEmail("Support", user.Email, Content, Subject, CC, CC_Emails);
                    }
                }
                else
                {
                    throw new Exception("Invalid user ID");
                }
            }
            catch (Exception ex)
            {
                gm.Status = "Failed";
                gm.Data = ex.Message;
            }

            return gm;
        }

        [HttpPost]
        [Route("Accounts/Recover")]
        public object RecoverAccount(UpdatePassword u)
        {
            GenericModel gm = new GenericModel();

            string Subject = "Forgot Password";
            string Content = "";

            try
            {

                //Validate input
                if (string.IsNullOrEmpty(u.UserID))
                {
                    throw new Exception("User ID required");
                }

                if (string.IsNullOrEmpty(u.NewPassword))
                {
                    throw new Exception("New password required");
                }


                var User = (from a in db.Users where a.UserID == u.UserID & a.Active == true select a).FirstOrDefault();

                if (User != null)
                {
                    User.Password = u.NewPassword;
                    User.LastPassword = u.NewPassword;


                    gm.Status = "Success";
                    gm.Data = "Your account password has been updated";
                }
                else
                {
                    throw new Exception("Invalid user ID");
                }
            }
            catch (Exception ex)
            {
                gm.Status = "Failed";
                gm.Data = ex.Message;
            }

            return gm;
        }


        #endregion


        #region Admin

        [HttpPost]
        [Route("Admin/School/Create")]
        public object CreateSchool()
        {
            GenericModel gm = new GenericModel();

            string Subject = "Welcome!";
            string Content = "";
            try
            {


                var httpRequest = HttpContext.Current.Request;

                if (string.IsNullOrEmpty(httpRequest.Params["UserID"]))
                {
                    throw new Exception("Access denied, unauthorized");
                }

                //Validate Admin User
                string UserID = httpRequest.Params["UserID"];

                string Verified = VerifyAdminUser(UserID);

                if (Verified == "Unauthorized")
                {
                    throw new Exception("Access denied, unauthorized");
                }

                //school validation
                if (string.IsNullOrEmpty(httpRequest.Params["SchoolName"]))
                {
                    throw new Exception("School name required");
                }

                string SchoolName = httpRequest.Params["SchoolName"];

                if (string.IsNullOrEmpty(httpRequest.Params["SchoolType"]))
                {
                    throw new Exception("Education level required");
                }

                string SchoolType = httpRequest.Params["SchoolType"];

                if (string.IsNullOrEmpty(httpRequest.Params["SchoolEmail"]))
                {
                    throw new Exception("School email required");
                }

                string SchoolEmail = httpRequest.Params["SchoolEmail"];

                if (string.IsNullOrEmpty(httpRequest.Params["SchoolTelephone"]))
                {
                    throw new Exception("School telephone required");
                }

                string SchoolTelephone = httpRequest.Params["SchoolTelephone"];


                if (string.IsNullOrEmpty(httpRequest.Params["CountryCode"]))
                {
                    throw new Exception("International dialling code required");
                }

                string CountryCode = httpRequest.Params["CountryCode"];

                if (string.IsNullOrEmpty(httpRequest.Params["Country"]))
                {
                    throw new Exception("Country name required");
                }

                string Country = httpRequest.Params["Country"];


                if (string.IsNullOrEmpty(httpRequest.Params["Address"]))
                {
                    throw new Exception("School address required");
                }

                string Address = httpRequest.Params["Address"];

                HttpPostedFile Logo = httpRequest.Files[0];

                if (Logo.ContentLength == 0)
                {
                    throw new Exception("School logo required");
                }

                if (string.IsNullOrEmpty(httpRequest.Params["CompanyRegistrationNumber"]))
                {
                    throw new Exception("Company registration number required");
                }

                string CompanyRegistrationNumber = httpRequest.Params["CompanyRegistrationNumber"];


                if (string.IsNullOrEmpty(httpRequest.Params["UserEmail"]))
                {
                    throw new Exception("User's email required");
                }

                string UserEmail = httpRequest.Params["UserEmail"];

                if (string.IsNullOrEmpty(httpRequest.Params["Name"]))
                {
                    throw new Exception("User's name required");
                }

                string Name = httpRequest.Params["Name"];

                if (string.IsNullOrEmpty(httpRequest.Params["Surname"]))
                {
                    throw new Exception("User's surname required");
                }

                string Surname = httpRequest.Params["Surname"];

                if (string.IsNullOrEmpty(httpRequest.Params["UserTelephone"]))
                {
                    throw new Exception("User's telephone required");
                }

                string UserTelephone = httpRequest.Params["UserTelephone"];

                string check = VerifyDuplicate(SchoolEmail, "Email", "SchoolTable");

                if (check == "Duplicate")
                {
                    throw new Exception("The school email provided is already in use, please try another email.");
                }

                check = VerifyDuplicate(UserEmail, "Email", "UserTable");

                if (check == "Duplicate")
                {
                    throw new Exception("The user's email provided is already in use, please try another email.");
                }

                check = VerifyDuplicate(SchoolName, "Name", "SchoolTable");

                if (check == "Duplicate")
                {
                    throw new Exception("The school name provided is already in use, please try another name.");
                }


                //create school
                School sc = new School();

                sc.School_ID = Guid.NewGuid().ToString();
                sc.SchoolName = SchoolName;
                sc.SchoolType = SchoolType;
                sc.Email = SchoolEmail;
                sc.Telephone = SchoolTelephone;
                sc.Country = Country;
                sc.CountryCode = CountryCode;
                sc.Address = Address;

                string path = UploadFile(Logo);

                sc.Logo = path;
                sc.Active = true;
                sc.CompanyRegistrationNumber = CompanyRegistrationNumber;
                sc.DateCreated = DateTime.Now;
                sc.DateModified = DateTime.Now;

                db.Schools.Add(sc);
                db.SaveChanges();

                //Add User to school

                User u = new User();

                u.UserID = Guid.NewGuid().ToString();
                u.Password = Guid.NewGuid().ToString().Substring(1, 6);
                u.LastPassword = u.Password;
                u.Email = UserEmail;
                u.Name = Name;
                u.Surname = Surname;
                u.Telephone = UserTelephone;
                u.UserType = "SchoolManagement";
                u.LastLogin = DateTime.Now;
                u.LoginAttempts = 0;
                u.LockedOut = false;
                u.Active = true;
                u.DateCreated = DateTime.Now;
                u.DateModified = DateTime.Now;

                db.Users.Add(u);
                db.SaveChanges();


                //Add User to School relationship

                UsersAndSchool m = new UsersAndSchool();

                m.School_ID = sc.School_ID;
                m.UserID = u.UserID;

                db.UsersAndSchools.Add(m);
                db.SaveChanges();


                //User to functions

                UserFunction uf = new UserFunction();

                uf.UserID = u.UserID;
                uf.CreateSchool = false;

                db.UserFunctions.Add(uf);
                db.SaveChanges();



                //Send welcome email


                //Content = System.IO.File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + @"MailTemplates\ForgotPassword.html");
                //    Content = Content.Replace("{Name}", User.Name);
                //    Content = Content.Replace("{Username}", User.Username);
                //    Content = Content.Replace("{Password}", User.Password);

                //    bool CC = false;

                //    SendEmail(User.Email, Content, Subject, CC, "Support");


                gm.Status = "Success";
                gm.Data = SchoolName + " has been added.";

            }
            catch (Exception ex)
            {
                gm.Status = "Failed";
                gm.Data = ex.Message;
            }

            return gm;
        }

        [HttpPost]
        [Route("Admin/School/All/{UserID}")]
        public object GetAllSchools(string UserID)
        {
            GenericModel gm = new GenericModel();


            try
            {
                //Validate Admin User

                if (string.IsNullOrEmpty(UserID))
                {
                    throw new Exception("Access denied, unauthorized");
                }

                string Verified = VerifyAdminUser(UserID);

                if (Verified == "Unauthorized")
                {
                    throw new Exception("Access denied, unauthorized");
                }

                var s = (from a in db.Schools
                         select new GetSchool
                         {
                             School_ID = a.School_ID,
                             SchoolName = a.SchoolName,
                             SchoolType = a.SchoolType,
                             Email = a.Email,
                             Telephone = a.Telephone,
                             Country = a.Country,
                             CountryCode = a.CountryCode,
                             Address = a.Address,
                             Active = a.Active,
                             CompanyRegistrationNumber = a.CompanyRegistrationNumber,
                             DateCreated = (DateTime)a.DateCreated,
                             DateModified = (DateTime)a.DateModified

                         }).ToList();

                if (s != null)
                {
                    gm.Status = "Success";
                    gm.Data = JsonConvert.SerializeObject(s);
                }

            }
            catch (Exception ex)
            {
                gm.Status = "Failed";
                gm.Data = ex.Message;
            }

            return gm;
        }

        [HttpPost]
        [Route("Admin/School/{UserID}/{School_ID}")]
        public object GetSchool(string UserID, string School_ID)
        {
            GetSchool gs = new GetSchool();

            try
            {
                //Validate Admin User
                if (string.IsNullOrEmpty(UserID))
                {
                    throw new Exception("Access denied, unauthorized");
                }

                string Verified = VerifyAdminUser(UserID);

                if (Verified == "Unauthorized")
                {
                    throw new Exception("Access denied, unauthorized");
                }

                if (string.IsNullOrEmpty(School_ID))
                {
                    throw new Exception("School ID required");
                }

                var s = (from a in db.Schools where a.School_ID == School_ID select a).FirstOrDefault();

                if (s != null)
                {
                    gs.Status = "Success";

                    gs.School_ID = s.School_ID;
                    gs.SchoolName = s.SchoolName;
                    gs.SchoolType = s.SchoolType;
                    gs.Email = s.Email;
                    gs.Telephone = s.Telephone;
                    gs.Country = s.Country;
                    gs.CountryCode = s.CountryCode;
                    gs.Address = s.Address;
                    gs.Logo = s.Logo;
                    gs.Active = s.Active;
                    gs.CompanyRegistrationNumber = s.CompanyRegistrationNumber;

                }
                else
                {
                    throw new Exception("Invalid school ID");
                }
            }

            catch (Exception ex)
            {
                gs.Status = "Failed";
                gs.Data = ex.Message;
            }
            return gs;
        }

        [HttpPost]
        [Route("Admin/School/Update")]
        public object UpdateSchool()
        {
            GenericModel gm = new GenericModel();

            string Subject = "Welcome!";
            string Content = "";
            try
            {


                var httpRequest = HttpContext.Current.Request;

                if (string.IsNullOrEmpty(httpRequest.Params["UserID"]))
                {
                    throw new Exception("Access denied, unauthorized");
                }

                //Validate Admin User
                string UserID = httpRequest.Params["UserID"];

                string Verified = VerifyAdminUser(UserID);

                if (Verified == "Unauthorized")
                {
                    throw new Exception("Access denied, unauthorized");
                }

                //school validation
                if (string.IsNullOrEmpty(httpRequest.Params["School_ID"]))
                {
                    throw new Exception("School ID required");
                }

                string School_ID = httpRequest.Params["School_ID"];

                if (string.IsNullOrEmpty(httpRequest.Params["SchoolName"]))
                {
                    throw new Exception("School name required");
                }

                string SchoolName = httpRequest.Params["SchoolName"];

                if (string.IsNullOrEmpty(httpRequest.Params["SchoolType"]))
                {
                    throw new Exception("Education level required");
                }

                string SchoolType = httpRequest.Params["SchoolType"];

                if (string.IsNullOrEmpty(httpRequest.Params["SchoolEmail"]))
                {
                    throw new Exception("School email required");
                }

                string SchoolEmail = httpRequest.Params["SchoolEmail"];

                if (string.IsNullOrEmpty(httpRequest.Params["SchoolTelephone"]))
                {
                    throw new Exception("School telephone required");
                }

                string SchoolTelephone = httpRequest.Params["SchoolTelephone"];


                if (string.IsNullOrEmpty(httpRequest.Params["CountryCode"]))
                {
                    throw new Exception("International dialling code required");
                }

                string CountryCode = httpRequest.Params["CountryCode"];

                if (string.IsNullOrEmpty(httpRequest.Params["Country"]))
                {
                    throw new Exception("Country name required");
                }

                string Country = httpRequest.Params["Country"];

                if (string.IsNullOrEmpty(httpRequest.Params["Address"]))
                {
                    throw new Exception("School address required");
                }

                string Address = httpRequest.Params["Address"];



                if (string.IsNullOrEmpty(httpRequest.Params["CompanyRegistrationNumber"]))
                {
                    throw new Exception("Company registration number required");
                }

                string CompanyRegistrationNumber = httpRequest.Params["CompanyRegistrationNumber"];


                //create school
                var sc = (from s in db.Schools where School_ID == s.School_ID select s).FirstOrDefault();

                if (sc != null)
                {
                    sc.SchoolName = SchoolName;
                    sc.SchoolType = SchoolType;
                    sc.Email = SchoolEmail;
                    sc.Telephone = SchoolTelephone;
                    sc.Country = Country;
                    sc.CountryCode = CountryCode;
                    sc.Address = Address;

                    if (httpRequest.Params["Logo"] != "false")
                    {
                        HttpPostedFile Logo = httpRequest.Files[0];

                        string path = UploadFile(Logo);
                        sc.Logo = path;
                    }


                    sc.CompanyRegistrationNumber = CompanyRegistrationNumber;
                    sc.DateModified = DateTime.Now;

                    db.SaveChanges();

                    gm.Status = "Success";
                    gm.Data = SchoolName + " has been updated.";
                }
                else
                {
                    gm.Status = "Failed";
                    gm.Data = "Update failed! could not find school from provided school ID.";
                }


            }
            catch (Exception ex)
            {
                gm.Status = "Failed";
                gm.Data = ex.Message;
            }

            return gm;
        }

        [HttpPost]
        [Route("Admin/School/Deactivate/{UserID}/{School_ID}")]
        public object DeactivateSchool(string UserID, string School_ID)
        {
            GenericModel gm = new GenericModel();

            try
            {
                //Validate Admin User
                if (string.IsNullOrEmpty(UserID))
                {
                    throw new Exception("Access denied, unauthorized");
                }

                string Verified = VerifyAdminUser(UserID);

                if (Verified == "Unauthorized")
                {
                    throw new Exception("Access denied, unauthorized");
                }

                if (string.IsNullOrEmpty(School_ID))
                {
                    throw new Exception("School ID required");
                }

                var s = (from a in db.Schools where a.School_ID == School_ID select a).FirstOrDefault();

                if (s != null)
                {
                    s.Email = "Deactivated-" + s.Email;
                    s.Active = false;
                    s.DateCreated = DateTime.Now;
                    db.SaveChanges();

                    gm.Status = "Success";
                    gm.Data = s.SchoolName + " has been deactivated";
                }
                else
                {
                    throw new Exception("Invalid school ID");
                }
            }

            catch (Exception ex)
            {
                gm.Status = "Failed";
                gm.Data = ex.Message;
            }
            return gm;
        }


        [HttpPost]
        [Route("Admin/School/Activate/{UserID}/{School_ID}")]
        public object ActivateSchool(string UserID, string School_ID)
        {
            GenericModel gm = new GenericModel();

            try
            {

                //Validate Admin User
                if (string.IsNullOrEmpty(UserID))
                {
                    throw new Exception("Access denied, unauthorized");
                }

                string Verified = VerifyAdminUser(UserID);

                if (Verified == "Unauthorized")
                {
                    throw new Exception("Access denied, unauthorized");
                }

                if (string.IsNullOrEmpty(School_ID))
                {
                    throw new Exception("School ID required");
                }

                var s = (from a in db.Schools where a.School_ID == School_ID select a).FirstOrDefault();

                if (s != null)
                {
                    s.Email = s.Email.Replace("Deactivated-", "");
                    s.Active = true;
                    s.DateModified = DateTime.Now;
                    db.SaveChanges();

                    gm.Status = "Success";
                    gm.Data = s.SchoolName + " is now active";
                }
                else
                {
                    throw new Exception("Invalid school ID");
                }
            }

            catch (Exception ex)
            {
                gm.Status = "Failed";
                gm.Data = ex.Message;
            }
            return gm;
        }

        [HttpPost]
        [Route("Admin/User/Create")]
        public object CreateUser(NewUser n)
        {

            string Subject = "Welcome!";
            string Content = "";
            var school = new School();
            try
            {

                //Validate Admin User
                if (n.CreatedBy == "Admin")
                {
                    if (string.IsNullOrEmpty(n.UserID))
                    {
                        throw new Exception("Access denied, unauthorized");
                    }

                }

                //validate input

                if (string.IsNullOrEmpty(n.Name))
                {
                    throw new Exception("Name required");
                }

                if (string.IsNullOrEmpty(n.Surname))
                {
                    throw new Exception("Surname required");
                }

                if (string.IsNullOrEmpty(n.Email))
                {
                    throw new Exception("Email required");
                }

                if (string.IsNullOrEmpty(n.Telephone))
                {
                    throw new Exception("Telephone required");
                }

                if (string.IsNullOrEmpty(n.UserType))
                {
                    throw new Exception("User type required");
                }

                if (string.IsNullOrEmpty(n.UserRole))
                {
                    throw new Exception("User role required");
                }

                string check = VerifyDuplicate(n.Email, "Email", "UserTable");

                if (check == "Duplicate")
                {
                    throw new Exception("The email provided is already in use, please try another email.");
                }

                //Create User in users table
                User u = new User();

                u.UserID = Guid.NewGuid().ToString();

                if (n.CreatedBy == "Admin")
                {
                    //Generate random password

                    u.Password = Guid.NewGuid().ToString().Substring(1, 6);
                    u.LastPassword = u.Password;
                }
                else
                {
                    if (string.IsNullOrEmpty(n.Password))
                    {
                        throw new Exception("Password required");
                    }

                    u.Password = n.Password;
                    u.LastPassword = u.Password;
                }


                u.Name = n.Name;
                u.Surname = n.Surname;
                u.Email = n.Email;
                u.Telephone = n.Telephone;
                u.UserType = n.UserType;
                u.Country = n.Country;
                u.CountryCode = n.CountryCode;
                u.LastLogin = DateTime.Now;
                u.LoginAttempts = 0;
                u.LockedOut = false;
                u.Active = true;
                u.DateCreated = DateTime.Now;
                u.DateModified = DateTime.Now;

                db.Users.Add(u);
                db.SaveChanges();


                //Add User to school if required

                if ((n.UserType == "SchoolManagement") || (n.UserType == "SchoolTeacher") || (n.UserType == "SchoolParent") || (n.UserType == "SchoolStudent"))
                {
                    UsersAndSchool m = new UsersAndSchool();

                    m.School_ID = n.School_ID;
                    m.UserID = u.UserID;

                    db.UsersAndSchools.Add(m);
                    db.SaveChanges();

                    school = (from a in db.Schools where a.School_ID == n.School_ID select a).FirstOrDefault();
                }

                //Add User to functions table

                UserFunction uf = new UserFunction();

                uf.UserID = u.UserID;

                if (n.UserType == "SystemAdmin")
                {
                    uf.CreateSchool = true;
                    uf.UserRole = n.UserRole;
                }

                if (n.UserType == "SchoolManagement")
                {
                    uf.CreateSchool = false;
                    uf.UserRole = n.UserRole;
                }

                if (n.UserType == "SchoolTeacher")
                {
                    uf.CreateSchool = false;
                    uf.UserRole = n.UserRole;
                }

                if (n.UserType == "SchoolParent")
                {
                    uf.CreateSchool = false;
                    uf.UserRole = n.UserRole;
                }

                if (n.UserType == "SchoolStudent")
                {
                    uf.CreateSchool = false;
                    uf.UserRole = n.UserRole;
                }

                if (n.UserType == "HealthCare")
                {
                    uf.CreateSchool = false;
                    uf.UserRole = n.UserRole;
                }

                db.UserFunctions.Add(uf);
                db.SaveChanges();


                //Send emails

                if (n.CreatedBy == "Admin")
                {
                    if (n.UserType != "SystemAdmin")
                    {
                        //Send user email that their account was created by organization admin

                        //school = school.SchoolName;
                    }
                    else
                    {
                        //Send admin user email that his account was created by admin
                    }
                }

                if (n.CreatedBy == "Self")
                {
                    if (n.UserType != "SystemAdmin")
                    {
                        //send user welcome email with details about system and school they belong to

                        //school = school.SchoolName;
                    }
                    else
                    {
                        //send email to admin user with their credentials
                    }
                }

                bool CC = false;
                List<string> CC_Emails = null;

                //SendEmail("Support", u.Email, Content, Subject, CC, CC_Emails);

                if (n.CreatedBy == "Admin")
                {
                    n.Status = "Success";
                    n.Data = "User has been added";
                }

                if (n.CreatedBy == "Self")
                {
                    n.Status = "Success";
                    n.Data = "All done " + n.Name + "! your account has been created, you will be redirected to your portal shortly.";
                }

            }

            catch (Exception ex)
            {
                n.Status = "Failed";
                n.Data = ex.Message;
            }
            return n;
        }


        [HttpPost]
        [Route("Admin/User/Deactivate/{UserID}")]
        public object DeactivateUser(string UserID)
        {
            GenericModel gm = new GenericModel();

            try
            {

                if (string.IsNullOrEmpty(UserID))
                {
                    throw new Exception("User ID required");
                }

                var s = (from a in db.Users where a.UserID == UserID select a).FirstOrDefault();

                if (s != null)
                {
                    s.Active = false;
                    db.SaveChanges();

                    gm.Status = "Success";
                    gm.Data = "User " + s.Email + " has been deactivated";
                }
                else
                {
                    throw new Exception("Invalid User ID");
                }
            }

            catch (Exception ex)
            {
                gm.Status = "Failed";
                gm.Data = ex.Message;
            }
            return gm;
        }

        [HttpPost]
        [Route("Admin/User/Activate/{UserID}")]
        public object ActivateUser(string UserID)
        {
            GenericModel gm = new GenericModel();

            try
            {

                if (string.IsNullOrEmpty(UserID))
                {
                    throw new Exception("User ID required");
                }

                var s = (from a in db.Users where a.UserID == UserID select a).FirstOrDefault();

                if (s != null)
                {
                    s.Active = true;
                    db.SaveChanges();

                    gm.Status = "Success";
                    gm.Data = "User " + s.Email + " is now active";
                }
                else
                {
                    throw new Exception("Invalid User ID");
                }
            }

            catch (Exception ex)
            {
                gm.Status = "Failed";
                gm.Data = ex.Message;
            }
            return gm;
        }

        [HttpPost]
        [Route("Admin/User/Unlock/{UserID}")]
        public object UnlockUser(string UserID)
        {
            GenericModel gm = new GenericModel();
            try
            {


                if (string.IsNullOrEmpty(UserID))
                {
                    throw new Exception("User ID required");
                }
                var user = (from u in db.Users where u.UserID == UserID select u).FirstOrDefault();

                if (user != null)
                {
                    user.LockedOut = false;
                    db.SaveChanges();

                    gm.Status = "Success";
                    gm.Data = "User " + user.Email + " is now unlocked";

                }
                else
                {
                    throw new Exception("Invalid user ID");
                }

            }
            catch (Exception ex)
            {
                gm.Status = "Failed";
                gm.Data = ex.Message;
            }

            return gm;
        }

        [HttpPost]
        [Route("Admin/User/Lock/{UserID}")]
        public object LockUser(string UserID)
        {
            GenericModel gm = new GenericModel();
            try
            {


                if (string.IsNullOrEmpty(UserID))
                {
                    throw new Exception("User ID required");
                }
                var user = (from u in db.Users where u.UserID == UserID select u).FirstOrDefault();

                if (user != null)
                {
                    user.LockedOut = true;
                    db.SaveChanges();

                    gm.Status = "Success";
                    gm.Data = "User  " + user.Email + " is now locked";
                }
                else
                {
                    throw new Exception("Invalid user ID");
                }

            }
            catch (Exception ex)
            {
                gm.Status = "Failed";
                gm.Data = ex.Message;
            }

            return gm;
        }

        #endregion

        #region Portal
        [HttpPost]
        [Route("Portal/User/{UserID}")]
        public object GetUser(string UserID)
        {
            GetUser gu = new GetUser();

            try
            {
                if (string.IsNullOrEmpty(UserID))
                {
                    throw new Exception("User ID required");
                }
                var UT = (from u in db.Users where u.UserID == UserID & u.Active == true select u).FirstOrDefault();


                if (UT != null)
                {

                    if (UT.UserType == "SystemAdmin")
                    {
                        gu = (GetUser)GetAdmin(UserID);
                    }

                    if (UT.UserType == "Management")
                    {

                    }

                    if (UT.UserType == "Teacher")
                    {
                        gu = (GetUser)GetTeacher(UserID);

                    }

                    if (UT.UserType == "Student")
                    {

                    }

                    if (UT.UserType == "Parent")
                    {

                    }
                }
                else
                {
                    throw new Exception("Invalid User ID");
                }

            }
            catch (Exception ex)
            {
                gu.Status = "Failed";
                gu.Data = ex.Message;

            }

            return gu;
        }

        [HttpPost]
        [Route("Portal/Dashboard/{UserID}")]
        public object GetDashboard(string UserID)
        {
            GetDashboard gd = new GetDashboard();


            try
            {
                if (string.IsNullOrEmpty(UserID))
                {
                    throw new Exception("User ID required");
                }
                var UT = (from u in db.Users where u.UserID == UserID select u).FirstOrDefault();

                if (UT != null)
                {

                    if (UT.UserType == "SystemAdmin")
                    {
                        gd.NumberOfSchools = (from s in db.Schools where s.Active == true select s).Count();
                        gd.NumberOfUsers = (from u in db.Users where u.Active == true select u).Count();
                    }

                    gd.Status = "Success";
                }
                else
                {
                    throw new Exception("Invalid User ID");
                }

            }
            catch (Exception ex)
            {
                gd.Status = "Failed";
                gd.Data = ex.Message;

            }

            return gd;
        }

        [HttpPost]
        [Route("Portal/Teachers/{School_ID}")]
        public object GetSchoolTeachers(string School_ID)
        {
            //Get teachers for a particular school for the user portal UI

            GenericModel gm = new GenericModel();
            try
            {
                //var s = (from b in dbP.Teachers
                //         join c in db.Users on b.UserID equals c.UserID
                //         join d in db.Schools on b.School_ID equals d.School_ID
                //         join e in dbP.Departments on b.Department_ID equals e.ID
                //         where d.School_ID == School_ID
                //         select new GetUser
                //         {
                //             Status = "Success",
                //             UserID = b.UserID,
                //             School_ID = b.School_ID,
                //             SchoolName = d.SchoolName,
                //             Name = c.Name,
                //             Surname = c.Surname,
                //             Email = c.Email,
                //             UserType = c.UserType,
                //             Department_ID = e.ID,
                //             DepartmentName = e.DepartmentName,
                //             HOD = b.IsHOD
                //         }).ToList();

                //if (s != null)
                //{
                //    return s;
                //}


            }
            catch (Exception ex)
            {
                gm.Status = "Failed";
                gm.Data = ex.Message;
            }

            return gm;
        }


        #endregion

        #region Methods

        public object GetTeacher(string UserID)
        {
            GetUser gu = new GetUser();
            try
            {
                //var user = (from b in dbP.Teachers
                //            join c in db.Users on b.UserID equals c.UserID
                //            join d in db.Schools on b.School_ID equals d.School_ID
                //            where b.UserID == UserID
                //            select new GetUser
                //            {
                //                Status = "Success",
                //                UserID = b.UserID,
                //                School_ID = b.School_ID,
                //                SchoolName = d.SchoolName,
                //                Logo = d.Logo,
                //                Name = c.Name,
                //                Surname = c.Surname,
                //                Email = c.Email,
                //                UserType = c.UserType,
                //                HOD = b.IsHOD
                //            }).FirstOrDefault();

                //if (user != null)
                //{
                //    return user;
                //}
                //else
                //{
                //    throw new Exception("Failed to get teacher's information.");
                //}

            }

            catch (Exception ex)
            {
                gu.Status = "Failed";
                gu.Data = ex.Message;
            }
            return gu;

        }

        public object GetAdmin(string UserID)
        {
            GetUser gu = new GetUser();
            try
            {
                var user = (from b in db.Users 
                            join c in db.UserFunctions on b.UserID equals c.UserID
                            where b.UserID == UserID
                            select new GetUser
                            {
                                Status = "Success",
                                UserID = b.UserID,
                                SchoolName = "The School System",
                                Logo = DefaultLogo,
                                Name = b.Name,
                                Surname = b.Surname,
                                Email = b.Email,
                                UserType = b.UserType,
                                UserRole = c.UserRole
                            }).FirstOrDefault();

                if (user != null)
                {
                    return user;
                }
                else
                {
                    throw new Exception("Failed to get admin's information.");
                }

            }

            catch (Exception ex)
            {
                gu.Status = "Failed";
                gu.Data = ex.Message;
                return gu;
            }

        }

        public void SendEmail(string From, string EmailTo, string Content, string Subject, bool CC, List<string> CC_Email)
        {

            //send email

            SmtpClient smtp = new SmtpClient(SMTP)
            {
                Host = SMTP,
                Port = int.Parse(SMTPPort),
                Credentials = new System.Net.NetworkCredential(SMTPUserName, SMTPPassword),

            };

            MailMessage mail = new MailMessage
            {
                From = new MailAddress(SMTPUserName, From),
            };

            if (CC)
            {
                //add cc email list
            }

            mail.Subject = Subject;

            mail.To.Add(new MailAddress(EmailTo, Subject));
            mail.IsBodyHtml = true;
            mail.BodyEncoding = Encoding.UTF8;

            mail.Body = Content;

            smtp.Send(mail);


        }

        //[HttpPost]
        //[Route("UploadFile")]
        public string UploadFile(HttpPostedFile Logo)
        {
            string Path = "";
            try
            {
                byte[] Byte = new byte[Logo.ContentLength];
                using (BinaryReader theReader = new BinaryReader(Logo.InputStream))
                {
                    Byte = theReader.ReadBytes(Logo.ContentLength);
                }

                Stream Stream = new MemoryStream(Byte);
                Stream.Position = 0;

                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(Logo.FileName, Stream),
                    PublicId = Logo.FileName,
                };

                var Result = cloudinary.Upload(uploadParams);

                Path = Result.SecureUrl.AbsoluteUri;

            }
            catch (Exception ex)
            {
                Path = "Failed";
            }
            return Path;
        }

        public string VerifyDuplicate(string Text, string Type, string Where)
        {
            string Status = "";
            var duplicate = "";
            try
            {
                if (Type == "Email")
                {
                    if (Where == "UserTable")
                    {
                        duplicate = (from a in db.Users where a.Email == Text select a.Email).FirstOrDefault();
                    }

                    if (Where == "SchoolTable")
                    {
                        duplicate = (from a in db.Schools where a.Email == Text select a.Email).FirstOrDefault();
                    }

                }

                if (Type == "Name")
                {

                    if (Where == "SchoolTable")
                    {
                        duplicate = (from a in db.Schools where a.SchoolName == Text select a.SchoolName).FirstOrDefault();
                    }

                }

                if (duplicate != null)
                {
                    Status = "Duplicate";
                }
                else
                {
                    Status = "Available";
                }

            }
            catch (Exception)
            {
                Status = "Failed";
            }
            return Status;
        }

        public string VerifyAdminUser(string UserID)
        {
            string Status = "";
            try
            {
                var user = (from u in db.Users where UserID == u.UserID & u.Active == true select u).FirstOrDefault();

                if (user.UserType == "SystemAdmin")
                {
                    Status = "Authorized";
                }
                else
                {
                    Status = "Unauthorized";
                }
            }
            catch (Exception)
            {
                Status = "Failed";
            }
            return Status;
        }


        #endregion
    }
}
