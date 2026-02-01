import { useEffect, useMemo, useState } from "react";
import { api } from "../lib/api";
import type { Employee, CreateEmployeeRequest, UpdateEmployeeRequest, Phone, EmployeeLookup } from "../types";
import "./EmployeesPage.css";

function emptyPhones(): Phone[] {
  return [
    { number: "" },
    { number: "" },
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
    <div className="bg-light min-vh-100">
      <nav className="navbar navbar-dark bg-dark shadow-sm">
        <div className="container">
          <span className="navbar-brand fw-semibold">Employee Manager</span>
          <div className="d-flex gap-2">
            {mode !== "create" && (
              <button
                className="btn btn-primary"
                onClick={() => { setMode("create"); setEditing(null); }}
              >
                + New
              </button>
            )}
            <button className="btn btn-outline-light" onClick={logout}>Exit</button>
          </div>
        </div>
      </nav>

      <div className="container py-4">
        <div className="d-flex flex-wrap align-items-baseline justify-content-between gap-2 mb-3">
          <div>
            <h2 className="h4 mb-1">{title}</h2>
            <p className="text-muted mb-0">Manage your team and key information.</p>
          </div>
        </div>

        {err && <div className="alert alert-danger" role="alert">{err}</div>}

        {mode === "create" && (
          <EmployeeForm
            mode="create"
            employees={items}
            onCancel={() => setMode("none")}
            onCreate={onCreate}
          />
        )}

        {mode === "edit" && editing && (
          <EmployeeForm
            mode="edit"
            employee={editing}
            employees={items}
            onCancel={() => { setMode("none"); setEditing(null); }}
            onUpdate={(payload) => onUpdate(editing.id, payload)}
          />
        )}

        {mode === "none" && (
          <div className="card shadow-sm">
            {loading ? (
              <div className="p-3 text-muted">Loading...</div>
            ) : (
              <div className="table-responsive">
                <table className="table table-striped table-hover align-middle mb-0">
                  <thead className="table-light">
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
                        <td className="text-capitalize">{formatRole(e.role)}</td>
                        <td>
                          {e.phones?.map((p, idx) => (
                            <div key={idx} className="small text-muted">
                              {p.number}
                            </div>
                          ))}
                        </td>
                        <td className="text-nowrap">
                          <div className="d-flex gap-2">
                            <button className="btn btn-sm btn-outline-primary" onClick={() => { setEditing(e); setMode("edit"); }}>
                              Edit
                            </button>
                            <button className="btn btn-sm btn-outline-danger" onClick={() => onDelete(e.id)}>
                              Delete
                            </button>
                          </div>
                        </td>
                      </tr>
                    ))}
                    {items.length === 0 && (
                      <tr>
                        <td colSpan={6} className="text-center text-muted py-4">No employees.</td>
                      </tr>
                    )}
                  </tbody>
                </table>
              </div>
            )}
          </div>
        )}

        {mode !== "none" && (
          <div className="d-flex justify-content-end mt-3">
            <button className="btn btn-outline-secondary" type="button" onClick={() => { setMode("none"); setEditing(null); }}>
              Back
            </button>
          </div>
        )}
      </div>
    </div>
  );
}

