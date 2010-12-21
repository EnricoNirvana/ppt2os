using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Drawing;
using System.Threading;
using System.ComponentModel;
using OpenMetaverse;
using OpenMetaverse.Http;
using OpenMetaverse.Imaging;
using OpenMetaverse.Packets;
using OpenMetaverse.Utilities;
using System.Diagnostics;
using Nwc.XmlRpc;
using Nini.Config;
using GridImageService;

namespace GridImageService
{
	public class XMLRPC_Service
	{
		GridClient Client;
		Boolean allow_uploads = false;
		Tools tools = new Tools();
		int counter = 1;
		UUID TextureID = UUID.Zero;
		IConfigSource config;
		
		public XMLRPC_Service (IConfigSource config)
		{
		this.config = config;
		tools.config = config;
		}
		
		public Boolean login(string firstname, string lastname, string password, string server) 
		{
			string startLocation = NetworkManager.StartLocation("Sandbox", 128, 128, 128);
			Client =  new GridClient();
			Client.Settings.LOGIN_SERVER = server;
            Client.Settings.ALWAYS_DECODE_OBJECTS = false;
            Client.Settings.ALWAYS_REQUEST_OBJECTS = false;
            Client.Settings.SEND_AGENT_UPDATES = false;
            Client.Settings.OBJECT_TRACKING = false;
            Client.Settings.STORE_LAND_PATCHES = false;
            Client.Settings.MULTIPLE_SIMS = false;
			Client.Settings.SEND_AGENT_APPEARANCE =false;
			Client.Settings.FETCH_MISSING_INVENTORY = false;
			Client.Network.EventQueueRunning += Network_OnEventQueueRunning;
			UUID agent_id = Client.Self.AgentID;	
            if (Client.Network.Login(firstname, lastname, password, "Image Upload Service", startLocation, "1.0"))
            {
				return true;
            }
            else
            {
				return false;
            }
		}
		
        private void Network_OnEventQueueRunning(object sender, EventQueueRunningEventArgs e)
        {
			allow_uploads = true;
        }
		
		static void Network_OnDisconnected ( NetworkManager.DisconnectType reason, string message )
		{
 
			Console.WriteLine(
				"Network_OnDisconnected:\n" +
				"\tReason: " + reason.ToString() + "\n" +
				"\tMessage: " + message + "\n" );
 
		}
		
		
		public ArrayList upload_textures(ArrayList files) 
		{
			if (allow_uploads == true)
			{	
				allow_uploads = false;
				tools.upload_textures(Client, files);  				
				while (tools.uploads != tools.uploaded) 
				{
				Thread.Sleep(1000);	
				}
				Client.Network.RequestLogout();
				allow_uploads = false;
				return tools.uuids;
			}
			else {
				return null;	
			}
		}
	} 
}

