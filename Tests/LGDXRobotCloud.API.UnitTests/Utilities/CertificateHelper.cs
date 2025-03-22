using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace LGDXRobotCloud.API.UnitTests.Utilities;

public static class CertificateHelper
{
  public static X509Certificate2 CreateSelfSignedCertificate(string subjectName = "LGDXRobotTest", int keySize = 2048, int validYears = 1)
  {
    using var rsa = RSA.Create(keySize);

    var certificateRequest = new CertificateRequest(
        new X500DistinguishedName($"CN={subjectName}"),
        rsa,
        HashAlgorithmName.SHA256,
        RSASignaturePadding.Pkcs1);

    var notBefore = DateTimeOffset.UtcNow;
    var notAfter = notBefore.AddYears(validYears);

    var basicConstraints = new X509BasicConstraintsExtension(true, false, 0, true);
      certificateRequest.CertificateExtensions.Add(basicConstraints);

    var cert = certificateRequest.CreateSelfSigned(notBefore, notAfter);

    return new X509Certificate2(cert);
  }
}