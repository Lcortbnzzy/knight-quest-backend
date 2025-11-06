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
        headers: { Authorization: `Bearer ${accessToken}` }
    })

    if (!resp.ok) {
        throw new Error(`Failed to fetch userinfo: ${resp.status} ${await resp.text()}`)
    }

    return await resp.json() as GoogleUserInfo
}

export const GET: Handler = async (req, res) => {
    const { code, state } = req.query as { code?: string, state?: string }

    if (!code) {
        return res.send(`
            <!DOCTYPE html>
            <html>
            <head>
                <meta name="viewport" content="width=device-width, initial-scale=1">
                <title>Error</title>
            </head>
            <body style="font-family: sans-serif; text-align: center; padding: 50px;">
                <h2>Authentication Error</h2>
                <p>Missing authorization code.</p>
                <a href="knightquest://login?error=missing_code" style="display: inline-block; padding: 12px 24px; background: #db4437; color: white; text-decoration: none; border-radius: 4px; margin-top: 20px;">
                    Return to App
                </a>
            </body>
            </html>
        `)
    }

    try {
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
            return res.send(`
                <!DOCTYPE html>
                <html>
                <head>
                    <meta name="viewport" content="width=device-width, initial-scale=1">
                    <title>Error</title>
                </head>
                <body style="font-family: sans-serif; text-align: center; padding: 50px;">
                    <h2>Authentication Error</h2>
                    <p>Failed to generate token.</p>
                    <a href="knightquest://login?error=token_issue" style="display: inline-block; padding: 12px 24px; background: #db4437; color: white; text-decoration: none; border-radius: 4px; margin-top: 20px;">
                        Return to App
                    </a>
                </body>
                </html>
            `)
        }

        const qs = new URLSearchParams({ token: jwt })
        if (state) qs.set('state', state)

        const deeplink = `knightquest://login?${qs.toString()}`

        // Return HTML page with auto-redirect + manual fallback button
        return res.send(`
            <!DOCTYPE html>
            <html>
            <head>
                <meta name="viewport" content="width=device-width, initial-scale=1">
                <title>Success - Redirecting...</title>
                <style>
                    body {
                        font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif;
                        text-align: center;
                        padding: 50px 20px;
                        background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
                        color: white;
                        min-height: 100vh;
                        display: flex;
                        flex-direction: column;
                        justify-content: center;
                        align-items: center;
                    }
                    .container {
                        background: white;
                        color: #333;
                        padding: 40px;
                        border-radius: 12px;
                        box-shadow: 0 10px 40px rgba(0,0,0,0.2);
                        max-width: 400px;
                    }
                    h2 {
                        color: #4285f4;
                        margin-bottom: 10px;
                    }
                    .spinner {
                        border: 4px solid #f3f3f3;
                        border-top: 4px solid #4285f4;
                        border-radius: 50%;
                        width: 40px;
                        height: 40px;
                        animation: spin 1s linear infinite;
                        margin: 20px auto;
                    }
                    @keyframes spin {
                        0% { transform: rotate(0deg); }
                        100% { transform: rotate(360deg); }
                    }
                    .btn {
                        display: inline-block;
                        padding: 14px 28px;
                        background: #4285f4;
                        color: white;
                        text-decoration: none;
                        border-radius: 6px;
                        margin-top: 20px;
                        font-weight: 600;
                        transition: background 0.3s;
                    }
                    .btn:hover {
                        background: #357ae8;
                    }
                    .note {
                        font-size: 14px;
                        color: #666;
                        margin-top: 15px;
                    }
                </style>
            </head>
            <body>
                <div class="container">
                    <h2>✅ Authentication Successful!</h2>
                    <div class="spinner"></div>
                    <p>Redirecting back to Knight Quest...</p>
                    <a href="${deeplink}" class="btn" id="manualBtn" style="display: none;">
                        🎮 Open Knight Quest
                    </a>
                    <p class="note" id="note" style="display: none;">
                        Tap the button above if you're not redirected automatically.
                    </p>
                </div>
                <script>
                    // Multiple redirect attempts
                    function attemptRedirect() {
                        try {
                            // Method 1: Direct assignment
                            window.location.href = '${deeplink}';
                            
                            // Method 2: Replace (after delay)
                            setTimeout(() => {
                                window.location.replace('${deeplink}');
                            }, 100);
                            
                            // Method 3: Try to open in new context
                            setTimeout(() => {
                                window.open('${deeplink}', '_self');
                            }, 200);
                            
                            // Show manual button after 2 seconds if still here
                            setTimeout(() => {
                                document.getElementById('manualBtn').style.display = 'inline-block';
                                document.getElementById('note').style.display = 'block';
                            }, 2000);
                        } catch (e) {
                            console.error('Redirect failed:', e);
                            // Show button immediately on error
                            document.getElementById('manualBtn').style.display = 'inline-block';
                            document.getElementById('note').style.display = 'block';
                        }
                    }
                    
                    // Start redirect attempts when page loads
                    if (document.readyState === 'loading') {
                        document.addEventListener('DOMContentLoaded', attemptRedirect);
                    } else {
                        attemptRedirect();
                    }
                </script>
            </body>
            </html>
        `)
        
    } catch (error) {
        console.error('Google OAuth callback error:', error)
        return res.send(`
            <!DOCTYPE html>
            <html>
            <head>
                <meta name="viewport" content="width=device-width, initial-scale=1">
                <title>Error</title>
            </head>
            <body style="font-family: sans-serif; text-align: center; padding: 50px;">
                <h2>Authentication Failed</h2>
                <p>An error occurred during authentication.</p>
                <a href="knightquest://login?error=auth_failed" style="display: inline-block; padding: 12px 24px; background: #db4437; color: white; text-decoration: none; border-radius: 4px; margin-top: 20px;">
                    Return to App
                </a>
            </body>
            </html>
        `)
    }
}