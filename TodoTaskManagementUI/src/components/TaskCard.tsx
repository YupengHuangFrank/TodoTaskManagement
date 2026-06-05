import { useState, type FormEvent } from 'react'
import { useDraggable } from '@dnd-kit/core'
import type { TaskApi } from '../types/task'

interface TaskCardProps {
    task: TaskApi | null
    isArchived?: boolean
    onSave: (data: Partial<TaskApi>) => void
    onDelete?: () => void
    onCancelCreate?: () => void
}

function toDateInput(iso: string | null | undefined): string {
    if (!iso) return ''
    return iso.slice(0, 10) // "2026-06-10T00:00:00Z" → "2026-06-10"
}

function formatDate(iso: string | null | undefined): string {
    if (!iso) return ''
    const [y, m, d] = iso.slice(0, 10).split('-').map(Number)
    return new Date(y, m - 1, d).toLocaleDateString('en-US', {
        month: 'short',
        day: 'numeric',
        year: 'numeric',
    })
}

function todayString(): string {
    const now = new Date()
    const y = now.getFullYear()
    const m = String(now.getMonth() + 1).padStart(2, '0')
    const d = String(now.getDate()).padStart(2, '0')
    return `${y}-${m}-${d}`
}

function localDateToUtcIso(dateInput: string): string {
    const [y, mo, d] = dateInput.split('-').map(Number)
    return new Date(y, mo - 1, d).toISOString()
}

export default function TaskCard({ task, isArchived, onSave, onDelete, onCancelCreate }: TaskCardProps) {
    const isNew = task === null
    const [isEditing, setIsEditing] = useState(isNew)

    const { attributes, listeners, setNodeRef, transform, isDragging } = useDraggable({
        id: task?.id ?? '__new__',
        disabled: isNew || isEditing || !!isArchived,
    })

    const dragStyle = transform
        ? { transform: `translate3d(${transform.x}px, ${transform.y}px, 0)`, zIndex: 50 }
        : undefined
    const [title, setTitle] = useState(task?.title ?? '')
    const [description, setDescription] = useState(task?.description ?? '')
    const [dueDate, setDueDate] = useState(toDateInput(task?.dueDate))

    const today = todayString()
    const dueDatePast = dueDate !== '' && dueDate < today
    const canSave = title.trim().length > 0 && !dueDatePast

    function handleSave(e: FormEvent) {
        e.preventDefault()
        if (!canSave) return
        onSave({
            title: title.trim(),
            description: description.trim() || null,
            dueDate: dueDate ? localDateToUtcIso(dueDate) : null,
        })
        if (!isNew) setIsEditing(false)
    }

    function handleCancel() {
        if (isNew) {
            onCancelCreate?.()
        } else {
            setTitle(task?.title ?? '')
            setDescription(task?.description ?? '')
            setDueDate(toDateInput(task?.dueDate))
            setIsEditing(false)
        }
    }

    if (isEditing) {
        return (
            <div className="bg-yellow-50 border border-yellow-200 rounded-xl p-3 shadow-sm">
                <form onSubmit={handleSave} className="space-y-2">
                    <div className="flex items-start gap-2">
                        <input
                            autoFocus
                            placeholder="Title"
                            value={title}
                            onChange={e => setTitle(e.target.value)}
                            className="flex-1 bg-transparent text-sm font-medium text-gray-800 placeholder-gray-400 border-b border-yellow-300 focus:outline-none focus:border-yellow-500 pb-0.5"
                        />
                        <div className="flex gap-1 shrink-0">
                            <button
                                type="submit"
                                disabled={!canSave}
                                title="Save"
                                className="text-green-600 hover:text-green-700 disabled:text-gray-300 disabled:cursor-not-allowed text-base leading-none"
                            >
                                ✓
                            </button>
                            <button
                                type="button"
                                onClick={handleCancel}
                                title="Cancel"
                                className="text-gray-400 hover:text-gray-600 text-base leading-none"
                            >
                                ✗
                            </button>
                        </div>
                    </div>

                    <textarea
                        placeholder="Description (optional)"
                        value={description}
                        onChange={e => setDescription(e.target.value)}
                        rows={2}
                        className="w-full bg-transparent text-xs text-gray-600 placeholder-gray-400 resize-none focus:outline-none"
                    />

                    <div>
                        <input
                            type="date"
                            value={dueDate}
                            onChange={e => setDueDate(e.target.value)}
                            className="text-xs text-gray-500 bg-transparent focus:outline-none"
                        />
                        {dueDatePast && (
                            <p className="text-xs text-red-500 mt-0.5">Due date cannot be in the past</p>
                        )}
                    </div>
                </form>
            </div>
        )
    }

    return (
        <div
            ref={setNodeRef}
            style={dragStyle}
            className={`bg-yellow-50 border border-yellow-200 rounded-xl p-3 shadow-sm touch-none select-none ${isDragging ? 'opacity-50' : ''}`}
            {...listeners}
            {...attributes}
        >
            <div className="flex items-start gap-2">
                <p className="flex-1 text-sm font-medium text-gray-800 break-words">{task?.title}</p>
                <div className="flex gap-1.5 shrink-0">
                    {!isArchived && (
                        <button
                            onClick={() => setIsEditing(true)}
                            title="Edit"
                            className="text-gray-400 hover:text-gray-600 text-xs leading-none"
                        >
                            ✏
                        </button>
                    )}
                    {onDelete && (
                        <button
                            onClick={onDelete}
                            title="Delete"
                            className="text-gray-400 hover:text-red-500 text-xs leading-none"
                        >
                            🗑
                        </button>
                    )}
                </div>
            </div>

            {task?.description && (
                <p className="mt-1 text-xs text-gray-500 break-words">{task.description}</p>
            )}

            {task?.dueDate && (
                <p className="mt-1.5 text-xs text-gray-400">📅 {formatDate(task.dueDate)}</p>
            )}
        </div>
    )
}
