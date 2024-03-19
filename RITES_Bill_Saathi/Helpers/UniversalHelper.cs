using System;
namespace RITES_Bill_Saathi.Helpers
{
	public static class UniversalHelper
	{
        public static string GenerateFileName(string extension = "")
        {
            // Get the current Unix timestamp
            var unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            // Generate the filename by appending the extension, if provided
            var fileName = extension.Length > 0 ? $"{unixTimestamp}.{extension}" : unixTimestamp.ToString();

            return fileName;
        }
    }
}

