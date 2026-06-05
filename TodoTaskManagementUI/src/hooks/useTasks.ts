import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import toast from 'react-hot-toast'
import {
    getActive,
    getArchived,
    createTask,
    updateTask,
    deleteTask,
    archiveAll,
} from '../api/tasks'
import { TaskStatus } from '../types/task'
import type { TaskApi } from '../types/task'

const ACTIVE_KEY = ['tasks', 'active'] as const
const ARCHIVED_KEY = ['tasks', 'archived'] as const

export function useActiveTasks() {
    return useQuery({ queryKey: ACTIVE_KEY, queryFn: getActive })
}

export function useArchivedTasks() {
    return useQuery({ queryKey: ARCHIVED_KEY, queryFn: getArchived })
}

export function useCreateTask() {
    const qc = useQueryClient()
    return useMutation({
        mutationFn: (body: TaskApi) => createTask(body),
        onMutate: async (newTask) => {
            await qc.cancelQueries({ queryKey: ACTIVE_KEY })
            const prev = qc.getQueryData<TaskApi[]>(ACTIVE_KEY)
            qc.setQueryData<TaskApi[]>(ACTIVE_KEY, old => [
                { ...newTask, id: '__temp__' },
                ...(old ?? []),
            ])
            return { prev }
        },
        onError: (_err, _vars, ctx) => {
            qc.setQueryData(ACTIVE_KEY, ctx?.prev)
        },
        onSettled: () => {
            qc.invalidateQueries({ queryKey: ACTIVE_KEY })
        },
    })
}

export function useUpdateTask() {
    const qc = useQueryClient()
    return useMutation({
        mutationFn: ({ id, body }: { id: string; body: TaskApi }) => updateTask(id, body),
        onMutate: async ({ id, body }) => {
            await qc.cancelQueries({ queryKey: ACTIVE_KEY })
            const prev = qc.getQueryData<TaskApi[]>(ACTIVE_KEY)
            qc.setQueryData<TaskApi[]>(ACTIVE_KEY, old =>
                old?.map(t => (t.id === id ? { ...t, ...body } : t)) ?? []
            )
            return { prev }
        },
        onError: (_err, _vars, ctx) => {
            qc.setQueryData(ACTIVE_KEY, ctx?.prev)
        },
        onSettled: () => {
            qc.invalidateQueries({ queryKey: ACTIVE_KEY })
        },
    })
}

export function useDeleteTask() {
    const qc = useQueryClient()
    return useMutation({
        mutationFn: (id: string) => deleteTask(id),
        onMutate: async (id) => {
            await qc.cancelQueries({ queryKey: ACTIVE_KEY })
            const prev = qc.getQueryData<TaskApi[]>(ACTIVE_KEY)
            qc.setQueryData<TaskApi[]>(ACTIVE_KEY, old =>
                old?.filter(t => t.id !== id) ?? []
            )
            return { prev }
        },
        onError: (_err, _vars, ctx) => {
            qc.setQueryData(ACTIVE_KEY, ctx?.prev)
        },
        onSettled: () => {
            qc.invalidateQueries({ queryKey: ACTIVE_KEY })
        },
    })
}

export function useDeleteArchivedTask() {
    const qc = useQueryClient()
    return useMutation({
        mutationFn: (id: string) => deleteTask(id),
        onMutate: async (id) => {
            await qc.cancelQueries({ queryKey: ARCHIVED_KEY })
            const prev = qc.getQueryData<TaskApi[]>(ARCHIVED_KEY)
            qc.setQueryData<TaskApi[]>(ARCHIVED_KEY, old =>
                old?.filter(t => t.id !== id) ?? []
            )
            return { prev }
        },
        onError: (_err, _vars, ctx) => {
            qc.setQueryData(ARCHIVED_KEY, ctx?.prev)
        },
        onSettled: () => {
            qc.invalidateQueries({ queryKey: ARCHIVED_KEY })
        },
    })
}

export function useMoveTask() {
    const qc = useQueryClient()
    return useMutation({
        mutationFn: ({ id, body }: { id: string; body: TaskApi }) => updateTask(id, body),
        onMutate: async ({ id, body }) => {
            await qc.cancelQueries({ queryKey: ACTIVE_KEY })
            const prev = qc.getQueryData<TaskApi[]>(ACTIVE_KEY)
            qc.setQueryData<TaskApi[]>(ACTIVE_KEY, old =>
                old?.map(t => (t.id === id ? { ...t, ...body } : t)) ?? []
            )
            return { prev }
        },
        onError: (_err, _vars, ctx) => {
            qc.setQueryData(ACTIVE_KEY, ctx?.prev)
            toast.error('Failed to move task — change reverted')
        },
        onSettled: () => {
            qc.invalidateQueries({ queryKey: ACTIVE_KEY })
        },
    })
}

export function useArchiveAll() {
    const qc = useQueryClient()
    return useMutation({
        mutationFn: archiveAll,
        onMutate: async () => {
            await qc.cancelQueries({ queryKey: ACTIVE_KEY })
            const prev = qc.getQueryData<TaskApi[]>(ACTIVE_KEY)
            qc.setQueryData<TaskApi[]>(ACTIVE_KEY, old =>
                old?.filter(t => t.status !== TaskStatus.Done) ?? []
            )
            return { prev }
        },
        onError: (_err, _vars, ctx) => {
            qc.setQueryData(ACTIVE_KEY, ctx?.prev)
        },
        onSettled: () => {
            qc.invalidateQueries({ queryKey: ACTIVE_KEY })
            qc.invalidateQueries({ queryKey: ARCHIVED_KEY })
        },
    })
}
