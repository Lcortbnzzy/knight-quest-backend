import type { Handler } from 'express'
import { prisma } from '@utils/database'
import { Role } from '@prisma/client'
import { getToken } from '@utils/oauth/google'

type GoogleUserInfo = {
    sub: string
    email: string
    email_verified: boolean
    given_name?: string
    family_name?: string
    name?: string
    picture?: string
}

async function fetchGoogleUser(accessToken: string): Promise<GoogleUserInfo> {
    const resp = await fetch('https://openidconnect.googleapis.com/v1/userinfo', {
        headers: { Authorization: `Bearer ${ accessToken }` }
    })

    if (!resp.ok) {
        throw new Error(`Failed to fetch userinfo: ${ resp.status } ${ await resp.text() }`)
    }

    return await resp.json() as GoogleUserInfo
}

export const get: Handler = async (req, res) => {
    const { code, state } = req.query as { code?: string, state?: string }

    if (!code) {
        return res.redirect('knightquest://login/?error=missing_code')
    }

    const tokens = await getToken(code)
    const userinfo = await fetchGoogleUser(tokens.access_token)

    const username = userinfo.email
    const firstName = userinfo.given_name || (userinfo.name?.split(' ')[0] ?? 'Google')
    const lastName = userinfo.family_name || (userinfo.name?.split(' ').slice(1).join(' ') || 'User')

    const user = await prisma.user.upsert({
        where: { username },
        create: {
            username,
            password: 'google-oauth-no-password',
            firstName,
            lastName,
            role: Role.Student,
            save: { create: {} }
        },
        update: {
            firstName,
            lastName
        },
        omit: { password: true, createdAt: true, updatedAt: true }
    })

    const jwt = await prisma.user.generateJwt(user.id)

    if (!jwt) {
        return res.redirect('knightquest://login/?error=token_issue')
    }

    const qs = new URLSearchParams({ token: jwt })
    if (state) qs.set('state', state)

    qs.set('token', jwt)

    return res.redirect(`knightquest://login/?${ qs.toString() }`)
}


