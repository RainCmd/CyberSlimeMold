using NAudio.CoreAudioApi;
using NAudio.Dsp;
using NAudio.Wave;
using System.Net;
using System.Net.Sockets;

namespace AudioCapturer
{
    internal unsafe class Program
    {
        private const int FFT_SIZE = 512;
        private static readonly int m = (int)Math.Log(FFT_SIZE, 2);
        private static void Main(string[] args)
        {
            var enumerator = new MMDeviceEnumerator();
            var device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia) ?? throw new Exception("未找到音频输出设备");
            var capture = new WasapiLoopbackCapture(device);

            var waveProvider = new BufferedWaveProvider(capture.WaveFormat);
            const int TYPE_SIZE = sizeof(float);
            var waveData = new byte[FFT_SIZE * TYPE_SIZE];
            var fftBuffer = new Complex[FFT_SIZE];
            var spectrumData = new float[FFT_SIZE];
            capture.DataAvailable += (sender, e) =>
            {
                waveProvider.AddSamples(e.Buffer, 0, e.BytesRecorded);
                if (e.BytesRecorded > FFT_SIZE * TYPE_SIZE)
                {
                    waveProvider.Read(waveData, 0, FFT_SIZE * TYPE_SIZE);
                    for (int i = 0; i < FFT_SIZE; i++)
                    {
                        fftBuffer[i].X = BitConverter.ToSingle(waveData, i * TYPE_SIZE) * (float)FastFourierTransform.HammingWindow(i, FFT_SIZE);
                        fftBuffer[i].Y = 0;
                    }
                    FastFourierTransform.FFT(true, m, fftBuffer);
                    for (int i = 0; i < FFT_SIZE; i++)
                    {
                        spectrumData[i] = (float)Math.Sqrt(fftBuffer[i].X * fftBuffer[i].X + fftBuffer[i].Y * fftBuffer[i].Y);
                    }
                    Send(spectrumData);

                    waveProvider.ClearBuffer();
                }
            };
            capture.StartRecording();
        }
        private static readonly UdpClient client = new();
        private static readonly IPEndPoint address = new(IPAddress.Loopback, 14576);
        private const int SEND_DATA_COUNT = 8;
        private static readonly byte[] sendBuffer = new byte[SEND_DATA_COUNT * 4];
        private static float volume;
        private static void Send(float[] raw)
        {
            fixed (byte* buffer = sendBuffer)
            {
                var step = raw.Length / SEND_DATA_COUNT;
                var min = float.MaxValue;
                var max = float.MinValue;
                for (var x = 0; x < SEND_DATA_COUNT; x++)
                {
                    float* data = (float*)(buffer + x * 4);
                    *data = 0;
                    for (var y = 0; y < step; y++) *data += raw[x + y * SEND_DATA_COUNT];
                    if (*data < min) min = *data;
                    if (*data > max) max = *data;
                }
                var lerp = .15f;
                volume = volume * lerp + max * (1 - lerp);
                volume = Math.Max(volume, .01f);
                min *= .9f;
                for (var x = 0; x < SEND_DATA_COUNT; x++)
                {
                    float* data = (float*)(buffer + x * 4);
                    *data = (*data - min) / (volume - min);
                }
            }
            client.Send(sendBuffer, sendBuffer.Length, address);
        }
    }
}
