"use client";

import { FormEvent, useEffect, useState } from "react";
import { api, apiError } from "@/lib/api";
import { Spinner, Alert, Badge, Modal } from "@/components/ui";
import type { ClassCourse, Role, User } from "@/lib/types";

const ROLE_COLOR: Record<Role, "purple" | "blue" | "green"> = {
  Admin: "purple",
  Teacher: "blue",
  Student: "green",
};

export default function AdminUsersPage() {
  const [users, setUsers] = useState<User[]>([]);
  const [classes, setClasses] = useState<ClassCourse[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [modalOpen, setModalOpen] = useState(false);
  const [editing, setEditing] = useState<User | null>(null);
  const [roleFilter, setRoleFilter] = useState<Role | "">("");

  const load = () => {
    setLoading(true);
    Promise.all([api.get<User[]>("/users"), api.get<ClassCourse[]>("/classes")])
      .then(([u, c]) => {
        setUsers(u.data);
        setClasses(c.data);
      })
      .catch((e) => setError(apiError(e)))
      .finally(() => setLoading(false));
  };
  useEffect(load, []);

  async function remove(u: User) {
    if (!confirm(`Deactivate ${u.fullName}?`)) return;
    try {
      await api.delete(`/users/${u.id}`);
      load();
    } catch (e) {
      setError(apiError(e));
    }
  }

  const filtered = roleFilter ? users.filter((u) => u.role === roleFilter) : users;

  return (
    <div className="space-y-5">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <h1 className="text-2xl font-medium">Users</h1>
        <button
          className="btn-primary"
          onClick={() => {
            setEditing(null);
            setModalOpen(true);
          }}
        >
          + Add user
        </button>
      </div>

      <div className="flex gap-2">
        {(["", "Admin", "Teacher", "Student"] as const).map((r) => (
          <button
            key={r || "all"}
            onClick={() => setRoleFilter(r)}
            className={`rounded-full px-3 py-1 text-sm ${
              roleFilter === r ? "bg-classroom-green text-white" : "bg-gray-100 text-classroom-gray"
            }`}
          >
            {r || "All"}
          </button>
        ))}
      </div>

      <Alert message={error} />

      {loading ? (
        <Spinner />
      ) : (
        <div className="card overflow-x-auto">
          <table className="w-full text-sm">
            <thead className="border-b bg-gray-50 text-left text-xs uppercase text-classroom-gray">
              <tr>
                <th className="px-4 py-3">Name</th>
                <th className="px-4 py-3">Email</th>
                <th className="px-4 py-3">Role</th>
                <th className="px-4 py-3">Class</th>
                <th className="px-4 py-3">Status</th>
                <th className="px-4 py-3"></th>
              </tr>
            </thead>
            <tbody className="divide-y">
              {filtered.map((u) => (
                <tr key={u.id} className="hover:bg-gray-50">
                  <td className="px-4 py-3 font-medium">{u.fullName}</td>
                  <td className="px-4 py-3 text-classroom-gray">{u.email}</td>
                  <td className="px-4 py-3">
                    <Badge color={ROLE_COLOR[u.role]}>{u.role}</Badge>
                  </td>
                  <td className="px-4 py-3 text-classroom-gray">{u.classCourseName ?? "—"}</td>
                  <td className="px-4 py-3">
                    {u.isActive ? <Badge color="green">Active</Badge> : <Badge color="red">Inactive</Badge>}
                  </td>
                  <td className="px-4 py-3 text-right">
                    <button
                      className="mr-3 text-classroom-blue hover:underline"
                      onClick={() => {
                        setEditing(u);
                        setModalOpen(true);
                      }}
                    >
                      Edit
                    </button>
                    <button className="text-classroom-red hover:underline" onClick={() => remove(u)}>
                      Deactivate
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      <UserModal
        open={modalOpen}
        onClose={() => setModalOpen(false)}
        classes={classes}
        editing={editing}
        onSaved={() => {
          setModalOpen(false);
          load();
        }}
      />
    </div>
  );
}

function UserModal({
  open,
  onClose,
  classes,
  editing,
  onSaved,
}: {
  open: boolean;
  onClose: () => void;
  classes: ClassCourse[];
  editing: User | null;
  onSaved: () => void;
}) {
  const [fullName, setFullName] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [role, setRole] = useState<Role>("Student");
  const [classId, setClassId] = useState("");
  const [isActive, setIsActive] = useState(true);
  const [error, setError] = useState("");
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    if (!open) return;
    setError("");
    setFullName(editing?.fullName ?? "");
    setEmail(editing?.email ?? "");
    setPassword("");
    setRole(editing?.role ?? "Student");
    setClassId(editing?.classCourseId ?? "");
    setIsActive(editing?.isActive ?? true);
  }, [open, editing]);

  async function submit(e: FormEvent) {
    e.preventDefault();
    setError("");
    setSaving(true);
    try {
      if (editing) {
        await api.put(`/users/${editing.id}`, {
          fullName,
          role,
          isActive,
          classCourseId: role === "Student" ? classId : null,
        });
      } else {
        await api.post("/users", {
          fullName,
          email,
          password,
          role,
          classCourseId: role === "Student" ? classId : null,
        });
      }
      onSaved();
    } catch (err) {
      setError(apiError(err));
    } finally {
      setSaving(false);
    }
  }

  return (
    <Modal open={open} onClose={onClose} title={editing ? "Edit user" : "Add user"}>
      <form onSubmit={submit} className="space-y-4">
        <div>
          <label className="label">Full name</label>
          <input className="input" value={fullName} onChange={(e) => setFullName(e.target.value)} required />
        </div>
        {!editing && (
          <>
            <div>
              <label className="label">Email</label>
              <input type="email" className="input" value={email} onChange={(e) => setEmail(e.target.value)} required />
            </div>
            <div>
              <label className="label">Password</label>
              <input type="text" className="input" value={password} onChange={(e) => setPassword(e.target.value)} required minLength={6} />
            </div>
          </>
        )}
        <div>
          <label className="label">Role</label>
          <select className="input" value={role} onChange={(e) => setRole(e.target.value as Role)}>
            <option value="Student">Student</option>
            <option value="Teacher">Teacher</option>
            <option value="Admin">Admin</option>
          </select>
        </div>
        {role === "Student" && (
          <div>
            <label className="label">Class / course</label>
            <select className="input" value={classId} onChange={(e) => setClassId(e.target.value)} required>
              <option value="">Select…</option>
              {classes.map((c) => (
                <option key={c.id} value={c.id}>
                  {c.name}
                </option>
              ))}
            </select>
          </div>
        )}
        {editing && (
          <label className="flex items-center gap-2 text-sm">
            <input type="checkbox" checked={isActive} onChange={(e) => setIsActive(e.target.checked)} />
            Active
          </label>
        )}
        <Alert message={error} />
        <div className="flex justify-end gap-2">
          <button type="button" className="btn-secondary" onClick={onClose}>
            Cancel
          </button>
          <button type="submit" className="btn-primary" disabled={saving}>
            {saving ? "Saving…" : "Save"}
          </button>
        </div>
      </form>
    </Modal>
  );
}
