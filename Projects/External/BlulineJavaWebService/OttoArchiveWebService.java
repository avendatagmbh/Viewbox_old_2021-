import java.io.*;

import com.ser.blueline.*;

public class OttoArchiveWebService {
	
	private static ISerClassFactory factory;
	private static IDocumentServer server;
	private static ISystem system;
	private static ISession session;
	
	void InitServer(){
		try {
			if(factory == null) factory = de.serac.bluelineimpl.SERACClassFactory.getInstance();
			
			System.out.println("Factory erstellt!");
			
			if(server == null) server = factory.getDocumentServerInstance(new BufferedInputStream(new FileInputStream("./conf/BL.ini")));
			
			System.out.println("Server instanziiert!");
			
			if(system == null) system = server.getSystem("SYSTEM1");
			
			System.out.println("System erstellt!");
			
			CreateSession();
		}catch(Exception e){
			e.printStackTrace();
		}
	}
	
	public String sayHello(){
		return "HELLO!";
	}
	
	void CreateSession(){
		if(session == null || !session.isValid()){
			try{
				ITicket ticket = server.login(system,"fkt_viewbox","?2HE?3jI".toCharArray());
				session = server.createSession(ticket);
				
				System.out.println("Session erstellt!");
			}catch(Exception e){
				e.printStackTrace();
			}
		}
	}
	
	public String GetDocumentPath(String id, String userId){
		InitServer();

		try {
			IDocument document = server.getDocument4ID(id, session);
			IDocumentFilter filter = server.getDocumentFilter(IDocumentFilter.TIFF_FILTER);
			
			int defaultRepresentation = document.getDefaultRepresentation();
			
			for(int i = 0; i < document.getPartDocumentCount(defaultRepresentation); i++){
				filter.appendDocPart(document.getPartDocument(defaultRepresentation, i), session, true);
				//filter.appendDocPart(document.getPartDocument(defaultRepresentation, i), session, false);
			}
			
			InputStream stream = filter.getDocumentInputStream();
			
			String path = "C:\\inetpub\\temp\\viewbox\\" + userId + "\\" + id + ".tif";
			
			File file = new File(path);
			file.mkdirs();
			
			FileOutputStream output = new FileOutputStream(file);
			
			int len;
			while((len = stream.read()) > 0){
				output.write(len);
			}
			
			output.close();
			stream.close();
			filter.close();
			
			return path;
		} catch (Exception e) {
			e.printStackTrace();
		}
		
		return null;
	}

	public long GetCount(String search){
		IDocumentHitList hitlist = GetHitList(search);
			
		return hitlist.getTotalHitCount();
	}
	
	private IDocumentHitList GetHitList(String search){
		InitServer();
		try{
			IDescriptor[] descriptors = server.getDescriptors(session);
			
			IQueryExpression query = null;
			
			//TODO: search einbauen!
			
			for(int i = 0; i < descriptors.length; i++){
				if(i == 0){
					query = factory.getExpressionInstance(descriptors[i], null);
				}else{
					IQueryExpression query1 = factory.getExpressionInstance(descriptors[i], null);
					query = factory.getExpressionInstance(query, query1, IQueryOperator.OR);
				}
			}
			
			IQueryParameter queryParameter = factory.getQueryParameterInstance(session.getDatabaseNames(), new java.util.Date(1970,1,1), new java.util.Date());
			IDocumentHitList hitlist = server.query(queryParameter, session);
			
			return hitlist;
		}catch(Exception e){
			e.printStackTrace();
		}
		return null;
	}

	public void GetDocuments(String search, int from, int to){
		IDocumentHitList hitlist = GetHitList(search);
		
		for(int i = 0; i < hitlist.getDocumentObjects().length; i++){
			//TODO: return IDocumentInformation
		}
	}
}
