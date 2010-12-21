import java.awt.Color;
import java.net.InetAddress;
import java.awt.Dimension;
import java.awt.Graphics2D;
import java.awt.geom.Rectangle2D;
import java.awt.image.BufferedImage;
import java.io.Console;
import java.io.File;
import java.io.FileInputStream;
import java.io.FileNotFoundException;
import java.io.FileOutputStream;
import java.io.IOException;
import java.util.ArrayList;

import org.apache.poi.hslf.HSLFSlideShow;
import org.apache.poi.hslf.model.Slide;
import org.apache.poi.hslf.usermodel.SlideShow;
import org.apache.xmlrpc.XmlRpcException;
import org.ini4j.Wini;

public class PPT2OSService
{
	private static Wini ini;
	
	
	public ArrayList ppt_to_png(String ppt_name, String target_dir, String format) throws IOException
	{
		ArrayList slides = new ArrayList();
		System.out.println("Converting " + ppt_name);
		FileInputStream is = new FileInputStream(ini.get("converter","workpath")+"/"+ppt_name);
		SlideShow ppt = new SlideShow(is);
		is.close();

		Dimension pgsize = ppt.getPageSize();

		Slide[] slide = ppt.getSlides();
		for (int i = 0; i < slide.length; i++) {

			BufferedImage img = new BufferedImage(pgsize.width, pgsize.height, BufferedImage.TYPE_INT_RGB);
			Graphics2D graphics = img.createGraphics();
			//clear the drawing area
			graphics.setPaint(Color.white);
			graphics.fill(new Rectangle2D.Float(0, 0, pgsize.width, pgsize.height));
			slide[i].draw(graphics);
			int index = ppt_name.lastIndexOf('.');
			String slidename = ppt_name.substring(0,index)+"-slide-"  + (i+1) + "." + format;
			slides.add(slidename);
			
			 // Create multiple directories
			File tdir = new File(ini.get("converter","workpath")+"/"+target_dir);
			if (!tdir.exists())
			{
			    boolean success = tdir.mkdir();
			    if (!success) {
			    	System.out.println("Problem while creating dirs!");		
			    }	
			}
			FileOutputStream out = new FileOutputStream(ini.get("converter","workpath")+"/"+target_dir+"/"+slidename);
			javax.imageio.ImageIO.write(img, format, out);
			out.close();
		}
		return slides;
	}
	
	public int upload(byte[] data) throws XmlRpcException, Exception
	{
	FileOutputStream fos = new FileOutputStream("/home/jeroen/workspace/PPT2OS/java.txt.out");
	fos.write(data);
	fos.close();
	return data.length;
	}
 
	public static void main(String[] args) throws Exception
	{
		ini = new Wini(new File("../ppt2os.ini"));
		int port = ini.get("converter","port",int.class);
		System.out.println("Serving on port "+port);
		SimpleXmlRpcServer server = new SimpleXmlRpcServer(port);
		server.addHandler("ppt2os", new PPT2OSService());
		server.serve_forever();
 
		while (true)
		{
			System.out.println("<HEARTBEAT>");
			Thread.sleep(60 * 60 * 60);
		}
	}
}