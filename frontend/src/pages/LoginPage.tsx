import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { api } from "../lib/api";
import type { LoginResponse } from "../types";
import "./LoginPage.css";

export default function LoginPage() {
  const nav = useNavigate();
  const [email, setEmail] = useState("director@local.dev");
  const [password, setPassword] = useState("Admin#12345678");
  const [showPassword, setShowPassword] = useState(false);
  const [err, setErr] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  async function submit(e: React.FormEvent) {
    e.preventDefault();
    setErr(null);
    setLoading(true);

    try {
      const res = await api<LoginResponse>("/auth/login", {
        method: "POST",
        body: JSON.stringify({ email, password }),
      });

      localStorage.setItem("accessToken", res.accessToken);
      nav("/employees", { replace: true });
    } catch (e: any) {
    setErr(e.message || "Login failed.");
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="min-vh-100 bg-light d-flex flex-column">
      <nav className="navbar navbar-dark bg-dark shadow-sm">
        <div className="container">
          <span className="navbar-brand fw-semibold">Employee Manager</span>
        </div>
      </nav>

      <div className="container d-flex flex-grow-1 align-items-center justify-content-center py-4">
        <div className="card shadow-sm w-100" style={{ maxWidth: 420 }}>
          <div className="card-body p-4">
            <div className="mb-3">
              <h2 className="h5 mb-1">Welcome back</h2>
              <p className="text-muted mb-0">Sign in to manage your team.</p>
            </div>

            <form onSubmit={submit}>
              <div className="mb-3">
                <label className="form-label">Email</label>
                <input
                  className="form-control"
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                />
              </div>

              <div className="mb-3">
                <label className="form-label">Password</label>
                <div className="input-group">
                  <input
                    className="form-control"
                    type={showPassword ? "text" : "password"}
                    value={password}
                    onChange={(e) => setPassword(e.target.value)}
                  />
                  <button
                    type="button"
                    className="btn btn-outline-secondary"
                    aria-label={showPassword ? "Hide password" : "Show password"}
                    aria-pressed={showPassword}
                    onClick={() => setShowPassword((prev) => !prev)}
                  >
                    <svg viewBox="0 0 24 24" width="18" height="18" aria-hidden="true">
                      <path
                        d="M12 5C6.5 5 2 9 1 12c1 3 5.5 7 11 7s10-4 11-7c-1-3-5.5-7-11-7Zm0 12a5 5 0 1 1 0-10 5 5 0 0 1 0 10Z"
                        fill="currentColor"
                      />
                      <circle cx="12" cy="12" r="2.5" fill="currentColor" />
                    </svg>
                  </button>
                </div>
              </div>

              {err && (
                <div className="alert alert-danger" role="alert">
                  {err}
                </div>
              )}

              <button className="btn btn-primary w-100" disabled={loading}>
                {loading ? "Signing in..." : "Sign in"}
              </button>
            </form>
          </div>
        </div>
      </div>
    </div>
  );
}