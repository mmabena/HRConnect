import { useState, useEffect, useRef } from "react";
import "./NotificationBell.css";

const NotificationBell = ({ role }) => {
  const [showPopup, setShowPopup] = useState(false);
  const popupRef = useRef();

  // ✅ Notifications with read status
  const [notifications, setNotifications] = useState([]);

  useEffect(() => {
    const roleNotifications = {
      superuser: [
        { id: 14, message: "System update required", read: false },
      ],
      normaluser: [
        { id: 16, message: "Payslip available", read: false },
        { id: 5, message: "Leave approved", read: true },
      ],
    };

    setNotifications(roleNotifications[role] || []);
  }, [role]);

  // ✅ Count unread
  const unreadCount = notifications.filter(n => !n.read).length;

  // ✅ Click outside close
  useEffect(() => {
    const handleClickOutside = (e) => {
      if (popupRef.current && !popupRef.current.contains(e.target)) {
        setShowPopup(false);
      }
    };

    document.addEventListener("mousedown", handleClickOutside);
    return () =>
      document.removeEventListener("mousedown", handleClickOutside);
  }, []);

  // ✅ Mark all as read when opened (optional)
  const handleToggle = () => {
    setShowPopup(prev => !prev);

    if (!showPopup) {
      setNotifications(prev =>
        prev.map(n => ({ ...n, read: true }))
      );
    }
  };

  return (
    <div className="notification-container" ref={popupRef}>
      <div className="bell-wrapper" onClick={handleToggle}>
        <img
          src="/images/bell.svg"
          alt="Bell icon"
          className="menu-icon"
        />

        {/* ✅ BADGE */}
        {unreadCount > 0 && (
          <span className="notification-badge">
            {unreadCount}
          </span>
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