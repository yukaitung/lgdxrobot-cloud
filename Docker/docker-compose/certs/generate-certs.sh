#!/bin/bash

# Generate Root CA and convert to pfx
openssl req -newkey rsa:4096 -keyout rootCA.key -out rootCA.csr -config rootCA.conf -nodes 
openssl x509 -req -days 9999 -extfile rootCA.conf -extensions v3_ca -in rootCA.csr -signkey rootCA.key -out rootCA.crt
openssl pkcs12 -export -out rootCA.pfx -inkey rootCA.key -in rootCA.crt -passout pass:""

# Generate Certicicate for gRPC
openssl req -newkey rsa:4096 -keyout grpc.key -out grpc.csr -config grpc.conf -nodes 
openssl x509 -req -in grpc.csr -CA rootCA.crt -CAkey rootCA.key -CAcreateserial -out grpc.crt -days 9999 -sha256 -extfile grpc.conf -extensions req_ext
openssl pkcs12 -export -out grpc.pfx -inkey grpc.key -in grpc.crt -certfile rootCA.crt -passout pass:""

# Generate Certificate for SSL Connection
openssl req -newkey rsa:4096 -keyout app.key -out app.csr -config app.conf -nodes 
openssl x509 -req -in app.csr -CA rootCA.crt -CAkey rootCA.key -CAcreateserial -out app.crt -days 9999 -sha256 -extfile app.conf -extensions req_ext
openssl pkcs12 -export -out app.pfx -inkey app.key -in app.crt -certfile rootCA.crt -passout pass:""

# Generate Certificate for UI Authentication
openssl req -newkey rsa:4096 -keyout ui.key -out ui.csr -config ui.conf -nodes 
openssl x509 -req -in ui.csr -CA rootCA.crt -CAkey rootCA.key -CAcreateserial -out ui.crt -days 9999 -sha256 -extfile ui.conf -extensions req_ext
openssl pkcs12 -export -out ui.pfx -inkey ui.key -in ui.crt -certfile rootCA.crt -passout pass:""

# Generate Certificate for Redis Server
openssl req -newkey rsa:4096 -keyout redis_server.key -out redis_server.csr -config redis_server.conf -nodes 
openssl x509 -req -in redis_server.csr -CA rootCA.crt -CAkey rootCA.key -CAcreateserial -out redis_server.crt -days 9999 -sha256 -extfile redis_server.conf -extensions req_ext
openssl pkcs12 -export -out redis_server.pfx -inkey redis_server.key -in redis_server.crt -certfile rootCA.crt -passout pass:""

# Generate Certificate for Redis Client
openssl req -newkey rsa:4096 -keyout redis_client.key -out redis_client.csr -config redis_client.conf -nodes 
openssl x509 -req -in redis_client.csr -CA rootCA.crt -CAkey rootCA.key -CAcreateserial -out redis_client.crt -days 9999 -sha256 -extfile redis_client.conf -extensions req_ext
openssl pkcs12 -export -out redis_client.pfx -inkey redis_client.key -in redis_client.crt -certfile rootCA.crt -passout pass:""

echo "END"

echo ""

echo "Copy to appsettings.api.json -> InternalCertificateThumbprint"
openssl x509 -in ui.crt -noout -fingerprint -sha1 | sed 's/://g'

echo ""

echo "Copy to appsettings.api.json -> RootCertificateSN"
openssl x509 -in rootCA.crt -noout -serial

echo ""

echo "Copy to appsettings.ui.json -> CertificateSN"
openssl x509 -in ui.crt -noout -serial

echo ""

echo "Copy to appsettings.ui.json -> CertificateSN"
openssl x509 -in redis_client.crt -noout -serial
