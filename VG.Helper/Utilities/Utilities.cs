using System;
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace VG.Helper.Utilities
{


    public static class Utilities
    {
        /// <summary>
        /// Extracts JSON content from a .save (gzipped) file by copying it to a temp location first.
        /// Includes retry logic to handle file locks during autosave.
        /// </summary>
        /// <param name="saveFilePath">Path to the .save file.</param>
        /// <returns>JSON string contained in the save file.</returns>
        public static string ExtractJsonFromSave(string saveFilePath)
        {
            if (string.IsNullOrWhiteSpace(saveFilePath))
                throw new ArgumentException("Save file path cannot be null or empty.", nameof(saveFilePath));

            if (!File.Exists(saveFilePath))
                throw new FileNotFoundException("Save file not found.", saveFilePath);

            const int maxRetries = 3;
            const int retryDelayMs = 500; // half a second

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".save");

                try
                {
                    // Copy the save file to temp to avoid file locks
                    File.Copy(saveFilePath, tempPath, overwrite: true);

                    using var fileStream = File.OpenRead(tempPath);
                    using var gzipStream = new GZipStream(fileStream, CompressionMode.Decompress);
                    using var reader = new StreamReader(gzipStream);

                    // Read the decompressed JSON directly
                    return reader.ReadToEnd();
                }
                catch (IOException) when (attempt < maxRetries)
                {
                    // If locked, wait and retry
                    Thread.Sleep(retryDelayMs);
                }
                finally
                {
                    // Clean up temp file
                    try { if (File.Exists(tempPath)) File.Delete(tempPath); }
                    catch { /* ignore cleanup errors */ }
                }
            }

            throw new IOException($"Unable to access save file after {maxRetries} attempts.");
        }
    }




}

