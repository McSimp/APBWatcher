using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APBClient
{
    class LoginStatuses
    {
        Dictionary<int, string> statuses = new Dictionary<int, string>() {
            { 11002, "LoginFailedInvalidStatus" },
            { 11006, "LoginFailedAgeRestricted" },
            { 10034, "LoginServerConnectCountryBlocked" },
            { 10005, "LoginFailedLoginInProgress" },
            { 10007, "LoginFailedAccountTypeBlocked" },
            { 10008, "LoginFailedAccountBlocked" },
            { 11001, "LoginFailedAccountOrPassword" },
            { 10001, "LoginFailedVersionMismatch" },
            { 8, "LoginServerConnectFailed" },
            { 5, "DatabaseBusy" },
            { -2, "LoginFailedAccountInUse" },
        };
    }
}
