import { useEffect, useMemo, useState } from "react";
import { api } from "../lib/api";
import type { Employee, CreateEmployeeRequest, UpdateEmployeeRequest, Phone } from "../types";
import "./EmployeesPage.css";

function emptyPhones(): Phone[] {
  return [
    { number: "+55 48 99999-1111" },
    { number: "+55 48 3333-2222" },
  ];
}

export default function EmployeesPage() {
  const [items, setItems] = useState<Employee[]>([]);
  const [loading, setLoading] = useState(false);
  const [err, setErr] = useState<string | null>(null);

  const [mode, setMode] = useState<"none" | "create" | "edit">("none");
  const [editing, setEditing] = useState<Employee | null>(null);

  async function load() {
    setErr(null);
    setLoading(true);
    try {
      const list = await api<Employee[]>("/employees");
      setItems(list);
    } catch (e: any) {
      setErr(e.message || "Failed to load.");
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    load();
  }, []);

  function logout() {
    localStorage.removeItem("accessToken");
    window.location.href = "/login";
  }

  const title = useMemo(() => {
    if (mode === "create") return "Create employee";
    if (mode === "edit") return "Edit employee";
    return "Employees";
  }, [mode]);


  function formatRole(role: number) {
    if (role === 0) return "employee";
    if (role === 1) return "leader";
    if (role === 2) return "director";
    return "unknown";
  }

  async function onDelete(id: string) {
    if (!confirm("Delete this employee?")) return;
    await api<void>(`/employees/${id}`, { method: "DELETE" });
    await load();
  }

  async function onCreate(payload: CreateEmployeeRequest) {
    await api<Employee>("/employees", { method: "POST", body: JSON.stringify(payload) });
    setMode("none");
    await load();
  }

  async function onUpdate(id: string, payload: UpdateEmployeeRequest) {
    await api<Employee>(`/employees/${id}`, { method: "PUT", body: JSON.stringify(payload) });
    setMode("none");
    setEditing(null);
    await load();
  }

  return (
    <div className="page">
      <nav className="topbar">
        <div className="topbar__brand">Employee Manager</div>
        <div className="topbar__actions">
          {mode !== "create" && (
            <button
              className="btn btn--primary"
              onClick={() => { setMode("create"); setEditing(null); }}
            >
              + New
            </button>
          )}
          <button className="btn btn--ghost" onClick={logout}>Exit</button>
        </div>
      </nav>

      <div className="page__header">
        <h2 className="page__title">{title}</h2>
        <p className="page__subtitle">Manage your team and key information.</p>
      </div>

      {err && <div className="alert alert--error">{err}</div>}

      {mode === "create" && (
        <EmployeeForm
          mode="create"
          onCancel={() => setMode("none")}
          onCreate={onCreate}
        />
      )}

      {mode === "edit" && editing && (
        <EmployeeForm
          mode="edit"
          employee={editing}
          onCancel={() => { setMode("none"); setEditing(null); }}
          onUpdate={(payload) => onUpdate(editing.id, payload)}
        />
      )}

      <div className="card table-card">
        {loading ? (
          <p className="muted">Loading...</p>
        ) : (
          <table className="table">
            <thead>
              <tr>
                <th>Name</th>
                <th>Email</th>
                <th>Document</th>
                <th>Role</th>
                <th>Phones</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              {items.map((e) => (
                <tr key={e.id}>
                  <td>{e.firstName} {e.lastName}</td>
                  <td>{e.email}</td>
                  <td>{e.docNumber}</td>
                  <td>{formatRole(e.role)}</td>
                  <td>
                    {e.phones?.map((p, idx) => (
                      <div key={idx} className="phone-item">
                        {p.number}
                      </div>
                    ))}
                  </td>
                  <td className="actions">
                    <button className="btn btn--light" onClick={() => { setEditing(e); setMode("edit"); }}>
                      Edit
                    </button>
                    <button className="btn btn--danger" onClick={() => onDelete(e.id)}>
                      Delete
                    </button>
                  </td>
                </tr>
              ))}
              {items.length === 0 && (
                <tr>
                  <td colSpan={6} className="empty">No employees.</td>
                </tr>
              )}
            </tbody>
          </table>
        )}
      </div>

      {mode !== "none" && (
        <div className="footer-actions">
          <button className="btn btn--light" type="button" onClick={() => { setMode("none"); setEditing(null); }}>
            Back
          </button>
        </div>
      )}
    </div>
  );
}

