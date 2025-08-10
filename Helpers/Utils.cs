using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NAudio.Wave;
using MahnitolaList.Models;

namespace MahnitolaList.Helpers
{
    public static class Utils
    {
        public static List<TrackItem> GetWavFiles(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
                throw new DirectoryNotFoundException($"Директорія не знайдена: {directoryPath}");

            var files = Directory.GetFiles(directoryPath, "*.wav", SearchOption.TopDirectoryOnly);

            return files
                .Where(f => !string.Equals(Path.GetFileName(f), "stop_stop.wav", StringComparison.OrdinalIgnoreCase))
                .Select(f => new TrackItem(Path.GetFileName(f), GetWavDurationInSeconds(f)))
                .ToList();
        }

        public static int GetWavDurationInSeconds(string filePath)
        {
            using var reader = new AudioFileReader(filePath);
            return (int)Math.Round(reader.TotalTime.TotalSeconds);
        }

        public static void Shuffle<T>(IList<T> list)
        {
            var rnd = new Random();
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rnd.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        public static string GenerateOscContent(List<TrackItem> tracks, bool includeRadio, string radioName)
        {
            if (tracks == null || tracks.Count == 0)
                throw new ArgumentException("Список треків порожній.");

            int n = tracks.Count - 1;
            var sb = new StringBuilder();

            sb.AppendLine("{macro:playlist_frame}");

            if (includeRadio)
                sb.AppendLine($"\t\"{EscapeOscString(radioName)}\" (S.$.p)");

            sb.AppendLine("\t(L.L.mp3_number_track) s0 -1 =");
            sb.AppendLine("\t{if}");
            sb.AppendLine("\t\t\"stop_stop\" (S.$.mp3_track_name)");
            sb.AppendLine("\t\t1 (S.L.mp3_track_time)");
            if (includeRadio)
            {
                sb.AppendLine("\t\t(L.L.music_mode)");
                sb.AppendLine("\t\t{if}");
                sb.AppendLine($"\t\t\t\"  {EscapeOscString(radioName)}  \" (S.$.mp3_display_track_name)");
                sb.AppendLine("\t\t{else}");
                sb.AppendLine("\t\t\t\"    \" (S.$.mp3_display_track_name)");
                sb.AppendLine("\t\t{endif}");
            }
            sb.AppendLine("\t{endif}");
            sb.AppendLine();
            sb.AppendLine($"\t{n} (S.L.mp3_max_number_track)");
            sb.AppendLine();

            foreach (var t in tracks)
            {
                sb.AppendLine($"\tl0 {n} =");
                sb.AppendLine("\t{if}");
                sb.AppendLine($"\t\t\"{EscapeOscString(t.FileName)}\" (S.$.mp3_track_name)");
                sb.AppendLine("\t\t0 (S.L.mp3_track_time)");
                sb.AppendLine($"\t\t\"  {EscapeOscString(t.FileName)}  \" (S.$.mp3_display_track_name)");
                sb.AppendLine("\t{endif}");
            }

            sb.AppendLine("{end}");
            return sb.ToString();
        }

        public static void SaveOscFile(string filePath, string content)
        {
            File.WriteAllText(filePath, content, Encoding.UTF8);
        }

        private static string EscapeOscString(string s)
        {
            return s?.Replace("\\", "\\\\").Replace("\"", "\\\"") ?? string.Empty;
        }
    }
}
