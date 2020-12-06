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
using System.Xml.XPath;

namespace SelfUpdater
{
    class SelfUpdater
    {
        const string MagicArg = "autoUpdateMagic";
        const string CertificateIssuerOU = "OU=Microsoft IT, O=Microsoft Corporation, L=Redmond, S=Washington, C=US";
        const string CertificateSubject = "CN=*.blob.core.windows.net";

        private static readonly HttpClientHandler httpClientHandler = new();
        private static readonly HttpClient client = new(httpClientHandler);

        record VersionAvailable
        {
            internal string Name;
            internal Uri Url;
            internal SemVersion Version;
        }

        static internal int CheckForNewerVersion()
        {
            client.DefaultRequestHeaders.Add("User-Agent", "SelfUpdater sample");
            // Return `true` to allow certificates that are untrusted/invalid
            httpClientHandler.ServerCertificateCustomValidationCallback = (message, certificate, chain, errors) =>
            {
                //skip the CA part
                string ou = certificate.Issuer.Substring(certificate.Issuer.IndexOf("OU="));
                // paranoid checks
                return ou == CertificateIssuerOU
                    && certificate.Subject == CertificateSubject;
            };

            bool newVer = DownloadAndUnpackNewVersionIfAvailable(out string unpackDirectory);
            if (newVer)
            {
                var self = Process.GetCurrentProcess();
                string selfExeFullPath = self.MainModule.FileName;
                string selfDirectory = Path.GetDirectoryName(selfExeFullPath);
                string updaterExeFullPath = Path.Combine(unpackDirectory, Path.GetFileName(selfExeFullPath));
                Process.Start(updaterExeFullPath, new string[] { MagicArg, self.Id.ToString(), unpackDirectory, selfDirectory });
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

            if (latestInfo.Version <= currentVersion)
            {
                return false;
            }

            DownloadAndExtractPackage(latestInfo, unpackDirectory);

            return true;
        }

        private static void DownloadAndExtractPackage(VersionAvailable latestInfo, string unpackDirectory)
        {
            using var response = client.GetAsync(latestInfo.Url).Result;
            if (response.IsSuccessStatusCode)
            {
                using var responseStream = response.Content.ReadAsStreamAsync().Result;
                using var zip = new ZipArchive(responseStream, ZipArchiveMode.Read);
                zip.ExtractToDirectory(unpackDirectory);
            }
        }

        private static VersionAvailable GetLatestVersion()
        {
            var versionUrls = GetVersionsAvailable();
            var latest = versionUrls.OrderBy(x => x.Version).First();
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

                result.Add(new VersionAvailable {
                    Name = name,
                    Url = new Uri(url),
                    Version = ExtractVersion(name)
                });
            }
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

        static internal int SelfUpdateIfRequired(string[] args)
        {
            if (args.Length < 3 || args[0] != MagicArg)
            {
                // I am not running in self-update mode
                return 0;//success
            }

            int parentProcessId = int.Parse(args[1]);
            string unpackedDirectory = args[2];
            string parentDirectory = args[3];

            var parentProcess = Process.GetProcessById(parentProcessId);
            if (!parentProcess.WaitForExit(30000))
            {
                // log a warning here
                parentProcess.Kill(false);
            }

            CopyFilesRecursively(new DirectoryInfo( unpackedDirectory), new DirectoryInfo(parentDirectory));

            var self = Process.GetCurrentProcess();
            string selfExeFullPath = self.MainModule.FileName;
            string newVersionExeFullPath = Path.Combine(parentDirectory, Path.GetFileName(selfExeFullPath));
            Process.Start(newVersionExeFullPath);
            // quick exit
            Environment.Exit(0);
            return 0;
        }

        static void CopyFilesRecursively(DirectoryInfo source, DirectoryInfo target)
        {
            foreach (DirectoryInfo dir in source.GetDirectories())
                CopyFilesRecursively(dir, target.CreateSubdirectory(dir.Name));
            foreach (FileInfo file in source.GetFiles())
                file.CopyTo(Path.Combine(target.FullName, file.Name), true);
        }
    }
}
