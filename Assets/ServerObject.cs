using System.Diagnostics;
using System.Threading;
using NetMQ;
using NetMQ.Sockets;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NetMqPublisher
{
    private readonly Thread _listenerWorker;

    private bool _listenerCancelled;

    public delegate string MessageDelegate(string message);

    private readonly MessageDelegate _messageDelegate;

    private readonly Stopwatch _contactWatch;

    private const long ContactThreshold = 1000;

    

    public bool Connected;

    private void ListenerWork()
    {
        AsyncIO.ForceDotNet.Force();
        using (var server = new ResponseSocket())
        {
            server.Bind("tcp://*:12346");

            while (!_listenerCancelled)
            {
                Connected = _contactWatch.ElapsedMilliseconds < ContactThreshold;
                string message;
                if (!server.TryReceiveFrameString(out message)) continue;
                _contactWatch.Restart();
                var response = _messageDelegate(message);
                server.SendFrame(response);
            }
        }
        NetMQConfig.Cleanup();
    }

    public NetMqPublisher(MessageDelegate messageDelegate)
    {
        _messageDelegate = messageDelegate;
        _contactWatch = new Stopwatch();
        _contactWatch.Start();
        _listenerWorker = new Thread(ListenerWork);
    }

    public void Start()
    {
        _listenerCancelled = false;
        _listenerWorker.Start();
    }

    public void Stop()
    {
        _listenerCancelled = true;
        _listenerWorker.Join();
    }
}

public class ServerObject : MonoBehaviour
{
    public bool Connected;
    private NetMqPublisher _netMqPublisher;
    private string _response;

    private void Start()
    {
        _netMqPublisher = new NetMqPublisher(HandleMessage);
        _netMqPublisher.Start();
    }

    private void LateUpdate()
    {
        Time.timeScale = 0;
        StartCoroutine(reply_at_end_of_frame());
        Connected = _netMqPublisher.Connected;
        
    }

    

    private string HandleMessage(string message)
    {
        // Not on main thread
        return _response;
    }

    private void OnDestroy()
    {
        _netMqPublisher.Stop();
    }

    IEnumerator reply_at_end_of_frame()
    {
        
        yield return new WaitForEndOfFrame();
        var position = transform.position;
        ScreenCapture.CaptureScreenshot("screen_grab.png");
        //_response = $"{position.x} {position.y} {position.z}";

        _response = $"{Time.frameCount}";
        Time.timeScale = 1;



        //byte[] byteArray = get_camera_view();
        //Dictionary<string, byte[]> data_dict = new Dictionary<string, byte[]>();
        //Dictionary<string, string> data_dict = new Dictionary<string, string>();
        //yield on a new YieldInstruction that waits for 5 seconds.

    }

    

    //private byte[] get_camera_view()
    //{
    //    Camera cam = gameObject.GetComponent<Camera>();
    //    RenderTexture screenTexture = new RenderTexture(Screen.width, Screen.height, 16);
    //    cam.targetTexture = screenTexture;
    //    RenderTexture.active = screenTexture;
    //    cam.Render();
    //    Texture2D renderedTexture = new Texture2D(Screen.width, Screen.height);
    //    renderedTexture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
    //    RenderTexture.active = null;
    //    byte[] byteArray = renderedTexture.EncodeToPNG();
    //    return byteArray;
    //    //System.IO.File.WriteAllBytes(Application.dataPath + "/cameracapture.png", byteArray);

    //}
}
