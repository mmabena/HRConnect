import { useEffect, useState, useCallback } from "react";
import { useNavigate, useLocation } from "react-router-dom";
import {
  fetchNotifications,
  markAsRead,
  markAllAsRead,
  deleteNotification,
} from "./notificationsApi";
import "./NotificationPage.css";

// ─── Meta maps ────────────────────────────────
const TYPE_META = {
  leave:   { icon: "📅", label: "Leave",   color: "type-leave"   },
  system:  { icon: "⚙️", label: "System",  color: "type-system"  },
  payroll: { icon: "💰", label: "Payroll", color: "type-payroll" },
  payslip: { icon: "📄", label: "Payslip", color: "type-payslip" },
  info:    { icon: "💬", label: "Info",    color: "type-info"    },
};

const NotificationPage = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const role     = location.state?.role || "normaluser";

  const [notifications, setNotifications] = useState([]);
  const [loading, setLoading]             = useState(true);
  const [error,   setError]               = useState(null);
  const [activeTab,   setActiveTab]       = useState("all");
  const [filterType,  setFilterType]      = useState("all");

  // ── Fetch from mock API ─────────────────────
  const loadNotifications = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await fetchNotifications(role);
      setNotifications(data);
    } catch {
      setError("Could not load notifications. Please try again.");
    } finally {
      setLoading(false);
    }
  }, [role]);

  useEffect(() => { loadNotifications(); }, [loadNotifications]);

  // ── Derived ─────────────────────────────────
  const unreadCount = notifications.filter((n) => !n.read).length;

  const typeOptions = [
    "all",
    ...Array.from(new Set(notifications.map((n) => n.type))),
  ];

  const filtered = notifications.filter((n) => {
    const tabOk  = activeTab  === "unread" ? !n.read : true;
    const typeOk = filterType === "all"    ? true    : n.type === filterType;
    return tabOk && typeOk;
  });

  const grouped = filtered.reduce((acc, note) => {
    acc[note.dateGroup] = acc[note.dateGroup] || [];
    acc[note.dateGroup].push(note);
    return acc;
  }, {});

  // ── Handlers ────────────────────────────────
  const handleClick = async (note) => {
    if (!note.read) {
      try {
        await markAsRead(note.id);
        setNotifications((prev) =>
          prev.map((n) => (n.id === note.id ? { ...n, read: true } : n))
        );
      } catch (_) {}
    }
    if (note.route) navigate(note.route);
  };

  const handleMarkAll = async () => {
    try {
      await markAllAsRead(role);
      setNotifications((prev) => prev.map((n) => ({ ...n, read: true })));
    } catch (_) {}
  };

  const handleDelete = async (e, id) => {
    e.stopPropagation();
    setNotifications((prev) => prev.filter((n) => n.id !== id));
    try {
      await deleteNotification(id);
    } catch (_) {
      loadNotifications();
    }
  };

  // ── Render ───────────────────────────────────
  return (
    <div className="menu-background custom-scrollbar">

      {/* ── HEADER ── */}
      <div className="notif-header">
        <div className="notif-header-left">
          <h2>Notifications</h2>
        </div>
        {unreadCount > 0 && (
          <button className="notif-mark-all" onClick={handleMarkAll}>
            Mark all as read
          </button>
        )}
      </div>

      {/* ── TABS ── */}
      <div className="notif-tabs">
        <span
          className={activeTab === "all" ? "active" : ""}
          onClick={() => setActiveTab("all")}
        >
          All
          <em>{notifications.length}</em>
        </span>
        <span
          className={activeTab === "unread" ? "active" : ""}
          onClick={() => setActiveTab("unread")}
        >
          Unread
          {unreadCount > 0 && <em className="em-unread">{unreadCount}</em>}
        </span>
      </div>

      {/* ── TYPE FILTERS ── */}
      <div className="notif-filters">
        {typeOptions.map((t) => (
          <button
            key={t}
            className={`notif-pill ${filterType === t ? "active" : ""}`}
            onClick={() => setFilterType(t)}
          >
            {t === "all" ? "All types" : TYPE_META[t]?.label || t}
          </button>
        ))}
      </div>

      {/* ── NOTIFICATION LIST ── */}
      <div className="notif-list">

        {/* Loading */}
        {loading && (
          <div className="notif-state">
            <div className="notif-spinner" />
            <p>Loading…</p>
          </div>
        )}

        {/* Error */}
        {error && !loading && (
          <div className="notif-state">
            <span>⚠️</span>
            <p>{error}</p>
            <button className="notif-retry" onClick={loadNotifications}>Retry</button>
          </div>
        )}

        {/* Empty */}
        {!loading && !error && Object.keys(grouped).length === 0 && (
          <div className="notif-state">
            <span className="notif-empty-icon">🔔</span>
            <p>You're all caught up!</p>
            <span>No notifications to show</span>
          </div>
        )}

        {/* Groups */}
        {!loading && !error &&
          Object.keys(grouped).map((group) => (
            <div key={group}>
              <p className="group-title">{group}</p>

              {grouped[group].map((note, i) => {
                const meta = TYPE_META[note.type] || TYPE_META.info;
                return (
                  <div
                    key={note.id}
                    className={`notif-card ${note.read ? "read" : "unread"}`}
                    style={{ animationDelay: `${i * 55}ms` }}
                    onClick={() => handleClick(note)}
                  >
                    {/* Left accent bar for unread */}
                    {!note.read && <div className="notif-accent" />}

                    {/* Icon */}
                    <div className={`notif-icon ${meta.color}`}>
                      {meta.icon}
                    </div>

                    {/* Content */}
                    <div className="notif-content">
                      <div className="notif-msg-row">
                        <p className="message">{note.message}</p>
                        {note.priority === "urgent" && (
                          <span className="notif-urgent">Urgent</span>
                        )}
                      </div>
                      {note.detail && (
                        <p className="notif-detail">{note.detail}</p>
                      )}
                      <div className="notif-meta-row">
                        <span className="notif-type-tag">{meta.label}</span>
                        <span className="notif-sep">·</span>
                        <span className="time">{note.time}</span>
                      </div>
                    </div>

                    {/* Right */}
                    <div className="notif-right">
                      {!note.read && <span className="dot" />}
                      <button
                        className="notif-delete"
                        onClick={(e) => handleDelete(e, note.id)}
                        title="Dismiss"
                      >
                        <svg width="10" height="10" viewBox="0 0 12 12" fill="none">
                          <path d="M1 1l10 10M11 1L1 11"
                            stroke="currentColor" strokeWidth="2" strokeLinecap="round"/>
                        </svg>
                      </button>
                    </div>
                  </div>
                );
              })}
            </div>
          ))
        }
      </div>
    </div>
  );
};

export default NotificationPage;