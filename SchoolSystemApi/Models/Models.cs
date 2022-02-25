using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SchoolSystemApi.Models
{

    #region Account
    public class Login
    {
        public string Email { get; set; }
        public string Password { get; set; }

    }

    public class GenericLog
    {
        public string ApiKey { get; set; }
        public string UserID { get; set; }
        public string School { get; set; }

    }

    public class LoginResponse
    {
        public string Status { get; set; }
        public string Email { get; set; }
        public string UserID { get; set; }
        public string School { get; set; }

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

    #region School

    public class SchoolModel
    {
        public string Status { get; set; }
        public string SchoolID { get; set; }
        public string SchoolName { get; set; }
        public int SchoolType { get; set; }
        public string SchoolTypeName { get; set; }
        public string Email { get; set; }
        public string Telephone { get; set; }
        public string Country { get; set; }
        public string PhoneCode { get; set; }
        public string UNCode { get; set; }
        public string Address { get; set; }
        public string Logo { get; set; }
        public bool? Active { get; set; }
        public string CompanyRegistrationNumber { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public HttpPostedFile SchoolLogo { get; set; }
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

    public partial class CountryModel
    {
        public string Status { get; set; }
        public int ID { get; set; }
        public string CountryID { get; set; }
        public string Name { get; set; }
        public string Iso2 { get; set; }
        public string Iso3 { get; set; }
        public string PhoneCode { get; set; }
        public string UNCode { get; set; }
        public bool Active { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public System.DateTime DateCreated { get; set; }
        public Nullable<System.DateTime> DateModified { get; set; }
    }

    public partial class AttendanceRegisterModel
    {
        public int ID { get; set; }
        public string School { get; set; }
        public string Class { get; set; }
        public string SchoolStaff { get; set; }
        public string Student { get; set; }
        public Nullable<System.DateTime> ClockIn { get; set; }
        public Nullable<System.DateTime> ClockOut { get; set; }
        public Nullable<System.DateTime> LunchStart { get; set; }
        public Nullable<System.DateTime> LunchEnd { get; set; }
        public Nullable<bool> Present { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public Nullable<System.DateTime> DateCreated { get; set; }
        public Nullable<System.DateTime> DateModified { get; set; }
    }

    public partial class SubjectModel
    {
        public string Status { get; set; }
        public int ID { get; set; }
        public string School { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Active { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public Nullable<System.DateTime> DateCreated { get; set; }
        public Nullable<System.DateTime> DateModified { get; set; }
        public List<TeachersList> TeachersList { get; set; }
        public List<SubTeachersList> SubTeachersList { get; set; }

    }

    public class TeachersList
    {
        public string Name { get; set; }
        public string Email { get; set; }
    }

    public class SubTeachersList
    {
        public string Name { get; set; }
        public string Email { get; set; }
    }

    public partial class ClassModel
    {
        public string Status { get; set; }
        public int ID { get; set; }
        public string School { get; set; }
        public string Name { get; set; }
        public string Teacher { get; set; }
        public string SubTeacher { get; set; }
        public string Description { get; set; }
        public bool Active { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public Nullable<System.DateTime> DateCreated { get; set; }
        public Nullable<System.DateTime> DateModified { get; set; }
    }

    #endregion


    #region Users
    public class UserModel
    {
        public int ID { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Status { get; set; }
        public string UserID { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public string Telephone { get; set; }
        public string CountryName { get; set; }
        public string UNCode { get; set; }
        public string PhoneCode { get; set; }
        public System.DateTime Age { get; set; }
        public string Gender { get; set; }
        public int UserType { get; set; }
        public string UserTypeName { get; set; }
        public int UserRole { get; set; }
        public string UserRoleName { get; set; }
        public DateTime LastLogin { get; set; }
        public bool LockedOut { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public bool Active { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateModified { get; set; }
        public string SchoolID { get; set; }
        public string SchoolName { get; set; }
        public string SchoolEmail { get; set; }
        public string SchoolTelephone { get; set; }
        public string SchoolAddress { get; set; }
        public string SchoolLogo { get; set; }
        public int SchoolTypeID { get; set; }
        public string SchoolTypeName { get; set; }
        public List<Functions> Functions { get; set; }

    }

    public class Functions
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Active { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public System.DateTime DateCreated { get; set; }
        public Nullable<System.DateTime> DateModified { get; set; }
    }

    public partial class UserTypeModel
    {
        public string Status { get; set; }
        public int ID { get; set; }
        public string Name { get; set; }
        public bool Active { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public System.DateTime DateCreated { get; set; }
        public Nullable<System.DateTime> DateModified { get; set; }
    }

    public partial class UserRoleModel
    {
        public string Status { get; set; }
        public int ID { get; set; }
        public string Name { get; set; }
        public bool Active { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public System.DateTime DateCreated { get; set; }
        public Nullable<System.DateTime> DateModified { get; set; }
    }

    #endregion
}