using UnityEngine;
using System.Collections;
using CookComputing.XmlRpc;
using ThalamusUnityClientMessages;


namespace BuildARocketGame {
	/*
	 * This class is very similar to ThalamusUnityClientService.
	 * The service will deal with the messages that arrive from the Thalamus client, so that corresponds to implementing the ITUCActions interface.
	 * */
	public class ThalamusUnityService : XmlRpcListenerService, ITUCActions
	{
		/*
	     * Again, declare a ThalamusUnity field and pass it in the constructor, so we can send the messages back to the MonoBehaviour class
	     * */
		ThalamusUnity thalamusUnity;
		public ThalamusUnityService(ThalamusUnity thalamusUnity)
		{
			this.thalamusUnity = thalamusUnity;
		}
		/*
	     * Just forward the message to the ThalamusUnity instance
	     * */
		public void SentFromThalamusToUnity()
		{
			thalamusUnity.SentFromThalamusToUnity();
		}
	}
}