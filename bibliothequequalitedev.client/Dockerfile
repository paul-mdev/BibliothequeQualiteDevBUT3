FROM node:20-alpine

WORKDIR /app

COPY package*.json ./
RUN npm install

COPY . .

EXPOSE 5173
ENV DOCKER=true
CMD ["npm", "run", "dev", "--", "--host"]
