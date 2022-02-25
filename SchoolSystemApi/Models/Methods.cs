using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Web;

namespace SchoolSystemApi.Models
{

    public class Methods
    {

        #region Variables

        string SMTP = System.Configuration.ConfigurationManager.AppSettings["SMTP"].ToString();
        string SMTPPort = System.Configuration.ConfigurationManager.AppSettings["SMTPPort"].ToString();
        string SMTPUserName = System.Configuration.ConfigurationManager.AppSettings["SMTPUserName"].ToString();
        string SMTPPassword = System.Configuration.ConfigurationManager.AppSettings["SMTPPassword"].ToString();

        string DefaultLogo = System.Configuration.ConfigurationManager.AppSettings["DefaultLogo"].ToString();

        string ZeroGuid = new Guid().ToString();

        static readonly Account account = new Account(
             "kazumbafoundation",
             "473666744374452",
             "5P1OtG-jl97yFrfzIquZIRWy3ww");

        Cloudinary cloudinary = new Cloudinary(account);

        Data.SchoolSystemEntities db = new Data.SchoolSystemEntities();



        #endregion

        #region Methods

        public int Log(int ID, string ApiKey, string Method, string Activity, string Data, string Error)
        {
            var newlog = new Data.Log();
            int LogID = 0;
            try
            {

                var log = (from a in db.Logs where a.ID == ID select a).FirstOrDefault();
                if (log != null)
                {

                    log.Activity = Activity;
                    log.ErrorData = Error;
                    log.DateModified = DateTime.Now;
                    db.SaveChanges();

                    LogID = log.ID;
                }
                else
                {
                    newlog.ApiKey = ApiKey;
                    newlog.Method = Method;
                    newlog.Activity = Activity;
                    newlog.RequestData = Data;
                    newlog.ErrorData = Error;
                    newlog.DateCreated = DateTime.Now;
                    db.Logs.Add(newlog);
                    db.SaveChanges();

                    LogID = newlog.ID;
                }


            }
            catch (Exception ex)
            {
                LogID = 0;
            }

            return LogID;
        }
        public void SendEmail(string From, string EmailTo, string Content, string Subject)
        {

            try
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

                mail.Subject = Subject;

                mail.To.Add(new MailAddress(EmailTo));
                mail.IsBodyHtml = true;
                mail.BodyEncoding = Encoding.UTF8;

                mail.Body = Content;

                smtp.Send(mail);
            }
            catch (Exception ex)
            {

            }

        }
        public string CheckUserAccess(string UserID, string Function)
        {
            string Status = "";
            try
            {
                var user = (from a in db.Users where a.UserID == UserID & a.Active == true & a.LockedOut == false select a).FirstOrDefault();

                if (user != null)
                {
                    bool action = (bool)user.GetType().GetProperty(Function).GetValue(user, null);

                    if (action)
                    {
                        Status = "YES";
                    }
                    else
                    {
                        Status = "NO";
                    }
                }
                else
                {
                    Status = "NO";
                }

            }
            catch (Exception ex)
            {
                Status = "NO";

            }
            return Status;
        }
        public string UserDuplicate(string Text, string Type)
        {
            string Status = "";
            var duplicate = "";
            try
            {
                if (Type == "Email")
                {
                    duplicate = (from a in db.Users where a.Email == Text select a.Email).FirstOrDefault();
                }

                if (Type == "Telephone")
                {
                    duplicate = (from a in db.Users where a.Telephone == Text select a.Telephone).FirstOrDefault();
                }

                if (duplicate != null)
                {
                    Status = "YES";
                }
                else
                {
                    Status = "NO";
                }

            }
            catch (Exception)
            {
                Status = "YES";
            }
            return Status;
        }
        public string SchoolDuplicate(string Text, string Type)
        {
            string Status = "";
            var duplicate = "";
            try
            {
                if (Type == "Email")
                {
                    duplicate = (from a in db.Schools where a.Email == Text select a.Email).FirstOrDefault();
                }

                if (Type == "Telephone")
                {
                    duplicate = (from a in db.Schools where a.Telephone == Text select a.Telephone).FirstOrDefault();
                }

                if (duplicate != null)
                {
                    Status = "YES";
                }
                else
                {
                    Status = "NO";
                }

            }
            catch (Exception)
            {
                Status = "YES";
            }
            return Status;
        }
        public void NewUsereEmail()
        {
            //Send emails
            //if (n.CreatedBy == "Admin")
            //{
            //    if (n.UserType != "SystemAdmin")
            //    {
            //        //Send user email that their account was created by respective school admin


            //        Content = System.IO.File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + @"MailTemplates\ForgotPassword.html");
            //        Content = Content.Replace("{Name}", u.Name);
            //        Content = Content.Replace("{Username}", u.Email);

            //        Subject = "Welcome to " + school.SchoolName;
            //        SendEmail(school.SchoolName, u.Email, Content, Subject, CC, CC_Emails);
            //    }
            //    else
            //    {
            //        //Send schoolsystem admin user email that his account was created

            //        Content = System.IO.File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + @"MailTemplates\ForgotPassword.html");
            //        Content = Content.Replace("{Name}", u.Name);
            //        Content = Content.Replace("{Username}", u.Email);

            //        Subject = "Welcome to The School System!";
            //        SendEmail("The School System", u.Email, Content, Subject, CC, CC_Emails);
            //    }
            //}
            //if (n.CreatedBy == "Self")
            //{

            //    Content = System.IO.File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + @"MailTemplates\ForgotPassword.html");
            //    Content = Content.Replace("{Name}", u.Name);
            //    Content = Content.Replace("{Username}", u.Email);

            //    Subject = "Welcome to " + school.SchoolName;

            //    SendEmail(school.SchoolName, u.Email, Content, Subject, CC, CC_Emails);
            //}
        }
        public string SetDefaultUserAccess(string ApiKey, string UserID)
        {
            string Status = "";
            string Activity = "You have set default user account functions on " + DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss");
            string ActivityError = "You failed to set default user account functions on " + DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss");
            int LogID = 0;
            string RequestData = "ApiKey: " + ApiKey + " UserID:" + UserID;
            try
            {
                LogID = Log(LogID, ApiKey, "SetDefaultUserAccess", Activity, RequestData, null);

                var n = (from a in db.Users where a.UserID == UserID select a).FirstOrDefault();

                if (n != null)
                {
                    //System User
                    if (n.UserType == 1)
                    {
                        //Manager
                        if (n.UserRole == 1)
                        {
                            UserFunctionAndFeatures(n.UserID, ApiKey, 1, 0);
                            UserFunctionAndFeatures(n.UserID, ApiKey, 2, 0);
                            UserFunctionAndFeatures(n.UserID, ApiKey, 3, 0);
                            UserFunctionAndFeatures(n.UserID, ApiKey, 4, 0);
                            UserFunctionAndFeatures(n.UserID, ApiKey, 5, 0);
                            UserFunctionAndFeatures(n.UserID, ApiKey, 6, 0);
                            UserFunctionAndFeatures(n.UserID, ApiKey, 7, 0);
                            UserFunctionAndFeatures(n.UserID, ApiKey, 8, 0);
                            UserFunctionAndFeatures(n.UserID, ApiKey, 9, 0);
                            UserFunctionAndFeatures(n.UserID, ApiKey, 10, 0);
                            UserFunctionAndFeatures(n.UserID, ApiKey, 11, 0);
                            UserFunctionAndFeatures(n.UserID, ApiKey, 12, 0);
                            UserFunctionAndFeatures(n.UserID, ApiKey, 13, 0);
                        }

                        //Standard
                        if (n.UserRole == 2)
                        {
                            UserFunctionAndFeatures(n.UserID, ApiKey, 3, 0);
                            UserFunctionAndFeatures(n.UserID, ApiKey, 5, 0);
                        }
                    }

                    //School User
                    if (n.UserType == 2)
                    {
                        //Principal
                        if (n.UserRole == 3)
                        {
                            UserFunctionAndFeatures(n.UserID, ApiKey, 1, 0);
                            UserFunctionAndFeatures(n.UserID, ApiKey, 2, 0);
                            UserFunctionAndFeatures(n.UserID, ApiKey, 3, 0);
                            UserFunctionAndFeatures(n.UserID, ApiKey, 4, 0);
                            UserFunctionAndFeatures(n.UserID, ApiKey, 5, 0);
                        }

                        //School Governing Body
                        if (n.UserRole == 4)
                        {
                            UserFunctionAndFeatures(n.UserID, ApiKey, 3, 0);
                            UserFunctionAndFeatures(n.UserID, ApiKey, 5, 0);
                        }

                        //Secretary
                        if (n.UserRole == 5)
                        {
                            UserFunctionAndFeatures(n.UserID, ApiKey, 3, 0);
                            UserFunctionAndFeatures(n.UserID, ApiKey, 5, 0);
                        }

                        //Head of Department
                        if (n.UserRole == 6)
                        {
                            UserFunctionAndFeatures(n.UserID, ApiKey, 3, 0);
                            UserFunctionAndFeatures(n.UserID, ApiKey, 5, 0);
                        }

                        //Teacher
                        if (n.UserRole == 7)
                        {
                            UserFunctionAndFeatures(n.UserID, ApiKey, 3, 0);
                            UserFunctionAndFeatures(n.UserID, ApiKey, 5, 0);
                        }

                        //Parent
                        if (n.UserRole == 8)
                        {
                            UserFunctionAndFeatures(n.UserID, ApiKey, 3, 0);
                            UserFunctionAndFeatures(n.UserID, ApiKey, 5, 0);
                        }

                        //Baby/Toddler/PreSchool
                        if (n.UserRole >= 9 && n.UserRole <= 11)
                        {
                            UserFunctionAndFeatures(n.UserID, ApiKey, 3, 0);
                            UserFunctionAndFeatures(n.UserID, ApiKey, 5, 0);
                        }

                        //High School
                        if (n.UserRole == 13)
                        {
                            UserFunctionAndFeatures(n.UserID, ApiKey, 3, 0);
                            UserFunctionAndFeatures(n.UserID, ApiKey, 5, 0);
                        }

                        //Tertiary
                        if (n.UserRole == 14)
                        {
                            UserFunctionAndFeatures(n.UserID, ApiKey, 3, 0);
                            UserFunctionAndFeatures(n.UserID, ApiKey, 5, 0);
                        }
                    }

                }
                else
                {
                    throw new Exception("User not found");
                }

            }

            catch (Exception ex)
            {
                string ErrorData = ex.Message + "\n" + ex.InnerException + "\n" + ex.StackTrace;
                Log(LogID, "", "", ActivityError, "", ErrorData);

                Status = "Failed";
            }

            return Status;
        }
        public string IsSystemManager(string ApiKey)
        {
            string Status = "";
            try
            {
                var u = (from a in db.Users where a.UserID == ApiKey select a).FirstOrDefault();

                if (u != null)
                {
                    if (u.UserType == 1 && u.UserRole == 1)
                    {
                        Status = "YES";
                    }
                    else
                    {
                        Status = "NO";
                    }
                }
                else
                {
                    Status = "NO";
                }
            }
            catch (Exception)
            {
                Status = "NO";
            }
            return Status;
        }
        public string GetUserEmail(string ApiKey)
        {
            string Email = "";
            try
            {
                var u = (from a in db.Users where a.UserID == ApiKey select a).FirstOrDefault();

                if (u != null)
                {
                    Email = u.Email;
                }
                else
                {
                    Email = ApiKey;
                }
            }
            catch (Exception)
            {
                Email = ApiKey;
            }
            return Email;
        }
        public string GetUserNameAndSurname(string ApiKey)
        {
            string Name = "";
            try
            {
                var u = (from a in db.Users where a.UserID == ApiKey select a).FirstOrDefault();

                if (u != null)
                {
                    Name = u.Name + " " + u.Surname;
                }
                else
                {
                    Name = ApiKey;
                }
            }
            catch (Exception)
            {
                Name = ApiKey;
            }
            return Name;
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
        public void UserFunctionAndFeatures(string UserID, string ApiKey, int Function, int Feature)
        {
            var f = new Data.UserFeaturesAndFunction();
            int LogID = 0;
            string RequestData = "ApiKey: " + ApiKey + " UserID:" + UserID + " Function:" + Function;
            try
            {
                LogID = Log(LogID, ApiKey, "UserFunctionAndFeatures", "", RequestData, null);

                f.UserID = UserID;
                f.FunctionID = Function;
                f.FeatureID = Feature;
                f.CreatedBy = ApiKey;
                f.Active = true;
                f.DateCreated = DateTime.Now;

                db.UserFeaturesAndFunctions.Add(f);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                string ErrorData = ex.Message + "\n" + ex.InnerException + "\n" + ex.StackTrace;
                Log(LogID, "", "", "", "", ErrorData);

            }

        }
        #endregion
    }

}