function EmployeeForm(props:
  | { mode: "create"; employees: Employee[]; onCancel: () => void; onCreate: (p: CreateEmployeeRequest) => Promise<void> }
  | { mode: "edit"; employee: Employee; employees: Employee[]; onCancel: () => void; onUpdate: (p: UpdateEmployeeRequest) => Promise<void> }
) {
  const isCreate = props.mode === "create";
  const employee = "employee" in props ? props.employee : null;

  function toInputDate(value: string) {
    if (!value) return "";
    const d = new Date(value);
    if (Number.isNaN(d.getTime())) return value;
    return d.toISOString().slice(0, 10);
  }

  const [firstName, setFirstName] = useState(isCreate ? "" : employee!.firstName);
  const [lastName, setLastName] = useState(isCreate ? "" : employee!.lastName);
  const [email, setEmail] = useState(isCreate ? "" : employee!.email);

  const [docNumber, setDocNumber] = useState(isCreate ? "" : employee!.docNumber);
  const [birthDate, setBirthDate] = useState(
    isCreate ? "" : toInputDate(employee!.birthDate)
  );

  const [role, setRole] = useState<number>(isCreate ? 1 : employee!.role);
  const [password, setPassword] = useState("");
  const [showPassword, setShowPassword] = useState(false);

  const [managerQuery, setManagerQuery] = useState("");
  const [managerResults, setManagerResults] = useState<EmployeeLookup[]>([]);
  const [managerLoading, setManagerLoading] = useState(false);
  const [managerOpen, setManagerOpen] = useState(false);
  const [managerSelected, setManagerSelected] = useState<EmployeeLookup | null>(null);

  const [phones, setPhones] = useState<Phone[]>(
    isCreate ? emptyPhones() : (employee!.phones?.length ? employee!.phones : emptyPhones())
  );

  const [saving, setSaving] = useState(false);
  const [err, setErr] = useState<string | null>(null);

  useEffect(() => {
    const q = managerQuery.trim();
    if (q.length < 2) {
      setManagerResults([]);
      setManagerLoading(false);
      return;
    }

    setManagerLoading(true);
    const handle = setTimeout(async () => {
      try {
        const res = await api<EmployeeLookup[]>(`/employees/search?q=${encodeURIComponent(q)}`);
        setManagerResults(res);
      } catch {
        setManagerResults([]);
      } finally {
        setManagerLoading(false);
      }
    }, 300);

    return () => clearTimeout(handle);
  }, [managerQuery]);

  useEffect(() => {
    if (isCreate) return;
    if (managerQuery.trim().length > 0) return;

    const managerId = employee!.managerEmployeeId;
    if (!managerId) return;

    const found = props.employees.find((e) => e.id === managerId);
    if (found) {
      setManagerQuery(`${found.firstName} ${found.lastName} (${found.email})`);
    }
  }, [isCreate, managerQuery, employee, props.employees]);

  function updatePhoneNumber(idx: number, value: string) {
    setPhones((prev) => prev.map((p, i) => (i === idx ? { ...p, number: value } : p)));
  }

  async function submit(e: React.FormEvent) {
    e.preventDefault();
    setErr(null);
    setSaving(true);

    try {
      if (phones.length < 2) throw new Error("At least two phones are required.");
      if (!birthDate) throw new Error("Birth date is required.");

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
          managerEmployeeId: managerSelected?.id ?? null,
        };
        await props.onCreate(payload);
      } else {
        const payload: UpdateEmployeeRequest = {
          firstName,
          lastName,
          email,
          docNumber,
          birthDate,
          role: role as any,
          phones,
          managerEmployeeId: managerSelected?.id ?? employee!.managerEmployeeId ?? null,
          password: password.trim() ? password : null,
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
    <form onSubmit={submit} className="card shadow-sm mb-4">
      <div className="card-body">
        <div className="row g-3">
          <div className="col-md-6">
            <label className="form-label">First name</label>
            <input className="form-control" value={firstName} onChange={(e) => setFirstName(e.target.value)} />
          </div>
          <div className="col-md-6">
            <label className="form-label">Last name</label>
            <input className="form-control" value={lastName} onChange={(e) => setLastName(e.target.value)} />
          </div>

          <div className="col-md-6">
            <label className="form-label">Email</label>
            <input className="form-control" value={email} onChange={(e) => setEmail(e.target.value)} />
          </div>

          <div className="col-md-6">
            <label className="form-label">Document number</label>
            <input
              className="form-control"
              value={docNumber}
              onChange={(e) => setDocNumber(e.target.value)}
            />
          </div>
          <div className="col-md-6">
            <label className="form-label">Birth date</label>
            <input
              className="form-control"
              type="date"
              value={birthDate}
              onChange={(e) => setBirthDate(e.target.value)}
            />
          </div>
          <div className="col-md-6">
            <label className="form-label">Role</label>
            <select className="form-select" value={role} onChange={(e) => setRole(Number(e.target.value))}>
              <option value={0}>employee</option>
              <option value={1}>leader</option>
              <option value={2}>director</option>
            </select>
          </div>
          <div className="col-md-6">
            <label className="form-label">Manager</label>
            <input
              className="form-control"
              value={managerQuery}
              onChange={(e) => {
                setManagerQuery(e.target.value);
                if (managerSelected) setManagerSelected(null);
                if (!managerOpen) setManagerOpen(true);
              }}
              onFocus={() => setManagerOpen(true)}
              onBlur={() => setTimeout(() => setManagerOpen(false), 100)}
            />
            {managerOpen && managerQuery.trim().length >= 2 && (
              <div className="list-group mt-1">
                {managerLoading && (
                  <div className="list-group-item small text-muted">Searching...</div>
                )}
                {!managerLoading && managerResults.length === 0 && (
                  <div className="list-group-item small text-muted">No results</div>
                )}
                {!managerLoading && managerResults.map((m) => (
                  <button
                    key={m.id}
                    type="button"
                    className="list-group-item list-group-item-action"
                    onMouseDown={() => {
                      setManagerSelected(m);
                      setManagerQuery(`${m.name} (${m.email})`);
                      setManagerResults([]);
                      setManagerOpen(false);
                    }}
                  >
                    <div className="fw-semibold">{m.name}</div>
                    <div className="small text-muted">{m.email}</div>
                  </button>
                ))}
              </div>
            )}
            {managerSelected && (
              <div className="small text-muted mt-1">Selected manager: {managerSelected.name}</div>
            )}
          </div>
          <div className="col-md-6">
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
            {!isCreate && (
              <div className="form-text">Leave blank to keep current password.</div>
            )}
          </div>
        </div>

        <div className="mt-3">
          <label className="form-label">Phones (minimum 2)</label>
          {phones.map((p, idx) => (
            <div key={idx} className="input-group mb-2">
              <span className="input-group-text"># {idx + 1}</span>
              <input
                className="form-control"
                value={p.number}
                onChange={(e) => updatePhoneNumber(idx, e.target.value)}
              />
            </div>
          ))}
          <div className="d-flex gap-2 mt-2">
            <button className="btn btn-outline-secondary" type="button" onClick={() => setPhones((p) => [...p, { number: "" }])}>
              + Add phone
            </button>
            <button className="btn btn-outline-secondary" type="button" onClick={() => setPhones((p) => (p.length > 2 ? p.slice(0, -1) : p))}>
              - Remove last
            </button>
          </div>
        </div>

        {err && <div className="alert alert-danger mt-3" role="alert">{err}</div>}

        <div className="d-flex gap-2 mt-3">
          <button className="btn btn-primary" disabled={saving} type="submit">
            {saving ? "Saving..." : "Save"}
          </button>
          <button className="btn btn-outline-secondary" type="button" onClick={props.onCancel}>Cancel</button>
        </div>
      </div>
    </form>
  );
}