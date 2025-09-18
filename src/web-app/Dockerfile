# Stage 1: Build
FROM node:20-alpine AS build

WORKDIR /app

# Copy package.json và package-lock.json
COPY web-app/package*.json ./

RUN npm install

# Copy toàn bộ source
COPY web-app ./

# Build Vite
RUN npm run build

# Stage 2: Nginx
FROM nginx:stable-alpine

# Copy build output vào Nginx
COPY --from=build /app/dist /usr/share/nginx/html

# Nếu có React Router SPA → thêm config nginx
COPY web-app/nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
