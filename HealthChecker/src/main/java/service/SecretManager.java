
package service;

import com.google.cloud.secretmanager.v1.SecretManagerServiceClient;
import com.google.cloud.secretmanager.v1.SecretPayload;
import com.google.cloud.secretmanager.v1.SecretVersionName;

public class SecretManager {

    public String getSecret(String secretName) {
        try (SecretManagerServiceClient client = SecretManagerServiceClient.create()) {
            SecretVersionName secretVersionName = SecretVersionName.of("inrio-401514", secretName, "latest");
            SecretPayload payload = client.accessSecretVersion(secretVersionName).getPayload();
            return payload.getData().toStringUtf8();
        } catch (Exception e) {
            // Handle exception
            return null;
        }
    }
}
