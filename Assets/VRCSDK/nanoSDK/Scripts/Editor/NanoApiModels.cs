using System.Collections.Generic;

namespace nanoSDK
{
    public class NanoUserData
    {
        public string ID { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public Permissions Permission { get; set; }
        public bool IsVerified { get; set; }
        public bool IsPremium { get; set; }
    }

    public class SdkVersionOutput<T>
    {
        public string Message { get; set; }
        public T Data { get; set; }
    }
    public class SdkVersionData
    {
        public string Url { get; set; }
        public string Version { get; set; }
        public ReleaseType Type { get; set; }

        public BranchType Branch { get; set; }

        public enum ReleaseType
        {
            Avatar = 0,
            World = 1
        }

        public enum BranchType
        {
            Release = 0,
            Beta = 1,
            PrivateBeta = 2
        }
    }

    public enum Permissions
    {
        User = 0,
        Moderator = 5,
        Admin = 10,
        System = 127
    }
    
    public class APIRegisterData
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
    }

    public class APILoginData
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class BaseResponse<T>
    {
        public string Message { get; set; }
        public T Data { get; set; }
    }

    public class SanityCheckResponse
    {
        public Dictionary<string, string> UsernameSanityCheck { get; set; }
        public Dictionary<string, string> PasswordSanityCheck { get; set; }
        public Dictionary<string, string> EmailSanityCheck { get; set; }
    }

    public class LoginResponse
    {
        public string AuthKey { get; set; }
    }

    public class LicenseData
    {
        public string Key { get; set; }
    }
}