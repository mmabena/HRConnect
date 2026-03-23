import { useState, useEffect, useRef } from "react";
import "./NotificationBell.css";
import { useNavigate } from "react-router-dom";

const NotificationBell = ({ role }) => {
  const [showPopup, setShowPopup] = useState(false);
  const popupRef = useRef();
  const navigate = useNavigate();

  // ✅ Notifications with read status
  const [notifications, setNotifications] = useState([]);

  useEffect(() => {
    const roleNotifications = {
      superuser: [
        {
          message: "System update required",
          read: false,
          type: "system",
        },
        {
          message: "New leave request submitted",
          read: false,
          type: "leave",
          route: "/leaveManagement",
        },
      ],

      normaluser: [
        {
          message: "Payslip available",
          read: false,
          type: "payslip",
          route: "/payslips",
        },
        {
          message: "Leave approved",
          read: true,
          type: "leave",
          route: "/leave-balance",
        },
      ],
    };

    setNotifications(roleNotifications[role] || []);
  }, [role]);

  // ✅ Count unread
  const unreadCount = notifications.filter((n) => !n.read).length;

  // ✅ Click outside close
  useEffect(() => {
    const handleClickOutside = (e) => {
      if (popupRef.current && !popupRef.current.contains(e.target)) {
        setShowPopup(false);
      }
    };

    document.addEventListener("mousedown", handleClickOutside);
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, []);

  // ✅ Mark all as read when opened (optional)
  const handleToggle = () => {
    setShowPopup((prev) => !prev);

    if (!showPopup) {
      setNotifications((prev) => prev.map((n) => ({ ...n, read: true })));
    }
  };

  const handleNotificationClick = (note) => {
    // mark as read
    setNotifications((prev) =>
      prev.map((n) => (n.id === note.id ? { ...n, read: true } : n)),
    );

    // redirect if route exists
    if (note.route) {
      navigate(note.route);
      setShowPopup(false);
    }
  };

  return (
    <div className="notification-container" ref={popupRef}>
      <div className="bell-wrapper" onClick={handleToggle}>
        <img src="/images/bell.svg" alt="Bell icon" className="menu-icon" />

        {/* ✅ BADGE */}
        {unreadCount > 0 && (
          <span className="notification-badge">{unreadCount}</span>
        )}
      </div>

      {showPopup && (
        <div className="notification-popup">
          <h4 className="title">Notifications</h4>

          {notifications.length > 0 ? (
            notifications.map((note) => (
              <div
                key={note.id}
                className={`notification-item ${note.read ? "read" : "unread"}`}
                onClick={() => handleNotificationClick(note)}
              >
                {note.message}
              </div>
            ))
          ) : (
            <div className="notification-item">No notifications</div>
          )}
        </div>
      )}
    </div>
  );
};

export default NotificationBell;
