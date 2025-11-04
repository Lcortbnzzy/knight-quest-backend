import { env } from '@utils/env'
import type { Request } from 'express'
import { AuthorizationCode, ModuleOptions } from 'simple-oauth2'

const config: ModuleOptions<'client_id'> = {
    client: {
        id: env.GOOGLE_CLIENT_ID,
        secret: env.GOOGLE_CLIENT_SECRET
    },
    auth: {
        tokenHost: 'https://oauth2.googleapis.com',
        tokenPath: '/token',
        authorizeHost: 'https://accounts.google.com',
        authorizePath: '/o/oauth2/v2/auth'
    },
    options: {
        authorizationMethod: 'body'
    }
}

export const client = new AuthorizationCode(config)

export function getAuthorizationUrl(req: Request, state?: string) {
    const protocol = req.protocol
    const host = req.get('host')

    if (!host) {
        return null
    }

    const redirectUri = env.GOOGLE_REDIRECT_URI

    return client.authorizeURL({
        redirect_uri: redirectUri,
        scope: 'openid email profile',
        state
    })
}

export async function getToken(code: string) {
    const tokenParams = {
        code,
        redirect_uri: env.GOOGLE_REDIRECT_URI
    }
    const accessToken = await client.getToken(tokenParams)
    return accessToken.token as any
}



