using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using OpenMetaverse;
using OpenMetaverse.Imaging;
using Nini.Config;
using GridImageService;

namespace GridImageService
{
	public class Tools
	{
		public IConfigSource config;
		public int uploads =0;
		public int uploaded =0;
		public ArrayList uuids = new ArrayList();
		private Inventory Inventory;
		private InventoryManager Manager;
		private UUID ppt2os_folder = UUID.Zero;
		
        public byte[] LoadImage(string fileName)
        {
            byte[] UploadData;
			//String workpath = config.Configs["uploader"].Get("workpath");
			string cwd = System.IO.Directory.GetCurrentDirectory();
			String workpath="../../process/";
			string filepath = workpath + fileName;
			string lowfilename = fileName.ToLower();            
            Bitmap bitmap = null;

            try
            {
                if (lowfilename.EndsWith(".jp2") || lowfilename.EndsWith(".j2c"))
                {
                    Image image;
                    ManagedImage managedImage;

                    // Upload JPEG2000 images untouched
                    UploadData = System.IO.File.ReadAllBytes(filepath);
                    
                    OpenJPEG.DecodeToImage(UploadData, out managedImage, out image);
                    bitmap = (Bitmap)image;
                }
                else
                {
                    if (lowfilename.EndsWith(".tga"))
                        bitmap = LoadTGAClass.LoadTGA(filepath);
                    else
					{
                        bitmap = (Bitmap)System.Drawing.Image.FromFile(filepath);
					}
                    int oldwidth = bitmap.Width;
                    int oldheight = bitmap.Height;

                    if (!IsPowerOfTwo((uint)oldwidth) || !IsPowerOfTwo((uint)oldheight))
                    {
                        Bitmap resized = new Bitmap(512, 512, bitmap.PixelFormat);
                        Graphics graphics = Graphics.FromImage(resized);

                        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                        graphics.InterpolationMode =
                           System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        graphics.DrawImage(bitmap, 0, 0, 512, 512);

                        bitmap.Dispose();
                        bitmap = resized;

                        oldwidth = 512;
                        oldheight = 512;
                    }

                    // Handle resizing to prevent excessively large images
                    if (oldwidth > 1024 || oldheight > 1024)
                    {
                        int newwidth = (oldwidth > 1024) ? 1024 : oldwidth;
                        int newheight = (oldheight > 1024) ? 1024 : oldheight;

                        Bitmap resized = new Bitmap(newwidth, newheight, bitmap.PixelFormat);
                        Graphics graphics = Graphics.FromImage(resized);

                        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                        graphics.InterpolationMode =
                           System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        graphics.DrawImage(bitmap, 0, 0, newwidth, newheight);

                        bitmap.Dispose();
                        bitmap = resized;
                    }

                    UploadData = OpenJPEG.EncodeFromImage(bitmap, false);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString() + " SL Image Upload ");
                return null;
            }
            return UploadData;
        }
		
		
		//Finds our ppt2os basefolder, or creates one if not existing yet
		private UUID find_ppt2os_folder(GridClient client) 
		{
            Manager = client.Inventory;
            Inventory = Manager.Store;
			InventoryFolder rootFolder = Inventory.RootFolder;	
			UUID texture_basefolder = client.Inventory.FindFolderForType(AssetType.Texture);
			List<InventoryBase> texture_contents = Inventory.GetContents(texture_basefolder);
			foreach (InventoryBase c in texture_contents)
			{
				InventoryFolder subfolder = c as InventoryFolder;
				if (c is InventoryFolder)
               	{		
					if (subfolder.Name == "ppt2os") {
					ppt2os_folder = subfolder.UUID;
					return ppt2os_folder;
					}
				}
			}	
			//No folder yet
			if (ppt2os_folder == UUID.Zero) {
				client.Inventory.CreateFolder(texture_basefolder, "ppt2os");
				find_ppt2os_folder(client);
			}
			return ppt2os_folder;
		}
		
		//Create a folder inside the ppt2os basefolder or returns an existing one
		private UUID get_ppt_folder(GridClient client, string folder_name)
		{
			UUID ppt_folder = UUID.Zero;
			List<InventoryBase> ppt2os_contents = Inventory.GetContents(ppt2os_folder);
			foreach (InventoryBase d in ppt2os_contents)
			{
				InventoryFolder _ppt_folder = d as InventoryFolder;
				if (d is InventoryFolder)
               	{		
					if (_ppt_folder.Name == folder_name) {
					ppt_folder = _ppt_folder.UUID;
					return ppt_folder;
					}
				}	
			}
			if (ppt_folder == UUID.Zero) {
				client.Inventory.CreateFolder(ppt2os_folder, folder_name);
				return get_ppt_folder(client,folder_name);
			}
			else {
			return ppt_folder;
			}
		}
		
		
        public void upload_textures(GridClient client, ArrayList files)
        {
			uploads = files.Count;
			uploaded = 0;
			UUID TextureID = UUID.Zero;	
			uuids.Clear();
			ppt2os_folder = find_ppt2os_folder(client);			
			foreach (string file in files) {						
				byte[] jpeg2k = LoadImage(file);
				UUID destiny_folder = get_ppt_folder(client,file.Substring(0,file.IndexOf("-")));	
           		if (jpeg2k == null) Console.WriteLine("Failed to compress image to JPEG2000");
				string name = System.IO.Path.GetFileNameWithoutExtension(file);									                                  
               	client.Inventory.RequestCreateItemFromAsset(jpeg2k, name, "Uploaded with PPT2OS",
                AssetType.Texture, InventoryType.Texture, destiny_folder,
                delegate(bool success, string status, UUID itemID, UUID assetID)
                {
                    Console.WriteLine(String.Format("RequestCreateItemFromAsset() returned: Success={0}, Status={1}, ItemID={2}, AssetID={3}",    success, status, itemID, assetID));
                    TextureID = assetID;
					uuids.Add(assetID.ToString());
					DateTime start = DateTime.Now;
					uploaded +=1;
                });
           	}
        }
		
		
        private static bool IsPowerOfTwo(uint n)
        {
            return (n & (n - 1)) == 0 && n != 0;
        }	
	}
}

