import { useArchivedTasks, useDeleteArchivedTask } from '../hooks/useTasks'
import TaskCard from '../components/TaskCard'

export default function ArchivePage() {
    const { data: tasks = [], isLoading, isError } = useArchivedTasks()
    const deleteTask = useDeleteArchivedTask()

    if (isLoading) {
        return (
            <div className="flex-1 flex items-center justify-center text-gray-400 text-sm">
                Loading…
            </div>
        )
    }

    if (isError) {
        return (
            <div className="flex-1 flex items-center justify-center text-red-500 text-sm">
                Failed to load archived tasks.
            </div>
        )
    }

    if (tasks.length === 0) {
        return (
            <div className="flex-1 flex items-center justify-center text-gray-400 text-sm">
                No archived tasks yet.
            </div>
        )
    }

    return (
        <div className="p-6 max-w-xl mx-auto flex flex-col gap-3">
            {tasks.map(task => (
                <TaskCard
                    key={task.id}
                    task={task}
                    isArchived
                    onSave={() => {}}
                    onDelete={() => task.id && deleteTask.mutate(task.id)}
                />
            ))}
        </div>
    )
}
