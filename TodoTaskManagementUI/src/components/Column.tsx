import { useDroppable } from '@dnd-kit/core'
import { TaskStatus } from '../types/task'
import type { TaskApi } from '../types/task'
import TaskCard from './TaskCard'

const COLUMN_NAMES: Record<number, string> = {
    [TaskStatus.Todo]: 'Todo',
    [TaskStatus.InProgress]: 'In Progress',
    [TaskStatus.Done]: 'Done',
}

interface ColumnProps {
    status: typeof TaskStatus[keyof typeof TaskStatus]
    tasks: TaskApi[]
    showNewCard?: boolean
    onCardSave: (id: string | undefined, data: Partial<TaskApi>) => void
    onCardDelete: (id: string) => void
    onCancelCreate: () => void
}

export default function Column({ status, tasks, showNewCard, onCardSave, onCardDelete, onCancelCreate }: ColumnProps) {
    const { setNodeRef, isOver } = useDroppable({ id: String(status) })

    return (
        <div className="flex flex-col gap-3 min-w-0">
            <div className="sticky top-0 bg-gray-100 py-2 z-10">
                <h2 className="text-sm font-semibold text-gray-500 uppercase tracking-wide">
                    {COLUMN_NAMES[status]}{' '}
                    <span className="font-normal text-gray-400">({tasks.length})</span>
                </h2>
            </div>

            <div
                ref={setNodeRef}
                className={`flex flex-col gap-2 min-h-24 rounded-xl p-1 transition-colors ${isOver ? 'bg-blue-50' : ''}`}
            >
                {showNewCard && (
                    <TaskCard
                        task={null}
                        onSave={data => onCardSave(undefined, data)}
                        onCancelCreate={onCancelCreate}
                    />
                )}
                {tasks.map(task => (
                    <TaskCard
                        key={task.id}
                        task={task}
                        onSave={data => onCardSave(task.id, data)}
                        onDelete={() => task.id && onCardDelete(task.id)}
                    />
                ))}
            </div>
        </div>
    )
}
