// NotificationBell.jsx
import React, { useState } from "react";
import "./NotificationBell.css";

export default function NotificationBell({ currentUser, notifications = [] }) {
  const [isOpen, setIsOpen] = useState(false);

  // Safe check for currentUser
  const role = currentUser?.role?.toLowerCase();
    
  // Filter notifications for this user's role
  const roleNotifications = notifications.filter(
    (n) => n.role.toLowerCase() === role
  );

  const toggleModal = () => setIsOpen((prev) => !prev);

  return (
    <div className="notification-bell-wrapper">
      <img
        src="/images/icon.png"
        alt="Notifications"
        className="cm-notification-icon"
        onClick={toggleModal}
      />
      {roleNotifications.length > 0 && <span className="notification-dot" />}

      {isOpen && (
        <div className="notification-modal">
          <div className="modal-header">
            <h3>Notifications</h3>
          </div>
          <ul className="modal-list">
            {roleNotifications.length > 0 ? (
              roleNotifications.map((n, idx) => (
                <li key={idx}>{n.message}</li>
              ))
            ) : (
              <li>No notifications</li>
            )}
          </ul>
        </div>
      )}
    </div>
  );
}