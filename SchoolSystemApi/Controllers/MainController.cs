﻿using SchoolSystemApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
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


        SchoolSystemEntities db = new SchoolSystemEntities();
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
                if (string.IsNullOrEmpty(l.Username))
                {
                    throw new Exception("Username required");
                }

                if (string.IsNullOrEmpty(l.Password))
                {
                    throw new Exception("Password required");
                }

                var User = (from a in db.Users where a.Username == l.Username select a).FirstOrDefault();

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
                        r.Username = User.Username;

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

            string Subject = "Forgot Password";
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
                    Content = Content.Replace("{Username}", User.Username);
                    Content = Content.Replace("{Password}", User.Password);

                    bool CC = false;

                    SendEmail(User.Email, Content, Subject, CC, "no-reply");


                    gm.Status = "Success";
                    gm.Data = "We have sent your login credentials to this email address: " + User.Email;
                }
                else
                {
                    throw new Exception("We do not recognize this email, please enter a valid email.");
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
        [Route("Accounts/Password/Forgot")]
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
                        Content = Content.Replace("{Link}", "/RecoverAccount?="+user.UserID);

                        bool CC = false;


                        SendEmail(user.Email, Content, Subject, CC, "Security Alert!");
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
        [Route("Accounts/Password/Forgot")]
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

        [HttpPost]
        [Route("Accounts/Unlock/{UserID}")]
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
        [Route("Accounts/Lock/{UserID}")]
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
                var UT = (from u in db.Users where u.UserID == UserID select u).FirstOrDefault();


                if (UT != null)
                {

                    if (UT.UserType == "SGB")
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

        #endregion

        #region Methods

        public object GetTeacher(string UserID)
        {
            GetUser gu = new GetUser();
            try
            {
                var user = (from b in db.Teachers
                            join c in db.Users on b.UserID equals c.UserID
                            join d in db.Schools on b.School_ID equals d.School_ID
                            join e in db.Departments on b.Department_ID equals e.ID
                            where b.UserID == UserID
                            select new GetUser
                            {
                                Status = "Success",
                                UserID = b.UserID,
                                School_ID = b.School_ID,
                                SchoolName = d.SchoolName,
                                Name = c.Name,
                                Surname = c.Surname,
                                Username = c.Username,
                                Email = c.Email,
                                UserType = c.UserType,
                                Department_ID = e.ID,
                                DepartmentName = e.DepartmentName,
                                HOD = b.IsHOD
                            }).FirstOrDefault();

                if (user != null)
                {
                    return user;
                }
                else
                {
                    throw new Exception("Failed to load teacher's information.");
                }

            }

            catch (Exception ex)
            {
                gu.Status = "Failed";
                gu.Data = ex.Message;
                return gu;
            }

        }
        public void SendEmail(string EmailTo, string Content, string Subject, bool CC, string EmailFrom)
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
                From = new MailAddress(SMTPUserName, EmailFrom),
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
        #endregion
    }
}