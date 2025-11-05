# Stage 1: Build
FROM node:20-alpine AS builder
RUN apk add --no-cache libc6-compat openssl
RUN apk update
WORKDIR /app

# Install pnpm
RUN npm install -g pnpm

# Copy package files and workspace config
COPY package.json pnpm-lock.yaml pnpm-workspace.yaml ./

# Copy prisma folder (needed for prisma generate)
COPY prisma ./prisma

# Install dependencies
RUN pnpm install --frozen-lockfile

# Generate Prisma client
RUN pnpm prisma generate

# Copy source code
COPY . .

# Build the application
RUN pnpm run build

# Debug: Check if routes compiled
RUN ls -la dist
RUN ls -la dist/routes || echo "routes folder not found"

# Stage 2: Production
FROM node:20-alpine AS runner
RUN apk add --no-cache openssl
WORKDIR /app

# Create non-root user
RUN addgroup --system --gid 1001 nodejs
RUN adduser --system --uid 1001 expressjs

# Copy built application and necessary files
COPY --from=builder --chown=expressjs:nodejs /app/dist ./dist
COPY --from=builder --chown=expressjs:nodejs /app/node_modules ./node_modules
COPY --from=builder --chown=expressjs:nodejs /app/package.json ./package.json
COPY --from=builder --chown=expressjs:nodejs /app/prisma ./prisma

# Switch to non-root user
USER expressjs

# Expose port
EXPOSE 8000

# Set NODE_ENV
ENV NODE_ENV=production

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD node -e "require('http').get('http://localhost:8000/health', (r) => {process.exit(r.statusCode === 200 ? 0 : 1)})"

# Start the application
CMD ["node", "dist/index.js"]