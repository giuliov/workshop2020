using Semver;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security;
using System.Security.Cryptography;
using System.Xml.XPath;

namespace SelfUpdater
{
    class SelfUpdater
    {
        const string MagicArg = "autoUpdateMagic";
        private static readonly string[] CertificateIssuer = new string[] {
            "OU=Microsoft IT, O=Microsoft Corporation, L=Redmond, S=Washington, C=US",
            "O=Microsoft Corporation, C=US"
        };
        const string CertificateSubject = "CN=*.blob.core.windows.net";

        private static readonly HttpClientHandler httpClientHandler = new();
        private static readonly HttpClient client = new(httpClientHandler);

        record VersionAvailable
        {
            internal string Name;
            internal Uri Url;
            internal SemVersion Version;
        }

        public static Action<string> Log = (message) => { /*noop*/ };

        static internal string[] AutoUpdate(string[] args)
        {
            if (args.Length > 0 && args[0] == MagicArg)
            {
                return SelfUpdateIfRequired(args);
            } else
            {
                CheckForNewerVersion(args);
                return args;
            }
        }

        static private int CheckForNewerVersion(string[] args)
        {
            Log("Check for newer version");

            client.DefaultRequestHeaders.Add("User-Agent", "SelfUpdater sample");
            // Return `true` to allow certificates that are untrusted/invalid
            httpClientHandler.ServerCertificateCustomValidationCallback = (message, certificate, chain, errors) =>
            {
                return CertificateIssuer.Any(goodIssuer => {
                    string matchAt = goodIssuer.Substring(0, goodIssuer.IndexOf('=') + 1);
                    //skip the CA part
                    int pos = certificate.Issuer.IndexOf(matchAt);
                    if (pos < 0) return false;
                    string toMatch = certificate.Issuer.Substring(certificate.Issuer.IndexOf(matchAt));
                    // paranoid checks
                    return toMatch == goodIssuer
                        && certificate.Subject == CertificateSubject;
                });                    
            };

            bool newVer = DownloadAndUnpackNewVersionIfAvailable(out string unpackDirectory);
            if (newVer)
            {
                Log("Newer version found, launching in special mode");

                var self = Process.GetCurrentProcess();
                string selfExeFullPath = self.MainModule.FileName;
                string selfDirectory = Path.GetDirectoryName(selfExeFullPath);
                string updaterExeFullPath = Path.Combine(unpackDirectory, Path.GetFileName(selfExeFullPath));
                // 4 arguments for ourself
                var updateArgs = new string[] { MagicArg, self.Id.ToString(), unpackDirectory, selfDirectory };
                // plus any user argument
                var allArgs = updateArgs.Concat(args);
                Process.Start(updaterExeFullPath, allArgs);
                // quick exit
                Environment.Exit(42);
            }

            return 0;//success
        }

        private static bool DownloadAndUnpackNewVersionIfAvailable(out string unpackDirectory)
        {
            unpackDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            var exeAssembly = Assembly.GetEntryAssembly();
            var currentInfo = exeAssembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            var currentVersion = SemVersion.Parse(currentInfo.InformationalVersion);

            var latestInfo = GetLatestVersion();

            Log($"Current: {currentVersion}, Latest: {latestInfo.Version}");

            if (latestInfo.Version <= currentVersion)
            {
                Log("You have the most recent version");

                return false;
            }

            DownloadAndExtractPackage(latestInfo, unpackDirectory);

            return true;
        }

        private static void DownloadAndExtractPackage(VersionAvailable latestInfo, string unpackDirectory)
        {
            Log($"Downloading version {latestInfo.Version}");

            using var response = client.GetAsync(latestInfo.Url).Result;
            if (response.IsSuccessStatusCode)
            {
                var hashAlg = new SHA256Managed();
                using var responseStream = response.Content.ReadAsStreamAsync().Result;
                using var cryptoStream = new CryptoStream(responseStream, hashAlg, CryptoStreamMode.Read);
                using var zip = new ZipArchive(cryptoStream, ZipArchiveMode.Read);
                zip.ExtractToDirectory(unpackDirectory);

                Log("Checking hash code of downloaded package");

                string hash = BitConverter.ToString(hashAlg.Hash).Replace("-", "");
                var hashUrl = latestInfo.Url.AbsoluteUri.Replace(".zip","-sha256.txt");
                using var hashResponse = client.GetAsync(hashUrl).Result;
                if (hashResponse.IsSuccessStatusCode)
                {
                    string expectedHash = hashResponse.Content.ReadAsStringAsync().Result.Split("  ")[0];
                    if (hash != expectedHash)
                    {
                        throw new ApplicationException("Hash does not match, aborting!");
                    }
                    Log("Hashes match");
                }
            }
        }

