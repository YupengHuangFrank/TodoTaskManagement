import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useNavigate } from 'react-router-dom'
import { login as apiLogin, signup as apiSignup, logout as apiLogout } from '../api/auth'

export function useAuth() {
    const navigate = useNavigate()

    const login = useMutation({
        mutationFn: ({ email, password }: { email: string; password: string }) =>
            apiLogin(email, password),
        onSuccess: () => navigate('/'),
    })

    const signup = useMutation({
        mutationFn: ({ email, password }: { email: string; password: string }) =>
            apiSignup(email, password),
        onSuccess: () => navigate('/'),
    })

    const qc = useQueryClient()

    const logout = useMutation({
        mutationFn: apiLogout,
        onSuccess: () => {
            qc.clear()
            navigate('/login')
        },
    })

    return { login, signup, logout }
}
