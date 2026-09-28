using System;
using System.IO;
using System.Security.Cryptography;

namespace GK2RustyToolDisposal.Helpers;

internal static class GameBuildHelper
{
    // Exact installed build investigated on 2026-09-28. Re-audit before updating these.
    internal static bool IsVerifiedBuild(string gameRoot)
    {
        return Matches(
                Path.Combine(gameRoot, @"GraveyardKeeper2_Data\Managed\Assembly-CSharp.dll"),
                "7ACB243A08897D8CC7B67EF17AED50EEA17857494AEF4278F4F3B00D324823E5"
            )
            && Matches(
                Path.Combine(gameRoot, @"GraveyardKeeper2_Data\resources.assets"),
                "FD64048DF834EC19DA725481A4CE703C79ABB98AE6808107182D82A78B87F1CC"
            )
            && Matches(
                Path.Combine(gameRoot, @"GraveyardKeeper2_Data\StreamingAssets\aa\catalog.bin"),
                "47426C3BECBF16E51C07042E415A570CC8A79991CBEECC6D20A3D4940CF54475"
            );
    }

    private static bool Matches(string path, string expected)
    {
        using (var stream = File.OpenRead(path))
        using (var sha = SHA256.Create())
            return string.Equals(
                BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", ""),
                expected,
                StringComparison.Ordinal
            );
    }
}