function EmployeeForm(props:
  | { mode: "create"; onCancel: () => void; onCreate: (p: CreateEmployeeRequest) => Promise<void> }
  | { mode: "edit"; employee: Employee; onCancel: () => void; onUpdate: (p: UpdateEmployeeRequest) => Promise<void> }
) {
  const isCreate = props.mode === "create";

  const [firstName, setFirstName] = useState(isCreate ? "John" : props.employee.firstName);
  const [lastName, setLastName] = useState(isCreate ? "Doe" : props.employee.lastName);
  const [email, setEmail] = useState(isCreate ? "john@ex.com" : props.employee.email);

  const [docNumber, setDocNumber] = useState(isCreate ? "DOC-001" : props.employee.docNumber);
  const [birthDate, setBirthDate] = useState(isCreate ? "1999-01-01" : props.employee.birthDate);

  const [role, setRole] = useState<number>(isCreate ? 1 : props.employee.role);
  const [password, setPassword] = useState(isCreate ? "Ana#12345678" : "");

  const [phones, setPhones] = useState<Phone[]>(
    isCreate ? emptyPhones() : (props.employee.phones?.length ? props.employee.phones : emptyPhones())
  );

  const [saving, setSaving] = useState(false);
  const [err, setErr] = useState<string | null>(null);

  function updatePhoneNumber(idx: number, value: string) {
    setPhones((prev) => prev.map((p, i) => (i === idx ? { ...p, number: value } : p)));
  }

  async function submit(e: React.FormEvent) {
    e.preventDefault();
    setErr(null);
    setSaving(true);

    try {
      if (phones.length < 2) throw new Error("At least two phones are required.");

      if (isCreate) {
        const payload: CreateEmployeeRequest = {
          firstName,
          lastName,
          email,
          docNumber,
          birthDate,
          role: role as any,
          phones,
          password,
          managerEmployeeId: null,
          managerName: null,
        };
        await props.onCreate(payload);
      } else {
        const payload: UpdateEmployeeRequest = {
          firstName,
          lastName,
          email,
          phones,
          managerEmployeeId: props.employee.managerEmployeeId ?? null,
          managerName: props.employee.managerName ?? null,
        };
        await props.onUpdate(payload);
      }
    } catch (e: any) {
      setErr(e.message || "Failed to save.");
    } finally {
      setSaving(false);
    }
  }

  return (
    <form onSubmit={submit} className="card employee-form">
      <div className="form-grid">
        <div className="form-field">
          <label>First name</label>
          <input className="input" value={firstName} onChange={(e) => setFirstName(e.target.value)} />
        </div>
        <div className="form-field">
          <label>Last name</label>
          <input className="input" value={lastName} onChange={(e) => setLastName(e.target.value)} />
        </div>

        <div className="form-field">
          <label>Email</label>
          <input className="input" value={email} onChange={(e) => setEmail(e.target.value)} />
        </div>

        {isCreate && (
          <>
            <div className="form-field">
              <label>Document number</label>
              <input className="input" value={docNumber} onChange={(e) => setDocNumber(e.target.value)} />
            </div>
            <div className="form-field">
              <label>Birth date</label>
              <input className="input" value={birthDate} onChange={(e) => setBirthDate(e.target.value)} />
            </div>
            <div className="form-field">
              <label>Role</label>
              <select className="input" value={role} onChange={(e) => setRole(Number(e.target.value))}>
                <option value={0}>employee</option>
                <option value={1}>leader</option>
                <option value={2}>director</option>
              </select>
            </div>
            <div className="form-field">
              <label>Password</label>
              <input className="input" type="password" value={password} onChange={(e) => setPassword(e.target.value)} />
            </div>
          </>
        )}
      </div>

      <div className="form-field">
        <label>Phones (minimum 2)</label>
        {phones.map((p, idx) => (
          <div key={idx} className="phone-row">
            <input
              className="input"
              placeholder="number"
              value={p.number}
              onChange={(e) => updatePhoneNumber(idx, e.target.value)}
            />
          </div>
        ))}
        <div className="inline-actions">
          <button className="btn btn--light" type="button" onClick={() => setPhones((p) => [...p, { number: "" }])}>
            + Add phone
          </button>
          <button className="btn btn--light" type="button" onClick={() => setPhones((p) => (p.length > 2 ? p.slice(0, -1) : p))}>
            - Remove last
          </button>
        </div>
      </div>

      {err && <div className="alert alert--error">{err}</div>}

      <div className="form-actions">
        <button className="btn btn--primary" disabled={saving} type="submit">
          {saving ? "Saving..." : "Save"}
        </button>
        <button className="btn btn--light" type="button" onClick={props.onCancel}>Cancel</button>
      </div>
    </form>
  );
}