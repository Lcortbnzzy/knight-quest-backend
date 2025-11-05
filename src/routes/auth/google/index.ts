import type { Handler } from 'express'
import { getAuthorizationUrl } from '@utils/oauth/google'

// ✅ Change to uppercase GET
export const GET: Handler = (req, res) => {
    const state = (req.query.state as string) || undefined
    const url = getAuthorizationUrl(req, state)

    if (!url) return res.error({ message: 'No state returned' })

    return res.redirect(url)
}