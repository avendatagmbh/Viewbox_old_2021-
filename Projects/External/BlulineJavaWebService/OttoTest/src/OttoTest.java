import java.io.BufferedInputStream;
import java.io.File;
import java.io.FileInputStream;
import java.io.FileOutputStream;
import java.io.InputStream;
import java.text.SimpleDateFormat;
import java.util.Date;

import com.ser.blueline.IDescriptor;
import com.ser.blueline.IDocument;
import com.ser.blueline.IDocumentFilter;
import com.ser.blueline.IDocumentHitList;
import com.ser.blueline.IDocumentServer;
import com.ser.blueline.IQueryExpression;
import com.ser.blueline.IQueryOperator;
import com.ser.blueline.IQueryParameter;
import com.ser.blueline.ISerClassFactory;
import com.ser.blueline.ISession;
import com.ser.blueline.ISystem;
import com.ser.blueline.ITicket;

public class OttoTest {

	/**
	 * @param args
	 */
	public static void main(String[] args) {
		getCount("%");
		// System.out.println(getCount(null));
		// System.out.println(getDocumentPath("","1"));
	}

	private static ISerClassFactory factory;
	private static IDocumentServer server;
	private static ISystem system;
	private static ISession session;

	static void initServer() {
		try {

			if (factory == null)
				factory = de.serac.bluelineimpl.SERACClassFactory.getInstance();

			System.out.println("Factory erstellt!");

			// if(server == null) server = factory.getDocumentServerInstance(new
			// BufferedInputStream(new FileInputStream("./conf/BL.ini")));
			if (server == null)
				server = factory.getDocumentServerInstance(new FileInputStream(
						"./conf/BL.ini"));

			System.out.println("Server instanziiert!");

			if (system == null)
				system = server.getSystem("SYSTEM1");

			System.out.println("System erstellt!");

			createSession();
		} catch (Exception e) {
			e.printStackTrace();
		}
	}

	static void closeServer() {
		try {
			if (server == null)
				server.close();
		} catch (Exception e) {
			e.printStackTrace();
		}
	}

	static void createSession() {
		if (session == null || !session.isValid()) {
			try {
				ITicket ticket = server.login(system, "fkt_viewbox",
						"?2HE?3jI".toCharArray());
				session = server.createSession(ticket);

				System.out.println("Session erstellt!");
			} catch (Exception e) {
				e.printStackTrace();
			}
		}
	}

	public static String getDocumentPath(String id, String userId) {
		initServer();

		try {
			IDocument document = server.getDocument4ID(id, session);
			IDocumentFilter filter = server
					.getDocumentFilter(IDocumentFilter.TIFF_FILTER);

			int defaultRepresentation = document.getDefaultRepresentation();

			for (int i = 0; i < document
					.getPartDocumentCount(defaultRepresentation); i++) {
				filter.appendDocPart(
						document.getPartDocument(defaultRepresentation, i),
						session, true);
				// filter.appendDocPart(document.getPartDocument(defaultRepresentation,
				// i), session, false);
			}

			InputStream stream = filter.getDocumentInputStream();

			String path = "C:\\inetpub\\temp\\viewbox\\" + userId + "\\" + id
					+ ".tif";

			File file = new File(path);
			file.mkdirs();

			FileOutputStream output = new FileOutputStream(file);

			int len;
			while ((len = stream.read()) > 0) {
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

	public static long getCount(String search) {
		initServer();
		try {
			IDescriptor[] descriptors = server.getDescriptors(session);

			IQueryExpression query = null;

			// TODO: search einbauen!

			String[] searchArray = new String[1];
			searchArray[0] = search;
			for (int i = 0; i < descriptors.length; i++) {
				if (i == 0) {
					query = factory.getExpressionInstance(descriptors[i],
							searchArray);
				} else {
					IQueryExpression query1 = factory.getExpressionInstance(
							descriptors[i], searchArray);
					query = factory.getExpressionInstance(query, query1,
							IQueryOperator.OR);
				}
			}

			SimpleDateFormat sdf = new SimpleDateFormat("yyyy/MM/dd");
			Date from = sdf.parse("1970/01/01");
			Date to = java.util.Calendar.getInstance().getTime();

			IQueryParameter queryParameter = factory.getQueryParameterInstance(
					session.getDatabaseNames(), from, to);
			IDocumentHitList hitlist = server.query(queryParameter, session);

			return hitlist != null ? hitlist.getTotalHitCount() : 0;
		} catch (Exception e) {
			e.printStackTrace();
		}
		closeServer();
		return 0;
	}

	public static String getDocuments(String search, int from, int to) {
		initServer();
		try {
			IDescriptor[] descriptors = server.getDescriptors(session);

			IQueryExpression query = null;

			// TODO: search einbauen!

			for (int i = 0; i < descriptors.length; i++) {
				if (i == 0) {
					query = factory.getExpressionInstance(descriptors[i], null);
				} else {
					IQueryExpression query1 = factory.getExpressionInstance(
							descriptors[i], null);
					query = factory.getExpressionInstance(query, query1,
							IQueryOperator.OR);
				}
			}

			IQueryParameter queryParameter = factory.getQueryParameterInstance(
					session.getDatabaseNames(), new java.util.Date(1970, 1, 1),
					new java.util.Date());
			IDocumentHitList hitlist = server.query(queryParameter, session);

			String documentsString = "";

			if (hitlist != null)
				for (int i = 0; i < hitlist.getDocumentObjects().length; i++) {
					IDocument document = hitlist.getDocumentObjects()[i];
					documentsString += document.getDocumentID().getID()
							+ "<<DIV>>";
					documentsString += document.getDisplayName() + "<<DIV>>";
					documentsString += document.getDisplayType() + "<<DIV>>";
					documentsString += "<<EOL>>";
				}

			return documentsString;
		} catch (Exception e) {
			e.printStackTrace();
		}

		return null;
	}

}
