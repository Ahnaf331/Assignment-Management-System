"use client";

import Link from "next/link";
import { usePathname, useRouter } from "next/navigation";
import { ReactNode, useEffect } from "react";
import { useAuth } from "@/lib/auth";
import { Spinner } from "./ui";
import type { Role } from "@/lib/types";

interface NavItem {
  href: string;
  label: string;
  icon: string;
  roles: Role[];
}

const NAV: NavItem[] = [
  { href: "/dashboard", label: "Dashboard", icon: "🏠", roles: ["Admin", "Teacher", "Student"] },
  { href: "/assignments", label: "Assignments", icon: "📚", roles: ["Admin", "Teacher", "Student"] },
  { href: "/submissions", label: "My Work", icon: "✍️", roles: ["Student"] },
  { href: "/admin/users", label: "Users", icon: "👥", roles: ["Admin"] },
  { href: "/admin/classes", label: "Classes", icon: "🏫", roles: ["Admin"] },
  { href: "/admin/subjects", label: "Subjects", icon: "📖", roles: ["Admin"] },
  { href: "/admin/teachers", label: "Teacher Assignments", icon: "🧑‍🏫", roles: ["Admin"] },
];

const ROLE_COLORS: Record<Role, string> = {
  Admin: "bg-classroom-purple",
  Teacher: "bg-classroom-blue",
  Student: "bg-classroom-green",
};

export default function AppShell({ children }: { children: ReactNode }) {
  const { user, loading, logout } = useAuth();
  const router = useRouter();
  const pathname = usePathname();

  useEffect(() => {
    if (!loading && !user) router.replace("/login");
  }, [loading, user, router]);

  if (loading) return <div className="flex min-h-screen items-center justify-center"><Spinner label="Loading..." /></div>;
  if (!user) return null;

  const items = NAV.filter((n) => n.roles.includes(user.role));

  return (
    <div className="min-h-screen">
      {/* Top app bar */}
      <header className="sticky top-0 z-30 flex h-16 items-center justify-between border-b bg-white px-4 shadow-sm md:px-6">
        <div className="flex items-center gap-2">
          <span className="text-2xl">🎓</span>
          <span className="text-xl font-medium text-classroom-gray">
            Class<span className="text-classroom-green">Work</span>
          </span>
        </div>
        <div className="flex items-center gap-3">
          <span className={`hidden rounded-full px-3 py-1 text-xs font-medium text-white sm:inline ${ROLE_COLORS[user.role]}`}>
            {user.role}
          </span>
          <div className="flex items-center gap-2">
            <div className="flex h-9 w-9 items-center justify-center rounded-full bg-classroom-green text-sm font-medium text-white">
              {user.fullName.charAt(0).toUpperCase()}
            </div>
            <div className="hidden text-right sm:block">
              <div className="text-sm font-medium leading-tight">{user.fullName}</div>
              <div className="text-xs text-classroom-gray">{user.email}</div>
            </div>
          </div>
          <button onClick={logout} className="btn-secondary !px-3 !py-1.5 text-xs">
            Sign out
          </button>
        </div>
      </header>

      <div className="mx-auto flex max-w-7xl">
        {/* Sidebar */}
        <aside className="sticky top-16 hidden h-[calc(100vh-4rem)] w-64 shrink-0 border-r bg-white px-3 py-4 md:block">
          <nav className="space-y-1">
            {items.map((item) => {
              const active = pathname === item.href || pathname.startsWith(item.href + "/");
              return (
                <Link
                  key={item.href}
                  href={item.href}
                  className={`flex items-center gap-3 rounded-full px-4 py-2.5 text-sm font-medium transition-colors ${
                    active
                      ? "bg-green-50 text-classroom-greenDark"
                      : "text-classroom-gray hover:bg-gray-100"
                  }`}
                >
                  <span className="text-lg">{item.icon}</span>
                  {item.label}
                </Link>
              );
            })}
          </nav>
        </aside>

        {/* Main content */}
        <main className="min-w-0 flex-1 px-4 py-6 md:px-8">{children}</main>
      </div>

      {/* Mobile bottom nav */}
      <nav className="fixed bottom-0 left-0 right-0 z-30 flex justify-around border-t bg-white py-1 md:hidden">
        {items.slice(0, 5).map((item) => {
          const active = pathname === item.href || pathname.startsWith(item.href + "/");
          return (
            <Link
              key={item.href}
              href={item.href}
              className={`flex flex-col items-center gap-0.5 px-2 py-1 text-[10px] ${
                active ? "text-classroom-greenDark" : "text-classroom-gray"
              }`}
            >
              <span className="text-lg">{item.icon}</span>
              {item.label.split(" ")[0]}
            </Link>
          );
        })}
      </nav>
    </div>
  );
}
