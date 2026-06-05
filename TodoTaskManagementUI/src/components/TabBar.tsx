import { NavLink } from 'react-router-dom'
import { useAuth } from '../hooks/useAuth'

export default function TabBar() {
    const { logout } = useAuth()

    return (
        <nav className="bg-white border-b border-gray-200 px-6 flex items-center">
            <div className="flex gap-1 flex-1">
                <NavLink
                    to="/"
                    end
                    className={({ isActive }) =>
                        `px-4 py-3 text-sm font-medium border-b-2 transition-colors ${
                            isActive
                                ? 'border-blue-600 text-blue-600'
                                : 'border-transparent text-gray-500 hover:text-gray-700'
                        }`
                    }
                >
                    Board
                </NavLink>
                <NavLink
                    to="/archive"
                    className={({ isActive }) =>
                        `px-4 py-3 text-sm font-medium border-b-2 transition-colors ${
                            isActive
                                ? 'border-blue-600 text-blue-600'
                                : 'border-transparent text-gray-500 hover:text-gray-700'
                        }`
                    }
                >
                    Archive
                </NavLink>
            </div>

            <button
                onClick={() => logout.mutate()}
                disabled={logout.isPending}
                className="text-xs text-gray-400 hover:text-gray-600 disabled:opacity-50 transition-colors"
            >
                Sign out
            </button>
        </nav>
    )
}
