//using Newtonsoft.Json;
//using SchoolSystemApi.Models;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net;
//using System.Net.Http;
//using System.Web.Http;

//namespace SchoolSystemApi.Controllers
//{
//    public class AccountController : ApiController
//    {
//        Data.SchoolSystemEntities db = new Data.SchoolSystemEntities();
//        Methods api = new Methods();
//        string ZeroGuid = new Guid().ToString();

//        [HttpPost]
//        [Route("Account/Login")]
//        public object Login(Login l)
//        {
//            var gm = new GenericModel();
//            int LogID = 0;
//            string Activity = "You logged in on " + DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss");
//            string ActivityError = "You failed to login on " + DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss");

//            try
//            {
//                LogID = api.Log(LogID, l.Email, "Login", Activity, JsonConvert.SerializeObject(l), null);

//                //Validate input
//                if (string.IsNullOrEmpty(l.Email))
//                {
//                    throw new Exception("Email required");
//                }

//                if (string.IsNullOrEmpty(l.Password))
//                {
//                    throw new Exception("Password required");
//                }

//                var Account = (from a in db.Users where a.Email == l.Email & a.Active == true select a).FirstOrDefault();

//                if (Account != null)
//                {
//                    var User = (from b in db.Users where Account.UserID == b.UserID & b.Active == true select b).FirstOrDefault();

//                    if (User != null)
//                    {
//                        if (User.UserRole > 6 && User.UserRole < 11)
//                        {
//                            throw new Exception("Under aged user account, please login in as parent or gaurdian");
//                        }

//                        if (User.LockedOut)
//                        {
//                            throw new Exception("You are locked out of your account, please contact customer support for assistance");
//                        }

//                        if (Account.Password == l.Password)
//                        {

//                            User.LastLogin = DateTime.Now;
//                            User.DateModified = DateTime.Now;
//                            User.LoginAttempts = 0;
//                            User.ModifiedBy = ZeroGuid;
//                            db.SaveChanges();

//                            gm.Status = "Success";
//                            gm.Data = Account.UserID;
//                        }
//                        else
//                        {
//                            if (User.LoginAttempts > 4)
//                            {
//                                User.LockedOut = true;
//                                User.DateModified = DateTime.Now;
//                                User.ModifiedBy = ZeroGuid;
//                                db.SaveChanges();

//                                throw new Exception("Your account has been locked due to multiple failed login attempts, please contact customer support for assistance");
//                            }
//                            else
//                            {
//                                User.LoginAttempts = User.LoginAttempts + 1;
//                                User.ModifiedBy = ZeroGuid;
//                                User.DateModified = DateTime.Now;
//                                db.SaveChanges();

//                                throw new Exception("Incorrect password");
//                            }
//                        }
//                    }
//                    else
//                    {
//                        throw new Exception("Invalid user account");
//                    }

//                }

//                else
//                {
//                    throw new Exception("Invalid user email");
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
//        [Route("Account/Password/Forgot")]
//        public object ForgotPassword(ForgotPassword f)
//        {
//            var gm = new GenericModel();

//            string Subject = "Account credentials request";
//            string Content = "";
//            int LogID = 0;
//            string Activity = "You requested your account credentials on " + DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss");
//            string ActivityError = "You failed to request your account credential on " + DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss");
//            try
//            {
//                LogID = api.Log(LogID, f.Email, "ForgotPassword", Activity, JsonConvert.SerializeObject(f), null);

//                //Validate input
//                if (string.IsNullOrEmpty(f.Email))
//                {
//                    throw new Exception("Email required");
//                }

//                var a = (from c in db.Users where c.Email == f.Email & c.Active == true select c).FirstOrDefault();

//                if (a != null)
//                {
//                    var b = (from d in db.Users where d.UserID == a.UserID & d.Active == true select d).FirstOrDefault();

//                    Content = System.IO.File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + @"MailTemplates\ForgotPassword.html");
//                    Content = Content.Replace("{Name}", a.Name);
//                    Content = Content.Replace("{Email}", a.Email);
//                    Content = Content.Replace("{Password}", a.Password);

//                    api.SendEmail("School Support", a.Email, Content, Subject);

