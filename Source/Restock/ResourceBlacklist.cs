using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Restock
{
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    public class ResourceBlacklist : MonoBehaviour
    {
        private readonly string[] COMMENT_SEPARATOR = { "//" };
        private void Start()
        {
            HashSet<UrlDir.UrlFile> blacklist = new HashSet<UrlDir.UrlFile>();
            HashSet<UrlDir.UrlFile> whitelist = new HashSet<UrlDir.UrlFile>();

            UrlDir gameData = GameDatabase.Instance.root.children.Find(dir => dir.type == UrlDir.DirectoryType.GameData);

            foreach (UrlDir.UrlFile file in GameDatabase.Instance.root.AllFiles)
            {
                if (file.fileExtension == "restockblacklist")
                {
                    Debug.Log($"[Restock] Reading blacklist {file.url}.{file.fileExtension}");
                    foreach (UrlDir.UrlFile blacklistFile in FindFilesInAssetListFile(file.fullPath, gameData))
                    {
                        blacklist.Add(blacklistFile);
                    }
                }
                else if (file.fileExtension == "restockwhitelist")
                {
                    Debug.Log($"[Restock] Reading whitelist {file.url}.{file.fileExtension}");
                    foreach (UrlDir.UrlFile whitelistFile in FindFilesInAssetListFile(file.fullPath, gameData))
                    {
                        whitelist.Add(whitelistFile);
                    }
                }
            }

            Debug.Log("[Restock] Removing blacklisted assets");
            foreach (UrlDir.UrlFile file in blacklist)
            {
                if (whitelist.Contains(file)) continue;

                Debug.Log($"[Restock] Removing {file.url}.{file.fileExtension}");
                UrlDir.UrlFile newFile2 = new UrlDir.UrlFile(file.parent, new FileInfo(file.fullPath + ".disabled"));
                file.parent.files[file.parent.files.IndexOf(file)] = newFile2;
            }

            Destroy(gameObject);
        }

        private IEnumerable<UrlDir.UrlFile> FindFilesInAssetListFile(string filePath, UrlDir dir)
        {
            foreach (string line in File.ReadAllLines(filePath))
            {
                string lineBeforeComment = line.Split(COMMENT_SEPARATOR, 2, StringSplitOptions.None)[0].Trim();
                if (lineBeforeComment == string.Empty) continue;

                bool matchedFile = false;
                foreach (UrlDir.UrlFile urlFile in FindFilesForUrl(lineBeforeComment, dir))
                {
                    yield return urlFile;
                    matchedFile = true;
                }

                if (!matchedFile)
                    Debug.LogError($"[Restock] No files found matching url {lineBeforeComment}");
            }
        }

        private readonly char[] PATH_SEPARATOR = new[] { '/' };
        private IEnumerable<UrlDir.UrlFile> FindFilesForUrl(string url, UrlDir dir)
        {
            string[] splits = url.Split(PATH_SEPARATOR, 2);

            if (splits.Length == 1)
            {
                if (splits[0] == string.Empty)
                {
                    foreach (UrlDir.UrlFile file in dir.files)
                    {
                        if (file.fileType == UrlDir.FileType.Config) continue;
                        yield return file;
                    }

                    // Already excludes configs
                    foreach (UrlDir.UrlFile file in dir.AllFiles)
                    {
                        yield return file;
                    }
                }
                else
                {
                    int idx = splits[0].LastIndexOf('.');
                    string fileName;
                    string fileExtension;

                    if (idx != -1)
                    {
                        fileName = splits[0].Substring(0, idx);
                        fileExtension = splits[0].Substring(idx + 1);
                    }
                    else
                    {
                        fileName = splits[0];
                        fileExtension = null;
                    }

                    string pattern = '^' + Regex.Escape(fileName).Replace(@"\*", ".*") + '$';
                    Regex regex = new Regex(pattern);

                    foreach (UrlDir.UrlFile file in dir.files)
                    {
                        if (file.fileType == UrlDir.FileType.Config) continue;
                        if (!regex.IsMatch(file.name)) continue;
                        if (fileExtension != null && fileExtension != file.fileExtension) continue;
                        yield return file;
                    }
                }
            }
            else if (splits.Length == 2)
            {
                string pattern = '^' + Regex.Escape(splits[0]).Replace(@"\*", ".*") + '$';
                Regex regex = new Regex(pattern);

                foreach (UrlDir subDir in dir.children)
                {
                    if (regex.IsMatch(subDir.name))
                    {
                        foreach (UrlDir.UrlFile file in FindFilesForUrl(splits[1], subDir))
                        {
                            yield return file;
                        }
                    }
                }
            }
            else
            {
                throw new NotImplementedException("This code should never be reached");
            }
        }
    }
}
