// ─────────────────────────────────────────────
//  notificationsApi.js  –  In-memory mock API
//  Simulates async REST calls with realistic delay
//  Endpoints mirrored:
//    GET    /api/notifications?role=
//    PATCH  /api/notifications/:id/read
//    PATCH  /api/notifications/read-all?role=
//    DELETE /api/notifications/:id
// ─────────────────────────────────────────────

const delay = (ms = 350) => new Promise((res) => setTimeout(res, ms));

// Priority weight – lower = shown first
const PRIORITY_WEIGHT = {
  urgent: 1,
  leave: 2,
  payroll: 3,
  payslip: 4,
  system: 5,
  info: 6,
};

// ── Seed data ────────────────────────────────
let _db = [
  // ── superuser ────────────────────────────
  {
    id: "su-1",
    role: "superuser",
    message: "Leave request from John Doe requires approval",
    detail: "Annual leave · 3 days · Starting 28 Mar",
    read: false,
    type: "leave",
    priority: "urgent",
    route: "/leaveManagement",
    time: "2m ago",
    dateGroup: "Today",
  },
  {
    id: "su-2",
    role: "superuser",
    message: "Critical system security patch available",
    detail: "v4.2.1 — apply before end of business",
    read: false,
    type: "system",
    priority: "urgent",
    time: "18m ago",
    dateGroup: "Today",
  },
  {
    id: "su-3",
    role: "superuser",
    message: "Sarah's sick leave request pending review",
    detail: "Sick leave · 1 day · 26 Mar",
    read: false,
    type: "leave",
    priority: "leave",
    route: "/leaveManagement",
    time: "1h ago",
    dateGroup: "Today",
  },
  {
    id: "su-4",
    role: "superuser",
    message: "March payroll processed successfully",
    detail: "48 employees · ZAR 1,240,000 disbursed",
    read: true,
    type: "payroll",
    priority: "payroll",
    time: "3h ago",
    dateGroup: "Today",
  },
  {
    id: "su-5",
    role: "superuser",
    message: "Q1 2026 compliance report ready",
    detail: "PDF available for download in reports",
    read: true,
    type: "info",
    priority: "info",
    time: "Yesterday",
    dateGroup: "Earlier",
  },
  {
    id: "su-6",
    role: "superuser",
    message: "February payroll records archived",
    detail: "Stored in compliance vault · ref #FEB-2026",
    read: true,
    type: "payroll",
    priority: "payroll",
    time: "2 days ago",
    dateGroup: "Earlier",
  },
  {
    id: "su-7",
    role: "superuser",
    message: "Updated tax table for new tax year has been uploaded",
    detail: "Will be active on the 1st of March",
    read: true,
    type: "info",
    priority: "info",
    time: "14 February 2026",
    dateGroup: "Last Month",
  },
  {
    id: "su-8",
    role: "superuser",
    message: "New Positions have been added",
    detail: "Executive",
    read: true,
    type: "info",
    priority: "info",
    time: "25 February 2026",
    dateGroup: "Last Month",
  },
  // ── normaluser ───────────────────────────
  {
    id: "nu-1",
    role: "normaluser",
    message: "Your leave request was approved",
    detail: "Annual leave · 25–27 Mar · Approved by Jane Smith",
    read: false,
    type: "leave",
    priority: "leave",
    time: "30m ago",
    dateGroup: "Today",
  },
  {
    id: "nu-2",
    role: "normaluser",
    message: "Your March payslip is available",
    detail: "Net pay: ZAR 24,350 · View & download",
    read: false,
    type: "payslip",
    priority: "payslip",
    route: "/payslips",
    time: "2h ago",
    dateGroup: "Today",
  },
  {
    id: "nu-3",
    role: "normaluser",
    message: "Remote work policy updated",
    detail: "New policy effective 1 April 2026 — please review",
    read: true,
    type: "info",
    priority: "info",
    time: "Yesterday",
    dateGroup: "Earlier",
  },
];

// ─────────────────────────────────────────────
//  API methods
// ─────────────────────────────────────────────

/**
 * GET /api/notifications?role=
 * Returns notifications for the given role, sorted by priority.
 */
export async function fetchNotifications(role) {
  await delay();

  if (!role) return [];

  const normalizedRole = role.toLowerCase();

  const results = _db
    .filter((n) => n.role.toLowerCase() === normalizedRole)
    .sort(
      (a, b) =>
        (PRIORITY_WEIGHT[a.priority] ?? 9) -
        (PRIORITY_WEIGHT[b.priority] ?? 9)
    );

  return results.map((n) => ({ ...n }));
}

/**
 * PATCH /api/notifications/:id/read
 * Marks a single notification as read.
 */
export async function markAsRead(id) {
  await delay(150);
  const item = _db.find((n) => n.id === id);
  if (!item) throw new Error(`Notification ${id} not found`);
  item.read = true;
  return { ...item };
}

/**
 * PATCH /api/notifications/read-all?role=
 * Marks all notifications for a role as read.
 */
export async function markAllAsRead(role) {
  await delay(200);
  _db.forEach((n) => {
    if (n.role === role) n.read = true;
  });
  return { success: true };
}

/**
 * DELETE /api/notifications/:id
 * Removes a notification.
 */
export async function deleteNotification(id) {
  await delay(150);
  const idx = _db.findIndex((n) => n.id === id);
  if (idx === -1) throw new Error(`Notification ${id} not found`);
  _db.splice(idx, 1);
  return { success: true };
}

/**
 * Helper – returns unread count for a role (used by the bell badge).
 */
export async function getUnreadCount(role) {
  await delay(100);

  if (!role || typeof role !== "string") {
    console.warn("getUnreadCount: invalid role", role);
    return 0; // 🔥 prevent crashes
  }

  const normalizedRole = role.toLowerCase();

  return _db.filter(
    (n) => n.role.toLowerCase() === normalizedRole && !n.read
  ).length;
}