import { Routes, Route, Navigate } from "react-router-dom";
import LoginPage from "./pages/LoginPage";
import EmployeesPage from "./pages/EmployeesPage";

function isAuthed() {
  return !!localStorage.getItem("accessToken");
}

function PrivateRoute({ children }: { children: JSX.Element }) {
  return isAuthed() ? children : <Navigate to="/login" replace />;
}

export default function App() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route
        path="/employees"
        element={
          <PrivateRoute>
            <EmployeesPage />
          </PrivateRoute>
        }
      />
      <Route path="*" element={<Navigate to="/employees" replace />} />
    </Routes>
  );
}