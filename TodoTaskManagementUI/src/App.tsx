import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import LoginPage from './pages/LoginPage'
import SignupPage from './pages/SignupPage'
import BoardPage from './pages/BoardPage'
import ArchivePage from './pages/ArchivePage'
import MainLayout from './components/MainLayout'

export default function App() {
    return (
        <BrowserRouter>
            <Routes>
                <Route path="/login" element={<LoginPage />} />
                <Route path="/signup" element={<SignupPage />} />
                <Route element={<MainLayout />}>
                    <Route path="/" element={<BoardPage />} />
                    <Route path="/archive" element={<ArchivePage />} />
                </Route>
                <Route path="*" element={<Navigate to="/" replace />} />
            </Routes>
        </BrowserRouter>
    )
}
