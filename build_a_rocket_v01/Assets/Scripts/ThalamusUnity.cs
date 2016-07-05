using UnityEngine;
using System.Collections;
using CookComputing.XmlRpc;
using ThalamusUnityClientMessages;
using System.Net;
using System.Threading;
using System;
using System.Collections.Generic;


/*
 * This class shares a lot of similarities with the one that is already explained in the ThalamusUnityClient solution.
 * The main difference here is that there is no implementation of a ThalamusClient.
 * However, all the code related to the XmlRpc client and server are nearly the same.
 * Incomming messages from thalamus will be routed to the corresponding methods in the ThalamusUnity class.
 * */


/*
* This interface will be used by XmlRpc client proxy to send messages to the Thalamus client.
* It is the same as ITUCEvents (which will be the ones received in the other end), but extends it by adding the IXmlRpcProxy interface
* */
public interface IIThalamusUnityRpc : ITUCEvents, IXmlRpcProxy {}


/*
 * This is a normal Unity MonoBehaviour class, but it extends the ITUCActions because it will be receiving the messages defined in it
 * */
public class ThalamusUnity : MonoBehaviour, ITUCActions
{
	/*
     * Thus class just encapsulates the messages from ITUCEvents so that you have a clean way of publishing them.
     * This way, from any other script in Unity that can access this component, you can just call the "thalamusUnity_instance.Publisher.METHOD(); to get messages published
     * */
	public ThalamusUnityPublisher Publisher;
	public class ThalamusUnityPublisher : ITUCEvents
	{
		ThalamusUnity thalamusUnity;
		public ThalamusUnityPublisher(ThalamusUnity thalamusUnity)
		{
			this.thalamusUnity = thalamusUnity;
		}
		public void SentFromUnityToThalamus(string something)
		{
			thalamusUnity.ThalamusProxy.SentFromUnityToThalamus(something);
		}
		public void PieceSelected(string pieceName)
		{
			thalamusUnity.ThalamusProxy.PieceSelected (pieceName);
		}
	}
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	public void SentFromThalamusToUnity()
	{
		Debug.Log("Got from Thalamus: SentFromThalamusToUnity");
		//let's just send back an 
		Publisher.SentFromUnityToThalamus("hello from Unity!");
	}
	
	#region constructor/dispose
	
	/*
     * We use the constructor to initialize all the XmlRpc stuff. It's fine, and that way you keep the Awake and Start methods clean for your Unity code.
     * */
	public ThalamusUnity()
	{
		Publisher = new ThalamusUnityPublisher(this);
		
		remoteUri = String.Format("http://{0}:{1}", remoteAddress, remotePort);
		thalamusProxy = XmlRpcProxyGen.Create<IIThalamusUnityRpc>();
		thalamusProxy.Timeout = 1000;
		thalamusProxy.Url = remoteUri;
		thalamusProxy.Url = remoteUri;
		Debug.Log("Thalamus endpoint set to " + remoteUri);
		
		dispatcherThread = new Thread(new ThreadStart(DispatcherThreadThalamus));
		messageDispatcherThread = new Thread(new ThreadStart(MessageDispatcherThalamus));
		dispatcherThread.Start();
		messageDispatcherThread.Start();
	}
	
	public void Dispose()
	{
		shutdown = true;
		
		try
		{
			if (listener != null) listener.Stop();
		}
		catch { }
		
		try
		{
			if (dispatcherThread != null) dispatcherThread.Join();
		}
		catch { }
		
		try
		{
			if (messageDispatcherThread != null) messageDispatcherThread.Join();
		}
		catch { }
	}
	
	#endregion
	
	#region XmlRpc code
	
	// This helps on debugging. Switch it off if you'r getting your Unity debig too cluttered.
	private bool printExceptions = true;
	
	//make sure these correspond (in opposite) to the ports used in the ThalamusUnityClient
	private int localPort = 7001;
	private string remoteAddress = "localhost";
	private int remotePort = 7000;
	
	private HttpListener listener;
	private bool serviceRunning;
	private bool shutdown;
	List<HttpListenerContext> httpRequestsQueue = new List<HttpListenerContext>();
	private Thread dispatcherThread;
	private Thread messageDispatcherThread;
	private string remoteUri = "";
	//XmLRpc client through which we send messages back to the Thalamus client
	IIThalamusUnityRpc thalamusProxy;
	public IIThalamusUnityRpc ThalamusProxy
	{
		get { return thalamusProxy; }
	}
	
	
	public void ProcessMessageThalamus(object oContext)
	{
		try
		{
			XmlRpcListenerService svc = new ThalamusUnityService(this);
			svc.ProcessRequest((HttpListenerContext)oContext);
		}
		catch (Exception e)
		{
			if (printExceptions) Debug.Log("Exception in ProcessMessageThalamus: " + e.ToString());
		}
	}
	
	public void DispatcherThreadThalamus()
	{
		while (!serviceRunning)
		{
			try
			{
				Debug.Log("Attempt to start service on port '" + localPort + "'");
				listener = new HttpListener();
				listener.Prefixes.Add(string.Format("http://*:{0}/", localPort));
				listener.Start();
				Debug.Log("XMLRPC Listening on " + string.Format("http://*:{0}/", localPort));
				serviceRunning = true;
			}
			catch
			{
				localPort++;
				Debug.Log("Port unavaliable.");
				serviceRunning = false;
			}
		}
		
		while (!shutdown)
		{
			try
			{
				HttpListenerContext context = listener.GetContext();
				lock (httpRequestsQueue)
				{
					httpRequestsQueue.Add(context);
				}
			}
			catch (Exception e)
			{
				if (printExceptions) Debug.Log("Exception in DispatcherThreadThalamus: " + e.ToString());
				serviceRunning = false;
				if (listener != null)
					listener.Close();
			}
		}
		Debug.Log("Terminated DispatcherThreadThalamus");
		listener.Close();
	}
	
	public void MessageDispatcherThalamus()
	{
		while (!shutdown)
		{
			bool performSleep = true;
			try
			{
				if (httpRequestsQueue.Count > 0)
				{
					performSleep = false;
					List<HttpListenerContext> httpRequests;
					lock (httpRequestsQueue)
					{
						httpRequests = new List<HttpListenerContext>(httpRequestsQueue);
						httpRequestsQueue.Clear();
					}
					foreach (HttpListenerContext r in httpRequests)
					{
						//ProcessRequest(r);
						(new Thread(new ParameterizedThreadStart(ProcessMessageThalamus))).Start(r);
						performSleep = false;
					}
				}
				
				
			}
			catch (Exception e)
			{
				if (printExceptions) Debug.Log("Exception in MessageDispatcherThalamus: " + e.ToString());
			}
			if (performSleep) Thread.Sleep(10);
		}
		Debug.Log("Terminated MessageDispatcherThalamus");
	}
	#endregion
}