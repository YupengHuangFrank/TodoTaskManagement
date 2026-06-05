let refreshPromise: Promise<void> | null = null

async function doRefresh(): Promise<void> {
    const res = await fetch('/api/auth/refresh', {
        method: 'POST',
        credentials: 'include',
    })
    if (!res.ok) throw new Error('refresh_failed')
}

export async function apiFetch(path: string, init?: RequestInit): Promise<Response> {
    const res = await fetch(path, { ...init, credentials: 'include' })

    if (res.status !== 401) return res

    // First 401: attempt a single refresh, queue any concurrent callers behind it
    if (!refreshPromise) {
        refreshPromise = doRefresh().finally(() => { refreshPromise = null })
    }

    try {
        await refreshPromise
    } catch {
        // Refresh failed — redirect to login; caller will never receive a response
        window.location.href = '/login'
        return res
    }

    // Retry original request with fresh cookie
    return fetch(path, { ...init, credentials: 'include' })
}
