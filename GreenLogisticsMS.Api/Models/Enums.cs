// ==========================================================================
//  Green Logistics Management System (GLMS) - Part 3 SOA  |  PROG7311
//  File: GreenLogisticsMS.Api/Models/Enums.cs
//  ----------------------------------------------------------------------
//  References (IIE Harvard):
//   - Microsoft. 2024. Enumeration types (C# reference). [Online]. Available at:
//     https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/enum [Accessed 31 May 2026].
//   - Albahari, J. 2022. C# 10 in a Nutshell: The Definitive Reference. Sebastopol, CA: O'Reilly Media.
// ==========================================================================

namespace GreenLogisticsMS.Api.Models
{
    public enum ContractStatus
    {
        Draft,
        Active,
        Expired,
        OnHold
    }

    public enum ServiceLevel
    {
        Standard,
        Priority,
        Premium
    }

    public enum ServiceRequestStatus
    {
        Pending,
        InProgress,
        Completed,
        Cancelled
    }

    public enum RequestType
    {
        Domestic,
        International
    }
}
