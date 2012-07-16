package com.tridion.extension.adf;

import java.net.URI;
import java.net.URLDecoder;
import java.util.List;
import java.util.Map;
import javax.xml.parsers.DocumentBuilder;
import javax.xml.parsers.DocumentBuilderFactory;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import org.w3c.dom.Document;
import org.w3c.dom.Element;
import org.w3c.dom.NodeList;

import com.tridion.ambientdata.AmbientDataException;
import com.tridion.ambientdata.claimstore.ClaimStore;
import com.tridion.ambientdata.processing.AbstractClaimProcessor;
import com.tridion.ambientdata.web.WebClaims;
import com.tridion.broker.StorageException;
import com.tridion.storage.CustomerCharacteristic;
import com.tridion.storage.StorageManagerFactory;
import com.tridion.storage.StorageTypeMapping;
import com.tridion.storage.TrackingKey;
import com.tridion.storage.User;
import com.tridion.storage.dao.PersonalizationDAO;

public class PnPClaimProcessor extends AbstractClaimProcessor {
	private static final Logger log = LoggerFactory.getLogger(PnPClaimProcessor.class);
	private String CONFIG_FILE = "pnp_processor_conf.xml";
	private Document config = null;
	private String cookieName = new String();
	private PersonalizationDAO personalizationDAO;
	
	private final String CHARACTERISTICS_CLAIM_URI = "taf:claim:pnp:characteristics:";
	private final String TRACKINGKEYS_CLAIM_URI = "taf:claim:pnp:trackingkeys:";
	
	public PnPClaimProcessor() {
		log.debug("PnPClaimProcessor :: Called");
		readConfig();
		this.cookieName = getConfigValue("cookie_name");
		
		try {
			this.personalizationDAO = ((PersonalizationDAO)StorageManagerFactory.getDefaultDAO(StorageTypeMapping.PERSONALIZATION));
		} catch (StorageException e) {
			log.error(e.getMessage(), e);
		}
	}

	@SuppressWarnings("rawtypes")
	@Override
	public void onRequestEnd(ClaimStore claimStore) throws AmbientDataException {
		log.debug("PnPClaimProcessor.onRequestEnd :: Called");
		boolean ccSet = false;
		boolean tkSet = false;
		Map cookies = (Map)claimStore.get(WebClaims.REQUEST_COOKIES);
		
		if (!this.cookieName.equals("")) {
			int userId = Integer.parseInt(cookies.get(this.cookieName).toString());
		
			if (userId > 0) {
				log.debug("onRequestEnd :: Got User ID #" + userId);
				
				if (this.personalizationDAO != null) {
					String claimUri;
					URI uri;
					
					try {
						User user = this.personalizationDAO.getUserById(userId);
						List<CustomerCharacteristic> characteristics = this.personalizationDAO.findAllCustomerCharacteristicByUser(user);
						List<TrackingKey> trackingKeys = this.personalizationDAO.findAllTrackingKeyByUser(user);
						
						for (CustomerCharacteristic cc : characteristics) {
							claimUri = this.CHARACTERISTICS_CLAIM_URI + cc.getName();
							uri = URI.create(claimUri);
							claimStore.put(uri, cc.getValue());
							ccSet = true;
						}
						
						for (TrackingKey key : trackingKeys) {
							claimUri = this.TRACKINGKEYS_CLAIM_URI + key.getName();
							uri = URI.create(claimUri);
							claimStore.put(uri, key.getValue());
							tkSet = true;
						}
					} catch (StorageException e) {
						log.error(e.getMessage(), e);
					}
					
					claimUri = this.CHARACTERISTICS_CLAIM_URI + "set";
					uri = URI.create(claimUri);
					claimStore.put(uri, ccSet);
					
					claimUri = this.TRACKINGKEYS_CLAIM_URI + "set";
					uri = URI.create(claimUri);
					claimStore.put(uri, tkSet);
				}
			}
		}
	}
	
	private void readConfig() {
		DocumentBuilderFactory factory = DocumentBuilderFactory.newInstance();
		
		try {
			DocumentBuilder builder = factory.newDocumentBuilder();
			String path = PnPClaimProcessor.class.getProtectionDomain().getCodeSource().getLocation().getPath();
			String decodedPath = URLDecoder.decode(path, "UTF-8");
			decodedPath = decodedPath.substring(0, decodedPath.lastIndexOf("/") + 1);
			decodedPath = decodedPath.replace("/lib/", "/classes/");
			this.config = builder.parse(decodedPath + this.CONFIG_FILE);
		} catch (Exception e) {
			log.error(e.getMessage(), e);
		}
	}
	
	private String getConfigValue(String name) {
		String value = new String();
		
		if (this.config != null) {
			try {
				//get the root element
				Element root = this.config.getDocumentElement();
				NodeList nodes = root.getElementsByTagName(name);
				
				if (nodes.getLength() > 0){
					value = nodes.item(0).getTextContent();
				}
			} catch (Exception e) {
				log.error(e.getMessage(), e);
			}
		}
		
		return value;
	}
}
