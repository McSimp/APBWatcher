using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APBClient.Networking
{
    internal enum APBOpCode : uint
    {
        GC2LS_ASK_LOGIN = 1000,
        GC2LS_LOGIN_PROOF = 1005,
        GC2LS_ASK_WORLD_LIST = 1007,
        GC2LS_ASK_WORLD_ENTER = 1012,
        GC2LS_KEY_EXCHANGE = 1016,
        GC2LS_HARDWARE_INFO = 1017,

        LS2GC_ERROR = 2000,
        LS2GC_KICK = 2001,
        LS2GC_LOGIN_PUZZLE = 2002,
        LS2GC_LOGIN_SALT = 2003,
        LS2GC_ANS_LOGIN_SUCCESS = 2004,
        LS2GC_ANS_LOGIN_FAILED = 2005,
        LS2GC_CHARACTER_LIST = 2006,
        LS2GC_ANS_WORLD_LIST = 2008,
        LS2GC_ANS_WORLD_ENTER = 2013,
        LS2GC_WMI_REQUEST = 2021,

        GC2WS_ASK_WORLD_ENTER = 3000,
        GC2WS_ASK_INSTANCE_LIST = 3002,
        GC2WS_ASK_DISTRICT_RESERVE = 3003,
        GC2WS_ASK_DISTRICT_ENTER = 3005,
        WS2GC_ANS_WORLD_ENTER = 4003,
        WS2GC_DISTRICT_LIST = 4006,
        WS2GC_ANS_INSTANCE_LIST = 4007,
        WS2GC_ANS_DISTRICT_RESERVE = 4008,
        WS2GC_ANS_DISTRICT_ENTER = 4010,
    }
}
