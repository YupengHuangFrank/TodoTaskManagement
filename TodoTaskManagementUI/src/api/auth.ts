import { apiFetch } from './client'

async function extractError(res: Response): Promise<string> {
    const text = await res.text().catch(() => '')
    try {
        const json = JSON.parse(text)
        return json.message ?? json.title ?? json.error ?? text
    } catch {
        return text || `Request failed (${res.status})`
    }
}

export async function signup(email: string, password: string): Promise<void> {
    const res = await apiFetch('/api/auth/signup', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email, password }),
    })
    if (!res.ok) throw new Error(await extractError(res))
}

export async function login(email: string, password: string): Promise<void> {
    const res = await apiFetch('/api/auth/login', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email, password }),
    })
    if (!res.ok) throw new Error(await extractError(res))
}

export async function logout(): Promise<void> {
    await apiFetch('/api/auth/logout', { method: 'POST' })
}
