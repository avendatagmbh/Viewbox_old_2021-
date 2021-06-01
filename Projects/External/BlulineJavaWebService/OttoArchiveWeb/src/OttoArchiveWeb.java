import java.io.File;
import java.io.FileOutputStream;
import java.io.InputStream;
import java.text.SimpleDateFormat;
import java.util.Date;

import org.apache.log4j.ConsoleAppender;
import org.apache.log4j.Level;
import org.apache.log4j.LogManager;
import org.apache.log4j.Logger;
import org.apache.log4j.PatternLayout;

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

/**
 * @author las
 * 
 *         Web service for reaching documents informations, and binary from blueline (OTTO)
 */

public class OttoArchiveWeb {
	private static String connectionString;
	private static ISerClassFactory factory;
	private static IDocumentServer server;
	private static ISystem system;
	private static ISession session;
	private static ITicket ticket;
	private static Logger logger = LogManager.getLogger("OttoArchiveWeb");

	public OttoArchiveWeb() {
		
	}

	/**
	 * If you want to see the blueline log messages, you should call this method
	 * after the factory instance created
	 */
	public static void initializeLogger() {
		LogManager.resetConfiguration();
		logger = LogManager.getLogger("OttoArchiveWeb");
		logger.addAppender(
				new ConsoleAppender(new PatternLayout()));
		logger.setLevel(Level.DEBUG);
		
		LogManager.getLogger("blueline").addAppender(
				new ConsoleAppender(new PatternLayout()));
		LogManager.getLogger("blueline").setLevel(Level.DEBUG);
	}

	/**
	 * It initalizes the server, and authenticates the user. IMPORTANT: if you
	 * call this, you should call closeServer in a try - finally block
	 */
	void initServer(String connection) {
		try {
			logger.info("Init server start...");

			// Initializing factory
			if (factory == null)
				factory = de.serac.bluelineimpl.SERACClassFactory.getInstance();

			initializeLogger();

			// if(server == null) server = factory.getDocumentServerInstance(new
			// BufferedInputStream(getClass().getResourceAsStream("/BL.ini")));

			// Initializing documentserver
			if (server == null || connectionString != connection)
			{
				connectionString = connection;
				server = factory.getDocumentServerInstance(connection);
			}	

			// Initializing system
			if (system == null)
				system = server.getSystem("SYSTEM1");

			// Initialize session, and login data
			if (session == null || !session.isValid()) {
				ticket = server.login(system, "fkt_viewbox",
						"?2HE?3jI".toCharArray());
				session = server.createSession(ticket);
			}

			logger.info("Init server successfull...");
		} catch (Exception e) {
			logger.error("Error while inicializing server", e);
		}
	}

	/**
	 * It closes the session, and the server. If you do not call this after
	 * calling InitServer, after a while, you will get error messages like
	 * "Ticket is not valid"
	 */
	void closeServer() {
		try {
			if (session != null)
				server.logout(session);
			if (server == null)
				server.close();
		} catch (Exception e) {
			logger.error("Error while closing server", e);
		}
	}

	/**
	 * Method for testing web service (without calling blueline stuff)
	 * 
	 * @return sample message
	 */
	public String sayHello() {
		logger.info("Method sayHello()");
		return "HELLO!";
	}

