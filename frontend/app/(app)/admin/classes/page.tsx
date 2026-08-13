"use client";

import { FormEvent, useEffect, useState } from "react";
import { api, apiError } from "@/lib/api";
import { Spinner, Alert, Badge, Modal } from "@/components/ui";
import type { ClassCourse } from "@/lib/types";

export default function AdminClassesPage() {
  const [classes, setClasses] = useState<ClassCourse[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [open, setOpen] = useState(false);
  const [editing, setEditing] = useState<ClassCourse | null>(null);

  const load = () => {
    setLoading(true);
    api
      .get<ClassCourse[]>("/classes")
      .then((res) => setClasses(res.data))
      .catch((e) => setError(apiError(e)))
      .finally(() => setLoading(false));
  };
  useEffect(load, []);

  async function remove(c: ClassCourse) {
    if (!confirm(`Delete ${c.name}?`)) return;
    try {
      await api.delete(`/classes/${c.id}`);
      load();
    } catch (e) {
      setError(apiError(e));
    }
  }

  return (
    <div className="space-y-5">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-medium">Classes &amp; Courses</h1>
        <button className="btn-primary" onClick={() => { setEditing(null); setOpen(true); }}>
          + Add class
        </button>
      </div>
      <Alert message={error} />
      {loading ? (
        <Spinner />
      ) : (
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {classes.map((c) => (
            <div key={c.id} className="card p-5">
              <div className="mb-2 flex items-start justify-between">
                <h3 className="text-lg font-medium">{c.name}</h3>
                <Badge color="blue">{c.code}</Badge>
              </div>
              <p className="mb-3 text-sm text-classroom-gray">{c.description || "No description"}</p>
              <div className="flex gap-2 text-xs text-classroom-gray">
                <Badge color="gray">{c.studentCount} students</Badge>
                <Badge color="gray">{c.subjectCount} subjects</Badge>
              </div>
              <div className="mt-4 flex gap-3 text-sm">
                <button className="text-classroom-blue hover:underline" onClick={() => { setEditing(c); setOpen(true); }}>
                  Edit
                </button>
                <button className="text-classroom-red hover:underline" onClick={() => remove(c)}>
                  Delete
                </button>
              </div>
            </div>
          ))}
        </div>
      )}
      <ClassModal open={open} onClose={() => setOpen(false)} editing={editing} onSaved={() => { setOpen(false); load(); }} />
    </div>
  );
}

function ClassModal({
  open,
  onClose,
  editing,
  onSaved,
}: {
  open: boolean;
  onClose: () => void;
  editing: ClassCourse | null;
  onSaved: () => void;
}) {
  const [name, setName] = useState("");
  const [code, setCode] = useState("");
  const [description, setDescription] = useState("");
  const [error, setError] = useState("");
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    if (!open) return;
    setError("");
    setName(editing?.name ?? "");
    setCode(editing?.code ?? "");
    setDescription(editing?.description ?? "");
  }, [open, editing]);

  async function submit(e: FormEvent) {
    e.preventDefault();
    setError("");
    setSaving(true);
    try {
      const payload = { name, code, description };
      if (editing) await api.put(`/classes/${editing.id}`, payload);
      else await api.post("/classes", payload);
      onSaved();
    } catch (err) {
      setError(apiError(err));
    } finally {
      setSaving(false);
    }
  }

  return (
    <Modal open={open} onClose={onClose} title={editing ? "Edit class" : "Add class"}>
      <form onSubmit={submit} className="space-y-4">
        <div>
          <label className="label">Name</label>
          <input className="input" value={name} onChange={(e) => setName(e.target.value)} required />
        </div>
        <div>
          <label className="label">Code</label>
          <input className="input" value={code} onChange={(e) => setCode(e.target.value)} required />
        </div>
        <div>
          <label className="label">Description</label>
          <textarea className="input" value={description} onChange={(e) => setDescription(e.target.value)} />
        </div>
        <Alert message={error} />
        <div className="flex justify-end gap-2">
          <button type="button" className="btn-secondary" onClick={onClose}>Cancel</button>
          <button type="submit" className="btn-primary" disabled={saving}>{saving ? "Saving…" : "Save"}</button>
        </div>
      </form>
    </Modal>
  );
}
