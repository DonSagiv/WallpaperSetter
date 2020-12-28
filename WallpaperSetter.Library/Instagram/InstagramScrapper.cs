﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Utilities;
using WallpaperSetter.Library.CustomExceptions;

namespace WallpaperSetter.Library.Instagram
{
    public class InstagramScrapper
    {
        private readonly HttpClient _client = new HttpClient();
        private readonly ILogger _logger;
        private readonly FileInfo _saveFile;
        private readonly string _tag;

        public InstagramScrapper(string tag)
        {
            _logger = new Logger(GetType(), LogOutput.Console);
            _tag = tag;
            _saveFile = new FileInfo(Path.Combine(Path.GetTempPath(), $"{tag}-imageUris.json"));
        }

        public async Task<IEnumerable<Uri>> GetImageUrisAsync()
        {
            var imageUris = await GetImageUrisFromInstagramAsync();
            //imageUris ??= await GetImagesFromFullInstaAsync();
            imageUris ??= await GetImagesFromPreviousResults();

            if (imageUris is null)
                throw new UnableToGetImageUrisException();

            return imageUris;
        }

        private async Task<IEnumerable<Uri>> GetImageUrisFromInstagramAsync()
        {
            var content = await _client.GetStringAsync(new Uri($"https://www.instagram.com/explore/tags/{_tag}/"));

            var aLinkRegex = new Regex("(<script type=\"text/javascript\">window._sharedData = ).*(</script>)");

            var parsedJson = aLinkRegex.Matches(content)[0].Value
                    .Replace("<script type=\"text/javascript\">window._sharedData = ", "")
                    .Replace(";</script>", "")
                    .Replace(@"\u0026", "&")
                ;

            var igResponse = JsonConvert.DeserializeObject<InstagramResponse>(parsedJson);

            var imageUris = igResponse.EntryData.TagPage[0].Graphql.Hashtag.EdgeHashtagToMedia.Edges
                .Select(edge => edge.Node.DisplayUrl).ToList();

            DumpImageUrisLocally(imageUris);

            _logger.Log("Populated images from Instagram");

            if (imageUris.Count > 0)
                return imageUris;
            
            return null;
        }

        private async Task<IEnumerable<Uri>> GetImagesFromFullInstaAsync()
        {
            var uris = new List<Uri>();

            var content = await _client.GetStringAsync(new Uri($"https://fullinsta.photo/hashtag/{_tag}/"));

            // TODO: Implement FullInsta

            var aUris = uris.ToArray();

            DumpImageUrisLocally(aUris);

            _logger.Log("Populated images from FullInsta");
            if (uris.Count > 0)
                return aUris;

            return null;
        }

        private async Task<IEnumerable<Uri>> GetImagesFromPreviousResults()
        {
            // Read text
            var text = await File.ReadAllTextAsync(_saveFile.FullName);

            // Deserialize
            var urisFromJson = JsonConvert.DeserializeObject<Uri[]>(text);

            _logger.Log("Populated images from previous results");

            if (urisFromJson.Length > 0)
                return urisFromJson;
            
            return null;
        }

        private void DumpImageUrisLocally(IEnumerable<Uri> imageUris)
        {
            var json = JsonConvert.SerializeObject(imageUris);
            File.WriteAllText(Path.Combine(_saveFile.FullName), json);
        }
    }
}