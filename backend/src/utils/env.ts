import { cleanEnv, num, str, testOnly } from 'envalid'
import * as process from 'node:process'
import 'dotenv/config';

export const env = cleanEnv(process.env, {
    NODE_ENV: str({ choices: ['development', 'production', 'test'], default: 'development' }),
    PORT: num({ default: 8000 }),
    JWT_SECRET: str({ devDefault: testOnly('') }),
    DATABASE_URL: str(),
    GOOGLE_CLIENT_ID: str({ devDefault: testOnly('YOUR_GOOGLE_CLIENT_ID') }),
    GOOGLE_CLIENT_SECRET: str({ devDefault: testOnly('YOUR_GOOGLE_CLIENT_SECRET') }),
    GOOGLE_REDIRECT_URI: str({ devDefault: testOnly('http://localhost:8000/auth/google/callback') })
});