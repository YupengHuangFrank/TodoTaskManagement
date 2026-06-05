export const TaskStatus = {
    Todo: 0,
    InProgress: 1,
    Done: 2,
} as const
export type TaskStatus = typeof TaskStatus[keyof typeof TaskStatus]

export interface TaskApi {
    id?: string
    title?: string | null
    description?: string | null
    dueDate?: string | null
    status?: TaskStatus | null
    isArchived?: boolean | null
    createdAt?: string | null
}
