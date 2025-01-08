using System.Security.Cryptography;
using System.Text;
using LGDXRobot2Cloud.Utilities.Enums;

namespace LGDXRobot2Cloud.Utilities.Helpers;

public static class LgdxHelper
{
  public readonly static List<int> AutoTaskRunningStateList = [(int)ProgressState.Template, (int)ProgressState.Waiting, (int)ProgressState.Completed, (int)ProgressState.Aborted];

  public static string GenerateSha256Hash(string input)
  {
    var inputBytes = Encoding.UTF8.GetBytes(input);
    var inputHash = SHA256.HashData(inputBytes);
    return Convert.ToHexString(inputHash);
  }
}
