using System.Security.Cryptography;
using System.Text;
using LGDXRobot2Cloud.Utilities.Enums;

namespace LGDXRobot2Cloud.Utilities.Helpers;

public static class LgdxHelper
{
  public readonly static List<int> AutoTaskStaticStates = [(int)ProgressState.Template, (int)ProgressState.Waiting, (int)ProgressState.Completed, (int)ProgressState.Aborted];

  public static string GenerateSha256Hash(string input)
  {
    var inputBytes = Encoding.UTF8.GetBytes(input);
    var inputHash = SHA256.HashData(inputBytes);
    return Convert.ToHexString(inputHash);
  }

  public static string GenerateMd5Hash(string input)
  {
    var inputBytes = Encoding.UTF8.GetBytes(input);
    var inputHash = MD5.HashData(inputBytes);
    return Convert.ToHexString(inputHash);
  }
}
