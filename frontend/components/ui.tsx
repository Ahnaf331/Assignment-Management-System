"use client";

import { ReactNode } from "react";
import type { AssignmentStatus, SubmissionStatus } from "@/lib/types";

export function Spinner({ label }: { label?: string }) {
  return (
    <div className="flex items-center justify-center gap-3 py-10 text-classroom-gray">
      <div className="h-6 w-6 animate-spin rounded-full border-2 border-gray-300 border-t-classroom-green" />
      {label && <span className="text-sm">{label}</span>}
    </div>
  );
}

export function Badge({
  children,
  color = "gray",
}: {
  children: ReactNode;
  color?: "green" | "blue" | "orange" | "red" | "gray" | "purple";
}) {
  const map: Record<string, string> = {
    green: "bg-green-100 text-green-800",
    blue: "bg-blue-100 text-blue-800",
    orange: "bg-orange-100 text-orange-800",
    red: "bg-red-100 text-red-800",
    gray: "bg-gray-100 text-gray-700",
    purple: "bg-purple-100 text-purple-800",
  };
  return (
    <span className={`inline-block rounded-full px-2.5 py-0.5 text-xs font-medium ${map[color]}`}>
      {children}
    </span>
  );
}

export function StatusBadge({ status }: { status: AssignmentStatus }) {
  return status === "Published" ? (
    <Badge color="green">Published</Badge>
  ) : (
    <Badge color="gray">Draft</Badge>
  );
}

export function SubmissionBadge({ status }: { status: SubmissionStatus }) {
  const map: Record<SubmissionStatus, "blue" | "orange" | "green" | "purple"> = {
    Submitted: "blue",
    Late: "orange",
    Graded: "green",
    Returned: "purple",
  };
  return <Badge color={map[status]}>{status}</Badge>;
}

export function EmptyState({ title, subtitle }: { title: string; subtitle?: string }) {
  return (
    <div className="card flex flex-col items-center justify-center gap-2 p-12 text-center">
      <div className="text-5xl">📋</div>
      <h3 className="text-lg font-medium text-gray-700">{title}</h3>
      {subtitle && <p className="max-w-md text-sm text-classroom-gray">{subtitle}</p>}
    </div>
  );
}

export function Modal({
  open,
  onClose,
  title,
  children,
}: {
  open: boolean;
  onClose: () => void;
  title: string;
  children: ReactNode;
}) {
  if (!open) return null;
  return (
    <div
      className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4"
      onClick={onClose}
    >
      <div
        className="w-full max-w-lg rounded-lg bg-white shadow-xl"
        onClick={(e) => e.stopPropagation()}
      >
        <div className="flex items-center justify-between border-b px-6 py-4">
          <h2 className="text-lg font-medium">{title}</h2>
          <button onClick={onClose} className="text-2xl leading-none text-gray-400 hover:text-gray-600">
            &times;
          </button>
        </div>
        <div className="max-h-[70vh] overflow-y-auto p-6">{children}</div>
      </div>
    </div>
  );
}

export function Alert({ message }: { message: string }) {
  if (!message) return null;
  return (
    <div className="rounded-md border border-red-200 bg-red-50 px-4 py-2 text-sm text-red-700">
      {message}
    </div>
  );
}

/** Deterministic accent color per class/subject for the Classroom-style card headers. */
export function accentColor(seed: string): string {
  const colors = ["#1e8e3e", "#1967d2", "#9334e6", "#00897b", "#e8710a", "#d93025", "#616161"];
  let hash = 0;
  for (let i = 0; i < seed.length; i++) hash = seed.charCodeAt(i) + ((hash << 5) - hash);
  return colors[Math.abs(hash) % colors.length];
}

export function formatDate(iso: string): string {
  return new Date(iso).toLocaleString(undefined, {
    dateStyle: "medium",
    timeStyle: "short",
  });
}
