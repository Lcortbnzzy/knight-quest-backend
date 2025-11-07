import { cleanEnv, num, str } from 'envalid'
import * as process from 'node:process'
import 'dotenv/config';

export const env = cleanEnv(process.env, {
    NODE_ENV: str({ choices: ['development', 'production', 'test'], default: 'development' }),
    PORT: num({ default: 8000 }),
    JWT_SECRET: str({ devDefault: '8K7x2PqR9mN3vL6wT4yU5jH8bC1dF0gA2eS9xZ7qM4n' }), // ✅ CHANGED THIS LINE
    DATABASE_URL: str(),
    GOOGLE_CLIENT_ID: str({ devDefault: 'YOUR_GOOGLE_CLIENT_ID' }),
    GOOGLE_CLIENT_SECRET: str({ devDefault: 'YOUR_GOOGLE_CLIENT_SECRET' }),
    GOOGLE_REDIRECT_URI: str({ devDefault: 'http://localhost:8000/auth/google/callback' })
});