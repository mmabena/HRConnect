// this component is a helper for the change password modal
import React, { useState } from "react";
import ChangePassword from "./ChangePassword";

export default function Dashboard() {
  const [showChangePassword, setShowChangePassword] = useState(false);

  return (
    <div>
      <button
        className="request-button"
        onClick={() => setShowChangePassword(true)}
      >
        Change Password
      </button>

      {showChangePassword && (
        <ChangePassword onClose={() => setShowChangePassword(false)} />
      )}
    </div>
  );
}