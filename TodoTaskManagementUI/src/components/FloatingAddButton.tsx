interface FloatingAddButtonProps {
    onClick: () => void
}

export default function FloatingAddButton({ onClick }: FloatingAddButtonProps) {
    return (
        <button
            onClick={onClick}
            title="Add task"
            className="fixed top-4 right-4 z-20 w-11 h-11 bg-blue-600 text-white rounded-full shadow-lg flex items-center justify-center text-2xl hover:bg-blue-700 active:scale-95 transition-all"
        >
            +
        </button>
    )
}