	/**
	 * Returns the documents, filtered by search string
	 * 
	 * @param search
	 *            the search string
	 * @return the count
	 */
	public long getCount(String connection, String search) {
		logger.info("Method getCount() start. Parameters: connection = " + connection + ", search = " + search);
		initServer(connection);
		try {
			// It returns the descriptors (accessible for user)
			IDescriptor[] descriptors = server.getDescriptors(session);

			// prepare the search string
			String[] searchArray = new String[1];
			searchArray[0] = search;

			// Create a query expression for descriptors

			IQueryExpression query = null;
			// The server.getDescriptors should return the accessible
			// descriptors, but i think, it returns all the descriptors,
			// so for test usage, we only process the first descriptor. If this
			// problem solved the i < 1 should change to i < descriptors.length
			for (int i = 0; i < 1; i++) {
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

			// We should give date filter for the query. Start date should not
			// be less than 1970..
			SimpleDateFormat sdf = new SimpleDateFormat("yyyy/MM/dd");
			Date from = sdf.parse("1970/01/01");
			Date to = java.util.Calendar.getInstance().getTime();

			// Creating the queryParameter from the expression
			IQueryParameter queryParameter = factory.getQueryParameterInstance(
					session.getDatabaseNames(), query, from, to);

			// Run the query
			IDocumentHitList hitlist = server.query(queryParameter, session);
			long count = hitlist != null ? hitlist.getTotalHitCount() : 0;
			logger.info("Method getCount() end. Parameters: connection = " + connection + ", search = " + search);
			return count;
		} catch (Exception e) {
			logger.error("Error in getCount() Parameters: connection = " + connection + ", search = " + search,
					e);
		} finally {
			closeServer();
		}
		return 0;
	}

	/**
	 * It downloads the file from OTTO, and puts to the specified directory
	 * 
	 * @param id
	 *            document`s id
	 * @param userId
	 *            viewbox.user.id
	 * @return the path, where the document located
	 */
	public String getDocumentPath(String connection, String id, String userId) {
		logger.info("Method getDocumentPath() start. Parameters: connection = " + connection + ", id = " + id
				+ ", userId = " + userId);
		String path = "C:\\inetpub\\temp\\viewbox\\" + userId + "\\" + id
				+ ".tif";
		initServer(connection);
		try {
			// Get the document
			IDocument document = server.getDocument4ID(id, session);

			// Get the document filter
			IDocumentFilter filter = server
					.getDocumentFilter(IDocumentFilter.TIFF_FILTER);
			try {
				int defaultRepresentation = document.getDefaultRepresentation();
				for (int i = 0; i < document
						.getPartDocumentCount(defaultRepresentation); i++) {
					filter.appendDocPart(
							document.getPartDocument(defaultRepresentation, i),
							session, true);
				}

				// Get the document stream
				InputStream stream = filter.getDocumentInputStream();
				try {
					File file = new File(path);
					file.mkdirs();
					// Write the document stream to file system
					FileOutputStream output = new FileOutputStream(file);
					try {
						int len;
						while ((len = stream.read()) > 0) {
							output.write(len);
						}
					} finally {
						output.close();
					}
				} finally {
					stream.close();
				}
			} finally {
				filter.close();
			}
			logger.info("Method getDocumentPath() end. Parameters: connection = " + connection + ", id = " + id
					+ ", userId = " + userId);
			return path;
		} catch (Exception e) {
			logger.error("Error in getDocumentPath. Parameters: connection = " + connection + ", id = " + id
					+ ", userId = " + userId, e);
		} finally {
			closeServer();
		}
		return null;
	}

	/**
	 * It collects information about the documents
	 * 
	 * @param search
	 *            the search string
	 * @return information about the documents
	 */
	public String getDocuments(String connection, String search) {
		logger.info("Method getDocuments() start. Parameters: connection = " + connection + ", search = "
				+ search);

		initServer(connection);
		try {
			// It returns the descriptors (accessible for user)
			IDescriptor[] descriptors = server.getDescriptors(session);

			// prepare the search string
			String[] searchArray = new String[1];
			searchArray[0] = search;

			// Create a query expression for descriptors

			IQueryExpression query = null;
			// The server.getDescriptors should return the accessible
			// descriptors, but i think, it returns all the descriptors,
			// so for test usage, we only process the first descriptor. If this
			// problem solved the i < 1 should change to i < descriptors.length
			for (int i = 0; i < 1; i++) {
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

			// We should give date filter for the query. Start date should not
			// be less than 1970..
			SimpleDateFormat sdf = new SimpleDateFormat("yyyy/MM/dd");
			Date fromDate = sdf.parse("1970/01/01");
			Date toDate = java.util.Calendar.getInstance().getTime();

			// Creating the queryParameter from the expression
			IQueryParameter queryParameter = factory.getQueryParameterInstance(
					session.getDatabaseNames(), query, fromDate, toDate);

			// Run the query
			IDocumentHitList hitlist = server.query(queryParameter, session);

			String documentsString = "";

			// Iterate through the document objects, and collect the information
			if (hitlist != null) {
				IDocument[] documents = hitlist.getDocumentObjects();
				for (int i = 0; i < documents.length; i++) {
					IDocument document = documents[i];
					documentsString += document.getDocumentID().getID()
							+ "<<DIV>>";
					documentsString += document.getDisplayName() + "<<DIV>>";
					documentsString += document.getDisplayType() + "<<DIV>>";
					documentsString += "<<EOL>>";
				}
			}
			logger.info("Method getDocuments() end. Parameters: connection = " + connection + ", search = "
					+ search);
			return documentsString;
		} catch (Exception e) {
			logger.error("Error in getDocuments. Parameters: connection = " + connection + ", search = "
					+ search, e);
		} finally {
			closeServer();
		}
		return null;
	}
}
