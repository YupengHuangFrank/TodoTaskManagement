import { useState } from 'react'
import { DndContext, PointerSensor, useSensor, useSensors, type DragEndEvent } from '@dnd-kit/core'
import { TaskStatus } from '../types/task'
import type { TaskApi } from '../types/task'
import { useActiveTasks, useCreateTask, useUpdateTask, useDeleteTask, useMoveTask, useArchiveAll } from '../hooks/useTasks'
import Column from '../components/Column'
import FloatingAddButton from '../components/FloatingAddButton'

export default function BoardPage() {
    const { data: tasks = [], isLoading, isError } = useActiveTasks()
    const createTask = useCreateTask()
    const updateTask = useUpdateTask()
    const deleteTask = useDeleteTask()
    const moveTask = useMoveTask()
    const archiveAll = useArchiveAll()

    const [showNewCard, setShowNewCard] = useState(false)

    const sensors = useSensors(
        useSensor(PointerSensor, { activationConstraint: { distance: 8 } }),
    )

    function handleSave(id: string | undefined, data: Partial<TaskApi>) {
        if (id === undefined) {
            createTask.mutate(
                { ...data, status: TaskStatus.Todo },
                { onSuccess: () => setShowNewCard(false) },
            )
        } else {
            const existing = tasks.find(t => t.id === id)
            if (existing) updateTask.mutate({ id, body: { ...existing, ...data } })
        }
    }

    function handleDelete(id: string) {
        deleteTask.mutate(id)
    }

    function handleDragEnd(event: DragEndEvent) {
        const { active, over } = event
        if (!over) return
        const taskId = active.id as string
        const newStatus = Number(over.id) as typeof TaskStatus[keyof typeof TaskStatus]
        const task = tasks.find(t => t.id === taskId)
        if (!task || task.status === newStatus) return
        moveTask.mutate({ id: taskId, body: { ...task, status: newStatus } })
    }

    if (isLoading) {
        return (
            <div className="min-h-screen flex items-center justify-center text-gray-400 text-sm">
                Loading…
            </div>
        )
    }

    if (isError) {
        return (
            <div className="min-h-screen flex items-center justify-center text-red-500 text-sm">
                Failed to load tasks.
            </div>
        )
    }

    const byStatus = (s: typeof TaskStatus[keyof typeof TaskStatus]) =>
        tasks.filter(t => t.status === s)

    const hasDoneTasks = tasks.some(t => t.status === TaskStatus.Done)

    return (
        <DndContext sensors={sensors} onDragEnd={handleDragEnd}>
            <div className="p-6 pt-16">
                <FloatingAddButton onClick={() => setShowNewCard(true)} />

                <div className="max-w-5xl mx-auto mb-4 flex justify-end">
                    <button
                        onClick={() => archiveAll.mutate()}
                        disabled={!hasDoneTasks || archiveAll.isPending}
                        className="text-sm px-4 py-1.5 bg-white border border-gray-300 rounded-lg text-gray-600 hover:bg-gray-50 disabled:opacity-40 disabled:cursor-not-allowed transition-colors"
                    >
                        Archive all done
                    </button>
                </div>

                <div className="grid grid-cols-3 gap-6 max-w-5xl mx-auto">
                    <Column
                        status={TaskStatus.Todo}
                        tasks={byStatus(TaskStatus.Todo)}
                        showNewCard={showNewCard}
                        onCardSave={handleSave}
                        onCardDelete={handleDelete}
                        onCancelCreate={() => setShowNewCard(false)}
                    />
                    <Column
                        status={TaskStatus.InProgress}
                        tasks={byStatus(TaskStatus.InProgress)}
                        onCardSave={handleSave}
                        onCardDelete={handleDelete}
                        onCancelCreate={() => {}}
                    />
                    <Column
                        status={TaskStatus.Done}
                        tasks={byStatus(TaskStatus.Done)}
                        onCardSave={handleSave}
                        onCardDelete={handleDelete}
                        onCancelCreate={() => {}}
                    />
                </div>
            </div>
        </DndContext>
    )
}
