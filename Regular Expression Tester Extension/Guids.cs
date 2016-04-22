// Guids.cs
// MUST match guids.h
using System;

namespace AndreasAndersen.Regular_Expression_Tester_Extension
{
    static class GuidList
    {
        public const string guidRegular_Expression_Tester_ExtensionPkgString = "a65d58d2-ead8-4eea-a47d-fa60865a6043";
        public const string guidRegular_Expression_Tester_ExtensionCmdSetString = "8078f68d-a94a-4c01-94e3-ac99160f0cb8";
        public const string guidToolWindowPersistanceString = "0d4883f9-71e4-491e-bd2b-02d53cb82a92";

        public static readonly Guid guidRegular_Expression_Tester_ExtensionCmdSet = new Guid(guidRegular_Expression_Tester_ExtensionCmdSetString);
    };
}