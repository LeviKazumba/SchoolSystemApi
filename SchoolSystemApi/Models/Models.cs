using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SchoolSystemApi.Models
{

    #region Accounts
    public class UserLogin
    {
        public string Email { get; set; }
        public string Password { get; set; }

    }

    public class UserLoginResponse
    {
        public string Status { get; set; }

        public string Email { get; set; }
        public string UserID { get; set; }
        public string School_ID { get; set; }


    }

    public class GenericModel
    {
        public string Status { get; set; }
        public string Data { get; set; }

    }

    public class ForgotPassword
    {
        public string Email { get; set; }
    }

    public class UpdatePassword
    {
        public string UserID { get; set; }
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }


    }

    #endregion

    #region Admin

    public class GetSchool
    {

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string School_ID { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string SchoolName { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string SchoolType { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Email { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Telephone { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Address { get; set; }

        //[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        //public string Logo { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool Active { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string CompanyRegistrationNumber { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTime DateCreated { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTime DateModified { get; set; }

    }

    public class NewSchool
    {
        public HttpPostedFile Logo { get; set; }
        public string SchoolName { get; set; }
        public string CountryName { get; set; }
        public string SchoolTelephone { get; set; }
        public string SchoolEmail { get; set; }
        public string SchoolType { get; set; }
        public string CompanyRegistrationNumber { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string UserEmail { get; set; }
        public string UserTelephone { get; set; }
        public string Address { get; set; }
    }
    #endregion

    #region Portal

    public class GetUser
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Status { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Data { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string UserID { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string School_ID { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string SchoolName { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Logo { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Surname { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Email { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string UserType { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? Department_ID { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string DepartmentName { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? HOD { get; set; }
    }


    public class GetDashboard
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Status { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Data { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? NumberOfSchools { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? NumberOfUsers { get; set; }
    }

    public class Teachers
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Status { get; set; }

    }
    #endregion
}

