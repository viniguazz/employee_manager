import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { api } from "../lib/api";
import type { LoginResponse } from "../types";
import "./LoginPage.css";

export default function LoginPage() {
  const nav = useNavigate();
  const [email, setEmail] = useState("director@local.dev");
  const [password, setPassword] = useState("Admin#12345678");
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
    <div className="login-page">
      <nav className="topbar">
        <div className="topbar__brand">Employee Manager</div>
      </nav>

      <div className="login-card card">
        <div className="login-card__header">
          <h2>Welcome back</h2>
          <p>Sign in to manage your team.</p>
        </div>

        <form onSubmit={submit} className="login-form">
          <label>Email</label>
          <input
            className="input"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
          />

          <label>Password</label>
          <input
            className="input"
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
          />

          {err && (
            <div className="alert alert--error">
              {err}
            </div>
          )}

          <button className="btn btn--primary" disabled={loading}>
            {loading ? "Signing in..." : "Sign in"}
          </button>
        </form>
      </div>
    </div>
  );
}