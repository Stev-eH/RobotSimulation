// NOTE: adapted from danielbierwirth/TCPTestClient.cs

// This work is licensed under the Creative Commons Attribution-ShareAlike 4.0 International License. 
// To view a copy of this license, visit http://creativecommons.org/licenses/by-sa/4.0/ 
// or send a letter to Creative Commons, PO Box 1866, Mountain View, CA 94042, USA.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;


public class RobotControllerServer : MonoBehaviour
{
	[Tooltip("The connection port on the machine to use.")]
	public int connectionPort = 11001;

	public UR10Controller ur10Controller;
	public UR10ControllerSlave ur10ControllerSlave;
	public GazeController gazeController;
	public BrickManager brickManager;

	public GameObject textForStatusbit;
	private string statusbit;

	#region private members 	
	/// <summary> 	
	/// TCPListener to listen for incomming TCP connection 	
	/// requests. 	
	/// </summary> 	
	private TcpListener tcpListener;
	/// <summary> 
	/// Background thread for TcpServer workload. 	
	/// </summary> 	
	private Thread tcpListenerThread;
	/// <summary> 	
	/// Create handle to connected tcp client. 	
	/// </summary> 	
	private TcpClient connectedTcpClient;
	#endregion

	// This enum list must be consistent with InterboServer message types
	private enum RobotControllerMessageType
	{
		Invalid, //0
		Acknowledge, //1
		Goodbye, //2
		UpdatePoseMaster, //3
		UpdatePoseSlave, //4
		InitBrick, //5
		GazePos, //6
		GetStatus //7
	}

	// Use this for initialization
	void Start()
	{
		// Start TcpServer background thread 		
		tcpListenerThread = new Thread(new ThreadStart(ListenForIncommingRequests));
		tcpListenerThread.IsBackground = true;
		tcpListenerThread.Start();

    }

	// Update is called once per frame
	void Update()
	{
		textForStatusbit.GetComponent<Text>().text = statusbit;
		if (Input.GetKeyDown(KeyCode.R))
		{
			SendServerMessage("Ready");
			Debug.Log("Sending Server message");
		}
	}

    private string HandleClientMessage(string message)
    {
        Debug.Log("received: " + message);

        string replyMessage = "acknowledge";

		RobotControllerMessageType messageType;
		string payload = ParseClientMessage(message, out messageType);

		switch (messageType)
		{
			case RobotControllerMessageType.UpdatePoseMaster:
								
				string[] cols = payload.Split(',');

				if (cols.Length == 14)
				{
					List<float> values = new List<float>();

					int iVal = 0;
					foreach (string col in cols)
					{
						values.Add(float.Parse(col));
						iVal++;
					}

					// Get joint portion of message
					float[] jointValues = values.GetRange(0, 7).ToArray();
					ur10Controller.jointValues = jointValues;

					// Get brick portion of message
					Vector3 brickPos = new Vector3(values[7], values[8], values[9]);
					Quaternion brickRot = new Quaternion(values[10], values[11], values[12], values[13]);
					//brickManager.UpdateMainBrickPose(brickPos, brickRot);
				}

				// dispatcher_.Enqueue(appManager_.ShowKeyboard);
				replyMessage = "Pose updated.";
				break;

			case RobotControllerMessageType.UpdatePoseSlave:
                string[] colms = payload.Split(',');

                if (colms.Length == 14)
                {
                    List<float> values = new List<float>();

                    int iVal = 0;
                    foreach (string col in colms)
                    {
                        values.Add(float.Parse(col));
                        iVal++;
                    }

                    // Get joint portion of message
                    float[] jointValues = values.GetRange(0, 7).ToArray();
                    ur10ControllerSlave.jointValues = jointValues;

                    // Get brick portion of message
                    Vector3 brickPos = new Vector3(values[7], values[8], values[9]);
                    Quaternion brickRot = new Quaternion(values[10], values[11], values[12], values[13]);
                    //brickManager.UpdateMainBrickPose(brickPos, brickRot);
                }

                // dispatcher_.Enqueue(appManager_.ShowKeyboard);
                replyMessage = "Pose updated.";
                break;

			case RobotControllerMessageType.InitBrick:
				brickManager.InitializeBricks(payload);
				replyMessage = "Layout initialized.";
				break;

			case RobotControllerMessageType.GazePos:
				Vector3 gazePos = gazeController.GazePosition();
				replyMessage = String.Format("{0:F4},{1:F4},{2:F4}", gazePos.x, gazePos.y, gazePos.z);
				break;

			case RobotControllerMessageType.GetStatus:
				uint status = Convert.ToUInt32(payload);
				statusbit = "Statusbit: " + Convert.ToString(status, 2).PadLeft(4, '0');
				Debug.Log(payload);
				break;

			default:
				replyMessage = "unrecognized request";
				Debug.Log("Unrecognized client message.");
				break;
		}

        return replyMessage;
    }

	private string ParseClientMessage(string message, out RobotControllerMessageType messageType)
	{
		messageType = RobotControllerMessageType.Invalid;
		string payload = "none";

		string[] splitString = message.Split('\t');

		messageType = (RobotControllerMessageType)Int32.Parse(splitString[0]);

		if (splitString.Length == 2)
		{
			payload = splitString[1];
		}

		return payload;
	}

	/// <summary> 	
	/// Runs in background TcpServerThread; Handles incomming TcpClient requests 	
	/// </summary> 	
	private void ListenForIncommingRequests()
	{
		try
		{
            Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture; // set default culture
            // Create listener on localhost port 8052. 			
            tcpListener = new TcpListener(IPAddress.Parse("127.0.0.1"), connectionPort);
			tcpListener.Start();
			Debug.Log("Server is listening");
			Byte[] bytes = new Byte[1024];
			while (true)
			{
				using (connectedTcpClient = tcpListener.AcceptTcpClient())
				{
					// Get a stream object for reading 					
					using (NetworkStream stream = connectedTcpClient.GetStream())
					{
						int length;
						// Read incomming stream into byte arrary. 						
						while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
						{
							var incommingData = new byte[length];
							Array.Copy(bytes, 0, incommingData, 0, length);
							// Convert byte array to string message. 							
							string clientMessage = Encoding.ASCII.GetString(incommingData);
							Debug.Log("client message received as: " + clientMessage);
							string[] messages = clientMessage.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

							for (int i = 0; i < messages.Length; i++)
							{
								string reply = HandleClientMessage(messages[i]);
                                SendServerMessage(reply);
                            }							
						}
					}
				}
			}
		}
		catch (SocketException socketException)
		{
			Debug.Log("SocketException " + socketException.ToString());
		}
	}
	/// <summary> 	
	/// Send message to client using socket connection. 	
	/// </summary> 	
	private void SendServerMessage(string message)
	{
		if (connectedTcpClient == null)
		{
			return;
		}

		try
		{
			// Get a stream object for writing. 			
			NetworkStream stream = connectedTcpClient.GetStream();
			if (stream.CanWrite)
			{
				string serverMessage = message;
				// Convert string message to byte array.                 
				byte[] serverMessageAsByteArray = Encoding.ASCII.GetBytes(serverMessage);
				// Write byte array to socketConnection stream.               
				stream.Write(serverMessageAsByteArray, 0, serverMessageAsByteArray.Length);
				Debug.Log("Server sent his message - should be received by client");
			}
		}
		catch (SocketException socketException)
		{
			Debug.Log("Socket exception: " + socketException);
		}
	}
}