import { Outlet } from 'react-router-dom'
import TabBar from './TabBar'

export default function MainLayout() {
    return (
        <div className="min-h-screen bg-gray-100 flex flex-col">
            <TabBar />
            <Outlet />
        </div>
    )
}
