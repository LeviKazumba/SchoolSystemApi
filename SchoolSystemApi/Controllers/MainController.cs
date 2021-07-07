﻿using CloudinaryDotNet;
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
        PreSchoolsEntities dbP = new PreSchoolsEntities();


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

                var User = (from a in db.Users where a.Email == l.Email select a).FirstOrDefault();

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

                        if ((User.UserType == "SchoolManagement") || (User.UserType == "SchoolTeachers") || (User.UserType == "SchoolStudents"))
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

                var User = (from a in db.Users where a.Email == f.Email select a).FirstOrDefault();

                if (User != null)
                {

                    Content = System.IO.File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + @"MailTemplates\ForgotPassword.html");
                    Content = Content.Replace("{Name}", User.Name);
                    Content = Content.Replace("{Username}", User.Email);
                    Content = Content.Replace("{Password}", User.Password);

                    bool CC = false;

                    SendEmail(User.Email, Content, Subject, CC);


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

                var user = (from a in db.Users where a.UserID == a.UserID select a).FirstOrDefault();

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


                        SendEmail(user.Email, Content, Subject, CC);
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


                var User = (from a in db.Users where a.UserID == u.UserID select a).FirstOrDefault();

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
                    throw new Exception("Access denied, authorized");
                }

                string UserID = httpRequest.Params["UserID"];

                //Validate Admin User
                string Verified = VerifyAdminUser(UserID);

                if (Verified == "Unauthorized")
                {
                    throw new Exception("Access denied, authorized");
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

                //create school
                School sc = new School();

                sc.School_ID = Guid.NewGuid().ToString();
                sc.SchoolName = SchoolName;
                sc.SchoolType = SchoolType;
                sc.Email = SchoolEmail;
                sc.Telephone = SchoolTelephone;
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
        [Route("Admin/School/Deactivate/{UserID}/{School_ID}")]
        public object DeactivateSchool(string UserID, string School_ID)
        {
            GenericModel gm = new GenericModel();

            try
            {
                //Validate Admin User
                if (string.IsNullOrEmpty(UserID))
                {
                    throw new Exception("Access denied, authorized");
                }

                string Verified = VerifyAdminUser(UserID);

                if (Verified == "Unauthorized")
                {
                    throw new Exception("Access denied, authorized");
                }

                if (string.IsNullOrEmpty(School_ID))
                {
                    throw new Exception("School ID required");
                }

                var s = (from a in db.Schools where a.School_ID == School_ID select a).FirstOrDefault();

                if (s != null)
                {
                    s.Email = "D-" + s.Email;
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
                    throw new Exception("Access denied, authorized");
                }

                string Verified = VerifyAdminUser(UserID);

                if (Verified == "Unauthorized")
                {
                    throw new Exception("Access denied, authorized");
                }

                if (string.IsNullOrEmpty(School_ID))
                {
                    throw new Exception("School ID required");
                }


                if (string.IsNullOrEmpty(School_ID))
                {
                    throw new Exception("School ID required");
                }

                var s = (from a in db.Schools where a.School_ID == School_ID select a).FirstOrDefault();

                if (s != null)
                {
                    s.Email = s.Email.Replace("D-", "");
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


        [HttpPost]
        [Route("Admin/Schools/{UserID}")]
        public object GetAllSchools(string UserID)
        {
            GenericModel gm = new GenericModel();


            try
            {
                //Validate Admin User
                string Verified = VerifyAdminUser(UserID);

                if (Verified == "Unauthorized")
                {
                    throw new Exception("Access denied, authorized");
                }

                var s = (from a in db.Schools
                         select new GetSchool
                         {
                             School_ID = a.School_ID,
                             SchoolName = a.SchoolName,
                             SchoolType = a.SchoolType,
                             Email = a.Email,
                             Telephone = a.Telephone,
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
                var UT = (from u in db.Users where u.UserID == UserID select u).FirstOrDefault();


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
                var s = (from b in dbP.Teachers
                         join c in db.Users on b.UserID equals c.UserID
                         join d in db.Schools on b.School_ID equals d.School_ID
                         join e in dbP.Departments on b.Department_ID equals e.ID
                         where d.School_ID == School_ID
                         select new GetUser
                         {
                             Status = "Success",
                             UserID = b.UserID,
                             School_ID = b.School_ID,
                             SchoolName = d.SchoolName,
                             Name = c.Name,
                             Surname = c.Surname,
                             Email = c.Email,
                             UserType = c.UserType,
                             Department_ID = e.ID,
                             DepartmentName = e.DepartmentName,
                             HOD = b.IsHOD
                         }).ToList();

                if (s != null)
                {
                    return s;
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

        #region Methods

        public object GetTeacher(string UserID)
        {
            GetUser gu = new GetUser();
            try
            {
                var user = (from b in dbP.Teachers
                            join c in db.Users on b.UserID equals c.UserID
                            join d in db.Schools on b.School_ID equals d.School_ID
                            where b.UserID == UserID
                            select new GetUser
                            {
                                Status = "Success",
                                UserID = b.UserID,
                                School_ID = b.School_ID,
                                SchoolName = d.SchoolName,
                                Logo = d.Logo,
                                Name = c.Name,
                                Surname = c.Surname,
                                Email = c.Email,
                                UserType = c.UserType,
                                HOD = b.IsHOD
                            }).FirstOrDefault();

                if (user != null)
                {
                    return user;
                }
                else
                {
                    throw new Exception("Failed to get teacher's information.");
                }

            }

            catch (Exception ex)
            {
                gu.Status = "Failed";
                gu.Data = ex.Message;
                return gu;
            }

        }

        public object GetAdmin(string UserID)
        {
            GetUser gu = new GetUser();
            try
            {
                var user = (from b in db.Users
                            where b.UserID == UserID
                            select new GetUser
                            {
                                Status = "Success",
                                UserID = b.UserID,
                                SchoolName = "School System",
                                Logo = DefaultLogo,
                                Name = b.Name,
                                Surname = b.Surname,
                                Email = b.Email,
                                UserType = b.UserType,
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

        public void SendEmail(string EmailTo, string Content, string Subject, bool CC)
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
                From = new MailAddress(SMTPUserName, "School System"),
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
                    File = new FileDescription("",Stream),
                };

                var Result = cloudinary.Upload(uploadParams);

                Path = Result.SecureUrl.AbsoluteUri;

            }
            catch (Exception)
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
                var user = (from u in db.Users where UserID == u.UserID select u).FirstOrDefault();

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
