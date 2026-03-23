import { useEffect, useState } from "react";
import { useNavigate, useLocation } from "react-router-dom";
import "./NotificationPage.css";

const NotificationPage = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const role = location.state?.role || "normaluser";

  const [notifications, setNotifications] = useState([]);
  const [activeTab, setActiveTab] = useState("all");

  // Initialize notifications
  useEffect(() => {
    const roleNotifications = {
      superuser: [
        {
          id: 1,
          message: "New leave request from John",
          read: false,
          type: "leave",
          route: "/leaveManagement",
          time: "2m ago",
          dateGroup: "Today",
        },
        {
          id: 2,
          message: "System update required",
          read: true,
          type: "system",
          time: "1h ago",
          dateGroup: "Today",
        },
        {
          id: 3,
          message: "Payroll processed successfully",
          read: true,
          type: "payroll",
          time: "Yesterday",
          dateGroup: "Earlier",
        },
      ],
      normaluser: [
        {
          id: 4,
          message: "Payslip available",
          read: false,
          type: "payslip",
          route: "/payslips",
          time: "Today",
          dateGroup: "Today",
        },
      ],
    };

    setNotifications(roleNotifications[role] || []);
  }, [role]);

  // Filter and group notifications
  const filteredNotifications =
    activeTab === "unread"
      ? notifications.filter((n) => !n.read)
      : notifications;

  const grouped = filteredNotifications.reduce((acc, note) => {
    acc[note.dateGroup] = acc[note.dateGroup] || [];
    acc[note.dateGroup].push(note);
    return acc;
  }, {});

  const handleClick = (note) => {
    setNotifications((prev) =>
      prev.map((n) => (n.id === note.id ? { ...n, read: true } : n))
    );
    if (note.route) navigate(note.route);
  };

  const markAllAsRead = () => {
    setNotifications((prev) => prev.map((n) => ({ ...n, read: true })));
  };

  const getIcon = (type) => {
    switch (type) {
      case "leave":
        return "📅";
      case "system":
        return "⚙️";
      case "payroll":
        return "💰";
      case "payslip":
        return "📄";
      default:
        return "🔔";
    }
  };

  return (
    <div className="menu-background custom-scrollbar">
      {/* HEADER */}
      <div className="notif-header">
        <h2>Notifications</h2>
        <button onClick={markAllAsRead}>Mark all as read</button>
      </div>

      {/* TABS */}
      <div className="notif-tabs">
        <span
          className={activeTab === "all" ? "active" : ""}
          onClick={() => setActiveTab("all")}
        >
          All
        </span>
        <span
          className={activeTab === "unread" ? "active" : ""}
          onClick={() => setActiveTab("unread")}
        >
          Unread
        </span>
      </div>

      {/* NOTIFICATION LIST */}
      <div className="notif-list">
        {Object.keys(grouped).length === 0 && (
          <p className="no-notifications">No notifications</p>
        )}
        {Object.keys(grouped).map((group) => (
          <div key={group}>
            <p className="group-title">{group}</p>
            {grouped[group].map((note) => (
              <div
                key={note.id}
                className={`notif-card ${note.read ? "read" : "unread"}`}
                onClick={() => handleClick(note)}
              >
                <div className={`notif-icon ${note.type}`}>
                  {getIcon(note.type)}
                </div>
                <div className="notif-content">
                  <p className="message">{note.message}</p>
                  <span className="time">{note.time}</span>
                </div>
                {!note.read && <span className="dot" />}
              </div>
            ))}
          </div>
        ))}
      </div>
    </div>
  );
};

export default NotificationPage;