FROM node:16-alpine

run apk add dumb-init

USER node

WORKDIR /backend

# Copie du fichier package
COPY ./package.json ./package.json

# Installer les dépendances
RUN npm install

# Copie du code source
COPY . .

# Exposer le port 80
EXPOSE 8080

# Démarrage du serveur
CMD ["node", "server.js"]