        private static VersionAvailable GetLatestVersion()
        {
            var versionUrls = GetVersionsAvailable();
            var latest = versionUrls.OrderBy(x => x.Version).Last();
            return latest;
        }

        private static IList<VersionAvailable> GetVersionsAvailable()
        {
            var result = new List<VersionAvailable>();
            // Azure Blob specific
            using var stream = client.GetStreamAsync(BuildConstants.QueryURL).Result;
            var doc = new XPathDocument(stream);
            var navigator = doc.CreateNavigator();
            var query = navigator.Compile("/EnumerationResults/Blobs/Blob");
            var iterator = navigator.Select(query);
            while (iterator.MoveNext())
            {
                iterator.Current?.MoveToChild("Name", "");
                string name = iterator.Current?.Value;
                iterator.Current.MoveToFollowing("Url", "");
                string url = iterator.Current?.Value;

                if (Path.GetExtension(name) == ".zip")
                {
                    result.Add(new VersionAvailable
                    {
                        Name = name,
                        Url = new Uri(url),
                        Version = ExtractVersion(name)
                    });
                }
            }
            Log($"Found {result.Count} version(s)");
            return result;

            // HACK
            SemVersion ExtractVersion(string filename)
            {
                var version = new SemVersion(0);
                string temp = Path.GetFileNameWithoutExtension(filename);
                if (SemVersion.TryParse(temp.Substring(1 + temp.IndexOf('-')), out version))
                    return version;
                if (SemVersion.TryParse(temp.Substring(1 + temp.LastIndexOf('-')), out version))
                    return version;
                return version;
            }
        }

        static private string[] SelfUpdateIfRequired(string[] args)
        {
            if (args.Length < 3 || args[0] != MagicArg)
            {
                // I am not running in self-update mode
                Log("No self update");
                return args;
            }

            int parentProcessId = int.Parse(args[1]);
            string unpackedDirectory = args[2];
            string parentDirectory = args[3];
            var originalArgs = args.Skip(4);

            Log("Self-update initiated");

            try
            {
                Log($"Making sure parent process isn't running anymore");
                var parentProcess = Process.GetProcessById(parentProcessId);
                if (parentProcess != null && !parentProcess.WaitForExit(30000))
                {
                    Log($"Killing existing instance {parentProcessId}");
                    // log a warning here
                    parentProcess.Kill(false);
                }
            }
            catch (ArgumentException ex)
            {
                //silent
            }

            Log($"Copying files from temporary location");
            CopyFilesRecursively(new DirectoryInfo(unpackedDirectory), new DirectoryInfo(parentDirectory));

            Log($"Starting newer version");
            var self = Process.GetCurrentProcess();
            string selfExeFullPath = Assembly.GetExecutingAssembly().Location; // works on Linux & Windows
            string newVersionExeFullPath = Path.Combine(parentDirectory, Path.GetFileName(selfExeFullPath));
            Process.Start(newVersionExeFullPath, originalArgs);
            Log($"Exiting Self-update mode");
            // quick exit
            Environment.Exit(0);
            // never executed
            return null;
        }

        static private void CopyFilesRecursively(DirectoryInfo source, DirectoryInfo target)
        {
            foreach (DirectoryInfo dir in source.GetDirectories())
                CopyFilesRecursively(dir, target.CreateSubdirectory(dir.Name));
            foreach (FileInfo file in source.GetFiles())
                file.CopyTo(Path.Combine(target.FullName, file.Name), true);
        }
    }
}
