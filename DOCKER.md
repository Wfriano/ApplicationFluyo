# Docker Setup para FluyoV2

## Requisitos previos
- [Docker](https://www.docker.com/products/docker-desktop) instalado
- [Docker Compose](https://docs.docker.com/compose/install/) instalado

## Construcción y ejecución

### Opción 1: Usando Docker Compose (Recomendado)

```bash
# Construir y ejecutar la aplicación con MongoDB
docker-compose up -d

# Visualizar logs
docker-compose logs -f fluyov2

# Detener los servicios
docker-compose down
```

La aplicación estará disponible en: **http://localhost:8080**

MongoDB estará disponible en: **mongodb://admin:password@localhost:27017**

### Opción 2: Construir y ejecutar manualmente

```bash
# Construir la imagen
docker build -t fluyo-api:latest .

# Ejecutar con MongoDB local o remoto
docker run -d \
  --name fluyo-api \
  -p 8080:8080 \
  -e "MongoDbSettings__ConnectionString=mongodb://your-connection-string" \
  fluyo-api:latest
```

## Configuración de variables de entorno

Puedes configurar las siguientes variables en `docker-compose.yml`:

- `ASPNETCORE_ENVIRONMENT`: Development o Production
- `MongoDbSettings__ConnectionString`: Cadena de conexión a MongoDB
- `MongoDbSettings__DatabaseName`: Nombre de la base de datos
- `JwtSettings__SecretKey`: Clave secreta JWT
- `JwtSettings__Issuer`: Emisor del JWT
- `JwtSettings__Audience`: Audiencia del JWT

## Verificar la salud de la aplicación

```bash
curl http://localhost:8080/health
```

## Limpiar contenedores e imágenes

```bash
# Detener y eliminar contenedores
docker-compose down

# Eliminar volúmenes (datos de MongoDB)
docker-compose down -v

# Eliminar la imagen
docker rmi fluyo-api:latest
```

## Troubleshooting

### La aplicación no puede conectar a MongoDB
- Verifica que MongoDB está corriendo: `docker ps`
- Verifica la cadena de conexión en las variables de entorno
- Revisa los logs: `docker-compose logs mongodb`

### Puerto 8080 ya está en uso
- Cambia el puerto en `docker-compose.yml` o ejecuta:
```bash
docker run -p 9000:8080 fluyo-api:latest
```

### Reconstruir sin caché
```bash
docker-compose build --no-cache
```
