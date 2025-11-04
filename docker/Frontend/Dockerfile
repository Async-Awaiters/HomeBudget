# ---------- СТАДИЯ 1: Сборка (Vue CLI) ----------
FROM node:18-alpine AS build

WORKDIR /app

# Устанавливаем git для клонирования репозитория
RUN apk add --no-cache git

# Клонируем проект фронтенда
RUN git clone https://github.com/Async-Awaiters/HomeBudgetFrontend.git .

# Добавляем переменные окружения для Vue CLI (будут видны в npm run build)
ARG VUE_APP_AUTH_URL
ARG VUE_APP_DIC_URL
ARG VUE_APP_ACC_URL

ENV VUE_APP_AUTH_URL=$VUE_APP_AUTH_URL
ENV VUE_APP_DIC_URL=$VUE_APP_DIC_URL
ENV VUE_APP_ACC_URL=$VUE_APP_ACC_URL

# Устанавливаем зависимости и собираем
RUN npm ci
RUN npm run build

# ---------- СТАДИЯ 2: Продакшн через Nginx ----------
FROM nginx:alpine

# Копируем сборку Vue CLI (dist/)
COPY --from=build /app/dist /usr/share/nginx/html

# Копируем favicon
COPY favicon.ico /usr/share/nginx/html/favicon.ico

# Копируем кастомный nginx.conf, чтобы исправить 404 при обновлении страниц
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Включаем порт
EXPOSE 80

# Запускаем nginx
CMD ["nginx", "-g", "daemon off;"]

