using NAudio.MediaFoundation;
using NAudio.Wave;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MahnitolaList.Services
{
    public class AudioConvertService
    {
        static AudioConvertService()
        {
            try
            {
                MediaFoundationApi.Startup();
            }
            catch
            {
            }
        }

        public Task ConvertToWav20kAsync(string inputPath, string outputPath)
        {
            return Task.Run(() =>
            {
                if (!File.Exists(inputPath))
                    throw new FileNotFoundException("Input file not found", inputPath);

                Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

                var outWaveFormat = new WaveFormat(20000, 16, 1);

                using var reader = new MediaFoundationReader(inputPath);

                using var resampler = new MediaFoundationResampler(reader, outWaveFormat)
                {
                    ResamplerQuality = 60
                };

                WaveFileWriter.CreateWaveFile(outputPath, resampler);
            });
        }
    }
}
