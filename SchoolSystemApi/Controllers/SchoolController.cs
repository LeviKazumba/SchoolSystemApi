using Newtonsoft.Json;
using SchoolSystemApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace SchoolSystemApi.Controllers
{
    public class SchoolController : ApiController
    {

        Data.SchoolSystemEntities db = new Data.SchoolSystemEntities();
        Methods api = new Methods();

        #region Users

        [HttpPost]
        [Route("School/User/Create")]
        public object CreateUser(UserModel n)
        {
            var gm = new GenericModel();
            string UserAccess;
            string Check;
            int LogID = 0;
            string Activity = "You created a new user account on " + DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss");
            string ActivityError = "You failed to create a new user account on " + DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss");

            try
            {
                if (!Request.Headers.Contains("ApiKey"))
                {
                    throw new Exception("Access denied, unauthorized");
                }

                string ApiKey = Request.Headers.GetValues("ApiKey").FirstOrDefault();

                LogID = api.Log(LogID, ApiKey, "CreateUser", Activity, JsonConvert.SerializeObject(n), null);

                if (string.IsNullOrEmpty(ApiKey))
                {
                    throw new Exception("Access denied, unauthorized");
                }

                if ((UserAccess = api.CheckUserAccess(ApiKey, "UserWrite")) == "NO")
                {
                    throw new Exception("Access denied, unauthorized");
                }

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

                if (string.IsNullOrEmpty(n.UNCode))
                {
                    throw new Exception("Country required");
                }

                if (string.IsNullOrEmpty(n.UserType.ToString()))
                {
                    throw new Exception("User type required");
                }

                if (string.IsNullOrEmpty(n.UserRole.ToString()))
                {
                    throw new Exception("User role required");
                }

                if (string.IsNullOrEmpty(n.SchoolID))
                {
                    throw new Exception("School ID required");
                }

                if ((Check = api.UserDuplicate(n.Email, "Email")) == "YES")
                {
                    throw new Exception("The email provided is already taken, please try another email");
                }

                if ((Check = api.UserDuplicate(n.Telephone, "Telephone")) == "YES")
                {
                    throw new Exception("The telephone number provided is already taken, please try another number");
                }

                var u = new Data.User();

                u.UserID = Guid.NewGuid().ToString("N");

                //Generate random password
                u.Password = Guid.NewGuid().ToString().Substring(1, 6);
                u.LastPassword = u.Password;

                u.Name = n.Name;
                u.Surname = n.Surname;
                u.Email = n.Email;
                u.Telephone = n.Telephone;
                u.Age = n.Age;
                u.Gender = n.Gender;
                u.Country = n.UNCode;
                u.UserType = n.UserType;
                u.UserRole = n.UserRole;
                u.LastLogin = DateTime.Now;
                u.LoginAttempts = 0;
                u.LockedOut = false;
                u.Active = true;
                u.CreatedBy = ApiKey;
                u.DateCreated = DateTime.Now;

                db.Users.Add(u);
                db.SaveChanges();

                api.SetDefaultUserAccess(ApiKey, u.UserID);

                var m = new Data.UsersInSchool();
                m.School = n.SchoolID;
                m.UserID = u.UserID;
                m.CreatedBy = ApiKey;
                m.DateCreated = DateTime.Now;

                db.UsersInSchools.Add(m);
                db.SaveChanges();

                gm.Status = "Success";
                gm.Data = u.Name + " has been created";

                //send welcome email
            }

            catch (Exception ex)
            {
                string ErrorData = ex.Message + "\n" + ex.InnerException + "\n" + ex.StackTrace;
                api.Log(LogID, "", "", ActivityError, "", ErrorData);

                gm.Status = "Failed";
                gm.Data = ex.Message;
            }
            return gm;
        }

        [HttpPost]
        [Route("School/User/Update")]
        public object UpdateUser(UserModel n)
        {
            var gm = new GenericModel();
            string UserAccess;
            int LogID = 0;
            string Activity = "You updated a user account on " + DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss");
            string ActivityError = "You failed to update a user account on " + DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss");

            try
            {

                if (!Request.Headers.Contains("ApiKey"))
                {
                    throw new Exception("Access denied, unauthorized");
                }

                string ApiKey = Request.Headers.GetValues("ApiKey").FirstOrDefault();

                LogID = api.Log(LogID, ApiKey, "UpdateUser", Activity, JsonConvert.SerializeObject(n), null);

                if (string.IsNullOrEmpty(ApiKey))
                {
                    throw new Exception("Access denied, unauthorized");
                }

                if ((UserAccess = api.CheckUserAccess(ApiKey, "UserWrite")) == "NO")
                {
                    throw new Exception("Access denied, unauthorized");
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

                if (string.IsNullOrEmpty(n.Age.ToString()))
                {
                    throw new Exception("Age required");
                }

                if (string.IsNullOrEmpty(n.UNCode))
                {
                    throw new Exception("Country required");
                }

                //Update User in KazumbaFoundation db
                var a = (from b in db.Users where b.UserID == n.UserID & b.Active select b).FirstOrDefault();

                if (a != null)
                {
                    //User is updating their own profile
                    if (ApiKey == a.UserID)
                    {
                        a.Name = n.Name;
                        a.Surname = n.Surname;
                        a.Age = n.Age;
                        a.Gender = n.Gender;
                        a.Country = n.UNCode;
                        a.ModifiedBy = ApiKey;
                        a.DateModified = DateTime.Now;

                        db.SaveChanges();
                    }
                    else
                    {
                        //Check if this user can update another's user's profile
                        if ((UserAccess = api.CheckUserAccess(ApiKey, "UserWrite")) == "NO")
                        {
                            throw new Exception("Access denied, unauthorized");
                        }

                        a.Name = n.Name;
                        a.Surname = n.Surname;
                        a.Age = n.Age;
                        a.Country = n.UNCode;
                        a.ModifiedBy = ApiKey;
                        a.DateModified = DateTime.Now;

                        db.SaveChanges();
                    }

                    gm.Status = "Success";
                    gm.Data = n.Name + " has been updated";
                }
                else
                {
                    throw new Exception("User not found");
                }

            }

            catch (Exception ex)
            {
                string ErrorData = ex.Message + "\n" + ex.InnerException + "\n" + ex.StackTrace;
                api.Log(LogID, "", "", ActivityError, "", ErrorData);

                gm.Status = "Failed";
                gm.Data = ex.Message;
            }
            return gm;
        }

        [HttpGet]
        [Route("School/UserTypes")]
        public object GetUserTypes()
        {
            var gm = new GenericModel();
            int LogID = 0;
            try
            {
                if (!Request.Headers.Contains("ApiKey"))
                {
                    throw new Exception("Access denied, unauthorized");
                }

                string ApiKey = Request.Headers.GetValues("ApiKey").FirstOrDefault();

                LogID = api.Log(LogID, ApiKey, "GetUserTypes", "", JsonConvert.SerializeObject(ApiKey), null);

                var c = (from a in db.UserTypes
                         join b in db.Users on a.CreatedBy equals b.UserID
                         join d in db.Users on a.ModifiedBy equals d.UserID
                         where a.Active == true
                         select new UserTypeModel
                         {
                             ID = a.ID,
                             Name = a.Name,
                             CreatedBy = b.Email,
                             ModifiedBy = d.Email,
                             DateCreated = a.DateCreated,
                             DateModified = a.DateModified
                         }).ToList();

                if (c != null)
                {
                    return c;
                }
                else
                {
                    throw new Exception("Failed to load user types");
                }
            }

            catch (Exception ex)
            {
                string ErrorData = ex.Message + "\n" + ex.InnerException + "\n" + ex.StackTrace;
                api.Log(LogID, "", "", "", "", ErrorData);

                gm.Status = "Failed";
                gm.Data = ex.Message;

                return gm;

            }
        }

        [HttpGet]
        [Route("School/UserRoles")]
        public object GetUserRoles()
        {
            var gm = new GenericModel();
            int LogID = 0;
            try
            {
                if (!Request.Headers.Contains("ApiKey"))
                {
                    throw new Exception("Access denied, unauthorized");
                }

                string ApiKey = Request.Headers.GetValues("ApiKey").FirstOrDefault();

                LogID = api.Log(LogID, ApiKey, "GetUserRoles", "", JsonConvert.SerializeObject(ApiKey), null);

                var c = (from a in db.UserRoles
                         join b in db.Users on a.CreatedBy equals b.UserID
                         join d in db.Users on a.ModifiedBy equals d.UserID
                         where a.Active == true
                         select new UserRoleModel
                         {

                             ID = a.ID,
                             Name = a.Name,
                             CreatedBy = b.Email,
                             ModifiedBy = d.Email,
                             DateCreated = a.DateCreated,
                             DateModified = a.DateModified
                         }).ToList();

                if (c != null)
                {
                    return c;
                }
                else
                {
                    throw new Exception("Failed to load user roles");
                }
            }

            catch (Exception ex)
            {
                string ErrorData = ex.Message + "\n" + ex.InnerException + "\n" + ex.StackTrace;
                api.Log(LogID, "", "", "", "", ErrorData);

                gm.Status = "Failed";
                gm.Data = ex.Message;

                return gm;

            }
        }

        [HttpPost]
        [Route("School/User/Activate/{UserID}/")]
        public object ActivateUser(string UserID)
        {
            var gm = new GenericModel();
            string UserAccess;
            string Check;
            int LogID = 0;
            string Activity = "You activated a user account on " + DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss");
            string ActivityError = "You failed to activate a user account on " + DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss");

            string Email = "";
            string Telephone = "";
            try
            {
                if (!Request.Headers.Contains("ApiKey"))
                {
                    throw new Exception("Access denied, unauthorized");
                }

                string ApiKey = Request.Headers.GetValues("ApiKey").FirstOrDefault();

                LogID = api.Log(LogID, ApiKey, "ActivateUser", Activity, JsonConvert.SerializeObject(UserID), null);

                if (string.IsNullOrEmpty(ApiKey))
                {
                    throw new Exception("Access denied, unauthorized");
                }

                if ((UserAccess = api.CheckUserAccess(ApiKey, "UserWrite")) == "NO")
                {
                    throw new Exception("Access denied, unauthorized");
                }

                if (string.IsNullOrEmpty(UserID))
                {
                    throw new Exception("User ID required");
                }

                var user = (from u in db.Users where u.UserID == UserID select u).FirstOrDefault();

                if (user != null)
                {
                    Email = user.Email.Replace("%", "");
                    Telephone = user.Telephone.Replace("%", "");

                    if ((Check = api.UserDuplicate(Email, "Email")) == "YES")
                    {
                        throw new Exception("Email: " + user.Email + ", is already in use. Please contact customer support for assistance in activating this account");
                    }

                    if ((Check = api.UserDuplicate(Telephone, "Telephone")) == "YES")
                    {
                        throw new Exception("Telephone: " + user.Telephone + ", is already in use. Please customer contact support for assistance in activating this account");
                    }


                    user.Email = Email;
                    user.Telephone = Telephone;
                    user.Active = true;
                    user.ModifiedBy = ApiKey;
                    user.DateModified = DateTime.Now;
                    db.SaveChanges();

                    gm.Status = "Success";
                    gm.Data = user.Name + " has been activated";

                }
                else
                {
                    throw new Exception("User not found");
                }

            }
            catch (Exception ex)
            {
                string ErrorData = ex.Message + "\n" + ex.InnerException + "\n" + ex.StackTrace;
                api.Log(LogID, "", "", "", "", ErrorData);

                gm.Status = "Failed";
                gm.Data = ex.Message;
            }
            return gm;
        }

        [HttpPost]
        [Route("School/User/Deactivate/{UserID}/")]
        public object DeactivateUser(string UserID)
        {
            var gm = new GenericModel();
            string UserAccess;
            int LogID = 0;
            string Activity = "You deactivated a user account on " + DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss");
            string ActivityError = "You failed to deactivate a user account on " + DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss");
            try
            {
                if (!Request.Headers.Contains("ApiKey"))
                {
                    throw new Exception("Access denied, unauthorized");
                }

                string ApiKey = Request.Headers.GetValues("ApiKey").FirstOrDefault();

                LogID = api.Log(LogID, ApiKey, "ActivateUser", Activity, JsonConvert.SerializeObject(UserID), null);

                if (string.IsNullOrEmpty(ApiKey))
                {
                    throw new Exception("Access denied, unauthorized");
                }

                if ((UserAccess = api.CheckUserAccess(ApiKey, "UserWrite")) == "NO")
                {
                    throw new Exception("Access denied, unauthorized");
                }

                if (string.IsNullOrEmpty(UserID))
                {
                    throw new Exception("User ID required");
                }

                var user = (from u in db.Users where u.UserID == UserID select u).FirstOrDefault();

                if (user != null)
                {
                    user.Email = "%" + user.Email;
                    user.Telephone = "%" + user.Telephone;
                    user.Active = false;
                    user.ModifiedBy = ApiKey;
                    user.DateModified = DateTime.Now;
                    db.SaveChanges();

                    gm.Status = "Success";
                    gm.Data = user.Name + " has been deactivated";

                }
                else
                {
                    throw new Exception("User not found");
                }

            }
            catch (Exception ex)
            {
                string ErrorData = ex.Message + "\n" + ex.InnerException + "\n" + ex.StackTrace;
                api.Log(LogID, "", "", "", "", ErrorData);

                gm.Status = "Failed";
                gm.Data = ex.Message;
            }
            return gm;
        }

        [HttpPost]
        [Route("School/User/Unlock/{UserID}")]
        public object UnlockUser(string UserID)
        {
            var gm = new GenericModel();
            string UserAccess;
            int LogID = 0;
            string Activity = "You unlocked a user account on " + DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss");
            string ActivityError = "You failed to unlock a user account on " + DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss");
            try
            {
                if (!Request.Headers.Contains("ApiKey"))
                {
                    throw new Exception("Access denied, unauthorized");
                }

                string ApiKey = Request.Headers.GetValues("ApiKey").FirstOrDefault();

                LogID = api.Log(LogID, ApiKey, "UnlockUser", Activity, JsonConvert.SerializeObject(UserID), null);

                if (string.IsNullOrEmpty(ApiKey))
                {
                    throw new Exception("Access denied, unauthorized");
                }

                if ((UserAccess = api.CheckUserAccess(ApiKey, "UserWrite")) == "NO")
                {
                    throw new Exception("Access denied, unauthorized");
                }

                if (string.IsNullOrEmpty(UserID))
                {
                    throw new Exception("User ID required");
                }

                var user = (from u in db.Users where u.UserID == UserID select u).FirstOrDefault();

                if (user != null)
                {
                    user.LockedOut = false;
                    user.ModifiedBy = ApiKey;
                    user.DateModified = DateTime.Now;
                    db.SaveChanges();

                    gm.Status = "Success";
                    gm.Data = user.Name + " has been unlocked";

                }
                else
                {
                    throw new Exception("User not found");
                }

            }
            catch (Exception ex)
            {
                string ErrorData = ex.Message + "\n" + ex.InnerException + "\n" + ex.StackTrace;
                api.Log(LogID, "", "", "", "", ErrorData);

                gm.Status = "Failed";
                gm.Data = ex.Message;
            }

            return gm;
        }

        [HttpPost]
        [Route("School/User/Lock/{UserID}")]
        public object LockUser(string UserID)
        {
            var gm = new GenericModel();
            string UserAccess;
            int LogID = 0;
            string Activity = "You locked a user account on " + DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss");
            string ActivityError = "You failed to lock a user account on " + DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss");
            try
            {
                if (!Request.Headers.Contains("ApiKey"))
                {
                    throw new Exception("Access denied, unauthorized");
                }

                string ApiKey = Request.Headers.GetValues("ApiKey").FirstOrDefault();

                LogID = api.Log(LogID, ApiKey, "LockUser", Activity, JsonConvert.SerializeObject(UserID), null);

                if (string.IsNullOrEmpty(ApiKey))
                {
                    throw new Exception("Access denied, unauthorized");
                }

                if ((UserAccess = api.CheckUserAccess(ApiKey, "UserWrite")) == "NO")
                {
                    throw new Exception("Access denied, unauthorized");
                }

                if (string.IsNullOrEmpty(UserID))
                {
                    throw new Exception("User ID required");
                }

                var user = (from u in db.Users where u.UserID == UserID select u).FirstOrDefault();

                if (user != null)
                {
                    user.LockedOut = true;
                    user.ModifiedBy = ApiKey;
                    user.DateModified = DateTime.Now;
                    db.SaveChanges();

                    gm.Status = "Success";
                    gm.Data = user.Name + " has been locked";

                }
                else
                {
                    throw new Exception("User not found");
                }

            }
            catch (Exception ex)
            {
                string ErrorData = ex.Message + "\n" + ex.InnerException + "\n" + ex.StackTrace;
                api.Log(LogID, "", "", "", "", ErrorData);

                gm.Status = "Failed";
                gm.Data = ex.Message;
            }

            return gm;
        }

        [HttpPost]
        [Route("School/User/{UserID}")]
        public object GetUser(string UserID)
        {
            var gm = new GenericModel();
            string UserAccess;
            int LogID = 0;
            string Activity = "You requested user information on " + DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss");
            string ActivityError = "You failed to request user information on " + DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss");

            try
            {
                if (!Request.Headers.Contains("ApiKey"))
                {
                    throw new Exception("Access denied, unauthorized");
                }

                string ApiKey = Request.Headers.GetValues("ApiKey").FirstOrDefault();

                LogID = api.Log(LogID, ApiKey, "GetUser", Activity, JsonConvert.SerializeObject(UserID), null);

                if (string.IsNullOrEmpty(ApiKey))
                {
                    throw new Exception("Access denied, unauthorized");
                }

                //if ((UserAccess = api.CheckUserAccess(ApiKey, "UserRead")) == "NO")
                //{
                //    throw new Exception("Access denied, unauthorized");
                //}

                if (string.IsNullOrEmpty(UserID))
                {
                    throw new Exception("User ID required");
                }

                var a = (from b in db.Users
                         join c in db.UserTypes on b.UserType equals c.ID
                         join d in db.UserRoles on b.UserRole equals d.ID
                         join e in db.Countries on b.Country equals e.UNCode
                         join f in db.UsersInSchools on b.UserID equals f.UserID
                         join g in db.Schools on f.School equals g.SchoolID
                         join h in db.SchoolTypes on g.SchoolType equals h.ID
                         where b.UserID == UserID && b.Active == true
                         select new UserModel
                         {
                             Status = "Success",
                             UserID = b.UserID,
                             Name = b.Name,
                             Surname = b.Surname,
                             Email = b.Email,
                             Telephone = b.Telephone,
                             Age = b.Age,
                             Gender = b.Gender,

                             CreatedBy = b.CreatedBy,
                             ModifiedBy = b.ModifiedBy,
                             DateCreated = b.DateCreated,
                             DateModified = b.DateModified,

                             CountryName = e.Name,
                             PhoneCode = e.PhoneCode,
                             UNCode = e.UNCode,

                             UserType = c.ID,
                             UserTypeName = c.Name,

                             UserRole = d.ID,
                             UserRoleName = d.Name,

                             LastLogin = b.LastLogin,
                             LockedOut = b.LockedOut,
                             Active = b.Active,

                             SchoolID = g.SchoolID,
                             SchoolName = g.SchoolName,
                             SchoolEmail = g.Email,
                             SchoolTelephone = g.Telephone,
                             SchoolAddress = g.Address,
                             SchoolLogo = g.Logo,

                             SchoolTypeID = h.ID,
                             SchoolTypeName = h.Name,

                         }).FirstOrDefault();

                if (a != null)
                {
                    var func = (from x in db.UserFeaturesAndFunctions where x.UserID == a.UserID && x.Active == true select x).ToList();

                    List<Functions> item = new List<Functions>();

                    foreach (var y in func)
                    {
                        var v = (from x in db.Functions where x.ID == y.ID select x).FirstOrDefault();

                        item.Add(new Functions
                        {
                            ID = v.ID,
                            Name = v.Name,
                            Description = v.Description,
                            Active = v.Active,
                            CreatedBy = v.CreatedBy,
                            ModifiedBy = v.ModifiedBy,
                            DateCreated = v.DateCreated,
                            DateModified = v.DateModified
                        });

                    }

                    a.Functions = item;

                    return a;
                }
                else
                {
                    throw new Exception("User not found");
                }
            }
            catch (Exception ex)
            {
                string ErrorData = ex.Message + "\n" + ex.InnerException + "\n" + ex.StackTrace;
                api.Log(LogID, "", "", ActivityError, "", ErrorData);

                gm.Status = "Failed";
                gm.Data = ex.Message;

                return gm;
            }
        }

        //[HttpPost]
        //[Route("School/Users/{School}")]
        //public object GetUsers(string School)
        //{
        //    var gm = new GenericModel();
        //    string UserAccess;
        //    int LogID = 0;
        //    string Activity = "You requested all school users information on " + DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss");
        //    string ActivityError = "You failed to request all school users information on " + DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss");

        //    try
        //    {
        //        if (!Request.Headers.Contains("ApiKey"))
        //        {
        //            throw new Exception("Access denied, unauthorized");
        //        }

        //        string ApiKey = Request.Headers.GetValues("ApiKey").FirstOrDefault();

        //        LogID = api.Log(LogID, ApiKey, "GetUsers", Activity, JsonConvert.SerializeObject(School), null);

        //        if (string.IsNullOrEmpty(ApiKey))
        //        {
        //            throw new Exception("Access denied, unauthorized");
        //        }

        //        if ((UserAccess = api.CheckUserAccess(ApiKey, "UserRead")) == "NO")
        //        {
        //            throw new Exception("Access denied, unauthorized");
        //        }

        //        if (string.IsNullOrEmpty(School))
        //        {
        //            throw new Exception("School ID required");
        //        }

        //        var sch = (from a in db.Schools where a.SchoolID == School & a.Active == true select a).FirstOrDefault();

        //        if (sch != null)
        //        {
        //            var list = (from a in db.Users
        //                        join c in db.Countries on a.Country equals c.UNCode
        //                        join d in db.UserTypes on a.UserType equals d.ID
        //                        join e in db.UserRoles on a.UserRole equals e.ID
        //                        join g in db.SchoolTypes on sch.SchoolType equals g.ID
        //                        join x in db.UsersInSchools on a.UserID equals x.UserID
        //                        where sch.SchoolID == x.School
        //                        select new UserModel
        //                        {
        //                            UserID = a.UserID,
        //                            Name = a.Name,
        //                            Surname = a.Surname,
        //                            Email = a.Email,
        //                            Telephone = a.Telephone,

        //                            CountryName = c.Name,
        //                            PhoneCode = c.PhoneCode,
        //                            UNCode = c.UNCode,

        //                            UserType = d.ID,
        //                            UserTypeName = d.Name,
        //                            UserRole = e.ID,
        //                            UserRoleName = e.Name,

        //                            LastLogin = a.LastLogin,
        //                            LockedOut = a.LockedOut,
        //                            Active = a.Active,
        //                            DateCreated = a.DateCreated,
        //                            DateModified = a.DateModified,

        //                            SetUserFunctions = a.SetUserFunctions,
        //                            SchoolWrite = a.SchoolWrite,
        //                            SchoolRead = a.SchoolRead,
        //                            UserWrite = a.UserWrite,
        //                            UserRead = a.UserRead,

        //                            SchoolID = sch.SchoolID,
        //                            SchoolName = sch.SchoolName,
        //                            SchoolTypeID = g.ID,
        //                            SchoolTypeName = g.Name,

        //                        }).ToList();

        //            return list;

        //        }
        //        else
        //        {
        //            throw new Exception("School not found");
        //        }

        //    }
        //    catch (Exception ex)
        //    {

        //        string ErrorData = ex.Message + "\n" + ex.InnerException + "\n" + ex.StackTrace;
        //        api.Log(LogID, "", "", ActivityError, "", ErrorData);

        //        gm.Status = "Failed";
        //        gm.Data = ex.Message;

        //        return gm;
        //    }
        //}

        #endregion

        #region School
        [HttpPost]
        [Route("School/Update")]
        public object UpdateSchool()
        {
            var gm = new GenericModel();
            string UserAccess;

            int LogID = 0;
            string Activity = "You updated a school on " + DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss");
            string ActivityError = "You failed to update a school on " + DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss");

            try
            {
                if (!Request.Headers.Contains("ApiKey"))
                {
                    throw new Exception("Access denied, unauthorized");
                }

                var httpRequest = HttpContext.Current.Request;

                string ApiKey = Request.Headers.GetValues("ApiKey").FirstOrDefault();

                Dictionary<string, string> Req = httpRequest.Params.AllKeys.ToDictionary(x => x, x => httpRequest.Params[x]);

                LogID = api.Log(LogID, ApiKey, "UpdateSchool", Activity, JsonConvert.SerializeObject(Req), null);

                if (string.IsNullOrEmpty(ApiKey))
                {
                    throw new Exception("Access denied, unauthorized");
                }

                if ((UserAccess = api.CheckUserAccess(ApiKey, "SchoolWrite")) == "NO")
                {
                    throw new Exception("Access denied, unauthorized");
                }


                //school validation
                if (string.IsNullOrEmpty(httpRequest.Params["SchoolID"]))
                {
                    throw new Exception("School ID required");
                }

                string SchoolID = httpRequest.Params["SchoolID"];

                if (string.IsNullOrEmpty(httpRequest.Params["SchoolName"]))
                {
                    throw new Exception("School name required");
                }

                string SchoolName = httpRequest.Params["SchoolName"];

                if (string.IsNullOrEmpty(httpRequest.Params["SchoolType"]))
                {
                    throw new Exception("Education level required");
                }

                int SchoolType = Convert.ToInt32(httpRequest.Params["SchoolType"]);

                if (string.IsNullOrEmpty(httpRequest.Params["Country"]))
                {
                    throw new Exception("Country required");
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
                var sc = (from s in db.Schools where SchoolID == s.SchoolID select s).FirstOrDefault();

                if (sc != null)
                {
                    sc.SchoolName = SchoolName;
                    sc.SchoolType = SchoolType;
                    sc.Country = Country;
                    sc.Address = Address;

                    if (httpRequest.Files[0].ContentLength > 0)
                    {
                        HttpPostedFile Logo = httpRequest.Files[0];

                        string path = api.UploadFile(Logo);
                        sc.Logo = path;
                    }

                    sc.CompanyRegistrationNumber = CompanyRegistrationNumber;
                    sc.DateModified = DateTime.Now;
                    sc.ModifiedBy = ApiKey;

                    db.SaveChanges();

                    gm.Status = "Success";
                    gm.Data = SchoolName + " has been updated";
                }
                else
                {
                    throw new Exception("School not found");
                }

            }
            catch (Exception ex)
            {
                string ErrorData = ex.Message + "\n" + ex.InnerException + "\n" + ex.StackTrace;
                api.Log(LogID, "", "", ActivityError, "", ErrorData);

                gm.Status = "Failed";
                gm.Data = ex.Message;
            }

            return gm;
        }

        [HttpPost]
        [Route("School/{SchoolID}")]
        public object GetSchool(string SchoolID)
        {
            var gm = new GenericModel();
            string UserAccess;
            int LogID = 0;
            string Activity = "You requested school information on " + DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss");
            string ActivityError = "You failed to request school information on " + DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss");

            try
            {

                if (!Request.Headers.Contains("ApiKey"))
                {
                    throw new Exception("Access denied, unauthorized");
                }

                string ApiKey = Request.Headers.GetValues("ApiKey").FirstOrDefault();

                LogID = api.Log(LogID, ApiKey, "GetSchool", Activity, JsonConvert.SerializeObject(SchoolID), null);

                if (string.IsNullOrEmpty(ApiKey))
                {
                    throw new Exception("Access denied, unauthorized");
                }

                if ((UserAccess = api.CheckUserAccess(ApiKey, "SchoolRead")) == "NO")
                {
                    throw new Exception("Access denied, unauthorized");
                }

                var s = (from a in db.Schools
                         join b in db.SchoolTypes on a.SchoolType equals b.ID
                         join c in db.Countries on a.Country equals c.UNCode

                         where a.SchoolID == SchoolID
                         select new SchoolModel
                         {
                             Status = "Success",
                             SchoolID = a.SchoolID,
                             SchoolName = a.SchoolName,

                             SchoolType = b.ID,
                             SchoolTypeName = b.Name,

                             Email = a.Email,
                             Telephone = a.Telephone,

                             Country = c.Name,
                             PhoneCode = c.PhoneCode,
                             UNCode = c.UNCode,

                             Address = a.Address,
                             Active = a.Active,
                             Logo = a.Logo,
                             CompanyRegistrationNumber = a.CompanyRegistrationNumber,

                             DateCreated = (DateTime)a.DateCreated,
                             DateModified = (DateTime)a.DateModified

                         }).FirstOrDefault();

                if (s != null)
                {
                    return s;
                }
                else
                {
                    throw new Exception("School not found");
                }

            }
            catch (Exception ex)
            {
                string ErrorData = ex.Message + "\n" + ex.InnerException + "\n" + ex.StackTrace;
                api.Log(LogID, "", "", ActivityError, "", ErrorData);

                gm.Status = "Failed";
                gm.Data = ex.Message;

                return gm;

            }
        }

        [HttpGet]
        [Route("School/Countries")]
        public object GetCountries()
        {
            var gm = new GenericModel();
            int LogID = 0;
            try
            {
                if (!Request.Headers.Contains("ApiKey"))
                {
                    throw new Exception("Access denied, unauthorized");
                }

                string ApiKey = Request.Headers.GetValues("ApiKey").FirstOrDefault();

                LogID = api.Log(LogID, ApiKey, "GetCountries", "", JsonConvert.SerializeObject(ApiKey), null);

                var c = (from a in db.Countries where a.Active == true select a).ToList();

                return c;
            }

            catch (Exception ex)
            {
                string ErrorData = ex.Message + "\n" + ex.InnerException + "\n" + ex.StackTrace;
                api.Log(LogID, "", "", "", "", ErrorData);

                gm.Status = "Failed";
                gm.Data = ex.Message;

                return gm;

            }
        }

        [HttpGet]
        [Route("School/UserTypes")]
        public object GetSchoolTypes()
        {
            var gm = new GenericModel();
            int LogID = 0;
            try
            {
                if (!Request.Headers.Contains("ApiKey"))
                {
                    throw new Exception("Access denied, unauthorized");
                }

                string ApiKey = Request.Headers.GetValues("ApiKey").FirstOrDefault();

                LogID = api.Log(LogID, ApiKey, "GetSchoolTypes", "", JsonConvert.SerializeObject(ApiKey), null);

                var c = (from a in db.UserTypes where a.Active == true select a).ToList();

                return c;
            }

            catch (Exception ex)
            {
                string ErrorData = ex.Message + "\n" + ex.InnerException + "\n" + ex.StackTrace;
                api.Log(LogID, "", "", "", "", ErrorData);

                gm.Status = "Failed";
                gm.Data = ex.Message;

                return gm;

            }
        }

        [HttpPost]
        [Route("Student/School/ClockIn")]
        public object StudentSchoolClockIn(AttendanceRegisterModel a)
        {
            var gm = new GenericModel();
            string UserAccess;
            int LogID = 0;
            string Activity = "You marked the school attendance on " + DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss");
            string ActivityError = "You failed to mark the school attendance on " + DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss");
            var att = new Data.AttendanceRegister();
            try
            {

                if (!Request.Headers.Contains("ApiKey"))
                {
                    throw new Exception("Access denied, unauthorized");
                }

                string ApiKey = Request.Headers.GetValues("ApiKey").FirstOrDefault();

                LogID = api.Log(LogID, ApiKey, "SchoolAttendance", Activity, JsonConvert.SerializeObject(a), null);

                if (string.IsNullOrEmpty(ApiKey))
                {
                    throw new Exception("Access denied, unauthorized");
                }

                if ((UserAccess = api.CheckUserAccess(ApiKey, "AttendanceWrite")) == "NO")
                {
                    throw new Exception("Access denied, unauthorized");
                }

                //validation
                if (string.IsNullOrEmpty(a.School))
                {
                    throw new Exception("School required");
                }

                if (string.IsNullOrEmpty(a.Student))
                {
                    throw new Exception("Student required");
                }

                att.School = a.School;
                att.SchoolStaff = ApiKey;
                att.Student = a.Student;
                att.ClockIn = DateTime.Now;
                att.CreatedBy = api.GetUserEmail(ApiKey);
                att.DateCreated = DateTime.Now;
                att.Present = true;
                db.AttendanceRegisters.Add(att);
                db.SaveChanges();

                gm.Status = "Success";
                gm.Data = "Student has been marked present";

                //notify parent that student has entered school

            }
            catch (Exception ex)
            {
                string ErrorData = ex.Message + "\n" + ex.InnerException + "\n" + ex.StackTrace;
                api.Log(LogID, "", "", ActivityError, "", ErrorData);

                gm.Status = "Failed";
                gm.Data = ex.Message;
            }

            return gm;
        }

        [HttpPost]
        [Route("School/Class/Create")]
        public object CreateClass(ClassModel sm)
        {
            var gm = new GenericModel();
            string UserAccess;
            int LogID = 0;
            string Activity = "You created a new school class on " + DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss");
            string ActivityError = "You failed to create a new school class on " + DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss");
            var a = new Data.Class();
            var b = new Data.TeachersAndClass();
            try
            {

                if (!Request.Headers.Contains("ApiKey"))
                {
                    throw new Exception("Access denied, unauthorized");
                }

                string ApiKey = Request.Headers.GetValues("ApiKey").FirstOrDefault();

                LogID = api.Log(LogID, ApiKey, "CreateClass", Activity, JsonConvert.SerializeObject(sm), null);

                if (string.IsNullOrEmpty(ApiKey))
                {
                    throw new Exception("Access denied, unauthorized");
                }

                if ((UserAccess = api.CheckUserAccess(ApiKey, "ClassWrite")) == "NO")
                {
                    throw new Exception("Access denied, unauthorized");
                }

                //validation
                if (string.IsNullOrEmpty(sm.School))
                {
                    throw new Exception("School required");
                }

                if (string.IsNullOrEmpty(sm.Name))
                {
                    throw new Exception("Class name required");
                }

                if (string.IsNullOrEmpty(sm.Teacher))
                {
                    throw new Exception("Class teacher required");
                }

                a.School = sm.School;
                a.Name = sm.Name;
                a.Description = sm.Description;
                a.Active = true;
                a.CreatedBy = ApiKey;
                a.DateCreated = DateTime.Now;

                db.Classes.Add(a);
                db.SaveChanges();

                b.Class = a.ID;
                b.Teacher = sm.Teacher;
                b.SubTeacher = sm.SubTeacher;
                b.Active = true;
                b.CreatedBy = ApiKey;
                b.DateCreated = DateTime.Now;

                gm.Status = "Success";
                gm.Data = a.Name + " has been added";

            }
            catch (Exception ex)
            {
                string ErrorData = ex.Message + "\n" + ex.InnerException + "\n" + ex.StackTrace;
                api.Log(LogID, "", "", ActivityError, "", ErrorData);

                gm.Status = "Failed";
                gm.Data = ex.Message;
            }

            return gm;
        }

        [HttpPost]
        [Route("School/Subject/Create")]
        public object CreateSubject(SubjectModel sm)
        {
            var gm = new GenericModel();
            string UserAccess;
            int LogID = 0;
            string Activity = "You created a new school subject on " + DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss");
            string ActivityError = "You failed to create a new school subject on " + DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss");
            var a = new Data.Subject();
            try
            {

                if (!Request.Headers.Contains("ApiKey"))
                {
                    throw new Exception("Access denied, unauthorized");
                }

                string ApiKey = Request.Headers.GetValues("ApiKey").FirstOrDefault();

                LogID = api.Log(LogID, ApiKey, "CreateSubject", Activity, JsonConvert.SerializeObject(sm), null);

                if (string.IsNullOrEmpty(ApiKey))
                {
                    throw new Exception("Access denied, unauthorized");
                }

                if ((UserAccess = api.CheckUserAccess(ApiKey, "SubjectWrite")) == "NO")
                {
                    throw new Exception("Access denied, unauthorized");
                }

                //validation
                if (string.IsNullOrEmpty(sm.School))
                {
                    throw new Exception("School required");
                }

                if (string.IsNullOrEmpty(sm.Name))
                {
                    throw new Exception("Subject title required");
                }

                a.School = sm.School;
                a.Name = sm.Name;
                a.Description = sm.Description;
                a.Active = true;
                a.CreatedBy = ApiKey;
                a.DateCreated = DateTime.Now;

                db.Subjects.Add(a);
                db.SaveChanges();

                gm.Status = "Success";
                gm.Data = a.Name + " has been added";


            }
            catch (Exception ex)
            {
                string ErrorData = ex.Message + "\n" + ex.InnerException + "\n" + ex.StackTrace;
                api.Log(LogID, "", "", ActivityError, "", ErrorData);

                gm.Status = "Failed";
                gm.Data = ex.Message;
            }

            return gm;
        }

        [HttpPost]
        [Route("School/Subject/Update")]
        public object UpdateSubject(SubjectModel sm)
        {
            var gm = new GenericModel();
            string UserAccess;
            int LogID = 0;
            string Activity = "You updated a school subject on " + DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss");
            string ActivityError = "You failed to update a school subject on " + DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss");
            try
            {

                if (!Request.Headers.Contains("ApiKey"))
                {
                    throw new Exception("Access denied, unauthorized");
                }

                string ApiKey = Request.Headers.GetValues("ApiKey").FirstOrDefault();

                LogID = api.Log(LogID, ApiKey, "UpdateSubject", Activity, JsonConvert.SerializeObject(sm), null);

                if (string.IsNullOrEmpty(ApiKey))
                {
                    throw new Exception("Access denied, unauthorized");
                }

                if ((UserAccess = api.CheckUserAccess(ApiKey, "SubjectWrite")) == "NO")
                {
                    throw new Exception("Access denied, unauthorized");
                }

                if (string.IsNullOrEmpty(sm.ID.ToString()))
                {
                    throw new Exception("Subject ID required");
                }

                if (string.IsNullOrEmpty(sm.Name))
                {
                    throw new Exception("Subject title required");
                }

                var a = (from x in db.Subjects where x.ID == sm.ID select x).FirstOrDefault();

                if (a != null)
                {
                    a.Name = sm.Name;
                    a.Description = sm.Description;
                    a.Active = sm.Active;
                    a.ModifiedBy = ApiKey;
                    a.DateModified = DateTime.Now;

                    db.SaveChanges();

                    gm.Status = "Success";
                    gm.Data = a.Name + " has been updated";
                }
                else
                {
                    throw new Exception("Subject not found");
                }

            }
            catch (Exception ex)
            {
                string ErrorData = ex.Message + "\n" + ex.InnerException + "\n" + ex.StackTrace;
                api.Log(LogID, "", "", ActivityError, "", ErrorData);

                gm.Status = "Failed";
                gm.Data = ex.Message;
            }

            return gm;
        }

        [HttpPost]
        [Route("School/Subject/Activate")]
        public object ActivateSubject(int ID)
        {
            var gm = new GenericModel();
            string UserAccess;
            int LogID = 0;
            string Activity = "You activated a school subject on " + DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss");
            string ActivityError = "You failed to activate a school subject on " + DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss");
            try
            {

                if (!Request.Headers.Contains("ApiKey"))
                {
                    throw new Exception("Access denied, unauthorized");
                }

                string ApiKey = Request.Headers.GetValues("ApiKey").FirstOrDefault();

                LogID = api.Log(LogID, ApiKey, "ActivateSubject", Activity, JsonConvert.SerializeObject(ID), null);

                if (string.IsNullOrEmpty(ApiKey))
                {
                    throw new Exception("Access denied, unauthorized");
                }

                if ((UserAccess = api.CheckUserAccess(ApiKey, "SubjectWrite")) == "NO")
                {
                    throw new Exception("Access denied, unauthorized");
                }

                //validation
                if (string.IsNullOrEmpty(ID.ToString()))
                {
                    throw new Exception("Subject ID required");
                }

                var a = (from x in db.Subjects where x.ID == ID select x).FirstOrDefault();

                if (a != null)
                {

                    a.Active = true;
                    a.ModifiedBy = ApiKey;
                    a.DateModified = DateTime.Now;

                    db.SaveChanges();

                    gm.Status = "Success";
                    gm.Data = a.Name + " has been activated";
                }
                else
                {
                    throw new Exception("Subject not found");
                }

            }
            catch (Exception ex)
            {
                string ErrorData = ex.Message + "\n" + ex.InnerException + "\n" + ex.StackTrace;
                api.Log(LogID, "", "", ActivityError, "", ErrorData);

                gm.Status = "Failed";
                gm.Data = ex.Message;
            }

            return gm;
        }

        [HttpPost]
        [Route("School/Subject/Deactivate")]
        public object DeactivateSubject(int ID)
        {
            var gm = new GenericModel();
            string UserAccess;
            int LogID = 0;
            string Activity = "You deactivated a school subject on " + DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss");
            string ActivityError = "You failed to deactivate a school subject on " + DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss");
            try
            {

                if (!Request.Headers.Contains("ApiKey"))
                {
                    throw new Exception("Access denied, unauthorized");
                }

                string ApiKey = Request.Headers.GetValues("ApiKey").FirstOrDefault();

                LogID = api.Log(LogID, ApiKey, "DeactivateSubject", Activity, JsonConvert.SerializeObject(ID), null);

                if (string.IsNullOrEmpty(ApiKey))
                {
                    throw new Exception("Access denied, unauthorized");
                }

                if ((UserAccess = api.CheckUserAccess(ApiKey, "SubjectWrite")) == "NO")
                {
                    throw new Exception("Access denied, unauthorized");
                }

                //validation
                if (string.IsNullOrEmpty(ID.ToString()))
                {
                    throw new Exception("Subject ID required");
                }

                var a = (from x in db.Subjects where x.ID == ID select x).FirstOrDefault();

                if (a != null)
                {

                    a.Active = false;
                    a.ModifiedBy = ApiKey;
                    a.DateModified = DateTime.Now;

                    db.SaveChanges();

                    gm.Status = "Success";
                    gm.Data = a.Name + " has been deactivated";
                }
                else
                {
                    throw new Exception("Subject not found");
                }

            }
            catch (Exception ex)
            {
                string ErrorData = ex.Message + "\n" + ex.InnerException + "\n" + ex.StackTrace;
                api.Log(LogID, "", "", ActivityError, "", ErrorData);

                gm.Status = "Failed";
                gm.Data = ex.Message;
            }

            return gm;
        }

        [HttpPost]
        [Route("School/Subject")]
        public object GetSubject(int ID)
        {
            var gm = new GenericModel();
            string UserAccess;
            int LogID = 0;
            string Activity = "You request for school subject's information on " + DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss");
            string ActivityError = "You failed to request for a school's subject information on " + DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss");
            try
            {

                if (!Request.Headers.Contains("ApiKey"))
                {
                    throw new Exception("Access denied, unauthorized");
                }

                string ApiKey = Request.Headers.GetValues("ApiKey").FirstOrDefault();

                LogID = api.Log(LogID, ApiKey, "GetSubject", Activity, JsonConvert.SerializeObject(ID), null);

                if (string.IsNullOrEmpty(ApiKey))
                {
                    throw new Exception("Access denied, unauthorized");
                }

                if ((UserAccess = api.CheckUserAccess(ApiKey, "SubjectRead")) == "NO")
                {
                    throw new Exception("Access denied, unauthorized");
                }

                //validation
                if (string.IsNullOrEmpty(ID.ToString()))
                {
                    throw new Exception("Subject ID required");
                }


                var a = (from x in db.Subjects
                         where x.ID == ID
                         join y in db.TeachersAndSubjects on x.ID equals y.ID
                         join z in db.Users on y.Teacher equals z.UserID
                         join b in db.Users on x.CreatedBy equals b.CreatedBy
                         join c in db.Users on x.ModifiedBy equals c.ModifiedBy
                         select new SubjectModel
                         {
                             Status = "Success",
                             Name = x.Name,
                             Description = x.Description,
                             Active = x.Active,
                             CreatedBy = b.Email,
                             ModifiedBy = c.Email,
                             DateCreated = x.DateCreated,
                             DateModified = x.DateModified,

                         }).FirstOrDefault();

                if (a != null)
                {
                    List<TeachersList> teachers = new List<TeachersList>();

                    var b = (from f in db.TeachersAndSubjects where f.Subject == ID select f);
                    return a;
                }
                else
                {
                    throw new Exception("Subject not found");
                }

            }
            catch (Exception ex)
            {
                string ErrorData = ex.Message + "\n" + ex.InnerException + "\n" + ex.StackTrace;
                api.Log(LogID, "", "", ActivityError, "", ErrorData);

                gm.Status = "Failed";
                gm.Data = ex.Message;

                return gm;

            }

        }
        #endregion
    }
}