//                    gm.Status = "Success";
//                    gm.Data = "We sent your account credentials to your email address: " + a.Email;
//                }
//                else
//                {
//                    throw new Exception("Account not found, please provide your account email");
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
//        [Route("Account/Password/Update")]
//        public object UpdatePassword(UpdatePassword u)
//        {
//            GenericModel gm = new GenericModel();

//            string Subject = "Account password update";
//            string Content = "";
//            int LogID = 0;
//            string Activity = "You updated your account password on " + DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss");
//            string ActivityError = "You failed to updated your account password on " + DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss");
//            try
//            {
//                if (!Request.Headers.Contains("ApiKey"))
//                {
//                    throw new Exception("Access denied, unauthorized");
//                }

//                string ApiKey = Request.Headers.GetValues("ApiKey").FirstOrDefault();

//                LogID = api.Log(LogID, ApiKey, "UpdatePassword", Activity, JsonConvert.SerializeObject(u), null);

//                if (ApiKey != u.UserID)
//                {
//                    throw new Exception("Access denied, unauthorized to update account password");
//                }

//                //Validate input
//                if (string.IsNullOrEmpty(u.UserID))
//                {
//                    throw new Exception("User ID required");
//                }

//                if (string.IsNullOrEmpty(u.CurrentPassword))
//                {
//                    throw new Exception("Current password required");
//                }

//                if (string.IsNullOrEmpty(u.NewPassword))
//                {
//                    throw new Exception("New password required");
//                }

//                var user = (from a in db.Users where a.UserID == a.UserID & a.Active == true select a).FirstOrDefault();

//                if (user != null)
//                {
//                    if (user.Password != u.CurrentPassword)
//                    {
//                        throw new Exception("The password entered does not match your current account password");
//                    }

//                    else
//                    {
//                        user.Password = u.NewPassword;
//                        user.LastPassword = u.CurrentPassword;
//                        user.ModifiedBy = ApiKey;
//                        user.DateModified = DateTime.Now;
//                        db.SaveChanges();

//                        var sch = (from a in db.Schools
//                                      join b in db.UsersInSchools on a.SchoolID equals b.School
//                                      where b.UserID == user.UserID
//                                      select a).FirstOrDefault();

//                        gm.Status = "Success";
//                        gm.Data = "Your account password has been updated";

//                        Content = System.IO.File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + @"MailTemplates\PasswordUpdate.html");
//                        Content = Content.Replace("{Name}", user.Name);
//                        Content = Content.Replace("{Date}", user.DateModified.ToString());
//                        //Content = Content.Replace("{Link}", "/RecoverAccount?ID=" + user.UserID); 
//                        Content = Content.Replace("{Email}", user.Email);
//                        Content = Content.Replace("{Password}", user.Password);

//                        api.SendEmail(sch.SchoolName, user.Email, Content, Subject);

//                    }
//                }
//                else
//                {
//                    throw new Exception("Account not found");
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

//        //[HttpPost]
//        //[Route("Account/Recover")]
//        //public object RecoverAccount(UpdatePassword u)
//        //{
//        //    GenericModel gm = new GenericModel();

//        //    string Subject = "Forgot Password";
//        //    string Content = "";

//        //    try
//        //    {

//        //        //Validate input
//        //        if (string.IsNullOrEmpty(u.UserID))
//        //        {
//        //            throw new Exception("User ID required");
//        //        }

//        //        if (string.IsNullOrEmpty(u.NewPassword))
//        //        {
//        //            throw new Exception("New password required");
//        //        }


//        //        var User = (from a in kz.Users where a.UserID == u.UserID & a.Active == true select a).FirstOrDefault();

//        //        if (User != null)
//        //        {
//        //            User.Password = u.NewPassword;
//        //            User.LastPassword = u.NewPassword;

//        //            kz.SaveChanges();

//        //            gm.Status = "Success";
//        //            gm.Data = "Your account password has been updated";

//        //            //send email with account details
//        //        }
//        //        else
//        //        {
//        //            throw new Exception("Invalid user ID");
//        //        }
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        gm.Status = "Failed";
//        //        gm.Data = ex.Message;
//        //    }

//        //    return gm;
//        //}

//    }
//}
