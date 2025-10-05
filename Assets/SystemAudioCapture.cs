using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class SystemAudioCapture : MonoBehaviour
{
    private readonly float[] spectrumData = new float[8];
    public float[] SpectrumData => spectrumData;
    private unsafe void Awake()
    {
        new Thread(() =>
        {
            var ip = new IPEndPoint(IPAddress.Any, 0);
            UdpClient client = null;
            try
            {
                client = new(14576);
                while (this)
                {
                    var data = client.Receive(ref ip);
                    if (data != null && data.Length == 32)
                    {
                        fixed (byte* ptr = data)
                        {
                            for (int i = 0; i < spectrumData.Length; i++)
                                spectrumData[i] = *(float*)(ptr + i * 4);
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
                client.Dispose();
            }
        }).Start();
    }
}
