import { apiFetch } from './client'
import type { TaskApi } from '../types/task'

export async function getActive(): Promise<TaskApi[]> {
    const res = await apiFetch('/api/tasks?archived=false')
    if (!res.ok) throw new Error(`Failed to load tasks (${res.status})`)
    return res.json()
}

export async function getArchived(): Promise<TaskApi[]> {
    const res = await apiFetch('/api/tasks?archived=true')
    if (!res.ok) throw new Error(`Failed to load archived tasks (${res.status})`)
    return res.json()
}

export async function createTask(body: TaskApi): Promise<TaskApi> {
    const res = await apiFetch('/api/tasks', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(body),
    })
    if (!res.ok) throw new Error(`Failed to create task (${res.status})`)
    return res.json()
}

export async function updateTask(id: string, body: TaskApi): Promise<TaskApi> {
    const res = await apiFetch(`/api/tasks/${id}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(body),
    })
    if (!res.ok) throw new Error(`Failed to update task (${res.status})`)
    return res.json()
}

export async function deleteTask(id: string): Promise<void> {
    const res = await apiFetch(`/api/tasks/${id}`, { method: 'DELETE' })
    if (!res.ok) throw new Error(`Failed to delete task (${res.status})`)
}

export async function archiveAll(): Promise<void> {
    const res = await apiFetch('/api/tasks/archive-all', { method: 'POST' })
    if (!res.ok) throw new Error(`Failed to archive tasks (${res.status})`)
}
